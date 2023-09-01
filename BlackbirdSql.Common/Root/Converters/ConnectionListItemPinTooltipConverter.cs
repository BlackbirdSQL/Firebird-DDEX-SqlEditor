// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.ConnectionListItemPinTooltipConverter
using System.ComponentModel;
using BlackbirdSql.Common.Properties;

namespace BlackbirdSql.Common.Converters;


[EditorBrowsable(EditorBrowsableState.Never)]
public class ConnectionListItemPinTooltipConverter : PinUnpinConverterBase<string>
{
	public static ConnectionListItemPinTooltipConverter Instance = new ConnectionListItemPinTooltipConverter();

	protected override object ConvertValue(bool isFavorite)
	{
		if (isFavorite)
		{
			return SharedResx.UnpinToolTip;
		}
		return SharedResx.PinToolTip;
	}
}
