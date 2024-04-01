// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces.ISqlEditorStrategy

using System;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Model;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBSqlEditorStrategy : IDisposable
{
	string DatabaseName { get; }

	bool IsOnline { get; }

	bool IsDw { get; }


	EnEditorMode Mode { get; }

	IBSqlEditorExtendedCommandHandler ExtendedCommandHandler { get; }

	object MetadataProviderProvider {  get; }

	IBSqlEditorErrorTaskFactory GetErrorTaskFactory();

	string ResolveSqlCmdVariable(string variableName);

	SqlConnectionStrategy CreateConnectionStrategy();
}
