// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.Traceable

using BlackbirdSql.Core.Diagnostics.Interfaces;
using BlackbirdSql.Core.Interfaces;


namespace BlackbirdSql.Core.Diagnostics;


public class Traceable : AbstractTraceableBase
{
	private readonly IBDependencyManager _DependencyManager;

	private IBTrace _trace;

	public IBDependencyManager DependencyManager => _DependencyManager;

	public override IBTrace Trace
	{
		get
		{
			if (_trace == null && _DependencyManager != null)
			{
				// ServiceManager<IBTrace> serviceManager = new ServiceManager<IBTrace>(_DependencyManager);
				// _trace = serviceManager.GetService(null);
			}

			return _trace;
		}
		set
		{
			_trace = value;
		}
	}

	public Traceable(IBDependencyManager dependencyManager)
	{
		_DependencyManager = dependencyManager;
	}

	public Traceable(IBTrace trace)
	{
		_trace = trace;
	}



	/*
	internal void AssertTraceEvent(bool v1, TraceEventType error, object connection, string v2)
	{
		NotImplementedException ex = new();
		Diag.Dug(ex);
		throw ex;
	}
	*/
}
