// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.ConnectionDialogTracer

using System.Threading;
using BlackbirdSql.Core.Diagnostics.Interfaces;

namespace BlackbirdSql.Core.Diagnostics;

internal static class UiTracer
{
	private static Traceable _Traceable = new Traceable((IBTrace)null);

	internal static Traceable TraceSource => _Traceable;

	internal static void Initialize(Traceable traceable)
	{
		if (traceable != null)
		{
			Interlocked.Exchange(ref _Traceable, traceable);
		}
	}
}
