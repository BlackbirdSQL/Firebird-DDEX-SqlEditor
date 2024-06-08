// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.PropertyFactory
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

using BlackbirdSql.Shared.Controls.Graphing.ComponentModel;


namespace BlackbirdSql.Shared.Controls.Graphing;

public static class PropertyFactory
{
	private static readonly PropertyDescriptorCollection Properties = TypeDescriptor.GetProperties(typeof(PropertyFactory));


	public static PropertyDescriptor CreateProperty(PropertyDescriptor property, object value)
	{
		Type type = null;
		if (property.Name == "Items" || property.Name == "Item")
		{
			type = value.GetType();
		}
		if (ObjectWrapperTypeConverter.Default.CanConvertFrom(property.PropertyType))
		{
			value = ObjectWrapperTypeConverter.Default.ConvertFrom(value);
		}
		if (value == null)
		{
			return null;
		}
		PropertyDescriptor propertyDescriptor = Properties[property.Name];
		if (propertyDescriptor != null)
		{
			return new PropertyValue(propertyDescriptor, value);
		}
		IEnumerable enumerable = property.Attributes;
		string name = property.Name;
		if (type != null)
		{
			enumerable = GetAttributeCollectionForChoiceElement(property);
		}
		foreach (object item in enumerable)
		{
			if (item is XmlElementAttribute xmlElementAttribute && !string.IsNullOrEmpty(xmlElementAttribute.ElementName) && (type == null || type.Equals(xmlElementAttribute.Type)))
			{
				name = xmlElementAttribute.ElementName;
				propertyDescriptor = Properties[name];
				if (propertyDescriptor != null)
				{
					return new PropertyValue(propertyDescriptor, value);
				}
			}
		}
		return new PropertyValue(name, value);
	}

	private static IEnumerable GetAttributeCollectionForChoiceElement(PropertyDescriptor property)
	{
		Type componentType = property.ComponentType;
		PropertyInfo property2 = componentType.GetProperty("Items") ?? componentType.GetProperty("Item");
		if (property2 != null)
		{
			return property2.GetCustomAttributes(inherit: true);
		}
		return property.Attributes;
	}

	public static PropertyDescriptor CreateProperty(string propertyName, object value)
	{
		PropertyDescriptor propertyDescriptor = Properties[propertyName];
		if (propertyDescriptor != null)
		{
			return new PropertyValue(propertyDescriptor, value);
		}
		return new PropertyValue(propertyName, value);
	}

}
