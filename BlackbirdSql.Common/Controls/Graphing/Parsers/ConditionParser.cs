// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ConditionParser
using System.Collections.Generic;
using BlackbirdSql.Common.Controls.Graphing.Gram;

namespace BlackbirdSql.Common.Controls.Graphing.Parsers;

internal sealed class ConditionParser : XmlPlanHierarchyParser
{
	private static ConditionParser conditionParser;

	public new static ConditionParser Instance
	{
		get
		{
			conditionParser ??= new ConditionParser();
			return conditionParser;
		}
	}

	public override IEnumerable<FunctionTypeItem> ExtractFunctions(object parsedItem)
	{
		if (parsedItem is StmtCondTypeCondition condition && condition.UDF != null)
		{
			FunctionType[] uDF = condition.UDF;
			foreach (FunctionType function in uDF)
			{
				yield return new FunctionTypeItem(function, FunctionTypeItem.ItemType.Udf);
			}
			condition.UDF = null;
		}
	}

	private ConditionParser()
	{
	}
}
