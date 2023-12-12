// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ToolsOptions.CurrentWndOptions
using System;
using System.ComponentModel;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl.Config;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Controls.Interfaces;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.EditorExtension.Properties;
using Microsoft.VisualStudio.VSHelp;



namespace BlackbirdSql.EditorExtension.Controls.Config;

public partial class AbstractCurrentWndOptionsDlg : Form
{
	protected class TreeNodeContext
	{
		private readonly int _ControlIndex = -1;

		private readonly bool _IsFolder;

		public int ControlIndex => _ControlIndex;

		public bool IsFolder => _IsFolder;

		private TreeNodeContext()
		{
		}

		public TreeNodeContext(int controlIndex, bool folder)
		{
			_ControlIndex = controlIndex;
			_IsFolder = folder;
		}
	}

	protected int _FolderImageIndex;

	protected int _LeafSelectedImageIndex = 1;

	protected int _LeafImageIndex = 2;

	protected IBSettingsPage[] _OptionViews;

	private bool[] _OptionViewsInitialized;

	private int _CurrentViewIndex;

	private Button _OkButton;

	private Button _HelpButton;

	private TreeView _ViewSwitcherTree;

	private Panel _CurrentViewPanel;

	private Button _CancelButton;

	public AbstractCurrentWndOptionsDlg()
	{
		InitializeComponent();
		Icon = AttributeResources.Properties_16x;
		Font = VsFontColorPreferences.EnvironmentFont;
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);

