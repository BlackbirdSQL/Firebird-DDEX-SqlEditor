// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.Traceable

using BlackbirdSql.Sys;


namespace BlackbirdSql.Core.Ctl.Diagnostics;

public class Traceable(IBTrace trace) : AbstractTraceableBase
{
	
	private IBTrace _Trace = trace;



	// private readonly IBDependencyManager _DependencyManager;
	// public IBDependencyManager DependencyManager => _DependencyManager;

	public override IBTrace Trace
	{
		get
		{
			/*
			if (_Trace == null && _DependencyManager != null)
			{
				// ServiceManager<IBTrace> serviceManager = new ServiceManager<IBTrace>(_DependencyManager);
				// _Trace = serviceManager.GetService(null);
			}
			*/

			return _Trace;
		}
		set
		{
			_Trace = value;
		}
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
