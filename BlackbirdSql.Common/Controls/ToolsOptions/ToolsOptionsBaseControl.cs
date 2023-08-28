#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Globalization;
using System.Windows.Forms;

using BlackbirdSql.Common.Config;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Controls.ToolsOptions
{
	public class ToolsOptionsBaseControl : UserControl
	{
		private bool _trackChanges;

		public bool TrackChanges
		{
			get
			{
				return _trackChanges;
			}
			set
			{
				_trackChanges = value;
			}
		}

		public ToolsOptionsBaseControl()
		{
			Font = VsFontColorPreferences.EnvironmentFont;
		}

		public void LoadSettings(IUserSettings options)
		{
			ApplySettingsToUI(options);
			_trackChanges = true;
		}

		public void SaveSettings(IUserSettings options)
		{
			SaveSettingsFromUI(options);
			UserSettings.Instance.Save();
		}

		public void ResetSettings()
		{
			ApplySettingsToUI(UserSettings.Instance.Default);
		}

		protected virtual void ApplySettingsToUI(IUserSettings options)
		{
		}

		protected virtual void SaveSettingsFromUI(IUserSettings options)
		{
		}

		protected virtual bool ValidateNumeric(NumericUpDown numControl, string errMessageToShow)
		{
			try
			{
				int num = int.Parse(numControl.Text, CultureInfo.InvariantCulture);
				if (num < numControl.Minimum || num > numControl.Maximum)
				{
					Cmd.ShowMessageBoxEx(null, errMessageToShow, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					numControl.Focus();
					return false;
				}

				return true;
			}
			catch
			{
				Cmd.ShowMessageBoxEx(null, errMessageToShow, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				numControl.Focus();
				return false;
			}
		}

		public virtual bool ValidateValuesInControls()
		{
			return true;
		}

		public virtual string GetHelpKeyword()
		{
			return string.Empty;
		}

		protected bool SaveOrCompareCurrentValueOfControls(bool save)
		{
			foreach (Control control in Controls)
			{
				if (control is ComboBox comboBox)
				{
					if (comboBox.DropDownStyle == ComboBoxStyle.DropDownList)
					{
						if (save)
						{
							comboBox.Tag ??= comboBox.SelectedIndex;
						}
						else if ((int)comboBox.Tag != comboBox.SelectedIndex)
						{
							return false;
						}
					}
					else if (save)
					{
						comboBox.Tag ??= comboBox.Text;
					}
					else if ((string)comboBox.Tag != comboBox.Text)
					{
						return false;
					}

					continue;
				}

				if (control is CheckBox checkBox)
				{
					if (save)
					{
						checkBox.Tag ??= checkBox.Checked;
					}
					else if ((bool)checkBox.Tag != checkBox.Checked)
					{
						return false;
					}

					continue;
				}

				if (control is TextBox textBox)
				{
					if (save)
					{
						textBox.Tag ??= textBox.Text;
					}
					else if ((string)textBox.Tag != textBox.Text)
					{
						return false;
					}

					continue;
				}

				if (control is not NumericUpDown numericUpDown)
				{
					continue;
				}

				if (save)
				{
					numericUpDown.Tag ??= numericUpDown.Value;
				}
				else if ((decimal)numericUpDown.Tag != numericUpDown.Value)
				{
					return false;
				}
			}

			return true;
		}
	}
}
