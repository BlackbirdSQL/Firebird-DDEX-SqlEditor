// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.DataConnectionDialog
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Controls;
using BlackbirdSql.Core.Controls.Interfaces;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.VisualStudio.Ddex.Controls.Events;
using BlackbirdSql.VisualStudio.Ddex.Ctl.DataTools;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using Microsoft.Data.ConnectionUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;


namespace BlackbirdSql.VisualStudio.Ddex.Controls.DataTools;

public partial class TDataConnectionDlg : Form, IBDataConnectionDlg
{
	private class TiDataSourceCollection(TDataConnectionDlg dialog)
		: ICollection<TDataSource>, IEnumerable<TDataSource>, IEnumerable
	{
		private readonly List<TDataSource> _DataSources = [];

		private readonly TDataConnectionDlg _Dialog = dialog;

		public int Count => _DataSources.Count;

		public bool IsReadOnly => _Dialog._ShowingDialog;

		public void Add(TDataSource item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			if (_Dialog._ShowingDialog)
			{
				throw new InvalidOperationException("ControlsResources.DataConnectionDlg_CannotModifyState");
			}
			if (!_DataSources.Contains(item))
			{
				_DataSources.Add(item);
			}
		}

		public bool Contains(TDataSource item)
		{
			return _DataSources.Contains(item);
		}

		public bool Remove(TDataSource item)
		{
			if (_Dialog._ShowingDialog)
			{
				throw new InvalidOperationException("ControlsResources.DataConnectionDlg_CannotModifyState");
			}
			bool result = _DataSources.Remove(item);
			if (item == _Dialog.SelectedDataSource)
			{
				_Dialog.SetSelectedDataSource(null, noSingleItemCheck: true);
			}
			return result;
		}

		public void Clear()
		{
			if (_Dialog._ShowingDialog)
			{
				throw new InvalidOperationException("ControlsResources.DataConnectionDlg_CannotModifyState");
			}
			_DataSources.Clear();
			_Dialog.SetSelectedDataSource(null, noSingleItemCheck: true);
		}

		public void CopyTo(TDataSource[] array, int arrayIndex)
		{
			_DataSources.CopyTo(array, arrayIndex);
		}

		public IEnumerator<TDataSource> GetEnumerator()
		{
			return _DataSources.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _DataSources.GetEnumerator();
		}
	}

	private class TiPropertyGridUIControl : UserControl, IDataConnectionUIControl
	{
		private IDataConnectionProperties connectionProperties;

		private readonly BlackbirdSql.VisualStudio.Ddex.Controls.DataTools.TDataConnectionAdvancedDlg.TiSpecializedPropertyGrid propertyGrid;

		public TiPropertyGridUIControl()
		{
			propertyGrid = new TDataConnectionAdvancedDlg.TiSpecializedPropertyGrid();
			SuspendLayout();
			propertyGrid.CommandsVisibleIfAvailable = true;
			propertyGrid.Dock = DockStyle.Fill;
			propertyGrid.Location = Point.Empty;
			propertyGrid.Margin = new Padding(0);
			propertyGrid.Name = "propertyGrid";
			propertyGrid.TabIndex = 0;
			Controls.Add(propertyGrid);
			Name = "TiPropertyGridUIControl";
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		public void Initialize(IDataConnectionProperties dataConnectionProperties)
		{
			connectionProperties = dataConnectionProperties;
		}

		public void LoadProperties()
		{
			propertyGrid.SelectedObject = connectionProperties;
		}

		public override Size GetPreferredSize(Size proposedSize)
		{
			return propertyGrid.GetPreferredSize(proposedSize);
		}
	}

	private class TiBasicConnectionProperties : IDataConnectionProperties
	{
		private string _s;

		[Browsable(false)]
		public bool IsExtensible => false;

		public object this[string propertyName]
		{
			get
			{
				if (propertyName == "ConnectionString")
				{
					return ConnectionString;
				}
				return null;
			}
			set
			{
				if (propertyName == "ConnectionString")
				{
					ConnectionString = value as string;
				}
			}
		}

		[Browsable(false)]
		public bool IsComplete => true;

		public string ConnectionString
		{
			get
			{
				return ToFullString();
			}
			set
			{
				Parse(value);
			}
		}

		public event EventHandler PropertyChanged;

		public void Reset()
		{
			_s = string.Empty;
		}

		public void Parse(string s)
		{
			_s = s;
			PropertyChanged?.Invoke(this, EventArgs.Empty);
		}

		public void Add(string propertyName)
		{
			throw new NotImplementedException();
		}

		public bool Contains(string propertyName)
		{
			return propertyName == "ConnectionString";
		}

		public void Remove(string propertyName)
		{
			throw new NotImplementedException();
		}

		public void Reset(string propertyName)
		{
			_s = string.Empty;
		}

		public void Test()
		{
		}

		public string ToFullString()
		{
			return _s;
		}

		public string ToDisplayString()
		{
			return _s;
		}
	}

	private Size _InitialContainerControlSize;

	private bool _ShowingDialog;

	private Label _HeaderLabel;

	private bool _TranslateHelpButton = true;

	private string _ChooseDataSourceTitle;

	private string _ChooseDataSourceHeaderLabel = string.Empty;

	private string _ChooseDataSourceAcceptText;

	private string _ChangeDataSourceTitle;

	private string _ChangeDataSourceHeaderLabel = string.Empty;

