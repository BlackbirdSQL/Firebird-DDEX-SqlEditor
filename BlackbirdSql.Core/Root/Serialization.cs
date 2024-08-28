// Microsoft.Data.Tools.Utilities, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Schema.Common.StringUtils

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;



namespace BlackbirdSql;


public static class Serialization
{

	/// <summary>
	/// Deserializes a base64 encoded string, then deserializes Base64 using
	/// System.Runtime.Serialization.Formatters.Binary.BinaryFormatter.
	/// </summary>
	/// <param name="serializedString">
	/// The serialized object that was serialized with SerializeBinary64().
	/// Can Be Null.
	/// </param>
	/// <param name="conversionType">
	/// The type to deserialize as.
	/// </param>
	public static object DeserializeBinary64(string serializedString, Type conversionType)
	{
		if (serializedString.Length == 0)
		{
			if (conversionType.IsValueType)
			{
				return Activator.CreateInstance(conversionType);
			}

			return null;
		}

		using MemoryStream serializationStream = new MemoryStream(Convert.FromBase64String(serializedString));
		return new BinaryFormatter().Deserialize(serializationStream);
	}



	/// <summary>
	/// Serializes a string to base64.
	/// </summary>
	/// <param name="value">
	/// The object that is to be serialized. Can Be Null.
	/// </param>
	/// <param name="lowerCase">
	/// Specifies wether string should be converted to lowercase first.
	/// Default is true.
	/// </param>
	public static string Serialize64(string value, bool lowerCase = true)
	{
		if (value == null)
			return "";

		if (lowerCase)
			value = value.ToLowerInvariant();

		byte[] arr = System.Text.Encoding.Default.GetBytes(value);

		// Convert the byte array to a Base64 string

		return Convert.ToBase64String(arr);
	}



	/// <summary>
	/// Serializes using System.Runtime.Serialization.Formatters.Binary.BinaryFormatter,
	/// then converts to a base64 string for storage. Returning an empty string represents
	/// a null object.
	/// </summary>
	/// <param name="value">
	/// The object that is to be serialized. Can Be Null.
	/// </param>
	public static string SerializeBinary64(object value)
	{
		if (value == null)
		{
			return "";
		}

		using MemoryStream memoryStream = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream, value);
		memoryStream.Flush();
		return Convert.ToBase64String(memoryStream.ToArray());
	}


}
