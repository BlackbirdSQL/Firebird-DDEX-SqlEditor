#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Globalization;
using BlackbirdSql.Core;
using Microsoft.VisualStudio.OLE.Interop;




// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration
namespace BlackbirdSql.Common.Ctl;

public sealed class ConnectionPointCookie : IDisposable
{
	private IConnectionPointContainer _cpc;

	private IConnectionPoint _connectionPoint;

	private uint cookie;

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
			if (_connectionPoint != null && cookie != 0)
			{
				_connectionPoint.Unadvise(cookie);
			}
		}
		finally
		{
			cookie = 0u;
			_connectionPoint = null;
			_cpc = null;
		}
	}

	public ConnectionPointCookie(object source, object sink, Type eventInterface, bool throwException)
	{
		Exception ex = null;
		if (source is IConnectionPointContainer container)
		{
			_cpc = container;
			try
			{
				Guid riid = eventInterface.GUID;
				_cpc.FindConnectionPoint(ref riid, out _connectionPoint);
			}
			catch
			{
				_connectionPoint = null;
			}

			if (_connectionPoint == null)
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
					_connectionPoint.Advise(sink, out cookie);
				}
				catch
				{
					cookie = 0u;
					_connectionPoint = null;
					ex = new Exception(string.Format(CultureInfo.CurrentCulture, "IConnectionPoint::Advise failed with for event interface '" + eventInterface.Name));
				}
			}
		}
		else
		{
			ex = new InvalidCastException("The source object does not expost IConnectionPointContainer");
		}

		if (throwException && (_connectionPoint == null || cookie == 0))
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
