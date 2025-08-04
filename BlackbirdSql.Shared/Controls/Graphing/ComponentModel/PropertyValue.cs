// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.PropertyValue

using System;
using System.Collections;
using System.ComponentModel;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Core.Ctl.ComponentModel;



namespace BlackbirdSql.Shared.Controls.Graphing.ComponentModel;


internal sealed class PropertyValue : PropertyDescriptor
{
	internal sealed class OrderComparer : IComparer
	{
		internal static readonly OrderComparer Default = new OrderComparer();

		int IComparer.Compare(object x, object y)
		{
			return Compare(x as PropertyValue, y as PropertyValue);
		}

		private static int Compare(PropertyValue x, PropertyValue y)
		{
			if (x.IsLongString != y.IsLongString)
			{
				if (!x.IsLongString)
				{
					return -1;
				}
				return 1;
			}
			int num = x?.DisplayOrder ?? 2147483646;
			int num2 = y?.DisplayOrder ?? 2147483646;
			return num - num2;
		}
	}

	private object _Value;

	private string _DisplayName;

	private string _DisplayNameKey;

	private string _Description;

	private string _DescriptionKey;

	private int _DisplayOrder = int.MaxValue;

	private readonly PropertyDescriptor _BaseProperty;

	private bool _IsLongString;

	private bool _Initialized;

	internal object Value
	{
		get
		{
			return _Value;
		}
		set
		{
			_Value = value;
		}
	}

	internal int DisplayOrder
	{
		get
		{
			InitializeDisplayAttributesIfNecessary();
			return _DisplayOrder;
		}
	}

	internal bool IsLongString
	{
		get
		{
			InitializeDisplayAttributesIfNecessary();
			return _IsLongString;
		}
	}

	internal override AttributeCollection Attributes
	{
		get
		{
			if (_BaseProperty == null)
			{
				return base.Attributes;
			}
			return _BaseProperty.Attributes;
		}
	}

	internal override bool IsReadOnly => true;

	internal override Type ComponentType => GetType();

	internal override Type PropertyType
	{
		get
		{
			if (_Value == null)
			{
				return typeof(string);
			}
			return _Value.GetType();
		}
	}

	internal override string DisplayName
	{
		get
		{
			InitializeDisplayAttributesIfNecessary();
			if (_DisplayName != null || _DisplayNameKey != null)
			{
				_DisplayName ??= ControlsResources.ResourceManager.GetString(_DisplayNameKey);
				return _DisplayName;
			}
			return base.DisplayName;
		}
	}

	internal override string Description
	{
		get
		{
			InitializeDisplayAttributesIfNecessary();
			if (_Description != null || _DescriptionKey != null)
			{
				_Description ??= ControlsResources.ResourceManager.GetString(_DescriptionKey);
				return _Description;
			}
			return base.Description;
		}
	}

	public PropertyValue(string name, object value)
		: base(name, null)
	{
		_Value = value;
	}

	public PropertyValue(PropertyDescriptor baseProperty, object value)
		: this(baseProperty.Name, value)
	{
		_BaseProperty = baseProperty;
	}

	internal void SetDisplayNameAndDescription(string newDisplayName, string newDescription)
	{
		_DisplayName = newDisplayName;
		_Description = newDescription;
	}

	internal override bool CanResetValue(object component)
	{
		return false;
	}

	internal override object GetValue(object component)
	{
		return _Value;
	}

	internal override void ResetValue(object component)
	{
	}

	internal override void SetValue(object component, object value)
	{
	}

	internal override bool ShouldSerializeValue(object component)
	{
		return false;
	}

	private void InitializeDisplayAttributesIfNecessary()
	{
		if (_Initialized)
		{
			return;
		}
		_Initialized = true;
		if (Attributes[typeof(DisplayNameDescriptionAttribute)] is DisplayNameDescriptionAttribute displayNameDescriptionAttribute)
		{
			_DisplayNameKey = displayNameDescriptionAttribute.DisplayName;
			_DescriptionKey = displayNameDescriptionAttribute.Description;
			_DescriptionKey ??= _DisplayNameKey;
		}
		if (Attributes[typeof(DisplayOrderAttribute)] is DisplayOrderAttribute displayOrderAttribute)
		{
			_DisplayOrder = displayOrderAttribute.DisplayOrder;
		}
		if (Attributes[typeof(ShowInToolTipAttribute)] is ShowInToolTipAttribute showInToolTipAttribute)
		{
			_IsLongString = showInToolTipAttribute.LongString;
		}
	}
}
