// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;
using BlackbirdSql.VisualStudio.Ddex.Extensions;
using BlackbirdSql.VisualStudio.Ddex.Schema;
using Microsoft.VisualStudio.OLE.Interop;

namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TObjectSelectorRoot Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectSelector"/> enumerator interface for the root node
/// </summary>
// =========================================================================================================
class TObjectSelectorRoot : AdoDotNetRootObjectSelector
{

	// ---------------------------------------------------------------------------------
	#region Variables - TObjectSelectorRoot
	// ---------------------------------------------------------------------------------


	#endregion Variables





	// =========================================================================================================
	#region Constructors / Destructors - TObjectSelectorRoot
	// =========================================================================================================


	public TObjectSelectorRoot() : base()
	{
		// Diag.Trace();
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
		// Diag.Trace(typeName);

		try
		{
			if (typeName == null || typeName.Length > 0)
				throw new ArgumentNullException("typeName");

			if (Site == null)
				throw new InvalidOperationException("Site is null");
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}


		LinkageParser parser = LinkageParser.Instance(Site);

		IVsDataReader reader;

		try
		{
			object lockedProviderObject = Site.GetLockedProviderObject();


			if (lockedProviderObject == null || lockedProviderObject is not DbConnection connection)
			{
				NotImplementedException ex = new("Site.GetLockedProviderObject()");
				Diag.Dug(ex);
				throw ex;
			}

			DataTable schema;


			try
			{
				parser?.SyncEnter();

				schema = GetRootSchema(connection, parameters);

				reader = new AdoDotNetTableReader(schema);


			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;

			}
			finally
			{
				parser?.SyncExit();
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
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
	/// Reads in the data source information schema and adds connection property descriptor
	/// columns to it as well as additional root node properties defined in
	/// <see cref="ConnectionResources.RootTypes"/>.
	/// </summary>
	/// <param name="connection"></param>
	/// <param name="parameters"></param>
	/// <returns>Thr root node ready DataSourceInformation schema.</returns>
	// ---------------------------------------------------------------------------------
	private DataTable GetRootSchema(DbConnection connection, object[] parameters)
	{
		Site.EnsureConnected();

		// Diag.Trace();

		DataTable schema = TSourceInformation.CreateSourceInformationSchema(Site);

		DataRow row = schema.Rows[0];
		object value;

		foreach (DataColumn column in schema.Columns)
		{
			value = row[column.Ordinal];

			if (value != null && value != DBNull.Value && (column.GetType() != typeof(int) || (int)value != int.MinValue))
			{
				continue;
			}

			if (!ConnectionResources.RootSynonyms.TryGetValue(column.ColumnName, out string param))
				continue;

			value = TSourceInformation.RetrieveValue(Site, schema, param);
			value ??= DBNull.Value;

			row[column.Ordinal] = value;
		}

		if (parameters != null && parameters.Length == 1 && parameters[0] is DictionaryEntry entry)
		{
			if (entry.Value is object[] array)
			{
				IDictionary<string, object> mappings = GetMappings(array);
				ApplyMappings(schema, mappings);
			}
		}


		return schema;
	}





	#endregion Methods

}
