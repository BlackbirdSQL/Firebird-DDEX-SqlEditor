// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Model;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										VxbObjectSelectorRoot Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectSelector"/> enumerator interface for the root node
/// </summary>
// =========================================================================================================
public class VxbObjectSelectorRoot : DataObjectSelector
{

	// ---------------------------------------------------------------------------------
	#region Fields - VxbObjectSelectorRoot
	// ---------------------------------------------------------------------------------

	private Csb _Csa = null;



	#endregion Fields





	// =========================================================================================================
	#region Constructors / Destructors - VxbObjectSelectorRoot
	// =========================================================================================================


	public VxbObjectSelectorRoot() : base()
	{
		// Evs.Trace(typeof(VxbObjectSelectorRoot), ".ctor");
	}


	public VxbObjectSelectorRoot(IVsDataConnection connection) : base(connection)
	{
		// Evs.Trace(typeof(VxbObjectSelectorRoot), ".ctor(IVsDataConnection)");
	}

	#endregion Constructors / Destructors





	// =================================================================================
	#region Property Accessors - VxbObjectSelectorRoot
	// =================================================================================


	#endregion Property Accessors





	// =========================================================================================================
	#region Implementations - VxbObjectSelectorRoot
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
		Evs.Trace(GetType(), nameof(SelectObjects), $"typeName: {typeName}.");


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

		// Evs.Trace(GetType(), nameof(SelectObjects), "TYPE IVsDataConnection: {0}.", Site.GetType().FullName);

		IVsDataExplorerConnection root = null;
		object lockedProviderObject = null;
		IVsDataReader reader = null;
		bool connectionCreated = false;
		DbConnection connection = null;

		try
		{
			if (RctManager.ShutdownState)
				return null;

			Site.EnsureConnected();

			lockedProviderObject = Site.GetLockedProviderObject();
			if (lockedProviderObject == null)
				throw new NotImplementedException("Site.GetLockedProviderObject()");

			connection = NativeDb.CastToNativeConnection(lockedProviderObject);


			// VS glitch. Null if ado has picked up a project data model firebird assembly.
			if (connection == null)
			{
				connectionCreated = true;
				connection = (DbConnection)NativeDb.CreateDbConnection(Site.DecryptedConnectionString());
				connection.Open();
			}

			// Evs.Trace(GetType(), nameof(SelectObjects), "Site type: {0}", Site.GetType().FullName);

			if (_Csa == null || _Csa.IsInvalidated)
			{

				_Csa = RctManager.EnsureVolatileInstance((IDbConnection)lockedProviderObject);
			}

			DataTable schema = CreateSchema(connection, typeName, parameters);


			reader = new AdoDotNetTableReader(schema);
		}
		catch (DbException exf)
		{
			Evs.Warning(GetType(), "SelectObjects", $"{NativeDb.DbEngineName} error: {exf.Message}.");

			root ??= Site.ExplorerConnection();
			root.DisposeLinkageParser(false);
			if (lockedProviderObject != null)
			{
				lockedProviderObject = null;
				Site.UnlockProviderObject();
			}
			Site.Close();

			reader = new AdoDotNetTableReader(new DataTable());
		}
		catch (Exception ex)
		{
			lockedProviderObject = null;
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			// Only force create the parser 2nd time in.
			if (lockedProviderObject != null)
				Site.UnlockProviderObject();

			if (connectionCreated)
			{
				if (connection.State == ConnectionState.Open)
					connection.Close();
				connection.Dispose();
			}
		}

