#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.ComponentModel;
using System.Windows.Forms;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Properties;


namespace BlackbirdSql.Common.Controls.ToolsOptions
{
	public sealed class CurrentWndSQLOptions : CurrentWndOptions
	{
#pragma warning disable CS0649 // Field 'CurrentWndSQLOptions.components' is never assigned to, and will always have its default value null
		private readonly Container components;
#pragma warning restore CS0649 // Field 'CurrentWndSQLOptions.components' is never assigned to, and will always have its default value null

		public CurrentWndSQLOptions()
		{
			InitializeDialog();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		private void InitializeDialog()
		{
			ToolsOptionsBaseControl[] array = new ToolsOptionsBaseControl[5];
			SqlExecutionGeneralSettingsDlg sqlExecutionGeneralSettingsDlg = new SqlExecutionGeneralSettingsDlg();
			sqlExecutionGeneralSettingsDlg.SetSqlCmdControlMode(showCheckBox: false);
			array[0] = sqlExecutionGeneralSettingsDlg;
			array[1] = new SqlExecutionAdvancedSettingsDlg();
			array[2] = new SqlExecutionAnsiSettingsDlg();
			array[3] = new SqlResultsToGridSettingsDlg();
			array[4] = new SqlResultToTextSettingsDlg();
			TreeNode[] array2 = new TreeNode[2]
			{
				new TreeNode(ControlsResources.QeQueryOptionsExecution),
				null
			};
			array2[0].ImageIndex = _folderImageIndex;
			array2[0].Expand();
			AssociateTreeNodeContext(array2[0], new TreeNodeContext(0, folder: true));
			TreeNode treeNode = new(ControlsResources.QeQueryOptionsGeneral)
			{
				ImageIndex = _leafImageIndex,
				SelectedImageIndex = _leafSelectedImageIndex
			};
			AssociateTreeNodeContext(treeNode, new TreeNodeContext(0, folder: false));
			TreeNode treeNode2 = new(ControlsResources.QeQueryOptionsAdvanced)
			{
				ImageIndex = _leafImageIndex,
				SelectedImageIndex = _leafSelectedImageIndex
			};
			AssociateTreeNodeContext(treeNode2, new TreeNodeContext(1, folder: false));
			TreeNode treeNode3 = new(ControlsResources.QeQueryOptionsAnsi)
			{
				ImageIndex = _leafImageIndex,
				SelectedImageIndex = _leafSelectedImageIndex
			};
			AssociateTreeNodeContext(treeNode3, new TreeNodeContext(2, folder: false));
			array2[0].Nodes.Add(treeNode);
			array2[0].Nodes.Add(treeNode2);
			array2[0].Nodes.Add(treeNode3);
			array2[1] = new(ControlsResources.QeQueryOptionsResults)
			{
				ImageIndex = _folderImageIndex
			};
			array2[1].Expand();
			AssociateTreeNodeContext(array2[1], new TreeNodeContext(3, folder: true));
			TreeNode treeNode4 = new(ControlsResources.QeQueryOptionsGrid)
			{
				ImageIndex = _leafImageIndex,
				SelectedImageIndex = _leafSelectedImageIndex
			};
			AssociateTreeNodeContext(treeNode4, new TreeNodeContext(3, folder: false));
			TreeNode treeNode5 = new(ControlsResources.QeQueryOptionsText)
			{
				ImageIndex = _leafImageIndex,
				SelectedImageIndex = _leafSelectedImageIndex
			};
			AssociateTreeNodeContext(treeNode5, new TreeNodeContext(4, folder: false));
			array2[1].Nodes.Add(treeNode4);
			array2[1].Nodes.Add(treeNode5);
			SetControls(array);
			AddTreeNodes(array2, treeNode);
		}

		public void Serialize(IUserSettings options, bool bToControls)
		{
			ToolsOptionsBaseControl[] optionViews = _optionViews;
			foreach (ToolsOptionsBaseControl toolsOptionsBaseControl in optionViews)
			{
				if (bToControls)
				{
					toolsOptionsBaseControl.LoadSettings(options);
				}
				else
				{
					toolsOptionsBaseControl.SaveSettings(options);
				}
			}
		}
	}
}
