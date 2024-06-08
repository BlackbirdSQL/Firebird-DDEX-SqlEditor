// System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// System.Diagnostics.Design.WorkingDirectoryEditor
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys;

namespace BlackbirdSql.Core.Controls.ComponentModel;

/// <summary>
/// Folder name dialog editor using ParametersAttribute and the local
/// Properties.AttributeResources.resx for parameterizing the dialog. 
/// Where arg[0]: Title resource. 
/// </summary>
[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
public abstract class AbstractFolderNameEditor : FolderNameEditor
{
	//
	// Summary:
	//     Represents a dialog box that allows the user to choose a folder. This class cannot
	//     be inherited.
	protected sealed class FolderBrowserDialog : Component
	{
		private const int MAX_PATH = 260;

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		public delegate int CallbackDelegate(IntPtr hwnd, int umsg, IntPtr wparam, IntPtr lpdata);


		private FolderBrowserFolder _StartLocation;
		private FolderBrowserStyles _PublicOptions = FolderBrowserStyles.RestrictToFilesystem;
		private readonly UnsafeNative.EnBrowseInfos _PrivateOptions = UnsafeNative.EnBrowseInfos.NewDialogStyle;
		private string _Title = string.Empty;
		private string _DirectoryPath = string.Empty;
		private string _InitialDirectory;

		private CallbackDelegate _BrowserCallback;


		public string InitialDirectory
		{
			set { _InitialDirectory = value; }
		}

		//
		// Summary:
		//     The styles the folder browser will use when browsing folders. This should be
		//     a combination of flags from the System.Windows.Forms.Design.FolderNameEditor.FolderBrowserStyles
		//     enumeration.
		//
		// Returns:
		//     A System.Windows.Forms.Design.FolderNameEditor.FolderBrowserStyles enumeration
		//     member that indicates behavior for the System.Windows.Forms.Design.FolderNameEditor.FolderBrowser
		//     to use.
		public FolderBrowserStyles Style
		{
			get
			{
				return _PublicOptions;
			}
			set
			{
				_PublicOptions = value;
			}
		}

		//
		// Summary:
		//     Gets the directory path to the object the user picked.
		//
		// Returns:
		//     The directory path to the object the user picked.
		public string DirectoryPath => _DirectoryPath;

		//
		// Summary:
		//     Gets or sets the start location of the root node.
		//
		// Returns:
		//     A System.Windows.Forms.Design.FolderNameEditor.FolderBrowserFolder that indicates
		//     the location for the folder browser to initially browse to.
		public FolderBrowserFolder StartLocation
		{
			get
			{
				return _StartLocation;
			}
			set
			{
				_StartLocation = value;
			}
		}

		//
		// Summary:
		//     Gets or sets a description to show above the folders.
		//
		// Returns:
		//     The description to show above the folders.
		public string Title
		{
			set
			{
				_Title = (value ?? string.Empty);
			}
		}

		public string DisplayName { get; private set; }

		//
		// Summary:
		//     Shows the folder browser dialog.
		//
		// Returns:
		//     The System.Windows.Forms.DialogResult from the form.
		public DialogResult ShowDialog()
		{
			return ShowDialog(null);
		}

		public DialogResult ShowDialog(IWin32Window owner)
		{
			IntPtr ppIdl = IntPtr.Zero;
			IntPtr idListPtr = IntPtr.Zero;
			IntPtr pszPath = IntPtr.Zero;
			IntPtr lpBrowserCallback = IntPtr.Zero;

			IntPtr handle = owner?.Handle ?? UnsafeNative.GetActiveWindow();

			int flags = (int)_PublicOptions | (int)_PrivateOptions;
			if ((flags & (int)UnsafeNative.EnBrowseInfos.NewDialogStyle) != 0)
			{
				Application.OleRequired();
			}


			if (!string.IsNullOrWhiteSpace(_InitialDirectory))
			{
				_BrowserCallback = new CallbackDelegate(BrowserCallback);
				lpBrowserCallback = Marshal.GetFunctionPointerForDelegate(_BrowserCallback);
			}
			else
			{
				_BrowserCallback = null;
				UnsafeNative.Shell32.SHGetSpecialFolderLocation(handle, (int)_StartLocation, ref ppIdl);

				if (ppIdl == IntPtr.Zero)
					return DialogResult.Cancel;
			}

			UnsafeNative.BROWSEINFO browseInfo = null;

			// Tracer.Trace("Title: " + (string.IsNullOrWhiteSpace(_Title) ? "Select folder" : _Title));

			try
			{
				browseInfo = new()
				{
					hwndOwner = handle,
					lpfn = lpBrowserCallback,
					pidlRoot = ppIdl,
					lParam = IntPtr.Zero,
					lpszTitle = string.IsNullOrWhiteSpace(_Title) ? AttributeResources.EditorFolderName_SelectFolder : _Title,
					ulFlags = flags,
					iImage = 0,
					pszDisplayName = Marshal.AllocHGlobal(256)
				};


				// Call dialog SHBrowseForFolder
				idListPtr = UnsafeNative.Shell32.SHBrowseForFolder(browseInfo);

				// check if the user cancelled out of the dialog or not.
				if (idListPtr == IntPtr.Zero)
					return DialogResult.Cancel;

				// allocate ncessary memory buffer to receive the folder path.
				pszPath = Marshal.AllocHGlobal(MAX_PATH);
				// call shgetpathfromidlist to get folder path.
				bool bret = UnsafeNative.Shell32.SHGetPathFromIDList(idListPtr, pszPath);
				// convert the returned native poiner to string.
				_DirectoryPath = Marshal.PtrToStringAuto(pszPath);
				DisplayName = Marshal.PtrToStringAuto(browseInfo.pszDisplayName);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				return DialogResult.Abort;
			}
			finally
			{
				// free the memory allocated by shell.
				if (ppIdl != IntPtr.Zero)
					Marshal.FreeHGlobal(ppIdl);
				if (idListPtr != IntPtr.Zero)
					Marshal.FreeHGlobal(idListPtr);
				if (pszPath != IntPtr.Zero)
					Marshal.FreeHGlobal(pszPath);
				if (browseInfo != null)
					Marshal.FreeHGlobal(browseInfo.pszDisplayName);
			}

			return DialogResult.OK;

		}




		private int BrowserCallback(IntPtr hwnd, int umsg, IntPtr wparam, IntPtr lpdata)
		{
			if (!string.IsNullOrWhiteSpace(_InitialDirectory) && umsg == (int)Native.EnBrowseForFolderMessages.Initialized)
			{
				HandleRef href = new HandleRef(null, hwnd);

				Native.SendMessage(href, (int)Native.EnBrowseForFolderMessages.SetSelectionW, 1, _InitialDirectory);
				Native.SendMessage(href, (int)Native.EnBrowseForFolderMessages.SetExpanded, 1, _InitialDirectory);

			}
			else if (umsg == (int)Native.EnBrowseForFolderMessages.SelChanged)
			{
				// We get in here whenever the selection in the dialog box changes. To cope with the bug in Win7 
				// (and above?) whereby the SHBrowseForFolder dialog won't always scroll the selection into view (see 
				// http://social.msdn.microsoft.com/Forums/en-US/vcgeneral/thread/a22b664e-cb30-44f4-bf77-b7a385de49f3/)
				// we follow the suggestion here: 
				// http://www.codeproject.com/Questions/179097/SHBrowseForFolder-and-SHGetPathFromIDList
				// to adjust the scroll position when the selection changes.
				IntPtr hbrowse = Native.FindWindowEx(hwnd, IntPtr.Zero, "SHBrowseForFolder ShellNameSpace Control", null);
				if (hbrowse == IntPtr.Zero)
				{
					COMException ex = new("Could not find window 'SHBrowseForFolder ShellNameSpace Control'.");
					Diag.Dug(ex);
					return 0;
				}
				IntPtr htree = Native.FindWindowEx(hbrowse, IntPtr.Zero, "SysTreeView32", null);
				if (htree == IntPtr.Zero)
				{
					COMException ex = new("Could not find window 'SysTreeView32'.");
					Diag.Dug(ex);
					return 0;
				}

				IntPtr htir = Native.SendMessage(new HandleRef(null, htree), Native.TVM_GETNEXTITEM, Native.TVGN_ROOT, IntPtr.Zero);
				if (htir == IntPtr.Zero)
				{
					COMException ex = new("Get next tree item from root returned null.");
					Diag.Dug(ex);
					return 0;
				}
				IntPtr htis = Native.SendMessage(new HandleRef(null, htree), Native.TVM_GETNEXTITEM, Native.TVGN_CARET, IntPtr.Zero);
				if (htis == IntPtr.Zero)
				{
					COMException ex = new("Get next tree item from caret returned null.");
					Diag.Dug(ex);
					return 0;
				}
				IntPtr htic = Native.SendMessage(new HandleRef(null, htree), Native.TVM_GETNEXTITEM, Native.TVGN_CHILD, htir);
				if (htic == IntPtr.Zero)
				{
					COMException ex = new("Get next tree child item returned null.");
					Diag.Dug(ex);
					return 0;
				}


				int count = 0;
				int pos = 0;
				for (; htic != IntPtr.Zero; htic = Native.SendMessage(new HandleRef(null, htree), Native.TVM_GETNEXTITEM, Native.TVGN_NEXTVISIBLE, htic), count++)
				{
					if (htis == htic)
						pos = count;
				}
				Native.SCROLLINFO si = new();
				si.cbSize = Marshal.SizeOf(si);
				si.fMask = Native.SIF_POS | Native.SIF_RANGE | Native.SIF_PAGE;
				Native.GetScrollInfo(htree, Native.SB_VERT, si);

				si.nPage /= 2;
				if ((pos > (int)(si.nMin + si.nPage)) && (pos <= (int)(si.nMax - si.nMin - si.nPage)))
				{
					si.nMax = si.nPos - si.nMin + (int)si.nPage;
					for (; pos < si.nMax; pos++) Native.PostMessage(htree, Native.WM_VSCROLL, Native.SB_LINEUP, 0);
					for (; pos > si.nMax; pos--) Native.PostMessage(htree, Native.WM_VSCROLL, Native.SB_LINEDOWN, 0);
				}
			}
			return 0;
		}



		//
		// Summary:
		//     Initializes a new instance of the System.Windows.Forms.Design.FolderNameEditor.FolderBrowser
		//     class.
		public FolderBrowserDialog()
		{
		}
	}



	// A private 'this' object lock
	// private readonly object _LockLocal = new();

	private ITypeDescriptorContext _Context = null;

	private FolderBrowserDialog _FolderBrowserDlg = null;



	protected FolderBrowserDialog FolderBrowserDlg => _FolderBrowserDlg;
	public System.Resources.ResourceManager LocalResMgr => AttributeResources.ResourceManager;
	public abstract System.Resources.ResourceManager ResMgr { get; }



	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (_FolderBrowserDlg == null)
		{
			_Context = context;
			InitializeDialog(new FolderBrowserDialog());
		}

		DirectoryInfo dirInfo;
		string initialDir = null;

		if (value is string directory && !string.IsNullOrWhiteSpace(directory))
		{
			dirInfo = new(directory);
			initialDir = dirInfo.FullName;

			_FolderBrowserDlg.InitialDirectory = initialDir;
		}

		if (_FolderBrowserDlg.ShowDialog() != DialogResult.OK)
		{
			return value;
		}

		dirInfo = new(_FolderBrowserDlg.DirectoryPath);
		string result = dirInfo.FullName;

		if (initialDir != null && result.Equals(initialDir, StringComparison.OrdinalIgnoreCase))
			result = initialDir;

		return result;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return base.GetEditStyle(context);
	}

	protected void InitializeDialog(FolderBrowserDialog folderBrowser)
	{
		bool localTitle = true;
		string title = "GlobalizedDialogGenericSelectFolder";

		if (_Context.PropertyDescriptor.Attributes[typeof(ParametersAttribute)] is ParametersAttribute paramsAttr)
		{
			// Tracer.Trace($"ParametersAttribute found for {context.PropertyDescriptor.Name}: {paramsAttr.Value1}.");
			if (paramsAttr.Length > 0)
			{
				localTitle = false;
				title = (string)paramsAttr[0];
			}
		}

		_FolderBrowserDlg = folderBrowser;

		folderBrowser.StartLocation = FolderBrowserFolder.MyDocuments;

		folderBrowser.Title = localTitle? LocalResMgr.GetString(title) : ResMgr.GetString(title);
	}



}
