using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Common;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Data.Services.SupportEntities.Interop;
using Microsoft.VisualStudio.DataTools.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace BlackbirdSql.VisualStudio.Ddex.Extensions
{
	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]
	internal abstract class ToolsDocument : IVsPersistDocData2, IVsPersistDocData, IVsFileBackup
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
				Diag.Dug();
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
				Diag.Dug();
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
				Diag.Dug();
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
				Diag.Dug();
				return _isReadOnly;
			}
			set
			{
				Diag.Dug();
				_isReadOnly = value;
			}
		}

		public bool IsDirty
		{
			get
			{
				Diag.Dug();
				UpdateIsDirty();
				return _isDirty;
			}
			set
			{
				Diag.Dug();
				_isDirty = value;
			}
		}

		public bool IsReloadable
		{
			get
			{
				Diag.Dug();
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
				Diag.Dug();
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
				Diag.Dug();
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
				Diag.Dug();
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
				Diag.Dug();
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
				Diag.Dug();
				if (_host == null)
				{
					_host = new Host(_serviceProvider);
				}

				return _host;
			}
		}

		protected ToolsDocument(Guid editorType, Guid commandUIGuid, Guid toolFactoryGuid, int owningItemId, IVsDataViewHierarchy owningHierarchy)
			: this((string)null, editorType, commandUIGuid, toolFactoryGuid, owningItemId, owningHierarchy)
		{
			Diag.Dug();
		}

		protected ToolsDocument(string moniker, Guid editorType, Guid commandUIGuid, Guid toolFactoryGuid, int owningItemId, IVsDataViewHierarchy owningHierarchy)
		{
			Diag.Dug();
			_moniker = moniker;
			_editorType = editorType;
			_commandUIGuid = commandUIGuid;
			_toolFactoryGuid = toolFactoryGuid;
			_owningItemId = owningItemId;
			_owningHierarchy = owningHierarchy;
			_serviceProvider = owningHierarchy.ServiceProvider;
		}

		protected ToolsDocument(IVsDataObject obj, Guid editorType, Guid commandUIGuid, Guid toolFactoryGuid, int owningItemId, IVsDataViewHierarchy owningHierarchy)
			: this(string.Format(null, "DataExplorer://{0}/{1}/{2}", owningHierarchy.ExplorerConnection.DisplayName, obj.Type.Name, obj.Identifier.ToString()), editorType, commandUIGuid, toolFactoryGuid, owningItemId, owningHierarchy)
		{
			Diag.Dug();
			Caption = string.Format(null, Properties.Resources.ToolsDocument_Caption, BuildConnectionName(owningHierarchy.ExplorerConnection.Connection), GetObjectTypeDisplayName(obj, owningItemId, owningHierarchy), obj.Identifier.ToString(DataObjectIdentifierFormat.ForDisplay));
		}

		public static bool ActivateIfOpen(Host host, IVsDataExplorerNode node)
		{
			return host.ActivateDocumentIfOpen(string.Format(null, "DataExplorer://{0}/{1}/{2}", node.ExplorerConnection.DisplayName, node.Object.Type.Name, node.Object.Identifier.ToString()));
		}

		public virtual void Register(int documentCookie)
		{
			Diag.Dug();
			if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
			{
				NativeMethods.WrapComCall(vsPersistDocData.OnRegisterDocData((uint)documentCookie, _owningHierarchy.ExplorerConnection as IVsHierarchy, (uint)_owningItemId));
			}

			_documentCookie = documentCookie;
		}

		public virtual void InitializeNew(string untitledMoniker)
		{
			Diag.Dug();
			if (UnderlyingDocument.DocData is not IVsPersistDocData vsPersistDocData)
			{
				throw new NotImplementedException();
			}

			NativeMethods.WrapComCall(vsPersistDocData.SetUntitledDocPath(untitledMoniker));
		}

		public virtual void Load(string moniker)
		{
			Diag.Dug();
			if (UnderlyingDocument.DocData is not IVsPersistDocData vsPersistDocData)
			{
				throw new NotImplementedException();
			}

			NativeMethods.WrapComCall(vsPersistDocData.LoadDocData(moniker));
		}

		public virtual void Show()
		{
			Diag.Dug();
			using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
			{
				Diag.Dug();
				NativeMethods.WrapComCall(WindowFrame.Show());
			}
			Diag.Dug();
		}

		public virtual void Reload(int flags)
		{
			Diag.Dug();
			if (UnderlyingDocument.DocData is not IVsPersistDocData vsPersistDocData)
			{
				throw new NotImplementedException();
			}

			NativeMethods.WrapComCall(vsPersistDocData.ReloadDocData((uint)flags));
		}

		public virtual void Rename(string newMoniker, int attributes)
		{
			Diag.Dug();
			if (UnderlyingDocument.DocData is IVsPersistDocData vsPersistDocData)
			{
				NativeMethods.WrapComCall(vsPersistDocData.RenameDocData((uint)attributes, _owningHierarchy.ExplorerConnection as IVsHierarchy, (uint)_owningItemId, newMoniker));
			}

			_moniker = newMoniker;
		}

		public virtual bool Save(VSSAVEFLAGS saveFlags)
		{
			Diag.Dug();
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
			Diag.Dug();
			(_underlyingDocument.DocData as IVsPersistDocData)?.Close();
			_windowFrame = null;
			_underlyingDocument = null;
			_documentCookie = 0;
			_dsRef = null;
			_owningItemId = -1;
			_owningHierarchy = null;
		}

		public int BackupFile(string backupFileName)
		{
			Diag.Dug();
			return 0;
		}

		public int IsBackupFileObsolete(out int isObsolete)
		{
			Diag.Dug();
			isObsolete = 0;
			return 0;
		}

		int IVsPersistDocData2.GetGuidEditorType(out Guid pClassID)
		{
			Diag.Dug();
			return ((IVsPersistDocData)this).GetGuidEditorType(out pClassID);
		}

		int IVsPersistDocData2.IsDocDataDirty(out int pfDirty)
		{
			Diag.Dug();
			return ((IVsPersistDocData)this).IsDocDataDirty(out pfDirty);
		}

		int IVsPersistDocData2.SetUntitledDocPath(string pszDocDataPath)
		{
			Diag.Dug();
			return ((IVsPersistDocData)this).SetUntitledDocPath(pszDocDataPath);
		}

		int IVsPersistDocData2.LoadDocData(string pszMkDocument)
		{
			Diag.Dug();
			return ((IVsPersistDocData)this).LoadDocData(pszMkDocument);
		}

		int IVsPersistDocData2.SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
		{
			Diag.Dug();
			return ((IVsPersistDocData)this).SaveDocData(dwSave, out pbstrMkDocumentNew, out pfSaveCanceled);
		}

		int IVsPersistDocData2.Close()
		{
			Diag.Dug();
			return ((IVsPersistDocData)this).Close();
		}

		int IVsPersistDocData2.OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
		{
			Diag.Dug();
			return ((IVsPersistDocData)this).OnRegisterDocData(docCookie, pHierNew, itemidNew);
		}

		int IVsPersistDocData2.RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
		{
			Diag.Dug();
			return ((IVsPersistDocData)this).RenameDocData(grfAttribs, pHierNew, itemidNew, pszMkDocumentNew);
		}

		int IVsPersistDocData2.IsDocDataReloadable(out int pfReloadable)
		{
			Diag.Dug();
			return ((IVsPersistDocData)this).IsDocDataReloadable(out pfReloadable);
		}

		int IVsPersistDocData2.ReloadDocData(uint grfFlags)
		{
			Diag.Dug();
			return ((IVsPersistDocData)this).ReloadDocData(grfFlags);
		}

		int IVsPersistDocData2.SetDocDataDirty(int fDirty)
		{
			Diag.Dug();
			IsDirty = fDirty != 0;
			return 0;
		}

		int IVsPersistDocData2.IsDocDataReadOnly(out int pfReadOnly)
		{
			Diag.Dug();
			pfReadOnly = (IsReadOnly ? 1 : 0);
			return 0;
		}

		int IVsPersistDocData2.SetDocDataReadOnly(int fReadOnly)
		{
			Diag.Dug();
			IsReadOnly = fReadOnly != 0;
			return 0;
		}

		int IVsPersistDocData.GetGuidEditorType(out Guid pClassID)
		{
			Diag.Dug();
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
			Diag.Dug();
			int result = 0;
			pfDirty = 0;
			try
			{
				pfDirty = (IsDirty ? 1 : 0);
				return result;
			}
			catch (Exception e)
			{
				return Marshal.GetHRForException(e);
			}
		}

		int IVsPersistDocData.SetUntitledDocPath(string pszDocDataPath)
		{
			Diag.Dug();
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
			Diag.Dug();
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
			Diag.Dug();
			int result = 0;
			pbstrMkDocumentNew = null;
			pfSaveCanceled = 0;
			try
			{
				string moniker = _moniker;
				pfSaveCanceled = ((!Save(dwSave)) ? 1 : 0);
				if (pfSaveCanceled == 0)
				{
					if (!string.Equals(moniker, _moniker, StringComparison.Ordinal))
					{
						pbstrMkDocumentNew = _moniker;
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
			Diag.Dug();
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
			Diag.Dug();
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
			Diag.Dug();
			int result = 0;
			try
			{
				Rename(pszMkDocumentNew, (int)grfAttribs);
				_owningItemId = (int)itemidNew;
				return result;
			}
			catch (Exception e)
			{
				return Marshal.GetHRForException(e);
			}
		}

		int IVsPersistDocData.IsDocDataReloadable(out int pfReloadable)
		{
			Diag.Dug();
			int result = 0;
			pfReloadable = 0;
			try
			{
				pfReloadable = (IsReloadable ? 1 : 0);
				return result;
			}
			catch (Exception e)
			{
				return Marshal.GetHRForException(e);
			}
		}

		int IVsPersistDocData.ReloadDocData(uint grfFlags)
		{
			Diag.Dug();
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
			Diag.Dug();
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

			Diag.Dug();

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
			Diag.Dug();

			return result;
		}

		protected static string GetObjectTypeDisplayName(IVsDataObject obj, int owningItemId, IVsDataViewHierarchy owningHierarchy)
		{
			Diag.Dug();
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
			Diag.Dug();
			IVsWindowFrame vsWindowFrame = null;

			vsWindowFrame = Host.CreateDocumentWindow(WindowFrameAttributes, _moniker, _caption, null, EditorType, null, CommandUIGuid, UnderlyingDocument.DocView, this, _owningItemId, _owningHierarchy.ExplorerConnection as IVsUIHierarchy, _serviceProvider as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
			Diag.Dug();

			IVsTrackSelectionEx vsTrackSelectionEx = null;
			System.IServiceProvider serviceProvider = null;
			object pvar = null;

			NativeMethods.WrapComCall(vsWindowFrame.GetProperty(-3002, out pvar));
			Diag.Dug();

			if (pvar is Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider2)
			{
				serviceProvider = new ServiceProvider(serviceProvider2);
			}
			Diag.Dug();

			if (serviceProvider != null)
			{
				vsTrackSelectionEx = serviceProvider.GetService(typeof(SVsTrackSelectionEx)) as IVsTrackSelectionEx;
			}
			Diag.Dug();

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
					Diag.Dug();

					intPtr = Marshal.GetComInterfaceForObject(_owningHierarchy.ExplorerConnection, typeof(IVsHierarchy));
					Diag.Dug();

					NativeMethods.WrapComCall(vsTrackSelectionEx.OnSelectChangeEx(intPtr, (uint)_owningItemId, null, ppSC));
					Diag.Dug();
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
			Diag.Dug();

			return vsWindowFrame;
		}

		protected IDTDocTool CreateUnderlyingDocument()
		{
			Diag.Dug();
			IDTDocTool document;
			IVsDataConnection connection;

			_owningHierarchy.ExplorerConnection.Connection.EnsureConnected();

			IDTDocToolFactoryProvider toolFactoryProvider = Host.GetService<IDTDocToolFactoryProvider>();
			IVsDataConnectionManager connectionManager = Host.GetService<IVsDataConnectionManager>();
			Diag.Dug();

			

			Guid explorerGuid = _owningHierarchy.ExplorerConnection.Provider;
			Diag.Dug("Explorer Guid: " + explorerGuid.ToString() + " Encrypted connection string: " + _owningHierarchy.ExplorerConnection.EncryptedConnectionString.ToString() + " ToolFactoryGUID: " + _toolFactoryGuid.ToString());

			connection = connectionManager.GetConnection(explorerGuid, _owningHierarchy.ExplorerConnection.EncryptedConnectionString, true);

			Diag.Dug("toolFactory class: " + toolFactoryProvider.GetType().FullName + " dll: " + toolFactoryProvider.GetType().AssemblyQualifiedName);

			Type queryDesigner = typeof(IDTQueryDesignerFactory);

			Diag.Dug("IDTQueryDesignerFactory: " + queryDesigner.FullName + " dll: " + queryDesigner.AssemblyQualifiedName);

			object factory;
			try
			{
				// ************** This is where it all falls flat.
				// ************** The COM object simply will not instantiate

				// factory = toolFactoryProvider.CreateToolFactory((object)connection, ref _toolFactoryGuid);
				factory = Activator.CreateInstance(typeof(IDTQueryDesignerFactory), new object[] {connection});
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}


			document = CreateUnderlyingDocument(factory);

			Diag.Dug();
			document.Initialize();
			Diag.Dug();
			return document;
		}

		protected abstract IDTDocTool CreateUnderlyingDocument(object factory);

		protected virtual object BuildDSRef()
		{
			Diag.Dug();
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
			Diag.Dug();
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

			if (iDSRefBuilder == null)
			{
				iDSRefBuilder = OwningHierarchy.ExplorerConnection.Connection.GetService(typeof(IDSRefBuilder)) as IDSRefBuilder;
			}

			if (iDSRefBuilder == null)
			{
				iDSRefBuilder = new DSRefBuilder(OwningHierarchy.ExplorerConnection.Connection);
			}

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

		protected string GetObjectSchemaFromDSRef()
		{
			Diag.Dug();
			IntPtr zero = IntPtr.Zero;
			Microsoft.VisualStudio.Data.Services.SupportEntities.Interop.IDSRefConsumer iDSRefConsumer = DSRef as Microsoft.VisualStudio.Data.Services.SupportEntities.Interop.IDSRefConsumer;
			IntPtr firstChildNode = iDSRefConsumer.GetFirstChildNode(DSRefConstants.DSREFNODEID_ROOT);
			zero = iDSRefConsumer.GetFirstChildNode(firstChildNode);
			return iDSRefConsumer.GetOwner(zero);
		}

		protected string GetObjectNameFromDSRef()
		{
			Diag.Dug();
			IntPtr zero = IntPtr.Zero;
			Microsoft.VisualStudio.Data.Services.SupportEntities.Interop.IDSRefConsumer iDSRefConsumer = DSRef as Microsoft.VisualStudio.Data.Services.SupportEntities.Interop.IDSRefConsumer;
			IntPtr firstChildNode = iDSRefConsumer.GetFirstChildNode(DSRefConstants.DSREFNODEID_ROOT);
			zero = iDSRefConsumer.GetFirstChildNode(firstChildNode);
			return iDSRefConsumer.GetName(zero);
		}

		protected void SetObjectNameInDSRef(string name)
		{
			Diag.Dug();
			IntPtr zero = IntPtr.Zero;
			Microsoft.VisualStudio.Data.Services.SupportEntities.Interop.IDSRefConsumer iDSRefConsumer = DSRef as Microsoft.VisualStudio.Data.Services.SupportEntities.Interop.IDSRefConsumer;
			IntPtr firstChildNode = iDSRefConsumer.GetFirstChildNode(DSRefConstants.DSREFNODEID_ROOT);
			zero = iDSRefConsumer.GetFirstChildNode(firstChildNode);
			Microsoft.VisualStudio.Data.Services.SupportEntities.Interop.IDSRefProvider iDSRefProvider = DSRef as Microsoft.VisualStudio.Data.Services.SupportEntities.Interop.IDSRefProvider;
			iDSRefProvider.SetName(zero, name);
		}

		protected virtual void UpdateIsDirty()
		{
			Diag.Dug();
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
			Diag.Dug();
			return DialogResult.OK;
		}

		protected virtual DialogResult PromptToOverwrite(int reason)
		{
			Diag.Dug();
			return DialogResult.OK;
		}

		protected virtual void OnDocumentSaved(bool overwriteExisting)
		{
			Diag.Dug();
		}
	}
}
