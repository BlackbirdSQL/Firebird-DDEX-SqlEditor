//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System;
using BlackbirdSql.Core.Enums;

namespace BlackbirdSql.Core.Ctl.ComponentModel
{
	//
	// Summary:
	//     Apply this attribute on a get/set property in the Community.VisualStudio.Toolkit.BaseOptionModel`1
	//     class to specify the type and mechanism used to store/retrieve the value of this
	//     property in the Microsoft.VisualStudio.Settings.SettingsStore. If not specified,
	//     the default mechanism is used is based on the property type.
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class OverrideDataTypeAttribute : Attribute
	{
		//
		// Summary:
		//     Specifies the type and method used to store and retrieve the value of the attributed
		//     property in the Microsoft.VisualStudio.Settings.SettingsStore.
		public EnSettingDataType SettingDataType { get; }

		//
		// Summary:
		//     If true, and the type has a System.ComponentModel.TypeConverterAttribute that
		//     allows for conversion to Community.VisualStudio.Toolkit.OverrideDataTypeAttribute.SettingDataType,
		//     this will be used to convert and store the property value. If the Community.VisualStudio.Toolkit.OverrideDataTypeAttribute.SettingDataType
		//     is Legacy or Serialized this has no effect. For other Community.VisualStudio.Toolkit.OverrideDataTypeAttribute.SettingDataType,
		//     false will use the default conversion mechanism of System.Convert.ChangeType(System.Object,System.Type,System.IFormatProvider),
		//     using System.Globalization.CultureInfo.InvariantCulture.
		public bool UseTypeConverter { get; }

		//
		// Summary:
		//     Alters the default type and mechanism used to store/retrieve the value of this
		//     property in the Microsoft.VisualStudio.Settings.SettingsStore.
		//
		// Parameters:
		//   settingDataType:
		//     Specifies the type and/or method used to store and retrieve the value of the
		//     attributed property in the Microsoft.VisualStudio.Settings.SettingsStore.
		//
		//   useTypeConverter:
		//     (Optional, default false) If true, and the type has a System.ComponentModel.TypeConverterAttribute
		//     that allows for conversion to settingDataType, this will be used to convert and
		//     store the property value. If the settingDataType is Legacy or Serialized this
		//     has no effect. For other Community.VisualStudio.Toolkit.OverrideDataTypeAttribute.SettingDataType
		//     values, false will use the default conversion mechanism of System.Convert.ChangeType(System.Object,System.Type,System.IFormatProvider),
		//     using System.Globalization.CultureInfo.InvariantCulture.
		public OverrideDataTypeAttribute(EnSettingDataType settingDataType, bool useTypeConverter = false)
		{
			SettingDataType = settingDataType;
			UseTypeConverter = useTypeConverter;
		}
	}
}
