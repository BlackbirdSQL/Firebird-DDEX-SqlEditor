// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TabbedEditorUI
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Widgets;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Common.Controls;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Class is UIThread compliant.")]

[DesignerCategory("code")]
public class TabbedEditorUI : Control, IServiceProvider
{
	private class EditorTabCollection : Collection<AbstractEditorTab>
	{
		private readonly TabbedEditorUI _TabbedEditorUI;

		public EditorTabCollection(TabbedEditorUI frame)
		{
			_TabbedEditorUI = frame;
		}

		protected override void ClearItems()
		{
			using (IEnumerator<AbstractEditorTab> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					enumerator.Current.Dispose();
				}
			}
			base.ClearItems();
		}

		protected override void InsertItem(int index, AbstractEditorTab item)
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

		protected override void SetItem(int index, AbstractEditorTab item)
		{
			throw new NotImplementedException();
		}
	}

	private EditorTabCollection _Tabs;

	private IServiceProvider _Provider;

	private string _designerMessage;

	protected Guid _toolbarGuid;

	protected uint _toolbarID;

	private readonly Panel _designerPanelAdditionalPanelFirst = new Panel();

	public EventHandler OnFocusHandler;

	private Panel _Panel;

	private SplitViewContainer _splitContainer;

	private ToolbarHost _ToolbarHost;

	protected AbstractTabbedEditorPane TabbedEditorPane { get; private set; }

	public AbstractEditorTab ActiveTab
	{
		get
		{
			foreach (AbstractEditorTab tab in Tabs)
			{
				if (tab.IsActive && tab.IsVisible)
				{
					return tab;
				}
			}
			return null;
		}
	}

	public AbstractEditorTab TopEditorTab
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

	public AbstractEditorTab BottomEditorTab
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
			return _designerMessage;
		}
		set
		{
			string text = value?.Replace("\\r\\n", "\r\n");
			if (!string.Equals(_designerMessage, text, StringComparison.CurrentCulture))
			{
				_designerMessage = text;
				_splitContainer.PanelDesign.Invalidate();
				_splitContainer.PanelDesign.Update();
			}
		}
	}

	public Collection<AbstractEditorTab> Tabs => _Tabs ??= new EditorTabCollection(this);



	public IWin32Window TopPanel
	{
		get
		{
			return _splitContainer.PanelTop;
		}
		set
		{
			_splitContainer.PanelTop = value as Panel;
		}
	}

	public IWin32Window BottomPanel
	{
		get
		{
			return _splitContainer.PanelBottom;
		}
		set
		{
			_splitContainer.PanelBottom = value as Panel;
		}
	}

	public IWin32Window DesignerPaneAdditionalPanelFirst
	{
		get
		{
			_designerPanelAdditionalPanelFirst.SizeChanged += Panel_SizeChanged;
			return _designerPanelAdditionalPanelFirst;
		}
	}

	public bool IsSplitterVisible
	{
		get
		{
			if (_splitContainer != null)
			{
				return _splitContainer.IsSplitterVisible;
			}
			return false;
		}
	}

	public SplitViewContainer SplitViewContainer => _splitContainer;

	public event EventHandler TabActivatedEvent;

	public TabbedEditorUI()
	{
	}

	public TabbedEditorUI(AbstractTabbedEditorPane tabbedEditorPane)
		: this(tabbedEditorPane, Guid.Empty, 0u)
	{
	}

	public TabbedEditorUI(AbstractTabbedEditorPane tabbedEditorPane, Guid toolbarGuid, uint mnuIdTabbedEditorToolbar)
		: this()
	{
		SuspendLayout();
		TabbedEditorPane = tabbedEditorPane;
		_toolbarGuid = toolbarGuid;
		_toolbarID = mnuIdTabbedEditorToolbar;
		_Provider = tabbedEditorPane;
		InitializeComponent();
		_Panel.SuspendLayout();
		BackColor = VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_WINDOW);
		_Panel.BackColor = VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_WINDOW);
		AddCustomControlsToPanel(_Panel);
		_splitContainer.PanelTop.SizeChanged += Panel_SizeChanged;
		_splitContainer.PanelBottom.SizeChanged += Panel_SizeChanged;
		_splitContainer.PanelSwappedEvent += SplitContainer_PanelSwapped;
		_splitContainer.IsSplitterVisibleChangedEvent += SplitContainer_IsSplitterVisibleChanged;
		_splitContainer.TabActivationRequestEvent += SplitContainer_TabActivationRequest;
		_splitContainer.UseCustomTabActivation = true;
		_splitContainer.CustomizeSplitterBarButton(VSConstants.LOGVIEWID_Designer, EnSplitterBarButtonDisplayStyle.ImageAndText, ControlsResources.TabbedEditor_SplitContainerButton1Text, ControlsResources.design);
		_splitContainer.CustomizeSplitterBarButton(VSConstants.LOGVIEWID_TextView, EnSplitterBarButtonDisplayStyle.ImageAndText, "FB-SQL", ControlsResources.sql);
		ResumeLayout(performLayout: false);
		_Panel.ResumeLayout(performLayout: false);
		PerformLayout();
	}

	public void InitializeToolbarHost(AbstractTabbedEditorPane tabbedEditorPane, Guid clsidCmdSet, uint menuIdTabbedEditorToolbar)
	{
		if (_ToolbarHost == null)
		{
			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			_ToolbarHost = new ToolbarHost();
			_ToolbarHost.SuspendLayout();
			_Panel.SuspendLayout();
			_ToolbarHost.AccessibleName = ControlsResources.ToolBarHostAccessibleName;
			SuspendLayout();
			_ToolbarHost.Dock = DockStyle.Top;
			_ToolbarHost.Name = "TabbedEditorUIToolbarHost";
			_ToolbarHost.TabStop = false;
			_Panel.Controls.Add(_ToolbarHost);
			_Panel.ResumeLayout(performLayout: false);
			_ToolbarHost.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
			PerformLayout();

			if (tabbedEditorPane != null && Guid.Empty != clsidCmdSet &&
				Package.GetGlobalService(typeof(SVsUIShell)) as SVsUIShell is IVsUIShell4 uiShell)
			{
				_ToolbarHost.SetToolbar(uiShell, clsidCmdSet, menuIdTabbedEditorToolbar, tabbedEditorPane);
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

	private void ShowTab(AbstractEditorTab newTab, bool isTopTab)
	{
		AbstractEditorTab editorTab = FindActiveTab(isTopTab);
		if (editorTab != newTab)
		{
			newTab?.Show();
			editorTab?.Hide();
		}
	}

	protected virtual void AddCustomControlsToPanel(Panel panel)
	{
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
			_splitContainer.SplitterBar.Dispose();
		}
		base.Dispose(disposing);
	}

	public void LoadWindowState(string section)
	{
		_splitContainer?.LoadWindowState(section);
	}

	public void SaveWindowState(string section)
	{
		_splitContainer?.SaveWindowState(section);
	}

	private void EnsureTopPaneActive()
	{
		if (!_splitContainer.IsSplitterVisible)
		{
			AbstractEditorTab bottomEditorTab = BottomEditorTab;
			AbstractEditorTab topEditorTab = TopEditorTab;
			bottomEditorTab?.UpdateActive(isActive: false);
			topEditorTab?.UpdateActive(isActive: true);
		}
	}

	public void ActivateTab(AbstractEditorTab tab, EnTabViewMode mode)
	{
		if (tab == null)
		{
			return;
		}
		switch (mode)
		{
			case EnTabViewMode.Maximize:
				ShowFullView(tab);
				break;
			case EnTabViewMode.Split:
				_splitContainer.IsSplitterVisible = true;
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
				if (!_splitContainer.IsSplitterVisible && !tab.IsTopTab)
				{
					ShowFullView(tab);
				}
				break;
		}
		tab.Activate();
	}

	private void ShowFullView(AbstractEditorTab tab)
	{
		_splitContainer.ShowView(tab);
	}

	private AbstractEditorTab FindActiveTab(bool isTopTab)
	{
		foreach (AbstractEditorTab tab in Tabs)
		{
			if (tab.IsTopTab == isTopTab && tab.IsVisible)
			{
				return tab;
			}
		}
		return null;
	}

	public AbstractEditorTab GetTabForView(Guid logicalView)
	{
		foreach (AbstractEditorTab tab in Tabs)
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
		if (_ToolbarHost != null)
		{
			return _ToolbarHost.VsToolbarHost;
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
		if (sender is AbstractEditorTab editorTab && editorTab.IsActive)
		{
			_splitContainer.UpdateActiveTab(editorTab.LogicalView);
			TabActivatedEvent?.Invoke(sender, e);
			return;
		}
		AbstractEditorTab activeTab = ActiveTab;
		if (activeTab == null)
		{
			_splitContainer.ClearActiveTab();
			return;
		}
		_splitContainer.UpdateActiveTab(activeTab.LogicalView);
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
		AbstractEditorTab topEditorTab = TopEditorTab;
		if (topEditorTab != null)
		{
			SizeTab(topEditorTab);
		}
	}

	public void Panel_SizeChanged(object sender, EventArgs e)
	{
		if (sender == _splitContainer.PanelTop)
		{
			AbstractEditorTab topEditorTab = TopEditorTab;
			if (topEditorTab != null)
			{
				SizeTab(topEditorTab);
			}
		}
		else
		{
			AbstractEditorTab bottomEditorTab = BottomEditorTab;
			if (bottomEditorTab != null)
			{
				SizeTab(bottomEditorTab);
			}
		}
	}

	private void SplitContainer_PanelSwapped(object sender, EventArgs e)
	{
		AbstractEditorTab bottomEditorTab = BottomEditorTab;
		foreach (AbstractEditorTab tab in Tabs)
		{
			tab.IsTopTab = !tab.IsTopTab;
			SizeTab(tab);
			if (!tab.IsTopTab || tab.IsVisible || tab != bottomEditorTab)
			{
				continue;
			}
			if (!_splitContainer.IsSplitterVisible)
			{
				_splitContainer.UpdateActiveTab(tab.LogicalView);
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
		if (_splitContainer.IsSplitterVisible)
		{
			AbstractEditorTab editorTab = null;
			foreach (AbstractEditorTab tab in Tabs)
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
		AbstractEditorTab tabForView = GetTabForView(e.LogicalView);
		if (e.ShowAtTop == true && !tabForView.IsTopTab)
		{
			_splitContainer.SwapPanels();
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
			AbstractEditorTab tab = (AbstractEditorTab)sender;
			SizeTab(tab);
		});
	}

	public void SizeTab(AbstractEditorTab tab)
	{
		if (!tab.IsClosed && (tab.IsVisible || tab.IsShowing))
		{
			tab.SetBounds(_splitContainer.GetAvailableBounds(tab.IsTopTab));
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
		this._splitContainer = new BlackbirdSql.Common.Controls.Widgets.SplitViewContainer();
		this._splitContainer.SuspendLayout();
		base.SuspendLayout();
		this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this._splitContainer.Name = "_splitContainer";
		this._splitContainer.TabStop = false;
		this._Panel.Dock = System.Windows.Forms.DockStyle.Fill;
		this._Panel.Controls.Add(this._splitContainer);
		this._Panel.Name = "_Panel";
		base.Controls.Add(this._Panel);
		base.Name = "TabbedEditorUI";
		this._splitContainer.ResumeLayout(false);
		this._Panel.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
