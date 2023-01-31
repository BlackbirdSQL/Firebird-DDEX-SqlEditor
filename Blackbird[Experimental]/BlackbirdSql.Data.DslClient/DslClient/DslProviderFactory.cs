/*
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
using System.Data.Common;

using BlackbirdSql.Common;
using BlackbirdSql.Data.Common;

// Debug
using System.Reflection;
namespace BlackbirdSql.Data.DslClient;

#if NET
public class DslProviderFactory : DbProviderFactory
#else
public class DslProviderFactory : DbProviderFactory/*, IServiceProvider*/
#endif
{
	#region Variables

	/// <summary>
	///     The singleton instance.
	/// </summary>
	public static readonly DslProviderFactory Instance = new();

	#endregion

	#region Properties


	public override bool CanCreateDataSourceEnumerator
	{
		get
		{
			Diag.Trace();
			return false;
		}
	}



	#endregion




	#region Constructors

	private DslProviderFactory()
	{
		Diag.Trace();
	}

	#endregion

	#region Methods

	/// <summary>
	///     Creates a new command.
	/// </summary>
	/// <returns>The new command.</returns>
	public override DbCommand CreateCommand()
	{
		Diag.Trace();

		return new DslCommand();
	}

	/// <summary>
	///     Creates a new connection.
	/// </summary>
	/// <returns>The new connection.</returns>
	public override DbConnection CreateConnection()
	{
		Diag.Trace();

		return new DslConnection();
	}

	public override DbCommandBuilder CreateCommandBuilder()
	{
		Diag.Trace();
		return new DslCommandBuilder();
	}


	/// <summary>
	///     Creates a new connection string builder.
	/// </summary>
	/// <returns>The new connection string builder.</returns>
	public override DbConnectionStringBuilder CreateConnectionStringBuilder()
	{
		Diag.Trace();

		return new ConnectionStringBuilder();
	}

	public override DbDataAdapter CreateDataAdapter()
	{
		Diag.Trace();
		return new DslDataAdapter();
	}

	/// <summary>
	///     Creates a new parameter.
	/// </summary>
	/// <returns>The new parameter.</returns>
	public override DbParameter CreateParameter()
	{
		Diag.Trace();

		return new DslParameter();
	}


	#endregion

#if NETFRAMEWORK

	/*
	// Debug
	private static Assembly _ProviderAssembly = null;
	private static object _ProviderServices = null;
	private static object _ConnectionFactory = null;


	public static object ProviderServices
	{
		get
		{
			if (_ProviderServices != null)
				return _ProviderServices;

			_ProviderServices = GetProviderServices();

			// If EntityFramework.BlackbirdSql was found place it in app.config.
			// if (_ProviderServices != null)
			//	_ = new DbConfigurationEx();


			return _ProviderServices;
		}
	}

	public static object ConnectionFactory
	{
		get
		{
			if (_ConnectionFactory != null)
				return _ConnectionFactory;

			_ConnectionFactory = GetConnectionFactory();


			return _ConnectionFactory;
		}
	}

	public static Assembly ProviderAssembly
	{
		get
		{
			string assemblyPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			Diag.Trace("GetExecutingAssembly().Location: " + assemblyPath);
			if (_ProviderAssembly == null)
				_ProviderAssembly = Assembly.LoadFrom(assemblyPath + "\\EntityFramework.BlackbirdSql.dll");

			if (_ProviderAssembly == null)
			{
				NullReferenceException ex = new NullReferenceException("Assembly EntityFramework.BlackbirdSql could not be loaded");
				Diag.Dug(ex);

				throw ex;
			}

			return _ProviderAssembly;
		}
	}

	#region IServiceProvider Members

	object IServiceProvider.GetService(Type serviceType)
	{
#pragma warning disable CS8603 // Possible null reference return.

		Diag.Trace("Getting service: " + serviceType.FullName);

		if (serviceType == typeof(DbProviderServices)
			|| serviceType == typeof(System.Data.Entity.Core.Common.DbProviderServices))
		{
			return ProviderServices;
		}
		else
		{
			Diag.Dug(true, "No service for: " + serviceType.FullName);

			return null;
		}
#pragma warning restore CS8603 // Possible null reference return.
	}



	/// <summary>
	/// Loads BlackbirdSql.EntityFramework.dll and returns an instance to ProviderServices.
	/// This call should only ever happen if the dev created an edmx and BlackbirdSql.EntityFramework
	/// was not configured in the app.config
	/// </summary>
	/// <returns></returns>
	/// <exception cref="NullReferenceException"></exception>
	private static object GetProviderServices()
	{
		Diag.Trace();

		Type providerServicesType = ProviderAssembly.GetType("BlackbirdSql.Data.Entity.ProviderServices");
		if (providerServicesType == null)
		{
			NullReferenceException ex = new NullReferenceException("[Assembly: EntitityFramework.BlackbirdSql] BlackbirdSql.Data.Entity.ProviderServices could not be located");
			Diag.Dug(ex);

			throw ex;
		}

		FieldInfo fieldInfo = providerServicesType.GetField("Instance");
		if (fieldInfo == null)
		{
			NullReferenceException ex = new NullReferenceException("[Assembly: EntitityFramework.BlackbirdSql] BlackbirdSql.Data.Entity.ProviderServices.Instance could not be located");
			Diag.Dug(ex);

			throw ex;
		}

		object instance = fieldInfo.GetValue(providerServicesType);

		if (instance == null)
		{
			NullReferenceException ex = new NullReferenceException("[Assembly: EntitityFramework.BlackbirdSql] BlackbirdSql.Data.Entity.ProviderServices could not be located");
			Diag.Dug(ex);

			throw ex;
		}

		return instance;


	}



	/// <summary>
	/// Loads BlackbirdSql.EntityFramework.dll and returns an instance to ConnectionFactory.
	/// This call should only ever happen if the dev created an edmx and BlackbirdSql.EntityFramework
	/// was not configured in the app.config
	/// </summary>
	/// <returns></returns>
	/// <exception cref="NullReferenceException"></exception>
	private static object GetConnectionFactory()
	{
		Diag.Trace();

		object instance = ProviderAssembly.CreateInstance("BlackbirdSql.Data.Entity.ConnectionFactory");

		if (instance == null)
		{
			NullReferenceException ex = new NullReferenceException("[Assembly: EntitityFramework.BlackbirdSql] BlackbirdSql.Data.Entity.ConnectionFactory could not be created");
			Diag.Dug(ex);

			throw ex;
		}


		return instance;
	}


	private static void AddEntityFrameworkProvider()
	{
		// Get the application configuration file.
		System.Configuration.Configuration config =
			ConfigurationManager.OpenExeConfiguration(
			ConfigurationUserLevel.None);

		// Get the conectionStrings section.
		ConnectionStringsSection csSection =
		  config.ConnectionStrings;

		//Create your connection string into a connectionStringSettings object
		ConnectionStringSettings connection = CreateNewConnectionString();

		//Add the object to the configuration
		csSection.ConnectionStrings.Add(connection);

		//Save the configuration
		config.Save(ConfigurationSaveMode.Modified);

		//Refresh the Section
		ConfigurationManager.RefreshSection("connectionStrings");
	}


	#endregion
	*/
#endif
}
