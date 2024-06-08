// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.IPropertyWindowQueryExecutorInitialize

using BlackbirdSql.Shared.Ctl.QueryExecution;

namespace BlackbirdSql.Shared.Interfaces;

public interface IBPropertyWindowQueryManagerInitialize
{
	bool IsInitialized();

	void Initialize(QueryManager qryMgr);
}
