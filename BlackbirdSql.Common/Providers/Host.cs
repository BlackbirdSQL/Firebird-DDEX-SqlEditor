using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Telemetry;
using Microsoft.Win32;


namespace BlackbirdSql.Common.Provider
{
	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]
	internal class Host
	{
		private delegate object CreateLocalInstanceDelegate(Guid classId);

		private delegate DialogResult ShowQuestionDelegate(string question, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, string helpId);

		private delegate void ShowErrorDelegate(string error);

		private delegate void ShowMessageDelegate(string message);

		private delegate void PostExecuteCommandDelegate(CommandID command);

		private delegate void QueryDesignerProviderTelemetryDelegate(int provider);

		private delegate void ServerExplorerTelemetryDelegate(string metric, string property, int count);

		public static class QualityMetric
		{
			public const string ServerExplorer_TableMetric = "VS/VSData/SqlObjectSelector/ServerExplorer/Count-Table";

			public const string ServerExplorer_ViewMetric = "VS/VSData/SqlObjectSelector/ServerExplorer/Count-View";

			public const string ServerExplorer_StoredProcedureMetric = "VS/VSData/SqlObjectSelector/ServerExplorer/Count-StoredProcedure";

			public const string ServerExplorer_FunctionMetric = "VS/VSData/SqlObjectSelector/ServerExplorer/Count-Function";

			public const string ServerExplorer_SqlAssemblyMetric = "VS/VSData/SqlObjectSelector/ServerExplorer/Count-SqlAssembly";

			public const string ServerExplorer_OtherMetric = "VS/VSData/SqlObjectSelector/ServerExplorer/Count-Other";

			public const string ServerExplorer_SynonymsMetric = "VS/VSData/SqlSynonymSelector/ServerExplorer/Count-Synonyms";

			public const string QueryDesigner_ProviderUsage = "VS/VSData/DataViewQueryCommandProvider/Create-Query";
		}

		public static class QualityMetricProperty
		{
			public const string ServerExplorer_SqlObjectSelectorProvider = "VS.VSData.SqlObjectSelector.ObjectCount";

			public const string ServerExplorer_SqlSynonymSelectorProvider = "VS.VSData.SqlSynonymSelector.ObjectCount";

			public const string QueryDesigner_ProviderUsage_SqlServer = "VS.VSData.DataViewQueryCommandProvider.Provider.SqlServer";

			public const string QueryDesigner_ProviderUsage_Oracle = "VS.VSData.DataViewQueryCommandProvider.Provider.Oracle";

			public const string QueryDesigner_ProviderUsage_OleDB = "VS.VSData.DataViewQueryCommandProvider.Provider.OleDB";

			public const string QueryDesigner_ProviderUsage_Odbc = "VS.VSData.DataViewQueryCommandProvider.Provider.Odbc";

			public const string QueryDesigner_ProviderUsage_SqlLocalDB = "VS.VSData.DataViewQueryCommandProvider.Provider.SqlLocalDB";

			public const string QueryDesigner_ProviderUsage_Unknown = "VS.VSData.DataViewQueryCommandProvider.Provider.Unknown";
		}

		public static class QualityMetricProvider
		{
			public const int SqlServer = 1;

			public const int Oracle = 2;

			public const int OleDB = 4;

			public const int Odbc = 8;

			public const int SqlLocalDB = 16;

			public const int Unknown = 262144;

			public static void QueryDesignerProviderEventProperty(UserTaskEvent userTaskEvent, int provider)
			{
				Diag.Trace();

				int[] array = new int[6] { 1, 2, 4, 8, 16, 262144 };
				string[] array2 = new string[6] { "VS.VSData.DataViewQueryCommandProvider.Provider.SqlServer", "VS.VSData.DataViewQueryCommandProvider.Provider.Oracle", "VS.VSData.DataViewQueryCommandProvider.Provider.OleDB", "VS.VSData.DataViewQueryCommandProvider.Provider.Odbc", "VS.VSData.DataViewQueryCommandProvider.Provider.SqlLocalDB", "VS.VSData.DataViewQueryCommandProvider.Provider.Unknown" };
				for (int i = 0; i < array.Length; i++)
				{
					bool flag = (provider & array[i]) != 0;
					userTaskEvent.Properties[array2[i]] = flag;
				}
			}
		}

		private IVsDataHostService _hostService;

		public string ApplicationDataDirectory
		{
			get
			{
				Diag.Trace();
				object pvar = null;
				IVsShell service = _hostService.GetService<IVsShell>();
				NativeMethods.WrapComCall(service.GetProperty(-9021, out pvar));
				return pvar as string;
			}
		}

		protected IVsDataHostService HostService => _hostService;

		public Host(System.IServiceProvider serviceProvider)
		{
			Diag.Trace();
			_hostService = serviceProvider.GetService(typeof(IVsDataHostService)) as IVsDataHostService;
			Assumes.Present(_hostService);
		}

		public object CreateLocalInstance(Guid classId)
		{
			Diag.Trace();
			if (Thread.CurrentThread == _hostService.UIThread)
			{
				return CreateLocalInstanceImpl(classId);
			}

			return _hostService.InvokeOnUIThread(new CreateLocalInstanceDelegate(CreateLocalInstanceImpl), classId);
		}

		private object CreateLocalInstanceImpl(Guid classId)
		{
			Diag.Trace();

			IntPtr ppvObj = IntPtr.Zero;
			ILocalRegistry2 service = _hostService.GetService<SLocalRegistry, ILocalRegistry2>();
			NativeMethods.WrapComCall(service.CreateInstance(classId, null, ref NativeMethods.IID_IUnknown, 1u, out ppvObj));
			object result = null;
			if (ppvObj != IntPtr.Zero)
			{
				result = Marshal.GetObjectForIUnknown(ppvObj);
				Marshal.Release(ppvObj);
			}

			return result;
		}

		public bool IsCommandUIContextActive(Guid commandUIGuid)
		{
			Diag.Trace();
			IVsMonitorSelection service = _hostService.GetService<IVsMonitorSelection>();
			uint pdwCmdUICookie = 0u;
			NativeMethods.WrapComCall(service.GetCmdUIContextCookie(ref commandUIGuid, out pdwCmdUICookie));
			int pfActive = 0;
			NativeMethods.WrapComCall(service.IsCmdUIContextActive(pdwCmdUICookie, out pfActive));
			return pfActive != 0;
		}

		public object GetDBToolsOption(DBToolsOption option)
		{
			Diag.Trace();
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected I4, but got Unknown
			object result = null;
			IDBToolsOptionProvider val = _hostService.TryGetService<IDBToolsOptionProvider>();
			if (val != null)
			{
				IDBToolsOptionReaderWriter dBToolsOptionReaderWriter = val.GetDBToolsOptionReaderWriter();
				if (dBToolsOptionReaderWriter != null)
				{
					result = dBToolsOptionReaderWriter.GetDBToolsOption((int)option);
				}
			}

			return result;
		}

		public DialogResult ShowDialog(Form form)
		{
			Diag.Trace();
			IUIService uiService = _hostService.GetService<IUIService>();
			UserPreferenceChangedEventHandler value = delegate
			{
				form.Font = uiService.Styles["DialogFont"] as Font;
			};
			SystemEvents.UserPreferenceChanged += value;
			try
			{
				return uiService.ShowDialog(form);
			}
			finally
			{
				SystemEvents.UserPreferenceChanged -= value;
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMethodsAsStatic")]
		public bool TranslateContextHelpMessage(Form form, ref Message m)
		{
			Diag.Trace();
			if (m.Msg != 274 || m.WParam != (IntPtr)61824)
			{
				return false;
			}

			Control control = form;
			for (ContainerControl containerControl = control as ContainerControl; containerControl != null; containerControl = control as ContainerControl)
			{
				control = containerControl.ActiveControl;
				if (control == null)
				{
					control = form;
					break;
				}
			}

			m.HWnd = control.Handle;
			m.Msg = 83;
			m.WParam = IntPtr.Zero;
			NativeMethods.HELPINFO hELPINFO = new NativeMethods.HELPINFO();
			hELPINFO.iContextType = 1;
			hELPINFO.iCtrlId = form.Handle.ToInt32();
			hELPINFO.hItemHandle = control.Handle;
			hELPINFO.dwContextId = 0;
			hELPINFO.MousePos.x = NativeMethods.LOWORD((uint)(int)m.LParam);
			hELPINFO.MousePos.y = NativeMethods.HIWORD((uint)(int)m.LParam);
			m.LParam = Marshal.AllocHGlobal(Marshal.SizeOf(hELPINFO));
			Marshal.StructureToPtr(hELPINFO, m.LParam, fDeleteOld: false);
			return true;
		}

		public DialogResult ShowQuestion(string question, MessageBoxDefaultButton defaultButton)
		{
			Diag.Trace();
			return ShowQuestion(question, defaultButton, null);
		}

		public DialogResult ShowQuestion(string question, string helpId)
		{
			Diag.Trace();
			return ShowQuestion(question, MessageBoxDefaultButton.Button2, helpId);
		}

		public DialogResult ShowQuestion(string question, MessageBoxDefaultButton defaultButton, string helpId)
		{
			Diag.Trace();
			return ShowQuestion(question, MessageBoxButtons.YesNo, defaultButton, helpId);
		}

		public DialogResult ShowQuestion(string question, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, string helpId)
		{
			Diag.Trace();
			if (Thread.CurrentThread == _hostService.UIThread)
			{
				return ShowQuestionImpl(question, buttons, defaultButton, helpId);
			}

			return (DialogResult)_hostService.InvokeOnUIThread(new ShowQuestionDelegate(ShowQuestionImpl), question, buttons, defaultButton, helpId);
		}

		private DialogResult ShowQuestionImpl(string question, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, string helpId)
		{
			Diag.Trace();
			IVsUIShell service = GetService<SVsUIShell, IVsUIShell>();
			Guid rclsidComp = Guid.Empty;
			OLEMSGDEFBUTTON msgdefbtn = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
			if (defaultButton == MessageBoxDefaultButton.Button2)
			{
				msgdefbtn = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND;
			}

			int pnResult = 0;
			NativeMethods.WrapComCall(service.ShowMessageBox(0u, ref rclsidComp, null, question, helpId, 0u, (OLEMSGBUTTON)buttons, msgdefbtn, OLEMSGICON.OLEMSGICON_QUERY, 0, out pnResult));
			return (DialogResult)pnResult;
		}

		public void ShowError(string error)
		{
			Diag.Trace();
			if (Thread.CurrentThread == HostService.UIThread)
			{
				ShowErrorImpl(error);
				return;
			}

			_hostService.InvokeOnUIThread(new ShowErrorDelegate(ShowErrorImpl), error);
		}

		private void ShowErrorImpl(string error)
		{
			Diag.Trace();
			IUIService service = _hostService.GetService<IUIService>();
			service.ShowError(error);
		}

		public void ShowMessage(string message)
		{
			Diag.Trace();
			if (Thread.CurrentThread == HostService.UIThread)
			{
				ShowMessageImpl(message);
				return;
			}

			_hostService.InvokeOnUIThread(new ShowMessageDelegate(ShowMessageImpl), message);
		}

		private void ShowMessageImpl(string message)
		{
			Diag.Trace();
			IUIService service = _hostService.GetService<IUIService>();
			service.ShowMessage(message);
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void ShowHelp(string keyword)
		{
			Diag.Trace();
			try
			{
				Microsoft.VisualStudio.VSHelp.Help service = _hostService.GetService<Microsoft.VisualStudio.VSHelp.Help, Microsoft.VisualStudio.VSHelp.Help>();
				service.DisplayTopicFromF1Keyword(keyword);
			}
			catch
			{
			}
		}

		public bool IsDocumentInAProject(string documentMoniker)
		{
			Diag.Trace();
			IVsUIShellOpenDocument service = _hostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
			uint pitemid = 0u;
			Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP = null;
			int pDocInProj = 0;
			NativeMethods.WrapComCall(service.IsDocumentInAProject(documentMoniker, out var _, out pitemid, out ppSP, out pDocInProj));
			return pDocInProj != 0;
		}

		public bool ActivateDocumentIfOpen(string documentMoniker)
		{
			Diag.Trace();
			return ActivateDocumentIfOpen(documentMoniker, doNotShowWindowFrame: false) != null;
		}

		public IVsWindowFrame ActivateDocumentIfOpen(string documentMoniker, bool doNotShowWindowFrame)
		{
			Diag.Trace();
			IVsUIShellOpenDocument service = _hostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
			Guid rguidLogicalView = Guid.Empty;
			uint grfIDO = 1u;
			if (doNotShowWindowFrame)
			{
				grfIDO = 0u;
			}

			IVsUIHierarchy ppHierOpen = null;
			IVsWindowFrame ppWindowFrame = null;
			int pfOpen = 0;
			NativeMethods.WrapComCall(service.IsDocumentOpen(null, uint.MaxValue, documentMoniker, ref rguidLogicalView, grfIDO, out ppHierOpen, null, out ppWindowFrame, out pfOpen));
			return ppWindowFrame;
		}

		public IVsWindowFrame OpenDocumentViaProject(string documentMoniker)
		{
			Diag.Trace();
			IVsUIShellOpenDocument service = _hostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
			Guid rguidLogicalView = Guid.Empty;
			Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP = null;
			IVsUIHierarchy ppHier = null;
			uint pitemid = 0u;
			IVsWindowFrame ppWindowFrame = null;
			NativeMethods.WrapComCall(service.OpenDocumentViaProject(documentMoniker, ref rguidLogicalView, out ppSP, out ppHier, out pitemid, out ppWindowFrame));
			return ppWindowFrame;
		}

		public void RenameDocument(string oldDocumentMoniker, string newDocumentMoniker)
		{
			Diag.Trace();
			RenameDocument(oldDocumentMoniker, -1, newDocumentMoniker);
		}

		public void RenameDocument(string oldDocumentMoniker, int newItemId, string newDocumentMoniker)
		{
			Diag.Trace();
			IVsRunningDocumentTable service = _hostService.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();
			NativeMethods.WrapComCall(service.RenameDocument(oldDocumentMoniker, newDocumentMoniker, NativeMethods.HIERARCHY_DONTCHANGE, (uint)newItemId));
		}

		public IVsWindowFrame CreateDocumentWindow(int attributes, string documentMoniker, string ownerCaption, string editorCaption, Guid editorType, string physicalView, Guid commandUIGuid, object documentView, object documentData, int owningItemId, IVsUIHierarchy owningHierarchy, Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider)
		{
			Diag.Trace();
			IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(documentView);
			IntPtr iUnknownForObject2 = Marshal.GetIUnknownForObject(documentData);
			IVsWindowFrame ppWindowFrame = null;
			try
			{
				IVsUIShell service = _hostService.GetService<SVsUIShell, IVsUIShell>();
				NativeMethods.WrapComCall(service.CreateDocumentWindow((uint)attributes, documentMoniker, owningHierarchy, (uint)owningItemId, iUnknownForObject, iUnknownForObject2, ref editorType, physicalView, ref commandUIGuid, serviceProvider, ownerCaption, editorCaption, null, out ppWindowFrame));
				return ppWindowFrame;
			}
			finally
			{
				Marshal.Release(iUnknownForObject2);
				Marshal.Release(iUnknownForObject);
			}
		}

		public void PostExecuteCommand(CommandID command)
		{
			Diag.Trace();
			if (Thread.CurrentThread == _hostService.UIThread)
			{
				PostExecuteCommandImpl(command);
				return;
			}

			try
			{
				_hostService.BeginInvokeOnUIThread(new PostExecuteCommandDelegate(PostExecuteCommandImpl), command);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}

		private void PostExecuteCommandImpl(CommandID command)
		{
			Diag.Trace();
			try
			{
				IVsUIShell service = _hostService.GetService<SVsUIShell, IVsUIShell>();
				Guid pguidCmdGroup = command.Guid;
				uint iD = (uint)command.ID;
				object pvaIn = null;
				NativeMethods.WrapComCall(service.PostExecCommand(ref pguidCmdGroup, iD, 0u, ref pvaIn));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}

		public void QueryDesignerProviderTelemetry(int provider)
		{
			Diag.Trace();
			if (Thread.CurrentThread == _hostService.UIThread)
			{
				QueryDesignerProviderTelemetryImpl(provider);
				return;
			}

			_hostService.BeginInvokeOnUIThread(new QueryDesignerProviderTelemetryDelegate(QueryDesignerProviderTelemetryImpl), provider);
		}

		private void QueryDesignerProviderTelemetryImpl(int provider)
		{
			Diag.Trace();
			try
			{
				UserTaskEvent userTaskEvent = new UserTaskEvent("VS/VSData/DataViewQueryCommandProvider/Create-Query", TelemetryResult.Success);
				QualityMetricProvider.QueryDesignerProviderEventProperty(userTaskEvent, provider);
				TelemetryService.DefaultSession.PostEvent(userTaskEvent);
			}
			catch (Exception)
			{
			}
		}

		public void SqlObjectSelectorTelemetry(string metric, int count)
		{
			Diag.Trace();
			ServerExplorerTelemetry(metric, "VS.VSData.SqlObjectSelector.ObjectCount", count);
		}

		public void SqlSynonymSelectorTelemetry(string metric, int count)
		{
			Diag.Trace();
			ServerExplorerTelemetry(metric, "VS.VSData.SqlSynonymSelector.ObjectCount", count);
		}

		public void ServerExplorerTelemetry(string metric, string property, int count)
		{
			Diag.Trace();
			if (Thread.CurrentThread == _hostService.UIThread)
			{
				ServerExplorerTelemetryImpl(metric, property, count);
				return;
			}

			_hostService.BeginInvokeOnUIThread(new ServerExplorerTelemetryDelegate(ServerExplorerTelemetryImpl), metric, property, count);
		}

		private void ServerExplorerTelemetryImpl(string metric, string property, int count)
		{
			Diag.Trace();
			try
			{
				OperationEvent operationEvent = new OperationEvent(metric, TelemetryResult.Success);
				operationEvent.Properties[property] = new TelemetryMetricProperty(count);
				TelemetryService.DefaultSession.PostEvent(operationEvent);
			}
			catch (Exception)
			{
			}
		}

		public T TryGetService<T>()
		{
			Diag.Trace();
			return _hostService.TryGetService<T>();
		}

		public TInterface TryGetService<TService, TInterface>()
		{
			Diag.Trace();
			return _hostService.TryGetService<TService, TInterface>();
		}

		public T TryGetService<T>(Guid serviceGuid)
		{
			Diag.Trace();
			return _hostService.TryGetService<T>(serviceGuid);
		}

		public T GetService<T>()
		{
			Diag.Trace();
			return _hostService.GetService<T>();
		}

		public TInterface GetService<TService, TInterface>()
		{
			Diag.Trace();
			return _hostService.GetService<TService, TInterface>();
		}

		public T GetService<T>(Guid serviceGuid)
		{
			Diag.Trace();
			return _hostService.GetService<T>(serviceGuid);
		}


	}
}
