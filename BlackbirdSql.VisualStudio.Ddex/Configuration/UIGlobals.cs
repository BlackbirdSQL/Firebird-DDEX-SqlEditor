using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell;


using BlackbirdSql.Common;
using static Microsoft.VisualStudio.VSConstants;


namespace BlackbirdSql.VisualStudio.Ddex.Configuration;



// ---------------------------------------------------------------------------------------------------
//
//								UIGlobals Class
//
// ---------------------------------------------------------------------------------------------------



/// <summary>
/// Manages Globals and Visual Studio Options
/// </summary>
internal class UIGlobals
{

	#region Private Variables

	static UIGlobals _Instance;

	bool _ValidateConfig = false;
	bool _ValidateEdmx = false;

	private readonly DTE _Dte = null;


	#endregion



	// ---------------------------------------------------------------------------------------------------
	//
	#region Constants - UIGlobals
	//
	// ---------------------------------------------------------------------------------------------------



	/// <summary>
	/// The [Project][Solution].Globals global is set to transitory during debug because there seems no way to delete it for testing
	/// other than programmatically. It's a single int32 using binary bitwise for the different status settings
	/// </summary>
#if DEBUG
	const bool G_Persistent = false;
	const string G_Key = "GlobalBlackbirdTransitory"; // For debug
#else
	const bool G_Persistent		= true;
	const string G_Key			= "GlobalBlackbirdPersistent";
#endif

	/// <summary>
	/// For Projects: has been validated (Once it's been validated it's always been validated)
	/// For Solutions: has been loaded and in a validation state if <see cref="G_Valid"/> is false else validated
	/// </summary>
	const int G_Validated = 1;
	/// <summary>
	/// For Projects: Validated project is a valid executable C#/VB app. (Once [in]valid always [in]valid)
	/// For Solutions: Off: Solution has been loaded and is in a validation state. On: Validated
	/// (Only applicable if <see cref="G_Validated"/> is set)
	/// </summary>	
	const int G_Valid = 2;
	/// <summary>
	/// The app.config has the client system.data/DbProviderFactory configured and is good to go. (Once successfully configured always configured)
	/// </summary>
	const int G_DbProviderConfigured = 4;
	/// <summary>
	/// The app.config has the EntityFramework provider services and connection factory configured and is good to go. (Once successfully configured always configured)
	/// </summary>
	const int G_EFConfigured = 8;
	/// <summary>
	/// Existing legacy edmx's have been updated and are good to go. (Once all successfully updated always updated)
	/// </summary>
	const int G_EdmxsUpdated = 16;
	/// <summary>
	///  If at any point in solution projects' validation there was a fail, this is set to true on the solution and the solution Globals
	///  is reset to zero.
	///  Validation on failed solution entities will resume the next time the solution is loaded.
	/// </summary>
	const int G_ValidateFailed = 32;


	#endregion



	// ---------------------------------------------------------------------------------------------------
	//
	#region Property Accessors - UIGlobals
	//
	// ---------------------------------------------------------------------------------------------------



	public bool ValidateConfig
	{
		get
		{
			return _ValidateConfig;
		}
	}

	public bool ValidateEdmx
	{
		get
		{
			return _ValidateEdmx;
		}
	}






	/// <summary>
	/// Get's or sets whether at any point a solution validation failed
	/// </summary>
	public bool IsValidateFailedStatus
	{
		get
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			int value;
			string str;

			try
			{
				if (_Dte.Solution.Globals == null)
				{
					Diag.Dug(true, _Dte.Solution.FullName + ": Solution.Globals is null");
					return false;
				}

				if (_Dte.Solution.Globals.get_VariableExists(G_Key))
				{
					str = (string)_Dte.Solution.Globals[G_Key];
					value = str == "" ? 0 : int.Parse(str);

					return (value & G_ValidateFailed) == G_ValidateFailed;
				}

			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				return false;
			}

			return false;
		}

