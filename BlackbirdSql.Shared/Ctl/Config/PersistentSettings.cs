// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Resources;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Events;



namespace BlackbirdSql.Shared.Ctl.Config;


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
public abstract class PersistentSettings : Core.Ctl.Config.PersistentSettings
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - PersistentSettings
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Protected singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected PersistentSettings() : base()
	{
	}

	protected PersistentSettings(bool transient) : base(transient)
	{
	}

	protected PersistentSettings(IDictionary<string, object> transientStore) : base(transientStore)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - PersistentSettings
	// =========================================================================================================


	private int _AssemblyId = -1;

	// Tracking variables
	private static bool _LayoutPropertyChanged = false;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - PersistentSettings
	// =========================================================================================================


	// Editor GeneralSettingsModel

	public static EnIntellisensePolicy EditorIntellisensePolicy => (EnIntellisensePolicy)GetPersistentSetting("EditorGeneralIntellisensePolicy", EnIntellisensePolicy.ActiveOnly);
	public static bool EditorExecuteQueryOnOpen => (bool)GetPersistentSetting("EditorGeneralExecuteQueryOnOpen", true);
	public static EnLanguageService EditorLanguageService => (EnLanguageService)GetPersistentSetting("EditorGeneralLanguageService",
		EnLanguageService.SSDT);
	public static bool EditorPromptSave => (bool)GetPersistentSetting("EditorGeneralPromptSave", true);
	public static bool EditorTtsDefault => (bool)GetPersistentSetting("EditorGeneralTtsDefault", true);



	// Editor TabAndStatusBarSettingsModel

	public static EnExecutionTimeMethod EditorStatusBarExecutionTimeMethod =>
		(EnExecutionTimeMethod)GetPersistentSetting("EditorStatusBarExecutionTimeMethod", EnExecutionTimeMethod.Elapsed);
	public static bool EditorStatusBarIncludeDatabaseName => (bool)GetPersistentSetting("EditorStatusBarIncludeDatabaseName", true);
	public static bool EditorStatusBarIncludeLoginName => (bool)GetPersistentSetting("EditorStatusBarIncludeLoginName", true);
	public static bool EditorStatusBarIncludeRowCount => (bool)GetPersistentSetting("EditorStatusBarIncludeRowCount", true);
	public static bool EditorStatusBarIncludeServerName => (bool)GetPersistentSetting("EditorStatusBarIncludeServerName", true);
	public static Color EditorStatusBarBackgroundColor => (Color)GetPersistentSetting("EditorStatusBarBackgroundColor", SystemColors.Control);
	public static bool EditorStatusTabTextIncludeDatabaseName => (bool)GetPersistentSetting("EditorStatusTabTextIncludeDatabaseName", false);
	public static bool EditorStatusTabTextIncludeLoginName => (bool)GetPersistentSetting("EditorStatusTabTextIncludeLoginName", false);
	public static bool EditorStatusTabTextIncludeFileName => (bool)GetPersistentSetting("EditorStatusTabTextIncludeFileName", true);
	public static bool EditorStatusTabTextIncludeServerName => (bool)GetPersistentSetting("EditorStatusTabTextIncludeServerName", false);



	// Editor ContextSettingsModel
	public static EnStatusBarPosition EditorContextStatusBarPosition =>
		(EnStatusBarPosition)GetPersistentSetting("EditorContextStatusBarPosition", EnStatusBarPosition.Bottom);



	// Tracking properties
	public static bool LayoutPropertyChanged
	{
		get
		{
			return _LayoutPropertyChanged;
		}
		set
		{
			_LayoutPropertyChanged = value;
		}
	}

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



	public static char GetTextDelimiter(object format, object delimiter) =>
		GetTextDelimiter((EnSqlOutputFormat)format, (string)delimiter);


	public static char GetTextDelimiter(EnSqlOutputFormat format, string delimiter)
	{
		switch (format)
		{
			case EnSqlOutputFormat.ColAligned:
				return '\0';
			case EnSqlOutputFormat.CommaDelim:
				return ',';
			case EnSqlOutputFormat.TabDelim:
				return '\t';
			case EnSqlOutputFormat.SpaceDelim:
				return ' ';
			default:
				return delimiter[0];
		}
	}



	/*
	/// <summary>
	/// Adds the extension's SettingsSavedDelegate to a package settings models SettingsSavedEvents.
	/// Only implemented by packages that have settings models, ie. are options providers.
	/// </summary>
	public override void RegisterSettingsEventHandlers(IBsPersistentSettings.SettingsSavedDelegate onSettingsSavedDelegate);


	/// <summary>
	/// Only implemented by packages that have settings models. Whenever a package
	/// settings model is saved it fires the extension's OnSettingsSaved event.
	/// That event handler then requests each package to populate SettingsEventArgs
	/// if it has settings relevant to the model.
	/// PopulateSettingsEventArgs is also called on loading by the extension without
	/// a specific model specified for a universal request for settings.
	/// </summary>
	public override bool PopulateSettingsEventArgs(ref SettingsEventArgs args);
	*/


	#endregion Methods





	// =========================================================================================================
	#region Event handlers - PersistentSettings
	// =========================================================================================================


	/*
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Settings saved event handler - only the final descendent class implements this.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract void OnSettingsSaved(object sender);
	*/


	/// <summary>
	/// Updates settings used by a library. This method will be initiated by the
	/// extension package and passed down through the chain of dll's to the Core.
	/// A dll will update settings relevant to itself from here.
	/// IOW these are push notifications of any settings loaded or saved throughout the
	/// extension and an opportunity to update any live settings.
	/// </summary>
	public override void PropagateSettings(PropagateSettingsEventArgs e)
	{

		if (e.Package == null || e.Package == "Editor")
		{
			if (e.Group == null || e.Group == "TabAndStatusBar")
			{
				if (!_LayoutPropertyChanged && HasSetting("EditorStatusBarIncludeDatabaseName"))
					_LayoutPropertyChanged |= EditorStatusBarIncludeDatabaseName != (bool)e["EditorStatusBarIncludeDatabaseName"];
				if (!_LayoutPropertyChanged && HasSetting("EditorStatusBarIncludeLoginName"))
					_LayoutPropertyChanged |= EditorStatusBarIncludeLoginName != (bool)e["EditorStatusBarIncludeLoginName"];
				if (!_LayoutPropertyChanged && HasSetting("EditorStatusBarIncludeRowCount"))
					_LayoutPropertyChanged |= EditorStatusBarIncludeRowCount != (bool)e["EditorStatusBarIncludeRowCount"];
				if (!_LayoutPropertyChanged && HasSetting("EditorStatusBarIncludeServerName"))
					_LayoutPropertyChanged |= EditorStatusBarIncludeServerName != (bool)e["EditorStatusBarIncludeServerName"];
			}
		}

		base.PropagateSettings(e);

	}


	#endregion Event handlers


}
