// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces.ISqlEditorStrategy


using System;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Model;

namespace BlackbirdSql.Common.Interfaces;


public interface ISqlEditorStrategy : IDisposable
{
	string DatabaseName { get; }

	bool IsOnline { get; }

	bool IsDw { get; }

	IMetadataProviderProvider MetadataProviderProvider { get; }

	EnEditorMode Mode { get; }

	ISqlEditorExtendedCommandHandler ExtendedCommandHandler { get; }

	ISqlEditorErrorTaskFactory GetErrorTaskFactory();

	string ResolveSqlCmdVariable(string variableName);

	SqlConnectionStrategy CreateConnectionStrategy();
}
