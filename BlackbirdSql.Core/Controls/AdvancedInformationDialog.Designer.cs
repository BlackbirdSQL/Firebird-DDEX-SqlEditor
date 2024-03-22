// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.AdvancedInformation

using System.Windows.Forms;



namespace BlackbirdSql.Core.Controls;


partial class AdvancedInformationDialog
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedInformationDialog));
			this.label1 = new System.Windows.Forms.Label();
			this.tree = new System.Windows.Forms.TreeView();
			this.label2 = new System.Windows.Forms.Label();
			this.txtDetails = new System.Windows.Forms.TextBox();
			this.imgButtons = new System.Windows.Forms.ImageList(this.components);
			this.btnClose = new System.Windows.Forms.Button();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.tbBtnCopy = new System.Windows.Forms.ToolStripButton();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.toolStrip1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// tree
			// 
			resources.ApplyResources(this.tree, "tree");
			this.tree.Name = "tree";
			this.tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tree_AfterSelect);
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// txtDetails
			// 
			resources.ApplyResources(this.txtDetails, "txtDetails");
			this.txtDetails.Name = "txtDetails";
			this.txtDetails.ReadOnly = true;
			// 
			// imgButtons
			// 
			this.imgButtons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgButtons.ImageStream")));
			this.imgButtons.TransparentColor = System.Drawing.Color.Transparent;
			this.imgButtons.Images.SetKeyName(0, "");
			this.imgButtons.Images.SetKeyName(1, "SaveTextAsEmail.ico");
			this.imgButtons.Images.SetKeyName(2, "");
			this.imgButtons.Images.SetKeyName(3, "");
			// 
			// btnClose
			// 
			resources.ApplyResources(this.btnClose, "btnClose");
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.Name = "btnClose";
			// 
			// toolStrip1
			// 
			this.toolStrip1.AllowMerge = false;
			this.toolStrip1.CanOverflow = false;
			resources.ApplyResources(this.toolStrip1, "toolStrip1");
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbBtnCopy});
			this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.toolStrip1.TabStop = true;
			// 
			// tbBtnCopy
			// 
			this.tbBtnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tbBtnCopy, "tbBtnCopy");
			this.tbBtnCopy.Name = "tbBtnCopy";
			this.tbBtnCopy.Click += new System.EventHandler(this.tbBtnCopy_Click);
			// 
			// tableLayoutPanel1
			// 
			resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label2, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.tree, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.txtDetails, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 2);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// tableLayoutPanel2
			// 
			resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
			this.tableLayoutPanel2.Controls.Add(this.toolStrip1, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.btnClose, 1, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			// 
			// AdvancedInformationDialog
			// 
			this.AcceptButton = this.btnClose;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnClose;
			this.Controls.Add(this.tableLayoutPanel1);
			this.DoubleBuffered = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AdvancedInformationDialog";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.AdvancedInformationDialog_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AdvancedInformation_KeyDown);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);

	}

	#endregion

	private Label label1;
	private Label label2;
	private ImageList imgButtons;
	private TreeView tree;
	private TextBox txtDetails;
	private Button btnClose;
	private ToolStrip toolStrip1;
	private ToolStripButton tbBtnCopy;
	private TableLayoutPanel tableLayoutPanel1;
	private TableLayoutPanel tableLayoutPanel2;


}
