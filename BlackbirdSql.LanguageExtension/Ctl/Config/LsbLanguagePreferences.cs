// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.SqlLanguagePreferences
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Package;
using Microsoft.Win32;


namespace BlackbirdSql.LanguageExtension.Ctl.Config;

[ComVisible(true)]
[Guid(PackageData.C_LanguagePreferencesGuid)]
public class LsbLanguagePreferences : LanguagePreferences
{

	public LsbLanguagePreferences(IServiceProvider site, Guid langsvc, string name)
		: base(site, langsvc, name)
	{
		SetDefaults();
	}


	public const int C_ValueUpperCase = 0;
	public const int C_ValueLowerCase = 1;
	private const bool C_AutoOutlining = true;
	private const bool C_UnderlineErrors = true;
	private const bool C_EnableIntellisense = true;
	private const int C_TextCasing = 0;
	private const int C_DefaultMaxScriptSize = 1048576;
	private const bool C_DefaultEnableAzureIntellisense = true;

	private const bool C_DefaultLineNumbers = true;
	private const bool C_DefaultWordWrap = true;
	private const bool C_DefaultWordWrapGlyphs = true;



	private bool _UnderlineErrors;
	private int _MaxScriptSize;
	private bool _EnableIntellisense;
	private int _TextCasing;


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsDirty => EnableIntellisense != PersistentSettings.LanguageServiceEnableIntellisense
		|| AutoOutlining != PersistentSettings.LanguageServiceAutoOutlining
		|| UnderlineErrors != PersistentSettings.LanguageServiceUnderlineErrors
		|| MaxScriptSize != PersistentSettings.LanguageServiceMaxScriptSize
		|| TextCasing != (int)PersistentSettings.LanguageServiceTextCasing
		|| EnableAzureIntellisense != C_DefaultEnableAzureIntellisense;



	public bool UnderlineErrors
	{
		get { return _UnderlineErrors; }
		set { _UnderlineErrors = value; }
	}


	public int MaxScriptSize
	{
		get { return _MaxScriptSize; }
		set { _MaxScriptSize = value; }
	}

	public bool EnableIntellisense
	{
		get { return _EnableIntellisense; }
		set { _EnableIntellisense = value; }
	}

	public int TextCasing
	{
		get { return _TextCasing; }
		set { _TextCasing = value; }
	}

	public bool EnableAzureIntellisense { get; set; }




	public override void Apply()
	{
		base.Apply();
		LanguageExtensionPackage.Instance.SaveUserPreferences();
	}



	public override void InitUserPreferences(RegistryKey key, string name)
	{
		bool exists = LanguageExtensionPackage.Instance.UserPreferencesExist;

		base.InitUserPreferences(key, name);

		if (!exists)
		{
			SetDefaults();
			Apply();
		}
	}



	private void SetDefaults()
	{
		AutoOutlining = C_AutoOutlining;
		_UnderlineErrors = C_UnderlineErrors;
		_MaxScriptSize = C_DefaultMaxScriptSize;
		EnableIntellisense = C_EnableIntellisense;
		_TextCasing = C_TextCasing;
		EnableAzureIntellisense = C_DefaultEnableAzureIntellisense;

		LineNumbers = C_DefaultLineNumbers;
		WordWrap = C_DefaultWordWrap;
		WordWrapGlyphs = C_DefaultWordWrapGlyphs;
	}


	/// <summary>
	/// Updates preferences from the settings model.
	/// </summary>
	public void Update()
	{
		EnableIntellisense = PersistentSettings.LanguageServiceEnableIntellisense;
		AutoOutlining = PersistentSettings.LanguageServiceAutoOutlining;
		UnderlineErrors = PersistentSettings.LanguageServiceUnderlineErrors;
		MaxScriptSize = PersistentSettings.LanguageServiceMaxScriptSize;
		TextCasing = (int)PersistentSettings.LanguageServiceTextCasing;
	}

}
