/*
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
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core;



namespace BlackbirdSql.VisualStudio.Ddex.Model;


internal abstract class AbstractDslSchema
{
	public AbstractDslSchema()
	{
		// Tracer.Trace(GetType(), "AbstractDslSchema");
	}

	#region Abstract Methods

	protected abstract StringBuilder GetCommandText(string[] restrictions);

	#endregion

	#region Methods

	public DataTable GetSchema(DbConnection connection, string collectionName, string[] restrictions)
	{
		// Tracer.Trace(GetType(), "GetSchema", "collectionName: {0}", collectionName);

		DataTable dataTable = new DataTable(collectionName);
		DbCommand command = BuildCommand(connection, collectionName, ParseRestrictions(restrictions));
		try
		{
			using (DbDataAdapter adapter = DbNative.CreateDbDataAdapter(command))
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
		ProcessResult(dataTable);
		return dataTable;
	}



	public async Task<DataTable> GetSchemaAsync(DbConnection connection, string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		// Tracer.Trace(GetType(), "GetSchemaAsync", "collectionName: {0}", collectionName);

		DataTable dataTable = new (collectionName);
		DbCommand command = BuildCommand(connection, collectionName, ParseRestrictions(restrictions));

		if (cancellationToken.IsCancellationRequested)
			return dataTable;

		try
		{
			using (DbDataAdapter adapter = DbNative.CreateDbDataAdapter(command))
			{
				try
				{
					adapter.Fill(dataTable);
				}
				catch (Exception ex)
				{
					if (cancellationToken.IsCancellationRequested)
						return dataTable;
					Diag.Dug(ex, collectionName);
					throw;
				}
			}
		}
		finally
		{
			command.Dispose();
			await Task.CompletedTask.ConfigureAwait(false);
		}

		if (cancellationToken.IsCancellationRequested)
			return dataTable;

		TrimStringFields(dataTable);
		if (cancellationToken.IsCancellationRequested)
			return dataTable;
		ProcessResult(dataTable);

		return dataTable;
	}


	#endregion



	#region Protected Methods

	protected DbCommand BuildCommand(DbConnection connection, string collectionName, string[] restrictions)
	{
		// Tracer.Trace(GetType(), "BuildCommand", "collectionName: {0}", collectionName);

		SetMajorVersionNumber(connection);

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

		string filter = string.Format("CollectionName='{0}'", schemaCollection);
		StringBuilder builder = GetCommandText(restrictions);
		DataRow[] restrictionRows = connection.GetSchema(DbMetaDataCollectionNames.Restrictions).Select(filter);
		// var transaction = connection.InnerConnection.ActiveTransaction;

		DbCommand command = connection.CreateDatabaseCommand(builder.ToString() /*, transaction*/);

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



	protected virtual void ProcessResult(DataTable schema)
	{ }

	protected virtual string[] ParseRestrictions(string[] restrictions)
	{
		// Tracer.Trace();
		return restrictions;
	}

	#endregion

	#region Private Methods
	/// <summary>
	/// Determines the major version number from the Serverversion on the inner connection.
	/// </summary>
	/// <param name="connection">an open connection, which is used to determine the version number of the connected database server</param>
	private void SetMajorVersionNumber(IDbConnection connection)
	{
		var serverVersion = connection.GetVersion();
		MajorVersionNumber = serverVersion.Major;
	}
	#endregion

	#region Private Static Methods

	private static void TrimStringFields(DataTable schema)
	{
		schema.BeginLoadData();

		foreach (DataRow row in schema.Rows)
		{
			for (var i = 0; i < schema.Columns.Count; i++)
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
	/// <summary>
	/// The major version of the connected Firebird server
	/// </summary>
	protected int MajorVersionNumber { get; private set; }
	#endregion
}