		if (DialogResult == DialogResult.OK)
		{
			if (!ValidateCurrentView())
			{
				e.Cancel = true;
			}
			else
			{
				foreach (IBSettingsPage page in _OptionViews)
				{
					page.SaveSettings();
				}
			}
		}
		else
		{
			e.Cancel = true;
		}
		// base.OnFormClosing
		// base.OnClosing
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		base.OnFormClosing(e);
	}


	protected override void OnHelpRequested(HelpEventArgs hevent)
	{
		DisplayHelpTopicForCurrentView();
		hevent.Handled = true;
	}


	protected override bool ProcessKeyPreview(ref Message m)
	{
		if (m.Msg == Native.WM_KEYFIRST)
		{
			KeyEventArgs keyEventArgs = new KeyEventArgs((Keys)((int)m.WParam | (int)ModifierKeys));
			if (keyEventArgs.KeyCode == Keys.Tab && (keyEventArgs.KeyData & Keys.Control) != 0)
			{
				bool forward = (keyEventArgs.KeyData & Keys.Shift) == 0;
				CycleView(forward);
				return true;
			}

			if (keyEventArgs.Modifiers == Keys.Control && (keyEventArgs.KeyCode == Keys.Next || keyEventArgs.KeyCode == Keys.Prior))
			{
				bool forward2 = keyEventArgs.KeyCode == Keys.Next;
				CycleView(forward2);
				return true;
			}
		}

		return base.ProcessKeyPreview(ref m);
	}

	private void InitializeComponent()
	{
		ComponentResourceManager resources = new ComponentResourceManager(typeof(AbstractCurrentWndOptionsDlg));
		_OkButton = new Button();
		_CancelButton = new Button();
		_HelpButton = new Button();
		_ViewSwitcherTree = new TreeView();
		_CurrentViewPanel = new Panel();
		SuspendLayout();
		resources.ApplyResources(_OkButton, "_OkButton");
		_OkButton.DialogResult = DialogResult.OK;
		_OkButton.Name = "_OkButton";
		resources.ApplyResources(_CancelButton, "_CancelButton");
		_CancelButton.DialogResult = DialogResult.Cancel;
		_CancelButton.Name = "_CancelButton";
		resources.ApplyResources(_HelpButton, "_HelpButton");
		_HelpButton.Name = "_HelpButton";
		_HelpButton.Click += new EventHandler(HelpButton_Click);
		_ViewSwitcherTree.HideSelection = false;
		resources.ApplyResources(_ViewSwitcherTree, "_ViewSwitcherTree");
		_ViewSwitcherTree.Name = "_ViewSwitcherTree";
		_ViewSwitcherTree.Scrollable = false;
		_ViewSwitcherTree.BeforeCollapse += new TreeViewCancelEventHandler(ViewSwitcherTree_BeforeCollapse);
		_ViewSwitcherTree.BeforeSelect += new TreeViewCancelEventHandler(ViewSwitcherTree_BeforeSelect);
		_ViewSwitcherTree.AfterSelect += new TreeViewEventHandler(ViewSwitcherTree_AfterSelect);
		resources.ApplyResources(_CurrentViewPanel, "_CurrentViewPanel");
		_CurrentViewPanel.Name = "_CurrentViewPanel";
		AcceptButton = _OkButton;
		resources.ApplyResources(this, "$this");
		AutoScaleMode = AutoScaleMode.Font;
		CancelButton = _CancelButton;
		Controls.Add(_CurrentViewPanel);
		Controls.Add(_ViewSwitcherTree);
		Controls.Add(_HelpButton);
		Controls.Add(_CancelButton);
		Controls.Add(_OkButton);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		Name = "CurrentWndOptions";
		ShowInTaskbar = false;
		ResumeLayout(false);
	}

	private void HelpButton_Click(object sender, EventArgs e)
	{
		DisplayHelpTopicForCurrentView();
	}

	private void ViewSwitcherTree_AfterSelect(object sender, TreeViewEventArgs e)
	{
		TreeNodeContext nodeContext = GetNodeContext(e.Node);
		if (!nodeContext.IsFolder)
		{
			ShowOptionsPage(nodeContext);
			return;
		}

		if (!e.Node.IsExpanded)
		{
			e.Node.Expand();
		}

		if (e.Node.Nodes.Count > 0)
		{
			ShowOptionsPage(nodeContext);
		}
	}

	private void ViewSwitcherTree_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
	{
	}

	private void ViewSwitcherTree_BeforeSelect(object sender, TreeViewCancelEventArgs e)
	{
		if (GetNodeContext(e.Node).ControlIndex != _CurrentViewIndex && !ValidateCurrentView())
		{
			e.Cancel = true;
		}
	}

	private void DisplayHelpTopicForCurrentView()
	{
		string helpKeyword = null; // _OptionViews[_CurrentViewIndex].GetHelpKeyword();
		if (!string.IsNullOrEmpty(helpKeyword))
		{
			DisplayHelpTopic(helpKeyword);
		}
	}

	private void DisplayHelpTopic(string keyWord)
	{
		try
		{
			(Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsHelp)) as Microsoft.VisualStudio.VSHelp.Help)?.DisplayTopicFromF1Keyword(keyWord);
		}
		catch (SystemException e)
		{
			Tracer.LogExCatch(GetType(), e);
		}
	}

	private void ShowOptionsPage(TreeNodeContext cx)
	{
		if (cx == null)
		{
			return;
		}

		int currentViewIndex = _CurrentViewIndex;
		int controlIndex = cx.ControlIndex;
		if (currentViewIndex != controlIndex)
		{
			_CurrentViewIndex = controlIndex;
			_CurrentViewPanel.SuspendLayout();
			if (_CurrentViewPanel.Controls.Count == 1)
			{
				_CurrentViewPanel.Controls[0].Visible = false;
			}

			_CurrentViewPanel.Controls.Clear();
			Control control = _OptionViews[_CurrentViewIndex].Grid;
			_CurrentViewPanel.Controls.Add(control);
			control.Visible = false;
			if (!_OptionViewsInitialized[_CurrentViewIndex])
			{
				_OptionViewsInitialized[_CurrentViewIndex] = true;
			}

			control.Bounds = _CurrentViewPanel.DisplayRectangle;
			control.Dock = DockStyle.Fill;
			_CurrentViewPanel.ResumeLayout();
			control.Visible = true;
		}
	}

	protected bool ValidateCurrentView()
	{
		return true; // _OptionViews[_CurrentViewIndex].ValidateValuesInControls();
	}

	private bool CycleView(bool forward)
	{
		return true;
	}

	protected void SetControls(IBSettingsPage[] pages)
	{
		_OptionViews = pages;
		_OptionViewsInitialized = new bool[pages.Length];

		for (int i = 0; i < _OptionViewsInitialized.Length; i++)
			_OptionViewsInitialized[i] = false;
	}

	protected void AddTreeNodes(TreeNode[] nodes, TreeNode initialView)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			_ViewSwitcherTree.Nodes.Add(nodes[i]);
		}

		_CurrentViewIndex = -1;
		TreeNodeContext nodeContext = GetNodeContext(initialView);
		ShowOptionsPage(nodeContext);
		_ViewSwitcherTree.SelectedNode = initialView;
		ActiveControl = _OptionViews[nodeContext.ControlIndex].Grid;
	}

	protected void AssociateTreeNodeContext(TreeNode node, TreeNodeContext cx)
	{
		node.Tag = cx;
	}

	protected TreeNodeContext GetNodeContext(TreeNode node)
	{
		return node.Tag as TreeNodeContext;
	}


}
