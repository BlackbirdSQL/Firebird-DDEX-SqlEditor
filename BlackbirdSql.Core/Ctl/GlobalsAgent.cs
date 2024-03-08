// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.IO;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Ctl.Interfaces;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.Core.Ctl;

// =========================================================================================================
//											GlobalsAgent Class
//
/// <summary>
/// Manages Globals and propagates Visual Studio Options events. This is instance class of
/// AbstractGlobalsAgent. This class definition deals with updating and reading of flags of _Value as per
/// the flags defined under Constants.
/// </summary>
// =========================================================================================================
public class GlobalsAgent : AbstractGlobalsAgent
{

	// ---------------------------------------------------------------------------------
	#region Constants - GlobalsAgent
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// For Projects: has been validated as a valid project type (Once it's been validated it's always been
	/// validated)
	/// For Solutions: has been loaded and in a validation state if <see cref="G_Valid"/> is false else
	/// validated
	/// </summary>
	const int G_Validated = 1;
	/// <summary>
	/// For Projects: Validated project is a valid executable C#/VB app (Project type). (Once [in]valid always
	/// [in]valid)
	/// For Solutions: Off: Solution has been loaded and is in a validation state. On: Validated
	/// (Only applicable if <see cref="G_Validated"/> is set)
	/// </summary>	
	const int G_Valid = 2;
	/// <summary>
	/// The app.config and all edmxs for a project have been scanned and configured if required. (Once
	/// successfully scanned always scanned)
	/// </summary>
	const int G_Scanned = 4;
	/// <summary>
	/// The app.config has the client system.data/DbProviderFactory configured and is good to go. (Once
	/// successfully configured always configured)
	/// </summary>
	const int G_DbProviderConfigured = 8;
	/// <summary>
	/// The app.config has the EntityFramework provider services and connection factory configured and is
	/// good to go. (Once successfully configured always configured)
	/// </summary>
	const int G_EFConfigured = 16;
	/// <summary>
	/// Existing legacy edmx's have been updated and are good to go. (Once all successfully updated always
	/// updated)
	/// </summary>
	const int G_EdmxsUpdated = 32;
	/// <summary>
	///  If at any point in solution projects' validation there was a fail, this is set to true on the
	///  solution and the solution GlobalsAgent is reset to zero.
	///  Validation on failed solution entities will resume the next time the solution is loaded.
	/// </summary>
	const int G_ValidateFailed = 64;


	#endregion Constants




	// =========================================================================================================
	#region Fields - GlobalsAgent
	// =========================================================================================================


	#endregion Fields




