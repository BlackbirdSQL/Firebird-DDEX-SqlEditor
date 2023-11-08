// Microsoft.VisualStudio.Data.Providers.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Providers.Common.DataToolsDocument
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Extensions;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Data.Services.SupportEntities.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

using static BlackbirdSql.Core.Ctl.CommandProviders.CommandProperties;

using IDSRefConsumer = Microsoft.VisualStudio.Data.Services.SupportEntities.Interop.IDSRefConsumer;
using IDSRefProvider = Microsoft.VisualStudio.Data.Services.SupportEntities.Interop.IDSRefProvider;
using IDTDocTool = Microsoft.VisualStudio.DataTools.Interop.IDTDocTool;
using IDTDocToolFactoryProvider = Microsoft.VisualStudio.DataTools.Interop.IDTDocToolFactoryProvider;
using IVsDataConnection = BlackbirdSql.Common.Ctl.DataTools.Interfaces.Interop.IVsDataConnection;
using IVsDataConnectionManager = BlackbirdSql.Common.Ctl.DataTools.Interfaces.Interop.IVsDataConnectionManager;
using ServiceProvider = Microsoft.VisualStudio.Data.Framework.ServiceProvider;
using Tracer = BlackbirdSql.Core.Ctl.Diagnostics.Tracer;

namespace BlackbirdSql.Common.Ctl.DataTools;

/// <summary>
/// This roadblocks because key services in DataTools.Interop are protected.
/// </summary>
public abstract class AbstractDataToolsDocument : IVsPersistDocData2, IVsPersistDocData, IVsFileBackup
{

	private const string C_MonikerFormat = "DataExplorer://{0}/{1}/{2}";

	private string _Moniker;

	private Guid _EditorType;

	private string _Caption;

	private Guid _CommandUIGuid;

	private bool _IsReadOnly;

	private bool _IsDirty;

	private bool _IsReloadable;

	private Guid _ToolFactoryGuid;

	private IVsWindowFrame _WindowFrame;

	private IDTDocTool _UnderlyingDocument;
	private int _DocumentCookie;

	private object _DsRef;

	private int _OwningItemId;

	private IVsDataViewHierarchy _OwningHierarchy;

	private readonly IServiceProvider _ServiceProvider;

	private Hostess _Host;

	private EnNodeSystemType _NodeSystemType = EnNodeSystemType.Global;

	public string Moniker
	{
		get
		{
			return _Moniker;
		}
		set
		{
			_Moniker = value;
		}
	}

	public Guid EditorType
	{
		get
		{
			if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
			{
				Native.WrapComCall(vsPersistDocData.GetGuidEditorType(out _EditorType));
			}
			return _EditorType;
		}
	}

