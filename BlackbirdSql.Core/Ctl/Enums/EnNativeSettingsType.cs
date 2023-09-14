//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System.IO;
using System;

namespace BlackbirdSql.Core.Ctl.Enums;

//
// Summary:
//     Data types of the properties that are stored inside the collections. This mostly
//     mirror Microsoft.VisualStudio.Settings.SettingsType, but adds UInt32 and UInt64
public enum EnNativeSettingsType
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


//
// Summary:
//     Extension methods for Community.VisualStudio.Toolkit.NativeSettingsType.
public static class NativeSettingsTypeExtensions
{
	//
	// Summary:
	//     Get the .NET System.Type based on the method signature for the Microsoft.VisualStudio.Settings.SettingsStore
	//     necessary to retrieve and store data.
	//
	// Parameters:
	//   nativeSettingsType:
	//     The nativeSettingsType to act on.
	//
	// Returns:
	//     The .NET System.Type. Not Null.
	//
	// Exceptions:
	//   T:System.ArgumentOutOfRangeException:
	//     Thrown when one or more arguments are outside the required range.
	public static Type GetDotNetTypeX(this EnNativeSettingsType nativeSettingsType)
	{
		return nativeSettingsType switch
		{
			EnNativeSettingsType.Int32 => typeof(int),
			EnNativeSettingsType.Int64 => typeof(long),
			EnNativeSettingsType.String => typeof(string),
			EnNativeSettingsType.Binary => typeof(MemoryStream),
			EnNativeSettingsType.UInt32 => typeof(uint),
			EnNativeSettingsType.UInt64 => typeof(ulong),
			_ => ((Func<Type>)(() =>
			{
				ArgumentOutOfRangeException exbb = new("nativeSettingsType", nativeSettingsType, null);
				Diag.Dug(exbb);
				throw exbb;
			}))(),
		};
	}
}

