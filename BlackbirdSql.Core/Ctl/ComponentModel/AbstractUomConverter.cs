// System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// System.Windows.Forms.OpacityConverter
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BlackbirdSql.Core.Controls.Events;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Interfaces;
using BlackbirdSql.Core.Properties;

namespace BlackbirdSql.Core.Ctl.ComponentModel;

// =========================================================================================================
//  									AbstractUomConverter TypeConverter Class
//
/// <summary>
/// Formats a value dependent on culture and minimum / maximum / default values.
/// Also provides user input synonyms for each of these values.
/// Uses LiteralRangeAttribute to identify min, max and unit of measure key.
/// Uses any attribute derived from or equal to DefaultValueAttribute for the default.
/// Refer to the RegisterModel() method for the resource naming conventions for
/// accessing globalized strings in AttributeResources.resx.
/// </summary>
// =========================================================================================================
public abstract class AbstractUomConverter : TypeConverter, IBEditConverter, IDisposable
{
	private IBSettingsModel _Model = null;
	private string _PropertyName = string.Empty;
	private int _Min = int.MinValue, _Max = int.MaxValue;
	private object _DefaultValue, _CurrentValue;
	private int _MinLen = -1, _MaxLen = -1;
	private string _UomKey = null, _UomText = null;
	private string[] _UomTextMin, _UomTextMax = null, _UomTextDefault = null;
	private string _UomFmtMin = null, _UomFmtMax = null, _UomFmtDefault = null;
	private string _UomFmt = null;
	private bool _EditActive = false;



	protected virtual System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;


	object CurrentValue => _CurrentValue;
	object DefaultValue => _DefaultValue;
	string StringValue => (string)_CurrentValue;
	int IntValue => (int)_CurrentValue;
	string StringDefault => (string)_DefaultValue;
	int IntDefault => (int)_DefaultValue;
	bool IsCardinal => _MaxLen < 1;

	public void Dispose()
	{
		if (_Model != null)
		{
			_Model.EditControlGotFocusEvent -= OnEditControlGotFocus;
			_Model.EditControlLostFocusEvent -= OnEditControlLostFocus;
			_Model = null;
		}
	}



	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		// Tracer.Trace("CanConvertFrom: " + context.PropertyDescriptor.Name);
		RegisterModel(context);

		if (sourceType == typeof(string))
			return true;