	public string Caption
	{
		set
		{
			_Caption = value;
			if (_WindowFrame != null)
			{
				Native.WrapComCall(_WindowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_OwnerCaption, value));
			}
		}
	}

	public Guid CommandUIGuid
	{
		get
		{
			if (_WindowFrame != null)
			{
				Native.WrapComCall(_WindowFrame.GetGuidProperty((int)__VSFPROPID.VSFPROPID_CmdUIGuid, out _CommandUIGuid));
			}
			return _CommandUIGuid;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			return _IsReadOnly;
		}
		set
		{
			_IsReadOnly = value;
		}
	}

	public bool IsDirty
	{
		get
		{
			UpdateIsDirty();
			return _IsDirty;
		}
		set
		{
			_IsDirty = value;
		}
	}

	public bool IsReloadable
	{
		get
		{
			if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
			{
				Native.WrapComCall(vsPersistDocData.IsDocDataReloadable(out int pfReloadable));
				_IsReloadable = pfReloadable != 0;
			}
			return _IsReloadable;
		}
	}

	public IVsWindowFrame WindowFrame => _WindowFrame ??= CreateWindowFrame();

	public IDTDocTool UnderlyingDocument => _UnderlyingDocument ??= CreateUnderlyingDocument();


	public object DSRef => _DsRef ??= BuildDSRef();


	public int OwningItemId => _OwningItemId;

	public IVsDataViewHierarchy OwningHierarchy => _OwningHierarchy;

	public IServiceProvider ServiceProvider => _ServiceProvider;

	protected static string MonikerFormat => C_MonikerFormat;

	protected virtual int WindowFrameAttributes
	{
		get
		{
			_VSRDTFLAGS flags = _VSRDTFLAGS.RDT_NonCreatable | _VSRDTFLAGS.RDT_CaseSensitive
				| _VSRDTFLAGS.RDT_DontAddToMRU;

			if (IsReadOnly)
				flags |= _VSRDTFLAGS.RDT_RequestUnlock | _VSRDTFLAGS.RDT_DontSaveAs;

			return (int)flags;
		}
	}

	protected Hostess Host => _Host ??= new(_ServiceProvider);

	protected EnNodeSystemType NodeSystemType => _NodeSystemType;


	protected AbstractDataToolsDocument(Guid editorType, Guid commandUIGuid, Guid toolFactoryGuid,
		int owningItemId, IVsDataViewHierarchy owningHierarchy)
		: this((string)null, editorType, commandUIGuid, toolFactoryGuid, owningItemId, owningHierarchy)
	{
	}

	protected AbstractDataToolsDocument(string moniker, Guid editorType, Guid commandUIGuid,
		Guid toolFactoryGuid, int owningItemId, IVsDataViewHierarchy owningHierarchy)
	{
		_Moniker = moniker;
		_EditorType = editorType;
		_CommandUIGuid = commandUIGuid;
		_ToolFactoryGuid = toolFactoryGuid;
		_OwningItemId = owningItemId;
		_OwningHierarchy = owningHierarchy;
		_ServiceProvider = owningHierarchy.ServiceProvider;
	}

	protected AbstractDataToolsDocument(IVsDataObject obj, Guid editorType, Guid commandUIGuid,
		Guid toolFactoryGuid, int owningItemId, IVsDataViewHierarchy owningHierarchy)
		: this(string.Format(null, C_MonikerFormat, owningHierarchy.ExplorerConnection.DisplayName,
			obj.Type.Name, obj.Identifier.ToString()), editorType, commandUIGuid, toolFactoryGuid,
			  owningItemId, owningHierarchy)
	{
		try
		{
			Caption = Resources.DataToolsDocument_Caption.FmtRes(
				BuildConnectionName(owningHierarchy.ExplorerConnection.Connection),
				GetObjectTypeDisplayName(obj, owningItemId, owningHierarchy),
				obj.Identifier.ToString(DataObjectIdentifierFormat.ForDisplay));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}

	public static bool ActivateIfOpen(Hostess host, IVsDataExplorerNode node)
	{
		return host.ActivateDocumentIfOpen(string.Format(null, C_MonikerFormat,
			node.ExplorerConnection.DisplayName, node.Object.Type.Name, node.Object.Identifier.ToString()));
	}

	public virtual void Register(int documentCookie)
	{
		if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
		{
			Native.WrapComCall(vsPersistDocData.OnRegisterDocData((uint)documentCookie,
				_OwningHierarchy.ExplorerConnection as IVsHierarchy, (uint)_OwningItemId));
		}
		_DocumentCookie = documentCookie;
		_ = _DocumentCookie; // Suppression
	}

	public virtual void InitializeNew(string untitledMoniker)
	{
		if (UnderlyingDocument.DocData is not IVsPersistDocData vsPersistDocData)
		{
			throw new NotImplementedException();
		}
		Native.WrapComCall(vsPersistDocData.SetUntitledDocPath(untitledMoniker));
	}

	public virtual void Load(string moniker)
	{
		if (UnderlyingDocument.DocData is not IVsPersistDocData vsPersistDocData)
		{
			throw new NotImplementedException();
		}
		Native.WrapComCall(vsPersistDocData.LoadDocData(moniker));
	}


	public virtual void Show(EnNodeSystemType nodeSystemType)
	{
		_NodeSystemType = nodeSystemType;
		Show();
	}


	public virtual void Show()
	{
		int hr;

		try
		{
			using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
			{
				hr = WindowFrame.Show();
			}
			Native.WrapComCall(hr);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

	}

	public virtual void Reload(int flags)
	{
		if (UnderlyingDocument.DocData is not IVsPersistDocData vsPersistDocData)
		{
			throw new NotImplementedException();
		}
		Native.WrapComCall(vsPersistDocData.ReloadDocData((uint)flags));
	}

	public virtual void Rename(string newMoniker, int attributes)
	{
		if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
		{
			Native.WrapComCall(vsPersistDocData.RenameDocData((uint)attributes,
				_OwningHierarchy.ExplorerConnection as IVsHierarchy, (uint)_OwningItemId, newMoniker));
		}
		_Moniker = newMoniker;
	}

	public virtual bool Save(VSSAVEFLAGS saveFlags)
	{
		if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
		{
			Native.WrapComCall(vsPersistDocData.SaveDocData(saveFlags, out string pbstrMkDocumentNew,
				out int pfSaveCanceled));

			if (pbstrMkDocumentNew != null)
				_Moniker = pbstrMkDocumentNew;

			return pfSaveCanceled == 0;
		}

		bool flag = false;
		while (true)
		{
			if (!flag)
			{
				DialogResult dialogResult = PromptBeforeSaving();
				if (dialogResult == DialogResult.Cancel)
					return false;
			}
			try
			{
				UnderlyingDocument.Save(DSRef, null, flag);
			}
			catch (COMException ex)
			{
				if (ex.ErrorCode == VS.STG_E_FILEALREADYEXISTS || ex.ErrorCode == VS.STG_E_NOTCURRENT) // 0x80030101
				{
					switch (PromptToOverwrite(ex.ErrorCode))
					{
						case DialogResult.Yes:
							flag = true;
							break;
						case DialogResult.No:
							flag = false;
							break;
						case DialogResult.Cancel:
							return false;
					}
				}
				else if (ex.ErrorCode == VS.OLE_E_NOTIFYCANCELLED || ex.ErrorCode == VSConstants.OLE_E_PROMPTSAVECANCELLED)
				{
					return false;
				}
				continue;
			}
			break;
		}
		OnDocumentSaved(flag);
		return true;
	}

	public virtual void Close()
	{
		if (_UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
		{
			vsPersistDocData.Close();
		}
		_WindowFrame = null;
		_UnderlyingDocument = null;
		_DocumentCookie = 0;
		_DsRef = null;
		_OwningItemId = -1;
		_OwningHierarchy = null;
	}

	public int BackupFile(string backupFileName)
	{
		return 0;
	}

	public int IsBackupFileObsolete(out int isObsolete)
	{
		isObsolete = 0;
		return 0;
	}

	int IVsPersistDocData2.GetGuidEditorType(out Guid pClassID)
	{
		return ((IVsPersistDocData)this).GetGuidEditorType(out pClassID);
	}

	int IVsPersistDocData2.IsDocDataDirty(out int pfDirty)
	{
		return ((IVsPersistDocData)this).IsDocDataDirty(out pfDirty);
	}

	int IVsPersistDocData2.SetUntitledDocPath(string pszDocDataPath)
	{
		return ((IVsPersistDocData)this).SetUntitledDocPath(pszDocDataPath);
	}

	int IVsPersistDocData2.LoadDocData(string pszMkDocument)
	{
		return ((IVsPersistDocData)this).LoadDocData(pszMkDocument);
	}

	int IVsPersistDocData2.SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
	{
		return ((IVsPersistDocData)this).SaveDocData(dwSave, out pbstrMkDocumentNew, out pfSaveCanceled);
	}

	int IVsPersistDocData2.Close()
	{
		return ((IVsPersistDocData)this).Close();
	}

	int IVsPersistDocData2.OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
	{
		return ((IVsPersistDocData)this).OnRegisterDocData(docCookie, pHierNew, itemidNew);
	}

	int IVsPersistDocData2.RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
	{
		return ((IVsPersistDocData)this).RenameDocData(grfAttribs, pHierNew, itemidNew, pszMkDocumentNew);
	}

	int IVsPersistDocData2.IsDocDataReloadable(out int pfReloadable)
	{
		return ((IVsPersistDocData)this).IsDocDataReloadable(out pfReloadable);
	}

	int IVsPersistDocData2.ReloadDocData(uint grfFlags)
	{
		return ((IVsPersistDocData)this).ReloadDocData(grfFlags);
	}

	int IVsPersistDocData2.SetDocDataDirty(int fDirty)
	{
		IsDirty = fDirty != 0;
		return 0;
	}

	int IVsPersistDocData2.IsDocDataReadOnly(out int pfReadOnly)
	{
		pfReadOnly = (IsReadOnly ? 1 : 0);
		return 0;
	}

	int IVsPersistDocData2.SetDocDataReadOnly(int fReadOnly)
	{
		IsReadOnly = fReadOnly != 0;
		return 0;
	}

	int IVsPersistDocData.GetGuidEditorType(out Guid pClassID)
	{
		int result = 0;
		pClassID = Guid.Empty;
		try
		{
			pClassID = EditorType;
		}
		catch (Exception e)
		{
			result = Marshal.GetHRForException(e);
		}
		return result;
	}

	int IVsPersistDocData.IsDocDataDirty(out int pfDirty)
	{
		int result = 0;
		pfDirty = 0;
		try
		{
			pfDirty = (IsDirty ? 1 : 0);
		}
		catch (Exception e)
		{
			result = Marshal.GetHRForException(e);
		}
		return result;
	}

	int IVsPersistDocData.SetUntitledDocPath(string pszDocDataPath)
	{
		int result = 0;
		try
		{
			InitializeNew(pszDocDataPath);
		}
		catch (Exception e)
		{
			result = Marshal.GetHRForException(e);
		}
		return result;
	}

	int IVsPersistDocData.LoadDocData(string pszMkDocument)
	{
		int result = 0;
		try
		{
			Load(pszMkDocument);
		}
		catch (Exception e)
		{
			result = Marshal.GetHRForException(e);
		}
		return result;
	}

	int IVsPersistDocData.SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
	{
		int result = 0;
		pbstrMkDocumentNew = null;
		pfSaveCanceled = 0;
		try
		{
			string moniker = _Moniker;
			pfSaveCanceled = ((!Save(dwSave)) ? 1 : 0);
			if (pfSaveCanceled == 0 && !string.Equals(moniker, _Moniker, StringComparison.Ordinal))
			{
				pbstrMkDocumentNew = _Moniker;
			}
		}
		catch (Exception e)
		{
			result = Marshal.GetHRForException(e);
		}
		return result;
	}

	int IVsPersistDocData.Close()
	{
		int result = 0;
		try
		{
			Close();
		}
		catch (Exception e)
		{
			result = Marshal.GetHRForException(e);
		}
		return result;
	}

	int IVsPersistDocData.OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
	{
		int result = 0;
		try
		{
			Register((int)docCookie);
		}
		catch (Exception e)
		{
			result = Marshal.GetHRForException(e);
		}
		return result;
	}

	int IVsPersistDocData.RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
	{
		int result = 0;
		try
		{
			Rename(pszMkDocumentNew, (int)grfAttribs);
			_OwningItemId = (int)itemidNew;
		}
		catch (Exception e)
		{
			result = Marshal.GetHRForException(e);
		}
		return result;
	}

	int IVsPersistDocData.IsDocDataReloadable(out int pfReloadable)
	{
		int result = 0;
		pfReloadable = 0;
		try
		{
			pfReloadable = (IsReloadable ? 1 : 0);
		}
		catch (Exception e)
		{
			result = Marshal.GetHRForException(e);
		}
		return result;
	}

	int IVsPersistDocData.ReloadDocData(uint grfFlags)
	{
		int result = 0;
		try
		{
			Reload((int)grfFlags);
		}
		catch (Exception e)
		{
			result = Marshal.GetHRForException(e);
		}
		return result;
	}

	protected static string BuildConnectionName(Microsoft.VisualStudio.Data.Services.IVsDataConnection connection)
	{
		string result = null;
		string text = null;
		string text2 = null;
		if (connection.GetService(typeof(IVsDataSourceInformation)) is IVsDataSourceInformation vsDataSourceInformation)
		{
			if (vsDataSourceInformation.Contains("DataSourceName"))
			{
				text = vsDataSourceInformation["DataSourceName"] as string;
			}
			if (vsDataSourceInformation.Contains("DefaultCatalog"))
			{
				text2 = vsDataSourceInformation["DefaultCatalog"] as string;
			}
		}
		if (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(text2))
		{
			result = "(";
			if (!string.IsNullOrEmpty(text))
			{
				result += text;
				if (!string.IsNullOrEmpty(text2))
				{
					result += ".";
				}
			}
			if (!string.IsNullOrEmpty(text2))
			{
				result += text2;
			}
			result += ")";
		}
		return result;
	}

	protected static string GetObjectTypeDisplayName(IVsDataObject obj, int owningItemId, IVsDataViewHierarchy owningHierarchy)
	{
		string text = null;
		IVsDataViewCommonNodeInfo viewCommonNodeInfo = owningHierarchy.GetViewCommonNodeInfo(owningItemId);

		if (viewCommonNodeInfo != null)
			text = viewCommonNodeInfo.TypeDisplayName;
		text ??= obj.Type.Name;

		return text;
	}

	protected virtual IVsWindowFrame CreateWindowFrame()
	{
		IVsWindowFrame vsWindowFrame = Host.CreateDocumentWindow(WindowFrameAttributes, _Moniker,
			_Caption, null, EditorType, null, CommandUIGuid, UnderlyingDocument.DocView, this,
			_OwningItemId, _OwningHierarchy.ExplorerConnection as IVsUIHierarchy,
			_ServiceProvider as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);

		if (vsWindowFrame == null)
		{
			InvalidComObjectException ex = new("Host.CreateDocumentWindow() returned (IVsWindowFrame)null");
			Diag.Dug(ex);
			throw ex;
		}

		IVsTrackSelectionEx vsTrackSelectionEx = null;
		IServiceProvider serviceProvider = null;
		object pvar;

		try
		{
			Native.WrapComCall(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_SPFrame, out pvar));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}


		if (pvar is Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider2)
		{
			serviceProvider = new ServiceProvider(serviceProvider2);
		}
		if (serviceProvider != null)
		{
			vsTrackSelectionEx = serviceProvider.GetService(typeof(SVsTrackSelectionEx)) as IVsTrackSelectionEx;
		}


		if (vsTrackSelectionEx != null)
		{
			IntPtr intPtr = IntPtr.Zero;
			try
			{
				int currentSelection = vsTrackSelectionEx.GetCurrentSelection(out IntPtr ppHier,
					out uint pitemid, out IVsMultiItemSelect ppMIS, out IntPtr ppSC);
				intPtr = Marshal.GetComInterfaceForObject(_OwningHierarchy.ExplorerConnection, typeof(IVsHierarchy));
				Native.WrapComCall(vsTrackSelectionEx.OnSelectChangeEx(intPtr, (uint)_OwningItemId, null, ppSC));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.Release(intPtr);
				}
			}
		}

		try
		{
			UnderlyingDocument.OnPostCreateFrame(vsWindowFrame);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		return vsWindowFrame;
	}

	protected IDTDocTool CreateUnderlyingDocument()
	{
		_OwningHierarchy.ExplorerConnection.Connection.EnsureConnected();

		IDTDocToolFactoryProvider service;


		try
		{
			service = Host.GetService<IDTDocToolFactoryProvider>();
			if (service == null)
				throw new ServiceUnavailableException(typeof(IDTDocToolFactoryProvider));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}


		IVsDataConnectionManager service2;
		// IVsDataConnectionManager service2 = Host.GetService<IVsDataConnectionManager>();
		try
		{
			service2 = Host.GetService<IVsDataConnectionManager>();
			if (service2 == null)
				throw new ServiceUnavailableException(typeof(IVsDataConnectionManager));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}


		Guid guidProvider = _OwningHierarchy.ExplorerConnection.Provider;

		IVsDataConnection vsDataConnection;
		// IVsDataConnection vsDataConnection = service2.GetConnection(guidProvider,
		//	_OwningHierarchy.ExplorerConnection.EncryptedConnectionString, encryptedString: true);
		try
		{
			vsDataConnection = service2.GetDataConnection(guidProvider, _OwningHierarchy.ExplorerConnection.EncryptedConnectionString, fEncryptedString: true);
			if (vsDataConnection == null)
				throw new InvalidComObjectException("IVsDataConnectionManager.GetDataConnection() returned (IVsDataConnection)null");
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}


		Tracer.Trace(GetType(), "CreateUnderlyingDocument", "Creating tool factory from IVsDataConnection: {0} and ToolFactoryGuid: {1}", vsDataConnection, _ToolFactoryGuid);

		object factory;
		try
		{
			factory = service.CreateToolFactory(vsDataConnection, ref _ToolFactoryGuid);
			if (factory == null)
				throw new InvalidComObjectException("IDTDocToolFactoryProvider.CreateToolFactory() returned (object)null");
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		IDTDocTool docTool;
		try
		{
			docTool = CreateUnderlyingDocument(factory);
			if (docTool == null)
				throw new InvalidComObjectException("CreateUnderlyingDocument() returned (IDTDocTool)null");
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		try
		{ 
			docTool.Initialize();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		return docTool;
	}

	protected abstract IDTDocTool CreateUnderlyingDocument(object factory);

	protected virtual object BuildDSRef()
	{
		IVsDataExplorerNode vsDataExplorerNode = _OwningHierarchy.ExplorerConnection.FindNode(_OwningItemId);
		IVsDataObject vsDataObject = null;
		if (vsDataExplorerNode != null)
		{
			vsDataObject = vsDataExplorerNode.Object;
		}
		IVsDataObjectType type = null;
		if (vsDataObject != null)
		{
			type = vsDataObject.Type;
		}
		return BuildDSRef(type, vsDataObject.Identifier.ToArray());
	}

	protected object BuildDSRef(IVsDataObjectType type, object[] identifier)
	{
		IVsDataObjectService value = null;
		type?.Services.TryGetValue(typeof(IDSRefBuilder).FullName, out value);
		Type type2 = null;
		if (value != null)
		{
			type2 = value.ImplementationType;
		}
		IDSRefBuilder dsRefBuilder = null;
		if (type2 != null)
		{
			dsRefBuilder = Activator.CreateInstance(type2) as IDSRefBuilder;
			if (dsRefBuilder is IVsDataSiteableObject<Microsoft.VisualStudio.Data.Services.IVsDataConnection> vsDataSiteableObject)
			{
				vsDataSiteableObject.Site = OwningHierarchy.ExplorerConnection.Connection;
			}
		}
		dsRefBuilder ??= OwningHierarchy.ExplorerConnection.Connection.GetService(typeof(IDSRefBuilder))
			as IDSRefBuilder;
		// IDSRefBuilder service should never be null. Defined in TConnectionSupport.
		dsRefBuilder ??= new Microsoft.VisualStudio.Data.Framework.DSRefBuilder(OwningHierarchy.ExplorerConnection.Connection); 

		object obj = Host.CreateLocalInstance(VS.CLSID_DSRef);
		if (identifier != null)
		{
			if (dsRefBuilder is IVsDataSupportObject<IDSRefBuilder> vsDataSupportObject && value != null)
			{
				object[] parameters = value.GetParameters("AppendToDSRef");
				vsDataSupportObject.Invoke("AppendToDSRef", new object[3] { obj, type.Name, identifier },
					parameters);
			}
			else
			{
				dsRefBuilder.AppendToDSRef(obj, type.Name, identifier);
			}
		}
		return obj;
	}

	protected string GetObjectSchemaFromDSRef()
	{
		IDSRefConsumer iDSRefConsumer = DSRef as IDSRefConsumer;
		IntPtr firstChildNode = iDSRefConsumer.GetFirstChildNode(DSRefConstants.DSREFNODEID_ROOT);
		IntPtr zero = iDSRefConsumer.GetFirstChildNode(firstChildNode);
		return iDSRefConsumer.GetOwner(zero);
	}

	protected string GetObjectNameFromDSRef()
	{
		IDSRefConsumer iDSRefConsumer = DSRef as IDSRefConsumer;
		IntPtr firstChildNode = iDSRefConsumer.GetFirstChildNode(DSRefConstants.DSREFNODEID_ROOT);
		IntPtr zero = iDSRefConsumer.GetFirstChildNode(firstChildNode);
		return iDSRefConsumer.GetName(zero);
	}

	protected void SetObjectNameInDSRef(string name)
	{
		IDSRefConsumer iDSRefConsumer = DSRef as IDSRefConsumer;
		IntPtr firstChildNode = iDSRefConsumer.GetFirstChildNode(DSRefConstants.DSREFNODEID_ROOT);
		IntPtr zero = iDSRefConsumer.GetFirstChildNode(firstChildNode);
		IDSRefProvider iDSRefProvider = DSRef as IDSRefProvider;
		iDSRefProvider.SetName(zero, name);
	}


	protected virtual void UpdateIsDirty()
	{
		if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
		{
			Native.WrapComCall(vsPersistDocData.IsDocDataDirty(out int pfDirty));
			_IsDirty = pfDirty != 0;
		}
		else
		{
			_IsDirty = UnderlyingDocument.IsDirty();
		}
	}

	protected virtual DialogResult PromptBeforeSaving()
	{
		return DialogResult.OK;
	}

	protected virtual DialogResult PromptToOverwrite(int reason)
	{
		return DialogResult.OK;
	}

	protected virtual void OnDocumentSaved(bool overwriteExisting)
	{
	}
}
