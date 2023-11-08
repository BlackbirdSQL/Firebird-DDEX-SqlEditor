#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using BlackbirdSql.Core;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;




// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration
namespace BlackbirdSql.Common.Ctl;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "UIThread compliance is performed by the class.")]

public sealed class ConnectionPointCookie : IDisposable
{
	private IConnectionPointContainer _Cpc;

	private IConnectionPoint _ConnectionPoint;

	private uint _Cookie;

	public ConnectionPointCookie(object source, object sink, Type eventInterface)
		: this(source, sink, eventInterface, throwException: true)
	{
	}

	~ConnectionPointCookie()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}

		try
		{
			if (_ConnectionPoint != null && _Cookie != 0)
			{
				_ConnectionPoint.Unadvise(_Cookie);
			}
		}
		finally
		{
			_Cookie = 0u;
			_ConnectionPoint = null;
			_Cpc = null;
		}
	}

	public ConnectionPointCookie(object source, object sink, Type eventInterface, bool throwException)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		Exception ex = null;
		if (source is IConnectionPointContainer container)
		{
			_Cpc = container;
			try
			{
				Guid riid = eventInterface.GUID;
				_Cpc.FindConnectionPoint(ref riid, out _ConnectionPoint);
			}
			catch
			{
				_ConnectionPoint = null;
			}

			if (_ConnectionPoint == null)
			{
				ex = new ArgumentException("The source object does not expose the " + eventInterface.Name + " event inteface");
			}
			else if (sink == null || !eventInterface.IsInstanceOfType(sink))
			{
				ex = new InvalidCastException("The sink object does not implement the eventInterface");
			}
			else
			{
				try
				{
					_ConnectionPoint.Advise(sink, out _Cookie);
				}
				catch
				{
					_Cookie = 0u;
					_ConnectionPoint = null;
					ex = new Exception(string.Format(CultureInfo.CurrentCulture, "IConnectionPoint::Advise failed with for event interface '" + eventInterface.Name));
				}
			}
		}
		else
		{
			ex = new InvalidCastException("The source object does not expost IConnectionPointContainer");
		}

		if (throwException && (_ConnectionPoint == null || _Cookie == 0))
		{
			if (ex == null)
			{
				ex = new ArgumentException("Could not create connection point for event interface '" + eventInterface.Name + "'");
				Diag.Dug(ex);
				throw ex;
			}

			Diag.Dug(ex);
			throw ex;
		}
	}

	[Obsolete("Use Dispose()", false)]
	public void Disconnect()
	{
		Dispose(disposing: false);
	}

	[Obsolete("Use Dispose()", false)]
	public void Disconnect(bool release)
	{
		Dispose(disposing: true);
	}
}
