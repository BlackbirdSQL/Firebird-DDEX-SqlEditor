// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// Microsoft.VisualStudio.Shell.UI.Internal, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.PlatformUI.Packages.Rdt.Model.WaitForSavesViewModel
using System;
using System.ComponentModel;
using Microsoft;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI.Packages.Rdt;

internal class WaitForSavesViewModel : ObservableObject, IDisposable
{
	private readonly ActiveTaskCollection<string> _activeTasks;

	private readonly string _messageFormat;

	private string _message;

	private int _activeSaveCount;

	public string Title { get; }

	public string Message
	{
		get
		{
			return _message;
		}
		private set
		{
			SetProperty(ref _message, value, "Message");
		}
	}

	public string ButtonText { get; }

	public int TotalSaveCount { get; }

	public int ActiveSaveCount
	{
		get
		{
			return _activeSaveCount;
		}
		private set
		{
			if (SetProperty(ref _activeSaveCount, value, "ActiveSaveCount"))
			{
				UpdateMessage();
			}
		}
	}

	private int CompletedSaveCount => TotalSaveCount - ActiveSaveCount;

	public WaitForSavesReason Reason { get; }

	public WaitForSavesViewModel(ActiveTaskCollection<string> activeTasks, WaitForSavesReason reason)
	{
		switch (reason)
		{
		case WaitForSavesReason.ClosingApplication:
			_messageFormat = Resources.WaitForSaveDialog_ApplicationMessageFormat;
			ButtonText = Resources.WaitForSaveDialog_ApplicationButtonText;
			break;
		case WaitForSavesReason.ClosingSolution:
			_messageFormat = Resources.WaitForSaveDialog_SolutionMessageFormat;
			ButtonText = Resources.WaitForSaveDialog_SolutionButtonText;
			break;
		case WaitForSavesReason.ClosingProject:
			_messageFormat = Resources.WaitForSaveDialog_ProjectMessageFormat;
			ButtonText = Resources.WaitForSaveDialog_ProjectButtonText;
			break;
		default:
			throw new ArgumentException($"Unexpected reason: {reason}", "reason");
		}
		_activeTasks = Requires.NotNull(activeTasks, "activeTasks");
		_activeTasks.PropertyChanged += ActiveTasks_PropertyChanged;
		Title = Utilities.GetAppName();
		Reason = reason;
		TotalSaveCount = _activeTasks.KeyCount;
		ActiveSaveCount = TotalSaveCount;
		_message = UpdateMessage();
	}

	public void Dispose()
	{
		_activeTasks.PropertyChanged -= ActiveTasks_PropertyChanged;
	}

	private void ActiveTasks_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		string propertyName = e.PropertyName;
		if (propertyName == "KeyCount")
		{
			ActiveSaveCount = _activeTasks.KeyCount;
		}
	}

	private string UpdateMessage()
	{
		return Message = string.Format(_messageFormat, CompletedSaveCount + 1, TotalSaveCount);
	}
}
