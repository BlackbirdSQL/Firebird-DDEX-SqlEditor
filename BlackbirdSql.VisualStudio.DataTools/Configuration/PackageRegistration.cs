using System;
using System.Collections.Generic;
// using System.ComponentModel;
using System.Diagnostics;
// using System.EnterpriseServices.Internal;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;

using BlackbirdSql.Common;
namespace BlackbirdSql.VisualStudio.DataTools.Configuration;


internal sealed class PackageRegistration : RegistrationAttribute
{
	/// <summary>
	/// Registers the package in the local user's VS private registry
	/// </summary>
	public override void Register(RegistrationContext context)
	{
		Diag.Dug();

		if (context == null)
		{
			Diag.Dug(true, "Null argument: context");

			throw new ArgumentNullException("context");
		}

		Key key = null;
		Key key2 = null;
		Key key3;
		SupportedObjects.RegistryValue registryValue;

		try
		{
			Type providerFactoryClass = typeof(Data.DslClient.DslProviderFactory);
			string invariantName = providerFactoryClass.Assembly.GetName().Name;
			string invariantFullName = providerFactoryClass.Assembly.FullName;


			Type providerObjectFactoryClass = typeof(IProviderObjectFactory);
			string nameProviderObjectFactory = providerObjectFactoryClass.Assembly.FullName;

			string dataSourceGuid = "{" + Data.Configuration.PackageData.DataSourceGuid + "}";
			string providerGuid = "{" + PackageData.ProviderGuid + "}";

			// Clean up
			context.RemoveKey("DataSources\\" + dataSourceGuid + "\\SupportingProviders\\" + providerGuid);

			// Add the Firebird data source (if not exists???)
			key = context.CreateKey("DataSources\\" + dataSourceGuid);

			key.SetValue(null, Data.Configuration.PackageData.TechnologyName);
			key.SetValue("DefaultProvider", providerGuid);


			// Add this package as a provider for the Firebird data source
			key2 = key.CreateSubkey("SupportingProviders");
			key3 = key2.CreateSubkey(providerGuid);
			key3.SetValue("DisplayName", Data.Configuration.PackageData.TechnologyName);
			key3.SetValue("UsingDescription", "DdexProvider_Description, "
				+ "BlackbirdSql.VisualStudio.DataTools.Properties.Resources, " + nameProviderObjectFactory);
			DisposeKey(ref key3);
			DisposeKey(ref key2);
			DisposeKey(ref key);

			key = context.CreateKey("DataProviders\\" + providerGuid);
			key.SetValue(null, Data.Properties.Resources.Provider_DisplayName);
			key.SetValue("Assembly", nameProviderObjectFactory);

			// key.SetValue("PlatformVersion", "2.0");
			key.SetValue("Technology", "{" + Data.Configuration.PackageData.TechnologyGuid + "}");
			key.SetValue("AssociatedSource", dataSourceGuid);
			key.SetValue("InvariantName", invariantName);
			key.SetValue("Description", "DdexProvider_Description, "
				+ "BlackbirdSql.VisualStudio.DataTools.Properties.Resources, " + nameProviderObjectFactory);
			key.SetValue("DisplayName", "DdexProvider_DisplayName, "
				+ "BlackbirdSql.VisualStudio.DataTools.Properties.Resources, " + nameProviderObjectFactory);
			key.SetValue("ShortDisplayName", "DdexProvider_ShortDisplayName, BlackbirdSql.VisualStudio.DataTools.Properties.Resources, " + nameProviderObjectFactory);

			// With everything working correctly we should need no codebase, no gac registration, and no dotnet system/machine config
			// Just run the vsix and go.
			// key.SetValue("CodeBase", PackageData.CodeBase);

			Attribute customAttribute = Attribute.GetCustomAttribute(providerObjectFactoryClass, typeof(GuidAttribute));
			if (customAttribute == null)
			{
				Diag.Dug(true, "IProviderObjectFactory doesn't have Guid attribute");

				Debug.Assert(condition: false);
				throw new ApplicationException("IProviderObjectFactory doesn't have Guid attribute.");
			}


			if (customAttribute is not GuidAttribute guidAttribute)
			{
				Diag.Dug(true, "IProviderObjectFactory's Guid attribute has the incorrect type");

				Debug.Assert(condition: false);
				throw new ApplicationException("IProviderObjectFactory's Guid attribute has the incorrect type.");
			}
			Debug.Assert(guidAttribute.Value == PackageData.ObjectFactoryServiceGuid);

			key.SetValue("FactoryService", "{" + guidAttribute.Value + "}");

			// Add in the supported sevices
			key2 = key.CreateSubkey("SupportedObjects");

			foreach (KeyValuePair<string, int> implementation in SupportedObjects.Implementations)
			{
				key3 = key2.CreateSubkey(implementation.Key);

				for (int i = 0; i < implementation.Value; i++)
				{
					registryValue = SupportedObjects.Values[implementation.Key + ":" + i.ToString()];

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
		Diag.Dug();

		if (context == null)
		{
			Diag.Dug(true, "Null argument: context");

			throw new ArgumentNullException("context");
		}

		string dataSourceGuid = "{" + Data.Configuration.PackageData.DataSourceGuid + "}";
		string providerGuid = "{" + PackageData.ProviderGuid + "}";
		Type typeProviderObjectFactory = typeof(IProviderObjectFactory);
		Attribute customAttribute = Attribute.GetCustomAttribute(typeProviderObjectFactory, typeof(GuidAttribute));

		if (customAttribute == null)
		{
			Diag.Dug(true, "IProviderObjectFactory doesn't have Guid attribute");

			Debug.Assert(condition: false);
			throw new ApplicationException("[BUG CHECK] IProviderObjectFactory doesn't have Guid attribute.");
		}

		if (customAttribute is not GuidAttribute guidAttribute)
		{
			Diag.Dug(true, "IProviderObjectFactory's Guid attribute has the incorrect type");

			Debug.Assert(condition: false);
			throw new ApplicationException("[BUG CHECK] IProviderObjectFactory's Guid attribute has the incorrect type.");
		}
		Debug.Assert(guidAttribute.Value == PackageData.ObjectFactoryServiceGuid);

		context.RemoveValue("DataSources\\" + dataSourceGuid, "DefaultProvider");
		context.RemoveKey("DataSources\\" + dataSourceGuid + "\\SupportingProviders\\" + providerGuid);

		context.RemoveKey("DataProviders\\" + providerGuid);

		context.RemoveKey("Services\\{" + guidAttribute.Value + "}");

		// UninstallGlobalAssemblyCache();

	}



	public static void InstallGlobalAssemblyCache()
	{
		Type factoryClass = typeof(Data.DslClient.DslProviderFactory);


		Diag.Dug("GAC Install: " + factoryClass.Assembly.Location);

		// Publish publisher = new Publish();
		// publisher.GacInstall(factoryClass.Assembly.Location);
	}

	public static void UninstallGlobalAssemblyCache()
	{
		Type factoryClass = typeof(Data.DslClient.DslProviderFactory);

		Diag.Dug("GAC Uninstall: " + factoryClass.Assembly.Location);

		// Publish publisher = new Publish();

		try
		{
			// publisher.GacRemove(factoryClass.Assembly.Location);
		}
		catch
		{
		}
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

}
