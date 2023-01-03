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

namespace BlackbirdSql.Data.Isql;

public class UnknownStatementEventArgs : EventArgs
{
	public DslStatement Statement { get; private set; }
	public bool Handled { get; set; }
	public bool Ignore { get; set; }
	public SqlStatementType NewStatementType { get; set; }

	public UnknownStatementEventArgs(DslStatement statement)
	{
		Statement = statement;
		Handled = false;
		Ignore = false;
	}
}
