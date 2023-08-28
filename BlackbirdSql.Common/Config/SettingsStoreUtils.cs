// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Common.SettingsStore

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;

using BlackbirdSql.Core;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Common.Config;

public static class SettingsStoreUtils
{
	private static IVsWritableSettingsStore _SUserSettings;

	public const string C_SqlStudioBasePath = "\\BlackbirdSql";

	private static IVsWritableSettingsStore UserSettings
	{
		get
		{
			try
			{
				if (_SUserSettings == null)
				{
					((IVsSettingsManager)Package.GetGlobalService(typeof(SVsSettingsManager)))
						.GetWritableSettingsStore((uint)__VsEnclosingScopes.EnclosingScopes_UserSettings,
						out _SUserSettings);
				}
				if (_SUserSettings == null)
				{
					InvalidOperationException ex = new("Could not get global service SVsSettingsManager");
					throw ex;
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}

			return _SUserSettings;
		}
	}

	public static string GetApplicationProjectsFolder()
	{
		try
		{
			IVsSettingsManager vsSettingsManager = (IVsSettingsManager)Package.GetGlobalService(typeof(SVsSettingsManager))
				?? throw new InvalidOperationException("Could not get global service SVsSettingsManager");

			if (Native.Succeeded(vsSettingsManager
				.GetApplicationDataFolder(
				(uint)__VsApplicationDataFolder.ApplicationDataFolder_Documents, out var folderPath)))
			{
				return Path.Combine(folderPath, "Projects");
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		return string.Empty;
	}

	public static string GetFullCollectionPath(string ssdtCollectionPath)
	{
		if (!ssdtCollectionPath.StartsWith("\\", StringComparison.Ordinal))
		{
			ssdtCollectionPath = "\\" + ssdtCollectionPath;
		}

		return "\\SSDT" + ssdtCollectionPath;
	}

	public static void DeleteCollection(string ssdtCollectionPath)
	{
		UserSettings.DeleteCollection(GetFullCollectionPath(ssdtCollectionPath));
	}

	public static bool UserSettingsCollectionExists(string ssdtCollectionPath)
	{
		UserSettings.CollectionExists(GetFullCollectionPath(ssdtCollectionPath), out var pfExists);
		return pfExists != 0;
	}

	public static bool UserSettingsPropertyExists(string ssdtCollectionPath, string propertyName)
	{
		UserSettings.PropertyExists(GetFullCollectionPath(ssdtCollectionPath), propertyName, out var pfExists);
		return pfExists != 0;
	}

	public static void DeleteProperty(string ssdtCollectionPath, string propertyName)
	{
		UserSettings.DeleteProperty(GetFullCollectionPath(ssdtCollectionPath), propertyName);
	}

	public static void EnsureUserSettingsCollectionExists(string ssdtCollectionPath)
	{
		if (!UserSettingsCollectionExists(ssdtCollectionPath))
		{
			UserSettings.CreateCollection(GetFullCollectionPath(ssdtCollectionPath));
		}
	}

	public static uint GetPropertyCount(string ssdtCollectionPath)
	{
		UserSettings.GetPropertyCount(GetFullCollectionPath(ssdtCollectionPath), out var propertyCount);
		return propertyCount;
	}

	public static string GetStringOrDefault(string ssdtCollectionPath, string propertyName, string defaultValue)
	{
		UserSettings.GetStringOrDefault(GetFullCollectionPath(ssdtCollectionPath), propertyName, defaultValue, out var value);
		return value;
	}

	public static NameValueCollection GetCollection(string ssdtCollectionPath, string namePrefix, string valuePrefix)
	{
		NameValueCollection nameValueCollection = new NameValueCollection();
		int num = 0;
		while (true)
		{
			string text = num.ToString(CultureInfo.InvariantCulture);
			string propertyName = namePrefix + "_" + text;
			string propertyName2 = valuePrefix + "_" + text;
			string stringOrDefault = GetStringOrDefault(ssdtCollectionPath, propertyName, null);
			string stringOrDefault2 = GetStringOrDefault(ssdtCollectionPath, propertyName2, null);
			if (string.IsNullOrWhiteSpace(stringOrDefault) || string.IsNullOrWhiteSpace(stringOrDefault2))
			{
				break;
			}

			nameValueCollection.Set(stringOrDefault, stringOrDefault2);
			num++;
		}

		return nameValueCollection;
	}

	public static void SetString(string ssdtCollectionPath, string propertyName, string value)
	{
		EnsureUserSettingsCollectionExists(ssdtCollectionPath);
		UserSettings.SetString(GetFullCollectionPath(ssdtCollectionPath), propertyName, value);
	}

	public static void SetCollection(string ssdtCollectionPath, NameValueCollection collection, string namePrefix, string valuePrefix)
	{
		EnsureUserSettingsCollectionExists(ssdtCollectionPath);
		for (int i = 0; i < collection.Count; i++)
		{
			string text = i.ToString(CultureInfo.InvariantCulture);
			string key = collection.GetKey(i);
			string value = collection.Get(i);
			SetString(ssdtCollectionPath, namePrefix + "_" + text, key);
			SetString(ssdtCollectionPath, valuePrefix + "_" + text, value);
		}
	}

	public static bool GetBoolOrDefault(string ssdtCollectionPath, string propertyName, bool defaultValue)
	{
		UserSettings.GetBoolOrDefault(GetFullCollectionPath(ssdtCollectionPath), propertyName, defaultValue ? 1 : 0, out var value);
		return value != 0;
	}

	public static bool? GetBool(string ssdtCollectionPath, string propertyName)
	{
		bool? result = null;
		int @bool = UserSettings.GetBool(GetFullCollectionPath(ssdtCollectionPath), propertyName, out int value);
		switch (@bool)
		{
			case 0:
				result = value != 0;
				break;
			default:
				Native.ThrowOnFailure(@bool, "Failed to read property");
				break;
			case VSConstants.E_INVALIDARG:
			case 1:
				break;
		}

		return result;
	}

	public static void SetBool(string ssdtCollectionPath, string propertyName, bool value)
	{
		EnsureUserSettingsCollectionExists(ssdtCollectionPath);
		UserSettings.SetBool(GetFullCollectionPath(ssdtCollectionPath), propertyName, value ? 1 : 0);
	}

	public static int GetIntOrDefault(string ssdtCollectionPath, string propertyName, int defaultValue)
	{
		UserSettings.GetIntOrDefault(GetFullCollectionPath(ssdtCollectionPath), propertyName, defaultValue, out var value);
		return value;
	}

	public static void SetInt(string ssdtCollectionPath, string propertyName, int value)
	{
		EnsureUserSettingsCollectionExists(ssdtCollectionPath);
		UserSettings.SetInt(GetFullCollectionPath(ssdtCollectionPath), propertyName, value);
	}
}