	// =========================================================================================================
	#region Property Accessors - GlobalsAgent
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Verifies whether or not a project's App.config was validated for
	/// FirebirdSql.Data.FirebirdClient
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool IsConfiguredDbProviderStatus => GetFlagStatus(G_DbProviderConfigured);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Verifies whether or not a project's App.config was validated for
	/// EntityFramework.Firebird
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool IsConfiguredEFStatus
	{
		get
		{
			return GetFlagStatus(G_EFConfigured);
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets/Sets a status indicator tagging a project as having been scanned and it's
	/// app.config and edmxs validated.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool IsScannedStatus
	{
		get
		{
			return GetFlagStatus(G_Scanned);
		}
		set
		{
			SetFlagStatus(G_Scanned, value);
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets status indicator tagging a project's app.config as having been validated
	/// for the DBProvider
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool IsValidatedDbProviderStatus
	{
		set
		{
			SetFlagStatus(G_DbProviderConfigured, true);
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets status indicator tagging a project's app.config as having been validated
	/// for EF.
	/// By definition the app.config will also have been validated for 
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool IsValidatedEFStatus
	{
		set
		{
			SetFlagStatus(G_EFConfigured, true, G_DbProviderConfigured, true);
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Verifies whether or not a solution is in a validation state (or previously
	/// validated) or a project has been validated as being valid or not.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool IsValidatedStatus
	{
		get { return GetFlagStatus(G_Validated); }
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets non-persistent status indicator tagging a project's existing edmx's as
	/// having been validated/upgraded from legacy provider settings.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool IsUpdatedEdmxsStatus
	{
		get
		{
			return GetFlagStatus(G_EdmxsUpdated);
		}
		set
		{
			SetFlagStatus(G_EdmxsUpdated, true);
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a boolean indicating whether or not validation flags are persistent.
	/// Validation flags are always persistent in Release builds.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool PersistentValidation
	{
		get { return _PersistentValidation; }
		set { _PersistentValidation = value; }
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The session current Solution Globals instance.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBGlobalsAgent SolutionGlobals
	{
		get { return _SolutionGlobals; }
		set { _SolutionGlobals = value; }
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The <see cref="EnvDTE.DTE.Solution"/> object.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static object SolutionObject => ApcManager.Instance.SolutionObject;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a boolean indicating whether or not a solution may be validated
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ValidateSolution => _ValidateSolution;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a boolean indicating whether or not the app.config may be validated
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ValidateConfig => _ValidateConfig;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a boolean indicating whether or not edmx files may be validated
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ValidateEdmx => _ValidateEdmx;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Get's or sets whether at any point a solution validation failed
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override bool IsValidateFailedStatus
	{
		get
		{
			if (_SolutionGlobals == null)
			{
				ArgumentNullException ex = new("SolutionGlobals is null");
				Diag.Dug(ex);
				throw ex;
			}

			return ((GlobalsAgent)_SolutionGlobals).GetFlagStatus(G_ValidateFailed);
		}

		set
		{
			if (_SolutionGlobals == null)
			{
				ArgumentNullException ex = new("SolutionGlobals is null");
				Diag.Dug(ex);
				throw ex;
			}


			((GlobalsAgent)_SolutionGlobals).SetFlagStatus(G_ValidateFailed, value);
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Verifies whether or not a solution has been validated or a project is a valid
	/// C#/VB executable. See remarks.
	/// For solutions: Sets a status indicator tagging it as previously validated or
	/// validated and valid.
	/// For projects: Sets a status indicator tagging it as previously validated for
	/// it's validity as a valid C#/VB executable.
	/// </summary>
	/// <remarks>
	/// Callers must call IsValidatedProjectStatus() before checking if a project is
	/// valid otherwise this indicator will be meaningless
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public override bool IsValidStatus
	{
		get { return GetFlagStatus(G_Valid); }
		set { SetFlagStatus(G_Validated, true, G_Valid, value); }
	}


	#endregion Property accessors




	// =========================================================================================================
	#region Constructors / Destructors - GlobalsAgent
	// =========================================================================================================


	/// <summary>
	/// Project Globals .ctor.
	/// </summary>
	/// <param name="projectFilePath">Full file path including project type extension</param>
	public GlobalsAgent(string projectFilePath) : base(projectFilePath)
	{
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Solution Globals .ctor.
	/// </summary>
	/// <param name="solution">The <see cref="EnvDTE.DTE.Solution"/> object</param>
	/// <param name="stream">The stream from <see cref="Package.OnLoadOptions" event./></param>
	// ---------------------------------------------------------------------------------
	public GlobalsAgent(Stream stream) :
		base(stream)
	{
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Destructor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void Dispose()
	{
	}

	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - GlobalsAgent
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clears the status indicator of a solution.
	/// </summary>
	/// <param name="solution"></param>
	/// <returns>True if the operation was successful else False</returns>
	// ---------------------------------------------------------------------------------
	public override bool ClearValidateStatus()
	{
		try
		{
			if (_SolutionGlobals == null)
			{
				Diag.StackException("_SolutionGlobals is null");
				return false;
			}

			if (!_SolutionGlobals.VariableExists)
				return true;

			_SolutionGlobals.Value = 0;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets a IBGlobalsAgent indicator flag.
	/// </summary>
	/// <param name="globals"></param>
	/// <param name="flag"></param>
	/// <param name="enabled"></param>
	/// <param name="flag2"></param>
	/// <param name="enabled2"></param>
	/// <returns>True if the operation was successful else False</returns>
	// ---------------------------------------------------------------------------------
	private bool SetFlagStatus(int flag, bool enabled, int flag2 = 0, bool enabled2 = false)
	{
		bool exists = false;
		int value = 0;

		try
		{
			if (VariableExists)
			{
				value = Value;
				exists = true;
			}

			if (exists && (value & flag) != 0 == enabled)
			{
				if (flag2 == 0 || (value & flag2) != 0 == enabled2)
				{
					return true;
				}
			}

			if (enabled)
				value |= flag;
			else
				value &= ~flag;

			if (flag2 != 0)
			{
				if (enabled2)
					value |= flag2;
				else
					value &= ~flag2;
			}

			Value = value;

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Retrieves an indicator flag's status
	/// </summary>
	/// <param name="globals"></param>
	/// <param name="flag"></param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	private bool GetFlagStatus(int flag)
	{
		int value;

		try
		{
			if (VariableExists)
			{
				value = Value;

				return (value & flag) != 0;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		return false;
	}


	#endregion Methods


}