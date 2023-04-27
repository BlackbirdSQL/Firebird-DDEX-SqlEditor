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
using System.Net.Security;
using BlackbirdDsl;

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

		ExpressionParser parser = ExpressionParser.Instance(connection);

		if (parser.ClearToLoadAsync && !RequiresTriggers(collectionName))
		{
			parser.AsyncLoad();
		}

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
				return connection.GetSchema(collectionName, restrictions);
		}


		if (parser.ClearToLoad && RequiresTriggers(schemaCollection))
		{
			parser.Load();
		}

		if (!parser.Requesting)
		{
			if (collectionName == "Generators")
				return parser.GetSequenceSchema(restrictions);
			else if (collectionName == "Triggers")
				return parser.GetTriggerSchema(restrictions, -1, -1);
			else if (collectionName == "StandardTriggers")
				return parser.GetTriggerSchema(restrictions, 0, 0);
			else if (collectionName == "IdentityTriggers")
				return parser.GetTriggerSchema(restrictions, 0, 1);
			else if (collectionName == "SystemTriggers")
				return parser.GetTriggerSchema(restrictions, 1, -1);
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
		if (xmlStream == null)
		{
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
		finally
		{
			Thread.CurrentThread.CurrentCulture = oldCulture;
		}

		var collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);

		if (collection.Length != 1)
		{
			NotSupportedException ex = new("Unsupported collection name " + schemaCollection);
			Diag.Dug(ex);
			throw ex;
		}

		if (restrictions != null && restrictions.Length > (int)collection[0]["NumberOfRestrictions"])
		{
			InvalidOperationException ex = new("The number of specified restrictions is not valid.");
			Diag.Dug(ex);
			throw ex;
		}

		if (ds.Tables[DbMetaDataCollectionNames.Restrictions].Select(filter).Length != (int)collection[0]["NumberOfRestrictions"])
		{
			InvalidOperationException ex = new("Incorrect restriction definition.");
			Diag.Dug(ex);
			throw ex;
		}

		switch (collection[0]["PopulationMechanism"].ToString())
		{
			case "PrepareCollection":
				return PrepareCollection(connection, collectionName, schemaCollection, restrictions);

			case "DataTable":
				return ds.Tables[collection[0]["PopulationString"].ToString()].Copy();

			case "SQLCommand":
				return SqlCommandSchema(connection, collectionName, restrictions);

			default:
				NotSupportedException ex = new("Unsupported population mechanism");
				Diag.Dug(ex);
				throw ex;
		}
	}


	public static Task<DataTable> GetSchemaAsync(FbConnection connection, string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
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
					return connection.GetSchemaAsync(collectionName, restrictions, cancellationToken);
				}
				catch (Exception)
				{
					if (cancellationToken.IsCancellationRequested)
						return Task.FromResult(new DataTable());
					throw;
				}
		}

		if (cancellationToken.IsCancellationRequested)
			return Task.FromResult(new DataTable());

		/*
		ExpressionParser parser = ExpressionParser.Instance(connection);

		if (!parser.Requesting)
		{
			if (collectionName == "Generators")
				return parser.GetSequenceSchemaAsync(restrictions, cancellationToken);
			else if (collectionName == "Triggers")
				return parser.GetTriggerSchemaAsync(restrictions, -1, -1, cancellationToken);
			else if (collectionName == "StandardTriggers")
				return parser.GetTriggerSchemaAsync(restrictions, 0, 0, cancellationToken);
			else if (collectionName == "IdentityTriggers")
				return parser.GetTriggerSchemaAsync(restrictions, 0, 1, cancellationToken);
			else if (collectionName == "SystemTriggers")
				return parser.GetTriggerSchemaAsync(restrictions, 1, -1, cancellationToken);
		}
		*/

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

		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			// ReadXml contains error: http://connect.microsoft.com/VisualStudio/feedback/Validation.aspx?FeedbackID=95116
			// that's the reason for temporarily changing culture
			ds.ReadXml(xmlStream);
		}
		finally
		{
			Thread.CurrentThread.CurrentCulture = oldCulture;
		}

		if (cancellationToken.IsCancellationRequested)
			return Task.FromResult(new DataTable());

		DataRow[] collection;
		try
		{
			collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		if (collection.Length != 1)
		{
			NotSupportedException ex = new("Unsupported collection name " + schemaCollection);
			Diag.Dug(ex);
			throw ex;
		}

		if (restrictions != null && restrictions.Length > (int)collection[0]["NumberOfRestrictions"])
		{
			InvalidOperationException exbb = new("The number of specified restrictions is not valid.");
			Diag.Dug(exbb);
			throw exbb;
		}

		if (cancellationToken.IsCancellationRequested)
			return Task.FromResult(new DataTable());

		if (ds.Tables[DbMetaDataCollectionNames.Restrictions].Select(filter).Length != (int)collection[0]["NumberOfRestrictions"])
		{
			InvalidOperationException exbb = new("Incorrect restriction definition.");
			Diag.Dug(exbb);
			throw exbb;
		}

		if (cancellationToken.IsCancellationRequested)
			return Task.FromResult(new DataTable());

		switch (collection[0]["PopulationMechanism"].ToString())
		{
			case "PrepareCollection":
				return PrepareCollectionAsync(connection, collectionName, schemaCollection, restrictions, cancellationToken);

			case "DataTable":
				return Task.FromResult(ds.Tables[collection[0]["PopulationString"].ToString()].Copy());

			case "SQLCommand":
				return SqlCommandSchemaAsync(connection, collectionName, restrictions, cancellationToken);

			default:
				throw new NotSupportedException("Unsupported population mechanism");
		}
	}

	#endregion

	#region Private Methods

	private static DataTable PrepareCollection(FbConnection connection, string collectionName, string schemaCollection, string[] restrictions)
	{
		DslSchema returnSchema = collectionName.ToUpperInvariant() switch
		{
			"COLUMNS" => new DslColumns(ExpressionParser.Instance(connection)),
			"FOREIGNKEYCOLUMNS" => new DslForeignKeyColumns(ExpressionParser.Instance(connection)),
			"FOREIGNKEYS" => new DslForeignKeys(),
			"FUNCTIONARGUMENTS" => new DslFunctionArguments(ExpressionParser.Instance(connection)),
			"FUNCTIONS" => new DslFunctions(),
			"GENERATORS" => new DslRawGenerators(ExpressionParser.Instance(connection)),
			"INDEXCOLUMNS" => new DslIndexColumns(ExpressionParser.Instance(connection)),
			"INDEXES" => new DslIndexes(),
			"PROCEDURES" => new DslProcedures(),
			"PROCEDUREPARAMETERS" => new DslProcedureParameters(ExpressionParser.Instance(connection)),
			"TABLES" => new DslTables(),
			"TRIGGERS" => new DslRawTriggers(ExpressionParser.Instance(connection)),
			"TRIGGERGENERATORS" => new DslRawTriggerGenerators(ExpressionParser.Instance(connection)),
			"IDENTITYTRIGGERS" => new DslIdentityTriggers(ExpressionParser.Instance(connection)),
			"STANDARDTRIGGERS" => new DslStandardTriggers(ExpressionParser.Instance(connection)),
			"SYSTEMTRIGGERS" => new DslSystemTriggers(ExpressionParser.Instance(connection)),
			"TRIGGERCOLUMNS" => new DslTriggerColumns(ExpressionParser.Instance(connection)),
			"VIEWCOLUMNS" => new DslViewColumns(ExpressionParser.Instance(connection)),
			_ => ((Func<DslSchema>)(() =>
				{
					NotSupportedException exbb = new(string.Format("The metadata collection {0} is not supported.", collectionName));
					Diag.Dug(exbb);
					throw exbb;
				}))(),
		};
		return returnSchema.GetSchema(connection, schemaCollection, restrictions);
	}



	private static Task<DataTable> PrepareCollectionAsync(FbConnection connection, string collectionName, string schemaCollection, string[] restrictions, CancellationToken cancellationToken = default)
	{
		DslSchema returnSchema = collectionName.ToUpperInvariant() switch
		{
			"COLUMNS" => new DslColumns(ExpressionParser.Instance(connection)),
			"FOREIGNKEYCOLUMNS" => new DslForeignKeyColumns(ExpressionParser.Instance(connection)),
			"FOREIGNKEYS" => new DslForeignKeys(),
			"FUNCTIONARGUMENTS" => new DslFunctionArguments(ExpressionParser.Instance(connection)),
			"FUNCTIONS" => new DslFunctions(),
			"GENERATORS" => new DslRawGenerators(ExpressionParser.Instance(connection)),
			"INDEXCOLUMNS" => new DslIndexColumns(ExpressionParser.Instance(connection)),
			"INDEXES" => new DslIndexes(),
			"PROCEDURES" => new DslProcedures(),
			"PROCEDUREPARAMETERS" => new DslProcedureParameters(ExpressionParser.Instance(connection)),
			"TABLES" => new DslTables(),
			"TRIGGERS" => new DslRawTriggers(ExpressionParser.Instance(connection)),
			"TRIGGERGENERATORS" => new DslRawTriggerGenerators(ExpressionParser.Instance(connection)),
			"IDENTITYTRIGGERS" => new DslIdentityTriggers(ExpressionParser.Instance(connection)),
			"STANDARDTRIGGERS" => new DslStandardTriggers(ExpressionParser.Instance(connection)),
			"SYSTEMTRIGGERS" => new DslSystemTriggers(ExpressionParser.Instance(connection)),
			"TRIGGERCOLUMNS" => new DslTriggerColumns(ExpressionParser.Instance(connection)),
			"VIEWCOLUMNS" => new DslViewColumns(ExpressionParser.Instance(connection)),
			_ => ((Func<DslSchema>)(() =>
				{
					NotSupportedException exbb = new(string.Format("The metadata collection {0} is not supported.", collectionName));
					Diag.Dug(exbb);
					throw exbb;
				}))(),
		};

		return returnSchema.GetSchemaAsync(connection, schemaCollection, restrictions, cancellationToken);
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
