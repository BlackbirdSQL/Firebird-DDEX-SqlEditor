#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.ComponentModel;

using BlackbirdSql.Common.Properties;




namespace BlackbirdSql.Common.Controls.PropertiesWindow;


public class AbstractPropertiesBase : ICustomTypeDescriptor
{
	public AttributeCollection GetAttributes()
	{
		return TypeDescriptor.GetAttributes(GetType());
	}

	public virtual string GetClassName()
	{
		return SharedResx.PropertyWindowCurrentConnectionParameters;
	}

	public string GetComponentName()
	{
		return null;
	}

	public TypeConverter GetConverter()
	{
		return null;
	}

	public EventDescriptor GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(GetType());
	}

	public PropertyDescriptor GetDefaultProperty()
	{
		return TypeDescriptor.GetDefaultProperty(GetType());
	}

	public object GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(GetType(), editorBaseType);
	}

	public EventDescriptorCollection GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(GetType(), attributes);
	}

	public EventDescriptorCollection GetEvents()
	{
		return TypeDescriptor.GetEvents(GetType());
	}

	public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		return TypeDescriptor.GetProperties(GetType(), attributes);
	}

	public PropertyDescriptorCollection GetProperties()
	{
		return TypeDescriptor.GetProperties(GetType());
	}

	public object GetPropertyOwner(PropertyDescriptor pd)
	{
		return this;
	}
}
