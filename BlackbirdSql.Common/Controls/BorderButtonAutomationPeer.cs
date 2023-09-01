// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Framework.BorderButtonAutomationPeer

using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Controls;


public class BorderButtonAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider
{
	private readonly IBSectionControl _SectionControl;

	public ExpandCollapseState ExpandCollapseState
	{
		get
		{
			if (!_SectionControl.IsExpanded)
			{
				return ExpandCollapseState.Collapsed;
			}
			return ExpandCollapseState.Expanded;
		}
	}

	public BorderButtonAutomationPeer(BorderButton owner)
		: base(owner)
	{
		_SectionControl = owner.Parent as IBSectionControl;
		if (_SectionControl == null)
		{
			InvalidOperationException ex = new("Invalid parent type for the control.");
			Diag.Dug(ex);
			throw ex;
		}
	}

	public void Collapse()
	{
		_SectionControl.IsExpanded = false;
	}

	public void Expand()
	{
		_SectionControl.IsExpanded = true;
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Button;
	}

	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.ExpandCollapse)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}
}
