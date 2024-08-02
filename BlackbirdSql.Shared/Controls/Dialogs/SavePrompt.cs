// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// Microsoft.VisualStudio.Shell.UI.Internal, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.PlatformUI.Packages.Rdt.View.SavePrompt
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI.Packages.Rdt.Model;

public class SavePrompt : DialogWindow, IComponentConnector
{
	internal Button SaveButton;

	private bool _contentLoaded;

	public SavePrompt()
	{
		InitializeComponent();
		base.DataContextChanged += OnDataContextChanged;
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.OldValue is SavePromptViewModel savePromptViewModel)
		{
			savePromptViewModel.PropertyChanged -= OnModelPropertyChanged;
		}
		if (e.NewValue is SavePromptViewModel savePromptViewModel2)
		{
			savePromptViewModel2.PropertyChanged += OnModelPropertyChanged;
		}
	}

	private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		string propertyName = e.PropertyName;
		if (propertyName == "Result")
		{
			try
			{
				base.DialogResult = true;
			}
			catch (InvalidOperationException)
			{
				Close();
			}
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "8.0.2.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/Microsoft.VisualStudio.Shell.UI.Internal;V17.0.0.0;component/packages/rdt/view/saveprompt.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "8.0.2.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		if (connectionId == 1)
		{
			SaveButton = (Button)target;
		}
		else
		{
			_contentLoaded = true;
		}
	}
}
