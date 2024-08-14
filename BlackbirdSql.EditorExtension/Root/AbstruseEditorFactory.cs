// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.BaseEditorFactory

using System;
using BlackbirdSql.EditorExtension.Ctl.Config;
using BlackbirdSql.Shared.Controls.Tabs;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.EditorExtension;


// =========================================================================================================
//
//											AbstruseEditorFactory Class
//
/// <summary>
/// Abstract base class for <see cref="AbstractEditorFactory"/> and <see cref="EditorFactoryResults"/>.
/// </summary>
// =========================================================================================================
public abstract class AbstruseEditorFactory : IVsEditorFactory
{

	// -------------------------------------------------------
	#region Constructors / Destructors - AbstruseEditorFactory
	// -------------------------------------------------------


	public AbstruseEditorFactory(bool encoded)
	{
		_Encoded = encoded;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstruseEditorFactory
	// =========================================================================================================


	private readonly bool _Encoded;
	private Guid _MandatedSqlLanguageServiceClsid = Guid.Empty;
	private Microsoft.VisualStudio.OLE.Interop.IServiceProvider _OleServiceProvider = null;
	private IServiceProvider _ServiceProvider = null;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstruseEditorFactory
	// =========================================================================================================


	public abstract Guid ClsidEditorFactory { get; }

	public bool Encoded => _Encoded;


	protected virtual Guid MandatedSqlLanguageServiceClsid => _MandatedSqlLanguageServiceClsid == Guid.Empty
		? _MandatedSqlLanguageServiceClsid = new(PersistentSettings.MandatedLanguageServiceGuid)
		: _MandatedSqlLanguageServiceClsid;


	protected Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider => _OleServiceProvider;

	protected IServiceProvider ServiceProvider => _ServiceProvider;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstruseEditorFactory
	// =========================================================================================================


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
			return VSConstants.E_NOTIMPL;
		}

		if (logicalView.Equals(VSConstants.LOGVIEWID_Primary)
			|| logicalView.Equals(VSConstants.LOGVIEWID_TextView))
		{
			strLogicalView = null;
			return VSConstants.S_OK;
		}


		if (logicalView.Equals(VSConstants.LOGVIEWID_Code))
		{
			strLogicalView = EditorCodeTab.S_FramePhysicalViewString;
			return VSConstants.S_OK;
		}

		// Tracer.Trace(GetType(), traceName, "IVsEditorFactory.MapLogicalView", "returning E_NOTIMPL");
		strLogicalView = null;

		return VSConstants.E_NOTIMPL;
	}



	public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider)
	{
		// Tracer.Trace(GetType(), "IVsEditorFactory.SetSite", "");
		_OleServiceProvider = serviceProvider;
		_ServiceProvider = new Microsoft.VisualStudio.Shell.ServiceProvider(serviceProvider);

		return VSConstants.S_OK;
	}



	public int Close()
	{
		// Tracer.Trace(GetType(), "IVsEditorFactory.Close", "");
		_OleServiceProvider = null;
		_ServiceProvider = null;

		return VSConstants.S_OK;
	}


	#endregion Methods

}
