//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using Microsoft.VisualStudio.Settings;

namespace BlackbirdSql.Common.Extensions.Options
{
	public interface IOptionModelPropertyWrapper
	{
		bool Load<TOptMdl>(AbstractOptionModel<TOptMdl> baseOptionModel, SettingsStore settingsStore) where TOptMdl : AbstractOptionModel<TOptMdl>, new();

		bool Save<TOptMdl>(AbstractOptionModel<TOptMdl> baseOptionModel, WritableSettingsStore settingsStore) where TOptMdl : AbstractOptionModel<TOptMdl>, new();
	}
}
