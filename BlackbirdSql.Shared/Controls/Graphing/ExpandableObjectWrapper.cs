// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ExpandableObjectWrapper
using System;
using System.ComponentModel;
using System.Drawing.Design;
using BlackbirdSql.Shared.Controls.Graphing.ComponentModel;

namespace BlackbirdSql.Shared.Controls.Graphing;

[TypeConverter(typeof(ExpandableObjectConverter))]
[Editor(typeof(LongStringUITypeEditor), typeof(UITypeEditor))]
public class ExpandableObjectWrapper : AbstractObjectParser, ICustomTypeDescriptor
{
	private readonly PropertyDescriptorCollection properties;

	private readonly PropertyDescriptor defaultProperty;

	private string displayName;

	public object this[string propertyName]
	{
		get
		{
			if (properties[propertyName] is not PropertyValue propertyValue)
			{
				return null;
			}
			return propertyValue.Value;
		}
		set
		{
			if (properties[propertyName] is PropertyValue propertyValue)
			{
				propertyValue.Value = value;
			}
			else
			{
				properties.Add(new PropertyValue(propertyName, value));
			}
		}
	}

	[Browsable(false)]
	public string DisplayName
	{
		get
		{
			return displayName;
		}
		set
		{
			displayName = value;
		}
	}

	[Browsable(false)]
	public PropertyDescriptorCollection Properties => properties;

	public ExpandableObjectWrapper()
		: this(null, null, string.Empty)
	{
	}

	public ExpandableObjectWrapper(object item)
		: this(item, null)
	{
	}

	public ExpandableObjectWrapper(object item, string defaultPropertyName)
		: this(item, defaultPropertyName, GetDefaultDisplayName(item))
	{
	}

	public ExpandableObjectWrapper(object item, string defaultPropertyName, string displayName)
	{
		properties = new PropertyDescriptorCollection(new PropertyDescriptor[0]);
		if (item != null)
		{
			ParseProperties(item, properties, null);
		}
		if (defaultPropertyName != null)
		{
			defaultProperty = properties[defaultPropertyName];
		}
		this.displayName = displayName;
	}

	public override string ToString()
	{
		return displayName;
	}

	public static string GetDefaultDisplayName(object item)
	{
		string text = item.ToString();
		if (!(text != item.GetType().ToString()))
		{
			return string.Empty;
		}
		return text;
	}

	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return TypeDescriptor.GetAttributes(GetType());
	}

	EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(GetType());
	}

	PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
	{
		return defaultProperty;
	}

	object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(GetType(), editorBaseType);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		return TypeDescriptor.GetEvents(GetType());
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(GetType(), attributes);
	}

	object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
	{
		return this;
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		return properties;
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		return properties;
	}

	string ICustomTypeDescriptor.GetComponentName()
	{
		return null;
	}

	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		return TypeDescriptor.GetConverter(GetType());
	}

	string ICustomTypeDescriptor.GetClassName()
	{
		return GetType().Name;
	}
}
