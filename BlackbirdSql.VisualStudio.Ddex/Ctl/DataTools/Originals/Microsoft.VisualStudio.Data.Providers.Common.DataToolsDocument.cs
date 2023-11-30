// Microsoft.VisualStudio.Data.Providers.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Providers.Common.DataToolsDocument
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Interop;
using Microsoft.VisualStudio.Data.Providers.Common;
using Microsoft.VisualStudio.Data.Providers.Common.Properties;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Data.Services.SupportEntities.Interop;
using Microsoft.VisualStudio.DataTools.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

internal abstract class DataToolsDocument : IVsPersistDocData2, IVsPersistDocData, IVsFileBackup
{
	private const string s_monikerFormat = "DataExplorer://{0}/{1}/{2}";

	private string _moniker;

	private Guid _editorType;

	private string _caption;

	private Guid _commandUIGuid;

	private bool _isReadOnly;

	private bool _isDirty;

	private bool _isReloadable;

	private Guid _toolFactoryGuid;

	private IVsWindowFrame _windowFrame;

	private IDTDocTool _underlyingDocument;

	[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
	private int _documentCookie;

	private object _dsRef;

	private int _owningItemId;

	private IVsDataViewHierarchy _owningHierarchy;

	private System.IServiceProvider _serviceProvider;

	private Host _host;

	public string Moniker
	{
		get
		{
			return _moniker;
		}
		set
		{
			_moniker = value;
		}
	}

	public Guid EditorType
	{
		get
		{
			if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
			{
				NativeMethods.WrapComCall(vsPersistDocData.GetGuidEditorType(out _editorType));
			}
			return _editorType;
		}
	}

	public string Caption
	{
		set
		{
			_caption = value;
			if (_windowFrame != null)
			{
				NativeMethods.WrapComCall(_windowFrame.SetProperty(-4001, value));
			}
		}
	}

	public Guid CommandUIGuid
	{
		get
		{
			if (_windowFrame != null)
			{
				NativeMethods.WrapComCall(_windowFrame.GetGuidProperty(-4007, out _commandUIGuid));
			}
			return _commandUIGuid;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			return _isReadOnly;
		}
		set
		{
			_isReadOnly = value;
		}
	}

	public bool IsDirty
	{
		get
		{
			UpdateIsDirty();
			return _isDirty;
		}
		set
		{
			_isDirty = value;
		}
	}

	public bool IsReloadable
	{
		get
		{
			if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
			{
				int pfReloadable = 0;
				NativeMethods.WrapComCall(vsPersistDocData.IsDocDataReloadable(out pfReloadable));
				_isReloadable = pfReloadable != 0;
			}
			return _isReloadable;
		}
	}

	public IVsWindowFrame WindowFrame
	{
		get
		{
			if (_windowFrame == null)
			{
				_windowFrame = CreateWindowFrame();
			}
			return _windowFrame;
		}
	}

	public IDTDocTool UnderlyingDocument
	{
		get
		{
			if (_underlyingDocument == null)
			{
				_underlyingDocument = CreateUnderlyingDocument();
			}
			return _underlyingDocument;
		}
	}

	public object DSRef
	{
		get
		{
			if (_dsRef == null)
			{
				_dsRef = BuildDSRef();
			}
			return _dsRef;
		}
	}

	public int OwningItemId => _owningItemId;

	public IVsDataViewHierarchy OwningHierarchy => _owningHierarchy;

	public System.IServiceProvider ServiceProvider => _serviceProvider;

	protected static string MonikerFormat => "DataExplorer://{0}/{1}/{2}";

	protected virtual int WindowFrameAttributes
	{
		get
		{
			int num = 65680;
			if (IsReadOnly)
			{
				num |= 0x28;
			}
			return num;
		}
	}

	protected Host Host
	{
		get
		{
			if (_host == null)
			{
				_host = new Host(_serviceProvider);
			}
			return _host;
		}
	}

	protected DataToolsDocument(Guid editorType, Guid commandUIGuid, Guid toolFactoryGuid, int owningItemId, IVsDataViewHierarchy owningHierarchy)
		: this((string)null, editorType, commandUIGuid, toolFactoryGuid, owningItemId, owningHierarchy)
	{
	}

	protected DataToolsDocument(string moniker, Guid editorType, Guid commandUIGuid, Guid toolFactoryGuid, int owningItemId, IVsDataViewHierarchy owningHierarchy)
	{
		_moniker = moniker;
		_editorType = editorType;
		_commandUIGuid = commandUIGuid;
		_toolFactoryGuid = toolFactoryGuid;
		_owningItemId = owningItemId;
		_owningHierarchy = owningHierarchy;
		_serviceProvider = owningHierarchy.ServiceProvider;
	}

	protected DataToolsDocument(IVsDataObject obj, Guid editorType, Guid commandUIGuid, Guid toolFactoryGuid, int owningItemId, IVsDataViewHierarchy owningHierarchy)
		: this(string.Format(null, "DataExplorer://{0}/{1}/{2}", owningHierarchy.ExplorerConnection.DisplayName, obj.Type.Name, obj.Identifier.ToString()), editorType, commandUIGuid, toolFactoryGuid, owningItemId, owningHierarchy)
	{
		Caption = string.Format(null, Resources.DataToolsDocument_Caption, BuildConnectionName(owningHierarchy.ExplorerConnection.Connection), GetObjectTypeDisplayName(obj, owningItemId, owningHierarchy), obj.Identifier.ToString(DataObjectIdentifierFormat.ForDisplay));
	}

	public static bool ActivateIfOpen(Host host, IVsDataExplorerNode node)
	{
		return host.ActivateDocumentIfOpen(string.Format(null, "DataExplorer://{0}/{1}/{2}", node.ExplorerConnection.DisplayName, node.Object.Type.Name, node.Object.Identifier.ToString()));
	}

	public virtual void Register(int documentCookie)
	{
		if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
		{
			NativeMethods.WrapComCall(vsPersistDocData.OnRegisterDocData((uint)documentCookie, _owningHierarchy.ExplorerConnection as IVsHierarchy, (uint)_owningItemId));
		}
		_documentCookie = documentCookie;
	}

	public virtual void InitializeNew(string untitledMoniker)
	{
		if (!(UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData))
		{
			throw new NotImplementedException();
		}
		NativeMethods.WrapComCall(vsPersistDocData.SetUntitledDocPath(untitledMoniker));
	}

	public virtual void Load(string moniker)
	{
		if (!(UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData))
		{
			throw new NotImplementedException();
		}
		NativeMethods.WrapComCall(vsPersistDocData.LoadDocData(moniker));
	}

	public virtual void Show()
	{
		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			NativeMethods.WrapComCall(WindowFrame.Show());
		}
	}

