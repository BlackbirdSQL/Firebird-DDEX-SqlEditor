/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/BlackbirdSQL/NETProvider/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$OriginalAuthors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using BlackbirdSql.Common;



namespace BlackbirdSql.Data.Common;

internal static class ExplicitCancellation
{
	public static ExplicitCancel Enter(CancellationToken cancellationToken, Action explicitCancel)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			explicitCancel();
			OperationCanceledException ex = new OperationCanceledException(cancellationToken);
			Diag.Dug(ex);
			throw ex;
		}
		var ctr = cancellationToken.Register(explicitCancel);
		return new ExplicitCancel(ctr);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void ExitExplicitCancel(CancellationTokenRegistration cancellationTokenRegistration)
	{
		cancellationTokenRegistration.Dispose();
	}

	[StructLayout(LayoutKind.Auto)]
	internal readonly struct ExplicitCancel : IDisposable
	{
		readonly CancellationTokenRegistration _cancellationTokenRegistration;

		public ExplicitCancel(CancellationTokenRegistration cancellationTokenRegistration)
		{
			_cancellationTokenRegistration = cancellationTokenRegistration;
		}

		public void Dispose()
		{
			ExitExplicitCancel(_cancellationTokenRegistration);
		}

		public readonly CancellationToken CancellationToken => CancellationToken.None;
	}
}
