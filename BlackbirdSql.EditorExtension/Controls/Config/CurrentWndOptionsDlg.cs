// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions.CurrentWndOptions

using System.Windows.Forms;
using BlackbirdSql.Core.Controls.Interfaces;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.EditorExtension.Ctl.Config;
using BlackbirdSql.EditorExtension.Properties;

namespace BlackbirdSql.EditorExtension.Controls.Config;

public sealed class CurrentWndOptionsDlg : AbstractCurrentWndOptionsDlg
{

	public CurrentWndOptionsDlg(IBTransientSettings settings)
	{
		InitializeDialog(settings);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	private void InitializeDialog(IBTransientSettings transientSettings)
	{
		IBSettingsPage[] pages = new IBSettingsPage[4];
		SettingsProvider.TransientExecutionSettingsPage executionSettingsDlg = new(transientSettings);
		pages[0] = executionSettingsDlg;
		SettingsProvider.TransientExecutionAdvancedSettingsPage executionAdvancedSettingsDlg = new(transientSettings);
		pages[1] = executionAdvancedSettingsDlg;
		SettingsProvider.TransientResultsGridSettingsPage resultsGridSettingsDlg = new(transientSettings);
		pages[2] = resultsGridSettingsDlg;
		SettingsProvider.TransientResultsTextSettingsPage resultsTextSettingsDlg = new(transientSettings);
		pages[3] = resultsTextSettingsDlg;


		TreeNode[] rootNode = 
		[
			new(AttributeResources.OptionPageExecution)
			{
				ImageIndex = _FolderImageIndex
			},
			new(AttributeResources.OptionPageResults)
			{
				ImageIndex = _FolderImageIndex
			}
		];


		rootNode[0].Expand();
		AssociateTreeNodeContext(rootNode[0], new TreeNodeContext(0, folder: true));
		TreeNode treeNode00 = new(AttributeResources.OptionPageExecutionGeneral)
		{
			ImageIndex = _LeafImageIndex,
			SelectedImageIndex = _LeafSelectedImageIndex
		};
		AssociateTreeNodeContext(treeNode00, new TreeNodeContext(0, folder: false));
		TreeNode treeNode01 = new(AttributeResources.OptionPageExecutionAdvanced)
		{
			ImageIndex = _LeafImageIndex,
			SelectedImageIndex = _LeafSelectedImageIndex
		};
		AssociateTreeNodeContext(treeNode01, new TreeNodeContext(1, folder: false));
		rootNode[0].Nodes.Add(treeNode00);
		rootNode[0].Nodes.Add(treeNode01);


				
		rootNode[1].Expand();
		AssociateTreeNodeContext(rootNode[1], new TreeNodeContext(2, folder: true));
		TreeNode treeNode10 = new(AttributeResources.OptionPageResultsGrid)
		{
			ImageIndex = _LeafImageIndex,
			SelectedImageIndex = _LeafSelectedImageIndex
		};
		AssociateTreeNodeContext(treeNode10, new TreeNodeContext(2, folder: false));
		TreeNode treeNode11 = new(AttributeResources.OptionPageResultsText)
		{
			ImageIndex = _LeafImageIndex,
			SelectedImageIndex = _LeafSelectedImageIndex
		};
		AssociateTreeNodeContext(treeNode11, new TreeNodeContext(3, folder: false));
		rootNode[1].Nodes.Add(treeNode10);
		rootNode[1].Nodes.Add(treeNode11);

		SetControls(pages);
		AddTreeNodes(rootNode, treeNode00);
	}

}
