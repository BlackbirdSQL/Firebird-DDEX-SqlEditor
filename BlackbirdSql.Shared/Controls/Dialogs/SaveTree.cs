// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// Microsoft.VisualStudio.Shell.UI.Internal, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.PlatformUI.Packages.Rdt.SaveTree
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI.Packages.Rdt;
using Microsoft.VisualStudio.PlatformUI.Packages.Rdt.Model;
using Microsoft.VisualStudio.PlatformUI.Packages.Rdt.View;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Telemetry;
using Microsoft.VisualStudio.Threading;

internal class SaveTree : IDisposable
{
	private class SaveScope : IDisposable
	{
		private readonly SaveTree savetree;

		private readonly IVsStatusbar statusBar;

		private readonly int totalSaveItemCount;

		private object animationIndex;

		private uint m_dwCookie;

		private int savedItemCount;

		public SaveScope(SaveTree savetree, IVsStatusbar statusBar, int totalSaveItemCount)
		{
			this.savetree = Requires.NotNull(savetree, "savetree");
			this.statusBar = Requires.NotNull(statusBar, "statusBar");
			this.totalSaveItemCount = totalSaveItemCount;
			savetree.InteractiveSaveScope = this;
			animationIndex = 2;
			this.statusBar.Animation(Convert.ToInt32(value: true), ref animationIndex);
			this.statusBar.Progress(ref m_dwCookie, Convert.ToInt32(value: true), null, 0u, (uint)totalSaveItemCount);
			savetree.DataLossChoice = DataLossChoice.NoChoiceMade;
			savetree.ApplyDataLossChoiceToAll = false;
		}

		public void Dispose()
		{
			savetree.DataLossChoice = DataLossChoice.NoChoiceMade;
			savetree.ApplyDataLossChoiceToAll = false;
			statusBar.Progress(ref m_dwCookie, Convert.ToInt32(value: false), null, 0u, 0u);
			statusBar.Animation(Convert.ToInt32(value: false), ref animationIndex);
			savetree.InteractiveSaveScope = null;
		}

		public void UpdateStatusBar()
		{
			if (m_dwCookie != 0)
			{
				statusBar.Progress(ref m_dwCookie, Convert.ToInt32(value: true), null, (uint)savedItemCount++, (uint)totalSaveItemCount);
			}
		}
	}

	private readonly SaveTreeItem rootItem;

	private IVsHierarchy? scopeHierarchy;

	private uint scopeItemid = 4294967294u;

	private TelemetryEvent saveDocumentsTelemetryEvent;

	public DataLossChoice DataLossChoice { get; set; }

	public bool ApplyDataLossChoiceToAll { get; set; }

	public bool IsInOpenFolderMode { get; }

	public bool IsSavingSingleItem => rootItem.FirstChild?.IsSavingSingleItem ?? false;

	private SaveScope? InteractiveSaveScope { get; set; }

	public SaveTree(Microsoft.VisualStudio.PlatformUI.Packages.Rdt.RunningDocumentTable rdt, bool isInOpenFolderMode, bool shouldHideSolutionNode, TelemetryEvent saveDocumentsTelemetryEvent)
	{
		this.saveDocumentsTelemetryEvent = Requires.NotNull(saveDocumentsTelemetryEvent, "saveDocumentsTelemetryEvent");
		rootItem = new SaveTreeItem(Requires.NotNull(rdt, "rdt"), shouldHideSolutionNode, saveDocumentsTelemetryEvent);
		IsInOpenFolderMode = isInOpenFolderMode;
	}

	public void Dispose()
	{
		rootItem.Dispose();
	}

