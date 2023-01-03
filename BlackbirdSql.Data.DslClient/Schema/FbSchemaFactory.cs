﻿/*
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
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Common;
using BlackbirdSql.Data.DslClient;

namespace BlackbirdSql.Data.Schema;

internal sealed class FbSchemaFactory
{
	#region Static Members

	private static readonly string ResourceName = "BlackbirdSql.Data.Schema.FbMetaData.xml";

	#endregion

	#region Constructors

	private FbSchemaFactory()
	{ }

	#endregion

	#region Methods

	public static DataTable GetSchema(DslConnection connection, string collectionName, string[] restrictions)
	{
		var filter = string.Format("CollectionName = '{0}'", collectionName);

		string str = "";
		DataTable dataTable;

		if (restrictions != null)
		{
			foreach (object item in restrictions)
			{
				str += (item != null && item != DBNull.Value ? item.ToString() : "null") + ", ";
			}
		}

		Diag.Dug(String.Format("{0} restrictions: {1}", filter, str));

		var ds = new DataSet();
		using (var xmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
		{
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
		}

		var collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);

		if (collection.Length != 1)
		{
			Diag.Dug(true, "Unsupported collection name.");
			throw new NotSupportedException("Unsupported collection name.");
		}

		if (restrictions != null && restrictions.Length > (int)collection[0]["NumberOfRestrictions"])
		{
			Diag.Dug(true, "The number of specified restrictions is not valid.");
			throw new InvalidOperationException("The number of specified restrictions is not valid.");
		}

		if (ds.Tables[DbMetaDataCollectionNames.Restrictions].Select(filter).Length != (int)collection[0]["NumberOfRestrictions"])
		{
			Diag.Dug(true, "Incorrect restriction definition.");
			throw new InvalidOperationException("Incorrect restriction definition.");
		}


		switch (collection[0]["PopulationMechanism"].ToString())
		{
			case "PrepareCollection":
				dataTable = PrepareCollection(connection, collectionName, restrictions);
				break;

			case "DataTable":
				dataTable = ds.Tables[collection[0]["PopulationString"].ToString()].Copy();
				break;

			case "SQLCommand":
				dataTable = SqlCommandSchema(connection, collectionName, restrictions);
				break;

			default:
				Diag.Dug(true, "Unsupported population mechanism");
				throw new NotSupportedException("Unsupported population mechanism");
		}

		Diag.Dug("PopulationMechanism: " + collection[0]["PopulationMechanism"].ToString() + " PopulationString: " + collection[0]["PopulationString"].ToString() + " Rowcount: " + dataTable.Rows.Count);

		return dataTable;


	}


	public static Task<DataTable> GetSchemaAsync(DslConnection connection, string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		var filter = string.Format("CollectionName = '{0}'", collectionName);
		var ds = new DataSet();
		using (var xmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
		{
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
		}

		var collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);

		if (collection.Length != 1)
		{
			Diag.Dug(true, "Unsupported collection name.");
			throw new NotSupportedException("Unsupported collection name.");
		}

		if (restrictions != null && restrictions.Length > (int)collection[0]["NumberOfRestrictions"])
		{
			Diag.Dug(true, "The number of specified restrictions is not valid.");
			throw new InvalidOperationException("The number of specified restrictions is not valid.");
		}

		if (ds.Tables[DbMetaDataCollectionNames.Restrictions].Select(filter).Length != (int)collection[0]["NumberOfRestrictions"])
		{
			Diag.Dug(true, "Incorrect restriction definition.");
			throw new InvalidOperationException("Incorrect restriction definition.");
		}

		switch (collection[0]["PopulationMechanism"].ToString())
		{
			case "PrepareCollection":
				return PrepareCollectionAsync(connection, collectionName, restrictions, cancellationToken);

			case "DataTable":
				return Task.FromResult(ds.Tables[collection[0]["PopulationString"].ToString()].Copy());

			case "SQLCommand":
				return SqlCommandSchemaAsync(connection, collectionName, restrictions, cancellationToken);

			default:
				Diag.Dug(true, "Unsupported population mechanism");
				throw new NotSupportedException("Unsupported population mechanism");
		}
	}

	#endregion

	#region Private Methods

	private static DataTable PrepareCollection(DslConnection connection, string collectionName, string[] restrictions)
	{
		Diag.Dug("collectionName: " + collectionName);

		FbSchema returnSchema = collectionName.ToUpperInvariant() switch
		{
			"CHARACTERSETS" => new FbCharacterSets(),
			"CHECKCONSTRAINTS" => new FbCheckConstraints(),
			"CHECKCONSTRAINTSBYTABLE" => new FbChecksByTable(),
			"COLLATIONS" => new FbCollations(),
			"COLUMNS" => new FbColumns(),
			"COLUMNPRIVILEGES" => new FbColumnPrivileges(),
			"DOMAINS" => new FbDomains(),
			"FOREIGNKEYCOLUMNS" => new FbForeignKeyColumns(),
			"FOREIGNKEYS" => new FbForeignKeys(),
			"FUNCTIONS" => new FbFunctions(),
			"FUNCTIONARGUMENTS" => new FbFunctionArguments(),
			"FUNCTIONPRIVILEGES" => new FbFunctionPrivileges(),
			"GENERATORS" => new FbGenerators(),
			"INDEXCOLUMNS" => new FbIndexColumns(),
			"INDEXES" => new FbIndexes(),
			"PRIMARYKEYS" => new FbPrimaryKeys(),
			"PROCEDURES" => new FbProcedures(),
			"PROCEDUREPARAMETERS" => new FbProcedureParameters(),
			"PROCEDUREPRIVILEGES" => new FbProcedurePrivileges(),
			"ROLES" => new FbRoles(),
			"TABLES" => new FbTables(),
			"TABLECONSTRAINTS" => new FbTableConstraints(),
			"TABLEPRIVILEGES" => new FbTablePrivileges(),
			"TRIGGERS" => new FbTriggers(),
			"UNIQUEKEYS" => new FbUniqueKeys(),
			"VIEWCOLUMNS" => new FbViewColumns(),
			"VIEWS" => new FbViews(),
			"VIEWPRIVILEGES" => new FbViewPrivileges(),
			_ => throw new NotSupportedException("The specified metadata collection is not supported."),
		};
		return returnSchema.GetSchema(connection, collectionName, restrictions);
	}
	private static Task<DataTable> PrepareCollectionAsync(DslConnection connection, string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		FbSchema returnSchema = collectionName.ToUpperInvariant() switch
		{
			"CHARACTERSETS" => new FbCharacterSets(),
			"CHECKCONSTRAINTS" => new FbCheckConstraints(),
			"CHECKCONSTRAINTSBYTABLE" => new FbChecksByTable(),
			"COLLATIONS" => new FbCollations(),
			"COLUMNS" => new FbColumns(),
			"COLUMNPRIVILEGES" => new FbColumnPrivileges(),
			"DOMAINS" => new FbDomains(),
			"FOREIGNKEYCOLUMNS" => new FbForeignKeyColumns(),
			"FOREIGNKEYS" => new FbForeignKeys(),
			"FUNCTIONS" => new FbFunctions(),
			"FUNCTIONARGUMENTS" => new FbFunctionArguments(),
			"FUNCTIONPRIVILEGES" => new FbFunctionPrivileges(),
			"GENERATORS" => new FbGenerators(),
			"INDEXCOLUMNS" => new FbIndexColumns(),
			"INDEXES" => new FbIndexes(),
			"PRIMARYKEYS" => new FbPrimaryKeys(),
			"PROCEDURES" => new FbProcedures(),
			"PROCEDUREPARAMETERS" => new FbProcedureParameters(),
			"PROCEDUREPRIVILEGES" => new FbProcedurePrivileges(),
			"ROLES" => new FbRoles(),
			"TABLES" => new FbTables(),
			"TABLECONSTRAINTS" => new FbTableConstraints(),
			"TABLEPRIVILEGES" => new FbTablePrivileges(),
			"TRIGGERS" => new FbTriggers(),
			"UNIQUEKEYS" => new FbUniqueKeys(),
			"VIEWCOLUMNS" => new FbViewColumns(),
			"VIEWS" => new FbViews(),
			"VIEWPRIVILEGES" => new FbViewPrivileges(),
			_ => throw new NotSupportedException("The specified metadata collection is not supported."),
		};
		return returnSchema.GetSchemaAsync(connection, collectionName, restrictions, cancellationToken);
	}

	private static DataTable SqlCommandSchema(DslConnection connection, string collectionName, string[] restrictions)
	{
		Diag.Dug(true, "Not implemented");
		throw new NotImplementedException();
	}
	private static Task<DataTable> SqlCommandSchemaAsync(DslConnection connection, string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		Diag.Dug(true, "Not implemented");
		throw new NotImplementedException();
	}

	#endregion
}
