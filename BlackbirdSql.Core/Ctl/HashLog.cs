// Microsoft.Data.Tools.Utilities, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Schema.Common.HashLog

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.Core.Ctl;


internal static class HashLog
{
	private const int C_MinHashBytes = 8;

	private const string C_NullStringValue = "null";

	// Defaults from Microsoft.Data.Tools.Schema.Common.LogSettings
	private static readonly bool HashObjectNamesInLogs = false;
	private static readonly byte[] HashLogSalt = new byte[0];
	private static readonly ConcurrentDictionary<string, string> HashedObjectsCache = new ConcurrentDictionary<string, string>();
	private static readonly SHA256 HashAlgorithm = SHA256.Create();
	private static readonly Action<string, string> LogObjectHashedHandler = null;

	internal static string Format(IFormatProvider formatProvider, string stringFormat, params object[] args)
	{
		return string.Format(formatProvider, stringFormat, args);
	}

	internal static string FormatHashed(IFormatProvider formatProvider, string stringFormat, params object[] args)
	{
		if (!HashObjectNamesInLogs || args == null || args.Length == 0)
		{
			return string.Format(formatProvider, stringFormat, args);
		}

		object[] array = new object[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			array[i] = FormatObject(args[i]);
		}

		return string.Format(formatProvider, stringFormat, array);
	}

	internal static string Join(string separator, params object[] args)
	{
		if (!HashObjectNamesInLogs)
		{
			return string.Join(separator, args);
		}

		if (args == null || args.Length == 0)
		{
			return string.Empty;
		}

		object[] array = new object[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			array[i] = FormatObject(args[i]);
		}

		return string.Join(separator, array);
	}

	internal static string Join(string separator, IEnumerable<object> argsList)
	{
		if (argsList == null)
		{
			return string.Empty;
		}

		return Join(separator, argsList.ToArray());
	}


	internal static string FormatObjectName(string objectName)
	{
		if (HashObjectNamesInLogs)
		{
			return HashLogObject(LogObject.CreateSensitiveData(objectName));
		}

		return objectName;
	}

	internal static string FormatFileName(string fileName)
	{
		if (HashObjectNamesInLogs)
		{
			return Path.Combine(HashLogObject(LogObject.CreateSensitiveData(Path.GetDirectoryName(fileName))), Path.GetFileName(fileName));
		}

		return fileName;
	}

	private static string FormatObject(object original)
	{
		if (original == null)
		{
			return C_NullStringValue;
		}

		Type type = original.GetType();
		if (type == typeof(LogObject))
		{
			return HashLogObject(original as LogObject);
		}

		if (type == typeof(string) || type == typeof(string))
		{
			string text = (string)original;
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}

			return HashLogObject(LogObject.CreateSensitiveData(text));
		}

		return original.ToString();
	}

	private static string HashLogObject(LogObject logObject)
	{
		if (logObject.ShouldMask())
		{
			return HashedObjectsCache.GetOrAdd(logObject.Name, GenerateHashString);
		}
		return logObject.Name;
	}

	private static string GenerateHashString(string original)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(original);
		bytes = [.. bytes, .. HashLogSalt];
		byte[] array = HashAlgorithm.ComputeHash(bytes);
		int val = Math.Max(C_MinHashBytes, original.Length / 2);
		val = Math.Min(val, array.Length);
		StringBuilder stringBuilder = new StringBuilder(val * 2);
		for (int i = 0; i < val; i++)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", array[i]);
		}

		string text = stringBuilder.ToString();
		LogObjectHashedHandler?.Invoke(original, text);

		return text;
	}

	internal static byte[] GenerateSalt()
	{
		return StringUtils.GetRandomBytes(32);
	}
}
