// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Properties;
using Microsoft.VisualStudio.Data.Core;



namespace BlackbirdSql.Core.Extensions;


// =========================================================================================================
//										DbProviderFactoriesEx Class 
//
/// <summary>
/// Static class for adding a DotNet Invariant's DBProviderFactory into assembly register on the fly.
/// </summary>
// =========================================================================================================
internal static class DbProviderFactoriesEx
{


	// ---------------------------------------------------------------------------------
	#region Constants - DbProviderFactoriesEx
	// ---------------------------------------------------------------------------------


	private const long C_MaxLateGracePeriod = 15000L;


	#endregion Constants



	// =====================================================================================================
	#region Property accessors - DbProviderFactoriesEx
	// =====================================================================================================


	internal static IDictionary<Guid, IVsDataSource> DataSources
	{
		get
		{
			IVsDataSourceManager sourceManager = ApcManager.GetService<IVsDataSourceManager>();

			return sourceManager.Sources;
		}
	}



	internal static IDictionary<Guid, IVsDataProvider> Providers
	{
		get
		{
			IVsDataProviderManager providerManager = ApcManager.GetService<IVsDataProviderManager>();

			return providerManager.Providers;
		}
	}




	#endregion Property accessors





	// =====================================================================================================
	#region Methods - DbProviderFactoriesEx
	// =====================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Deprecated: Use RegisterAssemblyDirect() instead.
	/// Another way of adding to local assembly register using ConfigurationManager.
	/// (DO NOT USE!!! ConfigurationManager may not be ready)
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool ConfigurationManagerRegisterAssembly(string invariant, string factoryName,
		string factoryDescription, string assemblyQualifiedName)
	{
		/*
		 * ConfigurationManager.GetSection()
		 * ---------------------------------
		 * 		 
		 * Returns a DataTable containing DataRow objects that contain the following 
		 * data of DbProviderFactories. 
		 * 
		 * Ordinal	Column name				Description
		 * -------	-----------				-----------
		 * 0		Name					Human-readable name for the data provider.
		 * 1		Description				Human-readable description of the data provider.
		 * 2		InvariantName			Name that can be used programmatically to refer to the data provider.
		 *									eg. FirebirdSql.Data.FirebirdClient
		 * 3		AssemblyQualifiedName	Fully qualified name of the factory class, which contains sufficient
		 *									information to instantiate the object.
		 *									eg. FirebirdSql.Data.FirebirdClient.FirebirdClientFactory,
		 *										FirebirdSql.Data.FirebirdClient, Version=9.1.1.0, Culture=neutral,
		 *										PublicKeyToken=3750abcc3150b00c
		*/


		DataSet dataSet;

		try
		{
			if (ConfigurationManager.GetSection("system.data") is not DataSet sectionDataSet)
			{
				throw new Exception("No \"system.data\" section found in configuration manager!");
			}

			dataSet = sectionDataSet;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw ex;
		}

		DataTable table;
		DataRow row;


		int index = dataSet.Tables.IndexOf("DbProviderFactories");

		if (index == -1)
		{
			Evs.Warning(typeof(DbProviderFactoriesEx), nameof(ConfigurationManagerRegisterAssembly),
				"Adding 'DbProviderFactories' section to assembly register");
			table = dataSet.Tables.Add("DbProviderFactories");
		}
		else
		{
			table = dataSet.Tables[index];
			row = table.Rows.Find(invariant);

			if (row != null)
			{
				Evs.Warning(typeof(DbProviderFactoriesEx), nameof(ConfigurationManagerRegisterAssembly),
					$"'DbProviderFactories' section (Columns:{table.Columns.Count}) already contains [{invariant}::{factoryName}::{factoryDescription}::{assemblyQualifiedName}] as [{row[2]}::{row[0]}::{row[1]}::{row[3]}]");

				return false;
			}


		}

		table.Rows.Add(factoryName, factoryDescription, invariant, assemblyQualifiedName);
		table.AcceptChanges();

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Remove factory using ConfigurationManager. (For recovery)
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static bool ConfigurationManagerRemoveAssembly(string invariant, out string factoryName,
		out string factoryDescription, out string assemblyQualifiedName)
	{

		DataSet dataSet;

		try
		{
			if (ConfigurationManager.GetSection("system.data") is not DataSet sectionDataSet)
			{
				throw new Exception("No \"system.data\" section found in configuration manager!");
			}

			dataSet = sectionDataSet;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw ex;
		}

		DataTable table;
		DataRow row;
		factoryName = factoryDescription = assemblyQualifiedName = null;


		int index = dataSet.Tables.IndexOf("DbProviderFactories");

		if (index == -1)
		{
			Diag.Ex(new Exception("No \"DbProviderFactories\" table found in configuration manager!"));
			return false;
		}

		table = dataSet.Tables[index];
		row = table.Rows.Find(invariant);

		if (row == null)
			return false;


		object @object = row[0] == DBNull.Value ? null : row[0];
		factoryName = @object?.ToString();

		@object = row[1] == DBNull.Value ? null : row[1];
		factoryDescription = @object?.ToString();

		@object = row[3] == DBNull.Value ? null : row[3];
		assemblyQualifiedName = @object?.ToString();

		table.Rows.Remove(row);
		table.AcceptChanges();

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Recovers DBProviderFactory's that have been tagged as invalid by Visual Studio
	/// after arriving late.
	/// </summary>
	/// <returns>
	/// True if successful else false if grace period expired and gave up waiting.
	/// </returns>
	// ---------------------------------------------------------------------------------
	internal static bool InvalidatedProviderFactoryRecovery()
	{
		// Evs.Trace(typeof(DbProviderFactoriesEx), nameof(InvalidatedProviderFactoryRecovery));

		if (!PersistentSettings.ValidateProviderFactories)
			return true;

		// Only allowed to run in thread pool
		Diag.ThrowIfOnUIThread();

		int waitTime = 0;

		Stopwatch stopwatch = new();
		stopwatch.Start();

		try
		{
			IVsDataProviderManager vsDataProviderMgr = ApcManager.GetService<IVsDataProviderManager>();

			IDictionary<Guid, IVsDataProvider> providers = vsDataProviderMgr.Providers;
			Dictionary<Guid, IVsDataProvider> remainingProviders = new(providers.Count);
			Dictionary<Guid, IVsDataProvider> unverifiedProviders;

			// List of known safe providers. (SqlServer, Oracle, BlackbirdSql etc)
			List<string> verifiedProviderGuids = [SystemData.C_ProviderGuid.ToLowerInvariant(),
				// SqlLite.
				"796a79e8-2579-4375-9e12-03a9e0d1fc02",
				// OLDB.
				"7f041d59-d76a-44ed-9aa2-fbf6b0548b80",
				// SqlClient.
				"8800600a-add9-47e8-81d2-1d13b5a09c13",
				// Oracle.
				"8f5c5018-ae09-42cf-b2cc-2cccc7cfc2bb",
				// SQL Server.
				"91510608-8809-4020-8897-fba057e22d54",
				// ODBC.
				"c3d4f4ce-2c48-4381-b4d6-34fa50c51c86",
				// PostgreSql
				"70ba90f8-3027-4af1-9b15-37abbd48744c"
			];


			// Build a list of unverified providers.

			foreach (KeyValuePair<Guid, IVsDataProvider> pair in providers)
			{
				if (verifiedProviderGuids.Contains(pair.Key.ToString().ToLowerInvariant()))
					continue;
				remainingProviders.Add(pair.Key, pair.Value);
			}

			if (remainingProviders.Count == 0)
				return true;

			bool firstCycle = true;
			int badCount = 0;
			int validationCount = remainingProviders.Count;
			string invariant = "";
			DbProviderFactory providerFactory = null;

			string fmt;

			DataTable providerTable = (DataTable)Reflect.InvokeMethod(typeof(DbProviderFactories), "GetProviderTable");

			while (stopwatch.ElapsedMilliseconds <= C_MaxLateGracePeriod && remainingProviders.Count > 0)
			{
				// Loop through each unverified provider and try to load.
				// If it fails try and remove it in configuraton manager.
				// If removal succeeds, re-add the provider into the factory table
				// directly and move onto next provider.
				// if there's a fail start all over again and keep retrying.
				// We'll give late loading providers up to 15 seconds to arrive.

				unverifiedProviders = new(remainingProviders);

				foreach (KeyValuePair<Guid, IVsDataProvider> pair in unverifiedProviders)
				{
					// Get the next provider's invariant name.
					invariant = (string)pair.Value.GetProperty("InvariantName");


					// if (firstCycle)
					// {
					//	Evs.Debug(typeof(DbProviderFactoriesEx), "InvalidatedProviderFactoryRecovery()",
					//		$"Verifying Invariant: {invariant}, Guid: {pair.Key}.");
					// }


					// Try and load the factory.

					providerFactory = null;
					DataRow dataRow = providerTable.Rows.Find(invariant);

					try
					{
						if (dataRow != null)
							providerFactory = DbProviderFactories.GetFactory(dataRow);
					}
					catch
					{
					}

					// If it succeeds move onto the next provider.
					if (providerFactory != null)
					{
						remainingProviders.Remove(pair.Key);
						continue;
					}


					if (firstCycle)
					{
						// Evs.Warning(typeof(DbProviderFactoriesEx), "InvalidatedProviderFactoryRecovery()", "\n\tBad Invariant: {0}. Attempting recovery...", invariant);

						if (badCount == 0)
						{
							Diag.OutputPaneWriteLineAsyup(Resources.DbProviderFactoriesEx_Recovery.Fmt(validationCount), false);

							// Give output time to breath.
							System.Threading.Thread.Sleep(10);
							System.Threading.Thread.Yield();
						}

						badCount++;

						Diag.OutputPaneWriteLineAsyup(Resources.DbProviderFactoriesEx_RecoveryInvariantFaulted.Fmt(invariant), false);
					}



					try
					{
						// Try and remove the factory using configuration manager.
						bool directRemoval = false;

						bool result = ConfigurationManagerRemoveAssembly(invariant,
							out string factoryName, out string factoryDescription, out string assemblyQualifiedName);

						// If that fails try and remove it directly.

						if (!result)
						{
							directRemoval = true;

							result = RemoveAssemblyDirect(invariant, out factoryName, out factoryDescription,
								out assemblyQualifiedName);
						}

						if (result)
						{
							// If the removal succeeds add it back using direct table updates and move onto next provider.
							RegisterAssemblyDirect(invariant, factoryName, factoryDescription, assemblyQualifiedName);
							remainingProviders.Remove(pair.Key);

							// Evs.Warning(typeof(DbProviderFactoriesEx), "InvalidatedProviderFactoryRecovery()",
							//	"\n\tRecovered invariant '{0}' using {1} removal after {2}.", invariant,
							//	directRemoval ? "direct" : "ConfigurationManager",
							//	dataRow == null ? "failed ConfigurationManager registration" : "being invalidated");

							if (directRemoval && dataRow == null)
								fmt = Resources.DbProviderFactoriesEx_RecoveryDirectConfigurationManager;
							else if (directRemoval && dataRow != null)
								fmt = Resources.DbProviderFactoriesEx_RecoveryDirectInvalidated;
							else if (!directRemoval && dataRow == null)
								fmt = Resources.DbProviderFactoriesEx_RecoveryConfigurationManagerConfigurationManager;
							else
								fmt = Resources.DbProviderFactoriesEx_RecoveryConfigurationManagerInvalidated;

							Diag.OutputPaneWriteLineAsyup(fmt.Fmt(invariant), false);


							continue;
						}
					}
					catch (Exception ex)
					{
						Diag.Ex(ex);
					}

				}

				waitTime += 50;

				// If total wait time is greater than 15 seconds we're done trying.
				if (stopwatch.ElapsedMilliseconds > C_MaxLateGracePeriod)
					break;

				System.Threading.Thread.Sleep(50);
				if (waitTime % 500 == 0)
					System.Threading.Thread.Yield();


				firstCycle = false;
			}

			if (stopwatch.IsRunning)
				stopwatch.Stop();

			if (badCount > 0)
			{
				if (remainingProviders.Count > 0)
				{
					foreach (KeyValuePair<Guid, IVsDataProvider> pair in remainingProviders)
					{
						invariant = (string)pair.Value.GetProperty("InvariantName");

						Diag.OutputPaneWriteLineAsyup(Resources.DbProviderFactoriesEx_RecoveryInvariantFailed.Fmt(invariant), false);
					}
				}

				// Evs.Warning(typeof(DbProviderFactoriesEx), "InvalidatedProviderFactoryRecovery()",
				//	"\n\tProvider recovery - Bad invariants: {0}, Recovered invariants: {1}, Total recovery time: {2}ms.",
				//	badCount, badCount - remainingProviders.Count, stopwatch.ElapsedMilliseconds);


				fmt = Resources.DbProviderFactoriesEx_RecoveryResult.Fmt(badCount,
					badCount - remainingProviders.Count, stopwatch.ElapsedMilliseconds);

				StringBuilder sb = new(fmt.Length);

				sb.Append('=', (fmt.Length - 10) / 2);
				sb.Append(" Done ");
				sb.Append('=', fmt.Length - sb.Length);

				fmt += "\n" + sb + "\n";

				Diag.OutputPaneWriteLineAsyup(fmt, false);
			}

		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}

		if (stopwatch.IsRunning)
			stopwatch.Stop();

		return stopwatch.ElapsedMilliseconds < C_MaxLateGracePeriod;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Directly adds a DotNet Invariant's DBProviderFactory into the local assembly
	/// register.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool RegisterAssemblyDirect(string invariant, string factoryName, string factoryDescription, string assemblyQualifiedName)
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


		DataRow row = table.Rows.Find(invariant);

		if (row != null)
		{
			Evs.Warning(typeof(DbProviderFactoriesEx), nameof(ConfigurationManagerRegisterAssembly),
				$"'DbProviderFactories' section (Columns:{table.Columns.Count}) aready contains [{invariant}::{factoryName}::{factoryDescription}::{assemblyQualifiedName}] as [{row[2]}::{row[0]}::{row[1]}::{row[3]}]");

			return false;
		}

		
		table.Rows.Add(factoryName, factoryDescription, invariant, assemblyQualifiedName);
		table.AcceptChanges();

		return Reflect.SetFieldValue(typeof(DbProviderFactories), "_providerTable", table);

	}






	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Directly removes factory. (For recovery)
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static bool RemoveAssemblyDirect(string invariant, out string factoryName,
		out string factoryDescription, out string assemblyQualifiedName)
	{

		DataTable table = DbProviderFactories.GetFactoryClasses();

		factoryName = factoryDescription = assemblyQualifiedName = null;

		DataRow row = table.Rows.Find(invariant);

		if (row == null)
			return false;


		object @object = row[0] == DBNull.Value ? null : row[0];
		factoryName = @object?.ToString();

		@object = row[1] == DBNull.Value ? null : row[1];
		factoryDescription = @object?.ToString();

		@object = row[3] == DBNull.Value ? null : row[3];
		assemblyQualifiedName = @object?.ToString();

		table.Rows.Remove(row);
		table.AcceptChanges();

		return Reflect.SetFieldValue(typeof(DbProviderFactories), "_providerTable", table);
	}


	#endregion Methods

}
