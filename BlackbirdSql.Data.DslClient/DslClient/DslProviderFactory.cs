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

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)
using System;
using System.Data.Common;


#if NET
using System.Data.Entity.Core.Common;
#endif

using BlackbirdSql.Common;
using BlackbirdSql.Data.Common;



namespace BlackbirdSql.Data.DslClient;

#if NET
public class DslProviderFactory : DbProviderFactory
#else
public class DslProviderFactory : DbProviderFactory, IServiceProvider
#endif
{
#region Variables

	/// <summary>
	///     The singleton instance.
	/// </summary>
	public static readonly DslProviderFactory Instance = new();

#endregion

#region Properties


	public override bool CanCreateDataSourceEnumerator
	{
		get
		{
			Diag.Dug();
			return false;
		}
	}

#endregion

#region Constructors

	private DslProviderFactory()
	{
		Diag.Dug();
	}

#endregion

#region Methods

	/// <summary>
	///     Creates a new command.
	/// </summary>
	/// <returns>The new command.</returns>
	public override DbCommand CreateCommand()
	{
		Diag.Dug();

		return new DslCommand();
	}

	/// <summary>
	///     Creates a new connection.
	/// </summary>
	/// <returns>The new connection.</returns>
	public override DbConnection CreateConnection()
	{
		Diag.Dug();

		return new DslConnection();
	}

	public override DbCommandBuilder CreateCommandBuilder()
	{
		Diag.Dug();
		return new DslCommandBuilder();
	}


	/// <summary>
	///     Creates a new connection string builder.
	/// </summary>
	/// <returns>The new connection string builder.</returns>
	public override DbConnectionStringBuilder CreateConnectionStringBuilder()
	{
		Diag.Dug();

		return new ConnectionStringBuilder();
	}

	public override DbDataAdapter CreateDataAdapter()
	{
		Diag.Dug();
		return new DslDataAdapter();
	}

	/// <summary>
	///     Creates a new parameter.
	/// </summary>
	/// <returns>The new parameter.</returns>
	public override DbParameter CreateParameter()
	{
		Diag.Dug();

		return new DslParameter();
	}


#endregion

#if NETFRAMEWORK

#region IServiceProvider Members

	object IServiceProvider.GetService(Type serviceType)
	{
		Diag.Dug();

		if (serviceType == typeof(DbProviderServices))
		{
			Diag.Dug("Instantiating ProviderServices");

			ProviderServices providerServices = ProviderServices.Instance;

			if (providerServices == null)
				Diag.Dug(true, "ProviderServices could not be instantiated");


			return providerServices;
		}
		else
		{
			Diag.Dug(true, "No service for: " + serviceType.FullName);

			return null;
		}
	}

#endregion

#endif
}