	public SaveTreeItem AddSaveTreeItem(IVsRunningDocumentTable4 rdt4, SaveOptions saveOpts, IVsHierarchy hierarchy, uint itemid, uint docCookie, bool addToRoot)
	{
		Requires.NotNull(rdt4, "rdt4");
		SaveTreeItem saveTreeItem = rootItem.FindSaveTreeItem(hierarchy, itemid);
		if (saveTreeItem != null)
		{
			if (saveTreeItem.DocCookie == 0)
			{
				saveTreeItem.DocCookie = docCookie;
			}
		}
		else
		{
			SaveTreeItem saveTreeItem2;
			if ((hierarchy == scopeHierarchy && itemid == scopeItemid) || addToRoot || IsInOpenFolderMode)
			{
				saveTreeItem2 = rootItem;
			}
			else
			{
				(IVsHierarchy? parentHierarchy, uint parentRelativeItemid) parentHierarchyReference = GetParentHierarchyReference(hierarchy);
				IVsHierarchy item = parentHierarchyReference.parentHierarchy;
				uint item2 = parentHierarchyReference.parentRelativeItemid;
				saveTreeItem2 = ((item != null) ? AddSaveTreeItem(rdt4, saveOpts, item, item2, 0u, addToRoot: false) : ((itemid == 4294967294u) ? rootItem : AddSaveTreeItem(rdt4, saveOpts, hierarchy, 4294967294u, 0u, addToRoot: false)));
			}
			saveTreeItem = new SaveTreeItem(rootItem.Rdt, rootItem.ShouldHideSolutionNode, saveOpts, hierarchy, itemid, docCookie, saveDocumentsTelemetryEvent);
			saveTreeItem2.AddChild(saveTreeItem);
		}
		return saveTreeItem;
	}

	public void AddSaveTreeItems(IVsRunningDocumentTable4 rdt4, VSSAVETREEITEM[] saveItems, bool addToRoot, IVsRdtBackChannel backChannel)
	{
		Requires.NotNull(rdt4, "rdt4");
		if (saveItems.Length > 1)
		{
			addToRoot = false;
		}
		for (int i = 0; i < saveItems.Length; i++)
		{
			VSSAVETREEITEM vSSAVETREEITEM = saveItems[i];
			uint num = vSSAVETREEITEM.docCookie;
			IVsHierarchy hierarchy;
			uint itemID;
			if (num == 0)
			{
				num = backChannel.CookieFromItem(vSSAVETREEITEM.pHier, vSSAVETREEITEM.itemid);
				hierarchy = vSSAVETREEITEM.pHier;
				itemID = vSSAVETREEITEM.itemid;
			}
			else
			{
				rdt4.GetDocumentHierarchyItem(num, out hierarchy, out itemID);
			}
			if (num != 0)
			{
				AddSaveTreeItem(rdt4, new SaveOptions(vSSAVETREEITEM.grfSave), hierarchy, itemID, num, addToRoot);
			}
		}
	}

	public void UpdateStatusBar()
	{
		InteractiveSaveScope?.UpdateStatusBar();
	}

	public int CountSaveItems()
	{
		return rootItem.CountSaveItems();
	}

	private async Task SaveChangesAsync(IAsyncServiceProvider asp, IVsSavePrimitives primitives, IVsProgress? progress, CancellationToken token)
	{
		Requires.NotNull(asp, "asp");
		Requires.NotNull(primitives, "primitives");
		int saveItemCount = CountSaveItems();
		if (saveItemCount == 0)
		{
			return;
		}
		Dictionary<SaveTreeItem, object> saveContexts = new Dictionary<SaveTreeItem, object>();
		try
		{
			BeforeSave(saveContexts);
			SaveScope saveScope2 = ((!primitives.IsInteractiveSave) ? null : new SaveScope(this, await asp.GetServiceAsync<SVsStatusbar, IVsStatusbar>(), saveItemCount));
			using (saveScope2)
			{
				SaveTreeItem saveTreeItem = rootItem;
				await saveTreeItem.SaveAsync(this, await asp.GetServiceAsync<SVsTaskSchedulerService, IVsTaskSchedulerService>(), await asp.GetServiceAsync<SVsRunningDocumentTable, IVsRdtBackChannel>(), primitives, progress, token);
			}
		}
		finally
		{
			AfterSave(saveContexts);
		}
	}

	private void BeforeSave(Dictionary<SaveTreeItem, object> saveContexts)
	{
		VisitItems(delegate(SaveTreeItem item, int _, Dictionary<SaveTreeItem, object>? saveContexts)
		{
			if (item.CanSave)
			{
				saveContexts[item] = item.BeforeSave();
			}
		}, saveContexts);
	}

	private void AfterSave(Dictionary<SaveTreeItem, object> saveContexts)
	{
		VisitItems(delegate(SaveTreeItem item, int _, Dictionary<SaveTreeItem, object>? saveContexts)
		{
			if (item.CanSave)
			{
				item.AfterSave(saveContexts[item]);
			}
		}, saveContexts);
	}

