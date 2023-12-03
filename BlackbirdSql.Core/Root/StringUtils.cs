// Microsoft.Data.Tools.Utilities, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Schema.Common.StringUtils

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Properties;
namespace BlackbirdSql.Core;

public static class StringUtils
{


	/// <summary>
	/// Converts a base64 encoded string to it's decoded string value.
	/// </summary>
	/// <param name="serializedString">
	/// The serialized string that was serialized by Serialize64().
	/// Can Be Null.
	/// </param>
	public static string Deserialize64(string value64)
	{
		if (string.IsNullOrWhiteSpace(value64))
			return string.Empty;

		byte[] arr = Convert.FromBase64String(value64);


		return System.Text.Encoding.Default.GetString(arr);
	}


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
			return string.Empty;

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
			return string.Empty;
		}

		using MemoryStream memoryStream = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream, value);
		memoryStream.Flush();
		return Convert.ToBase64String(memoryStream.ToArray());
	}



	public static string StripCR(string str)
	{
		if (str == null)
		{
			ArgumentNullException ex = new("str");
			Diag.Dug(ex);
			throw ex;
		}

		return str.Replace("\r", string.Empty);
	}

	public static int MultiLineCompare(string x, string y, StringComparison mode)
	{
		return string.Compare(StripCR(x), StripCR(y), mode);
	}

	public static bool MultiLineEquals(string x, string y, StringComparison mode)
	{
		return string.Equals(StripCR(x), StripCR(y), mode);
	}

	public static bool EmptyOrSpace(string str)
	{
		if (str != null)
		{
			return 0 >= str.Trim().Length;
		}

		return true;
	}

	public static bool NotEmptyAfterTrim(string str)
	{
		return !EmptyOrSpace(str);
	}

	public static bool EqualValue(string str1, string str2)
	{
		if (str1 != null && str2 != null)
		{
			return string.Compare(str1, str2, StringComparison.Ordinal) == 0;
		}

		return str1 == str2;
	}

	public static string CommentOut(string str)
	{
		if (str != null)
		{
			return "/*" + str + "*/";
		}

		return str;
	}

	public static bool IsSqlVariable(string variable)
	{
		if (variable.Length > 3 && variable.StartsWith("$(", StringComparison.Ordinal))
		{
			return variable.EndsWith(")", StringComparison.Ordinal);
		}

		return false;
	}

	public static string ExtractNameOfVariable(string variable)
	{
		return variable[2..^1];
	}

	public static List<Tuple<string, int>> GetVariablesFromString(string value)
	{
		List<Tuple<string, int>> list = [];
		if (!string.IsNullOrEmpty(value))
		{
			int num = 0;
			while (num < value.Length)
			{
				int num2 = value.IndexOf("$(", num, StringComparison.CurrentCultureIgnoreCase);
				if (num2 < 0)
				{
					break;
				}

				int num3 = value.IndexOf(")", num2 + 2, StringComparison.CurrentCultureIgnoreCase);
				if (num3 < num2 + 2)
				{
					break;
				}

				list.Add(new Tuple<string, int>(value.Substring(num2 + 2, num3 - num2 - 2), num2));
				num = num3 + 1;
			}
		}

		return list;
	}

	public static bool TryToParseCommandLineArguments(string arguments, out IList<KeyValuePair<string, string>> parameters, out string errorMessage)
	{
		arguments += " ";
		parameters = new List<KeyValuePair<string, string>>();
		errorMessage = string.Empty;
		Match match = Regex.Match(arguments, "\\s+", RegexOptions.CultureInvariant);
		int num = 0;
		while (match.Success)
		{
			if (match.Index > num)
			{
				string text = arguments[num..match.Index];
				text = text.TrimStart();
				num = arguments.IndexOf(text, num, StringComparison.Ordinal);
				if (text.StartsWith("\"", StringComparison.Ordinal))
				{
					int num2 = arguments.IndexOf('"', num + 1);
					if (num2 == -1)
					{
						errorMessage = HashLog.FormatHashed(CultureInfo.CurrentCulture, Resources.ArgumentParsing_DoubleQuoteMissingError);
						return false;
					}

					num++;
					text = arguments[num..num2];
					num = num2 + 1;
				}
				else
				{
					num = match.Index + match.Length;
				}

				if (text.StartsWith("/", StringComparison.OrdinalIgnoreCase))
				{
					parameters.Add(new KeyValuePair<string, string>(text, string.Empty));
				}
				else if (parameters.Count != 0)
				{
					parameters[parameters.Count - 1] = new KeyValuePair<string, string>(parameters[parameters.Count - 1].Key, text);
				}
			}

			match = match.NextMatch();
		}

		return true;
	}


	public static Encoding GetDacEncoding()
	{
		return Encoding.UTF8;
	}

	public static byte[] GetRandomBytes(int count)
	{
		using RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		byte[] array = new byte[count];
		rNGCryptoServiceProvider.GetBytes(array);
		return array;
	}

	public static string GeneratePassword(string userName)
	{
		if (userName == null)
		{
			return GeneratePassword();
		}

		int num = 5;
		string[] array = userName.Split(new char[6] { ',', '.', '\t', '-', '_', '#' }, StringSplitOptions.RemoveEmptyEntries);
		while (num-- > 0)
		{
			string text = GeneratePassword();
			if (userName.Length >= 3 && text.Contains(userName))
			{
				continue;
			}

			bool flag = true;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (text2.Length >= 3 && text.Contains(text2))
				{
					flag = false;
					break;
				}
			}

			if (flag)
			{
				return text;
			}
		}

		return GeneratePassword();
	}

	public static string GeneratePassword()
	{
		byte[] randomBytes = GetRandomBytes(48);
		Random random = new Random(Environment.TickCount);
		byte[] array = "msFT7_&#$!~<"u8.ToArray();
		Array.Copy(array, 0, randomBytes, randomBytes.GetLength(0) / 2, array.GetLength(0));
		StringBuilder stringBuilder = new StringBuilder();

		List<char> list = ['\'', '-', '*', '/', '\\', '"', '[', ']', ')', '(' ];

		for (int i = 0; i < randomBytes.GetLength(0); i++)
		{
			if (randomBytes[i] == 0)
			{
				randomBytes[i]++;
			}

			char c = Convert.ToChar(randomBytes[i]);
			if (c < ' ' || c > '~' || list.Contains(c))
			{
				c = (char)(97 + random.Next(0, 28));
			}

			stringBuilder.Append(c);
		}

		return stringBuilder.ToString();
	}

	[DebuggerStepThrough]
	public static string StripOffBrackets(string name)
	{
		if (!string.IsNullOrEmpty(name) && name.Length > 2)
		{
			int num = (name.StartsWith("[", StringComparison.Ordinal) ? 1 : 0);
			int num2 = (name.EndsWith("]", StringComparison.Ordinal) ? 1 : 0);
			if (num > 0 || num2 > 0)
			{
				name = name[num..^num2];
			}
		}

		return name;
	}

	public static bool HasMsBuildDelimiters(string name)
	{
		if (!string.IsNullOrEmpty(name) && name.Length > 3)
		{
			int num = (name.StartsWith("$(", StringComparison.Ordinal) ? 2 : 0);
			int num2 = (name.EndsWith(")", StringComparison.Ordinal) ? 1 : 0);
			if (num > 0 || num2 > 0)
			{
				return true;
			}
		}

		return false;
	}

	[DebuggerStepThrough]
	public static string StripOffMsBuildDelimiters(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return name;
		}

		if (name.Length > 3)
		{
			name = name.Trim();
			int num = (name.StartsWith("$(", StringComparison.Ordinal) ? 2 : 0);
			int num2 = (name.EndsWith(")", StringComparison.Ordinal) ? 1 : 0);
			if (num > 0 || num2 > 0)
			{
				name = name[num..^num2];
			}
		}

		return name.Trim();
	}

	public static string EnsureIsMsBuildDelimited(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return name;
		}

		name = name.Trim();
		bool flag = !name.StartsWith("$(", StringComparison.Ordinal);
		bool flag2 = !name.EndsWith(")", StringComparison.Ordinal);
		if (flag || flag2)
		{
			name = (flag ? "$(" : string.Empty) + name + (flag2 ? ")" : string.Empty);
		}

		return name;
	}

	public static string EscapeDatabaseName(string database)
	{
		if (string.IsNullOrEmpty(database))
		{
			return database;
		}

		return database.Replace("'", "''");
	}

	public static string EscapeSqlCmdVariable(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}

		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in value)
		{
			if (c == '"' || c == ']')
			{
				stringBuilder.Append(c);
			}

			stringBuilder.Append(c);
		}

		return stringBuilder.ToString();
	}

	public static string SQLString(string value, bool unicode)
	{
		return SQLString(value, 0, value.Length, unicode);
	}

	public unsafe static string SQLString(string value, int start, int length, bool unicode)
	{
		bool flag = true;
		int num = 0;
		if (value.Length == 0)
		{
			return (unicode ? "N" : "") + "''";
		}

		fixed (char* ptr = value)
		{
			for (int i = start; i < start + length; i++)
			{
				if (ptr[i] == '\'')
				{
					num++;
					flag = false;
				}

				if (ptr[i] == '\\' && i < start + length - 2 && ptr[i + 1] == '\r' && ptr[i + 2] == '\n')
				{
					num += 3;
					flag = false;
				}

				num++;
			}
		}

		if (flag)
		{
			return (unicode ? "N" : "") + "'" + value.Substring(start, length) + "'";
		}

		string text = new string(' ', num + (unicode ? 3 : 2));
		int num2 = 0;
		fixed (char* ptr3 = value)
		{
			fixed (char* ptr2 = text)
			{
				if (unicode)
				{
					ptr2[num2++] = 'N';
				}

				ptr2[num2++] = '\'';
				for (int j = start; j < start + length; j++)
				{
					if (ptr3[j] == '\'')
					{
						ptr2[num2++] = '\'';
					}

					if (ptr3[j] == '\\' && j < start + length - 2 && ptr3[j + 1] == '\r' && ptr3[j + 2] == '\n')
					{
						ptr2[num2++] = '\\';
						ptr2[num2++] = '\\';
						ptr2[num2++] = '\r';
						ptr2[num2++] = '\n';
						ptr2[num2++] = '\r';
						ptr2[num2++] = '\n';
						j += 2;
					}
					else
					{
						ptr2[num2++] = ptr3[j];
					}
				}

				ptr2[num2] = '\'';
			}
		}

		return text;
	}
}
