// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.IListViewHelperFilterProvider

using System.ComponentModel;


namespace BlackbirdSql.Common.Ctl.Interfaces;

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IBListViewHelperFilterProvider
{
	bool FilterItem(object item, string filterText);

	void StartFilterPass();

	void FilterPassComplete();

	void FilterCleared();
}
