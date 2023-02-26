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



namespace BlackbirdSql.VisualStudio.Ddex.Schema;


// Error suppression
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]


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

		string schemaCollection;

		switch (collectionName)
		{
			case "Columns":
			case "ForeignKeyColumns":
			case "FunctionArguments":
			case "Functions":
			case "Generators":
			case "IndexColumns":
			case "Indexes":
			case "Procedures":
			case "Tables":
			case "Triggers":
			case "ViewColumns":
				schemaCollection = collectionName;
				break;
			case "TriggerColumns":
				schemaCollection = "Columns";
				break;
			case "SystemTriggers":
			case "AutoIncrementTriggers":
			case "StandardTriggers":
				schemaCollection = "Triggers";
				break;
			default:
				return connection.GetSchema(collectionName, restrictions);
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

	#endregion

	#region Private Methods

	private static DataTable PrepareCollection(FbConnection connection, string collectionName, string schemaCollection, string[] restrictions)
	{
		DslSchema returnSchema = collectionName.ToUpperInvariant() switch
		{
			"COLUMNS" => new DslColumns(),
			"FOREIGNKEYCOLUMNS" => new DslForeignKeyColumns(),
			"FUNCTIONARGUMENTS" => new DslFunctionArguments(),
			"FUNCTIONS" => new DslFunctions(),
			"GENERATORS" => new DslGenerators(),
			"INDEXCOLUMNS" => new DslIndexColumns(),
			"INDEXES" => new DslIndexes(),
			"PROCEDURES" => new DslProcedures(),
			"TABLES" => new DslTables(),
			"TRIGGERS" => new DslTriggers(),
			"SYSTEMTRIGGERS" => new DslSystemTriggers(),
			"AUTOINCREMENTTRIGGERS" => new DslAutoIncrementTriggers(),
			"STANDARDTRIGGERS" => new DslStandardTriggers(),
			"TRIGGERCOLUMNS" => new DslTriggerColumns(),
			"VIEWCOLUMNS" => new DslViewColumns(),
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
			"COLUMNS" => new DslColumns(),
			"FOREIGNKEYCOLUMNS" => new DslForeignKeyColumns(),
			"FUNCTIONARGUMENTS" => new DslFunctionArguments(),
			"FUNCTIONS" => new DslFunctions(),
			"GENERATORS" => new DslGenerators(),
			"INDEXCOLUMNS" => new DslIndexColumns(),
			"INDEXES" => new DslIndexes(),
			"PROCEDURES" => new DslProcedures(),
			"TABLES" => new DslTables(),
			"TRIGGERS" => new DslTriggers(),
			"SYSTEMTRIGGERS" => new DslSystemTriggers(),
			"AUTOINCREMENTTRIGGERS" => new DslAutoIncrementTriggers(),
			"STANDARDTRIGGERS" => new DslStandardTriggers(),
			"TRIGGERCOLUMNS" => new DslTriggerColumns(),
			"VIEWCOLUMNS" => new DslViewColumns(),
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

	#endregion
}
