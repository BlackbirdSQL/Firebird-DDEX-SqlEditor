
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using BlackbirdSql.Common.Properties;
using Microsoft.VisualStudio.Data.Framework;


namespace BlackbirdSql.Common.Providers;

internal static class AdoDotNetProvider
{
	public static T CreateObject<T>(string invariantName) where T : class
	{
		if (invariantName == null)
		{
			DataException ex = new(Resources.AdoDotNetProvider_MissingInvariantName);
			Diag.Dug(ex);
			throw ex;
		}

		DbProviderFactory providerFactory = DependencyAcquisition.GetProviderFactory(invariantName);
		if (providerFactory == null)
		{
			DataException ex = new(string.Format(null, Resources.AdoDotNetProvider_NotRegistered, invariantName));
			Diag.Dug(ex);
			throw ex;
		}

		T val = null;
		if (typeof(T) == typeof(DbCommandBuilder))
		{
			val = providerFactory.CreateCommandBuilder() as T;
		}

		if (typeof(T) == typeof(DbConnection))
		{
			val = providerFactory.CreateConnection() as T;
		}

		if (typeof(T) == typeof(DbConnectionStringBuilder))
		{
			val = providerFactory.CreateConnectionStringBuilder() as T;
		}

		if (typeof(T) == typeof(DbParameter))
		{
			val = providerFactory.CreateParameter() as T;
		}

		if (val == null)
		{
			DataException ex = new(string.Format(null, Resources.AdoDotNetProvider_ObjectNotImplemented, invariantName, typeof(T).Name));
			Diag.Dug(ex);
			throw ex;
		}

		return val;
	}

	public static void ApplyMappings(DataTable dataTable, IDictionary<string, object> mappings)
	{
		if (dataTable == null)
		{
			ArgumentNullException ex = new("dataTable");
			Diag.Dug(ex);
			throw ex;
		}

		if (mappings == null)
		{
			return;
		}

		foreach (KeyValuePair<string, object> mapping in mappings)
		{
			DataColumn dataColumn = dataTable.Columns[mapping.Key];
			if (dataColumn != null)
			{
				continue;
			}

			if (mapping.Value is string text)
			{
				dataColumn = dataTable.Columns[text];
			}
			else
			{
				int num = (int)mapping.Value;
				if (num >= 0 && num < dataTable.Columns.Count)
				{
					dataColumn = dataTable.Columns[num];
				}
			}

			if (dataColumn != null)
			{
				dataTable.Columns.Add(new DataColumn(mapping.Key, dataColumn.DataType, "[" + dataColumn.ColumnName.Replace("]", "]]") + "]"));
			}
		}
	}
}
