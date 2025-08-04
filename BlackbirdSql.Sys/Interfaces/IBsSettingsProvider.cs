// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.PersistentSettings

using System;
using BlackbirdSql.Sys.Events;


namespace BlackbirdSql.Sys.Interfaces;

public interface IBsSettingsProvider
{
	object this[string name] { get; set; }

	delegate void SettingsSavedDelegate(object sender);


	/// <summary>
	/// Only implemented by packages that have settings models. Whenever a package
	/// settings model is saved it fires the extension's OnSettingsSaved event.
	/// That event handler then requests each package to populate SettingsEventArgs
	/// if it has settings relevant to the model.
	/// PopulateSettingsEventArgs is also called on loading by the extension without
	/// a specific model specified for a universal request for settings.
	/// </summary>
	bool PopulateSettingsEventArgs(ref PropagateSettingsEventArgs e);

	bool PropertyExists(string name);

	/// <summary>
	/// Adds the extension's SettingsSavedDelegate to a package settings models SettingsSavedEvents.
	/// Only implemented by packages that have settings models, ie. are options providers.
	/// </summary>
	void RegisterSettingsEventHandlers(SettingsSavedDelegate onSettingSaved);

	/// <summary>
	/// Updates settings used by a library. This method will be initiated by the
	/// extension package and passed down through the chain of dll's to the Core.
	/// A dll will update settings relevant to itself from here.
	/// IOW these are push notifications of any settings loaded or saved throughout the
	/// extension.
	/// </summary>
	void PropagateSettings(PropagateSettingsEventArgs e);

}