		set
		{
			Diag.Trace("Setting ValidateFailed");

			ThreadHelper.ThrowIfNotOnUIThread();

			bool exists = false;
			string str;

			int val = 0;

			try
			{
				if (_Dte.Solution.Globals == null)
				{
					NullReferenceException ex = new NullReferenceException(_Dte.Solution.FullName + ": Solution.Globals is null");
					throw ex;
				}


				if (_Dte.Solution.Globals.get_VariableExists(G_Key))
				{
					str = (string)_Dte.Solution.Globals[G_Key];
					val = str == "" ? 0 : int.Parse(str);
					exists = true;
				}

				if (exists)
				{
					if ((val & G_ValidateFailed) == G_ValidateFailed == value)
						return;
				}


				if (value)
					val |= G_ValidateFailed;
				else
					val &= (~G_ValidateFailed);


				_Dte.Solution.Globals[G_Key] = val.ToString();
				if (!exists)
					_Dte.Solution.Globals.set_VariablePersists(G_Key, G_Persistent);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

		}
	}


	#endregion



	// ---------------------------------------------------------------------------------------------------
	//
	#region Constructors / Destructors - UIGlobals
	//
	// ---------------------------------------------------------------------------------------------------



	/// <summary>
	/// UIGlobals .ctor
	/// </summary>
	/// <param name="dte"></param>
	private UIGlobals(DTE dte)
	{
		_Dte = dte;

		try
		{
			_ValidateConfig = VsGeneralOptionModel.Instance.ValidateConfig;
			_ValidateEdmx = VsGeneralOptionModel.Instance.ValidateEdmx;

#if DEBUG
			Diag.EnableTrace = VsDebugOptionModel.Instance.EnableTrace;
			Diag.EnableDiagnostics = VsDebugOptionModel.Instance.EnableDiagnostics;
#else
		Diag.EnableTrace = false;
		Diag.EnableDiagnostics = false;
#endif
			Diag.EnableWriteLog = VsGeneralOptionModel.Instance.EnableWriteLog;
			Diag.LogFile = VsGeneralOptionModel.Instance.LogFile;

			VsGeneralOptionModel.Saved += OnGeneralSettingsSaved;
			VsDebugOptionModel.Saved += OnDebugSettingsSaved;

		}
		catch (Exception ex)
		{
			Diag.Dug(ex, "Failed to retrieve Package settings");
		}

	}


	/// <summary>
	/// Gets the Singleton UIGlobals instance
	/// </summary>
	public static UIGlobals GetInstance(DTE dte)
	{
		return _Instance ??= new(dte);
	}


	#endregion



	// ---------------------------------------------------------------------------------------------------
	//
	#region Methods - UIGlobals
	//
	// ---------------------------------------------------------------------------------------------------