	private readonly ICollection<TDataSource> _DataSources;

	private TDataSource _UnspecifiedDataSource = null;

	private TDataSource _SelectedDataSource;

	private readonly IDictionary<TDataSource, TDataProvider> _DataProviderSelections = new Dictionary<TDataSource, TDataProvider>();

	private bool _SaveSelection = true;

	private readonly IDictionary<TDataSource, IDictionary<TDataProvider, IDataConnectionUIControl>> _ConnectionUIControlTable = new Dictionary<TDataSource, IDictionary<TDataProvider, IDataConnectionUIControl>>();

	private readonly IDictionary<TDataSource, IDictionary<TDataProvider, IDataConnectionProperties>> _ConnectionPropertiesTable = new Dictionary<TDataSource, IDictionary<TDataProvider, IDataConnectionProperties>>();

	private EventHandler _UpdateServerExplorerChangedEvent;

	public event EventHandler UpdateServerExplorerChangedEvent
	{
		add { _UpdateServerExplorerChangedEvent += value; }
		remove { _UpdateServerExplorerChangedEvent -= value; }
	}


	public string Title
	{
		get
		{
			return Text;
		}
		set
		{
			Text = value;
		}
	}

	public string HeaderLabel
	{
		get
		{
			if (_HeaderLabel == null)
			{
				return string.Empty;
			}
			return _HeaderLabel.Text;
		}
		set
		{
			if (_ShowingDialog)
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_CannotModifyState);
			}
			if ((_HeaderLabel == null && (value == null || value.Length == 0)) || (_HeaderLabel != null && value == _HeaderLabel.Text))
			{
				return;
			}
			if (value != null && value.Length > 0)
			{
				if (_HeaderLabel == null)
				{
					_HeaderLabel = new Label
					{
						Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
						FlatStyle = FlatStyle.System,
						Location = new Point(12, 12),
						Margin = new Padding(3),
						Name = "dataSourceLabel",
						Width = dataSourceTableLayoutPanel.Width,
						TabIndex = 100
					};
					Controls.Add(_HeaderLabel);
				}
				_HeaderLabel.Text = value;
				MinimumSize = Size.Empty;
				_HeaderLabel.Height = GetPreferredLabelHeight(_HeaderLabel);
				int num = _HeaderLabel.Bottom + _HeaderLabel.Margin.Bottom + dataSourceLabel.Margin.Top - dataSourceLabel.Top;
				containerControl.Anchor &= ~AnchorStyles.Bottom;
				Height += num;
				containerControl.Anchor |= AnchorStyles.Bottom;
				containerControl.Top += num;
				dataSourceTableLayoutPanel.Top += num;
				dataSourceLabel.Top += num;
				MinimumSize = Size;
			}
			else if (_HeaderLabel != null)
			{
				int num2 = _HeaderLabel.Height;
				try
				{
					Controls.Remove(_HeaderLabel);
				}
				finally
				{
					_HeaderLabel.Dispose();
					_HeaderLabel = null;
				}
				MinimumSize = Size.Empty;
				dataSourceLabel.Top -= num2;
				dataSourceTableLayoutPanel.Top -= num2;
				containerControl.Top -= num2;
				containerControl.Anchor &= ~AnchorStyles.Bottom;
				Height -= num2;
				containerControl.Anchor |= AnchorStyles.Bottom;
				MinimumSize = Size;
			}
		}
	}

	public bool TranslateHelpButton
	{
		get
		{
			return _TranslateHelpButton;
		}
		set
		{
			_TranslateHelpButton = value;
		}
	}

	public string ChooseDataSourceTitle
	{
		get
		{
			return _ChooseDataSourceTitle;
		}
		set
		{
			if (_ShowingDialog)
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_CannotModifyState);
			}
			value ??= string.Empty;
			if (!(value == _ChooseDataSourceTitle))
			{
				_ChooseDataSourceTitle = value;
			}
		}
	}

	public string ChooseDataSourceHeaderLabel
	{
		get
		{
			return _ChooseDataSourceHeaderLabel;
		}
		set
		{
			if (_ShowingDialog)
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_CannotModifyState);
			}
			value ??= string.Empty;
			if (!(value == _ChooseDataSourceHeaderLabel))
			{
				_ChooseDataSourceHeaderLabel = value;
			}
		}
	}

	public string ChooseDataSourceAcceptText
	{
		get
		{
			return _ChooseDataSourceAcceptText;
		}
		set
		{
			if (_ShowingDialog)
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_CannotModifyState);
			}
			value ??= string.Empty;
			if (!(value == _ChooseDataSourceAcceptText))
			{
				_ChooseDataSourceAcceptText = value;
			}
		}
	}

	public string ChangeDataSourceTitle
	{
		get
		{
			return _ChangeDataSourceTitle;
		}
		set
		{
			if (_ShowingDialog)
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_CannotModifyState);
			}
			value ??= string.Empty;
			if (!(value == _ChangeDataSourceTitle))
			{
				_ChangeDataSourceTitle = value;
			}
		}
	}

	public string ChangeDataSourceHeaderLabel
	{
		get
		{
			return _ChangeDataSourceHeaderLabel;
		}
		set
		{
			if (_ShowingDialog)
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_CannotModifyState);
			}
			value ??= string.Empty;
			if (!(value == _ChangeDataSourceHeaderLabel))
			{
				_ChangeDataSourceHeaderLabel = value;
			}
		}
	}

	public ICollection<TDataSource> DataSources => _DataSources;

	public TDataSource UnspecifiedDataSource => _UnspecifiedDataSource ??= TDataSource.CreateUnspecified();

	public TDataSource SelectedDataSource
	{
		get
		{
			if (_DataSources == null)
			{
				return null;
			}
			switch (_DataSources.Count)
			{
			case 0:
				return null;
			case 1:
			{
				IEnumerator<TDataSource> enumerator = _DataSources.GetEnumerator();
				enumerator.MoveNext();
				return enumerator.Current;
			}
			default:
				return _SelectedDataSource;
			}
		}
		set
		{
			if (SelectedDataSource != value)
			{
				if (_ShowingDialog)
				{
					throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_CannotModifyState);
				}
				SetSelectedDataSource(value, noSingleItemCheck: false);
			}
		}
	}

	public TDataProvider SelectedDataProvider
	{
		get
		{
			return GetSelectedDataProvider(SelectedDataSource);
		}
		set
		{
			if (SelectedDataProvider != value)
			{
				if (SelectedDataSource == null)
				{
					throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_NoDataSourceSelected);
				}
				SetSelectedDataProvider(SelectedDataSource, value);
			}
		}
	}

	public bool SaveSelection
	{
		get
		{
			return _SaveSelection;
		}
		set
		{
			_SaveSelection = value;
		}
	}

	public string DisplayConnectionString
	{
		get
		{
			string text = null;
			if (ConnectionProperties != null)
			{
				try
				{
					text = ConnectionProperties.ToDisplayString();
				}
				catch
				{
				}
			}
			if (text == null)
			{
				return string.Empty;
			}
			return text;
		}
	}

	public string ConnectionString
	{
		get
		{
			string text = null;
			if (ConnectionProperties != null)
			{
				try
				{
					text = ConnectionProperties.ToString();
				}
				catch
				{
				}
			}
			if (text == null)
			{
				return string.Empty;
			}
			return text;
		}
		set
		{
			if (_ShowingDialog)
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_CannotModifyState);
			}
			if (SelectedDataProvider == null)
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_NoDataProviderSelected);
			}
			ConnectionProperties?.Parse(value);
		}
	}

	public string AcceptButtonText
	{
		get
		{
			return acceptButton.Text;
		}
		set
		{
			acceptButton.Text = value;
		}
	}

	internal UserControl ConnectionUIControl
	{
		get
		{
			if (SelectedDataProvider == null)
			{
				return null;
			}
			if (!_ConnectionUIControlTable.ContainsKey(SelectedDataSource))
			{
				_ConnectionUIControlTable[SelectedDataSource] = new Dictionary<TDataProvider, IDataConnectionUIControl>();
			}
			if (!_ConnectionUIControlTable[SelectedDataSource].ContainsKey(SelectedDataProvider))
			{
				IDataConnectionUIControl dataConnectionUIControl = null;
				UserControl userControl = null;
				try
				{
					dataConnectionUIControl = ((SelectedDataSource != UnspecifiedDataSource)
						? SelectedDataProvider.CreateConnectionUIControl(SelectedDataSource)
						: SelectedDataProvider.CreateConnectionUIControl());

					userControl = dataConnectionUIControl as UserControl;

					if (userControl == null && dataConnectionUIControl is IContainerControl containerControl)
						userControl = containerControl.ActiveControl as UserControl;
				}
				catch
				{
				}
				if (dataConnectionUIControl == null || userControl == null)
				{
					dataConnectionUIControl = new TiPropertyGridUIControl();
					userControl = dataConnectionUIControl as UserControl;
				}
				userControl.Location = Point.Empty;
				userControl.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				userControl.AutoSize = false;
				try
				{
					dataConnectionUIControl.Initialize(ConnectionProperties);
				}
				catch (ArgumentException ex)
				{
					ShowError(null, ex);
					return null;
				}
				catch
				{
				}
				_ConnectionUIControlTable[SelectedDataSource][SelectedDataProvider] = dataConnectionUIControl;
				components.Add(userControl);
			}
			UserControl userControl2 = _ConnectionUIControlTable[SelectedDataSource][SelectedDataProvider] as UserControl;
			userControl2 ??= (_ConnectionUIControlTable[SelectedDataSource][SelectedDataProvider] as IContainerControl).ActiveControl as UserControl;
			return userControl2;
		}
	}

	internal IDataConnectionProperties ConnectionProperties
	{
		get
		{
			if (SelectedDataProvider == null)
			{
				return null;
			}
			if (!_ConnectionPropertiesTable.ContainsKey(SelectedDataSource))
			{
				_ConnectionPropertiesTable[SelectedDataSource] = new Dictionary<TDataProvider, IDataConnectionProperties>();
			}
			if (!_ConnectionPropertiesTable[SelectedDataSource].ContainsKey(SelectedDataProvider))
			{
				IDataConnectionProperties dataConnectionProperties = null;
				try
				{
					dataConnectionProperties = ((SelectedDataSource != UnspecifiedDataSource) ? SelectedDataProvider.CreateConnectionProperties(SelectedDataSource) : SelectedDataProvider.CreateConnectionProperties());
				}
				catch
				{
				}
				dataConnectionProperties ??= new TiBasicConnectionProperties();
				try
				{
					dataConnectionProperties.PropertyChanged += ConfigureAcceptButton;
				}
				catch
				{
				}
				_ConnectionPropertiesTable[SelectedDataSource][SelectedDataProvider] = dataConnectionProperties;
			}
			return _ConnectionPropertiesTable[SelectedDataSource][SelectedDataProvider];
		}
	}


	public bool UpdateServerExplorer => chkUpdateServerExplorer.Checked; 

	public event EventHandler VerifySettings;

	public event EventHandler<ContextHelpEventArgs> ContextHelpRequested;

	public event ThreadExceptionEventHandler DialogException;

	public event LinkClickedEventHandler LinkClicked;

	public TDataConnectionDlg()
	{
		InitializeComponent();
		dataSourceTextBox.Width = 0;
		components.Add(new UserPreferenceChangedHandler(this));
		_ChooseDataSourceTitle = ControlsResources.TDataConnectionDlg_ChooseDataSourceTitle;
		_ChooseDataSourceAcceptText = ControlsResources.TDataConnectionDlg_ChooseDataSourceAcceptText;
		_ChangeDataSourceTitle = ControlsResources.TDataConnectionDlg_ChangeDataSourceTitle;
		_DataSources = new TiDataSourceCollection(this);
	}

	public static DialogResult Show(TDataConnectionDlg dialog)
	{
		return Show(dialog, null);
	}

	public static DialogResult Show(TDataConnectionDlg dialog, IWin32Window owner)
	{
		if (dialog == null)
		{
			throw new ArgumentNullException("dialog");
		}
		if (dialog.DataSources.Count == 0)
		{
			throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_NoDataSourcesAvailable);
		}
		foreach (TDataSource dataSource in dialog.DataSources)
		{
			if (dataSource.Providers.Count == 0)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ControlsResources.TDataConnectionDlg_NoDataProvidersForDataSource, dataSource.DisplayName.Replace("'", "''")));
			}
		}

		Application.ThreadException += dialog.HandleDialogException;

		dialog._ShowingDialog = true;

		try
		{
			if (dialog.SelectedDataSource == null || dialog.SelectedDataProvider == null)
			{
				dialog.SelectedDataSource = TDataSource.FbDataSource;
				dialog.SelectedDataProvider = TDataProvider.BlackbirdSqlDataProvider;

				/*
				using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
				{
					using DataConnectionSourceDialog dataConnectionSourceDialog = new DataConnectionSourceDialog(dialog);
					dataConnectionSourceDialog.Title = dialog.ChooseDataSourceTitle;
					dataConnectionSourceDialog.HeaderLabel = dialog.ChooseDataSourceHeaderLabel;
					(dataConnectionSourceDialog.AcceptButton as Button).Text = dialog.ChooseDataSourceAcceptText;
					if (dialog.Container != null)
					{
						dialog.Container.Add(dataConnectionSourceDialog);
					}
					try
					{
						if (owner == null)
						{
							dataConnectionSourceDialog.StartPosition = FormStartPosition.CenterScreen;
						}
						dataConnectionSourceDialog.ShowDialog(owner);
						if (dialog.SelectedDataSource == null || dialog.SelectedDataProvider == null)
						{
							return DialogResult.Cancel;
						}
					}
					finally
					{
						if (dialog.Container != null)
						{
							dialog.Container.Remove(dataConnectionSourceDialog);
						}
					}
				}
				*/
			}
			else
			{
				dialog._SaveSelection = false;
			}
			if (owner == null)
			{
				dialog.StartPosition = FormStartPosition.CenterScreen;
			}

			DialogResult dialogResult;

			while (true)
			{
				dialogResult = DialogResult.None;
				using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
				{
					dialogResult = dialog.ShowDialog(owner);
				}
				if (dialogResult != DialogResult.Ignore)
				{
					break;
				}
				using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
				{
					/*
					using DataConnectionSourceDialog dataConnectionSourceDialog2 = new DataConnectionSourceDialog(dialog);
					dataConnectionSourceDialog2.Title = dialog.ChangeDataSourceTitle;
					dataConnectionSourceDialog2.HeaderLabel = dialog.ChangeDataSourceHeaderLabel;
					if (dialog.Container != null)
					{
						dialog.Container.Add(dataConnectionSourceDialog2);
					}
					try
					{
						if (owner == null)
						{
							dataConnectionSourceDialog2.StartPosition = FormStartPosition.CenterScreen;
						}
						dataConnectionSourceDialog2.ShowDialog(owner);
					}
					finally
					{
						if (dialog.Container != null)
						{
							dialog.Container.Remove(dataConnectionSourceDialog2);
						}
					}
					*/
				}
			}
			return dialogResult;
		}
		finally
		{
			dialog._ShowingDialog = false;
			Application.ThreadException -= dialog.HandleDialogException;
		}
	}


	private int GetPreferredHeight(Control c, bool useCompatibleTextRendering, int requiredWidth)
	{
		using Graphics graphics = Graphics.FromHwnd(c.Handle);
		if (useCompatibleTextRendering)
		{
			return graphics.MeasureString(c.Text, c.Font, c.Width).ToSize().Height;
		}
		return TextRenderer.MeasureText(graphics, c.Text, c.Font, new Size(requiredWidth, int.MaxValue), TextFormatFlags.WordBreak).Height;
	}

	public int GetPreferredLabelHeight(Label label)
	{
		return GetPreferredLabelHeight(label, label.Width);
	}

	public int GetPreferredLabelHeight(Label label, int requiredWidth)
	{
		return GetPreferredHeight(label, label.UseCompatibleTextRendering, requiredWidth);
	}

	public TDataProvider GetSelectedDataProvider(TDataSource dataSource)
	{
		if (dataSource == null)
		{
			return null;
		}
		switch (dataSource.Providers.Count)
		{
		case 0:
			return null;
		case 1:
		{
			IEnumerator<TDataProvider> enumerator = dataSource.Providers.GetEnumerator();
			enumerator.MoveNext();
			return enumerator.Current;
		}
		default:
			if (!_DataProviderSelections.ContainsKey(dataSource))
			{
				return null;
			}
			return _DataProviderSelections[dataSource];
		}
	}

	public void SetSelectedDataProvider(TDataSource dataSource, TDataProvider dataProvider)
	{
		if (GetSelectedDataProvider(dataSource) != dataProvider)
		{
			if (dataSource == null)
			{
				throw new ArgumentNullException("dataSource");
			}
			if (_ShowingDialog)
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_CannotModifyState);
			}
			SetSelectedDataProvider(dataSource, dataProvider, noSingleItemCheck: false);
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		if (!_ShowingDialog)
		{
			throw new NotSupportedException(ControlsResources.TDataConnectionDlg_ShowDialogNotSupported);
		}
		ConfigureDataSourceTextBox();
		ConfigureChangeDataSourceButton();
		ConfigureContainerControl();
		ConfigureAcceptButton(this, EventArgs.Empty);
		base.OnLoad(e);
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		dataSourceTextBox.Focus();
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		dataSourceTableLayoutPanel.Anchor &= ~AnchorStyles.Right;
		containerControl.Anchor &= ~(AnchorStyles.Bottom | AnchorStyles.Right);
		advancedButton.Anchor |= AnchorStyles.Top | AnchorStyles.Left;
		advancedButton.Anchor &= ~(AnchorStyles.Bottom | AnchorStyles.Right);
		separatorPanel.Anchor |= AnchorStyles.Top;
		separatorPanel.Anchor &= ~(AnchorStyles.Bottom | AnchorStyles.Right);
		testConnectionButton.Anchor |= AnchorStyles.Top;
		testConnectionButton.Anchor &= ~AnchorStyles.Bottom;
		buttonsTableLayoutPanel.Anchor |= AnchorStyles.Top | AnchorStyles.Left;
		buttonsTableLayoutPanel.Anchor &= ~(AnchorStyles.Bottom | AnchorStyles.Right);
		Size clientSize = new Size(containerControl.Right + containerControl.Margin.Right + Padding.Right, buttonsTableLayoutPanel.Bottom + buttonsTableLayoutPanel.Margin.Bottom + Padding.Bottom);
		clientSize = SizeFromClientSize(clientSize);
		Size size = Size - clientSize;
		MinimumSize -= size;
		Size -= size;
		buttonsTableLayoutPanel.Anchor |= AnchorStyles.Bottom | AnchorStyles.Right;
		buttonsTableLayoutPanel.Anchor &= ~(AnchorStyles.Top | AnchorStyles.Left);
		testConnectionButton.Anchor |= AnchorStyles.Bottom;
		testConnectionButton.Anchor &= ~AnchorStyles.Top;
		separatorPanel.Anchor |= AnchorStyles.Bottom | AnchorStyles.Right;
		separatorPanel.Anchor &= ~AnchorStyles.Top;
		advancedButton.Anchor |= AnchorStyles.Bottom | AnchorStyles.Right;
		advancedButton.Anchor &= ~(AnchorStyles.Top | AnchorStyles.Left);
		containerControl.Anchor |= AnchorStyles.Bottom | AnchorStyles.Right;
		dataSourceTableLayoutPanel.Anchor |= AnchorStyles.Right;
	}

	protected virtual void OnVerifySettings(EventArgs e)
	{
		VerifySettings?.Invoke(this, e);
	}

	protected internal virtual void OnContextHelpRequested(ContextHelpEventArgs e)
	{
		ContextHelpRequested?.Invoke(this, e);
		if (!e.Handled)
		{
			ShowError(null, ControlsResources.TDataConnectionDlg_NoHelpAvailable);
			e.Handled = true;
		}
	}

	internal void OnLinkClicked(object sender, LinkClickedEventArgs e)
	{
		LinkClicked?.Invoke(sender, e);
	}

	protected override void OnHelpRequested(HelpEventArgs hevent)
	{
		/*
		Control control = this;
		ContainerControl containerControl = null;
		while (control is ContainerControl containerControl2 && containerControl2 != ConnectionUIControl && containerControl2.ActiveControl != null)
		{
			control = containerControl2.ActiveControl;
		}
		EnDataConnectionDlgContext context = EnDataConnectionDlgContext.Main;
		if (control == dataSourceTextBox)
		{
			context = EnDataConnectionDlgContext.MainDataSourceTextBox;
		}
		if (control == changeDataSourceButton)
		{
			context = EnDataConnectionDlgContext.MainChangeDataSourceButton;
		}
		if (control == ConnectionUIControl)
		{
			context = EnDataConnectionDlgContext.MainConnectionUIControl;
			if (ConnectionUIControl is SqlConnectionUIControl)
			{
				context = EnDataConnectionDlgContext.MainSqlConnectionUIControl;
			}
			if (ConnectionUIControl is SqlFileConnectionUIControl)
			{
				context = EnDataConnectionDlgContext.MainSqlFileConnectionUIControl;
			}
			if (ConnectionUIControl is OracleConnectionUIControl)
			{
				context = EnDataConnectionDlgContext.MainOracleConnectionUIControl;
			}
			if (ConnectionUIControl is AccessConnectionUIControl)
			{
				context = EnDataConnectionDlgContext.MainAccessConnectionUIControl;
			}
			if (ConnectionUIControl is OleDbConnectionUIControl)
			{
				context = EnDataConnectionDlgContext.MainOleDbConnectionUIControl;
			}
			if (ConnectionUIControl is OdbcConnectionUIControl)
			{
				context = EnDataConnectionDlgContext.MainOdbcConnectionUIControl;
			}
			if (ConnectionUIControl is TiPropertyGridUIControl)
			{
				context = EnDataConnectionDlgContext.MainGenericConnectionUIControl;
			}
		}
		if (control == advancedButton)
		{
			context = EnDataConnectionDlgContext.MainAdvancedButton;
		}
		if (control == testConnectionButton)
		{
			context = EnDataConnectionDlgContext.MainTestConnectionButton;
		}
		if (control == acceptButton)
		{
			context = EnDataConnectionDlgContext.MainAcceptButton;
		}
		if (control == cancelButton)
		{
			context = EnDataConnectionDlgContext.MainCancelButton;
		}
		ContextHelpEventArgs contextHelpEventArgs = new ContextHelpEventArgs(context, hevent.MousePos);
		OnContextHelpRequested(contextHelpEventArgs);
		hevent.Handled = contextHelpEventArgs.Handled;
		if (!contextHelpEventArgs.Handled)
		{
			base.OnHelpRequested(hevent);
		}
		*/
	}

	protected virtual void OnDialogException(ThreadExceptionEventArgs e)
	{
		if (DialogException != null)
		{
			DialogException(this, e);
		}
		else
		{
			ShowError(null, e.Exception);
		}
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (DialogResult == DialogResult.OK)
		{
			try
			{
				OnVerifySettings(EventArgs.Empty);
			}
			catch (Exception ex)
			{
				if (ex is not ExternalException ex2 || ex2.ErrorCode != -2147217842)
				{
					/*
					string dataSource = ConnectionProperties["Data Source"] as string;
					Exception ex3 = SqlFileVersionCheckHelper.CheckIfConnectToNotInstalledSQLExpress(dataSource);
					if (ex3 != null)
					{
						ex = ex3;
					}
					*/
					ShowError(null, ex);
				}
				e.Cancel = true;
			}
		}
		base.OnFormClosing(e);
	}

	protected override void WndProc(ref Message m)
	{
		/*
		if (_TranslateHelpButton && HelpUtils.IsContextHelpMessage(ref m))
		{
			HelpUtils.TranslateContextHelpMessage(this, ref m);
			DefWndProc(ref m);
		}
		else
		{
		*/
			base.WndProc(ref m);
		/*
		}
		*/
	}

	internal void SetSelectedDataSourceInternal(TDataSource value)
	{
		SetSelectedDataSource(value, noSingleItemCheck: false);
	}

	internal void SetSelectedDataProviderInternal(TDataSource dataSource, TDataProvider value)
	{
		SetSelectedDataProvider(dataSource, value, noSingleItemCheck: false);
	}

	private void SetSelectedDataSource(TDataSource value, bool noSingleItemCheck)
	{
		if (!noSingleItemCheck && _DataSources.Count == 1 && _SelectedDataSource != value)
		{
			IEnumerator<TDataSource> enumerator = _DataSources.GetEnumerator();
			enumerator.MoveNext();
			if (value != enumerator.Current)
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_CannotChangeSingleDataSource);
			}
		}
		if (_SelectedDataSource == value)
		{
			return;
		}
		if (value != null)
		{
			if (!_DataSources.Contains(value))
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_DataSourceNotFound);
			}
			_SelectedDataSource = value;
			switch (_SelectedDataSource.Providers.Count)
			{
			case 0:
				SetSelectedDataProvider(_SelectedDataSource, null, noSingleItemCheck);
				break;
			case 1:
			{
				IEnumerator<TDataProvider> enumerator2 = _SelectedDataSource.Providers.GetEnumerator();
				enumerator2.MoveNext();
				SetSelectedDataProvider(_SelectedDataSource, enumerator2.Current, noSingleItemCheck: true);
				break;
			}
			default:
			{
				TDataProvider value2 = _SelectedDataSource.DefaultProvider;
				if (_DataProviderSelections.ContainsKey(_SelectedDataSource))
				{
					value2 = _DataProviderSelections[_SelectedDataSource];
				}
				SetSelectedDataProvider(_SelectedDataSource, value2, noSingleItemCheck);
				break;
			}
			}
		}
		else
		{
			_SelectedDataSource = null;
		}
		if (_ShowingDialog)
		{
			ConfigureDataSourceTextBox();
		}
	}

	private void SetSelectedDataProvider(TDataSource dataSource, TDataProvider value, bool noSingleItemCheck)
	{
		if (!noSingleItemCheck && dataSource.Providers.Count == 1 && ((_DataProviderSelections.ContainsKey(dataSource) && _DataProviderSelections[dataSource] != value) || (!_DataProviderSelections.ContainsKey(dataSource) && value != null)))
		{
			IEnumerator<TDataProvider> enumerator = dataSource.Providers.GetEnumerator();
			enumerator.MoveNext();
			if (value != enumerator.Current)
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_CannotChangeSingleDataProvider);
			}
		}
		if ((!_DataProviderSelections.ContainsKey(dataSource) || _DataProviderSelections[dataSource] == value) && (_DataProviderSelections.ContainsKey(dataSource) || value == null))
		{
			return;
		}
		if (value != null)
		{
			if (!dataSource.Providers.Contains(value))
			{
				throw new InvalidOperationException(ControlsResources.TDataConnectionDlg_DataSourceNoAssociation);
			}
			_DataProviderSelections[dataSource] = value;
		}
		else if (_DataProviderSelections.ContainsKey(dataSource))
		{
			_DataProviderSelections.Remove(dataSource);
		}
		if (_ShowingDialog)
		{
			ConfigureContainerControl();
		}
	}

	private void ConfigureDataSourceTextBox()
	{
		if (SelectedDataSource != null)
		{
			if (SelectedDataSource == UnspecifiedDataSource)
			{
				if (SelectedDataProvider != null)
				{
					dataSourceTextBox.Text = SelectedDataProvider.DisplayName;
				}
				else
				{
					dataSourceTextBox.Text = null;
				}
				dataProviderToolTip.SetToolTip(dataSourceTextBox, null);
			}
			else
			{
				dataSourceTextBox.Text = SelectedDataSource.DisplayName;
				if (SelectedDataProvider != null)
				{
					if (SelectedDataProvider.ShortDisplayName != null)
					{
						dataSourceTextBox.Text = string.Format(CultureInfo.CurrentCulture, ControlsResources.TDataConnectionDlg_DataSourceWithShortProvider, dataSourceTextBox.Text, SelectedDataProvider.ShortDisplayName);
					}
					dataProviderToolTip.SetToolTip(dataSourceTextBox, SelectedDataProvider.DisplayName);
				}
				else
				{
					dataProviderToolTip.SetToolTip(dataSourceTextBox, null);
				}
			}
		}
		else
		{
			dataSourceTextBox.Text = null;
			dataProviderToolTip.SetToolTip(dataSourceTextBox, null);
		}
		dataSourceTextBox.Select(0, 0);
	}

	private void ConfigureChangeDataSourceButton()
	{
		changeDataSourceButton.Enabled = DataSources.Count > 1 || SelectedDataSource.Providers.Count > 1;
	}

	private void ConfigureContainerControl()
	{
		acceptButton.ClearHandlers();
		acceptButton.Click += HandleAccept;
		if (containerControl.Controls.Count == 0)
		{
			_InitialContainerControlSize = containerControl.Size;
		}
		if ((containerControl.Controls.Count == 0 && ConnectionUIControl != null) || (containerControl.Controls.Count > 0 && ConnectionUIControl != containerControl.Controls[0]))
		{
			containerControl.Controls.Clear();
			if (ConnectionUIControl != null && ConnectionUIControl.PreferredSize.Width > 0 && ConnectionUIControl.PreferredSize.Height > 0)
			{
				containerControl.Controls.Add(ConnectionUIControl);
				MinimumSize = Size.Empty;
				Size size = containerControl.Size;
				containerControl.Size = _InitialContainerControlSize;
				Size preferredSize = ConnectionUIControl.PreferredSize;
				containerControl.Size = size;
				int val = _InitialContainerControlSize.Width - (Width - ClientSize.Width) - Padding.Left - containerControl.Margin.Left - containerControl.Margin.Right - Padding.Right;
				val = Math.Max(val, testConnectionButton.Width + testConnectionButton.Margin.Right + buttonsTableLayoutPanel.Margin.Left + buttonsTableLayoutPanel.Width + buttonsTableLayoutPanel.Margin.Right);
				preferredSize.Width = Math.Max(preferredSize.Width, val);
				Size += preferredSize - containerControl.Size;
				if (containerControl.Bottom == advancedButton.Top)
				{
					containerControl.Margin = new Padding(containerControl.Margin.Left, dataSourceTableLayoutPanel.Margin.Bottom, containerControl.Margin.Right, advancedButton.Margin.Top);
					Height += containerControl.Margin.Bottom + advancedButton.Margin.Top;
					containerControl.Height -= containerControl.Margin.Bottom + advancedButton.Margin.Top;
				}
				Size size2 = SystemInformation.PrimaryMonitorMaximizedWindowSize - SystemInformation.FrameBorderSize - SystemInformation.FrameBorderSize;
				if (Width > size2.Width)
				{
					Width = size2.Width;
					if (Height + SystemInformation.HorizontalScrollBarHeight <= size2.Height)
					{
						Height += SystemInformation.HorizontalScrollBarHeight;
					}
				}
				if (Height > size2.Height)
				{
					if (Width + SystemInformation.VerticalScrollBarWidth <= size2.Width)
					{
						Width += SystemInformation.VerticalScrollBarWidth;
					}
					Height = size2.Height;
				}
				MinimumSize = Size;
				advancedButton.Enabled = ConnectionUIControl is not TiPropertyGridUIControl;
			}
			else
			{
				MinimumSize = Size.Empty;
				if (containerControl.Bottom != advancedButton.Top)
				{
					containerControl.Height += containerControl.Margin.Bottom + advancedButton.Margin.Top;
					Height -= containerControl.Margin.Bottom + advancedButton.Margin.Top;
					containerControl.Margin = new Padding(containerControl.Margin.Left, 0, containerControl.Margin.Right, 0);
				}
				Size -= containerControl.Size - new Size(300, 0);
				MinimumSize = Size;
				advancedButton.Enabled = true;
			}
		}
		if (ConnectionUIControl != null)
		{
			try
			{
				_ConnectionUIControlTable[SelectedDataSource][SelectedDataProvider].LoadProperties();
			}
			catch
			{
			}
		}
	}

	private void ConfigureAcceptButton(object sender, EventArgs e)
	{
		try
		{
			acceptButton.Enabled = ConnectionProperties != null && ConnectionProperties.IsComplete;
		}
		catch
		{
			acceptButton.Enabled = true;
		}
	}

	private void HandleDialogException(object sender, ThreadExceptionEventArgs e)
	{
		OnDialogException(e);
	}


	internal void ShowError(IUIService uiService, string title, Exception ex)
	{
		if (uiService != null)
		{
			uiService.ShowError(ex);
		}
		else
		{
			Diag.ExceptionService(typeof(IUIService));
			// RTLAwareMessageBox.Show(title, ex.Message, MessageBoxIcon.Exclamation);
		}
	}

	internal void ShowError(string title, Exception ex)
	{
		IUIService uiService = Package.GetGlobalService(typeof(IUIService)) as IUIService;
		if (ex is AggregateException ex2)
		{
			ex2.Flatten().Handle(delegate(Exception e)
			{
				ShowError(uiService, title, e);
				return true;
			});
		}
		else
		{
			ShowError(uiService, title, ex);
		}
	}

	private void ShowError(string title, string message)
	{
		if (Package.GetGlobalService(typeof(IUIService)) is IUIService iUIService)
		{
			iUIService.ShowError(message);
		}
		else
		{
			Diag.ExceptionService(typeof(IUIService));
			// RTLAwareMessageBox.Show(title, message, MessageBoxIcon.Exclamation);
		}
	}


	private void ChangeDataSource(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Ignore;
		Close();
	}

	private void HandleAccept(object sender, EventArgs e)
	{
		acceptButton.Focus();
	}

	private void PaintSeparator(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		Pen pen = new Pen(ControlPaint.Dark(BackColor, 0f));
		Pen pen2 = new Pen(ControlPaint.Light(BackColor, 1f));
		int x = separatorPanel.Width;
		graphics.DrawLine(pen, 0, 0, x, 0);
		graphics.DrawLine(pen2, 0, 1, x, 1);
	}

	private void ShowAdvanced(object sender, EventArgs e)
	{
		TDataConnectionAdvancedDlg dataConnectionAdvancedDlg = new (ConnectionProperties, this);
		DialogResult dialogResult = DialogResult.None;
		try
		{
			Container?.Add(dataConnectionAdvancedDlg);
			dialogResult = dataConnectionAdvancedDlg.ShowDialog(this);
		}
		finally
		{
			Container?.Remove(dataConnectionAdvancedDlg);
			dataConnectionAdvancedDlg.Dispose();
		}
		if (dialogResult == DialogResult.OK && ConnectionUIControl != null)
		{
			try
			{
				_ConnectionUIControlTable[SelectedDataSource][SelectedDataProvider].LoadProperties();
			}
			catch
			{
			}
			ConfigureAcceptButton(this, EventArgs.Empty);
		}
	}

	private void SetConnectionUIControlDockStyle(object sender, EventArgs e)
	{
		if (containerControl.Controls.Count > 0)
		{
			DockStyle dock = DockStyle.None;
			Size size = containerControl.Size;
			Size minimumSize = containerControl.Controls[0].MinimumSize;
			if (size.Width >= minimumSize.Width && size.Height >= minimumSize.Height)
			{
				dock = DockStyle.Fill;
			}
			if (size.Width - SystemInformation.VerticalScrollBarWidth >= minimumSize.Width && size.Height < minimumSize.Height)
			{
				dock = DockStyle.Top;
			}
			if (size.Width < minimumSize.Width && size.Height - SystemInformation.HorizontalScrollBarHeight >= minimumSize.Height)
			{
				dock = DockStyle.Left;
			}
			containerControl.Controls[0].Dock = dock;
		}
	}

	private void TestConnection(object sender, EventArgs e)
	{
		Cursor current = Cursor.Current;
		Cursor.Current = Cursors.WaitCursor;
		try
		{
			ConnectionProperties.Test();
		}
		/*
		catch (InvalidOperationException ex) when (DataUtils.IsProviderMismatchException(ex))
		{
			Cursor.Current = current;
			ShowError(ControlsResources.TDataConnectionDlg_TestResults, DataUtils.CreateProviderMismatchException());
			return;
		}
		*/
		catch (Exception ex3)
		{
			// string dataSource = ConnectionProperties["Data Source"] as string;
			Cursor.Current = current;
			ShowError(ControlsResources.TDataConnectionDlg_TestResults, ex3);
			return;
		}
		Cursor.Current = current;
		MessageCtl.ShowEx(ControlsResources.TDataConnectionDlg_TestConnectionSucceeded, ControlsResources.TDataConnectionDlg_TestResults);
	}


	private void ChkUpdateServerExplorer_CheckedChanged(object sender, EventArgs e)
	{
		_UpdateServerExplorerChangedEvent?.Invoke(sender, e);
	}
}
