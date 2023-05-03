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
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using FirebirdSql.Data.FirebirdClient;

using BlackbirdSql.Common;
using BlackbirdSql.Common.Extensions;



namespace BlackbirdSql.VisualStudio.Ddex.Schema;




internal sealed class DslSchemaFactory
{
	#region Static Members

	private static readonly string ResourceName = "FirebirdSql.Data.Schema.FbMetaData.xml";

	#endregion

	#region Constructors

	private DslSchemaFactory()
	{ }

	#endregion

	#region Methods

	// Schema factory to handle custom collections
	public static DataTable GetSchema(FbConnection connection, string collectionName, string[] restrictions)
	{
		// Diag.Trace();

		LinkageParser parser = LinkageParser.Instance(connection);

		string schemaCollection;

		switch (collectionName)
		{
			case "Columns":
			case "ForeignKeyColumns":
			case "ForeignKeys":
			case "FunctionArguments":
			case "Functions":
			case "Generators":
			case "IndexColumns":
			case "Indexes":
			case "Procedures":
			case "ProcedureColumns":
			case "Tables":
			case "Triggers":
			case "ViewColumns":
				schemaCollection = collectionName;
				break;
			case "TriggerColumns":
				schemaCollection = "Columns";
				break;
			case "TriggerGenerators":
			case "IdentityTriggers":
			case "StandardTriggers":
			case "SystemTriggers":
				schemaCollection = "Triggers";
				break;
			default:
				try
				{
					parser.EnterSync();
					return connection.GetSchema(collectionName, restrictions);
				}
				finally
				{
					parser.ExitSync();
				}
		}


		if (parser.ClearToLoad && RequiresTriggers(schemaCollection))
		{
			parser.Execute();
		}


		switch (collectionName)
		{
			case "Generators":
				return parser.GetSequenceSchema(restrictions);
			case "Triggers":
				return parser.GetTriggerSchema(restrictions, -1, -1);
			case "StandardTriggers":
				return parser.GetTriggerSchema(restrictions, 0, 0);
			case "IdentityTriggers":
				return parser.GetTriggerSchema(restrictions, 0, 1);
			case "SystemTriggers":
				return parser.GetTriggerSchema(restrictions, 1, -1);
			default:
				break;
		}


		var filter = string.Format("CollectionName = '{0}'", schemaCollection);
		var ds = new DataSet();

		Assembly assembly = typeof(FirebirdClientFactory).Assembly;
		if (assembly == null)
		{
			DllNotFoundException ex = new(typeof(FirebirdClientFactory).Name + " class assembly not found");
			Diag.Dug(ex);
			throw ex;
		}

		parser.EnterSync();

		var xmlStream = assembly.GetManifestResourceStream(ResourceName);
		if (xmlStream == null)
		{
			parser.ExitSync();
			NullReferenceException ex = new("Resource not found: " + ResourceName);
			Diag.Dug(ex);
			throw ex;
		}

		var oldCulture = Thread.CurrentThread.CurrentCulture;

		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			// ReadXml contains error: http://connect.microsoft.com/VisualStudio/feedback/Validation.aspx?FeedbackID=95116
			// that's the reason for temporarily changing culture
			ds.ReadXml(xmlStream);
		}
		catch
		{
			parser.ExitSync();
			throw;
		}
		finally
		{
			Thread.CurrentThread.CurrentCulture = oldCulture;
		}