		return base.CanConvertFrom(context, sourceType);
	}



	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		// Tracer.Trace("ConvertFrom: " + context.PropertyDescriptor.Name);
		RegisterModel(context);

		if (value is string text)
		{
			// Simple synonyms checks
			if (_UomTextMin != null && _UomTextMin.Contains(text, StringComparer.InvariantCultureIgnoreCase))
				text = _Min.ToString(CultureInfo.CurrentCulture);
			else if (_UomTextMax != null && _UomTextMax.Contains(text, StringComparer.InvariantCultureIgnoreCase))
				text = _Max.ToString(CultureInfo.CurrentCulture);
			else if (_UomTextDefault != null && _UomTextDefault.Contains(text, StringComparer.InvariantCultureIgnoreCase))
				text = !IsCardinal ? StringDefault : IntDefault.ToString(CultureInfo.CurrentCulture);

			else if (_UomFmtMin != null && text.Equals(_UomFmtMin, StringComparison.InvariantCultureIgnoreCase))
				text = _Min.ToString(CultureInfo.CurrentCulture);
			else if (_UomFmtMax != null && text.Equals(_UomFmtMax, StringComparison.InvariantCultureIgnoreCase))
				text = _Max.ToString(CultureInfo.CurrentCulture);
			else if (_UomFmtDefault != null && text.Equals(_UomFmtDefault, StringComparison.InvariantCultureIgnoreCase))
				text = !IsCardinal ? StringDefault : IntDefault.ToString(CultureInfo.CurrentCulture);

			else if (!string.IsNullOrEmpty(_UomText))
				text = text.Replace(_UomText, "").Trim();

			if (IsCardinal)
			{
				int num = int.Parse(text, CultureInfo.CurrentCulture);

				if (num < _Min)
					num = _Min;
				if (num > _Max)
					num = _Max;

				return num;
			}
			else
			{
				if (text.Length < _MinLen)
					text = StringDefault;
				else
					text = text[.._MaxLen];
				return text;
			}
		}

		return base.ConvertFrom(context, culture, value);
	}



	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
			Diag.ThrowException(new ArgumentNullException("destinationType"));

		// Tracer.Trace("ConvertTo: " + context.PropertyDescriptor.Name);

		RegisterModel(context);

		if (destinationType == typeof(string))
		{
			_CurrentValue = IsCardinal ? (int)value : (string)value;
			if (string.IsNullOrEmpty(_UomFmt) || _EditActive)
			{
				// Tracer.Trace("Converted To: " + _CurrentValue.ToString(CultureInfo.CurrentCulture));
				return !IsCardinal ? StringValue : IntValue.ToString(CultureInfo.CurrentCulture);
			}
			else
			{
				// Tracer.Trace("Converted To: " + string.Format(_UomFmt, _CurrentValue));
				if (_UomFmtMin != null && IntValue == _Min)
					return _UomFmtMin;
				else if (_UomFmtMax != null && IntValue == _Max)
					return _UomFmtMax;
				else if (_UomFmtDefault != null && CurrentValue == DefaultValue)
					return _UomFmtDefault;
				else
					return string.Format(_UomFmt, CurrentValue);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}


	private void RegisterModel(ITypeDescriptorContext context)
	{

		if (context == null || context.Instance is not IBSettingsModel model)
			return;

		// The model instance may have changed on the same property between
		// persistent and transient models, which will require a reset.

		if (_Model != null && object.ReferenceEquals(_Model, model))
			return;

		if (_Model != null)
		{
			_Model.Disposed -= OnModelDisposed;
			_Model.EditControlGotFocusEvent -= OnEditControlGotFocus;
			_Model.EditControlLostFocusEvent -= OnEditControlLostFocus;
		}


		_Model = model;
		_Model.Disposed += OnModelDisposed;

		_PropertyName = context.PropertyDescriptor.Name;

		// Tracer.Trace(GetType(), "RegisterModel()", "Model: {0}.", _Model.GetType().FullName);

		_Model.EditControlGotFocusEvent += OnEditControlGotFocus;
		_Model.EditControlLostFocusEvent += OnEditControlLostFocus;

		if (context.PropertyDescriptor.Attributes[typeof(LiteralRangeAttribute)] is LiteralRangeAttribute literalAttr)
		{
			_Min = literalAttr.Min;
			_Max = literalAttr.Max;
			_MinLen = literalAttr.MinLen;
			_MaxLen = literalAttr.MaxLen;
			_UomKey = literalAttr.Uom;

			if (!string.IsNullOrEmpty(_UomKey))
			{
				// These are all the values used to convert to and from the current value.

				// The uom text string.
				_UomText = ResMgr.GetString("ConverterUom_" + _UomKey);

				// Synonyms for entering the uom's minimum value.
				string str = ResMgr.GetString("ConverterUomMin_" + _UomKey);
				if (str != null)
					_UomTextMin = str.Split(',');

				// Synonyms for entering the uom's maximum value.
				str = ResMgr.GetString("ConverterUomMax_" + _UomKey);
				if (str != null)
					_UomTextMax = str.Split(',');

				// Synonyms for entering the uom's default value.
				str = ResMgr.GetString("ConverterUomDefault_" + _UomKey);
				if (str != null)
					_UomTextDefault = str.Split(',');

				// The format for displaying the value in the current culture.
				_UomFmt = ResMgr.GetString("ConverterUomFormat_" + _UomKey);

				// The formatted display string of the minimum value in the current culture.
				string fmt = ResMgr.GetString("ConverterUomFormatMin_" + _UomKey);
				if (fmt != null)
					_UomFmtMin = fmt.FmtRes(_Min);

				// The formatted display string of the maximum value in the current culture.
				fmt = ResMgr.GetString("ConverterUomFormatMax_" + _UomKey);
				if (fmt != null)
					_UomFmtMax = fmt.FmtRes(_Max);

				_DefaultValue = (int)model[_PropertyName].DefaultValue;

				// The formatted display string of the default value in the current culture.
				fmt = ResMgr.GetString("ConverterUomFormatDefault_" + _UomKey);
				if (fmt != null)
					_UomFmtDefault = fmt.FmtRes(_Max);

			}
		}
		else if (context.PropertyDescriptor.Attributes[typeof(RangeAttribute)] is RangeAttribute rangeAttr)
		{
			_Min = (int)rangeAttr.Minimum;
			_Max = (int)rangeAttr.Maximum;
		}

	}

	public void OnModelDisposed(object sender, EventArgs e)
	{
		_Model = null;
	}



	public void OnEditControlGotFocus(object sender, EditControlFocusEventArgs e)
	{
		// Tracer.Trace(GetType(), "OnEditControlGotFocus()");

		if (e.SelectionItem.PropertyDescriptor.Name != _PropertyName)
			return;

		if (e.SelectionItem.PropertyDescriptor.Attributes[typeof(ReadOnlyAttribute)] is ReadOnlyAttribute attr)
		{
			if ((bool)Reflect.GetFieldValue(attr, "isReadOnly",
				BindingFlags.NonPublic | BindingFlags.Instance))
			{
				return;
			}
		}

		_EditActive = true;

		e.ValidateValue = true;
		e.Value = !IsCardinal ? StringValue : IntValue.ToString(CultureInfo.CurrentCulture);
	}



	public void OnEditControlLostFocus(object sender, EditControlFocusEventArgs e)
	{
		if (e.SelectionItem.PropertyDescriptor.Name == _PropertyName)
			_EditActive = false;
	}
}
