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
using System.Collections.Generic;

namespace BlackbirdSql.Data.DslClient;

public class DslTransactionOptions
{
	private TimeSpan? _waitTimeout;
	public TimeSpan? WaitTimeout
	{
		get { return _waitTimeout; }
		set
		{
			if (value.HasValue)
			{
				var secs = ((TimeSpan)value).TotalSeconds;
				if (secs < 1 || secs > short.MaxValue)
					throw new ArgumentException($"The value must be between 1 and {short.MaxValue}.");
			}

			_waitTimeout = value;
		}
	}
	internal short? WaitTimeoutTPBValue => (short?)_waitTimeout?.TotalSeconds;

	public DslTransactionBehavior TransactionBehavior { get; set; }

	private IDictionary<string, DslTransactionBehavior> _lockTables;
	public IDictionary<string, DslTransactionBehavior> LockTables
	{
		get
		{
			return _lockTables ??= new Dictionary<string, DslTransactionBehavior>();
		}
		set
		{
			_lockTables = value ?? throw new ArgumentNullException($"{nameof(LockTables)} cannot be null.");
		}
	}

	public long? SnapshotAtNumber { get; set; }
}
