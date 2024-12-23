﻿/*
 *	This is an override of the FirebirdClient Schema
 *	We're maintaining the same structure so that it's easy to overload any GetSchema's that may need it.
 *	We still use the original Firebird metadata manifest pulled from the Firebird assembly
 *	
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

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;



namespace BlackbirdSql.Data.Model.Schema;


internal abstract class AbstractDslSchema
{
	public AbstractDslSchema()
	{
		// Evs.Trace(GetType(), "AbstractDslSchema");
	}

	#region Abstract Methods

	protected abstract StringBuilder GetCommandText(string[] restrictions);

	#endregion

	#region Methods

	public DataTable GetSchema(IDbConnection connection, string collectionName, string[] restrictions)
	{
		// Evs.Trace(GetType(), "GetSchema", "collectionName: {0}", collectionName);

		DataTable dataTable = new (collectionName);
		FbCommand command = BuildCommand(connection, collectionName, ParseRestrictions(restrictions));

		try
		{
			using (FbDataAdapter adapter = new (command))
			{
				// Exceptions will be handled in SelectObjects();
				adapter.Fill(dataTable);
			}
		}
		finally
		{
			command.Dispose();
		}

		TrimStringFields(dataTable);
		ProcessResult(dataTable, RequiresTriggers(collectionName)
			? connection.ConnectionString : null, RequiresTriggers(collectionName) ? restrictions : null);

		return dataTable;
	}



	public async Task<DataTable> GetSchemaAsync(IDbConnection connection, string collectionName, string[] restrictions, CancellationToken cancelToken = default)
	{
		// Evs.Trace(GetType(), "GetSchemaAsync", "collectionName: {0}", collectionName);

		DataTable dataTable = new (collectionName);
		FbCommand command = await BuildCommandAsync(connection, collectionName, ParseRestrictions(restrictions), cancelToken);

		if (cancelToken.Cancelled())
			return dataTable;

		try
		{
			using (FbDataAdapter adapter = new (command))
			{
				try
				{
					adapter.Fill(dataTable);
				}
				catch (Exception ex)
				{
					if (cancelToken.Cancelled())
						return dataTable;
					Diag.Ex(ex, collectionName);
					throw;
				}
			}
		}
		finally
		{
			command.Dispose();
			await Task.CompletedTask.ConfigureAwait(false);
		}

		if (cancelToken.Cancelled())
			return dataTable;

		TrimStringFields(dataTable);
		if (cancelToken.Cancelled())
			return dataTable;
		ProcessResult(dataTable, connection.ConnectionString, restrictions);

		return dataTable;
	}


	#endregion



	#region Protected Methods

	protected FbCommand BuildCommand(IDbConnection connection, string collectionName, string[] restrictions)
	{
		// Evs.Trace(GetType(), "BuildCommand", "collectionName: {0}", collectionName);

		FbConnection dbConnection = connection as FbConnection;

		GetServerProperties(dbConnection);

		string schemaCollection;

		switch (collectionName)
		{
			case "TriggerColumns":
				schemaCollection = "Columns";
				break;
			case "IdentityTriggers":
			case "StandardTriggers":
			case "SystemTriggers":
				schemaCollection = "Triggers";
				break;
			default:
				schemaCollection = collectionName;
				break;
		}

		string filter = "CollectionName='{0}'".Fmt(schemaCollection);
		StringBuilder builder = GetCommandText(restrictions);
		DataRow[] restrictionRows = dbConnection.GetSchema(DbMetaDataCollectionNames.Restrictions).Select(filter);
		// IDbTransaction transaction = connection.InnerConnection.ActiveTransaction;

		FbCommand command = new(builder.ToString(), dbConnection /*, transaction*/);

		if (restrictions != null && restrictions.Length > 0)
		{
			int index = 0;
			int newIndex;
			string rname;

			for (int i = 0; i < restrictions.Length; i++)
			{
				if (restrictions[i] != null)
				{
					rname = restrictionRows[i]["RestrictionName"].ToString();
					newIndex = command.AddParameter(rname, index, restrictions[i]);

					if (newIndex != -1)
						index = newIndex;
				}
			}
		}


		return command;
	}

	protected async Task<FbCommand> BuildCommandAsync(IDbConnection connection, string collectionName, string[] restrictions, CancellationToken cancelToken)
	{
		// Evs.Trace(GetType(), "BuildCommandAsync", "collectionName: {0}", collectionName);

		FbConnection dbConnection = connection as FbConnection;

		GetServerProperties(dbConnection);

		string schemaCollection;

		switch (collectionName)
		{
			case "TriggerColumns":
				schemaCollection = "Columns";
				break;
			case "IdentityTriggers":
			case "StandardTriggers":
			case "SystemTriggers":
				schemaCollection = "Triggers";
				break;
			default:
				schemaCollection = collectionName;
				break;
		}

		string filter = "CollectionName='{0}'".Fmt(schemaCollection);
		StringBuilder builder = GetCommandText(restrictions);
		DataRow[] restrictionRows = (await dbConnection.GetSchemaAsync(DbMetaDataCollectionNames.Restrictions, cancelToken)).Select(filter);
		// IDbTransaction transaction = connection.InnerConnection.ActiveTransaction;

		FbCommand command = new(builder.ToString(), dbConnection /*, transaction*/);

		if (restrictions != null && restrictions.Length > 0)
		{
			int index = 0;
			int newIndex;
			string rname;

			for (int i = 0; i < restrictions.Length; i++)
			{
				if (restrictions[i] != null)
				{
					rname = restrictionRows[i]["RestrictionName"].ToString();
					newIndex = command.AddParameter(rname, index, restrictions[i]);

					if (newIndex != -1)
						index = newIndex;
				}
			}
		}


		return command;
	}



	protected virtual void ProcessResult(DataTable schema, string connectionString, string[] restrictions)
	{ }

	protected virtual string[] ParseRestrictions(string[] restrictions)
	{
		// Evs.Trace();
		return restrictions;
	}

	#endregion





	#region Private Methods


	/// <summary>
	/// Determines the major version number from the Serverversion on the inner connection.
	/// </summary>
	/// <param name="connection">an open connection, which is used to determine the version number of the connected database server</param>
	private void GetServerProperties(FbConnection connection)
	{
		Version serverVersion = FbServerProperties.ParseServerVersion(connection.ServerVersion);
		MajorVersionNumber = serverVersion.Major;

		try
		{
			FbServerProperties serverProps = new(connection.ConnectionString);

			// int isccode = IscCodes.isc_spb_auth_plugin_name;

			List<object> list = null; // (List<object>)Reflect.InvokeAmbiguousMethod(serverProps, "GetInfo", BindingFlags.Default, [isccode]);

			Authentication = list == null ? "Legacy_UserManager" : (string)list[0];
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}

		// Evs.Debug(GetType(), nameof(GetServerProperties), $"Authentication: {Authentication}.");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if an SE node's collection requires completed Trigger/Generator
	/// linkage tables in order to render, else false.
	/// If true and linkage is incomplete or non-existent, the caller will first
	/// initiate linkage, if required, and wait for the owning Explorer ConnectionNode's
	/// linkage tables to be prepared before allowing a node to be rendered.
	/// </summary>
	// ----------------------------------------------------------------------------------
	private static bool RequiresTriggers(string collection)
	{
		switch (collection)
		{
			case "Columns":
			case "ForeignKeyColumns":
			case "IndexColumns":
			case "TriggerColumns":
				return true;
			default:
				break;
		}

		return false;
	}


	#endregion

	#region Private Static Methods

	private static void TrimStringFields(DataTable schema)
	{
		schema.BeginLoadData();

		foreach (DataRow row in schema.Rows)
		{
			for (int i = 0; i < schema.Columns.Count; i++)
			{
				if (!row.IsNull(schema.Columns[i]) &&
					schema.Columns[i].DataType == typeof(System.String))
				{
					row[schema.Columns[i]] = row[schema.Columns[i]].ToString().Trim();
				}
			}
		}

		schema.EndLoadData();
		schema.AcceptChanges();
	}

	#endregion

	#region Properties


	protected string Authentication { get; private set; }


	/// <summary>
	/// The major version of the connected Firebird server
	/// </summary>
	protected int MajorVersionNumber { get; private set; }


	#endregion

}