		return reader;
	}


	#endregion Implementations





	// =========================================================================================================
	#region Methods - VxbObjectSelectorRoot
	// =========================================================================================================


	public static void ApplyMappings(DataTable dataTable, IDictionary<string, object> mappings)
	{
		if (dataTable == null)
			throw new ArgumentNullException("dataTable");

		if (mappings == null)
			return;


		foreach (KeyValuePair<string, object> mapping in mappings)
		{
			DataColumn dataColumn = dataTable.Columns[mapping.Key];

			if (dataColumn != null)
				continue;

			if (mapping.Value is string name)
			{
				dataColumn = dataTable.Columns[name];
			}
			else
			{
				int num = (int)mapping.Value;

				if (num >= 0 && num < dataTable.Columns.Count)
					dataColumn = dataTable.Columns[num];
			}

			if (dataColumn != null)
				dataTable.Columns.Add(new DataColumn(mapping.Key, dataColumn.DataType, "[" + dataColumn.ColumnName.Replace("]", "]]") + "]"));
		}
	}

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
	private DataTable CreateSchema(DbConnection connection, string typeName, object[] parameters)
	{
		// Evs.Trace(GetType(), nameof(CreateSchema), "typename: {0}", typeName);

		DataTable schema = new DataTable();

		if (string.IsNullOrWhiteSpace(typeName))
			typeName = "Root";

		Describer[] describers = typeName == "Database"
			? [.. Csb.Describers.DescriberKeys]
			: [Csb.Describers[CoreConstants.C_KeyExDatasetKey], Csb.Describers[CoreConstants.C_KeyExConnectionKey]];


		foreach (Describer describer in describers)
			schema.Columns.Add(describer.Name, describer.DataType);

		schema.AcceptChanges();

		schema.BeginLoadData();

		DataRow row = schema.NewRow();

		foreach (DataColumn column in schema.Columns)
			row[column.ColumnName] = RetrieveValue(connection, column.ColumnName);


		schema.Rows.Add(row);

		schema.EndLoadData();
		schema.AcceptChanges();

		/*
		string str = $"Data row for {typeName}: ";

		foreach (DataColumn col in schema.Columns)
		{
			str += col.ColumnName + ":" + (schema.Rows[0][col.Ordinal] == null ? "null" : (schema.Rows[0][col.Ordinal] == DBNull.Value ? "DBNull" : schema.Rows[0][col.Ordinal].ToString())) + ", ";
		}
		*/
		// Evs.Trace(GetType(), nameof(CreateSchema), "{0}", str);

		// Not used.
		if (parameters != null && parameters.Length > 1 && parameters[1] is DictionaryEntry entry
			&& entry.Value is object[] array)
		{
			IDictionary<string, object> mappings = GetMappings(array);
			ApplyMappings(schema, mappings);
		}
		

		// Evs.Trace(GetType(), nameof(CreateSchema), "Schema type '{0}' loaded with {1} rows.", typeName, schema.Rows.Count);


		return schema;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves a value for a specified node column.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	private object RetrieveValue(DbConnection connection, string name, bool retrying = false)
	{
		object retval;
		string strval;
		object errval = DBNull.Value;

		if (retrying)
		{
			// Evs.Trace(GetType(), nameof(RetrieveValue), "Retrying");
			try
			{
				connection.Close();
				connection.Open();
			}
			catch (Exception ex)
			{
				Diag.Expected(ex, $"\nConnection string: {connection?.ConnectionString}");
			}
		}


		try
		{
			switch (name)
			{
				case CoreConstants.C_KeyExDatasetKey:
					retval = _Csa.DatasetKey;
					break;
				case CoreConstants.C_KeyExConnectionKey:
					retval = _Csa.ConnectionKey;
					break;
				case CoreConstants.C_KeyExConnectionSource:
					retval = EnConnectionSource.ServerExplorer;
					break;
				case SysConstants.C_KeyDataSource:
					retval = connection.DataSource;
					break;
				case CoreConstants.C_KeyExDataset:
					retval = _Csa.Dataset;
					break;
				case SysConstants.C_KeyDatabase:
					retval = connection.Database;
					break;
				case SysConstants.C_KeyExDatasetName:
					retval = _Csa.DisplayDatasetName;
					break;
				case CoreConstants.C_KeyExAdornedQualifiedName:
					retval = _Csa.AdornedQualifiedName;
					break;
				case CoreConstants.C_KeyExAdornedQualifiedTitle:
					retval = _Csa.AdornedQualifiedTitle;
					break;
				case CoreConstants.C_KeyExAdornedDisplayName:
					retval = _Csa.AdornedDisplayName;
					break;
				case CoreConstants.C_KeyExClientVersion:
					retval = NativeDb.ClientVersion;
					break;
				case CoreConstants.C_KeyExMemoryUsage:
					errval = -1;
					retval = CoreConstants.C_DefaultExMemoryUsage;
					if ((connection.State & ConnectionState.Open) > 0)
					{
						NativeDatabaseInfoProxy info = new(connection);
						(strval, _) = ((long)info.GetCurrentMemory()).FmtByteSize();
						retval = strval;
					}
					break;
				case CoreConstants.C_KeyExActiveUsers:
					errval = -1;
					retval = CoreConstants.C_DefaultExActiveUsers;
					if ((connection.State & ConnectionState.Open) != 0)
					{
						NativeDatabaseInfoProxy info = new(connection);
						retval = info.GetActiveUsers().Count;
					}
					break;
				default:
					Describer describer = Csb.Describers[name];
					if (!_Csa.ContainsKey(describer.Name))
						retval = describer.DefaultValue ?? DBNull.Value;
					else if (describer.DataType == typeof(int))
						retval = Convert.ToInt32(_Csa[describer.Name]);
					else
						retval = _Csa[describer.Name];
					// Evs.Trace(GetType(), nameof(RetrieveValue), "Name: {0}, CsbName: {1}, retval: {2}, ContainsKey(CsbName): {3}, _Csa[CsbName]: {4}.", name, describer.Name, retval, _Csa.ContainsKey(describer.Name), _Csa.ContainsKey(describer.Name) ? _Csa[describer.Name] : "NoExist");

					break;
			}
		}
		catch (Exception ex)
		{
			if (!retrying)
				return RetrieveValue(connection, name, true);

			Diag.Dug(ex, $"Error retrieving PropertyName: '{name}'");
			return errval;
		}

		retval ??= DBNull.Value;

		return retval;
	}


	#endregion Methods




	// =========================================================================================================
	#region Event handlers - VxbObjectSelectorRoot
	// =========================================================================================================


	protected override void OnSiteChanged(EventArgs e)
	{
		// Evs.Trace(GetType(), nameof(OnSiteChanged));

		base.OnSiteChanged(e);
	}


	#endregion Event handlers


}
