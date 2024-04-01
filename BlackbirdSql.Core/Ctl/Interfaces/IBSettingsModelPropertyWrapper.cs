//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using BlackbirdSql.Core.Model.Config;
using Microsoft.VisualStudio.Settings;


namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IBSettingsModelPropertyWrapper : IBModelPropertyWrapper
{

	bool Load<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, SettingsStore settingsStore) where TOptMdl : AbstractSettingsModel<TOptMdl>;
	bool Load<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, IBTransientSettings transientSettings) where TOptMdl : AbstractSettingsModel<TOptMdl>;

	bool LoadDefault<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel) where TOptMdl : AbstractSettingsModel<TOptMdl>;

	bool Save<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, WritableSettingsStore settingsStore) where TOptMdl : AbstractSettingsModel<TOptMdl>;
	bool Save<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, IBTransientSettings liveSettings) where TOptMdl : AbstractSettingsModel<TOptMdl>;
}
