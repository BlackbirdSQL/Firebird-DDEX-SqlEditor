// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.AdvancedInformation

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows.Forms;
using BlackbirdSql.Sys.Controls.Widgets;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Properties;



namespace BlackbirdSql.Sys.Controls;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]


// =========================================================================================================
//									AdvancedInformationDialog Class 
//
/// <summary>
/// Advanced info dialog for the Universal MessageBox <see cref="AdvancedMessageBox"/>.
/// </summary>
// =========================================================================================================
public partial class AdvancedInformationDialog : Form
{

	// -----------------------------------------------------------
	#region Constructors / Destructors - AdvancedInformationDialog
	// -----------------------------------------------------------


	/// <summary>
	/// AdvancedInformationDialog .ctor
	/// </summary>
	public AdvancedInformationDialog()
	{
		InitializeComponent();
		Icon = null;
		toolStrip1.Renderer = new PrivateRenderer();
	}


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AdvancedInformationDialog
	// =========================================================================================================


	public AdvancedMessageBox MessageBoxForm { get; set; }


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - AdvancedInformationDialog
	// =========================================================================================================


	private void CopyToClipboard()
	{
		try
		{
			Clipboard.SetDataObject(txtDetails.SelectionLength == 0 ? txtDetails.Text : txtDetails.SelectedText, copy: true);
		}
		catch (Exception exError)
		{
			MessageBoxForm.ShowError(ControlsResources.AdvancedMessageBox_CopyToClipboardError, exError);
		}
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AdvancedInformationDialog
	// =========================================================================================================


	private void AdvancedInformation_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Modifiers == Keys.Control && (e.KeyData & Keys.C) == Keys.C || (e.KeyData & Keys.Insert) == Keys.Insert)
		{
			CopyToClipboard();
			e.Handled = true;
		}
	}



	private void AdvancedInformationDialog_Load(object sender, EventArgs e)
	{
		TreeNode treeNode = new TreeNode(ControlsResources.AdvancedInformationDialog_AllMessages);
		for (Exception ex = MessageBoxForm.ExMessage; ex != null; ex = ex.InnerException)
		{
			try
			{
				string msg = MessageBoxForm.BuildAdvancedInfo(ex, EnAdvancedInfoType.All);
				bool isDbClientInfo = msg.Contains(NativeDb.DbEngineName);
				string[] msgList = ex.Message.Split('\n');

				string nodeLabel = ControlsResources.AdvancedInformationDialog_NodeExceptionInfo.Fmt(isDbClientInfo ? NativeDb.DbEngineName : "BlackbirdSql", msgList[0]);

				TreeNode treeNode3 = new(nodeLabel)
				{
					Tag = msg
				};
				TreeNode treeNode2 = new(ControlsResources.AdvancedInformationDialog_Message)
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
						treeNode2 = new(ControlsResources.AdvancedInformationDialog_HelpLink)
						{
							Tag = text
						};
						treeNode3.Nodes.Add(treeNode2);
					}
				}
				catch (Exception ex2)
				{
					Diag.Ex(ex2);
				}

				try
				{
					text = MessageBoxForm.BuildAdvancedInfo(ex, EnAdvancedInfoType.Data);
					if (text != null && text.Length > 0)
					{
						treeNode2 = new(ControlsResources.AdvancedInformationDialog_AdditionalData)
						{
							Tag = text
						};
						treeNode3.Nodes.Add(treeNode2);
					}
				}
				catch (Exception ex2)
				{
					Diag.Ex(ex2);
				}

				if (ex.StackTrace != null)
				{
					try
					{
						text = MessageBoxForm.BuildAdvancedInfo(ex, EnAdvancedInfoType.StackTrace);
						if (text != null && text.Length > 0)
						{
							treeNode2 = new(ControlsResources.AdvancedMessageBox_CodeLocation[..^1])
							{
								Tag = text
							};
							treeNode3.Nodes.Add(treeNode2);
						}
					}
					catch (Exception ex2)
					{
						Diag.Ex(ex2);
					}
				}
			}
			catch (Exception ex2)
			{
				Diag.Ex(ex2);
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



	private void tbBtnCopy_Click(object sender, EventArgs e)
	{
		CopyToClipboard();
	}



	private void tree_AfterSelect(object sender, TreeViewEventArgs e)
	{
		txtDetails.Text = (string)e.Node.Tag;
	}


	#endregion Event Handling

}
