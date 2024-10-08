// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.FindTargetAdapter

using BlackbirdSql.Shared.Controls;
using BlackbirdSql.Shared.Controls.Tabs;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared.Ctl;


public class FindTargetAdapter : IVsFindTarget, IVsFindTarget2, IBsVsFindTarget3
{

	public FindTargetAdapter(AbstractTabbedEditorPane tabbedEditor)
	{
		TabbedEditor = tabbedEditor;
	}


	protected AbstractTabbedEditorPane TabbedEditor { get; set; }





	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);



	public int Find(string pszSearch, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out uint pResult)
	{
		IVsFindTarget findTarget = GetFindTarget(ensureTabs: true);
		if (findTarget != null)
		{
			return findTarget.Find(pszSearch, grfOptions, fResetStartPoint, pHelper, out pResult);
		}
		pResult = 0u;
		return VSConstants.E_NOTIMPL;
	}

	public int GetCapabilities(bool[] pfImage, uint[] pgrfOptions)
	{
		IVsFindTarget findTarget = GetFindTarget(ensureTabs: false);
		if (findTarget != null)
		{
			return findTarget.GetCapabilities(pfImage, pgrfOptions);
		}
		if (pfImage != null && pfImage.Length != 0)
		{
			pfImage[0] = true;
		}
		if (pgrfOptions != null && pgrfOptions.Length != 0)
		{
			pgrfOptions[0] = (uint)__VSFINDOPTIONS.FR_ActionMask;
			pgrfOptions[0] |= (uint)__VSFINDOPTIONS.FR_SyntaxMask;
			pgrfOptions[0] |= (uint)__VSFINDOPTIONS.FR_CommonOptions;
			pgrfOptions[0] |= (uint)__VSFINDOPTIONS.FR_Selection;
			pgrfOptions[0] |= (uint)__VSFINDOPTIONS.FR_Backwards;
		}
		return 0;
	}

	public int GetCurrentSpan(TextSpan[] pts)
	{
		return GetFindTarget(ensureTabs: false)?.GetCurrentSpan(pts) ?? VSConstants.E_NOTIMPL;
	}

	public int GetFindState(out object ppunk)
	{
		IVsFindTarget findTarget = GetFindTarget(ensureTabs: false);
		if (findTarget != null)
		{
			return findTarget.GetFindState(out ppunk);
		}
		ppunk = null;
		return VSConstants.E_NOTIMPL;
	}

	public int GetMatchRect(RECT[] prc)
	{
		return GetFindTarget(ensureTabs: false)?.GetMatchRect(prc) ?? VSConstants.E_NOTIMPL;
	}

	public int GetProperty(uint propid, out object pvar)
	{
		IVsFindTarget findTarget = GetFindTarget(ensureTabs: false);
		if (findTarget != null)
		{
			return findTarget.GetProperty(propid, out pvar);
		}
		pvar = null;
		return VSConstants.E_NOTIMPL;
	}

	public int GetSearchImage(uint grfOptions, IVsTextSpanSet[] ppSpans, out IVsTextImage ppTextImage)
	{
		IVsFindTarget findTarget = GetFindTarget(ensureTabs: false);
		if (findTarget != null)
		{
			return findTarget.GetSearchImage(grfOptions, ppSpans, out ppTextImage);
		}
		ppTextImage = null;
		return VSConstants.E_NOTIMPL;
	}

	public int MarkSpan(TextSpan[] pts)
	{
		return GetFindTarget(ensureTabs: false)?.MarkSpan(pts) ?? VSConstants.E_NOTIMPL;
	}

	public int NavigateTo(TextSpan[] pts)
	{
		Diag.ThrowIfNotOnUIThread();

		IVsWindowFrame vsWindowFrame = (IVsWindowFrame)((System.IServiceProvider)TabbedEditor).GetService(typeof(SVsWindowFrame));

		if (vsWindowFrame != null)
		{
			int hresult = vsWindowFrame.Show();

			if (!__(hresult))
				return hresult;

			IVsFindTarget findTarget = GetFindTarget(ensureTabs: true);

			if (findTarget != null)
			{
				ActivateTabForNavigateTo();
				return findTarget.NavigateTo(pts);
			}

			return VSConstants.E_NOTIMPL;
		}
		return VSConstants.E_NOTIMPL;
	}

	protected virtual void ActivateTabForNavigateTo()
	{
		EditorUIControl tabControl = TabbedEditor.TabbedEditorUiCtl;

		foreach (AbstruseEditorTab tab in tabControl.Tabs)
		{
			if (tab.LogicalView == VSConstants.LOGVIEWID_TextView)
			{
				tabControl.ActivateTab(tab, EnTabViewMode.Default);
			}
		}
	}

	public int NotifyFindTarget(uint notification)
	{
		return (notification != 2 ? GetFindTarget(ensureTabs: false) : GetFindTarget(ensureTabs: true))?.NotifyFindTarget(notification) ?? 0;
	}

	public int Replace(string pszSearch, string pszReplace, uint grfOptions, int fResetStartPoint, IVsFindHelper pHelper, out int pfReplaced)
	{
		IVsFindTarget findTarget = GetFindTarget(ensureTabs: true);
		if (findTarget != null)
		{
			return findTarget.Replace(pszSearch, pszReplace, grfOptions, fResetStartPoint, pHelper, out pfReplaced);
		}
		pfReplaced = 0;
		return VSConstants.E_NOTIMPL;
	}

	public int SetFindState(object pUnk)
	{
		return GetFindTarget(ensureTabs: true)?.SetFindState(pUnk) ?? VSConstants.E_NOTIMPL;
	}

	private IVsFindTarget GetFindTarget(bool ensureTabs)
	{
		AbstractEditorUIControl tabControl = TabbedEditor.TabbedEditorUiCtl;

		if (tabControl == null || tabControl.Tabs == null)
			return null;

		IVsFindTarget vsFindTarget = null;

		if (ensureTabs && tabControl.Tabs.Count == 0)
		{
			TabbedEditor.EnsureTabs(activateTextView: true);
		}

		AbstruseEditorTab activeTab = tabControl.ActiveTab;

		if (activeTab != null)
		{
			vsFindTarget = activeTab.GetFindTarget();
			if (vsFindTarget != null)
			{
				return vsFindTarget;
			}
		}
		foreach (AbstruseEditorTab tab in tabControl.Tabs)
		{
			vsFindTarget = tab.GetView() as IVsFindTarget;
			if (vsFindTarget != null)
			{
				break;
			}
		}
		if (vsFindTarget == TabbedEditor)
		{
			return null;
		}
		return vsFindTarget;
	}

	public int NavigateTo2(IVsTextSpanSet pSpans, TextSelMode iSelMode)
	{
		int result = (int)Microsoft.VisualStudio.OLE.Interop.Constants.MSOCMDERR_E_NOTSUPPORTED;
		if (GetFindTarget(ensureTabs: true) is IVsFindTarget2 vsFindTarget)
		{
			result = vsFindTarget.NavigateTo2(pSpans, iSelMode);
		}
		return result;
	}

	public int IsNewUISupported
	{
		get
		{
			return 0;
		}
	}

	public int NotifyShowingNewUI()
	{
		return 0;
	}
}
