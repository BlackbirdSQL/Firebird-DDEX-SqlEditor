﻿#region Assembly Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Threading;

// namespace Microsoft.SqlServer.ConnectionDlg.Core
namespace BlackbirdSql.Common.Ctl
{
	public class AutoLock
	{
		private readonly ReaderWriterLock _LockObject;

		private readonly bool _IsWriteLocked;

		public AutoLock(ReaderWriterLock lockObj, bool isWriteLock, TimeSpan timeOut, Action action, out Exception exception)
		{
			exception = null;
			try
			{
				_LockObject = lockObj;
				_IsWriteLocked = isWriteLock;
				if (_IsWriteLocked)
				{
					_LockObject.AcquireWriterLock(timeOut);
				}
				else
				{
					_LockObject.AcquireReaderLock(timeOut);
				}

				action();
			}
			catch (Exception ex)
			{
				Exception ex2 = exception = ex;
			}
			finally
			{
				if (_IsWriteLocked && _LockObject.IsWriterLockHeld)
				{
					_LockObject.ReleaseWriterLock();
				}
				else if (!_IsWriteLocked && _LockObject.IsReaderLockHeld)
				{
					_LockObject.ReleaseReaderLock();
				}
			}
		}
	}
}