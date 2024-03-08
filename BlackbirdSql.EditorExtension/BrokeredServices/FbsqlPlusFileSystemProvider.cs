// Microsoft.VisualStudio.LiveShare.VslsFileSystemProvider.VSCore, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.LiveShare.FileSystemProvider.VslsFileSystemProvider

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model;
using Microsoft;
using Microsoft.VisualStudio.RpcContracts;
using Microsoft.VisualStudio.RpcContracts.FileSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Telemetry;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;

using DirectoryInfo = Microsoft.VisualStudio.RpcContracts.FileSystem.DirectoryInfo;
using FileInfo = Microsoft.VisualStudio.RpcContracts.FileSystem.FileInfo;


namespace BlackbirdSql.BrokeredServices;

// =========================================================================================================
//								FbsqlPlusFileSystemProvider Class (Deprecated)
//
/// <summary>
/// BlackbirdSql File System Provider for the fbsql++ document moniker protocol.
/// DEPRECATED in favour of using the registered extension (.fbsql) and letting VS handle everything, which
/// it does pretty efficiently. There are simply too many roadblocks in opening explorer node sql/ddl
/// documents as virtual files. In any case we successfully track a document's associated node connection
/// as a pseudo moniker that links back to the original database.
/// From that point on the user can in any case change the connection, which is tightly coupled to the
/// DocData, so a future move to live data updating is on the cards. 
/// </summary>
// =========================================================================================================
#pragma warning disable CS8632 // Suppress '#nullable' annotations context warning.
public class FbsqlPlusFileSystemProvider : IFileSystemProvider, IRemoteFileSystemProvider,
	IUriDisplayInfoProvider, IDisposable
{


	// -----------------------------------------------------------------------------------------------------
	#region Constructors / Destructors - FbsqlPlusFileSystemProvider
	// -----------------------------------------------------------------------------------------------------


	public FbsqlPlusFileSystemProvider(Func<Task<IVsAsyncFileChangeEx>> fileChangeSvc,
		JoinableTaskContext joinableTaskContext, bool logTelemetryEvents)
	{
		// Tracer.Trace(typeof(FbsqlPlusFileSystemProvider), ".ctor");

		_JoinableTaskContext = Requires.NotNull(joinableTaskContext, "joinableTaskContext");
		_JoinableTaskCollection = joinableTaskContext.CreateCollection();
		_JoinableTaskFactory = new JoinableTaskFactory(_JoinableTaskCollection);

		_LogTelemetryEvents = logTelemetryEvents;

		TrySubscribeToFileSystemChangedEvent(fileChangeSvc);
	}



	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}


	protected virtual void Dispose(bool disposing)
	{
		if (_Disposed)
			return;

		if (disposing)
		{
			// _DisposalTokenSource?.Cancel();

			// Tracer.Trace(GetType(), "Dispose(bool)");

			if (_WatchedFileSystemEntries != null && _WatchedFileSystemEntries.Count > 0)
			{
				ThreadHelper.JoinableTaskFactory.Run(async delegate
				{
					foreach (KeyValuePair<WatchResult, FileWatcherSubscription> subsPair in _WatchedFileSystemEntries)
					{
						await UnwatchAsync(subsPair.Key, default);
					}
				});

				_WatchedFileSystemEntries = null;
			}

			try
			{
				_JoinableTaskContext.Factory.Run(() => _JoinableTaskCollection.JoinTillEmptyAsync());
			}
			catch (OperationCanceledException)
			{
			}
			catch (AggregateException ex)
			{
				ex.Handle((Exception inner) => inner is OperationCanceledException);
			}
		}

		_Disposed = true;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - FbsqlPlusFileSystemProvider
	// =========================================================================================================

	private const int C_DefaultCopyBufferSize = 81920;
	private const string C_FileSystemEntryChangedFaultName = "BlackbirdSql/FbsqlPlusFileSystemProvider/FileSystemEntryChangedFault";
	private const string C_TelemetryEventLogValue = "blackbirdsql/fbsqlplusfilesystemprovider/rootentrieschanged-hooked";


	#endregion Constants





	// =========================================================================================================
	#region Fields - FbsqlPlusFileSystemProvider
	// =========================================================================================================


	private readonly object _LockLocal = new object();
	private bool _Disposed = false;
	// private CancellationTokenSource _DisposalTokenSource = null;
	private readonly bool _LogTelemetryEvents;
	private bool _LoggedTelemetryEvents = false;

	private static readonly DateTime _S_MinDateUtc = new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc);

	private static readonly IReadOnlyList<Uri> _S_DefaultRoots = new List<Uri>
		{
			new Uri(SystemData.Scheme)
		};

	private static readonly JsonRpcEnumerableSettings _S_AsyncEnumerableRpcSettings = new JsonRpcEnumerableSettings
		{
			MaxReadAhead = 10,
			MinBatchSize = 10
		};


	private readonly JoinableTaskContext _JoinableTaskContext;
	private readonly JoinableTaskCollection _JoinableTaskCollection;
	private readonly JoinableTaskFactory _JoinableTaskFactory;

	private AsyncLazy<IVsAsyncFileChangeEx> _LazyFileSystemChangeService = null;
	private IDictionary<WatchResult, FileWatcherSubscription> _WatchedFileSystemEntries = null;

	private EventHandler _Connected;
	private EventHandler _Disconnected;
	private EventHandler<DirectoryEntryChangedEventArgs> _DirectoryEntryChanged;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - FbsqlPlusFileSystemProvider
	// =========================================================================================================


	/// <summary>
	/// <see cref="IRemoteFileSystemProvider.Connected"/> Implementation.
	/// </summary>
	public event EventHandler? Connected
	{
		add { _Connected += value; }
		remove { _Connected -= value; }
	}


	/// <summary>
	/// <see cref="IRemoteFileSystemProvider.Disconnected"/> implementation.
	/// </summary>
	public event EventHandler? Disconnected
	{
		add { _Disconnected += value; }
		remove { _Disconnected -= value; }
	}


	/// <summary>
	/// <see cref="IFileSystemProvider.DirectoryEntryChanged"/> implementation.
	/// </summary>
	public event EventHandler<DirectoryEntryChangedEventArgs>? DirectoryEntryChanged
	{
		add { _DirectoryEntryChanged += value; }
		remove { _DirectoryEntryChanged -= value; }
	}

	// private CancellationTokenSource DisposalTokenSource => _DisposalTokenSource ??= new CancellationTokenSource();
	// private CancellationToken DisposalToken => DisposalTokenSource.Token;

	/// <summary>
	/// <see cref="IFileSystemProvider.RootEntriesChanged"/> implementation.
	/// </summary>
	public event EventHandler<RootEntriesChangedEventArgs>? RootEntriesChanged
	{
		add
		{
			if (_LogTelemetryEvents && !_LoggedTelemetryEvents)
			{
				// Tracer.Trace(GetType(), "RootEntriesChanged add()");

				_LoggedTelemetryEvents = true;
				LogTelemetryEvents();
			}
		}
		remove
		{
		}
	}


	private AsyncLazy<IVsAsyncFileChangeEx> LazyFileSystemChangeService => _LazyFileSystemChangeService;


	public IDictionary<WatchResult, FileWatcherSubscription> WatchedFileSystemEntries =>
		_WatchedFileSystemEntries ??= new Microsoft.Internal.VisualStudio.PlatformUI.HybridDictionary<WatchResult, FileWatcherSubscription>(WatchResultComparer.Instance);


	#endregion Property accessors






	// =========================================================================================================
	#region Methods - FbsqlPlusFileSystemProvider
	// =========================================================================================================


	private static DirectoryEntryChangeType ChangeTypeFromChangeFlags(uint changeAsUint)
	{
		// Tracer.Trace(typeof(FbsqlPlusFileSystemProvider), "ChangeTypeFromChangeFlags()");

		if ((changeAsUint & 0x10u) != 0)
		{
			return DirectoryEntryChangeType.Created;
		}
		if ((changeAsUint & 8u) != 0)
		{
			return DirectoryEntryChangeType.Deleted;
		}
		return DirectoryEntryChangeType.Changed;
	}



	public async Task<bool> ConnectAsync(object sender)
	{
		// Tracer.Trace(GetType(), "Connect()");

		await TaskScheduler.Default;

		OnConnect(sender, new EventArgs());

		return true;
	}

	public async Task<bool> DisconnectAsync(object sender)
	{
		// Tracer.Trace(GetType(), "Disconnect()");

		await TaskScheduler.Default;

		OnDisconnect(sender, new EventArgs());

		return true;
	}



	private async Task<DirectoryEntryInfo?> GetInfoImplAsync(Uri uri, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "GetInfoAsync()");

		ValidateUri(uri, "uri");

		await TaskScheduler.Default;

		if (uri.Scheme.Equals(SystemData.Protocol, StringComparison.OrdinalIgnoreCase))
			return null;

		string localPath = uri.LocalPath;

		if (Directory.Exists(localPath))
		{
			System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(localPath);
			return new Microsoft.VisualStudio.RpcContracts.FileSystem.DirectoryInfo(uri, directoryInfo.Attributes, directoryInfo.CreationTimeUtc, directoryInfo.LastWriteTimeUtc);
		}

		if (File.Exists(localPath))
		{
			System.IO.FileInfo fileInfo = new System.IO.FileInfo(localPath);
			return new FileInfo(uri, fileInfo.Attributes, fileInfo.Length, fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc);
		}
		return null;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IRemoteFileSystemProvider.GetIsConnectedAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public Task<bool> GetIsConnectedAsync(CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "GetIsConnectedAsync()");

		return Task.FromResult(true); 
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IRemoteFileSystemProvider.DownloadFileAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public async Task<Uri> DownloadFileAsync(Uri uri, IProgress<OperationProgressData>? progress, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "DownloadFileAsync()", "uri: {0}.", uri.ToString());

		Requires.NotNull(uri, "uri");
		Requires.Argument(uri.Scheme == Uri.UriSchemeFile
			|| uri.Scheme.Equals(SystemData.Protocol, StringComparison.OrdinalIgnoreCase),
			"uri", "Unsupported URI scheme");

		Uri remoteUri;
		Uri localUri;

		if (uri.Scheme.Equals(SystemData.Protocol, StringComparison.OrdinalIgnoreCase))
		{
			remoteUri = uri;
			localUri = await ((IRemoteFileSystemProvider)this).ConvertRemoteUriToLocalUriAsync(uri, cancellationToken);
			string mkDocument = remoteUri.ToString();

			uint docCookie = RdtManager.GetRdtCookie(mkDocument);

			if (docCookie == 0u)
			{
				Diag.ThrowException(new FileNotFoundException($"File '{remoteUri}' could not be downloaded as '{localUri.LocalPath}' because document cookie could not be retrieved for document moniker '{mkDocument}'"));
			}

			if (!((IBEditorPackage)ApcManager.DdexPackage).TryGetTabbedEditorService(docCookie, false,
				out IBTabbedEditorService tabbedEditorService))
			{
				Diag.ThrowException(new COMException($"Could not find tabbed editor service for document cookie '{docCookie}', and document moniker '{mkDocument}'"));
			}

			IBSqlEditorWindowPane editorPane = tabbedEditorService as IBSqlEditorWindowPane;

			StreamWriter sw = File.AppendText(localUri.AbsolutePath);

			await sw.WriteAsync(editorPane.GetCodeText());

			// Tracer.Trace(GetType(), "DownloadFileAsync()", "Remote to local WRITTEN. remoteUri: {0}, localUri: {1}.", remoteUri.ToString(), localUri.ToString());

		}
		else
		{
			localUri = uri;
			// remoteUri = await ((IRemoteFileSystemProvider)this).ConvertLocalUriToRemoteUriAsync(uri, cancellationToken);
			// Tracer.Trace(GetType(), "DownloadFileAsync()", "Local to remote NOT implemented. localUri: {0}, remoteUri: {1}.", localUri.ToString(), remoteUri.ToString());
		}

		// Get and save script

		return localUri;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IRemoteFileSystemProvider.ConvertLocalUriToRemoteUriAsync"/>
	/// implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public Task<Uri> ConvertLocalUriToRemoteUriAsync(Uri localUri, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "ConvertLocalUriToRemoteUriAsync()", "in/out localUri: {0}.", localUri.ToString());


		// Stays local.
		return Task.FromResult(localUri);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IRemoteFileSystemProvider.ConvertRemoteUriToLocalUriAsync"/>
	/// implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public Task<Uri> ConvertRemoteUriToLocalUriAsync(Uri remoteUri, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "ConvertRemoteUriToLocalUriAsync()", "remoteUri: {0}.", remoteUri.ToString());

		Requires.NotNull(remoteUri, "remoteUri");
		// Requires.Argument(remoteUri.IsAbsoluteUri, "remoteUri", "URI is relative");
		Requires.Argument(remoteUri.Scheme == SystemData.Protocol, "remoteUri", $"Only the '{SystemData.Protocol}' scheme is supported");


		uint docCookie = RdtManager.GetRdtCookie(remoteUri.ToString());

		if (docCookie == 0)
			Diag.ThrowException(new ArgumentException($"Could not find DocCookie for document moniker: {remoteUri}."));

		RunningDocumentInfo docInfo = RdtManager.GetDocumentInfo(docCookie);

		uint saveOptions = (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_PromptSave;


		int hresult = RdtManager.SaveDocuments(saveOptions, docInfo.Hierarchy, docInfo.ItemId, docCookie);

		if (!Native.Succeeded(hresult))
			return Task.FromResult(remoteUri);

		docInfo.Sync();

		return Task.FromResult(new Uri(docInfo.Moniker));
		
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.GetRootEntriesAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public async Task<IReadOnlyList<Uri>> GetRootEntriesAsync(CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "GetRootEntriesAsync()");

		// TBC: Get a list of registered connections database urls.

		return await Task.Run(() => _S_DefaultRoots);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.CopyAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public async Task CopyAsync(Uri sourceUri, Uri destinationUri, bool overwrite,
		IProgress<OperationProgressData>? progress, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "CopyAsync()", "sourceUri : {0}, destinationUri: {1}.", sourceUri.ToString(), destinationUri.ToString());

		// TBC: Dunno
		await TaskScheduler.Default;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.CreateDirectoryAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public async Task CreateDirectoryAsync(Uri uri, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "CreateDirectoryAsync()", "Uri : {0}.", uri.ToString());

		// This cannot be done. Do nothing
		await TaskScheduler.Default;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.DeleteAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public async Task DeleteAsync(Uri uri, bool recursive, IProgress<OperationProgressData>? progress, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "DeleteAsync()", "Uri : {0}.", uri.ToString());

		// TBC
		await TaskScheduler.Default;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.EnumerateDirectoriesAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public IAsyncEnumerable<DirectoryInfo> EnumerateDirectoriesAsync(Uri uri, string searchPattern, SearchOption searchOption, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "EnumerateDirectoriesAsync()", "Uri : {0}.", uri.ToString());

		Uri uri2 = uri;
		string searchPattern2 = searchPattern;


		return EnumerableExtensions.AsAsync(() => ListAsync<DirectoryInfo>(uri2, Requires.NotNull(searchPattern2, "searchPattern"), searchOption, cancellationToken), cancellationToken);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.EnumerateDirectoryEntriesAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public IAsyncEnumerable<DirectoryEntryInfo> EnumerateDirectoryEntriesAsync(Uri uri, string searchPattern, SearchOption searchOption, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "EnumerateDirectoryEntriesAsync()", "Uri : {0}.", uri.ToString());

		Uri uri2 = uri;
		string searchPattern2 = searchPattern;

		return EnumerableExtensions.AsAsync(() => ListAsync<DirectoryEntryInfo>(uri2, Requires.NotNull(searchPattern2, "searchPattern"), searchOption, cancellationToken), cancellationToken);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.EnumerateFilesAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public IAsyncEnumerable<FileInfo> EnumerateFilesAsync(Uri uri, string searchPattern, SearchOption searchOption, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "EnumerateFilesAsync()", "Uri : {0}, searchPattern: {1}.", uri.ToString(), searchPattern);


		static IEnumerable<System.IO.FileInfo> localFuncEnumerateDirEntries(System.IO.DirectoryInfo dirInfo, string pattern, SearchOption option) =>
				dirInfo.EnumerateFiles(pattern, option);

		// IEnumerable<System.IO.FileInfo> localFuncEnumerateUriGroupEntries(Uri uriGroup, string pattern, SearchOption option) =>
		//	EnumerateUriGroupDocs(uriGroup, pattern, option);


		static FileInfo localFuncLocalResultFactory(Uri entryUri, FileSystemInfo info) =>
				new FileInfo(entryUri, info.Attributes, ((System.IO.FileInfo)info).Length, info.CreationTimeUtc, info.LastWriteTimeUtc);

		// static FileInfo localFuncUriResultFactory(Uri entryUri, FileSystemInfo info) =>
		//	new FileInfo(entryUri, info.Attributes, ((System.IO.FileInfo)info).Length, info.CreationTimeUtc, info.LastWriteTimeUtc);

		IAsyncEnumerable<FileInfo> enumerable = EnumerateFileSystemEntriesCoreAsync(uri,
			searchPattern, searchOption, cancellationToken, localFuncEnumerateDirEntries,
			(Func<Uri, FileSystemInfo, FileInfo>)localFuncLocalResultFactory);

		return JsonRpcExtensions.WithJsonRpcSettings(enumerable, _S_AsyncEnumerableRpcSettings);
	}


	/* TBC
	private IEnumerable<System.IO.FileInfo> EnumerateUriGroupDocs(Uri uriGroup, string pattern, SearchOption option)
	{
		// Tracer.Trace(GetType(), "EnumerateUriGroupDocs()");

		// TBC

		return null;
	}
	*/



	private async IAsyncEnumerable<TResult> EnumerateFileSystemEntriesCoreAsync<TResult, TIntermediate>(Uri uri,
		string searchPattern, SearchOption searchOption, [EnumeratorCancellation] CancellationToken cancellationToken,
		Func<System.IO.DirectoryInfo, string, SearchOption, IEnumerable<TIntermediate>> enumerateEntries,
		Func<Uri, TIntermediate, TResult> resultFactory)
		where TResult : DirectoryEntryInfo where TIntermediate : FileSystemInfo
	{
		// Tracer.Trace(GetType(), "EnumerateFileSystemEntriesCoreAsync()");

		ValidateUri(uri, "uri");
		Requires.NotNull(searchPattern, "searchPattern");

		await TaskScheduler.Default;

		System.IO.DirectoryInfo arg = new System.IO.DirectoryInfo(uri.LocalPath);

		foreach (TIntermediate item in enumerateEntries(arg, searchPattern, searchOption))
		{
			cancellationToken.ThrowIfCancellationRequested();
			string text = item.FullName;
			if (item is System.IO.DirectoryInfo && !text.EndsWith("/"))
			{
				text += "/";
			}
			Uri arg2 = new Uri(text, UriKind.Absolute);
			yield return resultFactory(arg2, item);
			await TaskScheduler.Default;
		}
	}


	/* TBC
	/// <summary>
	/// Handle fbsql++ scheme.
	/// </summary>
	private async IAsyncEnumerable<TResult> EnumerateFileSystemEntriesCoreAsync<TResult, TIntermediate>(Uri uri,
		string searchPattern, SearchOption searchOption, [EnumeratorCancellation] CancellationToken cancellationToken,
		Func<Uri, string, SearchOption, IEnumerable<TIntermediate>> enumerateEntries,
		Func<Uri, TIntermediate, TResult> resultFactory)
		where TResult : Uri where TIntermediate : FileSystemInfo
	{
		// Tracer.Trace(GetType(), "EnumerateFileSystemEntriesCoreAsync()");

		ValidateUri(uri, "uri");
		Requires.NotNull(searchPattern, "searchPattern");

		await TaskScheduler.Default;

		Uri arg = uri;

		foreach (TIntermediate item in enumerateEntries(arg, searchPattern, searchOption))
		{
			cancellationToken.ThrowIfCancellationRequested();
			string text = item.FullName;
			if (item is System.IO.DirectoryInfo && !text.EndsWith("/"))
			{
				text += "/";
			}
			Uri arg2 = new Uri(text, UriKind.Absolute);
			yield return resultFactory(arg2, item);
			await TaskScheduler.Default;
		}
	}
	*/


	private static string GetCodeEditorText(Uri uri)
	{
		// Tracer.Trace(typeof(FbsqlPlusFileSystemProvider), "GetCodeEditorText()");


		string code = string.Empty;

		// Find the code window textspan for the matching document moniker and
		// pump it into the writer.

		if (RdtManager.TryGetCodeWindow(uri.ToString(), out IVsCodeWindow codeWindow)
			&& codeWindow is IBSqlEditorWindowPane editorWindow)
		{
			SqlTextSpan sqlTextSpan = editorWindow.GetAllCodeEditorTextSpan2();

			if (sqlTextSpan != null && !string.IsNullOrEmpty(sqlTextSpan.Text))
				code = sqlTextSpan.Text;
		}

		return code;
	}


	private static long GetFileSize(string path)
	{
		// Tracer.Trace(typeof(FbsqlPlusFileSystemProvider), "GetFileSize()");

		if (!path.StartsWith(SystemData.Scheme, StringComparison.OrdinalIgnoreCase))
		{
			return new System.IO.FileInfo(path).Length;
		}

		return GetCodeEditorText(new Uri(path)).Length;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.GetInfoAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public async Task<DirectoryEntryInfo?> GetInfoAsync(Uri uri, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "GetInfoAsync()", "Uri : {0}.", uri.ToString());

		IEnumerable<DirectoryEntryInfo> source = await ListAsync<DirectoryEntryInfo>(uri, null, null, cancellationToken);
		if (source.Count() > 1)
		{
			throw new InvalidOperationException($"More than one directory entry discovered for '{uri}");
		}
		DirectoryEntryInfo directoryEntryInfo = source.SingleOrDefault();
		if (uri.AbsolutePath.EndsWith("/", StringComparison.OrdinalIgnoreCase) && directoryEntryInfo.Attributes == System.IO.FileAttributes.Directory && !directoryEntryInfo.Uri.AbsolutePath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
		{
			UriBuilder uriBuilder = new UriBuilder(directoryEntryInfo.Uri);
			uriBuilder.Path += "/";
			directoryEntryInfo = new DirectoryInfo(uriBuilder.Uri, System.IO.FileAttributes.Directory, directoryEntryInfo.CreationTime, directoryEntryInfo.LastWriteTime);
		}
		return directoryEntryInfo;
	}



	private void LogTelemetryEvents()
	{
		// Tracer.Trace(GetType(), "LogTelemetryEvents()");

		TelemetryEvent telemetryEvent = new TelemetryEvent(C_TelemetryEventLogValue);
		TelemetryService.DefaultSession.PostEvent(telemetryEvent);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.MoveAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public async Task MoveAsync(Uri oldUri, Uri newUri, bool overwrite, IProgress<OperationProgressData>? progress, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "MoveAsync()", "oldUri : {0}, newUri: {1}, overwrite: {2}.", oldUri.ToString(), newUri.ToString(), overwrite);
		// TBC
		await TaskScheduler.Default;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.ReadFileAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public async Task ReadFileAsync(Uri uri, PipeWriter writer, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "ReadFileAsync()", "Uri : {0}.", uri.ToString());

		Requires.NotNull(writer, "writer");
		ValidateUri(uri, "uri");

		string code = GetCodeEditorText(uri);

		if (!string.IsNullOrEmpty(code) || SystemData.Protocol.Equals(uri.Scheme))
		{
			byte[] array = Encoding.Default.GetBytes(code);

			await writer.WriteAsync(array);
			await writer.CompleteAsync();
		}
		else
		{
			await TaskScheduler.Default;

			string localPath = uri.LocalPath;
			int val = (int)GetFileSize(localPath);
			int bufferSize = Math.Min(val, C_DefaultCopyBufferSize);

			using Stream writerStream = writer.AsStream();
			using FileStream stream = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);

			await stream.CopyToAsync(writerStream, bufferSize, cancellationToken);
		}


	}



	private string SaveQueryToLocalPathDialog(Uri remoteUri)
	{
		// Tracer.Trace(GetType(), "SaveQueryToLocalPathDialog()", "remoteUri: {0}.", remoteUri.ToString());

		Requires.NotNull(remoteUri, "remoteUri");

		Requires.Argument(remoteUri.Scheme == SystemData.Protocol, "remoteUri", $"Only the '{SystemData.Protocol}' scheme is supported");

		Uri uri = string.IsNullOrEmpty(remoteUri.Query)
			? remoteUri
			: new UriBuilder(remoteUri) { Query = null }.Uri;

		string[] segments = uri.Segments;


		SaveFileDialog dlg = new()
		{
			// dlg.InitialDirectory = @ "C:\";

			Title = "Save Firebird Query",
			FileName = segments[^1],
			CheckFileExists = false,
			CheckPathExists = true,
			DefaultExt = SystemData.Extension.TrimStart('.'),
			Filter = "Firebird Query files (*.fbsql)|*.fbsql",
			FilterIndex = 1,
			RestoreDirectory = false
		};


		if (dlg.ShowDialog() == DialogResult.OK)
			return dlg.FileName;

		return null;
	}



	public bool Unwatch(string moniker)
	{
		if (_WatchedFileSystemEntries == null)
			return false;

		foreach (KeyValuePair<WatchResult, FbsqlPlusFileSystemProvider.FileWatcherSubscription> subsPair in WatchedFileSystemEntries)
		{
			if (subsPair.Value.MkDocument.Equals(moniker))
			{
				ThreadHelper.JoinableTaskFactory.Run(async delegate
				{
					// Tracer.Trace(GetType(), "RemoveEditorStatus()", "UnW1atching {0}.", moniker);
					await UnwatchAsync(subsPair.Key, default);
				});
				return true;
			}
		}

		return false;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.UnwatchAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public async ValueTask UnwatchAsync(WatchResult watchResult, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "UnwatchAsync()");

		Requires.NotNull(watchResult, "watchResult");
		await TaskScheduler.Default;

		FileWatcherSubscription value;

		lock (WatchedFileSystemEntries)
		{
			if (!WatchedFileSystemEntries.TryGetValue(watchResult, out value))
			{
				throw new ArgumentException("The watch result does not exist", "watchResult");
			}
			WatchedFileSystemEntries.Remove(watchResult);
		}
		await value.UnsubscribeAsync(cancellationToken);
	}



	private bool TrySubscribeToFileSystemChangedEvent(Func<Task<IVsAsyncFileChangeEx>> fileChangeSvc)
	{
		lock (_LockLocal)
		{
			_LazyFileSystemChangeService = new AsyncLazy<IVsAsyncFileChangeEx>(fileChangeSvc, _JoinableTaskFactory);

			return true;
		}
	}



	private static void ValidateUri(Uri uri, string paramName)
	{
		// Tracer.Trace(typeof(FbsqlPlusFileSystemProvider), "ValidateUri()");

		Requires.NotNull(uri, paramName);

		if (uri.Scheme.Equals(SystemData.Protocol, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		if (!uri.IsAbsoluteUri)
		{
			Diag.ThrowException(new ArgumentException($"Only the '{SystemData.Protocol}' scheme and absolute file URIs are supported", paramName));
		}
		if (!uri.IsFile)
		{
			Diag.ThrowException(new ArgumentException($"Only the '{SystemData.Protocol}' scheme and file URIs are supported", paramName));
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.WatchDirectoryAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public ValueTask<WatchResult> WatchDirectoryAsync(Uri uri, bool recursive, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "WatchDirectoryAsync()");

		return WatchFileSystemEntryAsync(uri, true, recursive, cancellationToken);
	}



	private async ValueTask<WatchResult> WatchFileSystemEntryAsync(Uri uri, bool isDirectory, bool recursive, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "WatchFileSystemEntryAsync()");

		ValidateUri(uri, "uri");

		await TaskScheduler.Default;


		if (_WatchedFileSystemEntries != null)
		{
			string moniker = uri.ToString();

			foreach (KeyValuePair<WatchResult, FbsqlPlusFileSystemProvider.FileWatcherSubscription> subsPair in WatchedFileSystemEntries)
			{
				if (subsPair.Value.MkDocument.Equals(moniker))
					return subsPair.Key;
			}
		}


		FileWatcherSubscription subscription = new FileWatcherSubscription(this, isDirectory);

		WatchResult watchResult = await subscription.SubscribeAsync(uri, recursive, cancellationToken);

		lock (WatchedFileSystemEntries)
		{
			WatchedFileSystemEntries[watchResult] = subscription;

			return watchResult;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.WatchFileAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public ValueTask<WatchResult> WatchFileAsync(Uri uri, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "WatchFileAsync()");

		return WatchFileSystemEntryAsync(uri, false, false, cancellationToken);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IFileSystemProvider.WriteFileAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public async Task WriteFileAsync(Uri uri, PipeReader reader, bool overwrite, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "WriteFileAsync()");

		ValidateUri(uri, "uri");
		Requires.NotNull(reader, "reader");

		await TaskScheduler.Default;

		string localPath;

		if (SystemData.Protocol.Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
		{
			localPath = SaveQueryToLocalPathDialog(uri);

			if (localPath == null)
				Diag.ThrowException(new ArgumentException($"No local path provided for {uri}."));

		}
		else
		{
			localPath = uri.LocalPath;
		}

		if (!Directory.Exists(Path.GetDirectoryName(localPath)))
		{
			throw new DirectoryNotFoundException();
		}

		if (!overwrite && File.Exists(localPath))
		{
			throw ErrorCodes.ExceptionFromCode(1000, "The file already exists", uri.AbsolutePath);
		}

		using FileStream stream = new FileStream(localPath, (!overwrite) ? FileMode.CreateNew : FileMode.Create, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);

		await reader.CopyToAsync(stream, cancellationToken);


	}



	private async Task<IEnumerable<T>> ListAsync<T>(Uri uri, string? searchPattern, SearchOption? searchOption, CancellationToken cancellationToken) where T : DirectoryEntryInfo
	{
		// Tracer.Trace(GetType(), "ListAsync()");

		await TaskScheduler.Default;
		return Array.Empty<T>();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// <see cref="IUriDisplayInfoProvider.GetDisplayInfoAsync"/> implementation.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public Task<UriDisplayInfo> GetDisplayInfoAsync(Uri uri, CancellationToken cancellationToken)
	{
		// Tracer.Trace(GetType(), "GetDisplayInfoAsync()", "uri: {0}.", uri);

		try
		{
			Requires.NotNull(uri, "uri");
			Requires.Argument(uri.IsAbsoluteUri, "uri", "URI is relative");
			bool validProtocol = string.Equals(uri.Scheme, SystemData.Protocol, StringComparison.OrdinalIgnoreCase);
			// bool validPointer = string.Equals(uri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase);
			Requires.Argument(validProtocol, "uri", $"Unsupported URI scheme. Only '{SystemData.Protocol}' is supported");

			char dot = SystemData.CompositeSeparator;
			string dotstr = dot.ToString();

			string mkDocument = uri.ToString();

			char sep = '\\';

			if (mkDocument.IndexOf("/") != -1)
				sep = '/';

			string sepstr = sep.ToString();

			string[] segments = mkDocument.Split(sep);

			// Validate extesion
			if (!SystemData.Extension.Equals(Path.GetExtension(segments[^1]), StringComparison.OrdinalIgnoreCase))
				throw new UriFormatException($"Uri must have '{SystemData.Extension}' extension. Uri: {uri}.");

			// Tracer.Trace(GetType(), "GetDisplayInfoAsync()", "segments: {0}.", string.Join(" || ", segments));

			// Server
			// string server = segments[1];

			// Database
			// Deserialize database and convert each component to '/' delimited.
			string database = StringUtils.Deserialize64(segments[^3]).Replace('\\', sep)
				.Replace(":", sepstr).Replace(sepstr + sepstr, sepstr).Replace(sepstr + sepstr, sepstr).Trim(sep);

			// Node type (groupName)
			// The parent folder of the file - in our case the node type.
			string nodeType = segments[^2];

			// Filename.ext (name)
			string fileName = segments[^1];

			// fullGroupName and serverGroupName
			string mkGroup = mkDocument[..(mkDocument.Length - fileName.Length - 1)];



			// Tracer.Trace(GetType(), "GetDisplayInfoAsync()", "uri (mkDocument): {0}\nname (fileName): {1}, fullname: [mkDocument]\ngroupName (nodeType): {2}, fullGroupName (mkGroup): {3}\nserverFullName: [mkDocument], serverFullGroupName: [mkGroup].",
			//	mkDocument, fileName, nodeType, mkGroup);

			UriDisplayInfo info = new(fileName, mkDocument, nodeType, nodeType, mkDocument, mkGroup, mkDocument, mkGroup);

			return Task.FromResult(info);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}


	#endregion Methods and Implementations





	// =========================================================================================================
	#region Event handlers - FbsqlPlusFileSystemProvider
	// =========================================================================================================


	private void OnConnect(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnConnect()");

		_Connected?.Invoke(this, e);
	}



	private void OnDisconnect(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnDisconnect()");

		_Disconnected?.Invoke(this, e);
	}



	private async Task RaiseFileSystemEntryChangedAsync(WatchResult watchResult, uint count, string[] files, uint[] changes, bool isDirectory)
	{
		// Tracer.Trace(GetType(), "RaiseDirectoryEntryChangedAsync()");

		EventHandler<DirectoryEntryChangedEventArgs> fileSystemEntryChanged = _DirectoryEntryChanged;

		if (fileSystemEntryChanged == null)
			return;

		for (int i = 0; i < count; i++)
		{
			Uri uri;

			if (!isDirectory && files[i].StartsWith(SystemData.Scheme, true, CultureInfo.InvariantCulture))
				uri = new Uri(files[i]);
			else
				uri = new Uri(files[i], UriKind.Absolute);


			DirectoryEntryInfo directoryEntryInfo = await GetInfoImplAsync(uri, CancellationToken.None);

			if (directoryEntryInfo == null)
			{
				DateTime dateTime = _S_MinDateUtc;

				directoryEntryInfo = (!isDirectory)
					? new FileInfo(uri, FileAttributes.Normal, 0L, dateTime, dateTime)
					: new DirectoryInfo(uri, FileAttributes.Directory, dateTime, dateTime);
			}

			DirectoryEntryChangeType changeType = ChangeTypeFromChangeFlags(changes[i]);
			fileSystemEntryChanged(this, new DirectoryEntryChangedEventArgs(watchResult, directoryEntryInfo, changeType));
		}
	}


	#endregion Event handlers





	// =========================================================================================================
	#region Private Classes - FbsqlPlusFileSystemProvider
	// =========================================================================================================


	public class FileWatcherSubscription(FbsqlPlusFileSystemProvider provider, bool isDirectory)
		: IVsFreeThreadedFileChangeEvents2
	{
		private readonly FbsqlPlusFileSystemProvider _Provider = provider;
		private readonly bool _IsDirectory = isDirectory;
		private string _MkDocument;


		private const _VSFILECHANGEFLAGS C_AllChangeFlags = _VSFILECHANGEFLAGS.VSFILECHG_Attr
			| _VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Size
			| _VSFILECHANGEFLAGS.VSFILECHG_Del | _VSFILECHANGEFLAGS.VSFILECHG_Add;

		private WatchResult? _WatchResult;

		public string MkDocument => _MkDocument;

		public uint SubscriptionCookie { get; private set; }


		public async ValueTask<WatchResult> SubscribeAsync(Uri uri, bool recursive, CancellationToken cancellationToken)
		{
			// Tracer.Trace(GetType(), "SubscribeAsync()");

			if (SubscriptionCookie != 0)
				Diag.ThrowException(new InvalidOperationException("A FileWatcherSubscription can only be subscribed once"));

			await TaskScheduler.Default;

			IVsAsyncFileChangeEx vsAsyncFileChangeEx = null;

			try
			{
				vsAsyncFileChangeEx = await _Provider.LazyFileSystemChangeService.GetValueAsync(cancellationToken);
				Assumes.Present(vsAsyncFileChangeEx);
			}
			catch (Exception ex)
			{
				Diag.ThrowException(ex);
			}

			uint cookie = 0;
			string scheme = "file";

			if (_IsDirectory)
			{
				_MkDocument = uri.LocalPath;
				cookie = await vsAsyncFileChangeEx.AdviseDirChangeAsync(_MkDocument, recursive, this, cancellationToken);
			}
			else if (!SystemData.Protocol.Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
			{
				_MkDocument = uri.LocalPath;
				cookie = await vsAsyncFileChangeEx.AdviseFileChangeAsync(_MkDocument, C_AllChangeFlags, this, cancellationToken);
			}
			else
			{
				_MkDocument = uri.ToString();
				try
				{
					cookie = await vsAsyncFileChangeEx.AdviseFileChangeAsync(_MkDocument, C_AllChangeFlags, this, cancellationToken);
				}
				catch (Exception ex)
				{
					Diag.ThrowException(ex, $"Exception thrown using moniker: {_MkDocument}.");
				}

				scheme = SystemData.Protocol;
			}

			SubscriptionCookie = cookie;

			_WatchResult = new WatchResult(scheme, (int)SubscriptionCookie, _IsDirectory);

			return _WatchResult;
		}



		public async ValueTask UnsubscribeAsync(CancellationToken cancellationToken)
		{
			// Tracer.Trace(GetType(), "UnsubscribeAsync()");

			if (SubscriptionCookie != 0)
			{
				await TaskScheduler.Default;

				IVsAsyncFileChangeEx vsAsyncFileChangeEx = await _Provider.LazyFileSystemChangeService.GetValueAsync(cancellationToken);
				Assumes.Present(vsAsyncFileChangeEx);

				if (!_IsDirectory)
				{
					await vsAsyncFileChangeEx.UnadviseFileChangeAsync(SubscriptionCookie);
				}
				else
				{
					await vsAsyncFileChangeEx.UnadviseDirChangeAsync(SubscriptionCookie);
				}
				SubscriptionCookie = 0u;
				_WatchResult = null;
			}
		}



		public int FilesChanged(uint count, string[] files, uint[] changes)
		{
			// Tracer.Trace(GetType(), "FilesChanged()");

			NotifyFilesChangedAsync(count, files, changes).FileAndForget(C_FileSystemEntryChangedFaultName);
			return 0;
		}



		public Task NotifyFilesChangedAsync(uint count, string[] files, uint[] changes)
		{
			// Tracer.Trace(GetType(), "NotifyFilesChangedAsync()");

			return _Provider.RaiseFileSystemEntryChangedAsync(_WatchResult, count, files, changes, isDirectory: false);
		}



		int IVsFreeThreadedFileChangeEvents2.DirectoryChangedEx2(string directory, uint count, string[] files, uint[] changes)
		{
			// Tracer.Trace(GetType(), "DirectoryChangedEx2()");

			NotifyDirectoryChangedAsync(count, files, changes).FileAndForget(C_FileSystemEntryChangedFaultName);
			return 0;
		}



		public Task NotifyDirectoryChangedAsync(uint count, string[] files, uint[] changes)
		{
			// Tracer.Trace(GetType(), "NotifyDirectoryChangedAsync()");

			return _Provider.RaiseFileSystemEntryChangedAsync(_WatchResult, count, files, changes, isDirectory: true);
		}



		public int DirectoryChanged(string pszDirectory)
		{
			// Tracer.Trace(GetType(), "DirectoryChanged()");

			return 0;
		}



		public int DirectoryChangedEx(string pszDirectory, string pszFile)
		{
			// Tracer.Trace(GetType(), "DirectoryChangedEx()");

			return 0;
		}
	}


	#endregion Private Classes


}
#pragma warning restore CS8632 // Suppress '#nullable' annotations context warning.
