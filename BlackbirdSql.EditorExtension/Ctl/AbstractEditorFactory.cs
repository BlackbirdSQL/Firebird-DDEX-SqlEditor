// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.BaseEditorFactory/SqlEditorFactory

using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

using Cmd = BlackbirdSql.Common.Cmd;
using Tracer = BlackbirdSql.Core.Ctl.Diagnostics.Tracer;




namespace BlackbirdSql.EditorExtension.Ctl;

public abstract class AbstractEditorFactory : AbstruseEditorFactory
{

	protected static volatile int _EditorId;


	protected static uint dacTSqlEditorLaunchingCookie;

	protected static IVsMonitorSelection _monitorSelection;

	protected bool IsOpeningViaTsdataButton
	{
		get
		{
			if (_monitorSelection == null)
			{
				Microsoft.VisualStudio.OLE.Interop.IServiceProvider oleServiceProvider = OleServiceProvider;

				Guid guidService = typeof(IVsMonitorSelection).GUID;
				Guid riid = VSConstants.IID_IUnknown;

				int num = oleServiceProvider.QueryService(ref guidService, ref riid, out IntPtr ppvObject);

				if (Native.Failed(num) && num != VSConstants.E_FAIL && num != VSConstants.E_NOINTERFACE)
				{
					if (ppvObject != IntPtr.Zero)
					{
						Marshal.Release(ppvObject);
					}

					return false;
				}

				if (ppvObject != IntPtr.Zero)
				{
					try
					{
						_monitorSelection = (IVsMonitorSelection)Marshal.GetObjectForIUnknown(ppvObject);
						Guid rguidCmdUI = VS.UICONTEXT_DacTSqlEditorLaunching;
						_monitorSelection.GetCmdUIContextCookie(ref rguidCmdUI, out dacTSqlEditorLaunchingCookie);
					}
					finally
					{
						Marshal.Release(ppvObject);
					}
				}
			}

			if (_monitorSelection != null)
			{
				_monitorSelection.IsCmdUIContextActive(dacTSqlEditorLaunchingCookie, out var pfActive);
				return pfActive == 1;
			}

			return false;
		}
	}

	public override Guid ClsidEditorFactory
	{
		get
		{
			if (WithEncoding)
			{
				return new Guid(SystemData.DslEditorEncodedFactoryGuid);
			}

			return new Guid(SystemData.DslEditorFactoryGuid);
		}
	}

	private string EditorId { get; set; }

	public AbstractEditorFactory(bool withEncoding)
		: base(withEncoding)
	{
	}

