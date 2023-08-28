//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using BlackbirdSql.Core.Options;
using Microsoft.VisualStudio.Settings;

namespace BlackbirdSql.Core.Interfaces
{
	public interface IBOptionModelPropertyWrapper
	{
		bool Load<TOptMdl>(AbstractOptionModel<TOptMdl> baseOptionModel, SettingsStore settingsStore) where TOptMdl : AbstractOptionModel<TOptMdl>, new();

		bool Save<TOptMdl>(AbstractOptionModel<TOptMdl> baseOptionModel, WritableSettingsStore settingsStore) where TOptMdl : AbstractOptionModel<TOptMdl>, new();
	}
}
