// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.MergeTypeParser
using System.ComponentModel;
using BlackbirdSql.Shared.Controls.Graphing.Gram;



namespace BlackbirdSql.Shared.Controls.Graphing.Parsers;

internal sealed class MergeTypeParser : RelOpBaseTypeParser
{
	private static MergeTypeParser mergeTypeParser;

	public new static MergeTypeParser Instance
	{
		get
		{
			mergeTypeParser ??= new MergeTypeParser();
			return mergeTypeParser;
		}
	}

	private MergeTypeParser()
	{
	}

	internal override void ParseProperties(object parsedItem, PropertyDescriptorCollection targetPropertyBag, NodeBuilderContext context)
	{
		base.ParseProperties(parsedItem, targetPropertyBag, context);
		object value = ObjectWrapperTypeConverter.Convert(new MergeColumns(parsedItem as MergeType));
		PropertyDescriptor propertyDescriptor = PropertyFactory.CreateProperty("WhereJoinColumns", value);
		if (propertyDescriptor != null)
		{
			targetPropertyBag.Add(propertyDescriptor);
		}
	}

	protected override bool ShouldSkipProperty(PropertyDescriptor property)
	{
		if (property.Name == "InnerSideJoinColumns" || property.Name == "OuterSideJoinColumns")
		{
			return true;
		}
		return base.ShouldSkipProperty(property);
	}
}
