#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;

using BlackbirdSql.Common.Config;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core.Diagnostics;

using Microsoft.VisualStudio.VSHelp;


namespace BlackbirdSql.Common.Controls.ToolsOptions
{
	public class CurrentWndOptions : Form
	{
		protected class TreeNodeContext
		{
			private readonly int _controlIndex = -1;

			private readonly bool _folder;

			public int ControlIndex => _controlIndex;

			public bool IsFolder => _folder;

			private TreeNodeContext()
			{
			}

			public TreeNodeContext(int controlIndex, bool folder)
			{
				_controlIndex = controlIndex;
				_folder = folder;
			}
		}

		protected int _folderImageIndex;

		protected int _leafSelectedImageIndex = 1;

		protected int _leafImageIndex = 2;

		protected ToolsOptionsBaseControl[] _optionViews;

		private bool[] _optionViewsInitialized;

		private int _currentViewIndex;

		private Button _okButton;

		private Button _helpButton;

		private TreeView _viewSwitcherTree;

		private Panel _currentViewPanel;

		private Separator _buttonsSeparator;

		private Button _cancelButton;

		public CurrentWndOptions()
		{
			InitializeComponent();
			Icon = ControlsResources.Properties_16x;
			Font = VsFontColorPreferences.EnvironmentFont;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			if (DialogResult == DialogResult.OK && !ValidateCurrentView())
			{
				e.Cancel = true;
			}
		}

		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			DisplayHelpTopicForCurrentView();
			hevent.Handled = true;
		}

		protected override bool ProcessKeyPreview(ref Message m)
		{
			if (m.Msg == 256)
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
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Expected O, but got Unknown
			ComponentResourceManager resources = new ComponentResourceManager(typeof(CurrentWndOptions));
			_okButton = new Button();
			_cancelButton = new Button();
			_helpButton = new Button();
			_viewSwitcherTree = new TreeView();
			_currentViewPanel = new Panel();
			_buttonsSeparator = new Separator();
			SuspendLayout();
			resources.ApplyResources(_okButton, "_okButton");
			_okButton.DialogResult = DialogResult.OK;
			_okButton.Name = "_okButton";
			resources.ApplyResources(_cancelButton, "_cancelButton");
			_cancelButton.DialogResult = DialogResult.Cancel;
			_cancelButton.Name = "_cancelButton";
			resources.ApplyResources(_helpButton, "_helpButton");
			_helpButton.Name = "_helpButton";
			_helpButton.Click += new EventHandler(HelpButton_Click);
			_viewSwitcherTree.HideSelection = false;
			resources.ApplyResources(_viewSwitcherTree, "_viewSwitcherTree");
			_viewSwitcherTree.Name = "_viewSwitcherTree";
			_viewSwitcherTree.Scrollable = false;
			_viewSwitcherTree.BeforeCollapse += new TreeViewCancelEventHandler(ViewSwitcherTree_BeforeCollapse);
			_viewSwitcherTree.BeforeSelect += new TreeViewCancelEventHandler(ViewSwitcherTree_BeforeSelect);
			_viewSwitcherTree.AfterSelect += new TreeViewEventHandler(ViewSwitcherTree_AfterSelect);
			resources.ApplyResources(_currentViewPanel, "_currentViewPanel");
			_currentViewPanel.Name = "_currentViewPanel";
			resources.ApplyResources(_buttonsSeparator, "_buttonsSeparator");
			((Control)(object)_buttonsSeparator).Name = "_buttonsSeparator";
			AcceptButton = _okButton;
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = _cancelButton;
			Controls.Add((Control)(object)_buttonsSeparator);
			Controls.Add(_currentViewPanel);
			Controls.Add(_viewSwitcherTree);
			Controls.Add(_helpButton);
			Controls.Add(_cancelButton);
			Controls.Add(_okButton);
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
			if (GetNodeContext(e.Node).ControlIndex != _currentViewIndex && !ValidateCurrentView())
			{
				e.Cancel = true;
			}
		}

		private void DisplayHelpTopicForCurrentView()
		{
			string helpKeyword = _optionViews[_currentViewIndex].GetHelpKeyword();
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

			int currentViewIndex = _currentViewIndex;
			int controlIndex = cx.ControlIndex;
			if (currentViewIndex != controlIndex)
			{
				_currentViewIndex = controlIndex;
				_currentViewPanel.SuspendLayout();
				if (_currentViewPanel.Controls.Count == 1)
				{
					_currentViewPanel.Controls[0].Visible = false;
				}

				_currentViewPanel.Controls.Clear();
				Control control = _optionViews[_currentViewIndex];
				_currentViewPanel.Controls.Add(control);
				control.Visible = false;
				if (!_optionViewsInitialized[_currentViewIndex])
				{
					_optionViewsInitialized[_currentViewIndex] = true;
				}

				control.Bounds = _currentViewPanel.DisplayRectangle;
				control.Dock = DockStyle.Fill;
				_currentViewPanel.ResumeLayout();
				control.Visible = true;
			}
		}

		private bool ValidateCurrentView()
		{
			return _optionViews[_currentViewIndex].ValidateValuesInControls();
		}

		private bool CycleView(bool forward)
		{
			return true;
		}

		protected void SetControls(ToolsOptionsBaseControl[] c)
		{
			_optionViews = c;
			_optionViewsInitialized = new bool[c.Length];
			for (int i = 0; i < _optionViewsInitialized.Length; i++)
			{
				_optionViewsInitialized[i] = false;
			}
		}

		protected void AddTreeNodes(TreeNode[] nodes, TreeNode initialView)
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				_viewSwitcherTree.Nodes.Add(nodes[i]);
			}

			_currentViewIndex = -1;
			TreeNodeContext nodeContext = GetNodeContext(initialView);
			ShowOptionsPage(nodeContext);
			_viewSwitcherTree.SelectedNode = initialView;
			ActiveControl = _optionViews[nodeContext.ControlIndex];
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
}
