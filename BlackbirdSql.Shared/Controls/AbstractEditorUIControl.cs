// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TabbedEditorUI

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Controls.Tabs;
using BlackbirdSql.Shared.Controls.Widgets;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Shared.Controls;

[DesignerCategory("code")]


public abstract class AbstractEditorUIControl : Control, IServiceProvider
{

	public AbstractEditorUIControl(IBsTabbedEditorPane tabbedEditor, Guid toolbarGuid, uint mnuIdTabbedEditorToolbar)
		: base()
	{
		SuspendLayout();

		try
		{
			TabbedEditor = tabbedEditor;
			_Provider = tabbedEditor;
			InitializeComponent();


			_Panel.SuspendLayout();

			try
			{
				BackColor = VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_WINDOW);
				_Panel.BackColor = VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_WINDOW);
				AddCustomControlsToPanel(_Panel);
				_SplitContainer.PanelTop.SizeChanged += Panel_SizeChanged;
				_SplitContainer.PanelBottom.SizeChanged += Panel_SizeChanged;
				_SplitContainer.PanelSwappedEvent += SplitContainer_PanelSwapped;
				_SplitContainer.IsSplitterVisibleChangedEvent += SplitContainer_IsSplitterVisibleChanged;
				_SplitContainer.TabActivationRequestEvent += SplitContainer_TabActivationRequest;
				_SplitContainer.UseCustomTabActivation = true;
				_SplitContainer.CustomizeSplitterBarButton(VSConstants.LOGVIEWID_Designer, EnSplitterBarButtonDisplayStyle.ImageAndText,
					ControlsResources.ToolStripButton_SplitContainerButton1Text, ControlsResources.ImgDesign);
				_SplitContainer.CustomizeSplitterBarButton(VSConstants.LOGVIEWID_TextView, EnSplitterBarButtonDisplayStyle.ImageAndText,
					ControlsResources.ToolStripButton_Sql_Button_Text, ControlsResources.ImgSql);
			}
			finally
			{
				_Panel.ResumeLayout(performLayout: false);
			}
		}
		finally
		{
			ResumeLayout(performLayout: false);
		}

		PerformLayout();
	}



	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			SuspendLayout();
			EditorTabCollection tabs = _Tabs;
			_Tabs = null;
			tabs?.Clear();
			_Provider = null;
			_SplitContainer.SplitterBar.Dispose();
		}
		base.Dispose(disposing);
	}





	private class EditorTabCollection(AbstractEditorUIControl frame) : Collection<AbstruseEditorTab>
	{
		private readonly AbstractEditorUIControl _TabbedEditorUI = frame;

		protected override void ClearItems()
		{
			using (IEnumerator<AbstruseEditorTab> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					enumerator.Current.Dispose();
				}
			}
			base.ClearItems();
		}

		protected override void InsertItem(int index, AbstruseEditorTab item)
		{
			item.Owner = _TabbedEditorUI;
			item.ShownEvent += _TabbedEditorUI.OnTabShown;
			item.ActiveChangedEvent += _TabbedEditorUI.OnActiveTabChanged;
			base.InsertItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			base[index].Dispose();
			base.RemoveItem(index);
		}

		protected override void SetItem(int index, AbstruseEditorTab item)
		{
			throw new NotImplementedException();
		}
	}

	private EditorTabCollection _Tabs;

	private IServiceProvider _Provider;

	private string _DesignerMessage;

	private readonly Panel _DesignerPanelAdditionalPanelFirst = new Panel();

	public EventHandler OnFocusHandler;

	private Panel _Panel;

	private SplitViewContainer _SplitContainer;

	private ToolbarHost _ToolbarHostCtl;

	protected IBsTabbedEditorPane TabbedEditor { get; private set; }

	public AbstruseEditorTab ActiveTab
	{
		get
		{
			foreach (AbstruseEditorTab tab in Tabs)
			{
				if (tab.IsActive && tab.IsVisible)
				{
					return tab;
				}
			}
			return null;
		}
	}

	public AbstruseEditorTab TopEditorTab
	{
		get
		{
			return FindActiveTab(isTopTab: true);
		}
		set
		{
			TopPanel = value.ParentPanel;
			ShowTab(value, isTopTab: true);
		}
	}

	public AbstruseEditorTab BottomEditorTab
	{
		get
		{
			return FindActiveTab(isTopTab: false);
		}
		set
		{
			BottomPanel = value.ParentPanel;
			ShowTab(value, isTopTab: false);
		}
	}

	public string DesignerMessage
	{
		get
		{
			return _DesignerMessage;
		}
		set
		{
			string text = value?.Replace("\\r\\n", "\r\n");
			if (!string.Equals(_DesignerMessage, text, StringComparison.CurrentCulture))
			{
				_DesignerMessage = text;
				_SplitContainer.PanelDesign.Invalidate();
				_SplitContainer.PanelDesign.Update();
			}
		}
	}

	public Collection<AbstruseEditorTab> Tabs => _Tabs ??= new EditorTabCollection(this);


	public ToolbarHost ToolbarHostCtl => _ToolbarHostCtl;


	public IWin32Window TopPanel
	{
		get
		{
			return _SplitContainer.PanelTop;
		}
		set
		{
			_SplitContainer.PanelTop = value as Panel;
		}
	}

	public IWin32Window BottomPanel
	{
		get
		{
			return _SplitContainer.PanelBottom;
		}
		set
		{
			_SplitContainer.PanelBottom = value as Panel;
		}
	}

	public IWin32Window DesignerPaneAdditionalPanelFirst
	{
		get
		{
			_DesignerPanelAdditionalPanelFirst.SizeChanged += Panel_SizeChanged;
			return _DesignerPanelAdditionalPanelFirst;
		}
	}

	public bool IsSplitterVisible
	{
		get
		{
			if (_SplitContainer != null)
			{
				return _SplitContainer.IsSplitterVisible;
			}
			return false;
		}
	}

	public SplitViewContainer SplitViewContainer => _SplitContainer;

	public event EventHandler TabActivatedEvent;



	public void InitializeToolbarHost(AbstractTabbedEditorPane tabbedEditor, Guid clsidCmdSet, uint menuIdTabbedEditorToolbar)
	{
		if (_ToolbarHostCtl == null)
		{
			Diag.ThrowIfNotOnUIThread();

			_ToolbarHostCtl = new ToolbarHost();
			_ToolbarHostCtl.SuspendLayout();
			_Panel.SuspendLayout();
			SuspendLayout();

			_ToolbarHostCtl.AccessibleName = ControlsResources.ToolBarHost_AccessibleName;

			try
			{
				_ToolbarHostCtl.Dock = DockStyle.Top;
				_ToolbarHostCtl.Name = "TabbedEditorUIToolbarHost";
				_ToolbarHostCtl.TabStop = false;
				_Panel.Controls.Add(_ToolbarHostCtl);
			}
			finally
			{
				ResumeLayout(performLayout: false);
				_Panel.ResumeLayout(performLayout: false);
				_ToolbarHostCtl.ResumeLayout(performLayout: false);
			}

			PerformLayout();

			if (tabbedEditor != null && Guid.Empty != clsidCmdSet &&
				Package.GetGlobalService(typeof(SVsUIShell)) as SVsUIShell is IVsUIShell4 uiShell)
			{
				_ToolbarHostCtl.SetToolbar(uiShell, clsidCmdSet, menuIdTabbedEditorToolbar, tabbedEditor);
			}
		}
	}

	private void OnColorsChanged(object sender, EventArgs e)
	{
	}

	private void SetFontStyles(ControlCollection controls)
	{
		foreach (Control control in controls)
		{
			if (control.Controls != null)
			{
				SetFontStyles(control.Controls);
			}
			if (control.Font != Font)
			{
				try
				{
					control.Font = new Font(Font, control.Font.Style);
				}
				catch (ArgumentException)
				{
				}
			}
			if (control is not ToolStrip toolStrip || toolStrip.Items == null)
			{
				continue;
			}
			foreach (ToolStripItem item in toolStrip.Items)
			{
				if (item.Font != Font)
				{
					try
					{
						item.Font = new Font(Font, item.Font.Style);
					}
					catch (ArgumentException)
					{
					}
				}
			}
		}
	}

	private void ShowTab(AbstruseEditorTab newTab, bool isTopTab)
	{
		AbstruseEditorTab editorTab = FindActiveTab(isTopTab);
		if (editorTab != newTab)
		{
			// Evs.Trace(GetType(), nameof(ShowTab), "logview: {0}.", newTab.LogicalView);
			newTab?.Show();
			editorTab?.Hide();
		}
	}

	protected virtual void AddCustomControlsToPanel(Panel panel)
	{
	}



	public void LoadWindowState(string section)
	{
		_SplitContainer?.LoadWindowState(section);
	}

	public void SaveWindowState(string section)
	{
		_SplitContainer?.SaveWindowState(section);
	}

	private void EnsureTopPaneActive()
	{
		if (!_SplitContainer.IsSplitterVisible)
		{
			AbstruseEditorTab bottomEditorTab = BottomEditorTab;
			AbstruseEditorTab topEditorTab = TopEditorTab;
			bottomEditorTab?.UpdateActive(isActive: false);
			topEditorTab?.UpdateActive(isActive: true);
		}
	}

	public void ActivateTab(AbstruseEditorTab tab, EnTabViewMode mode)
	{
		if (tab == null)
			return;

		switch (mode)
		{
			case EnTabViewMode.Maximize:
				ShowFullView(tab);
				break;
			case EnTabViewMode.Split:
				_SplitContainer.IsSplitterVisible = true;
				if (tab != null)
				{
					if (tab.IsTopTab)
					{
						BottomEditorTab.UpdateActive(isActive: false);
						TopEditorTab = tab;
						TopEditorTab.UpdateActive(isActive: true);
						TopEditorTab.Activate();
					}
					else
					{
						BottomEditorTab = tab;
						TopEditorTab.UpdateActive(isActive: false);
						BottomEditorTab.UpdateActive(isActive: true);
						BottomEditorTab.Activate();
					}
				}
				break;
			default:
				if (!_SplitContainer.IsSplitterVisible && !tab.IsTopTab)
				{
					ShowFullView(tab);
				}
				break;
		}
		tab.Activate();
	}

	private void ShowFullView(AbstruseEditorTab tab)
	{
		_SplitContainer.ShowView(tab);
	}

	private AbstruseEditorTab FindActiveTab(bool isTopTab)
	{
		foreach (AbstruseEditorTab tab in Tabs)
		{
			if (tab.IsTopTab == isTopTab && tab.IsVisible)
			{
				return tab;
			}
		}
		return null;
	}

	public AbstruseEditorTab GetTabForView(Guid logicalView)
	{
		foreach (AbstruseEditorTab tab in Tabs)
		{
			if (tab.LogicalView == logicalView)
			{
				return tab;
			}
		}
		return null;
	}

	public IVsToolWindowToolbarHost GetVsToolbarHost()
	{
		if (_ToolbarHostCtl != null)
		{
			return _ToolbarHostCtl.VsToolbarHost;
		}
		return null;
	}

	protected override object GetService(Type service)
	{
		object obj = null;
		if (_Provider != null)
		{
			obj = _Provider.GetService(service);
		}
		obj ??= base.GetService(service);
		return obj;
	}

	private void OnActiveTabChanged(object sender, EventArgs e)
	{
		if (sender is AbstruseEditorTab editorTab && editorTab.IsActive)
		{
			_SplitContainer.UpdateActiveTab(editorTab.LogicalView);
			TabActivatedEvent?.Invoke(sender, e);
			return;
		}
		AbstruseEditorTab activeTab = ActiveTab;
		if (activeTab == null)
		{
			_SplitContainer.ClearActiveTab();
			return;
		}
		_SplitContainer.UpdateActiveTab(activeTab.LogicalView);
		TabActivatedEvent?.Invoke(activeTab, e);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		ActiveTab?.Activate();
		OnFocusHandler?.Invoke(this, e);
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		AbstruseEditorTab topEditorTab = TopEditorTab;
		if (topEditorTab != null)
		{
			SizeTab(topEditorTab);
		}
	}

	public void Panel_SizeChanged(object sender, EventArgs e)
	{
		if (sender == _SplitContainer.PanelTop)
		{
			AbstruseEditorTab topEditorTab = TopEditorTab;
			if (topEditorTab != null)
			{
				SizeTab(topEditorTab);
			}
		}
		else
		{
			AbstruseEditorTab bottomEditorTab = BottomEditorTab;
			if (bottomEditorTab != null)
			{
				SizeTab(bottomEditorTab);
			}
		}
	}

	private void SplitContainer_PanelSwapped(object sender, EventArgs e)
	{
		AbstruseEditorTab bottomEditorTab = BottomEditorTab;
		foreach (AbstruseEditorTab tab in Tabs)
		{
			tab.IsTopTab = !tab.IsTopTab;
			SizeTab(tab);
			if (!tab.IsTopTab || tab.IsVisible || tab != bottomEditorTab)
			{
				continue;
			}
			if (!_SplitContainer.IsSplitterVisible)
			{
				_SplitContainer.UpdateActiveTab(tab.LogicalView);
			}
			bool flag = tab.EditorTabType == EnEditorTabType.TopDesign || tab.EditorTabType == EnEditorTabType.BottomDesign;
			try
			{
				if (flag)
				{
					DesignerMessage = "";
				}
				TopEditorTab = tab;
			}
			finally
			{
				if (flag)
				{
					DesignerMessage = null;
				}
			}
		}
		EnsureTopPaneActive();
	}

	private void SplitContainer_IsSplitterVisibleChanged(object sender, EventArgs e)
	{
		if (_SplitContainer.IsSplitterVisible)
		{
			AbstruseEditorTab editorTab = null;
			foreach (AbstruseEditorTab tab in Tabs)
			{
				if (!tab.IsTopTab)
				{
					if (tab.IsVisible)
					{
						editorTab = null;
						break;
					}
					editorTab = tab;
				}
			}
			if (editorTab != null)
			{
				bool flag = editorTab.EditorTabType == EnEditorTabType.TopDesign || editorTab.EditorTabType == EnEditorTabType.BottomDesign;
				try
				{
					if (flag)
					{
						DesignerMessage = "";
					}
					if (editorTab != null)
					{
						BottomEditorTab = editorTab;
					}
				}
				finally
				{
					if (flag)
					{
						DesignerMessage = null;
					}
				}
			}
		}
		EnsureTopPaneActive();
	}

	private void SplitContainer_TabActivationRequest(object sender, TabActivationEventArgs e)
	{
		AbstruseEditorTab tabForView = GetTabForView(e.LogicalView);
		if (e.ShowAtTop == true && !tabForView.IsTopTab)
		{
			_SplitContainer.SwapPanels();
		}
		if (tabForView != null)
		{
			if (tabForView.IsTopTab)
			{
				BottomEditorTab.UpdateActive(isActive: false);
				TopEditorTab = tabForView;
				TopEditorTab.UpdateActive(isActive: true);
				TopEditorTab.Activate();
			}
			else
			{
				BottomEditorTab = tabForView;
				TopEditorTab.UpdateActive(isActive: false);
				BottomEditorTab.UpdateActive(isActive: true);
				BottomEditorTab.Activate();
			}
		}
	}

	private void OnTabShown(object sender, EventArgs e)
	{
		BeginInvoke((EventHandler)delegate
		{
			AbstruseEditorTab tab = (AbstruseEditorTab)sender;
			SizeTab(tab);
		});
	}

	public void SizeTab(AbstruseEditorTab tab)
	{
		if (!tab.IsClosed && (tab.IsVisible || tab.IsShowing))
		{
			tab.SetBounds(_SplitContainer.GetAvailableBounds(tab.IsTopTab));
		}
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		return GetService(serviceType);
	}

	private void InitializeComponent()
	{
		this._Panel = new System.Windows.Forms.Panel();

		this._Panel.SuspendLayout();
		this._SplitContainer = new BlackbirdSql.Shared.Controls.Widgets.SplitViewContainer();
		this._SplitContainer.SuspendLayout();
		base.SuspendLayout();

		try
		{
			this._SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this._SplitContainer.Name = "_SplitContainer";
			this._SplitContainer.TabStop = false;
			this._Panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._Panel.Controls.Add(this._SplitContainer);
			this._Panel.Name = "_Panel";
			base.Controls.Add(this._Panel);
			base.Name = "AbstractEditorUIControl";
		}
		finally
		{
			base.ResumeLayout(false);
			this._SplitContainer.ResumeLayout(false);
			this._Panel.ResumeLayout(false);
		}

		base.PerformLayout();
	}
}
