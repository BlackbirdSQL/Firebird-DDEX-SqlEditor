// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model;
using BlackbirdSql.VisualStudio.Ddex.Extensions;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


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


	#endregion Variables





	// =========================================================================================================
	#region Constructors / Destructors - TObjectSelectorRoot
	// =========================================================================================================


	public TObjectSelectorRoot() : base()
	{
		Tracer.Trace(GetType(), "TObjectSelectorRoot.TObjectSelectorRoot");
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
		Tracer.Trace(GetType(), "TObjectSelectorRoot.SelectObjects", "typeName: {0}", typeName);

		try
		{
			if (typeName == null || typeName.Length > 0)
				throw new ArgumentNullException("typeName");

			if (Site == null)
				throw new InvalidOperationException(Resources.ExceptionSiteIsNull);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}


		LinkageParser parser = LinkageParser.Instance(Site, true);

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
				parser?.SyncEnter(true);

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
	/// <see cref="Common.Schema.CoreProperties.SourceInformationTypes"/>.
	/// </summary>
	/// <param name="connection"></param>
	/// <param name="parameters"></param>
	/// <returns>Thr root node ready DataSourceInformation schema.</returns>
	// ---------------------------------------------------------------------------------
	private DataTable GetRootSchema(DbConnection connection, object[] parameters)
	{
		Tracer.Trace(GetType(), "TObjectSelectorRoot.GetRootSchema");

		Site.EnsureConnected();


		// We use TSourceInformation for the dual purpose of the Root IVsDataObjectSelector
		// schema and IVsDataSourceInformation.
		// We're counting on Source info for a Site being required first otherwise could bomb.
		TSourceInformation sourceInformation = TSourceInformation.Instance(Site);
		DataTable schema = sourceInformation.SourceInformation;

		DataRow row = schema.Rows[0];
		object value;

		schema.BeginLoadData();

		foreach (DataColumn column in schema.Columns)
		{
			value = row[column.Ordinal];

			if (value != DBNull.Value && (column.GetType() != typeof(int) || (int)value != int.MinValue))
			{
				continue;
			}

			value = sourceInformation.RetrieveSourceInformationValue(column.ColumnName);

			if (value == null || value == DBNull.Value)
				continue;

			row[column.Ordinal] = value;
		}

		schema.EndLoadData();


		DataTable rootSchema = schema.Copy();

		// Convert SourceInformation columns to root columns
		foreach (KeyValuePair<string, string> pair in ModelPropertySet.RootTranslations)
			rootSchema.Columns[pair.Key].ColumnName = pair.Value;

		rootSchema.AcceptChanges();

		/*
		string txt = "Metadata: ";
		foreach (DataColumn col in rootSchema.Columns)
		{
			txt += col.ColumnName + ":" + (rootSchema.Rows[0][col.Ordinal] == null ? "null" : (rootSchema.Rows[0][col.Ordinal] == DBNull.Value ? "DBNull" : rootSchema.Rows[0][col.Ordinal].ToString())) + ", ";
		}
		*/
		// Diag.Trace(txt);

		if (parameters != null && parameters.Length == 1 && parameters[0] is DictionaryEntry entry)
		{
			if (entry.Value is object[] array)
			{
				IDictionary<string, object> mappings = GetMappings(array);
				ApplyMappings(rootSchema, mappings);
			}
		}




		return rootSchema;
	}





	#endregion Methods

}
