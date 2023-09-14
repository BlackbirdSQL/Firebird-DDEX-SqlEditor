// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces.ISqlEditorStrategyProvider

using System;
using BlackbirdSql.Common.Model;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBSqlEditorStrategyProvider : IDisposable
{
	IBSqlEditorStrategy CreateEditorStrategy(string documentMoniker, AuxiliaryDocData auxiliaryDocData);
}
