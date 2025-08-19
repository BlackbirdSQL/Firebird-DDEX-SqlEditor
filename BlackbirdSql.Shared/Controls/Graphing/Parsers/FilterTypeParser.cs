// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.FilterTypeParser
using System.ComponentModel;
using BlackbirdSql.Shared.Controls.Graphing.ComponentModel;
using BlackbirdSql.Shared.Controls.Graphing.Gram;
using BlackbirdSql.Shared.Properties;

namespace BlackbirdSql.Shared.Controls.Graphing.Parsers;

internal sealed class FilterTypeParser : RelOpBaseTypeParser
{
	private static FilterTypeParser filterTypeParser;

	public new static FilterTypeParser Instance
	{
		get
		{
			filterTypeParser ??= new FilterTypeParser();
			return filterTypeParser;
		}
	}

	private FilterTypeParser()
	{
	}

	internal override void ParseProperties(object parsedItem, PropertyDescriptorCollection targetPropertyBag, NodeBuilderContext context)
	{
		base.ParseProperties(parsedItem, targetPropertyBag, context);
		if ((parsedItem as FilterType).StartupExpression && targetPropertyBag["Predicate"] is PropertyValue propertyValue)
		{
			propertyValue.SetDisplayNameAndDescription(ControlsResources.Graphing_StartupExpressionPredicate, null);
		}
	}
}
