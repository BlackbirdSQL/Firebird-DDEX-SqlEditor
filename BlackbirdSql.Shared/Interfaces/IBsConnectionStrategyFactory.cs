// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces.ISqlEditorStrategy

using System;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Model;


namespace BlackbirdSql.Shared.Interfaces;

public interface IBsConnectionStrategyFactory : IDisposable
{
	string DatabaseName { get; }

	bool IsOnline { get; }

	EnEditorMode Mode { get; }

	IBsExtendedCommandHandler ExtendedCommandHandler { get; }

	object MetadataProviderProvider { get; }

	IBsErrorTaskFactory GetErrorTaskFactory();

	ConnectionStrategy CreateConnectionStrategy();
}
