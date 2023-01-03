/*
 *  Visual Studio DDEX Provider for BlackbirdSql DslClient
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.blackbirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Data.Common.BlackbirdSql;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.DataTools;

/// <summary>
/// This is the class that implements the package exposed by this assembly.
///
/// The minimum requirement for a class to be considered a valid package for Visual Studio
/// is to implement the IVsPackage interface and register itself with the shell.
/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
/// to do it: it derives from the Package class that provides the implementation of the 
/// IVsPackage interface and uses the registration attributes defined in the framework to 
/// register itself and its components with the shell.
/// </summary>
// This attribute tells the registration utility (regpkg.exe) that this class needs
// to be registered as package.
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]

// A Visual Studio component can be registered under different regitry roots; for instance
// when you debug your package you want to register it in the experimental hive. This
// attribute specifies the registry root to use if no one is provided to regpkg.exe with
// the /root switch.
// [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\15.0")]
// This attribute is used to register the informations needed to show the this package
// in the Help/About dialog of Visual Studio.
[InstalledProductRegistration("#100", "#102", "1.0", IconResourceID = 400)]

// In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
// package needs to have a valid load key (it can be requested at 
// http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
// package has a load key embedded in its resources.
// [ProvideLoadKey("Standard", "1.0", "DDEX Provider for BlackbirdClient", "..", 106)]

[Guid(Configuration.PackageData.PackageGuid)]
[ProvideAutoLoad(Configuration.PackageData.ShellInitializedContextRuleGuid, PackageAutoLoadFlags.BackgroundLoad)]

[ProvideService(typeof(IProviderObjectFactory), IsAsyncQueryable = true, ServiceName = "Blackbird Legacy DDEX Provider Object Factory")]
[Configuration.PackageRegistration]

public sealed class BlackbirdSqlDataPackage : AsyncPackage
{

	#region · Constructors ·

	/// <summary>
	/// Default constructor of the package.
	/// Inside this method you can place any initialization code that does not require 
	/// any Visual Studio service because at this point the package object is created but 
	/// not sited yet inside Visual Studio environment. The place to do all the other 
	/// initialization is the Initialize method.
	/// </summary>
	public BlackbirdSqlDataPackage()
	{
		Diag.Dug();
	}

	#endregion

	#region · Package Members ·

	/// <summary>
	/// Initialization of the package; this method is called right after the package is sited, so this is the place
	/// where you can put all the initilaization code that rely on services provided by VisualStudio.
	/// </summary>
	protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
	{
		Diag.Dug();


		await base.InitializeAsync(cancellationToken, progress);


		AddService
		(
			typeof(IProviderObjectFactory),
			(_, _, _) => Task.FromResult<object>(new ProviderObjectFactory()),
			promote: true
		);


		// ((IServiceContainer)this).AddService(typeof(IProviderObjectFactory), new ServiceCreatorCallback(this.CreateService), true);

		Diag.Dug("IProviderObjectFactory service added");



		if (DbProviderFactoriesEx.ConfigurationManagerRegisterFactory(typeof(Data.DslClient.DslProviderFactory),
			Data.Properties.Resources.Provider_ShortDisplayName, Data.Properties.Resources.Provider_DisplayName))
		{
			Diag.Dug("DbProviderFactory registration completed during package registration");
		}
		else
		{
			Diag.Dug("DbProviderFactory registration not completed during package registration. Factory already registered");
		}

		Diag.Dug("Package initialization complete - Switched to main thread");

		// When initialized asynchronously, we *may* be on a background thread at this point.
		// Do any initialization that requires the UI thread after switching to the UI thread.
		// Otherwise, remove the switch to the UI thread if you don't need it.
		// await JoinableTaskFactory.SwitchToMainThreadAsync(false, cancellationToken);
	}

	#endregion

	#region · Private Methods ·

	private object CreateService(IServiceContainer container, Type serviceType)
	{
		Diag.Dug("Service type: " + serviceType.FullName);

		if (serviceType == typeof(ProviderObjectFactory))
		{
			return new ProviderObjectFactory();
		}

		return null;
	}

	#endregion
}