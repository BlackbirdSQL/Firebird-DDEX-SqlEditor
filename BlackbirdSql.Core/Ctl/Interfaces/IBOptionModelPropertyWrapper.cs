﻿//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using BlackbirdSql.Core.Ctl.Config;
using Microsoft.VisualStudio.Settings;


namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IBOptionModelPropertyWrapper
{
	bool Load<TOptMdl>(AbstractOptionModel<TOptMdl> baseOptionModel, SettingsStore settingsStore) where TOptMdl : AbstractOptionModel<TOptMdl>, new();

	bool Save<TOptMdl>(AbstractOptionModel<TOptMdl> baseOptionModel, WritableSettingsStore settingsStore) where TOptMdl : AbstractOptionModel<TOptMdl>, new();
}