	public async Task<bool> PromptSaveChangesAsync(JoinableTaskFactory jtf, IAsyncServiceProvider asp, IVsSavePrimitives primitives, IVsProgress? progress, CancellationToken token)
	{
		Requires.NotNull(jtf, "jtf");
		Requires.NotNull(asp, "asp");
		Requires.NotNull(primitives, "primitives");
		if (rootItem == null)
		{
			throw new InvalidOperationException();
		}
		if (!rootItem.NeedsPrompt)
		{
			await SaveChangesAsync(asp, primitives, progress, token);
			return true;
		}
		await jtf.SwitchToMainThreadAsync(token);
		SavePromptViewModel savePromptViewModel = new SavePromptViewModel(this, await asp.GetServiceAsync<SVsSolution, IVsSolution>());
		MessageBoxResult messageBoxResult = (savePromptViewModel.HasDirtyItems ? MessageBoxResult.Yes : MessageBoxResult.No);
		if (savePromptViewModel.HasDirtyItemsToShow)
		{
			SavePrompt savePrompt = new SavePrompt
			{
				DataContext = savePromptViewModel
			};
			messageBoxResult = ((savePrompt.ShowModal() == true) ? savePromptViewModel.Result : MessageBoxResult.Cancel);
		}
		switch (messageBoxResult)
		{
		case MessageBoxResult.Yes:
			await SaveChangesAsync(asp, primitives, progress, token);
			return true;
		case MessageBoxResult.No:
			rootItem.PreventSaving();
			return false;
		case MessageBoxResult.Cancel:
			throw new OperationCanceledException();
		default:
			return false;
		}
	}

	public void SetScope(IVsHierarchy? hierarchy, uint itemid)
	{
		scopeHierarchy = hierarchy;
		scopeItemid = ((hierarchy == null) ? 4294967294u : itemid);
	}

	public void UpdateRootForAllSaveTreeItems(VSSAVETREEITEM[] saveItems, IVsRunningDocumentTable4 rdt4, IVsRdtBackChannel backChannel)
	{
		for (int i = 0; i < saveItems.Length; i++)
		{
			VSSAVETREEITEM vSSAVETREEITEM = saveItems[i];
			uint num = vSSAVETREEITEM.docCookie;
			IVsHierarchy hierarchy;
			if (num == 0)
			{
				num = backChannel.CookieFromItem(vSSAVETREEITEM.pHier, vSSAVETREEITEM.itemid);
				hierarchy = vSSAVETREEITEM.pHier;
			}
			else
			{
				rdt4.GetDocumentHierarchyItem(num, out hierarchy, out var _);
			}
			if (num != 0 && hierarchy != scopeHierarchy)
			{
				SetScope(null, uint.MaxValue);
				break;
			}
		}
	}

	internal void VisitItems<TState>(Action<SaveTreeItem, int, TState?> action, TState? state, bool visitChildrenFirst = false)
	{
		rootItem.VisitItems(action, state, 0, visitChildrenFirst);
	}

	internal bool VisitItems<TState>(Func<SaveTreeItem, int, TState?, bool> func, TState? state, bool visitChildrenFirst = false)
	{
		return rootItem.VisitItems(func, state, 0, visitChildrenFirst);
	}

	internal Task VisitItemsAsync<TState>(Func<SaveTreeItem, int, TState, Task> action, TState state, bool visitChildrenFirst = false)
	{
		return rootItem.VisitItemsAsync(action, state, 0, visitChildrenFirst);
	}

	private (IVsHierarchy? parentHierarchy, uint parentRelativeItemid) GetParentHierarchyReference(IVsHierarchy childHierarchy)
	{
		ThreadHelper.ThrowIfNotOnUIThread("GetParentHierarchyReference");
		if (ErrorHandler.Failed(childHierarchy.GetProperty(4294967294u, -2032, out var pvar)) || ErrorHandler.Failed(childHierarchy.GetProperty(4294967294u, -2033, out var pvar2)))
		{
			return (parentHierarchy: null, parentRelativeItemid: uint.MaxValue);
		}
		uint num3 = ((pvar2 is int num) ? ((uint)num) : ((!(pvar2 is uint num2)) ? uint.MaxValue : num2));
		uint item = num3;
		return (parentHierarchy: pvar as IVsHierarchy, parentRelativeItemid: item);
	}
}
