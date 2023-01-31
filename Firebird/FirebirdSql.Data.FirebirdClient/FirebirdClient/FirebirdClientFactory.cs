/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Data.Common;

using BlackbirdSql.Common;

namespace FirebirdSql.Data.FirebirdClient;

public class FirebirdClientFactory : DbProviderFactory, IServiceProvider
{
	object IServiceProvider.GetService(Type serviceType)
	{
		NotSupportedException ex = new NotSupportedException("Service: " + serviceType.FullName);

		Diag.Dug(ex);

		throw ex;
	}

	#region Static Properties

	public static readonly FirebirdClientFactory Instance = new FirebirdClientFactory();

	#endregion

	#region Properties

	public override bool CanCreateDataSourceEnumerator
	{
		get { return false; }
	}

	#endregion

	#region Constructors

	private FirebirdClientFactory()
		: base()
	{ }

	#endregion

	#region Methods

	public override DbCommand CreateCommand()
	{
		Diag.Trace();
		return new FbCommand();
	}

	public override DbCommandBuilder CreateCommandBuilder()
	{
		Diag.Trace();
		return new FbCommandBuilder();
	}

	public override DbConnection CreateConnection()
	{
		Diag.Trace();
		return new FbConnection();
	}

	public override DbConnectionStringBuilder CreateConnectionStringBuilder()
	{
		Diag.Trace();
		return new FbConnectionStringBuilder();
	}

	public override DbDataAdapter CreateDataAdapter()
	{
		Diag.Trace();
		return new FbDataAdapter();
	}

	public override DbParameter CreateParameter()
	{
		Diag.Trace();
		return new FbParameter();
	}

	#endregion
}