	public virtual void Reload(int flags)
	{
		if (!(UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData))
		{
			throw new NotImplementedException();
		}
		NativeMethods.WrapComCall(vsPersistDocData.ReloadDocData((uint)flags));
	}

	public virtual void Rename(string newMoniker, int attributes)
	{
		if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
		{
			NativeMethods.WrapComCall(vsPersistDocData.RenameDocData((uint)attributes, _owningHierarchy.ExplorerConnection as IVsHierarchy, (uint)_owningItemId, newMoniker));
		}
		_moniker = newMoniker;
	}

	public virtual bool Save(VSSAVEFLAGS saveFlags)
	{
		if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
		{
			string pbstrMkDocumentNew = null;
			int pfSaveCanceled = 0;
			NativeMethods.WrapComCall(vsPersistDocData.SaveDocData(saveFlags, out pbstrMkDocumentNew, out pfSaveCanceled));
			if (pbstrMkDocumentNew != null)
			{
				_moniker = pbstrMkDocumentNew;
			}
			return pfSaveCanceled == 0;
		}
		bool flag = false;
		while (true)
		{
			if (!flag)
			{
				DialogResult dialogResult = PromptBeforeSaving();
				if (dialogResult == DialogResult.Cancel)
				{
					return false;
				}
			}
			try
			{
				UnderlyingDocument.Save(DSRef, null, flag);
			}
			catch (COMException ex)
			{
				if (ex.ErrorCode == -2147286960 || ex.ErrorCode == -2147286783)
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
				else if (ex.ErrorCode == -2147217842 || ex.ErrorCode == -2147221492)
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
		if (_underlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
		{
			vsPersistDocData.Close();
		}
		_windowFrame = null;
		_underlyingDocument = null;
		_documentCookie = 0;
		_dsRef = null;
		_owningItemId = -1;
		_owningHierarchy = null;
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
			string moniker = _moniker;
			pfSaveCanceled = ((!Save(dwSave)) ? 1 : 0);
			if (pfSaveCanceled == 0 && !string.Equals(moniker, _moniker, StringComparison.Ordinal))
			{
				pbstrMkDocumentNew = _moniker;
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
			_owningItemId = (int)itemidNew;
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
		if (connection.GetService(typeof(Microsoft.VisualStudio.Data.Services.SupportEntities.IVsDataSourceInformation)) is Microsoft.VisualStudio.Data.Services.SupportEntities.IVsDataSourceInformation vsDataSourceInformation)
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
		{
			text = viewCommonNodeInfo.TypeDisplayName;
		}
		if (text == null)
		{
			text = obj.Type.Name;
		}
		return text;
	}

	protected virtual IVsWindowFrame CreateWindowFrame()
	{
		IVsWindowFrame vsWindowFrame = null;
		vsWindowFrame = Host.CreateDocumentWindow(WindowFrameAttributes, _moniker, _caption, null, EditorType, null, CommandUIGuid, UnderlyingDocument.DocView, this, _owningItemId, _owningHierarchy.ExplorerConnection as IVsUIHierarchy, _serviceProvider as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
		IVsTrackSelectionEx vsTrackSelectionEx = null;
		System.IServiceProvider serviceProvider = null;
		object pvar = null;
		NativeMethods.WrapComCall(vsWindowFrame.GetProperty(-3002, out pvar));
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
				IntPtr ppHier;
				uint pitemid;
				IVsMultiItemSelect ppMIS;
				IntPtr ppSC;
				int currentSelection = vsTrackSelectionEx.GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC);
				intPtr = Marshal.GetComInterfaceForObject(_owningHierarchy.ExplorerConnection, typeof(IVsHierarchy));
				NativeMethods.WrapComCall(vsTrackSelectionEx.OnSelectChangeEx(intPtr, (uint)_owningItemId, null, ppSC));
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.Release(intPtr);
				}
			}
		}
		UnderlyingDocument.OnPostCreateFrame(vsWindowFrame);
		return vsWindowFrame;
	}

	protected IDTDocTool CreateUnderlyingDocument()
	{
		IDTDocTool iDTDocTool = null;
		_owningHierarchy.ExplorerConnection.Connection.EnsureConnected();
		IDTDocToolFactoryProvider service = Host.GetService<IDTDocToolFactoryProvider>();
		Microsoft.VisualStudio.Data.Interop.IVsDataConnection vsDataConnection = null;
		Microsoft.VisualStudio.Data.Interop.IVsDataConnectionManager service2 = Host.GetService<Microsoft.VisualStudio.Data.Interop.IVsDataConnectionManager>();
		Guid guidProvider = _owningHierarchy.ExplorerConnection.Provider;
		vsDataConnection = service2.GetDataConnection(ref guidProvider, _owningHierarchy.ExplorerConnection.EncryptedConnectionString, fEncryptedString: true);
		object factory = service.CreateToolFactory(vsDataConnection, ref _toolFactoryGuid);
		iDTDocTool = CreateUnderlyingDocument(factory);
		iDTDocTool.Initialize();
		return iDTDocTool;
	}

	protected abstract IDTDocTool CreateUnderlyingDocument(object factory);

	protected virtual object BuildDSRef()
	{
		IVsDataExplorerNode vsDataExplorerNode = _owningHierarchy.ExplorerConnection.FindNode(_owningItemId);
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
		type?.Services.TryGetValue(typeof(Microsoft.VisualStudio.Data.Services.SupportEntities.IDSRefBuilder).FullName, out value);
		Type type2 = null;
		if (value != null)
		{
			type2 = value.ImplementationType;
		}
		Microsoft.VisualStudio.Data.Services.SupportEntities.IDSRefBuilder iDSRefBuilder = null;
		if (type2 != null)
		{
			iDSRefBuilder = Activator.CreateInstance(type2) as Microsoft.VisualStudio.Data.Services.SupportEntities.IDSRefBuilder;
			if (iDSRefBuilder is IVsDataSiteableObject<Microsoft.VisualStudio.Data.Services.IVsDataConnection> vsDataSiteableObject)
			{
				vsDataSiteableObject.Site = OwningHierarchy.ExplorerConnection.Connection;
			}
		}
		if (iDSRefBuilder == null)
		{
			iDSRefBuilder = OwningHierarchy.ExplorerConnection.Connection.GetService(typeof(Microsoft.VisualStudio.Data.Services.SupportEntities.IDSRefBuilder)) as Microsoft.VisualStudio.Data.Services.SupportEntities.IDSRefBuilder;
		}
		if (iDSRefBuilder == null)
		{
			iDSRefBuilder = new DSRefBuilder(OwningHierarchy.ExplorerConnection.Connection);
		}
		object obj = Host.CreateLocalInstance(NativeMethods.CLSID_DSRef);
		if (identifier != null)
		{
			if (iDSRefBuilder is IVsDataSupportObject<Microsoft.VisualStudio.Data.Services.SupportEntities.IDSRefBuilder> vsDataSupportObject && value != null)
			{
				object[] parameters = value.GetParameters("AppendToDSRef");
				vsDataSupportObject.Invoke("AppendToDSRef", new object[3] { obj, type.Name, identifier }, parameters);
			}
			else
			{
				iDSRefBuilder.AppendToDSRef(obj, type.Name, identifier);
			}
		}
		return obj;
	}

	protected string GetObjectSchemaFromDSRef()
	{
		IntPtr zero = IntPtr.Zero;
		IDSRefConsumer iDSRefConsumer = DSRef as IDSRefConsumer;
		IntPtr firstChildNode = iDSRefConsumer.GetFirstChildNode(DSRefConstants.DSREFNODEID_ROOT);
		zero = iDSRefConsumer.GetFirstChildNode(firstChildNode);
		return iDSRefConsumer.GetOwner(zero);
	}

	protected string GetObjectNameFromDSRef()
	{
		IntPtr zero = IntPtr.Zero;
		IDSRefConsumer iDSRefConsumer = DSRef as IDSRefConsumer;
		IntPtr firstChildNode = iDSRefConsumer.GetFirstChildNode(DSRefConstants.DSREFNODEID_ROOT);
		zero = iDSRefConsumer.GetFirstChildNode(firstChildNode);
		return iDSRefConsumer.GetName(zero);
	}

	protected void SetObjectNameInDSRef(string name)
	{
		IntPtr zero = IntPtr.Zero;
		IDSRefConsumer iDSRefConsumer = DSRef as IDSRefConsumer;
		IntPtr firstChildNode = iDSRefConsumer.GetFirstChildNode(DSRefConstants.DSREFNODEID_ROOT);
		zero = iDSRefConsumer.GetFirstChildNode(firstChildNode);
		IDSRefProvider iDSRefProvider = DSRef as IDSRefProvider;
		iDSRefProvider.SetName(zero, name);
	}

	protected virtual void UpdateIsDirty()
	{
		if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
		{
			int pfDirty = 0;
			NativeMethods.WrapComCall(vsPersistDocData.IsDocDataDirty(out pfDirty));
			_isDirty = pfDirty != 0;
		}
		else
		{
			_isDirty = UnderlyingDocument.IsDirty();
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
