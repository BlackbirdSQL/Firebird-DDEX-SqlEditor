using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Markup;
using BlackbirdSql.Common.Controls.Converters.Wpf;
using BlackbirdSql.Common.Controls.Wpf;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Wpf.Controls.Widgets;
using BlackbirdSql.Wpf.Model;


namespace BlackbirdSql.Wpf.Controls;

/// <summary>
/// Interaction logic for ConnectionDialogFrame.xaml
/// </summary>
public partial class ConnectionDialogFrame : DialogWindow, IDisposable, IComponentConnector
{
	private bool _IsDisposed;

	private ConnectionDialogViewModel m_viewModel;

	private readonly EventsChannel _Channel;

	// private readonly IBDependencyManager _DependencyManager; // Firewall

	private const int C_GWL_STYLE = -16;

	private const int C_WS_MAXIMIZEBOX = -65537;

	private const int C_WS_MINIMIZEBOX = -131073;

	private IntPtr _windowHandle;


	public UIConnectionInfo UIConnectionInfo { get; private set; }

	private Traceable Trace { get; set; }

	public ConnectionDialogViewModel ViewModel
	{
		get
		{
			return m_viewModel;
		}
		set
		{
			if (m_viewModel != null)
			{
				m_viewModel.SectionInitializedEvent -= ViewModel_SectionInitialized;
				m_viewModel.SectionClosingEvent -= ViewModel_SectionClosing;
			}

			m_viewModel = value;
			if (m_viewModel != null)
			{
				m_viewModel.SectionInitializedEvent += ViewModel_SectionInitialized;
				m_viewModel.SectionClosingEvent += ViewModel_SectionClosing;
			}
		}
	}

	[DllImport("user32.dll")]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	public ConnectionDialogFrame(IBDependencyManager dependencyManager, EventsChannel channel, ConnectionDialogConfiguration config = null)
		: this(dependencyManager, channel, new UIConnectionInfo(), null, config)
	{
	}

	public ConnectionDialogFrame(IBDependencyManager dependencyManager, EventsChannel channel, UIConnectionInfo ci, VerifyConnectionDelegate verifierDelegate = null, ConnectionDialogConfiguration config = null)
	{
		Common.Cmd.CheckForNull(dependencyManager, "dependencyManager");
		Common.Cmd.CheckForNull(channel, "channel");
		Common.Cmd.CheckForNull(ci, "ci");
		config ??= new ConnectionDialogConfiguration
		{
			IsConnectMode = true
		};
		Trace = new Traceable(dependencyManager);
		InitializeComponent();
		// _DependencyManager = dependencyManager; // Firewall
		_Channel = channel;
		channel.ShowMessageEvent += ShowMessage;
		channel.CloseWindowEvent += CloseWindow;
		// channel.FirewallRuleDetectedEvent += OnFirewallRuleDetected;
		channel.AdvancedPropertiesRequestedEvent += OnAdvancedPropertiesRequested;
		UIConnectionInfo = ci;
		ViewModel = new ConnectionDialogViewModel(dependencyManager, _Channel, ci, verifierDelegate, config);
		DataContext = ViewModel;
		Closing += OnClosing;
		SourceInitialized += OnSourceInitialized;
	}

	private void OnSourceInitialized(object sender, EventArgs eventArgs)
	{
		_windowHandle = new WindowInteropHelper(this).Handle;
		HideMinimizeAndMaximizeButtons();
	}

	private void HideMinimizeAndMaximizeButtons()
	{
		_ = _windowHandle;
		SetWindowLong(_windowHandle, C_GWL_STYLE, GetWindowLong(_windowHandle, C_GWL_STYLE) & C_WS_MAXIMIZEBOX & C_WS_MINIMIZEBOX);
	}

	private void OnClosing(object sender, CancelEventArgs e)
	{
		UIConnectionInfo = ViewModel.UiConnectionInfo;
		ViewModel.HandleCancelClose();
	}

	private void OnAdvancedPropertiesRequested(object sender, AdvancedPropertiesRequestedEventArgs e)
	{
		Trace.AssertTraceEvent(e != null, TraceEventType.Error, EnUiTraceId.Connection, "event argument is null");
		try
		{
			if (e == null)
			{
				return;
			}

			IBPropertyAgent connectionProperties = e.ConnectionProperties;
			using DataConnectionAdvancedDlg dataConnectionAdvancedDialog = new DataConnectionAdvancedDlg(connectionProperties);
			NativeWindow nativeWindow = new NativeWindow();
			nativeWindow.AssignHandle(new WindowInteropHelper(this).Handle);
			if (dataConnectionAdvancedDialog.ShowDialog(nativeWindow) == System.Windows.Forms.DialogResult.OK)
			{
				ViewModel.ConnectionPropertyViewModel.InfoConnection.UpdatePropertyInfo(connectionProperties.ToUiConnectionInfo());
			}
		}
		catch (Exception ex)
		{
			_Channel.OnShowMessage(ex);
		}
	}

	public void CloseWindow(object sender, CloseWindowEventArgs e)
	{
		Trace.AssertTraceEvent(e != null, TraceEventType.Error, EnUiTraceId.Connection, "even argument is null");
		if (e != null)
		{
			DialogResult = e.Success;
			Close();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (_IsDisposed)
		{
			return;
		}

		if (disposing)
		{
			if (_Channel != null)
			{
				_Channel.ShowMessageEvent -= ShowMessage;
				_Channel.CloseWindowEvent -= CloseWindow;
				// _Channel.FirewallRuleDetectedEvent -= OnFirewallRuleDetected;
				_Channel.AdvancedPropertiesRequestedEvent -= OnAdvancedPropertiesRequested;
			}

			ViewModel.Dispose();
		}

		_IsDisposed = true;
	}

	private void ViewModel_SectionInitialized(object sender, SectionEventArgs e)
	{
		try
		{
			CreateSectionControl(e.SectionHost);
		}
		catch (Exception exception)
		{
			Trace.TraceException(TraceEventType.Error, EnUiTraceId.Connection, exception, "Failed to initialize the sections", 237, "ConnectionDialogFrame.xaml.cs", "ViewModel_SectionInitialized");
		}
	}

	private void CreateSectionControl(SectionHost sectionHost)
	{
		Trace.TraceEvent(TraceEventType.Information, EnUiTraceId.Connection, "SectionControlCreated section={0}", sectionHost.Id);
		SectionControl sectionControl = new()
		{
			DataContext = sectionHost,
			ShowProgressWhenBusy = ViewModel.Model.ShowSectionProgress
		};
		sectionControl.SetBinding(SectionControl.HeaderTextProperty, "Section.Title");
		sectionControl.SetBinding(SectionControl.ContentProperty, "Section.SectionContent");
		sectionControl.SetBinding(SectionControl.IsBusyProperty, "Section.IsBusy");

		CommonUtils.BindProperty(sectionControl, SectionControl.IsExpandedProperty, "Section.IsExpanded", BindingMode.TwoWay);

		System.Windows.Data.Binding binding = new("Section.IsVisible")
		{
			Source = sectionControl.DataContext,
			Converter = new TrueToVisibleConverter(),
			Mode = BindingMode.TwoWay
		};
		sectionControl.SetBinding(VisibilityProperty, binding);

		browsePageView.AddSection(sectionControl);
	}

	private void ViewModel_SectionClosing(object sender, SectionEventArgs e)
	{
	}

}
