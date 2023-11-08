//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System;
using System.ComponentModel;
using System.Reflection;
using BlackbirdSql.Core.Model.Config;
using Microsoft.VisualStudio.Settings;


namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IBSettingsModelPropertyWrapper
{
	PropertyInfo PropInfo { get; }
	string PropertyName { get; }
	string Automator { get; }
	bool IsAutomator { get; }
	bool InvertAutomation { get; }
	int AutomationEnableValue { get; }


	int DisplayOrder { get; }

	object DefaultValue { get; }

	Func<object, object> WrappedPropertyGetMethod { get; }


	bool Load<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, SettingsStore settingsStore) where TOptMdl : AbstractSettingsModel<TOptMdl>;
	bool Load<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, IBLiveSettings liveSettings) where TOptMdl : AbstractSettingsModel<TOptMdl>;

	bool LoadDefault<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel) where TOptMdl : AbstractSettingsModel<TOptMdl>;

	bool Save<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, WritableSettingsStore settingsStore) where TOptMdl : AbstractSettingsModel<TOptMdl>;
	bool Save<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, IBLiveSettings liveSettings) where TOptMdl : AbstractSettingsModel<TOptMdl>;
}