	public override int CreateEditorInstance(uint createFlags, string moniker, string physicalView, IVsHierarchy hierarchy, uint itemId, IntPtr existingDocData, out IntPtr docViewIntPtr, out IntPtr docDataIntPtr, out string caption, out Guid cmdUIGuid, out int result)
	{
		docViewIntPtr = IntPtr.Zero;
		docDataIntPtr = IntPtr.Zero;
		caption = "";
		cmdUIGuid = Guid.Empty;
		result = 1;
		EditorId = "Editor" + _EditorId++;

		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			Cursor current = Cursor.Current;
			try
			{
				Tracer.Trace(GetType(), "IVsEditorFactory.CreateEditorInstance", "nCreateFlags = {0}, strMoniker = {1}, strPhysicalView = {2}, itemid = {3}", createFlags, moniker, physicalView, itemId);
				SqlEtwProvider.EventWriteTSqlEditorLaunch(IsStart: true, EditorId ?? string.Empty);
				if ((createFlags & 0x10) != 16)
				{
					if (IsOpeningViaTsdataButton)
					{
						PlatformNotSupportedException ex = new("TsData");
						Diag.Dug(ex);
						result = 0;
						return VSConstants.VS_E_UNSUPPORTEDFORMAT;
					}

					GetCurrentHierarchies(out var foundTsData, out var foundSqlStudio);
					if (foundTsData && !foundSqlStudio)
					{
						PlatformNotSupportedException ex = new("SqlStudio");
						Diag.Dug(ex);
						result = 0;
						return VSConstants.VS_E_UNSUPPORTEDFORMAT;
					}

					if (IsTsDataProject(hierarchy))
					{
						PlatformNotSupportedException ex = new("TsData project");
						Diag.Dug(ex);
						result = 0;
						return VSConstants.VS_E_UNSUPPORTEDFORMAT;
					}
				}

				if (!string.IsNullOrEmpty(physicalView) /* && physicalView != "CodeFrame" */)
				{
					ArgumentException ex = new("physicalView is not CodeFrame or empty: " + physicalView);
					Diag.Dug(ex);
					Tracer.Trace(GetType(), "IVsEditorFactory.CreateEditorInstance", "physicalView is not null or empty- returning E_INVALIDARG");
					return VSConstants.E_INVALIDARG;
				}

				if ((createFlags & 6) == 0)
				{
					ArgumentException ex = new("invalid create flags: " + createFlags);
					Diag.Dug(ex);
					Tracer.Trace(GetType(), "IVsEditorFactory.CreateEditorInstance", "invalid create flags - returning E_INVALIDARG");
					return VSConstants.E_INVALIDARG;
				}

				IVsTextLines vsTextLines = null;
				if (existingDocData != IntPtr.Zero)
				{
					if (WithEncoding)
					{
						// DataException ex = new("Data is not encoding compatible");
						// Diag.Dug(ex);
						return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
					}

					object objectForIUnknown = Marshal.GetObjectForIUnknown(existingDocData);
					vsTextLines = objectForIUnknown as IVsTextLines;
					if (vsTextLines == null)
					{
						DataException ex = new("Data is not IVsTextLines compatible");
						Diag.Dug(ex);
						return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
					}

					if (objectForIUnknown is not IVsUserData)
					{
						DataException ex = new("Data is not IVsUserData compatible");
						Diag.Dug(ex);
						return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
					}

					Guid pguidLangService = Guid.Empty;
					vsTextLines.GetLanguageServiceID(out pguidLangService);

					if (!(pguidLangService == MandatedSqlLanguageServiceClsid)
						&& !(pguidLangService == VS.CLSID_LanguageServiceDefault))
					{
						InvalidOperationException ex = new("Invalid language service: " + pguidLangService);
						Diag.Dug(ex);
						return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
					}

					/*
					if (IsDocDataOpenByDev10SqlEditor(objectForIUnknown))
					{
						InvalidOperationException ex = new("Invalid editor VS10");
						Diag.Dug(ex);
						return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
					}
					*/
				}

				result = 0;
				int result2 = VSConstants.E_FAIL;
				Cursor.Current = Cursors.WaitCursor;
				IVsTextLines vsTextLines2 = null;
				if (vsTextLines == null)
				{
					Guid clsid = typeof(VsTextBufferClass).GUID;
					Guid iid = VSConstants.IID_IUnknown;
					object obj = ((AsyncPackage)Controller.DdexPackage).CreateInstance(ref clsid, ref iid, typeof(object));
					if (WithEncoding)
					{
						IVsUserData obj2 = obj as IVsUserData;
						Guid riidKey = VSConstants.VsTextBufferUserDataGuid.VsBufferEncodingPromptOnLoad_guid;
						obj2.SetData(ref riidKey, 1u);
					}

					(obj as IObjectWithSite)?.SetSite(OleServiceProvider);
					vsTextLines2 = obj as IVsTextLines;
				}
				else
				{
					vsTextLines2 = vsTextLines;
				}

				if (vsTextLines2 != null)
				{
					EnsureAuxilliaryDocData(hierarchy, moniker, vsTextLines2);
					SqlEditorTabbedEditorPane o = CreateTabbedEditorPane(vsTextLines2, moniker);
					docViewIntPtr = Marshal.GetIUnknownForObject(o);
					docDataIntPtr = Marshal.GetIUnknownForObject(vsTextLines2);
					caption = string.Empty;
					cmdUIGuid = VSConstants.GUID_TextEditorFactory;
					Guid guidLangService = MandatedSqlLanguageServiceClsid;
					if (guidLangService != Guid.Empty)
					{
						vsTextLines2.SetLanguageServiceID(ref guidLangService);
						IVsUserData obj3 = (IVsUserData)vsTextLines2;
						Guid riidKey2 = VSConstants.VsTextBufferUserDataGuid.VsBufferDetectLangSID_guid;
						Native.ThrowOnFailure(obj3.SetData(ref riidKey2, false));
					}

					result2 = 0;
				}

				return result2;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				if (ex is NullReferenceException || ex is ApplicationException || ex is ArgumentException || ex is InvalidOperationException)
				{
					Cmd.ShowExceptionInDialog(SharedResx.BaseEditorFactory_FailedToCreateEditor, ex);
					return VSConstants.E_FAIL;
				}

				throw;
			}
			finally
			{
				Cursor.Current = current;
			}
		}
	}

	protected virtual SqlEditorTabbedEditorPane CreateTabbedEditorPane(IVsTextLines vsTextLines, string moniker)
	{
		return new SqlEditorTabbedEditorPane(ServiceProvider, EditorExtensionAsyncPackage.Instance, vsTextLines, moniker);
	}

	protected virtual void EnsureAuxilliaryDocData(IVsHierarchy hierarchy, string documentMoniker, object docData)
	{
		EditorExtensionAsyncPackage.Instance.EnsureAuxilliaryDocData(hierarchy, documentMoniker, docData);
	}

	private static bool IsTsDataProject(IVsHierarchy hierarchy)
	{
		return GetProjectGuid(hierarchy) == VS.CLSID_TSqlDataProjectNode;
	}

	private static Guid GetProjectGuid(IVsHierarchy hierarchy)
	{
		Native.ThrowOnFailure(hierarchy.GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_TypeGuid, out var pguid));
		return pguid;
	}

	public static bool IsDocDataOpenByDev10SqlEditor(object existingDocData)
	{
		return false;
	}

	private void GetCurrentHierarchies(out bool foundTsData, out bool foundSqlStudio)
	{
		foundTsData = false;
		foundSqlStudio = false;
		Guid clsidSSDTProjectNodeFactory = VS.CLSID_SSDTProjectNode;
		Guid clsidTsDataProject = VS.CLSID_TSqlDataProjectNode;

		if (ServiceProvider.GetService(typeof(IVsSolution)) is not IVsSolution vsSolution)
		{
			return;
		}

		Guid rguidEnumOnlyThisType = Guid.Empty;
		int projectEnum = vsSolution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLINSOLUTION, ref rguidEnumOnlyThisType, out IEnumHierarchies ppenum);

		ErrorHandler.ThrowOnFailure(projectEnum);
		if (projectEnum != 0 || ppenum == null)
		{
			return;
		}

		IVsHierarchy[] array = new IVsHierarchy[1];
		while (ppenum.Next(1u, array, out uint pceltFetched) == 0 && pceltFetched == 1)
		{
			if (Native.Succeeded(array[0].GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_TypeGuid, out rguidEnumOnlyThisType)))
			{
				if (rguidEnumOnlyThisType == clsidSSDTProjectNodeFactory)
				{
					foundSqlStudio = true;
				}
				else if (rguidEnumOnlyThisType == clsidTsDataProject)
				{
					foundTsData = true;
				}

				if (foundTsData & foundSqlStudio)
				{
					break;
				}
			}
		}
	}
}