	/// <summary>
	/// Sets a status indicator tagging a solution as previously validated or validated and valid or a project as having been validated and if it is
	/// a valid C#/VB executable
	/// </summary>
	/// <param name="global"></param>
	/// <param name="valid"></param>
	/// <returns>True if the operation was successful else False</returns>
	public bool SetIsValidStatus(Globals global, bool valid)
	{
		// Should never happen
		ThreadHelper.ThrowIfNotOnUIThread();

		bool exists = false;
		int value = 0;
		string str;

		Diag.Trace("Setting IsValid to " + valid.ToString());

		try
		{
			if (global == null)
			{
				Diag.Dug(true, "Globals is null");
				return false;
			}


			if (global.get_VariableExists(G_Key))
			{
				str = (string)global[G_Key];
				value = str == "" ? 0 : int.Parse(str);
				exists = true;
			}

			if (exists)
			{
				if ((value & G_Validated) == G_Validated == valid)
					return true;
			}

			value |= G_Validated;

			if (valid)
				value |= G_Valid;
			else
				value &= (~G_Valid);

			global[G_Key] = value.ToString();
			if (!exists)
				global.set_VariablePersists(G_Key, G_Persistent);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}



	/// <summary>
	/// Sets status indicator tagging a project's app.config as having been validated for the DBProvider
	/// </summary>
	/// <param name="project"></param>
	/// <param name="valid"></param>
	/// <returns>True if the operation was successful else False</returns>
	public bool SetIsValidatedDbProviderStatus(Project project)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		bool exists = false;
		int value = 0;
		string str;

		try
		{
			if (project.Globals == null)
			{
				Diag.Dug(true, project.Name + ": Globals is null");
				return false;
			}


			if (project.Globals.get_VariableExists(G_Key))
			{
				str = (string)project.Globals[G_Key];
				value = str == "" ? 0 : int.Parse(str);
				exists = true;
			}

			if (exists)
			{
				if ((value & G_DbProviderConfigured) == G_DbProviderConfigured == true)
					return true;
			}

			value |= G_DbProviderConfigured;


			project.Globals[G_Key] = value.ToString();
			if (!exists)
				project.Globals.set_VariablePersists(G_Key, G_Persistent);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}



	/// <summary>
	/// Sets status indicator tagging a project's app.config as having been validated for EF.
	/// By definition the app.config will also have been validated for 
	/// </summary>
	/// <param name="project"></param>
	/// <param name="valid"></param>
	/// <returns>True if the operation was successful else False</returns>
	public bool SetIsValidatedEFStatus(Project project)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		bool exists = false;
		int value = 0;
		string str;

		try
		{
			if (project.Globals == null)
			{
				Diag.Dug(true, project.Name + ": Globals is null");
				return false;
			}


			if (project.Globals.get_VariableExists(G_Key))
			{
				str = (string)project.Globals[G_Key];
				value = str == "" ? 0 : int.Parse(str);
				exists = true;
			}

			if (exists
				&& (value & G_EFConfigured) == G_EFConfigured == true
				&& (value & G_DbProviderConfigured) == G_DbProviderConfigured == true)
			{ 
				return true;
			}

			value |= G_EFConfigured;
			value |= G_DbProviderConfigured;


			project.Globals[G_Key] = value.ToString();
			if (!exists)
				project.Globals.set_VariablePersists(G_Key, G_Persistent);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}



	/// <summary>
	/// Sets non-persistent status indicator tagging a project's existing edmx's as having been validated/upgraded
	/// from legacy provider settings
	/// </summary>
	/// <param name="project"></param>
	/// <param name="valid"></param>
	/// <returns>True if the operation was successful else False</returns>
	public bool SetIsUpdatedEdmxsStatus(Project project)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		bool exists = false;
		int value = 0;
		string str;

		try
		{
			if (project.Globals == null)
			{
				Diag.Dug(true, project.Name + ": Project.Globals is null");
				return false;
			}


			if (project.Globals.get_VariableExists(G_Key))
			{
				str = (string)project.Globals[G_Key];
				value = str == "" ? 0 : int.Parse(str);
				exists = true;
			}

			if (exists)
			{
				if ((value & G_EdmxsUpdated) == G_EdmxsUpdated == true)
					return true;
			}

			value |= G_EdmxsUpdated;


			project.Globals[G_Key] = value.ToString();
			if (!exists)
				project.Globals.set_VariablePersists(G_Key, G_Persistent);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}



	/// <summary>
	/// Clears the status indicator of a solution.
	/// </summary>
	/// <param name="solution"></param>
	/// <returns>True if the operation was successful else False</returns>
	public bool ClearValidateStatus()
	{

		ThreadHelper.ThrowIfNotOnUIThread();

		try
		{
			if (_Dte.Solution.Globals == null)
			{
				Diag.Dug(true, _Dte.Solution.FullName + ": Solution.Globals is null");
				return false;
			}

			if (!_Dte.Solution.Globals.get_VariableExists(G_Key))
			{
				return true;
			}

			_Dte.Solution.Globals[G_Key] = 0.ToString();
			_Dte.Solution.Globals.set_VariablePersists(G_Key, G_Persistent);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}






	/// <summary>
	/// Verifies whether or not a solution is in a validation state (or previously validated) or a project has been validated as being valid or not
	/// </summary>
	/// <param name="global"></param>
	/// <returns></returns>
	public bool IsValidatedStatus(Globals global)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int value;
		string str;

		try
		{
			if (global == null)
			{
				Diag.Dug(true, "Globals is null");
				return false;
			}


			if (global.get_VariableExists(G_Key))
			{
				str = (string)global[G_Key];
				value = str == "" ? 0 : int.Parse(str);

				return (value & G_Validated) == G_Validated;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return false;
	}



	/// <summary>
	/// Verifies whether or not a solution has been validated or a project is a valid C#/VB executable. See remarks.
	/// </summary>
	/// <param name="global"></param>
	/// <returns></returns>
	/// <remarks>
	/// Callers must call IsValidatedProjectStatus() before checking if a project is valid otherwise this indicator will be meaningless
	/// </remarks>
	public bool IsValidStatus(Globals global)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int value;
		string str;

		try
		{
			if (global == null)
			{
				Diag.Dug(true, "Globals is null");
				return false;
			}

			if (global.get_VariableExists(G_Key))
			{
				str = (string)global[G_Key];
				value = str == "" ? 0 : int.Parse(str);

				return (value & G_Valid) == G_Valid;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return false;
	}




	/// <summary>
	/// Verifies whether or not a project's App.config was validated for FirebirdSql.Data.FirebirdClient
	/// </summary>
	/// <param name="project"></param>
	/// <returns></returns>
	public bool IsConfiguredDbProviderStatus(Project project)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int value;
		string str;

		try
		{
			if (project.Globals == null)
			{
				Diag.Dug(true, project.Name + ": Project.Globals is null");
				return false;
			}

			if (project.Globals.get_VariableExists(G_Key))
			{
				str = (string)project.Globals[G_Key];
				value = str == "" ? 0 : int.Parse(str);

				return (value & G_DbProviderConfigured) == G_DbProviderConfigured;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return false;
	}



	/// <summary>
	/// Verifies whether or not a project's App.config was validated for EntityFramework.Firebird
	/// </summary>
	/// <param name="project"></param>
	/// <returns></returns>
	public bool IsConfiguredEFStatus(Project project)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int value;
		string str;

		try
		{
			if (project.Globals == null)
			{
				Diag.Dug(true, project.Name + ": Project.Globals is null");
				return false;
			}

			if (project.Globals.get_VariableExists(G_Key))
			{
				str = (string)project.Globals[G_Key];
				value = str == "" ? 0 : int.Parse(str);

				return (value & G_EFConfigured) == G_EFConfigured;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return false;
	}



	/// <summary>
	/// Verifies whether or not a project's existing edmx models were updated from using legacy data providers to current
	/// Firebird Client and EntityFramework providers.
	/// </summary>
	/// <param name="project"></param>
	/// <returns></returns>
	public bool IsUpdatedEdmxsStatus(Project project)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		int value;
		string str;

		try
		{
			if (project.Globals == null)
			{
				Diag.Dug(true, project.Name + ": Project.Globals is null");
				return false;
			}

			if (project.Globals.get_VariableExists(G_Key))
			{
				str = (string)project.Globals[G_Key];
				value = str == "" ? 0 : int.Parse(str);

				return (value & G_EdmxsUpdated) == G_EdmxsUpdated;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return false;
	}


	#endregion



	// ---------------------------------------------------------------------------------------------------
	//
	#region Event handlers - UIGlobals
	//
	// ---------------------------------------------------------------------------------------------------



	/// <summary>
	/// Debug options saved event handler 
	/// </summary>
	/// <param name="e"></param>
	void OnDebugSettingsSaved(VsDebugOptionModel e)
	{
		Diag.EnableTrace = e.EnableTrace;
		Diag.EnableDiagnostics = e.EnableDiagnostics;
	}



	/// <summary>
	/// General options saved event handler
	/// </summary>
	/// <param name="e"></param>
	void OnGeneralSettingsSaved(VsGeneralOptionModel e)
	{
		Diag.EnableWriteLog = e.EnableWriteLog;
		Diag.LogFile = e.LogFile;

		_ValidateConfig = e.ValidateConfig;
		_ValidateEdmx = e.ValidateEdmx;
	}


	#endregion
}