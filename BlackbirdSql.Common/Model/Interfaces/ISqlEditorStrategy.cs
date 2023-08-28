#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion


using System;

using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Interfaces;




namespace BlackbirdSql.Common.Model.Interfaces;


public interface ISqlEditorStrategy : IDisposable
{
	string DatabaseName { get; }

	bool IsOnline { get; }

	bool IsDw { get; set; }

	IMetadataProviderProvider MetadataProviderProvider { get; }

	EnEditorMode Mode { get; }

	ISqlEditorExtendedCommandHandler ExtendedCommandHandler { get; }

	ISqlEditorErrorTaskFactory GetErrorTaskFactory();

	string ResolveSqlCmdVariable(string variableName);

	SqlConnectionStrategy CreateConnectionStrategy();
}
