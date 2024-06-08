// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.MemGrantRunTimeCounters
using System.ComponentModel;
using System.Globalization;


namespace BlackbirdSql.Shared.Controls.Graphing;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class MemGrantRunTimeCounters : RunTimeCounters
{
	public override string ToString()
	{
		ulong num = base.TotalCounters;
		if (base.NumOfCounters > 1)
		{
			foreach (Counter counter in counters)
			{
				if (counter.Thread == 0)
				{
					num -= counter.Value;
					break;
				}
			}
		}
		return num.ToString(CultureInfo.CurrentCulture);
	}
}
