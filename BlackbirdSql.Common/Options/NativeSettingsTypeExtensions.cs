//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System;
using System.IO;

namespace BlackbirdSql.Common.Options
{
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
		public static Type GetDotNetTypeX(this NativeSettingsType nativeSettingsType)
		{
			return nativeSettingsType switch
			{
				NativeSettingsType.Int32 => typeof(int),
				NativeSettingsType.Int64 => typeof(long),
				NativeSettingsType.String => typeof(string),
				NativeSettingsType.Binary => typeof(MemoryStream),
				NativeSettingsType.UInt32 => typeof(uint),
				NativeSettingsType.UInt64 => typeof(ulong),
				_ => ((Func<Type>)(() =>
					{
						ArgumentOutOfRangeException exbb = new("nativeSettingsType", nativeSettingsType, null);
						Diag.Dug(exbb);
						throw exbb;
					}))(),
			};
		}
	}
}
