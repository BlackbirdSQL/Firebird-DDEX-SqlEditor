//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.Settings;


namespace BlackbirdSql.Core.Interfaces;

public interface IBSettingsModelPropertyWrapper : IBsModelPropertyWrapper
{

	bool Load<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, SettingsStore settingsStore) where TOptMdl : AbstractSettingsModel<TOptMdl>;
	bool Load<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, IBsTransientSettings transientSettings) where TOptMdl : AbstractSettingsModel<TOptMdl>;

	bool LoadDefault<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel) where TOptMdl : AbstractSettingsModel<TOptMdl>;

	bool Save<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, WritableSettingsStore settingsStore) where TOptMdl : AbstractSettingsModel<TOptMdl>;
	bool Save<TOptMdl>(AbstractSettingsModel<TOptMdl> baseOptionModel, IBsTransientSettings liveSettings) where TOptMdl : AbstractSettingsModel<TOptMdl>;
}
