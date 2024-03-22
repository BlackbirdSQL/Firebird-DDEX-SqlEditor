// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions.CurrentWndOptions

using System.Windows.Forms;



namespace BlackbirdSql.EditorExtension.Controls.Config;


partial class AbstractLiveSettingsDialog
{
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>

	private void InitializeComponent()
	{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AbstractLiveSettingsDialog));
			this._OkButton = new System.Windows.Forms.Button();
			this._CancelButton = new System.Windows.Forms.Button();
			this._ViewSwitcherTree = new System.Windows.Forms.TreeView();
			this._CurrentViewPanel = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// _OkButton
			// 
			resources.ApplyResources(this._OkButton, "_OkButton");
			this._OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._OkButton.Name = "_OkButton";
			// 
			// _CancelButton
			// 
			resources.ApplyResources(this._CancelButton, "_CancelButton");
			this._CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._CancelButton.Name = "_CancelButton";
			// 
			// _ViewSwitcherTree
			// 
			this._ViewSwitcherTree.HideSelection = false;
			resources.ApplyResources(this._ViewSwitcherTree, "_ViewSwitcherTree");
			this._ViewSwitcherTree.Name = "_ViewSwitcherTree";
			this._ViewSwitcherTree.Scrollable = false;
			this._ViewSwitcherTree.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.ViewSwitcherTree_BeforeCollapse);
			this._ViewSwitcherTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.ViewSwitcherTree_BeforeSelect);
			this._ViewSwitcherTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ViewSwitcherTree_AfterSelect);
			// 
			// _CurrentViewPanel
			// 
			resources.ApplyResources(this._CurrentViewPanel, "_CurrentViewPanel");
			this._CurrentViewPanel.Name = "_CurrentViewPanel";
			// 
			// AbstractLiveSettingsDialog
			// 
			this.AcceptButton = this._OkButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._CancelButton;
			this.Controls.Add(this._CurrentViewPanel);
			this.Controls.Add(this._ViewSwitcherTree);
			this.Controls.Add(this._CancelButton);
			this.Controls.Add(this._OkButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AbstractLiveSettingsDialog";
			this.ShowInTaskbar = false;
			this.ResumeLayout(false);

	}

	#endregion

	private Button _OkButton;
	private TreeView _ViewSwitcherTree;
	private Panel _CurrentViewPanel;
	private Button _CancelButton;

}
