﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.IPropertyWindowQueryExecutorInitialize

using BlackbirdSql.Shared.Ctl;



namespace BlackbirdSql.Shared.Interfaces;


public interface IBsConnectedPropertiesWindow
{
	bool IsInitialized();

	void Initialize(QueryManager qryMgr);
}
