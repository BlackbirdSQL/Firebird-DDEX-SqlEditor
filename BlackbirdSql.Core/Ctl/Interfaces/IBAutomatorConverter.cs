// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.ITrace

using BlackbirdSql.Sys;



namespace BlackbirdSql.Core.Ctl.Interfaces;


public interface IBAutomatorConverter
{
	void OnAutomatorPropertyValueChanged(object sender, AutomatorPropertyValueChangedEventArgs e);

	bool UpdateReadOnly(object oldValue, object newValue);
}
