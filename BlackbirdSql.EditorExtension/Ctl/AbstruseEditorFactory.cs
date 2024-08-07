﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.BaseEditorFactory

using System;
using System.Runtime.InteropServices;
using BlackbirdSql.EditorExtension.Ctl.Config;
using BlackbirdSql.Shared.Controls.Tabs;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.EditorExtension.Ctl;


public abstract class AbstruseEditorFactory : IVsEditorFactory
{
	public AbstruseEditorFactory(bool withEncoding)
	{
		WithEncoding = withEncoding;
	}



	public bool WithEncoding { get; private set; }



	protected static readonly string traceName = "BlackbirdSql.EditorExtension.AbstruseEditorFactory";

	private Guid _MandatedSqlLanguageServiceClsid = Guid.Empty;

	public uint Cookie { get; set; }

	public abstract Guid ClsidEditorFactory { get; }

	protected virtual Guid MandatedSqlLanguageServiceClsid
	{
		get
		{
			if (_MandatedSqlLanguageServiceClsid == Guid.Empty)
				_MandatedSqlLanguageServiceClsid = new(PersistentSettings.MandatedLanguageServiceGuid);

			return _MandatedSqlLanguageServiceClsid;
		}
	}

	protected Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider { get; private set; }

	protected IServiceProvider ServiceProvider { get; private set; }





	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	public abstract int CreateEditorInstance(uint createFlags, string moniker, string physicalViewName,
		IVsHierarchy hierarchy, uint itemId, IntPtr pExistingDocData, out IntPtr pDocView,
		out IntPtr pDocData, out string caption, out Guid cmdUIGuid, out int hresult);



	public int MapLogicalView(ref Guid logicalView, out string strLogicalView)
	{
		// Tracer.Trace(GetType(), "IVsEditorFactory.MapLogicalView", "logicalView = {0}", logicalView.ToString());
		if (logicalView.Equals(VSConstants.LOGVIEWID_Debugging))
		{
			Diag.StackException("Debugging view not supported");
			strLogicalView = null;
			return VSConstants.S_OK;
		}

		if (logicalView.Equals(VSConstants.LOGVIEWID_Primary)
			|| logicalView.Equals(VSConstants.LOGVIEWID_TextView))
		{
			strLogicalView = null;
			return VSConstants.S_OK;
		}


		if (logicalView.Equals(VSConstants.LOGVIEWID_Code))
		{
			strLogicalView = SqlEditorCodeTab.S_FramePhysicalViewString;
			return VSConstants.S_OK;
		}

		// Tracer.Trace(GetType(), traceName, "IVsEditorFactory.MapLogicalView", "returning E_NOTIMPL");
		strLogicalView = null;

		return VSConstants.E_NOTIMPL;
	}

	public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider)
	{
		// Tracer.Trace(GetType(), "IVsEditorFactory.SetSite", "");
		OleServiceProvider = serviceProvider;
		ServiceProvider = new Microsoft.VisualStudio.Shell.ServiceProvider(serviceProvider);
		return VSConstants.S_OK;
	}

	public int Close()
	{
		// Tracer.Trace(GetType(), "IVsEditorFactory.Close", "");
		OleServiceProvider = null;
		ServiceProvider = null;
		return VSConstants.S_OK;
	}
}
