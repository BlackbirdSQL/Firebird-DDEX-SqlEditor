// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions.SqlResultsGeneralSettingsDlg

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core.Diagnostics;

using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Common.Controls.ToolsOptions;


public class SqlResultsGeneralSettingsDlg : ToolsOptionsBaseControl
{
	private readonly EnSqlExecutionMode[] _ExecModeToComboIndexMap = new EnSqlExecutionMode[3]
	{
		EnSqlExecutionMode.ResultsToGrid,
		EnSqlExecutionMode.ResultsToText,
		EnSqlExecutionMode.ResultsToFile
	};

	private IVsUIShell m_shell;

	private static readonly string helpKeyword = "BlackbirdSql.Common.Controls.ToolsOptions.SqlResultsGeneralSettingsDlg";

	private Label _resFilesDirLabel;

	private TextBox _resFileDirEdit;

	private Button _browseButton;

	private Label _defResTargetlabel;

	private ComboBox _defResTargetComboBox;

	private Label _resultOptionsExplainLabel;

	private CheckBox _playSound;

	private TableLayoutPanel _tableLayoutPanel;

	private TableLayoutPanel tableLayoutPanel1;

	private Button _resetButton;

	public SqlResultsGeneralSettingsDlg()
	{
		InitializeComponent();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SqlResultsGeneralSettingsDlg));
		tableLayoutPanel1 = new TableLayoutPanel();
		_resetButton = new Button();
		_tableLayoutPanel = new TableLayoutPanel();
		_resultOptionsExplainLabel = new Label();
		_defResTargetlabel = new Label();
		_defResTargetComboBox = new ComboBox();
		_resFilesDirLabel = new Label();
		_resFileDirEdit = new TextBox();
		_browseButton = new Button();
		_playSound = new CheckBox();
		tableLayoutPanel1.SuspendLayout();
		_tableLayoutPanel.SuspendLayout();
		SuspendLayout();
		resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
		tableLayoutPanel1.Controls.Add(_resetButton, 0, 0);
		tableLayoutPanel1.Name = "tableLayoutPanel1";
		resources.ApplyResources(_resetButton, "_resetButton");
		tableLayoutPanel1.SetColumnSpan(_resetButton, 2);
		_resetButton.Name = "_resetButton";
		_resetButton.Click += new EventHandler(ResetButton_Click);
		resources.ApplyResources(_tableLayoutPanel, "_tableLayoutPanel");
		_tableLayoutPanel.Controls.Add(_resultOptionsExplainLabel, 0, 0);
		_tableLayoutPanel.Controls.Add(_defResTargetlabel, 0, 1);
		_tableLayoutPanel.Controls.Add(_defResTargetComboBox, 0, 2);
		_tableLayoutPanel.Controls.Add(_resFilesDirLabel, 0, 3);
		_tableLayoutPanel.Controls.Add(_resFileDirEdit, 0, 4);
		_tableLayoutPanel.Controls.Add(_browseButton, 1, 4);
		_tableLayoutPanel.Controls.Add(_playSound, 0, 5);
		_tableLayoutPanel.Name = "_tableLayoutPanel";
		resources.ApplyResources(_resultOptionsExplainLabel, "_resultOptionsExplainLabel");
		_tableLayoutPanel.SetColumnSpan(_resultOptionsExplainLabel, 2);
		_resultOptionsExplainLabel.Name = "_resultOptionsExplainLabel";
		resources.ApplyResources(_defResTargetlabel, "_defResTargetlabel");
		_tableLayoutPanel.SetColumnSpan(_defResTargetlabel, 2);
		_defResTargetlabel.Name = "_defResTargetlabel";
		resources.ApplyResources(_defResTargetComboBox, "_defResTargetComboBox");
		_defResTargetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
		_defResTargetComboBox.FormattingEnabled = true;
		_defResTargetComboBox.Items.AddRange(new object[3]
		{
			resources.GetString("_defResTargetComboBox.Items"),
			resources.GetString("_defResTargetComboBox.Items1"),
			resources.GetString("_defResTargetComboBox.Items2")
		});
		_defResTargetComboBox.Name = "_defResTargetComboBox";
		resources.ApplyResources(_resFilesDirLabel, "_resFilesDirLabel");
		_resFilesDirLabel.Name = "_resFilesDirLabel";
		resources.ApplyResources(_resFileDirEdit, "_resFileDirEdit");
		_resFileDirEdit.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
		_resFileDirEdit.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
		_resFileDirEdit.Name = "_resFileDirEdit";
		resources.ApplyResources(_browseButton, "_browseButton");
		_browseButton.Name = "_browseButton";
		_browseButton.Click += new EventHandler(BrowseButton_Click);
		resources.ApplyResources(_playSound, "_playSound");
		_tableLayoutPanel.SetColumnSpan(_playSound, 2);
		_playSound.Name = "_playSound";
		resources.ApplyResources(this, "$this");
		Controls.Add(tableLayoutPanel1);
		Controls.Add(_tableLayoutPanel);
		Name = "SqlResultsGeneralSettingsDlg";
		tableLayoutPanel1.ResumeLayout(false);
		tableLayoutPanel1.PerformLayout();
		_tableLayoutPanel.ResumeLayout(false);
		_tableLayoutPanel.PerformLayout();
		ResumeLayout(false);
		PerformLayout();
	}

	public void SetIVsUIShell(IVsUIShell shell)
	{
		Tracer.Trace(GetType(), "EditorFactoryPackage.TName", "SqlResultsGeneral.SetIVsUIShell", "", null);
		m_shell = shell;
	}

	public override string GetHelpKeyword()
	{
		return helpKeyword;
	}

	public override bool ValidateValuesInControls()
	{
		Tracer.Trace(GetType(), "SqlResultsGeneral.ValidateValuesInControls", "", null);
		if (TrackChanges && !SaveOrCompareCurrentValueOfControls(save: false))
		{
			Cmd.ShowMessageBoxEx(null, SharedResx.WarnSqlGeneralResultsChanges, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			TrackChanges = false;
		}
		return true;
	}

	protected override void ApplySettingsToUI(IUserSettings options)
	{
		if (options == null)
		{
			return;
		}
		_resFileDirEdit.Text = options.ExecutionResults.ResultsDirectory;
		for (int i = 0; i < _ExecModeToComboIndexMap.Length; i++)
		{
			if (options.ExecutionResults.SqlExecutionMode == _ExecModeToComboIndexMap[i])
			{
				_defResTargetComboBox.SelectedIndex = i;
				break;
			}
		}
		_playSound.Checked = options.ExecutionResults.ProvideFeedbackWithSounds;
		if (TrackChanges)
		{
			SaveOrCompareCurrentValueOfControls(save: true);
		}
	}

	protected override void SaveSettingsFromUI(IUserSettings options)
	{
		options.ExecutionResults.ProvideFeedbackWithSounds = _playSound.Checked;
		options.ExecutionResults.ResultsDirectory = _resFileDirEdit.Text;
		options.ExecutionResults.SqlExecutionMode = _ExecModeToComboIndexMap[_defResTargetComboBox.SelectedIndex];
	}

	private void BrowseButton_Click(object sender, EventArgs e)
	{
		Tracer.Trace(GetType(), "SqlResultsGeneral.BrowseButton_Click", "", null);
		if (m_shell == null)
		{
			return;
		}
		int num = 512;
		char[] array = new char[num];
		IntPtr zero = Marshal.AllocCoTaskMem(array.Length * 2);
		Marshal.Copy(array, 0, zero, array.Length);
		VSBROWSEINFOW vSBROWSEINFOW = default;
		vSBROWSEINFOW.lStructSize = (uint)Marshal.SizeOf(typeof(VSBROWSEINFOW));
		vSBROWSEINFOW.hwndOwner = Handle;
		vSBROWSEINFOW.pwzDlgTitle = SharedResx.PleaseSelectDirectory;
		vSBROWSEINFOW.pwzInitialDir = _resFileDirEdit.Text;
		vSBROWSEINFOW.nMaxDirName = (uint)num;
		vSBROWSEINFOW.pwzDirName = zero;
		vSBROWSEINFOW.dwFlags = 1u;
		try
		{
			Native.ThrowOnFailure(m_shell.GetDirectoryViaBrowseDlg(new VSBROWSEINFOW[1] { vSBROWSEINFOW }));
			Marshal.Copy(zero, array, 0, array.Length);
			int i;
			for (i = 0; i < array.Length && array[i] != 0; i++)
			{
			}
			_resFileDirEdit.Text = new string(array, 0, i);
		}
		catch (Exception e2)
		{
			Tracer.LogExCatch(GetType(), e2);
		}
	}

	private void ResetButton_Click(object sender, EventArgs e)
	{
		Tracer.Trace(GetType(), "SQLEnvironmentOptions.ResetButton_Click", "", null);
		ResetSettings();
	}
}
