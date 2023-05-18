using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Windows.Forms;

using Microsoft;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Data.Services.SupportEntities.Interop;
using Microsoft.VisualStudio.DataTools.Interop;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;




namespace BlackbirdSql.Common.Providers;


[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]



// [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]



/// <summary>
/// Partly Plagiarized from <see cref="Microsoft.VisualStudio.Data.Providers.Common.DataToolsDocument"/>.
/// </summary>
internal abstract class AbstractDataToolsDocument : IVsPersistDocData2, IVsPersistDocData, IVsFileBackup
{
	private string _Moniker;

	private Guid _EditorType;

	private string _Caption;

	private Guid _CommandUIGuid;

	private bool _IsReadOnly;

	private bool _IsDirty;

	private bool _IsReloadable;

	protected Guid _ToolFactoryGuid;

	private IVsWindowFrame _WindowFrame;

	private IDTDocTool _UnderlyingDocument;

	private object _DsRef;

	private int _OwningItemId;

	protected int _DocumentCookie;

	private IVsDataViewHierarchy _OwningHierarchy;

	private readonly IServiceProvider _ServiceProvider;

	private Hostess _Host;

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
				NativeMethods.WrapComCall(vsPersistDocData.GetGuidEditorType(out _EditorType));
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
				NativeMethods.WrapComCall(_WindowFrame.SetProperty(-4001, value));
			}
		}
	}

	public Guid CommandUIGuid
	{
		get
		{
			if (_WindowFrame != null)
			{
				NativeMethods.WrapComCall(_WindowFrame.GetGuidProperty(-4007, out _CommandUIGuid));
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
				NativeMethods.WrapComCall(vsPersistDocData.IsDocDataReloadable(out int pfReloadable));
				_IsReloadable = pfReloadable != 0;
			}

			return _IsReloadable;
		}
	}

	public IVsWindowFrame WindowFrame
	{
		get
		{
			_WindowFrame ??= CreateWindowFrame();

			return _WindowFrame;
		}
	}

	public IDTDocTool UnderlyingDocument
	{
		get
		{
			_UnderlyingDocument ??= CreateUnderlyingDocument();

			return _UnderlyingDocument;
		}
	}

	public object DSRef
	{
		get
		{
			_DsRef ??= BuildDSRef();

			return _DsRef;
		}
	}

	public int OwningItemId => _OwningItemId;

	public IVsDataViewHierarchy OwningHierarchy => _OwningHierarchy;

	public IServiceProvider ServiceProvider => _ServiceProvider;

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

	protected Hostess Host
	{
		get
		{
			_Host ??= new(_ServiceProvider);

			return _Host;
		}
	}

	protected AbstractDataToolsDocument(Guid editorType, Guid commandUIGuid, Guid toolFactoryGuid, int owningItemId, IVsDataViewHierarchy owningHierarchy)
		: this((string)null, editorType, commandUIGuid, toolFactoryGuid, owningItemId, owningHierarchy)
	{
		// Diag.Trace();
	}

	protected AbstractDataToolsDocument(string moniker, Guid editorType, Guid commandUIGuid, Guid toolFactoryGuid, int owningItemId, IVsDataViewHierarchy owningHierarchy)
	{
		// Diag.Trace();
		_Moniker = moniker;
		_EditorType = editorType;
		_CommandUIGuid = commandUIGuid;
		_ToolFactoryGuid = toolFactoryGuid;
		_OwningItemId = owningItemId;
		_OwningHierarchy = owningHierarchy;
		_ServiceProvider = owningHierarchy.ServiceProvider;
	}

	protected AbstractDataToolsDocument(IVsDataObject obj, Guid editorType, Guid commandUIGuid, Guid toolFactoryGuid,
		int owningItemId, IVsDataViewHierarchy owningHierarchy)
		: this(string.Format(null, SqlMonikerHelper.PrefixFormat, owningHierarchy.ExplorerConnection.DisplayName,
			obj.Type.Name, obj.Identifier.ToString()), editorType, commandUIGuid, toolFactoryGuid, owningItemId, owningHierarchy)
	{
		// Diag.Trace();
		Caption = string.Format(null, Properties.Resources.ToolsDocument_Caption, BuildConnectionName(owningHierarchy.ExplorerConnection.Connection), GetObjectTypeDisplayName(obj, owningItemId, owningHierarchy), obj.Identifier.ToString(DataObjectIdentifierFormat.ForDisplay));
	}

	public static bool ActivateIfOpen(Hostess host, IVsDataExplorerNode node)
	{
		return host.ActivateDocumentIfOpen(string.Format(null, SqlMonikerHelper.PrefixFormat, node.ExplorerConnection.DisplayName,
			node.Object.Type.Name, node.Object.Identifier.ToString()));
	}

	public virtual void Register(int documentCookie)
	{

		if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
		{
			NativeMethods.WrapComCall(vsPersistDocData.OnRegisterDocData((uint)documentCookie,
				_OwningHierarchy.ExplorerConnection as IVsHierarchy, (uint)_OwningItemId));
		}

		_DocumentCookie = documentCookie;
	}

	public virtual void InitializeNew(string untitledMoniker)
	{
		if (UnderlyingDocument.DocData is not IVsPersistDocData vsPersistDocData)
		{
			NotImplementedException ex = new();
			Diag.Dug(ex);
			throw ex;
		}

		NativeMethods.WrapComCall(vsPersistDocData.SetUntitledDocPath(untitledMoniker));
	}

	public virtual void Load(string moniker)
	{
		if (UnderlyingDocument.DocData is not IVsPersistDocData vsPersistDocData)
		{
			NotImplementedException ex = new();
			Diag.Dug(ex);
			throw ex;
		}

		NativeMethods.WrapComCall(vsPersistDocData.LoadDocData(moniker));
	}

	public virtual void Show()
	{
		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			Diag.Trace("Calling window frame show");
			NativeMethods.WrapComCall(WindowFrame.Show());
		}
	}

	public virtual void Reload(int flags)
	{
		if (UnderlyingDocument.DocData is not IVsPersistDocData vsPersistDocData)
		{
			NotImplementedException ex = new();
			Diag.Dug(ex);
			throw ex;
		}

		NativeMethods.WrapComCall(vsPersistDocData.ReloadDocData((uint)flags));
	}

	public virtual void Rename(string newMoniker, int attributes)
	{
		if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
		{
			NativeMethods.WrapComCall(vsPersistDocData.RenameDocData((uint)attributes, _OwningHierarchy.ExplorerConnection as IVsHierarchy, (uint)_OwningItemId, newMoniker));
		}

		_Moniker = newMoniker;
	}

	public virtual bool Save(VSSAVEFLAGS saveFlags)
	{
		Diag.Trace("Saving");
		if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
		{
			NativeMethods.WrapComCall(vsPersistDocData.SaveDocData(saveFlags, out string pbstrMkDocumentNew, out int pfSaveCanceled));
			if (pbstrMkDocumentNew != null)
			{
				_Moniker = pbstrMkDocumentNew;
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
				UnderlyingDocument.Save(DSRef, (string)null, flag);
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
		(_UnderlyingDocument.DocData as IVsPersistDocData)?.Close();
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
		pfReadOnly = IsReadOnly ? 1 : 0;
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
			return result;
		}
		catch (Exception e)
		{
			return Marshal.GetHRForException(e);
		}
	}

	int IVsPersistDocData.IsDocDataDirty(out int pfDirty)
	{
		int result = 0;
		pfDirty = 0;
		try
		{
			pfDirty = IsDirty ? 1 : 0;
			return result;
		}
		catch (Exception e)
		{
			return Marshal.GetHRForException(e);
		}
	}

	int IVsPersistDocData.SetUntitledDocPath(string pszDocDataPath)
	{
		int result = 0;
		try
		{
			InitializeNew(pszDocDataPath);
			return result;
		}
		catch (Exception e)
		{
			return Marshal.GetHRForException(e);
		}
	}

	int IVsPersistDocData.LoadDocData(string pszMkDocument)
	{
		int result = 0;
		try
		{
			Load(pszMkDocument);
			return result;
		}
		catch (Exception e)
		{
			return Marshal.GetHRForException(e);
		}
	}

	int IVsPersistDocData.SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
	{
		int result = 0;
		pbstrMkDocumentNew = null;
		pfSaveCanceled = 0;
		try
		{
			string moniker = _Moniker;
			pfSaveCanceled = !Save(dwSave) ? 1 : 0;
			if (pfSaveCanceled == 0)
			{
				if (!string.Equals(moniker, _Moniker, StringComparison.Ordinal))
				{
					pbstrMkDocumentNew = _Moniker;
					return result;
				}

				return result;
			}

			return result;
		}
		catch (Exception e)
		{
			return Marshal.GetHRForException(e);
		}
	}

	int IVsPersistDocData.Close()
	{
		int result = 0;
		try
		{
			Close();
			return result;
		}
		catch (Exception e)
		{
			return Marshal.GetHRForException(e);
		}
	}

	int IVsPersistDocData.OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
	{
		int result = 0;
		try
		{
			Register((int)docCookie);
			return result;
		}
		catch (Exception e)
		{
			return Marshal.GetHRForException(e);
		}
	}

	int IVsPersistDocData.RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
	{
		int result = 0;
		try
		{
			Rename(pszMkDocumentNew, (int)grfAttribs);
			_OwningItemId = (int)itemidNew;
			return result;
		}
		catch (Exception e)
		{
			return Marshal.GetHRForException(e);
		}
	}

	int IVsPersistDocData.IsDocDataReloadable(out int pfReloadable)
	{
		int result = 0;
		pfReloadable = 0;
		try
		{
			pfReloadable = IsReloadable ? 1 : 0;
			return result;
		}
		catch (Exception e)
		{
			return Marshal.GetHRForException(e);
		}
	}

	int IVsPersistDocData.ReloadDocData(uint grfFlags)
	{
		int result = 0;
		try
		{
			Reload((int)grfFlags);
			return result;
		}
		catch (Exception e)
		{
			return Marshal.GetHRForException(e);
		}
	}

	protected static string BuildConnectionName(IVsDataConnection connection)
	{
		string result = null;
		string text = null;
		string text2 = null;
		if (connection.GetService(typeof(IVsDataSourceInformation)) is IVsDataSourceInformation vsDataSourceInformation)
		{
			if (vsDataSourceInformation.Contains("DataSource"))
			{
				text = vsDataSourceInformation["DataSource"] as string;
			}

			if (vsDataSourceInformation.Contains("Database"))
			{
				text2 = vsDataSourceInformation["Database"] as string;
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


	public static string BuildObjectMoniker(string objectType, object[] identifier, IVsDataViewHierarchy owningHierarchy)
	{
		SqlMonikerHelper sqlMoniker = new SqlMonikerHelper();

		IVsDataObject @rootObj = owningHierarchy.ExplorerConnection.ConnectionNode.Object;

		if (@rootObj != null)
		{
			sqlMoniker.Server = (string)@rootObj.Properties["Server"];
			// This could be ambiguous
			sqlMoniker.Database = (string)@rootObj.Properties["Database"];
			sqlMoniker.User = (string)@rootObj.Properties["UserId"]; ;
		}

		// IVsDataSourceInformation vsDataSourceInformation = owningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataSourceInformation)) as IVsDataSourceInformation;

		sqlMoniker.ObjectType = objectType;
		sqlMoniker.ObjectName = SqlMonikerHelper.ToString(identifier);

		return sqlMoniker.Moniker;
	}



	protected static string GetObjectTypeDisplayName(IVsDataObject obj, int owningItemId, IVsDataViewHierarchy owningHierarchy)
	{
		string text = null;
		IVsDataViewCommonNodeInfo viewCommonNodeInfo = owningHierarchy.GetViewCommonNodeInfo(owningItemId);
		if (viewCommonNodeInfo != null)
		{
			text = viewCommonNodeInfo.TypeDisplayName;
		}

		text ??= obj.Type.Name;

		return text;
	}

	protected virtual IVsWindowFrame CreateWindowFrame()
	{
		IVsWindowFrame vsWindowFrame;

		vsWindowFrame = Host.CreateDocumentWindow(WindowFrameAttributes, _Moniker, _Caption, null, EditorType, null, CommandUIGuid, UnderlyingDocument.DocView, this, _OwningItemId, _OwningHierarchy.ExplorerConnection as IVsUIHierarchy, _ServiceProvider as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);

		IVsTrackSelectionEx vsTrackSelectionEx = null;
		IServiceProvider serviceProvider = null;

		NativeMethods.WrapComCall(vsWindowFrame.GetProperty(-3002, out object pvar));

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

				int currentSelection = vsTrackSelectionEx.GetCurrentSelection(out IntPtr ppHier, out uint pitemid, out IVsMultiItemSelect ppMIS, out IntPtr ppSC);

				intPtr = Marshal.GetComInterfaceForObject(_OwningHierarchy.ExplorerConnection, typeof(IVsHierarchy));

				NativeMethods.WrapComCall(vsTrackSelectionEx.OnSelectChangeEx(intPtr, (uint)_OwningItemId, null, ppSC));
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.Release(intPtr);
				}
			}
		}

		UnderlyingDocument.OnPostCreateFrame((object)vsWindowFrame);

		return vsWindowFrame;
	}

	protected IDTDocTool CreateUnderlyingDocument()
	{
		IDTDocTool document;
		IVsDataConnection connection;

		_OwningHierarchy.ExplorerConnection.Connection.EnsureConnected();

		// IDTDocToolFactoryProvider toolFactoryProvider = Host.GetService<IDTDocToolFactoryProvider>();
		IVsDataConnectionManager connectionManager = Host.GetService<IVsDataConnectionManager>();



		Guid explorerGuid = _OwningHierarchy.ExplorerConnection.Provider;

		connection = connectionManager.GetConnection(explorerGuid, _OwningHierarchy.ExplorerConnection.EncryptedConnectionString, true);


		// Type queryDesigner = typeof(IDTQueryDesignerFactory);


		object factory;
		try
		{
			// ************** This is where it all falls flat.
			// ************** The COM object simply will not instantiate

			// factory = toolFactoryProvider.CreateToolFactory((object)connection, ref _ToolFactoryGuid);
			factory = Activator.CreateInstance(typeof(IDTQueryDesignerFactory), new object[] { connection });
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}


		document = CreateUnderlyingDocument(factory);

		document.Initialize();

		return document;
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

		IDSRefBuilder iDSRefBuilder = null;
		if (type2 != null)
		{
			iDSRefBuilder = Activator.CreateInstance(type2) as IDSRefBuilder;
			if (iDSRefBuilder is IVsDataSiteableObject<IVsDataConnection> vsDataSiteableObject)
			{
				vsDataSiteableObject.Site = OwningHierarchy.ExplorerConnection.Connection;
			}
		}

		iDSRefBuilder ??= OwningHierarchy.ExplorerConnection.Connection.GetService(typeof(IDSRefBuilder)) as IDSRefBuilder;
		iDSRefBuilder ??= new DSRefBuilder(OwningHierarchy.ExplorerConnection.Connection);

		try { Assumes.Present(iDSRefBuilder); }
		catch (Exception ex) { Diag.Dug(ex); throw; }

		object obj = Host.CreateLocalInstance(NativeMethods.CLSID_DSRef);
		if (identifier != null)
		{
			if (iDSRefBuilder is IVsDataSupportObject<IDSRefBuilder> vsDataSupportObject && value != null)
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


	public static object[] BuildNewObjectIdentifier(string newObjectSchema, string objectType, string objectPrefix, ref int newObjectCount, IVsDataViewHierarchy owningHierarchy)
	{
		object[] array = new object[3];
		IVsDataSourceInformation vsDataSourceInformation = owningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataSourceInformation)) as IVsDataSourceInformation;
		array[0] = vsDataSourceInformation["Database"] as string;
		array[1] = newObjectSchema;
		newObjectCount = UpdateNewObjectCount(newObjectSchema, objectType, objectPrefix, newObjectCount, owningHierarchy);
		array[2] = objectPrefix + newObjectCount;
		return array;
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
			NativeMethods.WrapComCall(vsPersistDocData.IsDocDataDirty(out int pfDirty));
			_IsDirty = pfDirty != 0;
		}
		else
		{
			_IsDirty = UnderlyingDocument.IsDirty();
		}
	}

	private static int UpdateNewObjectCount(string newObjectSchema, string objectType, string objectPrefix, int currentNewObjectCount, IVsDataViewHierarchy owningHierarchy)
	{
		currentNewObjectCount++;
		IList<string> list = new List<string>();
		IVsDataCommand vsDataCommand = owningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataCommand)) as IVsDataCommand;

		try { Assumes.Present(vsDataCommand); }
		catch (Exception ex) { Diag.Dug(ex); throw; }

		IVsDataSourceVersionComparer vsDataSourceVersionComparer = owningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataSourceVersionComparer)) as IVsDataSourceVersionComparer;

		try { Assumes.Present(vsDataSourceVersionComparer); }
		catch (Exception ex) { Diag.Dug(ex); throw; }

		IVsDataReader vsDataReader = vsDataCommand.Execute(string.Format(format: (vsDataSourceVersionComparer.CompareTo("9") >= 0) ? "SELECT name FROM sys.objects WHERE SCHEMA_NAME(schema_id) = '{0}' AND \tname LIKE '{1}%'" : "SELECT name FROM sysobjects WHERE USER_NAME(uid) = '{0}' AND \tname LIKE '{1}%'", provider: CultureInfo.InvariantCulture, arg0: newObjectSchema.Replace("'", "''"), arg1: objectPrefix));
		using (vsDataReader)
		{
			while (vsDataReader.Read())
			{
				list.Add(vsDataReader.GetItem(0) as string);
			}
		}

		IVsDataObjectMemberComparer vsDataObjectMemberComparer = owningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataObjectMemberComparer)) as IVsDataObjectMemberComparer;

		try { Assumes.Present(vsDataObjectMemberComparer); }
		catch (Exception ex) { Diag.Dug(ex); throw; }

		IVsDataSourceInformation vsDataSourceInformation = owningHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataSourceInformation)) as IVsDataSourceInformation;
		object[] array = new object[3]
		{
			vsDataSourceInformation["Database"],
			newObjectSchema,
			null
		};

		bool flag;
		do
		{
			flag = false;
			array[2] = objectPrefix + currentNewObjectCount;
			foreach (string item in list)
			{
				if (vsDataObjectMemberComparer.Compare(objectType, array, 2, item) == 0)
				{
					flag = true;
					break;
				}
			}

			if (flag)
			{
				currentNewObjectCount++;
			}
		}
		while (flag);
		return currentNewObjectCount;
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
