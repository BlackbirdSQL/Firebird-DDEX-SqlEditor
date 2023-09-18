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

using BlackbirdSql.Core;
using BlackbirdSql.VisualStudio.Ddex.Extensions;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.VisualStudio.Ddex.Model;




internal sealed class DslSchemaFactory
{
	#region Static Members

	private static readonly string ResourceName = "FirebirdSql.Data.Schema.FbMetaData.xml";

	#endregion

	#region Constructors

	private DslSchemaFactory()
	{
		Tracer.Trace(GetType(), "DslSchemaFactory.DslSchemaFactory");
	}

	#endregion

	#region Methods

	// Schema factory to handle custom collections
	public static DataTable GetSchema(FbConnection connection, string collectionName, string[] restrictions)
	{
		Tracer.Trace(typeof(DslSchemaFactory), "DslSchemaFactory.GetSchema", "collectionName: {0}", collectionName);

		LinkageParser parser = null;
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
			case "ProcedureParameters":
			case "Tables":
			case "Triggers":
			case "ViewColumns":
				schemaCollection = collectionName;
				break;
			case "TriggerColumns":
				schemaCollection = "Columns";
				break;
			case "TriggerDependencies":
			case "IdentityTriggers":
			case "StandardTriggers":
			case "SystemTriggers":
				schemaCollection = "Triggers";
				break;
			default:
				try
				{
					parser = LinkageParser.Instance(connection);
					parser?.SyncEnter(true);
					return connection.GetSchema(collectionName, restrictions);
				}
				finally
				{
					parser?.SyncExit();
				}
		}


		if (RequiresTriggers(schemaCollection))
		{
			parser = LinkageParser.EnsureInstance(connection);
			if (parser.ClearToLoad)
				parser.Execute();
		}
		else
		{
			parser = LinkageParser.Instance(connection);
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

		parser?.SyncEnter(true);

		var xmlStream = assembly.GetManifestResourceStream(ResourceName);
		if (xmlStream == null)
		{
			parser?.SyncExit();
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
			parser?.SyncExit();
			throw;
		}
		finally
		{
			Thread.CurrentThread.CurrentCulture = oldCulture;
		}

		var collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);

		if (collection.Length != 1)
		{
			parser?.SyncExit();
			NotSupportedException ex = new("Unsupported collection name " + schemaCollection);
			Diag.Dug(ex);
			throw ex;
		}

		if (restrictions != null && restrictions.Length > (int)collection[0]["NumberOfRestrictions"])
		{
			parser?.SyncExit();
			InvalidOperationException ex = new("The number of specified restrictions is not valid.");
			Diag.Dug(ex);
			throw ex;
		}

		if (ds.Tables[DbMetaDataCollectionNames.Restrictions].Select(filter).Length != (int)collection[0]["NumberOfRestrictions"])
		{
			parser?.SyncExit();
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
				parser?.SyncExit();
				NotSupportedException ex = new("Unsupported population mechanism");
				Diag.Dug(ex);
				throw ex;
		}

