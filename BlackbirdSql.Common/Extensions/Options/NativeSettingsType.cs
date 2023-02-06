//
// Plagiarized from Community.VisualStudio.Toolkit extension
//


namespace BlackbirdSql.Common.Extensions.Options
{
	//
	// Summary:
	//     Data types of the properties that are stored inside the collections. This mostly
	//     mirror Microsoft.VisualStudio.Settings.SettingsType, but adds UInt32 and UInt64
	public enum NativeSettingsType
	{
		//
		// Summary:
		//     Data type used to store 4 byte (32 bits) properties which are Boolean and Int32.
		//     Note that Boolean is stored 1 byte in the .NET environment but as a property
		//     inside the SettingsStore, it is kept as 4 byte value and any value other than
		//     0 is converted to true and 0 is converted to false. NOTE: In .NET we need to
		//     explicitly use the unsigned methods to successfully store unsigned types. This
		//     enumeration adds Community.VisualStudio.Toolkit.NativeSettingsType.UInt32 for
		//     that purpose.
		Int32 = 1,
		//
		// Summary:
		//     Data type used to store 8 byte (64 bit) properties which are Int64. NOTE: In
		//     .NET we need to explicitly use the unsigned methods to successfully store unsigned
		//     types. This enumeration adds Community.VisualStudio.Toolkit.NativeSettingsType.UInt64
		//     for that purpose.
		Int64,
		//
		// Summary:
		//     Data type used to store the strings.
		String,
		//
		// Summary:
		//     Data type used to store byte streams (arrays).
		Binary,
		//
		// Summary:
		//     Data type used to store 4 byte (32 bits) properties which is UInt32. NOTE: This
		//     value is not in Microsoft.VisualStudio.Settings.SettingsType, but is necessary
		//     so we can use the appropriate methods to successfully store unsigned types.
		UInt32,
		//
		// Summary:
		//     Data type used to store 8 byte (64 bit) properties which is UInt64. NOTE: This
		//     value is not in Microsoft.VisualStudio.Settings.SettingsType, but is necessary
		//     so we can use the appropriate methods to successfully store unsigned types.
		UInt64
	}
}
