// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.GeneralSettings

using System;
using BlackbirdSql.Common.Interfaces;



namespace BlackbirdSql.Common.Config;

public sealed class GeneralSettings : IGeneralSettings, ICloneable
{
	public static class Defaults
	{
		public static readonly bool PromptForSaveWhenClosingQueryWindows = true;
	}

	private bool? _promptForSaveWhenClosingQueryWindows;

	public bool PromptForSaveWhenClosingQueryWindows
	{
		get
		{
			if (!_promptForSaveWhenClosingQueryWindows.HasValue)
			{
				return Defaults.PromptForSaveWhenClosingQueryWindows;
			}

			return _promptForSaveWhenClosingQueryWindows.Value;
		}
		set
		{
			_promptForSaveWhenClosingQueryWindows = value;
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public void ResetToDefault()
	{
		_promptForSaveWhenClosingQueryWindows = Defaults.PromptForSaveWhenClosingQueryWindows;
	}
}
