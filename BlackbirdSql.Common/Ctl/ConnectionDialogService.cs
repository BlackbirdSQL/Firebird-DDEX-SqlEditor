#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Interfaces;




namespace BlackbirdSql.Common.Ctl;


public class ConnectionDialogService
{
	private static readonly ConnectionDialogService _Instance = new ConnectionDialogService();

	private IBDependencyManager _DependencyManager;

	private ExtensionProperties _ExtensionProperties = new ExtensionProperties();

	private readonly object _LocalLock = new object();

	public static ConnectionDialogService Instance => _Instance;

	public IBDependencyManager DependencyManager
	{
		get
		{
			if (_ExtensionProperties == null || _DependencyManager == null)
			{
				Initialize(_ExtensionProperties);
			}

			return _DependencyManager;
		}
	}

	public Traceable Trace { get; set; }

	private ConnectionDialogService()
	{
	}

	public void Initialize(ExtensionProperties extensionProperties)
	{
		lock (_LocalLock)
		{
			_ExtensionProperties = extensionProperties ?? new ExtensionProperties(useDefaultLocations: true);
			_DependencyManager = new DependencyManager(_ExtensionProperties);
			Trace = new Traceable(_DependencyManager);
			UiTracer.Initialize(Trace);
		}
	}
}
