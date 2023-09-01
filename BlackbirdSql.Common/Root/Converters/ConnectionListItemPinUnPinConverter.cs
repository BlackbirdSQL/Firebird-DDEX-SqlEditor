// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.ConnectionListItemPinUnPinConverter

using System.ComponentModel;
using System.Windows.Media;
using BlackbirdSql.Core;

namespace BlackbirdSql.Common.Converters;


[EditorBrowsable(EditorBrowsableState.Never)]
public class ConnectionListItemPinUnPinConverter : PinUnpinConverterBase<ImageSource>
{
	public static ConnectionListItemPinUnPinConverter Instance = new ConnectionListItemPinUnPinConverter();

	protected override object ConvertValue(bool isFavorite)
	{
		if (isFavorite)
		{
			return CoreIconsCollection.Instance.GetImage(CoreIconsCollection.Instance.Pushpin_16);
		}
		return CoreIconsCollection.Instance.GetImage(CoreIconsCollection.Instance.PushpinUnpin_16);
	}
}
