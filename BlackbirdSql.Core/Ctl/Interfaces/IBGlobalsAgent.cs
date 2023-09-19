

using System;
using BlackbirdSql.Core.Ctl.Events;
using EnvDTE;

using Microsoft.VisualStudio.Shell.Interop;




namespace BlackbirdSql.Core.Ctl.Interfaces;


public interface IBGlobalsAgent
{

	DTE Dte { get; }

	IBPackageController Controller { get; }

	IBAsyncPackage DdexPackage { get; }


	bool ShowDiagramPane { get; }

	bool ValidateConfig { get; }

	bool PeristentValidation { get; }

	bool ValidateEdmx { get; }

	bool IsValidateFailedStatus { get; set; }



	bool SetIsValidStatus(Globals globals, bool valid);

	bool SetIsScannedStatus(Project project);

	bool SetIsValidatedDbProviderStatus(Project project);

	bool SetIsValidatedEFStatus(Project project);

	bool SetIsUpdatedEdmxsStatus(Project project);

	bool ClearValidateStatus();

	bool ClearPersistentFlag(Globals globals, string key);

	bool IsValidatedStatus(Globals globals);

	bool IsValidExecutableProjectType(IVsSolution solution, Project project);

	bool IsValidStatus(Globals globals);

	bool IsScannedStatus(Project project);

	bool IsConfiguredDbProviderStatus(Project project);

	bool IsConfiguredEFStatus(Project project);

	bool IsUpdatedEdmxsStatus(Project project);

	bool SetFlagStatus(Globals globals, int flag, bool enabled, int flag2 = 0, bool enabled2 = false);

	void UpdatePackageGlobals(GlobalEventArgs e);

}

public static class ExtensionMembers
{

	internal static IBGlobalsAgent _GlobalsAgentInstance;


	public static IBGlobalsAgent GetInstance(this IBGlobalsAgent value, bool throwIfNotExists = true)
	{
		if (_GlobalsAgentInstance == null && throwIfNotExists)
		{
			NullReferenceException ex = new("Attempt to access uninitialized GlobalsAgent instance");
			Diag.Dug(ex);
			throw ex;
		}

		return _GlobalsAgentInstance;
	}

	public static IBGlobalsAgent SetInstance(this IBGlobalsAgent instance, IBGlobalsAgent value)
	{
		if (_GlobalsAgentInstance != null)
		{
			ArgumentException ex = new("GlobalsAgent instance already created");
			Diag.Dug(ex);
			throw ex;
		}

		_GlobalsAgentInstance = instance;

		return _GlobalsAgentInstance;
	}

}
