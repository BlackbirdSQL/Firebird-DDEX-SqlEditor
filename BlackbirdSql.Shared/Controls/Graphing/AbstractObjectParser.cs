// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ObjectParser
using System;
using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;


namespace BlackbirdSql.Shared.Controls.Graphing;

public abstract class AbstractObjectParser
{
	private static readonly Attribute XmlIgnoreAttribute = new XmlIgnoreAttribute();

	public virtual void ParseProperties(object parsedItem, PropertyDescriptorCollection targetPropertyBag, NodeBuilderContext context)
	{
		PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(parsedItem);
		foreach (PropertyDescriptor item in properties)
		{
			if (item.Attributes.Contains(XmlIgnoreAttribute))
			{
				continue;
			}
			PropertyDescriptor propertyDescriptor2 = properties[item.Name + "Specified"];
			if (propertyDescriptor2 != null && propertyDescriptor2.GetValue(parsedItem).Equals(false))
			{
				continue;
			}
			object value = item.GetValue(parsedItem);
			if (value == null || (Type.GetTypeCode(item.PropertyType) == TypeCode.Object && ShouldSkipProperty(item)))
			{
				continue;
			}
			if (item.Name == "Items" || item.Name == "Item")
			{
				if (value is ICollection collection)
				{
					foreach (object item2 in collection)
					{
						AddProperty(targetPropertyBag, item, item2);
					}
				}
				else
				{
					AddProperty(targetPropertyBag, item, value);
				}
			}
			else
			{
				AddProperty(targetPropertyBag, item, value);
			}
		}
	}

	private static void AddProperty(PropertyDescriptorCollection targetPropertyBag, PropertyDescriptor property, object value)
	{
		PropertyDescriptor propertyDescriptor = PropertyFactory.CreateProperty(property, value);
		if (propertyDescriptor != null)
		{
			targetPropertyBag.Add(propertyDescriptor);
		}
	}

	protected virtual bool ShouldSkipProperty(PropertyDescriptor property)
	{
		return false;
	}
}
