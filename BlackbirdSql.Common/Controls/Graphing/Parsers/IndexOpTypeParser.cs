// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.IndexOpTypeParser
using System.ComponentModel;
using BlackbirdSql.Common.Controls.Graphing.Gram;

namespace BlackbirdSql.Common.Controls.Graphing.Parsers;

internal sealed class IndexOpTypeParser : RelOpBaseTypeParser
{
	private static IndexOpTypeParser indexOpTypeParser;

	public new static IndexOpTypeParser Instance
	{
		get
		{
			indexOpTypeParser ??= new IndexOpTypeParser();
			return indexOpTypeParser;
		}
	}

	private IndexOpTypeParser()
	{
	}

	private ObjectType GetObjectTypeFromProperties(object parsedItem)
	{
		ObjectType result = null;
		PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(parsedItem)["Object"];
		if (propertyDescriptor != null)
		{
			object value = propertyDescriptor.GetValue(parsedItem);
			if (value != null && value is ObjectType[] array && array.Length == 1)
			{
				result = array[0];
			}
		}
		return result;
	}

	private void AddIndexKindAsPhysicalOperatorKind(ObjectType objectType, PropertyDescriptorCollection targetPropertyBag)
	{
		if (objectType.IndexKindSpecified && 0 < objectType.IndexKind.ToString().Length)
		{
			PropertyDescriptor propertyDescriptor = PropertyFactory.CreateProperty("PhysicalOperationKind", objectType.IndexKind.ToString());
			if (propertyDescriptor != null)
			{
				targetPropertyBag.Add(propertyDescriptor);
			}
		}
	}

	public override void ParseProperties(object parsedItem, PropertyDescriptorCollection targetPropertyBag, NodeBuilderContext context)
	{
		base.ParseProperties(parsedItem, targetPropertyBag, context);
		if (parsedItem is IndexScanType || parsedItem is CreateIndexType)
		{
			ObjectType objectTypeFromProperties = GetObjectTypeFromProperties(parsedItem);
			if (objectTypeFromProperties != null)
			{
				AddIndexKindAsPhysicalOperatorKind(objectTypeFromProperties, targetPropertyBag);
			}
		}
	}
}
