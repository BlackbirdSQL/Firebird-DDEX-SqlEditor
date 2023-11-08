// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.IPropertyWindowQueryExecutorInitialize

using BlackbirdSql.Common.Model.QueryExecution;


namespace BlackbirdSql.Common.Controls.Interfaces;

public interface IBPropertyWindowQueryManagerInitialize
{
	bool IsInitialized();

	void Initialize(QueryManager qryMgr);
}
