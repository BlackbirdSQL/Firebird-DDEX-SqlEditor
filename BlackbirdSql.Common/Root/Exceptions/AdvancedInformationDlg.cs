#region Assembly Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Properties;

namespace BlackbirdSql.Common.Exceptions
{
	public class AdvancedInformationDlg : Form
	{
		private IContainer components;

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

		public ExceptionMessageBoxDlg MessageBoxForm { get; set; }

		public AdvancedInformationDlg()
		{
			InitializeComponent();
			base.Icon = null;
			toolStrip1.Renderer = new PrivateRenderer();
		}

		private void AdvancedInformation_Load(object sender, EventArgs e)
		{
			TreeNode treeNode = new TreeNode(ExceptionsResources.AdvInfoAllMessages);
			for (Exception ex = MessageBoxForm.Message; ex != null; ex = ex.InnerException)
			{
				try
				{
					TreeNode treeNode3 = new(ex.Message)
					{
						Tag = MessageBoxForm.BuildAdvancedInfo(ex, EnAdvancedInfoType.All)
					};
					TreeNode treeNode2 = new(ExceptionsResources.AdvInfoMessage)
					{
						Tag = MessageBoxForm.BuildAdvancedInfo(ex, EnAdvancedInfoType.Message)
					};
					treeNode3.Nodes.Add(treeNode2);
					treeNode.Nodes.Add(treeNode3);
					string text;
					try
					{
						text = MessageBoxForm.BuildAdvancedInfo(ex, EnAdvancedInfoType.HelpLink);
						if (text != null && text.Length > 0)
						{
							treeNode2 = new(ExceptionsResources.AdvInfoHelpLink)
							{
								Tag = text
							};
							treeNode3.Nodes.Add(treeNode2);
						}
					}
					catch (Exception)
					{
					}

					try
					{
						text = MessageBoxForm.BuildAdvancedInfo(ex, EnAdvancedInfoType.Data);
						if (text != null && text.Length > 0)
						{
							treeNode2 = new(ExceptionsResources.ADvInfoData)
							{
								Tag = text
							};
							treeNode3.Nodes.Add(treeNode2);
						}
					}
					catch (Exception)
					{
					}

					if (ex.StackTrace != null)
					{
						try
						{
							text = MessageBoxForm.BuildAdvancedInfo(ex, EnAdvancedInfoType.StackTrace);
							if (text != null && text.Length > 0)
							{
								treeNode2 = new(ExceptionsResources.CodeLocation[..^1])
								{
									Tag = text
								};
								treeNode3.Nodes.Add(treeNode2);
							}
						}
						catch (Exception)
						{
						}
					}
				}
				catch (Exception)
				{
				}
			}

			if (treeNode.Nodes.Count == 0)
			{
				Close();
				return;
			}

			StringBuilder stringBuilder = new StringBuilder();
			foreach (TreeNode node in treeNode.Nodes)
			{
				stringBuilder.Append(node.Tag.ToString());
			}

			treeNode.Tag = stringBuilder.ToString();
			tree.Nodes.Add(treeNode);
			tree.ExpandAll();
			tree.SelectedNode = treeNode;
		}

		private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			txtDetails.Text = (string)e.Node.Tag;
		}

		private void AdvancedInformation_KeyDown(object sender, KeyEventArgs e)
		{
			if ((e.Modifiers == Keys.Control && (e.KeyData & Keys.C) == Keys.C) || (e.KeyData & Keys.Insert) == Keys.Insert)
			{
				CopyToClipboard();
				e.Handled = true;
			}
		}

		private void CopyToClipboard()
		{
			try
			{
				Clipboard.SetDataObject((txtDetails.SelectionLength == 0) ? txtDetails.Text : txtDetails.SelectedText, copy: true);
			}
			catch (Exception exError)
			{
				MessageBoxForm.ShowError(ExceptionsResources.CopyToClipboardError, exError);
			}
		}

		private void TbBtnCopy_Click(object sender, EventArgs e)
		{
			CopyToClipboard();
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlackbirdSql.Common.Exceptions.AdvancedInformationDlg));
			label1 = new System.Windows.Forms.Label();
			tree = new System.Windows.Forms.TreeView();
			label2 = new System.Windows.Forms.Label();
			txtDetails = new System.Windows.Forms.TextBox();
			imgButtons = new System.Windows.Forms.ImageList(components);
			btnClose = new System.Windows.Forms.Button();
			toolStrip1 = new System.Windows.Forms.ToolStrip();
			tbBtnCopy = new System.Windows.Forms.ToolStripButton();
			tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			toolStrip1.SuspendLayout();
			tableLayoutPanel1.SuspendLayout();
			tableLayoutPanel2.SuspendLayout();
			SuspendLayout();
			resources.ApplyResources(label1, "label1");
			label1.Name = "label1";
			resources.ApplyResources(tree, "tree");
			tree.Name = "tree";
			tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(Tree_AfterSelect);
			resources.ApplyResources(label2, "label2");
			label2.Name = "label2";
			resources.ApplyResources(txtDetails, "txtDetails");
			txtDetails.Name = "txtDetails";
			txtDetails.ReadOnly = true;
			imgButtons.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imgButtons.ImageStream");
			imgButtons.TransparentColor = System.Drawing.Color.Transparent;
			imgButtons.Images.SetKeyName(0, "");
			imgButtons.Images.SetKeyName(1, "SaveTextAsEmail.ico");
			imgButtons.Images.SetKeyName(2, "");
			imgButtons.Images.SetKeyName(3, "");
			resources.ApplyResources(btnClose, "btnClose");
			btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			btnClose.Name = "btnClose";
			toolStrip1.AllowMerge = false;
			toolStrip1.CanOverflow = false;
			resources.ApplyResources(toolStrip1, "toolStrip1");
			toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { tbBtnCopy });
			toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			toolStrip1.Name = "toolStrip1";
			toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			toolStrip1.TabStop = true;
			tbBtnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(tbBtnCopy, "tbBtnCopy");
			tbBtnCopy.Name = "tbBtnCopy";
			tbBtnCopy.Click += new System.EventHandler(TbBtnCopy_Click);
			resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
			tableLayoutPanel1.Controls.Add(label1, 0, 0);
			tableLayoutPanel1.Controls.Add(label2, 1, 0);
			tableLayoutPanel1.Controls.Add(tree, 0, 1);
			tableLayoutPanel1.Controls.Add(txtDetails, 1, 1);
			tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 1, 2);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			resources.ApplyResources(tableLayoutPanel2, "tableLayoutPanel2");
			tableLayoutPanel2.Controls.Add(toolStrip1, 0, 0);
			tableLayoutPanel2.Controls.Add(btnClose, 1, 0);
			tableLayoutPanel2.Name = "tableLayoutPanel2";
			base.AcceptButton = btnClose;
			resources.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = btnClose;
			base.Controls.Add(tableLayoutPanel1);
			DoubleBuffered = true;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "AdvancedInformation";
			base.ShowInTaskbar = false;
			base.Load += new System.EventHandler(AdvancedInformation_Load);
			base.KeyDown += new System.Windows.Forms.KeyEventHandler(AdvancedInformation_KeyDown);
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel1.PerformLayout();
			tableLayoutPanel2.ResumeLayout(false);
			tableLayoutPanel2.PerformLayout();
			ResumeLayout(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
