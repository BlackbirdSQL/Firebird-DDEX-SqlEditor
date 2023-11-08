#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.ComponentModel;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core.Controls.Interfaces;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.EditorExtension.Ctl.Config;
using BlackbirdSql.EditorExtension.Properties;

namespace BlackbirdSql.EditorExtension.Controls.Config;

public sealed class CurrentWndOptionsDlg : AbstractCurrentWndOptionsDlg
{
#pragma warning disable CS0649 // Field 'CurrentWndSQLOptions.components' is never assigned to, and will always have its default value null
	private readonly Container _Components;
#pragma warning restore CS0649 // Field 'CurrentWndSQLOptions.components' is never assigned to, and will always have its default value null


	public CurrentWndOptionsDlg(IBLiveSettings settings)
	{
		InitializeDialog(settings);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && _Components != null)
		{
			_Components.Dispose();
		}

		base.Dispose(disposing);
	}

	private void InitializeDialog(IBLiveSettings liveSettings)
	{
		IBSettingsPage[] array = new IBSettingsPage[5];
		SettingsProvider.ExecutionSettingsPage executionSettingsDlg
			= SettingsProvider.ExecutionSettingsPage.CreateInstance(liveSettings);
		// sqlExecutionGeneralSettingsDlg.SetSqlCmdControlMode(showCheckBox: false);
		array[0] = executionSettingsDlg;
		SettingsProvider.ExecutionAdvancedSettingsPage executionAdvancedSettingsDlg
			= SettingsProvider.ExecutionAdvancedSettingsPage.CreateInstance(liveSettings);
		array[1] = executionAdvancedSettingsDlg;
		SettingsProvider.ResultsSettingsPage resultsSettingsDlg
			 = SettingsProvider.ResultsSettingsPage.CreateInstance(liveSettings);
		array[2] = resultsSettingsDlg;
		SettingsProvider.ResultsGridSettingsPage resultsGridSettingsDlg
			 = SettingsProvider.ResultsGridSettingsPage.CreateInstance(liveSettings);
		array[3] = resultsGridSettingsDlg;
		SettingsProvider.ResultsTextSettingsPage resultsTextSettingsDlg
			 = SettingsProvider.ResultsTextSettingsPage.CreateInstance(liveSettings);
		array[4] = resultsTextSettingsDlg;

		TreeNode[] array2 = new TreeNode[2]
		{
			new TreeNode(AttributeResources.OptionPageExecution),
			null
		};
		array2[0].ImageIndex = _FolderImageIndex;
		array2[0].Expand();
		AssociateTreeNodeContext(array2[0], new TreeNodeContext(0, folder: true));
		TreeNode treeNode = new(AttributeResources.OptionPageExecutionGeneral)
		{
			ImageIndex = _LeafImageIndex,
			SelectedImageIndex = _LeafSelectedImageIndex
		};
		AssociateTreeNodeContext(treeNode, new TreeNodeContext(0, folder: false));
		TreeNode treeNode2 = new(AttributeResources.OptionPageExecutionAdvanced)
		{
			ImageIndex = _LeafImageIndex,
			SelectedImageIndex = _LeafSelectedImageIndex
		};
		AssociateTreeNodeContext(treeNode2, new TreeNodeContext(1, folder: false));
		array2[0].Nodes.Add(treeNode);
		array2[0].Nodes.Add(treeNode2);
		array2[1] = new(AttributeResources.OptionPageResults)
		{
			ImageIndex = _FolderImageIndex
		};
		array2[1].Expand();
		AssociateTreeNodeContext(array2[1], new TreeNodeContext(3, folder: true));
		TreeNode treeNode4 = new(AttributeResources.OptionPageResultsGrid)
		{
			ImageIndex = _LeafImageIndex,
			SelectedImageIndex = _LeafSelectedImageIndex
		};
		AssociateTreeNodeContext(treeNode4, new TreeNodeContext(3, folder: false));
		TreeNode treeNode5 = new(AttributeResources.OptionPageResultsText)
		{
			ImageIndex = _LeafImageIndex,
			SelectedImageIndex = _LeafSelectedImageIndex
		};
		AssociateTreeNodeContext(treeNode5, new TreeNodeContext(4, folder: false));
		array2[1].Nodes.Add(treeNode4);
		array2[1].Nodes.Add(treeNode5);
		SetControls(array);
		AddTreeNodes(array2, treeNode);
	}

}
