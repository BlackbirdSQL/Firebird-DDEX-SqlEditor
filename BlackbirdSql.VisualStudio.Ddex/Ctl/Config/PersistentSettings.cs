// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdSql.Sys.Events;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.VisualStudio.Ddex.Model.Config;
using BlackbirdSql.VisualStudio.Ddex.Properties;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.Config;

// =============================================================================================================
//										PersistentSettings Class
//
/// <summary>
/// Consolidated single access point for daisy-chained packages settings models (IBsSettingsModel).
/// As a rule we name descendent classes PersistentSettings as well. We hardcode bind the PersistentSettings
/// descendent tree from the top-level extension lib down to Sys.
/// PersistentSettings can be either consumers or providers of options, or both.
/// Property accessors should only be declared at the hierarchy level that they are first required. They do
/// not need to be declared in PeristentSettings if they're only required in TransientSettings.
/// There is no point using services as this hierarchy is fixed. ie:
/// VisualStudio.Ddex > Controller > EditorExtension > LanguageExtension > Shared > Core > Sys.
/// </summary>
// =============================================================================================================
public class PersistentSettings : Controller.Ctl.Config.PersistentSettings
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - PersistentSettings
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Protected singleton .ctor
	/// </summary>
	protected PersistentSettings() : base()
	{
	}


	public static IBsSettingsProvider CreateInstance()
	{
		return new PersistentSettings();
	}


	/// <summary>
	/// Gets the singleton PersistentSettings instance
	/// </summary>
	public new static IBsSettingsProvider Instance => _Instance ??= new PersistentSettings();



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Starts up extension user options push notifications. Only the final class in
	/// the <see cref="IBsAsyncPackage"/> class hierarchy should implement the method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override void Initialize()
	{
		if (_SettingsStore != null)
			return;

		_SettingsStore = [];

		PropagateSettingsEventArgs e = new();

		PopulateSettingsEventArgs(ref e);
		PropagateSettings(e);
		RegisterSettingsEventHandlers(OnSettingsSaved);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - PersistentSettings
	// =========================================================================================================


	private int _AssemblyId = -1;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - PersistentSettings
	// =========================================================================================================


	/// <summary>
	/// Flag indicating whether or not the extrapolated VxbObjectSupport.xml, after
	/// imports, must be saved to the diagnostics logfile directory.
	/// </summary>
	public static bool EnableSaveExtrapolatedXml => (bool)GetPersistentSetting("DdexDebugEnableSaveExtrapolatedXml", false);


	public static bool ValidateSessionConnectionOnFormAccept => (bool)GetPersistentSetting("DdexGeneralValidateSessionConnectionOnFormAccept", true);


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - PersistentSettings
	// =========================================================================================================


	public override int GetEvsAssemblyId(Type type)
	{
		int id = base.GetEvsAssemblyId(type);

		if (id > 0)
			return id;

		--id;

		if (type.Assembly.FullName == typeof(PersistentSettings).Assembly.FullName)
		{
			_AssemblyId = -id;
			return _AssemblyId;
		}

		return id;
	}


	/// <summary>
	/// Adds the extension's SettingsSavedDelegate to a package settings models SettingsSavedEvents.
	/// Only implemented by packages that have settings models, ie. are options providers.
	/// </summary>
	public override void RegisterSettingsEventHandlers(IBsSettingsProvider.SettingsSavedDelegate onSettingsSavedDelegate)
	{
		base.RegisterSettingsEventHandlers(onSettingsSavedDelegate);

		try
		{
			GeneralSettingsModel.SettingsSavedEvent += new Action<GeneralSettingsModel>(onSettingsSavedDelegate);
			DebugSettingsModel.SettingsSavedEvent += new Action<DebugSettingsModel>(onSettingsSavedDelegate);
			EquivalencySettingsModel.SettingsSavedEvent += new Action<EquivalencySettingsModel>(onSettingsSavedDelegate);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, Resources.ExceptionFailedToSubscribeSettingsEvents);
		}
	}


	/// <summary>
	/// Only implemented by packages that have settings models. Whenever a package
	/// settings model is saved it fires the extension's OnSettingsSaved event.
	/// That event handler then requests each package to populate SettingsEventArgs
	/// if it has settings relevant to the model.
	/// PopulateSettingsEventArgs is also called on loading by the extension without
	/// a specific model specified for a universal request for settings.
	/// </summary>
	public override bool PopulateSettingsEventArgs(ref PropagateSettingsEventArgs e)
	{
		bool result = false;

		// Evs.Trace(GetType(), nameof(PopulateSettingsEventArgs), "Package: {0}, Group: {1}.", e.Package, e.Group);

		if (e.Package == null || e.Package != "Ddex")
			result |= base.PopulateSettingsEventArgs(ref e);


		if (e.Package == null || e.Package == "Ddex")
		{
			if (e.Group == null || e.Group == "General")
			{
				e.AddRange(GeneralSettingsModel.Instance);
				result = true;
			}

			if (e.Group == null || e.Group == "Debug")
			{
				e.AddRange(DebugSettingsModel.Instance);
				result = true;
			}

			// Equivalency only on startup.
			if (e.Package == null && e.Group == null /* || e.Group == "Equivalency" */)
			{
				e.AddRange(EquivalencySettingsModel.Instance);
				result = true;
			}
		}

		return result;
	}


	#endregion Methods




	// =========================================================================================================
	#region Event handlers - PersistentSettings
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Settings saved event handler - only the final descendent class implements this.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnSettingsSaved(object sender)
	{
		// Evs.Trace(GetType(), nameof(OnSettingsSaved), "sender: {0}", sender?.GetType().ToString() ?? "Null");

		IBsSettingsModel model = (IBsSettingsModel)sender;

		PropagateSettingsEventArgs e = new(model.SettingsPackage, model.SettingsGroup);

		PopulateSettingsEventArgs(ref e);
		PropagateSettings(e);
	}


	/// <summary>
	/// Updates settings used by a library. This method will be initiated by the
	/// extension package and passed down through the chain of dll's to the Core.
	/// A dll will update settings relevant to itself from here.
	/// IOW these are push notifications of any settings loaded or saved throughout the
	/// extension and an opportunity to update any live settings.
	/// </summary>
	public override void PropagateSettings(PropagateSettingsEventArgs e)
	{
		try
		{
			base.PropagateSettings(e);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
	}


	#endregion Event handlers


}