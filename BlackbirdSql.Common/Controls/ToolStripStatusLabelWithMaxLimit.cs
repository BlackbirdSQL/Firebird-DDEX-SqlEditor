// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Design.Core.Controls.ToolStripStatusLabelWithMaxLimit
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;
using BlackbirdSql.Common.Config;



namespace BlackbirdSql.Common.Controls;

public sealed class ToolStripStatusLabelWithMaxLimit : ToolStripStatusLabel
{
	private readonly int maxCharacters;

	private VsFontColorPreferences _vsFontColorPreferences;

	public override string Text
	{
		get
		{
			return base.Text;
		}
		set
		{
			base.ToolTipText = value;
			if (!string.IsNullOrEmpty(value) && maxCharacters < value.Length)
			{
				base.Text = string.Format(CultureInfo.InvariantCulture, "{0}...", value[..(maxCharacters - 3)]);
			}
			else
			{
				base.Text = value;
			}
		}
	}

	public ToolStripStatusLabelWithMaxLimit(int maxCharacters, int initialWidth)
	{
		SqlTracer.AssertTraceEvent(4 < maxCharacters, TraceEventType.Error, EnSqlTraceId.VSShell, "minimum number of characters is 4, otherwise truncation results in just \"...\"");
		this.maxCharacters = maxCharacters;
		base.Width = initialWidth;
		base.AutoToolTip = false;
		_vsFontColorPreferences = new VsFontColorPreferences();
		Font = VsFontColorPreferences.EnvironmentFont;
		_vsFontColorPreferences.PreferencesChangedEvent += VsFontColorPreferences_PreferencesChanged;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && _vsFontColorPreferences != null)
		{
			_vsFontColorPreferences.PreferencesChangedEvent -= VsFontColorPreferences_PreferencesChanged;
			_vsFontColorPreferences.Dispose();
			_vsFontColorPreferences = null;
		}
		base.Dispose(disposing);
	}

	private void VsFontColorPreferences_PreferencesChanged(object sender, EventArgs args)
	{
		Font = VsFontColorPreferences.EnvironmentFont;
	}
}
