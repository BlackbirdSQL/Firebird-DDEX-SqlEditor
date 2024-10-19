// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using BlackbirdSql.Core;
using BlackbirdSql.VisualStudio.Ddex.Interfaces;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;

internal class VsPackageRegistrationAttribute: RegistrationAttribute
{
	public VsPackageRegistrationAttribute() : base()
	{
		BlackbirdSqlDdexExtension.RegisterDataServices();
	}



	/// <summary>
	/// Registers the package in the local user's VS private registry
	/// </summary>
	/// <remarks>
	/// This is pretty much solid so commenting out diags
	/// </remarks>
	public override void Register(RegistrationContext context)
	{
		if (context == null)
		{
			ArgumentNullException ex = new("context");
			Diag.Dug(ex);
			throw ex;
		}

		Key key = null;
		Key key2 = null;
		Key key3;
		ExtensionData.RegistryValue registryValue;

		try
		{
			Type providerFactoryClass = NativeDb.ClientFactoryType; 
			string invariantName = providerFactoryClass.Assembly.GetName().Name;
			// string invariantFullName = providerFactoryClass.Assembly.FullName;

			Type providerObjectFactoryInterface = typeof(IBsProviderObjectFactory);
			string providerObjectFactoryAssembly = providerObjectFactoryInterface.Assembly.FullName;

			string dataSourceGuid = SystemData.C_DataSourceGuid.StartsWith("{")
				? SystemData.C_DataSourceGuid
				: $"{{{SystemData.C_DataSourceGuid}}}";

			string providerGuid = SystemData.C_ProviderGuid.StartsWith("{")
				? SystemData.C_ProviderGuid
				: $"{{{SystemData.C_ProviderGuid}}}";

			// Clean up
			context.RemoveKey("DataSources\\" + dataSourceGuid + "\\SupportingProviders\\" + providerGuid);

			// Add the Firebird data source (if not exists???)
			key = context.CreateKey("DataSources\\" + dataSourceGuid);

			key.SetValue(null, NativeDb.DataProviderName);
			key.SetValue("DefaultProvider", providerGuid);

			// Add this package as a provider for the Firebird data source
			key2 = key.CreateSubkey("SupportingProviders");
			key3 = key2.CreateSubkey(providerGuid);
			key3.SetValue("DisplayName", "DataSource_FirebirdServer, "
				+ "BlackbirdSql.VisualStudio.Ddex.Properties.Resources, " + providerObjectFactoryAssembly);
			key3.SetValue("UsingDescription", "DataProvider_Ddex_DataSource_Description, "
				+ "BlackbirdSql.VisualStudio.Ddex.Properties.Resources, " + providerObjectFactoryAssembly);
			DisposeKey(ref key3);
			DisposeKey(ref key2);
			DisposeKey(ref key);

			key = context.CreateKey("DataProviders\\" + providerGuid);

			string providerName = Properties.Resources.DataProvider_Name
				.FmtRes(GetType().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
			key.SetValue(null, providerName);
			key.SetValue("Assembly", providerObjectFactoryAssembly);

			key.SetValue("PlatformVersion", "2.0");
			key.SetValue("Technology", "{" + VS.AdoDotNetTechnologyGuid + "}");
			key.SetValue("AssociatedSource", dataSourceGuid);
			key.SetValue("InvariantName", invariantName);
			key.SetValue("Description", "DataProvider_Ddex_Description, "
				+ "BlackbirdSql.VisualStudio.Ddex.Properties.Resources, " + providerObjectFactoryAssembly);
			key.SetValue("DisplayName", "DataProvider_Ddex, "
				+ "BlackbirdSql.VisualStudio.Ddex.Properties.Resources, " + providerObjectFactoryAssembly);
			key.SetValue("ShortDisplayName",
			"DataProvider_Ddex_Short, BlackbirdSql.VisualStudio.Ddex.Properties.Resources, "
				+ providerObjectFactoryAssembly);

			// With everything working correctly we should need no codebase, no gac registration, and no dotnet system/machine config
			// Just run the vsix and go.
			// key.SetValue("CodeBase", PackageData.CodeBase);

			Attribute customAttribute = Attribute.GetCustomAttribute(providerObjectFactoryInterface, typeof(GuidAttribute));
			if (customAttribute == null)
			{
				Debug.Assert(condition: false);
				ApplicationException ex = new("IBsProviderObjectFactory doesn't have Guid attribute.");
				Diag.Dug(ex);
				throw ex;
			}


			if (customAttribute is not GuidAttribute guidAttribute)
			{
				Debug.Assert(condition: false);
				ApplicationException ex = new("IBsProviderObjectFactory's Guid attribute has the incorrect type.");
				Diag.Dug(ex);
				throw ex;
			}

			key.SetValue("FactoryService", "{" + guidAttribute.Value + "}");

			// Add in the supported sevices
			key2 = key.CreateSubkey("SupportedObjects");
			
			foreach (KeyValuePair<string, int> implementation in ExtensionData.Implementations)
			{
				key3 = key2.CreateSubkey(implementation.Key);

				for (int i = 0; i < implementation.Value; i++)
				{
					registryValue = ExtensionData.ImplementationValues[implementation.Key + (i==0?"":i.ToString())];

					// Evs.Trace(implementation.Key + ": " + (registryValue.Name == null ? "null" : registryValue.Name) + ":" + registryValue.Value);
					key3.SetValue(registryValue.Name, registryValue.Value);
				}

				DisposeKey(ref key3);
			}

			// InstallGlobalAssemblyCache();


		}
		finally
		{
			DisposeKey(ref key2);
			DisposeKey(ref key);
		}
	}



	/// <summary>
	/// Deregisters the package in the local user's VS private registry
	/// </summary>
	public override void Unregister(RegistrationContext context)
	{
		if (context == null)
		{
			ArgumentNullException ex = new("context");
			Diag.Dug(ex);
			throw ex;
		}

		string dataSourceGuid = SystemData.C_DataSourceGuid.StartsWith("{")
			? SystemData.C_DataSourceGuid
			: $"{{{SystemData.C_DataSourceGuid}}}";

		string providerGuid = SystemData.C_ProviderGuid.StartsWith("{")
			? SystemData.C_ProviderGuid
			: $"{{{SystemData.C_ProviderGuid}}}";

		Type providerObjectFactoryInterface = typeof(IBsProviderObjectFactory);
		Attribute customAttribute = Attribute.GetCustomAttribute(providerObjectFactoryInterface, typeof(GuidAttribute));

		if (customAttribute == null)
		{
			Debug.Assert(condition: false);
			ApplicationException ex = new("[BUG CHECK] IBsProviderObjectFactory doesn't have Guid attribute.");
			Diag.Dug(ex);
			throw ex;
		}

		if (customAttribute is not GuidAttribute guidAttribute)
		{
			Debug.Assert(condition: false);
			ApplicationException ex = new("[BUG CHECK] IBsProviderObjectFactory's Guid attribute has the incorrect type.");
			Diag.Dug(ex);
			throw ex;
		}
		Debug.Assert(guidAttribute.Value == SystemData.C_ProviderObjectFactoryServiceGuid);

		context.RemoveValue("DataSources\\" + dataSourceGuid, "DefaultProvider");
		context.RemoveKey("DataSources\\" + dataSourceGuid + "\\SupportingProviders\\" + providerGuid);

		context.RemoveKey("DataProviders\\" + providerGuid);

		context.RemoveKey("Services\\{" + guidAttribute.Value + "}");

		// UninstallGlobalAssemblyCache();

	}




	private static void DisposeKey(ref Key key)
	{
		if (key != null)
		{
			Key lockKey = Interlocked.Exchange(ref key, null);
			lockKey.Close();
			((IDisposable)lockKey).Dispose();
		}
	}


	// We're not going to use the gac so this is all redundant. Can be deleted
	/*
	public static void InstallGlobalAssemblyCache()
	{
		Type providerFactoryClass = typeof(FirebirdClientFactory);


		// Evs.Trace("GAC Install: " + providerFactoryClass.Assembly.Location);

		Publish publisher = new Publish();
		publisher.GacInstall(factoryClass.Assembly.Location);
	}

	public static void UninstallGlobalAssemblyCache()
	{
		Type providerFactoryClass = typeof(FirebirdClientFactory);

		// Evs.Trace("GAC Uninstall: " + providerFactoryClass.Assembly.Location);
		Publish publisher = new Publish();

		try
		{
			publisher.GacRemove(factoryClass.Assembly.Location);
		}
		catch
		{
		}
	}
	*/
}
