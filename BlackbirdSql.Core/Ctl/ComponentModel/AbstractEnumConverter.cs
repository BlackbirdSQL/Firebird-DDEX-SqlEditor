// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions2.GlobalEnumConverter
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Sys;


namespace BlackbirdSql.Core.Ctl.ComponentModel;

public abstract class AbstractEnumConverter(Type type) : EnumConverter(type), IBAutomatorConverter, IDisposable
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractEnumConverter
	// ---------------------------------------------------------------------------------


	public void Dispose()
	{
		if (_Model != null)
		{
			if (_IsAutomator)
				_Model.AutomatorPropertyValueChangedEvent -= OnAutomatorPropertyValueChanged;
			_Model = null;
		}
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - AbstractEnumConverter
	// =========================================================================================================


	private readonly object _LockLocal = new object();

	IBSettingsModel _Model;
	private string _PropertyName;
	private bool _IsAutomator = false;

	private int _DefaultValue;
	private bool _Building = false;
	private int _BuildValue;
	private PropertyInfo _PropInfo;

	private IDictionary<string, int> _Dependents = null;
	private readonly Dictionary<CultureInfo, Dictionary<string, object>> _LookupTables = [];


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractEnumConverter
	// =========================================================================================================


	private int CurrentValue => (int)_PropInfo.GetValue(_Model);


	#endregion Property accessors




	// =========================================================================================================
	#region Methods - AbstractEnumConverter
	// =========================================================================================================


	private Dictionary<string, object> GetLookupTable(CultureInfo culture)
	{
		culture ??= CultureInfo.CurrentCulture;
		if (!_LookupTables.TryGetValue(culture, out Dictionary<string, object> value))
		{
			lock (_LockLocal)
			{
				// There are two radio button strings per enum value: Selected/Unselected.
				_Building = true;
				value = [];
				foreach (object standardValue in GetStandardValues())
				{
					_BuildValue = (int)standardValue;
					string text = ConvertToString(null, culture, standardValue);
					if (text != null)
						value.Add(text, standardValue);
					_BuildValue = (int)standardValue + 1;
					text = ConvertToString(null, culture, standardValue);
					if (text != null)
						value.Add(text, standardValue);
				}
				_LookupTables.Add(culture, value);
				_Building = false;
			}
		}
		return value;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			lock (_LockLocal)
			{
				Dictionary<string, object> lookupTable = GetLookupTable(culture);
				if (!lookupTable.TryGetValue(value as string, out object value2))
				{
					value2 = base.ConvertFrom(context, culture, value);
				}

				return value2;
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (value is Enum @enum && destinationType == typeof(string))
		{
			lock (_LockLocal)
			{
				RegisterModel(context, (int)value);


				FieldInfo enumFieldInfo = @enum.GetType().GetField(value.ToString());
				IEnumerable<AbstractGlobalizedRadioAttribute> enumList =
					enumFieldInfo.GetCustomAttributes<AbstractGlobalizedRadioAttribute>(false);

				AbstractGlobalizedRadioAttribute[] array = (AbstractGlobalizedRadioAttribute[])enumList;

				if (array.Length != 0)
				{
					bool selected;
					int current = _Building ? 0 : CurrentValue;

					if (!_Building)
						selected = (current == (int)value && current == _DefaultValue);
					else
						selected = (_BuildValue == (int)value);

					return selected ? array[0].SelectedDescription : array[0].UnselectedDescription;
				}
				return value.ToString();
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}


	public bool UpdateReadOnly(object oldValue, object newValue)
	{
		int nOldValue = (int)oldValue;
		int nNewValue = (int)newValue;

		if (nOldValue == nNewValue)
			return false;

		bool result = false;
		bool readOnly;

		PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(_Model);

		foreach (KeyValuePair<string, int> pair in _Dependents)
		{
			PropertyDescriptor descriptor = descriptors[pair.Key];

			if (descriptor.Attributes[typeof(ReadOnlyAttribute)] is ReadOnlyAttribute attr)
			{
				FieldInfo fieldInfo = Reflect.GetFieldInfo(attr, "isReadOnly");

				readOnly = pair.Value < 0 ? nNewValue == -pair.Value : nNewValue != pair.Value;

				// Tracer.Trace($"Automator {_PropertyName} with new value {newValue} updating dependent: {pair.Key} using {pair.Value} from {fld.GetValue(attr)} to {readOnly}.");

				if ((bool)Reflect.GetFieldInfoValue(attr, fieldInfo) != readOnly)
				{
					result = true;
					Reflect.SetFieldInfoValue(attr, fieldInfo, readOnly);
				}
			}
		}

		return result;
	}


	private bool RegisterModel(ITypeDescriptorContext context, int value)
	{
		if (context == null || context.Instance is not IBSettingsModel model)
			return false;

		// The model instance may have changed on the same property between
		// persistent and transient models, which will require a reset.

		if (_Model != null && object.ReferenceEquals(_Model, model))
			return false;

		IBSettingsModel prevModel = null;

		if (_Model != null)
		{
			_Model.Disposed -= OnModelDisposed;
			prevModel = _Model;
		}


		_Model = model;
		_Model.Disposed += OnModelDisposed;

		string name = context.PropertyDescriptor.Name;
		IBModelPropertyWrapper wrapper = model[name];


		_PropertyName = name;
		_DefaultValue = (int)wrapper.DefaultValue;
		_PropInfo = wrapper.PropInfo;

		if (context.PropertyDescriptor.Attributes[typeof(AutomatorAttribute)] is not AutomatorAttribute attr
			|| attr.Automator != null)
		{
			// Tracer.Trace($"Property {name} is not an automator.");
			return true;
		}

		// Tracer.Trace($"Property {name} IS an automator.");

		if (prevModel != null)
			prevModel.AutomatorPropertyValueChangedEvent -= OnAutomatorPropertyValueChanged;

		_IsAutomator = true;
		_Model.AutomatorPropertyValueChangedEvent += OnAutomatorPropertyValueChanged;

		// Tracer.Trace($"Registering automator {_PropertyName}.");

		_Dependents = new Dictionary<string, int>(model.PropertyWrappers.Count);

		foreach (IBModelPropertyWrapper property in model.PropertyWrappers)
		{
			if (property.Automator != null && property.Automator == _PropertyName)
			{
				// Tracer.Trace($"Registering automator dependent for {_PropertyName}: Dependent: {property.PropertyName}.");
				_Dependents.Add(property.PropertyName, property.AutomatorEnableValue);
			}
		}

		UpdateReadOnly(value + 1, value);

		return true;

	}


	#endregion Methods




	// =========================================================================================================
	#region Event Handling - AbstractEnumConverter
	// =========================================================================================================


	public void OnAutomatorPropertyValueChanged(object sender, AutomatorPropertyValueChangedEventArgs e)
	{
		if (e.ChangedItem.PropertyDescriptor.Name != _PropertyName)
			return;

		e.ReadOnlyChanged = UpdateReadOnly((int)e.OldValue, (int)e.ChangedItem.Value);
	}



	public void OnModelDisposed(object sender, EventArgs e)
	{
		_Model = null;
	}


	#endregion Event Handling

}
