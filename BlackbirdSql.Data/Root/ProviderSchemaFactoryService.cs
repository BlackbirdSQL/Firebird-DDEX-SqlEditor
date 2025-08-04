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
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Data.Model;
using BlackbirdSql.Data.Model.Schema;
using BlackbirdSql.Data.Properties;
using BlackbirdSql.Sys.Interfaces;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Threading;



namespace BlackbirdSql.Data;


internal sealed class ProviderSchemaFactoryService : SBsNativeProviderSchemaFactory, IBsNativeProviderSchemaFactory
{
	#region Static Members

	private static readonly string ResourceName = LibraryData.C_SchemaMetaDataXml;

	#endregion

	#region Constructors

	private ProviderSchemaFactoryService()
	{
		// Evs.Trace(GetType(), "DslProviderSchemaFactory.DslProviderSchemaFactory");
	}

	internal static IBsNativeProviderSchemaFactory EnsureInstance() => _Instance ??= new ProviderSchemaFactoryService();


	#endregion


	#region Fields

	internal static ProviderSchemaFactoryService _Instance = null;

	#endregion Fields


	#region Methods




	// Schema factory to handle custom collections
	DataTable IBsNativeProviderSchemaFactory.GetSchema(IDbConnection connection, string collectionName, string[] restrictions)
	{
		return GetSchema(connection, collectionName, restrictions);
	}

	// Schema factory to handle custom collections
	internal static DataTable GetSchema(IDbConnection connection, string collectionName, string[] restrictions)
	{
		// Evs.Trace(typeof(ProviderSchemaFactoryService), nameof(GetSchema), "collectionName: {0}", collectionName);

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
			case "Roles":
			case "Tables":
			case "Triggers":
			case "Users":
			case "ViewColumns":
				schemaCollection = collectionName;
				break;
			case "RawGenerators":
				schemaCollection = "Generators";
				break;
			case "TriggerColumns":
				schemaCollection = "Columns";
				break;
			case "RawTriggerDependencies":
			case "RawTriggers":
			case "TriggerDependencies":
			case "IdentityTriggers":
			case "StandardTriggers":
			case "SystemTriggers":
				schemaCollection = "Triggers";
				break;
			default:
				return ((FbConnection)connection).GetSchema(collectionName, restrictions);
		}


		switch (collectionName)
		{
			case "Generators":
				return LinkageParser.EnsureLoaded(connection).GetSequenceSchema(restrictions);
			case "Triggers":
				return LinkageParser.EnsureLoaded(connection).GetTriggerSchema(restrictions, -1, -1);
			case "StandardTriggers":
				return LinkageParser.EnsureLoaded(connection.ConnectionString, restrictions).GetTriggerSchema(restrictions, 0, 0);
			case "IdentityTriggers":
				return LinkageParser.EnsureLoaded(connection.ConnectionString, restrictions).GetTriggerSchema(restrictions, 0, 1);
			case "SystemTriggers":
				return LinkageParser.EnsureLoaded(connection).GetTriggerSchema(restrictions, 1, -1);
			default:
				break;
		}


		string filter = "CollectionName = '{0}'".Fmt(schemaCollection);
		DataSet ds = new ();

		Assembly assembly = typeof(ProviderSchemaFactoryService).Assembly;

		if (assembly == null)
		{
			DllNotFoundException ex = new(typeof(ProviderSchemaFactoryService).Name + " class assembly not found");
			Diag.Ex(ex);
			throw ex;
		}

		// Evs.Trace(typeof(DslProviderSchemaFactory), nameof(GetSchema), parser == null ? "no parser to pause" : "making linker pause request");

		Stream xmlStream = assembly.GetManifestResourceStream(ResourceName);

		if (xmlStream == null)
		{
			NullReferenceException ex = new("Resource not found: " + ResourceName);
			Diag.Ex(ex);
			throw ex;
		}

		CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;

		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			// ReadXml contains error: http://connect.microsoft.com/VisualStudio/feedback/Validation.aspx?FeedbackID=95116
			// that's the reason for temporarily changing culture
			ds.ReadXml(xmlStream);
		}
		catch
		{
			throw;
		}
		finally
		{
			Thread.CurrentThread.CurrentCulture = oldCulture;
		}

		DataRow[] collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);

		if (collection.Length != 1)
		{
			NotSupportedException ex = new("Unsupported collection name " + schemaCollection);
			Diag.Ex(ex);
			throw ex;
		}

		if (restrictions != null && restrictions.Length > (int)collection[0]["NumberOfRestrictions"])
		{
			InvalidOperationException ex = new("The number of specified restrictions is not valid.");
			Diag.Ex(ex);
			throw ex;
		}

		if (ds.Tables[DbMetaDataCollectionNames.Restrictions].Select(filter).Length != (int)collection[0]["NumberOfRestrictions"])
		{
			InvalidOperationException ex = new("Incorrect restriction definition.");
			Diag.Ex(ex);
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
				NotSupportedException ex = new("Unsupported population mechanism");
				Diag.Ex(ex);
				throw ex;
		}

		return schema;
	}


	// Schema factory to handle custom collections asynchronously
	async Task<DataTable> IBsNativeProviderSchemaFactory.GetSchemaAsync(IDbConnection connection,
		string collectionName, string[] restrictions, CancellationToken cancelToken)
	{
		return await GetSchemaAsync(connection, collectionName, restrictions, cancelToken);
	}



	internal static async Task<DataTable> GetSchemaAsync(IDbConnection connection, string collectionName, string[] restrictions,
		CancellationToken cancelToken)
	{
		// Evs.Trace(typeof(ProviderSchemaFactoryService), "DslProviderSchemaFactory.GetSchemaAsync", "collectionName: {0}", collectionName);

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
			case "Roles":
			case "Triggers":
			case "Tables":
			case "Users":
			case "ViewColumns":
				schemaCollection = collectionName;
				break;
			case "RawGenerators":
				schemaCollection = "Generators";
				break;
			case "TriggerColumns":
				schemaCollection = "Columns";
				break;
			case "TriggerDependencies":
			case "IdentityTriggers":
			case "RawTriggerDependencies":
			case "RawTriggers":
			case "StandardTriggers":
			case "SystemTriggers":
				schemaCollection = "Triggers";
				break;
			default:
				try
				{
					return await ((FbConnection)connection).GetSchemaAsync(collectionName, restrictions, cancelToken);
				}
				catch (Exception)
				{
					if (cancelToken.Cancelled())
						return new DataTable();
					throw;
				}
		}

		if (cancelToken.Cancelled())
			return new DataTable();


		switch (collectionName)
		{
			case "Generators":
				return LinkageParser.EnsureLoaded(connection).GetSequenceSchema(restrictions);
			case "Triggers":
				return LinkageParser.EnsureLoaded(connection).GetTriggerSchema(restrictions, -1, -1);
			case "StandardTriggers":
				return LinkageParser.EnsureLoaded(connection.ConnectionString, restrictions).GetTriggerSchema(restrictions, 0, 0);
			case "IdentityTriggers":
				return LinkageParser.EnsureLoaded(connection.ConnectionString, restrictions).GetTriggerSchema(restrictions, 0, 1);
			case "SystemTriggers":
				return LinkageParser.EnsureLoaded(connection).GetTriggerSchema(restrictions, 1, -1);
			default:
				break;
		}


		string filter = "CollectionName = '{0}'".Fmt(schemaCollection);
		DataSet ds = new ();

		Assembly assembly = typeof(ProviderSchemaFactoryService).Assembly;

		if (assembly == null)
		{
			DllNotFoundException ex = new(Resources.ExceptionClassAssemblyNotFound.Fmt(typeof(ProviderSchemaFactoryService).Name));
			Diag.Ex(ex);
			throw ex;
		}

		Stream xmlStream = assembly.GetManifestResourceStream(ResourceName);

		if (cancelToken.Cancelled())
			return new DataTable();

		if (xmlStream == null)
		{
			NullReferenceException ex = new(Resources.ExceptionResourceNotFound.Fmt(ResourceName));
			Diag.Ex(ex);
			throw ex;
		}

		CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;

		// Evs.Trace(typeof(DslProviderSchemaFactory), nameof(GetSchemaAsync), parser == null ? "no parser to pause" : "making linker pause request");

		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			// ReadXml contains error: http://connect.microsoft.com/VisualStudio/feedback/Validation.aspx?FeedbackID=95116
			// that's the reason for temporarily changing culture
			ds.ReadXml(xmlStream);
		}
		catch
		{
			throw;
		}
		finally
		{
			Thread.CurrentThread.CurrentCulture = oldCulture;
		}

		if (cancelToken.Cancelled())
		{
			return new DataTable();
		}

		DataRow[] collection;
		try
		{
			collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}

		if (cancelToken.Cancelled())
		{
			return new DataTable();
		}

		if (collection.Length != 1)
		{
			NotSupportedException ex = new(Resources.ExceptionCollectionNotSupported.Fmt(schemaCollection));
			Diag.Ex(ex);
			throw ex;
		}

		if (restrictions != null && restrictions.Length != (int)collection[0]["NumberOfRestrictions"])
		{
			InvalidOperationException exbb =
				new(Resources.ExceptionRestrictionsNotEqualToSpecified.Fmt(restrictions.Length,
				(int)collection[0]["NumberOfRestrictions"]));
			Diag.Ex(exbb);
			throw exbb;
		}


		DataTable table;

		switch (collection[0]["PopulationMechanism"].ToString())
		{
			case "PrepareCollection":
				table = await PrepareCollectionAsync(connection, collectionName, schemaCollection, restrictions, cancelToken);
				break;
			case "DataTable":
				table = ds.Tables[collection[0]["PopulationString"].ToString()].Copy();
				break;
			case "SQLCommand":
				table = await SqlCommandSchemaAsync(connection, collectionName, restrictions, cancelToken);
				break;
			default:
				NotSupportedException ex = new(Resources.ExceptionUnsupportedPopulationMechanism.Fmt(collection[0]["PopulationMechanism"].ToString()));
				Diag.Ex(ex);
				throw ex;
		}

		return table;
	}

	#endregion

	#region Private Methods

	private static DataTable PrepareCollection(IDbConnection connection, string collectionName, string schemaCollection,
		string[] restrictions)
	{
		// Evs.Trace(typeof(DslProviderSchemaFactory), "DslProviderSchemaFactory.PrepareCollection", "collectionName: {0}, schemaCollection: {1}", collectionName, schemaCollection);

		AbstractDslSchema dslSchema;
		NotSupportedException ex;


		switch (collectionName.ToUpperInvariant())
		{
			case "COLUMNS":
				dslSchema = new DslColumns();
				break;
			case "FOREIGNKEYCOLUMNS":
				dslSchema = new DslForeignKeyColumns();
				break;
			case "FOREIGNKEYS":
				dslSchema = new DslForeignKeys();
				break;
			case "FUNCTIONARGUMENTS":
				dslSchema = new DslFunctionArguments();
				break;
			case "FUNCTIONS":
				dslSchema = new DslFunctions();
				break;
			case "INDEXCOLUMNS":
				dslSchema = new DslIndexColumns();
				break;
			case "INDEXES":
				dslSchema = new DslIndexes();
				break;
			case "PROCEDURES":
				dslSchema = new DslProcedures();
				break;
			case "PROCEDUREPARAMETERS":
				dslSchema = new DslProcedureParameters();
				break;
			case "RAWGENERATORS":
				dslSchema = new DslRawGenerators();
				break;
			case "RAWTRIGGERS":
				dslSchema = new DslRawTriggers();
				break;
			case "RAWTRIGGERDEPENDENCIES":
				dslSchema = new DslRawTriggerDependencies();
				break;
			case "ROLES":
				dslSchema = new DslRoles();
				break;
			case "TABLES":
				dslSchema = new DslTables();
				break;
			case "TRIGGERCOLUMNS":
				dslSchema = new DslTriggerColumns();
				break;
			case "USERS":
				dslSchema = new DslUsers();
				break;
			case "VIEWCOLUMNS":
				dslSchema = new DslViewColumns();
				break;
			case "GENERATORS":
			case "TRIGGERDEPENDENCIES":
			case "TRIGGERS":
			case "IDENTITYTRIGGERS":
			case "STANDARDTRIGGERS":
			case "SYSTEMTRIGGERS":
				ex = new(Resources.ExceptionInvalidPrebuiltMetadataCall.Fmt(collectionName));
				Diag.Ex(ex);
				throw ex;
			default:
				ex = new(Resources.ExceptionCollectionNotSupported.Fmt(collectionName));
				Diag.Ex(ex);
				throw ex;
		}

		return dslSchema.GetSchema(connection, schemaCollection, restrictions);
	}



	private static async Task<DataTable> PrepareCollectionAsync(IDbConnection connection, string collectionName, string schemaCollection, string[] restrictions, CancellationToken cancelToken)
	{
		// Evs.Trace(typeof(DslProviderSchemaFactory), "DslProviderSchemaFactory.PrepareCollectionAsync", "collectionName: {0}, schemaCollection: {1}", collectionName, schemaCollection);

		AbstractDslSchema dslSchema;
		NotSupportedException ex;


		switch (collectionName.ToUpperInvariant())
		{
			case "COLUMNS":
				dslSchema = new DslColumns();
				break;
			case "FOREIGNKEYCOLUMNS":
				dslSchema = new DslForeignKeyColumns();
				break;
			case "FOREIGNKEYS":
				dslSchema = new DslForeignKeys();
				break;
			case "FUNCTIONARGUMENTS":
				dslSchema = new DslFunctionArguments();
				break;
			case "FUNCTIONS":
				dslSchema = new DslFunctions();
				break;
			case "INDEXCOLUMNS":
				dslSchema = new DslIndexColumns();
				break;
			case "INDEXES":
				dslSchema = new DslIndexes();
				break;
			case "PROCEDURES":
				dslSchema = new DslProcedures();
				break;
			case "PROCEDUREPARAMETERS":
				dslSchema = new DslProcedureParameters();
				break;
			case "RAWGENERATORS":
				dslSchema = new DslRawGenerators();
				break;
			case "RAWTRIGGERS":
				dslSchema = new DslRawTriggers();
				break;
			case "RAWTRIGGERDEPENDENCIES":
				dslSchema = new DslRawTriggerDependencies();
				break;
			case "ROLES":
				dslSchema = new DslRoles();
				break;
			case "TABLES":
				dslSchema = new DslTables();
				break;
			case "TRIGGERCOLUMNS":
				dslSchema = new DslTriggerColumns();
				break;
			case "USERS":
				dslSchema = new DslUsers();
				break;
			case "VIEWCOLUMNS":
				dslSchema = new DslViewColumns();
				break;
			case "GENERATORS":
			case "TRIGGERDEPENDENCIES":
			case "TRIGGERS":
			case "IDENTITYTRIGGERS":
			case "STANDARDTRIGGERS":
			case "SYSTEMTRIGGERS":
				ex = new(Resources.ExceptionInvalidPrebuiltMetadataCall.Fmt(collectionName));
				Diag.Ex(ex);
				throw ex;
			default:
				ex = new(Resources.ExceptionCollectionNotSupported.Fmt(collectionName));
				Diag.Ex(ex);
				throw ex;
		}

		return await dslSchema.GetSchemaAsync(connection, schemaCollection, restrictions, cancelToken);
	}



	private static DataTable SqlCommandSchema(IDbConnection connection, string collectionName, string[] restrictions)
	{
		NotImplementedException exbb = new();
		Diag.Ex(exbb);
		throw exbb;
	}
	private static async Task<DataTable> SqlCommandSchemaAsync(IDbConnection connection, string collectionName, string[] restrictions, CancellationToken cancelToken = default)
	{
		await Cmd.AwaitableAsync();

		NotImplementedException exbb = new();
		Diag.Ex(exbb);
		throw exbb;
	}


	#endregion
}
