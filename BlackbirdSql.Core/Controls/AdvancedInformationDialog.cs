﻿// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.AdvancedInformation

using System;
using System.Text;
using System.Windows.Forms;
using BlackbirdSql.Core.Controls.Enums;
using BlackbirdSql.Core.Controls.Widgets;
using BlackbirdSql.Core.Properties;



namespace BlackbirdSql.Core.Controls;


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
			MessageBoxForm.ShowError(ControlsResources.CopyToClipboardError, exError);
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
		TreeNode treeNode = new TreeNode(ControlsResources.AdvInfoAllMessages);
		for (Exception ex = MessageBoxForm.ExMessage; ex != null; ex = ex.InnerException)
		{
			try
			{
				TreeNode treeNode3 = new(ex.Message)
				{
					Tag = MessageBoxForm.BuildAdvancedInfo(ex, EnAdvancedInfoType.All)
				};
				TreeNode treeNode2 = new(ControlsResources.AdvInfoMessage)
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
						treeNode2 = new(ControlsResources.AdvInfoHelpLink)
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
						treeNode2 = new(ControlsResources.AdvInfoData)
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
							treeNode2 = new(ControlsResources.CodeLocation[..^1])
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
