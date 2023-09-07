// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces.ISqlEditorStrategyProvider

using System;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Interfaces;

public interface ISqlEditorStrategyProvider : IDisposable
{
	ISqlEditorStrategy CreateEditorStrategy(string documentMoniker, AuxiliaryDocData auxiliaryDocData);
}
