// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using BlackbirdDsl;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Model;
using BlackbirdSql.VisualStudio.Ddex.Model;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.LanguageServer.Client;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

// =========================================================================================================
//										TObjectSelectorRoot Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectSelector"/> enumerator interface for the root node
/// </summary>
// =========================================================================================================
public class TObjectSelectorRoot : AdoDotNetRootObjectSelector
{

	// ---------------------------------------------------------------------------------
	#region Variables - TObjectSelectorRoot
	// ---------------------------------------------------------------------------------

	// Sanity checker.
	private readonly bool _Ctor = false;
	FbConnection _Connection;
	FbConnectionStringBuilder _Csb;
	MonikerAgent _Moniker;


	#endregion Variables





	// =========================================================================================================
	#region Constructors / Destructors - TObjectSelectorRoot
	// =========================================================================================================


	public TObjectSelectorRoot() : base()
	{
		Tracer.Trace(GetType(), "TObjectSelectorRoot.TObjectSelectorRoot()");
	}

	public TObjectSelectorRoot(IVsDataConnection connection) : base()
	{
		Tracer.Trace(GetType(), "TObjectSelectorRoot(IVsDataConnection)", "NOTE THIS!!!");

		_Ctor = true;
		Site = connection;
		_Ctor = false;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Implementations - TObjectSelectorRoot
	// =========================================================================================================



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Enumerates the root node object.
	/// </summary>
	/// <param name="typeName"></param>
	/// <param name="restrictions"></param>
	/// <param name="properties"></param>
	/// <param name="parameters"></param>
	/// <returns>A data reader of the root object</returns>
	// ---------------------------------------------------------------------------------
	protected override IVsDataReader SelectObjects(string typeName, object[] restrictions, string[] properties, object[] parameters)
	{
		Tracer.Trace(GetType(), "SelectObjects()", "typeName: {0}.", typeName);

		try
		{
			if (typeName == null)
				throw new ArgumentNullException("typeName");

			if (parameters == null || parameters.Length != 1 || parameters[0] is not string)
				throw new ArgumentNullException(Resources.ExceptionInvalidParameters);

			if (Site == null)
				throw new InvalidOperationException(Resources.ExceptionSiteIsNull);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		int syncCardinal = 0;
		object lockedProviderObject = null;
		FbConnection connection = null;
		LinkageParser parser = null;
		IVsDataReader reader = null;

		try
		{
			lockedProviderObject = Site.GetLockedProviderObject();
			connection = lockedProviderObject as FbConnection;

			if (lockedProviderObject == null || connection == null)
			{
				NotImplementedException ex = new("Site.GetLockedProviderObject()");
				throw ex;
			}

			Site.EnsureConnected();
			parser = LinkageParser.GetInstance(connection);

			Tracer.Trace(GetType(), "SelectObjects()", parser == null ? "no parser to pause" : "making linker pause request");
			syncCardinal = parser != null ? parser.SyncEnter() : 0;

			_Connection = connection;
			_Csb = new FbConnectionStringBuilder(connection.ConnectionString);
			_Moniker = new(connection);

			DataTable schema = CreateSchema(typeName, parameters);

			_Moniker = null;
			_Csb = null;
			_Connection = null;

			reader = new AdoDotNetTableReader(schema);


		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			parser?.SyncExit(syncCardinal);

			// Only force create the parser 2nd time in.
			if (parser == null && connection != null)
				LinkageParser.EnsureInstance(connection, typeof(DslProviderSchemaFactory));

			if (lockedProviderObject != null)
				Site.UnlockProviderObject();
		}

		return reader;
	}


	#endregion Implementations





	// =========================================================================================================
	#region Methods - TObjectSelectorRoot
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Reads in the data source information schema and adds connection property
	/// descriptor columns to it as well as additional root node properties defined in
	/// <see cref="Common.Schema.CoreProperties.SourceInformationTypes"/>.
	/// </summary>
	/// <param name="connection"></param>
	/// <param name="parameters"></param>
	/// <returns>Thr root node ready DataSourceInformation schema.</returns>
	// ---------------------------------------------------------------------------------
	private DataTable CreateSchema(string typeName, object[] parameters)
	{
		Tracer.Trace(GetType(), "CreateSchema()", "typename: {0}", typeName);

		DataTable schema = new DataTable();

		Describer[] describers = typeName == "Database"
			? ModelPropertySet.ConnectionNodeDescribers
			: ModelPropertySet.RootNodeDescribers;


		foreach (Describer describer in describers)
			schema.Columns.Add(describer.Name, describer.DataType);

		schema.AcceptChanges();

		DataRow row = schema.NewRow();

		foreach (DataColumn column in schema.Columns)
			row[column.Ordinal] = RetrieveValue(column.ColumnName);


		schema.BeginLoadData();
		schema.Rows.Add(row);
		schema.EndLoadData();
		schema.AcceptChanges();

		/*
		string txt = $"Data row for {typeName}: ";

		foreach (DataColumn col in schema.Columns)
		{
			txt += col.ColumnName + ":" + (schema.Rows[0][col.Ordinal] == null ? "null" : (schema.Rows[0][col.Ordinal] == DBNull.Value ? "DBNull" : schema.Rows[0][col.Ordinal].ToString())) + ", ";
		}
		Diag.Trace(txt);
		*/

		if (parameters != null && parameters.Length == 1 && parameters[0] is DictionaryEntry entry)
		{
			if (entry.Value is object[] array)
			{
				IDictionary<string, object> mappings = GetMappings(array);
				ApplyMappings(schema, mappings);
			}
		}

		Tracer.Trace(GetType(), "CreateSchema()", "Schema type '{0}' loaded with {1} rows.", typeName, schema.Rows.Count);


		return schema;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves a value for a specified node column.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	private object RetrieveValue(string name)
	{
		string strval;
		object retval;

		try
		{
			switch (name)
			{
				case ModelConstants.C_KeyNodeDatasetKey:
					retval = _Moniker.DatasetKey;
					break;
				case ModelConstants.C_KeyNodeDataSource:
					retval = _Connection.DataSource;
					break;
				case ModelConstants.C_KeyNodePort:
					retval = _Csb.Port;
					break;
				case ModelConstants.C_KeyNodeServerType:
					retval = _Csb.ServerType;
					break;
				case ModelConstants.C_KeyNodeDatabase:
					retval = _Connection.Database;
					break;
				case ModelConstants.C_KeyNodeDisplayMember:
					retval = _Moniker.DisplayMember;
					break;
				case ModelConstants.C_KeyNodeUserId:
					retval = _Csb.UserID;
					break;
				case ModelConstants.C_KeyNodePassword:
					retval = _Csb.Password;
					break;
				case ModelConstants.C_KeyNodeRole:
					retval = _Csb.Role;
					break;
				case ModelConstants.C_KeyNodeCharset:
					retval = _Csb.Charset;
					break;
				case ModelConstants.C_KeyNodeDialect:
					retval = _Csb.Dialect;
					break;
				case ModelConstants.C_KeyNodeNoDbTriggers:
					retval = _Csb.NoDatabaseTriggers;
					break;
				case ModelConstants.C_KeyNodeMemoryUsage:
					retval = ModelConstants.C_DefaultNodeMemoryUsage;
					if ((_Connection.State & ConnectionState.Open) > 0)
					{
						FbDatabaseInfo info = new(_Connection);
						(strval, _) = ((long)info.GetCurrentMemory()).FmtByteSize();
						retval = strval;
					}
					break;
				case ModelConstants.C_KeyNodeActiveUsers:
					retval = ModelConstants.C_DefaultNodeActiveUsers;
					if ((_Connection.State & ConnectionState.Open) != 0)
					{
						FbDatabaseInfo info = new(_Connection);
						retval = info.GetActiveUsers().Count;
					}
					break;
				default:
					ArgumentException ex = new($"Invalid root/connection node property {name}.");
					Diag.Dug(ex);
					retval = null;
					break;
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"PropertyName: '{name}'");
			return null;
		}


		return retval;
	}


	#endregion Methods




	// =========================================================================================================
	#region Event handlers - TObjectSelectorRoot
	// =========================================================================================================


	protected override void OnSiteChanged(EventArgs e)
	{
		Tracer.Trace(GetType(), "OnSiteChanged()");

		if (!_Ctor)
			base.OnSiteChanged(e);
	}


	#endregion Event handlers


}
