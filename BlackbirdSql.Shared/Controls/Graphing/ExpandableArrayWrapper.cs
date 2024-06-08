// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ExpandableArrayWrapper
using System.Collections;
using System.Globalization;
using System.Text;


namespace BlackbirdSql.Shared.Controls.Graphing;

public class ExpandableArrayWrapper : ExpandableObjectWrapper
{
	public ExpandableArrayWrapper(ICollection collection)
	{
		PopulateProperties(collection);
	}

	private void PopulateProperties(ICollection collection)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (object item in collection)
		{
			if (!ObjectWrapperTypeConverter.Default.CanConvertFrom(item.GetType()))
			{
				continue;
			}
			object obj = ObjectWrapperTypeConverter.Default.ConvertFrom(item);
			if (obj != null)
			{
				base[GetPropertyName(++num)] = obj;
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
					stringBuilder.Append(" ");
				}
				stringBuilder.Append(obj.ToString());
			}
		}
		base.DisplayName = stringBuilder.ToString();
	}

	public static string GetPropertyName(int index)
	{
		return string.Format(CultureInfo.CurrentCulture, "[{0}]", index);
	}
}
