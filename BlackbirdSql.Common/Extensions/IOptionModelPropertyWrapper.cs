using Microsoft.VisualStudio.Settings;

namespace BlackbirdSql.Common.Extensions
{
	public interface IOptionModelPropertyWrapper
	{
		bool Load<TOptMdl>(AbstractOptionModel<TOptMdl> baseOptionModel, SettingsStore settingsStore) where TOptMdl : AbstractOptionModel<TOptMdl>, new();

		bool Save<TOptMdl>(AbstractOptionModel<TOptMdl> baseOptionModel, WritableSettingsStore settingsStore) where TOptMdl : AbstractOptionModel<TOptMdl>, new();
	}
}