		parser?.SyncExit();
		return schema;
	}


	public static Task<DataTable> GetSchemaAsync(FbConnection connection, string collectionName, string[] restrictions, CancellationToken cancellationToken = default)
	{
		Tracer.Trace(typeof(DslSchemaFactory), "DslSchemaFactory.GetSchemaAsync", "collectionName: {0}", collectionName);

		LinkageParser parser = null;
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
			case "ProcedureParameters":
			case "Tables":
			case "Triggers":
			case "ViewColumns":
				schemaCollection = collectionName;
				break;
			case "TriggerColumns":
				schemaCollection = "Columns";
				break;
			case "TriggerDependencies":
			case "IdentityTriggers":
			case "StandardTriggers":
			case "SystemTriggers":
				schemaCollection = "Triggers";
				break;
			default:
				try
				{
					parser = LinkageParser.Instance(connection);
					parser?.SyncEnter(true);
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
					parser?.SyncExit();
				}
		}

		if (cancellationToken.IsCancellationRequested)
			return Task.FromResult(new DataTable());


		if (RequiresTriggers(schemaCollection))
		{
			parser = LinkageParser.EnsureInstance(connection);
			if (parser.ClearToLoad)
				parser.Execute();
		}
		else
		{
			parser = LinkageParser.Instance(connection);
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
			DllNotFoundException ex = new(Resources.ExceptionClassAssemblyNotFound.Res(typeof(FirebirdClientFactory).Name));
			Diag.Dug(ex);
			throw ex;
		}

		var xmlStream = assembly.GetManifestResourceStream(ResourceName);

		if (cancellationToken.IsCancellationRequested)
			return Task.FromResult(new DataTable());

		if (xmlStream == null)
		{
			NullReferenceException ex = new(Resources.ExceptionResourceNotFound.Res(ResourceName));
			Diag.Dug(ex);
			throw ex;
		}

		var oldCulture = Thread.CurrentThread.CurrentCulture;

		parser?.SyncEnter(true);

		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			// ReadXml contains error: http://connect.microsoft.com/VisualStudio/feedback/Validation.aspx?FeedbackID=95116
			// that's the reason for temporarily changing culture
			ds.ReadXml(xmlStream);
		}
		catch
		{
			parser?.SyncExit();
			throw;
		}
		finally
		{
			Thread.CurrentThread.CurrentCulture = oldCulture;
		}

		if (cancellationToken.IsCancellationRequested)
		{
			parser?.SyncExit();
			return Task.FromResult(new DataTable());
		}

		DataRow[] collection;
		try
		{
			collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);
		}
		catch (Exception ex)
		{
			parser?.SyncExit();
			Diag.Dug(ex);
			throw;
		}

		if (cancellationToken.IsCancellationRequested)
		{
			parser?.SyncExit();
			return Task.FromResult(new DataTable());
		}

		if (collection.Length != 1)
		{
			parser?.SyncExit();
			NotSupportedException ex = new(Resources.ExceptionCollectionNotSupported.Res(schemaCollection));
			Diag.Dug(ex);
			throw ex;
		}

		if (restrictions != null && restrictions.Length != (int)collection[0]["NumberOfRestrictions"])
		{
			parser?.SyncExit();
			InvalidOperationException exbb =
				new(Resources.ExceptionRestrictionsNotEqualToSpecified.Res(restrictions.Length,
				(int)collection[0]["NumberOfRestrictions"]));
			Diag.Dug(exbb);
			throw exbb;
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
				parser?.SyncExit();
				NotSupportedException ex = new(Resources.ExceptionUnsupportedPopulationMechanism.Res(collection[0]["PopulationMechanism"].ToString()));
				Diag.Dug(ex);
				throw ex;
		}

		parser?.SyncExit();
		return task;
	}

	#endregion

	#region Private Methods

	private static DataTable PrepareCollection(FbConnection connection, string collectionName, string schemaCollection, string[] restrictions)
	{
		Tracer.Trace(typeof(DslSchemaFactory), "DslSchemaFactory.PrepareCollection", "collectionName: {0}, schemaCollection: {1}", collectionName, schemaCollection);

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
			case "TRIGGERDEPENDENCIES":
				ex = new(Resources.ExceptionInvalidRawMetadataCall.Res(collectionName));
				Diag.Dug(ex);
				throw ex;
			case "IDENTITYTRIGGERS":
			case "STANDARDTRIGGERS":
			case "SYSTEMTRIGGERS":
				ex = new(Resources.ExceptionInvalidPrebuiltMetadataCall.Res(collectionName));
				Diag.Dug(ex);
				throw ex;
			default:
				ex = new(Resources.ExceptionCollectionNotSupported.Res(collectionName));
				Diag.Dug(ex);
				throw ex;
		}

		return dslSchema.GetSchema(connection, schemaCollection, restrictions);
	}



	private static Task<DataTable> PrepareCollectionAsync(FbConnection connection, string collectionName, string schemaCollection, string[] restrictions, CancellationToken cancellationToken = default)
	{
		Tracer.Trace(typeof(DslSchemaFactory), "DslSchemaFactory.PrepareCollectionAsync", "collectionName: {0}, schemaCollection: {1}", collectionName, schemaCollection);

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
			case "TRIGGERDEPENDENCIES":
				ex = new(Resources.ExceptionInvalidRawMetadataCall.Res(collectionName));
				Diag.Dug(ex);
				throw ex;
			case "IDENTITYTRIGGERS":
			case "STANDARDTRIGGERS":
			case "SYSTEMTRIGGERS":
				ex = new(Resources.ExceptionInvalidPrebuiltMetadataCall.Res(collectionName));
				Diag.Dug(ex);
				throw ex;
			default:
				ex = new(Resources.ExceptionCollectionNotSupported.Res(collectionName));
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
			case "ProcedureParameters":
			case "Triggers":
			case "ViewColumns":
			case "TriggerColumns":
			case "TriggerDependencies":
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
