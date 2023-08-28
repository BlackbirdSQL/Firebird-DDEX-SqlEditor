// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.ConnectionListItemPinMenuItemConverter
using System.ComponentModel;
using BlackbirdSql.Common.Properties;

namespace BlackbirdSql.Common.Ctl.Converters;


[EditorBrowsable(EditorBrowsableState.Never)]
public class ConnectionListItemPinMenuItemConverter : PinUnpinConverterBase<string>
{
	public static ConnectionListItemPinMenuItemConverter Instance = new ConnectionListItemPinMenuItemConverter();

	protected override object ConvertValue(bool isFavorite)
	{
		if (isFavorite)
		{
			return SharedResx.UnPinMenuAction;
		}
		return SharedResx.PinMenuAction;
	}
}
