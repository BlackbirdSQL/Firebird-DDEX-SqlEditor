//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

namespace BlackbirdSql.Common.Extensions.Options
{
	//
	// Summary:
	//     Enumeration that specifies both the underlying type that is to be stored/retrieved
	//     from the Microsoft.VisualStudio.Settings.SettingsStore and method of type conversion.
	public enum SettingDataType
	{
		//
		// Summary:
		//     Value of the property is persisted in the Microsoft.VisualStudio.Settings.SettingsStore
		//     as a System.String. null strings are converted to an empty string, therefore
		//     will not round-trip. Type conversions, if needed, are performed via System.Convert.ChangeType(System.Object,System.Type,System.IFormatProvider),
		//     using System.Globalization.CultureInfo.InvariantCulture. Types such as System.Single,
		//     System.Double, System.Decimal, and System.Char are stored this way.
		String,
		//
		// Summary:
		//     Value of the property is persisted in the Microsoft.VisualStudio.Settings.SettingsStore
		//     as an System.Int32. Type conversions, if needed, are performed via System.Convert.ChangeType(System.Object,System.Type,System.IFormatProvider),
		//     using System.Globalization.CultureInfo.InvariantCulture. System.Drawing.Color
		//     is converted using To[From]Argb.
		Int32,
		//
		// Summary:
		//     Value of the property is persisted in the Microsoft.VisualStudio.Settings.SettingsStore
		//     as an System.UInt32. Type conversions, if needed, are performed via System.Convert.ChangeType(System.Object,System.Type,System.IFormatProvider),
		//     using System.Globalization.CultureInfo.InvariantCulture.
		UInt32,
		//
		// Summary:
		//     Value of the property is persisted in the Microsoft.VisualStudio.Settings.SettingsStore
		//     as an System.Int64. Type conversions, if needed, are performed via System.Convert.ChangeType(System.Object,System.Type,System.IFormatProvider),
		//     using System.Globalization.CultureInfo.InvariantCulture. System.DateTime is converted
		//     via To[From]Binary, and System.DateTimeOffset is converted via To[From]UnixTimeMilliseconds.
		Int64,
		//
		// Summary:
		//     Value of the property is persisted in the Microsoft.VisualStudio.Settings.SettingsStore
		//     as an System.UInt64. Type conversions, if needed, are performed via System.Convert.ChangeType(System.Object,System.Type,System.IFormatProvider),
		//     using System.Globalization.CultureInfo.InvariantCulture.
		UInt64,
		//
		// Summary:
		//     Value of the property is persisted in the Microsoft.VisualStudio.Settings.SettingsStore
		//     as a System.IO.MemoryStream. Array of System.Byte is wrapped in a System.IO.MemoryStream.
		//     null values are converted to an empty System.IO.MemoryStream, therefore will
		//     not round-trip.
		Binary,
		//
		// Summary:
		//     Value of the property is persisted in the Microsoft.VisualStudio.Settings.SettingsStore
		//     as a System.String. Conversion uses System.Runtime.Serialization.Formatters.Binary.BinaryFormatter,
		//     with the bytes converted to/from a Base64 encoded string, the string value is
		//     what is stored. null values are stored as an empty string.
		Legacy,
		//
		// Summary:
		//     Value of the property is persisted in the Microsoft.VisualStudio.Settings.SettingsStore
		//     as a System.String. The methods Community.VisualStudio.Toolkit.BaseOptionModel`1.SerializeValue(System.Object,System.Type,System.String)
		//     and Community.VisualStudio.Toolkit.BaseOptionModel`1.DeserializeValue(System.String,System.Type,System.String)
		//     are used to convert to and from storage.
		Serialized
	}
}