		var collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);

		if (collection.Length != 1)
		{
			parser.ExitSync();
			NotSupportedException ex = new("Unsupported collection name " + schemaCollection);
			Diag.Dug(ex);
			throw ex;
		}

		if (restrictions != null && restrictions.Length > (int)collection[0]["NumberOfRestrictions"])
		{
			parser.ExitSync();
			InvalidOperationException ex = new("The number of specified restrictions is not valid.");
			Diag.Dug(ex);
			throw ex;
		}

		if (ds.Tables[DbMetaDataCollectionNames.Restrictions].Select(filter).Length != (int)collection[0]["NumberOfRestrictions"])
		{
			parser.ExitSync();
			InvalidOperationException ex = new("Incorrect restriction definition.");
			Diag.Dug(ex);
			throw ex;
		}

		DataTable schema;

		switch (collection[0]["PopulationMechanism"].ToString())
		{
			case "PrepareCollection":
				schema = PrepareCollection(connection, collectionName, schemaCollection, restrictions);
				break;
			case "DataTable":
				schema = ds.Tables[collection[0]["PopulationString"].ToString()].Copy();
				break;
			case "SQLCommand":
				schema = SqlCommandSchema(connection, collectionName, restrictions);
				break;
			default:
				parser.ExitSync();
				NotSupportedException ex = new("Unsupported population mechanism");
				Diag.Dug(ex);
				throw ex;
		}

		parser.ExitSync();
		return schema;
	}


	public static Task<DataTable> GetSchemaAsync(FbConnection connection, string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		LinkageParser parser = LinkageParser.Instance(connection);

		string schemaCollection;

		switch (collectionName)
		{
			case "Columns":
			case "ForeignKeyColumns":
			case "ForeignKeys":
			case "FunctionArguments":
			case "Functions":
			case "Generators":
			case "IndexColumns":
			case "Indexes":
			case "Procedures":
			case "ProcedureColumns":
			case "Tables":
			case "Triggers":
			case "ViewColumns":
				schemaCollection = collectionName;
				break;
			case "TriggerColumns":
				schemaCollection = "Columns";
				break;
			case "TriggerGenerators":
			case "IdentityTriggers":
			case "StandardTriggers":
			case "SystemTriggers":
				schemaCollection = "Triggers";
				break;
			default:
				try
				{
					parser.EnterSync();
					return connection.GetSchemaAsync(collectionName, restrictions, cancellationToken);
				}
				catch (Exception)
				{
					if (cancellationToken.IsCancellationRequested)
						return Task.FromResult(new DataTable());
					throw;
				}
				finally
				{
					parser.ExitSync();
				}
		}

		if (cancellationToken.IsCancellationRequested)
			return Task.FromResult(new DataTable());


		if (parser.ClearToLoad && RequiresTriggers(schemaCollection))
		{
			parser.Execute();
		}


		switch (collectionName)
		{
			case "Generators":
				return Task.FromResult(parser.GetSequenceSchema(restrictions));
			case "Triggers":
				return Task.FromResult(parser.GetTriggerSchema(restrictions, -1, -1));
			case "StandardTriggers":
				return Task.FromResult(parser.GetTriggerSchema(restrictions, 0, 0));
			case "IdentityTriggers":
				return Task.FromResult(parser.GetTriggerSchema(restrictions, 0, 1));
			case "SystemTriggers":
				return Task.FromResult(parser.GetTriggerSchema(restrictions, 1, -1));
			default:
				break;
		}


		var filter = string.Format("CollectionName = '{0}'", schemaCollection);
		var ds = new DataSet();

		Assembly assembly = typeof(FirebirdClientFactory).Assembly;
		if (assembly == null)
		{
			DllNotFoundException ex = new(typeof(FirebirdClientFactory).Name + " class assembly not found");
			Diag.Dug(ex);
			throw ex;
		}

		var xmlStream = assembly.GetManifestResourceStream(ResourceName);

		if (cancellationToken.IsCancellationRequested)
			return Task.FromResult(new DataTable());

		if (xmlStream == null)
		{
			NullReferenceException ex = new("Resource not found: " + ResourceName);
			Diag.Dug(ex);
			throw ex;
		}

		var oldCulture = Thread.CurrentThread.CurrentCulture;

		parser.EnterSync();

		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			// ReadXml contains error: http://connect.microsoft.com/VisualStudio/feedback/Validation.aspx?FeedbackID=95116
			// that's the reason for temporarily changing culture
			ds.ReadXml(xmlStream);
		}
		catch
		{
			parser.ExitSync();
			throw;
		}
		finally
		{
			Thread.CurrentThread.CurrentCulture = oldCulture;
		}

		if (cancellationToken.IsCancellationRequested)
		{
			parser.ExitSync();
			return Task.FromResult(new DataTable());
		}

		DataRow[] collection;
		try
		{
			collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);
		}
		catch (Exception ex)
		{
			parser.ExitSync();
			Diag.Dug(ex);
			throw;
		}

		if (collection.Length != 1)
		{
			parser.ExitSync();
			NotSupportedException ex = new("Unsupported collection name " + schemaCollection);
			Diag.Dug(ex);
			throw ex;
		}

		if (restrictions != null && restrictions.Length > (int)collection[0]["NumberOfRestrictions"])
		{
			parser.ExitSync();
			InvalidOperationException exbb = new("The number of specified restrictions is not valid.");
			Diag.Dug(exbb);
			throw exbb;
		}

		if (cancellationToken.IsCancellationRequested)
		{
			parser.ExitSync();
			return Task.FromResult(new DataTable());
		}

		if (ds.Tables[DbMetaDataCollectionNames.Restrictions].Select(filter).Length != (int)collection[0]["NumberOfRestrictions"])
		{
			parser.ExitSync();
			InvalidOperationException exbb = new("Incorrect restriction definition.");
			Diag.Dug(exbb);
			throw exbb;
		}

		if (cancellationToken.IsCancellationRequested)
		{
			parser.ExitSync();
			return Task.FromResult(new DataTable());
		}

		Task<DataTable> task;

		switch (collection[0]["PopulationMechanism"].ToString())
		{
			case "PrepareCollection":
				task = PrepareCollectionAsync(connection, collectionName, schemaCollection, restrictions, cancellationToken);
				break;
			case "DataTable":
				task = Task.FromResult(ds.Tables[collection[0]["PopulationString"].ToString()].Copy());
				break;
			case "SQLCommand":
				task = SqlCommandSchemaAsync(connection, collectionName, restrictions, cancellationToken);
				break;
			default:
				parser.ExitSync();
				throw new NotSupportedException("Unsupported population mechanism");
		}

		parser.ExitSync();
		return task;
	}

	#endregion

	#region Private Methods

	private static DataTable PrepareCollection(FbConnection connection, string collectionName, string schemaCollection, string[] restrictions)
	{
		DslSchema dslSchema;
		NotSupportedException ex;

		switch (collectionName.ToUpperInvariant())
		{
			case "COLUMNS":
				dslSchema = new DslColumns(LinkageParser.Instance(connection));
				break;
			case "FOREIGNKEYCOLUMNS":
				dslSchema = new DslForeignKeyColumns(LinkageParser.Instance(connection));
				break;
			case "FOREIGNKEYS":
				dslSchema = new DslForeignKeys();
				break;

			case "FUNCTIONARGUMENTS":
				dslSchema = new DslFunctionArguments(LinkageParser.Instance(connection));
				break;
			case "FUNCTIONS":
				dslSchema = new DslFunctions();
				break;
			case "INDEXCOLUMNS":
				dslSchema = new DslIndexColumns(LinkageParser.Instance(connection));
				break;
			case "INDEXES":
				dslSchema = new DslIndexes();
				break;
			case "PROCEDURES":
				dslSchema = new DslProcedures();
				break;
			case "PROCEDUREPARAMETERS":
				dslSchema = new DslProcedureParameters(LinkageParser.Instance(connection));
				break;
			case "TABLES":
				dslSchema = new DslTables();
				break;
			case "TRIGGERCOLUMNS":
				dslSchema = new DslTriggerColumns(LinkageParser.Instance(connection));
				break;
			case "VIEWCOLUMNS":
				dslSchema = new DslViewColumns(LinkageParser.Instance(connection));
				break;
			case "GENERATORS":
			case "TRIGGERS":
			case "TRIGGERGENERATORS":
				ex = new(string.Format("The raw metadata collection {0} may not be called from here.", collectionName));
				Diag.Dug(ex);
				throw ex;
			case "IDENTITYTRIGGERS":
			case "STANDARDTRIGGERS":
			case "SYSTEMTRIGGERS":
				ex = new(string.Format("The metadata collection {0} is pre-built and may not be called from here.", collectionName));
				Diag.Dug(ex);
				throw ex;
			default:
				ex = new(string.Format("The metadata collection {0} is not supported.", collectionName));
				Diag.Dug(ex);
				throw ex;
		}

		return dslSchema.GetSchema(connection, schemaCollection, restrictions);
	}



	private static Task<DataTable> PrepareCollectionAsync(FbConnection connection, string collectionName, string schemaCollection, string[] restrictions, CancellationToken cancellationToken = default)
	{

		DslSchema dslSchema;
		NotSupportedException ex;

		switch (collectionName.ToUpperInvariant())
		{
			case "COLUMNS":
				dslSchema = new DslColumns(LinkageParser.Instance(connection));
				break;
			case "FOREIGNKEYCOLUMNS":
				dslSchema = new DslForeignKeyColumns(LinkageParser.Instance(connection));
				break;
			case "FOREIGNKEYS":
				dslSchema = new DslForeignKeys();
				break;
			case "FUNCTIONARGUMENTS":
				dslSchema = new DslFunctionArguments(LinkageParser.Instance(connection));
				break;
			case "FUNCTIONS":
				dslSchema = new DslFunctions();
				break;
			case "INDEXCOLUMNS":
				dslSchema = new DslIndexColumns(LinkageParser.Instance(connection));
				break;
			case "INDEXES":
				dslSchema = new DslIndexes();
				break;
			case "PROCEDURES":
				dslSchema = new DslProcedures();
				break;
			case "PROCEDUREPARAMETERS":
				dslSchema = new DslProcedureParameters(LinkageParser.Instance(connection));
				break;
			case "TABLES":
				dslSchema = new DslTables();
				break;
			case "TRIGGERCOLUMNS":
				dslSchema = new DslTriggerColumns(LinkageParser.Instance(connection));
				break;
			case "VIEWCOLUMNS":
				dslSchema = new DslViewColumns(LinkageParser.Instance(connection));
				break;
			case "GENERATORS":
			case "TRIGGERS":
			case "TRIGGERGENERATORS":
				ex = new(string.Format("The raw metadata collection {0} may not be called from here.", collectionName));
				Diag.Dug(ex);
				throw ex;
			case "IDENTITYTRIGGERS":
			case "STANDARDTRIGGERS":
			case "SYSTEMTRIGGERS":
				ex = new(string.Format("The metadata collection {0} is pre-built and may not be called from here.", collectionName));
				Diag.Dug(ex);
				throw ex;
			default:
				ex = new(string.Format("The metadata collection {0} is not supported.", collectionName));
				Diag.Dug(ex);
				throw ex;
		}

		return dslSchema.GetSchemaAsync(connection, schemaCollection, restrictions, cancellationToken);
	}

	private static DataTable SqlCommandSchema(FbConnection connection, string collectionName, string[] restrictions)
	{
		NotImplementedException exbb = new();
		Diag.Dug(exbb);
		throw exbb;
	}
	private static Task<DataTable> SqlCommandSchemaAsync(FbConnection connection, string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		NotImplementedException exbb = new();
		Diag.Dug(exbb);
		throw exbb;
	}


	static bool RequiresTriggers(string collection)
	{
		switch (collection)
		{
			case "ForeignKeys":
			case "Functions":
			case "Indexes":
			case "Procedures":
			case "Tables":
				return false;
			case "Columns":
			case "ForeignKeyColumns":
			case "FunctionArguments":
			case "Generators":
			case "IndexColumns":
			case "ProcedureColumns":
			case "Triggers":
			case "ViewColumns":
			case "TriggerColumns":
			case "TriggerGenerators":
			case "IdentityTriggers":
			case "StandardTriggers":
			case "SystemTriggers":
			default:
				break;
		}

		return true;
	}

	#endregion
}
