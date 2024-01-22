// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Reflection;


namespace BlackbirdSql.Core.Ctl.Extensions;

/// <summary>
/// Static class for adding a FirebirdClient as a DotNet system DBProviderFactory into assembly cache on the fly
/// </summary>
public static class DbProviderFactoriesEx
{

	/// <summary>
	/// Adds the Firebird client FirebirdSql.Data.FirebirdClient as a DotNet DBProviderFactory
	/// in the local assembly cache to avoid the gac using DbProviderFactories directly
	/// </summary>

	public static bool AddAssemblyToCache(Type factoryClass, string factoryName, string factoryDescription)
	{
		/*
		 * Spreading this code out for brevity
		*/

		/*
		 * 
		 * DbProviderFactories.GetFactoryClasses()
		 * ---------------------------------------
		 * 		 
		 * Returns a System.Data.DataTable containing System.Data.DataRow objects that contain
		 * the following data of DbProviderFactories. 
		 * Ordinal	Column name				Description
		 * -------	-----------				-----------
		 * 0		Name					Human-readable name for the data provider.
		 * 1		Description				Human-readable description of the data provider.
		 * 2		InvariantName			Name that can be used programmatically to refer to the data provider.
		 *									eg. FirebirdSql.Data.FirebirdClient
		 * 3		AssemblyQualifiedName	Fully qualified name of the factory class, which contains enough
		 *									information to instantiate the object.
		 *									eg. FirebirdSql.Data.FirebirdClient.FirebirdClientFactory,
		 *										FirebirdSql.Data.FirebirdClient, Version=9.1.1.0, Culture=neutral,
		 *										PublicKeyToken=3750abcc3150b00c
		*/
		DataTable table = DbProviderFactories.GetFactoryClasses();

		string invariantName = factoryClass.Assembly.GetName().Name;

		if ((_ = table.Rows.Find(invariantName)) != null)
		{
			/*
			// Diag.Trace(
				String.Format("'DbProviderFactories' section (Columns:{0}) aready contains [{1}:{2}:{3}:{4}] as [{5}:{6}:{7}:{8}]",
				table.Columns.Count, invariantName, factoryName, factoryDescription, factoryClass.AssemblyQualifiedName,
				row[2].ToString(), row[0].ToString(), row[1].ToString(), row[3].ToString()));
			*/

			table.Dispose();

			return false;
		}

		// Diag.Trace(
		//	string.Format("Adding FirebirdSql in DbProviderFactories section (Columns:{0}) [{1}:{2}:{3}:{4}]",
		//	table.Columns.Count, invariantName, factoryName, factoryDescription, factoryClass.AssemblyQualifiedName));
		table.BeginLoadData();


		table.Rows.Add(factoryName, factoryDescription, invariantName, factoryClass.AssemblyQualifiedName);

		table.EndLoadData();
		table.AcceptChanges();

		return Reflect.SetFieldValue(typeof(DbProviderFactories), "_providerTable",
			BindingFlags.Static | BindingFlags.NonPublic, table);

	}



	/// <summary>
	/// Another way of adding to local assembly cache using ConfigurationManager
	/// </summary>
	public static bool AddAssemblyToCache2(Type factoryClass, string factoryName, string factoryDescription)
	{
		// Diag.Trace();

		/*
		 * ConfigurationManager.GetSection()
		 * ---------------------------------
		 * 		 
		 * Returns a System.Data.DataTable containing System.Data.DataRow objects that contain
		 * the following data of DbProviderFactories. 
		 * Ordinal	Column name				Description
		 * -------	-----------				-----------
		 * 0		Name					Human-readable name for the data provider.
		 * 1		Description				Human-readable description of the data provider.
		 * 2		InvariantName			Name that can be used programmatically to refer to the data provider.
		 *									eg. FirebirdSql.Data.FirebirdClient
		 * 3		AssemblyQualifiedName	Fully qualified name of the factory class, which contains enough
		 *									information to instantiate the object.
		 *									eg. FirebirdSql.Data.FirebirdClient.FirebirdClientFactory,
		 *										FirebirdSql.Data.FirebirdClient, Version=9.1.1.0, Culture=neutral,
		 *										PublicKeyToken=3750abcc3150b00c
		*/

		string invariantName = factoryClass.Assembly.GetName().Name;

		if (ConfigurationManager.GetSection("system.data") is not DataSet dataSet)
		{
			Exception ex = new Exception("No \"system.data\" section found in configuration manager!");
			Diag.Dug(ex);
			throw ex;
		}

		int num = dataSet.Tables.IndexOf("DbProviderFactories");
		// DataRow row;

		DataTable table;
		if (num == -1)
		{
			// Diag.Trace(string.Format("Adding \"{0}\" section to assembly cache", "DbProviderFactories"));
			table = dataSet.Tables.Add("DbProviderFactories");
			dataSet.AcceptChanges();
		}
		else
		{
			table = dataSet.Tables[num];
			// if ((row = table.Rows.Find(invariantName)) != null)
			if (table.Rows.Find(invariantName) != null)
			{
				// Diag.Trace(
				//	string.Format("'DbProviderFactories' section (Columns:{0}) aready contains [{1}:{2}:{3}:{4}] as [{5}:{6}:{7}:{8}]",
				//	table.Columns.Count, invariantName, factoryName, factoryDescription, factoryClass.AssemblyQualifiedName,
				//	row[2].ToString(), row[0].ToString(), row[1].ToString(), row[3].ToString()));

				table.Dispose();

				return false;
			}


		}

		// Diag.Trace(
		//	String.Format("Adding FirebirdSql in DbProviderFactories section (Columns:{0}) [{1}:{2}:{3}:{4}]",
		// 	table.Columns.Count, invariantName, factoryName, factoryDescription, factoryClass.AssemblyQualifiedName));

		table.BeginLoadData();

		table.Rows.Add(factoryName, factoryDescription, invariantName, factoryClass.AssemblyQualifiedName);

		table.EndLoadData();
		table.AcceptChanges();

		table.Dispose();

		dataSet.AcceptChanges();

		return true;
	}

}
