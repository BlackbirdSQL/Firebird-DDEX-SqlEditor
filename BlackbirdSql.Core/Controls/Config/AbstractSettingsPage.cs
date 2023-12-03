//
// Plagiarized from Community.VisualStudio.Toolkit extension
//
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

using BlackbirdSql.Core.Controls.Events;
using BlackbirdSql.Core.Controls.Interfaces;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Config;
using Microsoft.VisualStudio.Shell;

using Control = System.Windows.Forms.Control;
using TextBox = System.Windows.Forms.TextBox;


namespace BlackbirdSql.Core.Controls.Config;

[SuppressMessage("Usage", "VSTHRD104:Offer async methods")]

// =========================================================================================================
//										AbstractSettingsPage Class
//
/// <summary>
/// VS Options DialogPage base class.
/// Disclosure: This class exposes some PropertyGridView members with hidden access modifiers using
/// the Visual Studio's Reflection library, so that we can implement a few standard or comparable windows
/// functionality features like single-click check boxes, radio buttons and cardinal synonyms into the
/// DialogPage property grid.
/// Common cardinal synonyms include current culture min[imum], max[imum], unlimited, default etc.
/// </summary>
// =========================================================================================================
[ComVisible(true)]
public class AbstractSettingsPage<TPage, T> : DialogPage, IBSettingsPage where TPage : AbstractSettingsPage<TPage, T> where T : AbstractSettingsModel<T>
{

	// ---------------------------------------------------------------------------------
	#region Variables - AbstractSettingsPage
	// ---------------------------------------------------------------------------------

	private bool _EventActive = false;
	private bool _Exposed = false;
	private bool _Initialized = false;
	private Control _GridView = null;

	protected static bool _ActivatorActive = false;
	protected static object _LockClass = new object();
	// A private 'this' object lock
	private readonly object _LockLocal = new();
	private AbstractSettingsModel<T> _Model;
	private PropertyGrid _Window = null;


	#endregion Variables




	// =========================================================================================================
	#region Exposing Property Accessors - AbstractSettingsPage
	// =========================================================================================================


	/// <summary>
	/// *** Exposes the private PropertyGrid.gridView.edit field ***.
	/// </summary>
	[Browsable(false)] // For brevity.
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	private TextBox EditField
	{
		get
		{
			Control gridView = GridView;

			if (gridView == null)
				return null;

			Type typeGridView = gridView.GetType();

			FieldInfo editFieldInfo = typeGridView.GetField("edit",
				BindingFlags.NonPublic | BindingFlags.Instance);

			if (editFieldInfo == null)
				return null;

			return editFieldInfo.GetValue(gridView) as TextBox;
		}
	}



	/// <summary>
	/// *** Exposes the private PropertyGrid.gridView field ***.
	/// </summary>
	[Browsable(false)] // For brevity.
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	private Control GridView
	{
		get
		{
			lock (_LockLocal)
			{
				if (_GridView == null)
				{
					Type t = _Window.GetType();
					FieldInfo fInfo = t.GetField("gridView",
						BindingFlags.NonPublic | BindingFlags.Instance);

					_GridView = (Control)fInfo.GetValue(_Window);
				}
				return _GridView;
			}
		}
	}



	/// <summary>
	/// *** Exposes the private PropertyGridView.originalTextValue field ***.
	/// We do this every time because unsure if reflection would manage an exposed
	/// built-in object, ie. not sure our string memory reference is locked in.
	/// </summary>
	[Browsable(false)] // For brevity.
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	private string OriginalTextValue
	{
		get
		{
			lock (_LockLocal)
			{
				Control gridview = GridView;
				if (gridview != null)
				{
					Type t = gridview.GetType();
					FieldInfo fInfo = t.GetField("originalTextValue",
						BindingFlags.NonPublic | BindingFlags.Instance);

					return (string)fInfo.GetValue(gridview);
				}
				return null;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				Control gridview = GridView;
				if (gridview != null)
				{
					Type t = gridview.GetType();
					FieldInfo fInfo = t.GetField("originalTextValue",
						BindingFlags.NonPublic | BindingFlags.Instance);

					fInfo.SetValue(gridview, value);
				}
			}
		}
	}



	/// <summary>
	/// *** Exposes the private PropertyGridView.selectedRow field value ***.
	/// </summary>
	[Browsable(false)] // For brevity.
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	private int SelectedRow
	{
		get
		{
			lock (_LockLocal)
			{
				Control gridView = GridView;

				if (gridView == null)
					return -1;

				Type t = gridView.GetType();
				FieldInfo fInfo = t.GetField("selectedRow",
					BindingFlags.NonPublic | BindingFlags.Instance);

				return (int)fInfo.GetValue(gridView);
			}
		}
	}



	/// <summary>
	/// *** Exposes the private PropertyGrid.SortedByCategories property ***.
	/// </summary>
	[Browsable(false)] // For brevity.
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	private bool SortedByCategories
	{
		get
		{
			lock (_LockLocal)
			{
				Type t = _Window.GetType();
				PropertyInfo pInfo = t.GetProperty("SortedByCategories",
						BindingFlags.NonPublic | BindingFlags.Instance);

				if (pInfo == null)
				{
					COMException ex = new("Could not get SortedByCategories info.");
					Diag.Dug(ex);
					return false;
				}

				return (bool)pInfo.GetValue(_Window);
			}
		}
	}


	#endregion Exposing Property Accessors




	// =========================================================================================================
	#region Property Accessors - AbstractSettingsPage
	// =========================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override object AutomationObject => _Model;


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public PropertyGrid Grid => (PropertyGrid)Window;



	/// <summary>
	/// The settings page PropertyGrid window.
	/// </summary>
	[Browsable(false)] // For brevity.
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	protected override IWin32Window Window
	{
		get
		{
			lock (_LockLocal)
			{
				if (_Window != null)
					return _Window;

				_Window = new()
				{
					Location = new Point(0, 0),
					ToolbarVisible = true,
					CommandsVisibleIfAvailable = true,
					SelectedObject = AutomationObject,
					// CommandsBorderColor = SystemColors.ControlDarkDark,
					CommandsLinkColor = Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC),
					CommandsActiveLinkColor = Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC),
				};

				_Window.SelectedGridItemChanged += OnSelectedItemChanged;
				_Window.GotFocus += OnGotFocus;
				/*
				_Window.Invalidated += delegate { TraceEvent("Invalidated"); };
				_Window.VisibleChanged += delegate { TraceEvent($"VisibleChanged[Visible: {_Window.Visible}]"); };
				_Window.Enter += delegate { TraceEvent("Enter"); };
				_Window.Leave += delegate { TraceEvent("Leave"); };
				_Window.LostFocus += delegate { TraceEvent("LostFocus"); };
				_Window.Validated += delegate { TraceEvent("Validated"); };
				*/
				_Window.PropertyValueChanged += OnPropertyValueChanged;

				return _Window;
			}
		}
	}


	#endregion Property Accessors




	// =========================================================================================================
	#region Constructors / Destructors - AbstractSettingsPage
	// =========================================================================================================


	public AbstractSettingsPage()
	{
		lock (_LockClass)
		{
			if (!_ActivatorActive)
			{
				_Model = ThreadHelper.JoinableTaskFactory.Run(new Func<Task<T>>(AbstractSettingsModel<T>.CreateAsync));
				_Model.Owner = this;
				_Model.SettingsResetEvent += OnResetSettings;
			}
		}
	}

	

	public static TPage CreateInstance(IBLiveSettings liveSettings)
	{
		lock (_LockClass)
			_ActivatorActive = true;

		TPage instance = (TPage) Activator.CreateInstance(typeof(TPage));
		instance._Model = ThreadHelper.JoinableTaskFactory.Run(() => AbstractSettingsModel<T>.CreateAsync(liveSettings));

		instance._Model.Owner = instance;
		instance._Model.SettingsResetEvent += instance.OnResetSettings;
		lock (_LockClass)
			_ActivatorActive = false;

		return instance;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_GridView = null;
			_Window = null;
		}

		base.Dispose(disposing);
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Methods - AbstractSettingsPage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// *** Exposes the private PropertyGridView.edit field's event delegates ***.
	/// No reason why this isn't perfectly legal other than it bypasses access modifiers.
	/// We're doing this because the PropertyGrid control is just too restrictive on access
	/// modifiers and some of the behavior breaks ms windows conventions anyway.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void ExposeEventDelegates()
{
		lock (_LockLocal)
		{
			if (_Exposed)
				return;

			TextBox editCtl = EditField;

			if (editCtl == null)
				return;

			_Exposed = true;

			editCtl.GotFocus += OnGridEditBoxGotFocus;
			editCtl.LostFocus += OnGridEditBoxLostFocus;
			editCtl.MouseDown += OnGridEditBoxMouseDown;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the default value of a property.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override object GetDefaultPropertyValue(PropertyDescriptor property)
	{
		lock (_LockLocal)
		{
			foreach (Attribute customAttribute in property.PropertyType.GetCustomAttributes())
			{
				if (customAttribute is DefaultValueAttribute defaultAttr)
				{
					// Diag.Trace($"Getting default for {property.Name}: {defaultAttr.Value}.");
					return defaultAttr.Value;
				}
			}
			return null;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Invokes methodinfo method synchronously.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected bool InvokeMethod(object obj, string method, BindingFlags bindings, object[] parameters, bool resetFocus = false)
	{
		Type t = obj.GetType();

		MethodInfo methodInfo = t.GetMethod(method, bindings);

		if (methodInfo == null)
		{
			COMException ex = new($"Could not find method info for {method}(). Aborting.");
			Diag.Dug(ex);
			return false;
		}

		methodInfo.Invoke(obj, parameters);

		// Diag.Trace($"InvokeMethod() done {methodInfo.Name}.");

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Invokes methodinfo method asynchronously.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected async Task<bool> InvokeMethodAsync(string method, BindingFlags bindings, object[] parameters, bool resetFocus = false)
	{
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		Control gridView = GridView;
		Type t = gridView.GetType();

		MethodInfo methodInfo = t.GetMethod(method, bindings);

		if (methodInfo == null)
		{
			COMException ex = new($"Could not find method info for {method}(). Aborting.");
			Diag.Dug(ex);
			return false;
		}

		methodInfo.Invoke(gridView, parameters);

		// Diag.Trace($"InvokeMethodAsync() done {methodInfo.Name}.");

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads the settings from live storage. TBC!!!
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void LoadSettings()
	{
		lock (_LockLocal)
			_Model.Load();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads the settings from storage.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void LoadSettingsFromStorage()
	{
		lock (_LockLocal)
			_Model.Load();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Override of the DialogPage ResetSettings method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void ResetSettings()
	{
		lock (_LockLocal)
			_Model.LoadDefaults();

		_Window?.Refresh();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Saves settings to live storage. TBC!!!
	/// </summary>
	// ---------------------------------------------------------------------------------
	public void SaveSettings()
	{
		lock (_LockLocal)
			_Model.Save();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Saves settings to internal storage.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void SaveSettingsToStorage()
	{
		lock (_LockLocal)
			_Model.Save();
	}


	#endregion Methods




	// =========================================================================================================
	#region Event Handling - AbstractSettingsPage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// OnActivate handler. Fixes a glitch in VS initial display - incorrect scroll and
	/// focus.
	/// This method and OnGridEditBoxMouseDown are currently the only methods that invoke
	/// inaccessible methods.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override void OnActivate(CancelEventArgs e)
	{
		base.OnActivate(e);

		if (!_Initialized)
		{
			_Initialized = true;

			// Fix glitch.

			InvokeMethod(GridView, "SetScrollOffset", BindingFlags.Public | BindingFlags.Instance, [0]);

			InvokeMethod(GridView, "SelectRow", BindingFlags.NonPublic | BindingFlags.Instance,
				[SortedByCategories ? 1 : 0]);
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// PropertyGrid GotFocus handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnGotFocus(object sender, EventArgs e)
	{
		ExposeEventDelegates();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Edit textbox GotFocus handler.
	/// We're keeping any access or updating of inaccessible members that are exposed
	/// using Reflection contained within this class so that the rest of the extension
	/// does not break any coding protocols.
	/// This is currently the only method that injects values into inaccessible members.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnGridEditBoxGotFocus(object sender, EventArgs e)
	{
		if (_EventActive)
			return;

		lock (_LockLocal)
		{
			GridItem gridEntry = _Window.SelectedGridItem;

			if (gridEntry == null || gridEntry.PropertyDescriptor == null
				|| string.IsNullOrWhiteSpace(gridEntry.PropertyDescriptor.Name))
			{
				// Diag.Trace($"Aborting OnGridEditBoxLostFocus() Invalid GridEntry.");
				return;
			}

			// Diag.Trace($"Executing OnGridEditBoxGotFocus() typeof sender: {sender.GetType().Name} SelectedGridItem: {_Window.SelectedGridItem.PropertyDescriptor.Name}.");

			SelectedGridItemFocusEventArgs args = new(_Window.SelectedGridItem);
			_Model.OnGridEditBoxGotFocus(sender, args);

			// Injection occurs here.
			// We don't want the user being given the display text of a grid entry textbox to edit.
			// That defeats the purpose of a type converter, so we convert it to an editable form
			// here. The model property returns the editable value and we simply update the textbox
			// value to it's correct value here.
			// Also, we're careful with the PropertyGridView's originalTextValue. We re-perform
			// access to the field on both reads and writes. The textbox is picked up using the
			// 'sender' argument, so that is 100% safe.
			if (args.ValidateValue)
			{
				if (args.Value != OriginalTextValue)
					OriginalTextValue = args.Value;

				TextBox editCtl = ((TextBox)sender);

				if (args.Value != editCtl.Text)
					editCtl.Text = args.Value;
			}
		}

		// Diag.Trace("Done OnEditBoxGotFocus().");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Edit textbox LostFocus handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnGridEditBoxLostFocus(object sender, EventArgs e)
	{
		if (_EventActive)
			return;

		lock (_LockLocal)
		{
			GridItem gridEntry = _Window.SelectedGridItem;

			if (gridEntry == null || gridEntry.PropertyDescriptor == null
				|| string.IsNullOrWhiteSpace(gridEntry.PropertyDescriptor.Name))
			{
				// Diag.Trace($"Aborting OnGridEditBoxLostFocus() Invalid GridEntry or Not AbstractBoolConverter.");
				return;
			}

			// Diag.Trace($"Executing OnGridEditBoxLostFocus() package: {_Model.GetPackage()} group: {_Model.GetGroup()}.");

			SelectedGridItemFocusEventArgs args = new(gridEntry);
			_Model.OnGridEditBoxLostFocus(sender, args);
		}

		// Diag.Trace("Done OnEditBoxLostFocus().");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Overload for PropertyGridView.OnEditMouseDown. Invokes double click for boolean
	/// converters and drop down for enum converters.
	/// We're keeping any access or updating of inaccessible members that are exposed
	/// using Reflection contained within this class so that the rest of the extension
	/// does not break any coding protocols.
	/// This method and OnActivate are currently the only methods that invoke
	/// inaccessible methods.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnGridEditBoxMouseDown(object sender, MouseEventArgs e)
	{
		if (_EventActive || e.Clicks % 2 == 0 || e.Button != MouseButtons.Left)
			return;

		GridItem gridEntry = _Window.SelectedGridItem;

		if (gridEntry == null || gridEntry.PropertyDescriptor == null)
		{
			// Diag.Trace($"Aborting OnGridEditBoxMouseDown() SelectedGridItem or SelectedGridItem.PropertyDescriptor is null.");
			return;
		}

		AbstractEnumConverter enumConverter = gridEntry.PropertyDescriptor.Converter
			as AbstractEnumConverter;

		if (enumConverter == null && gridEntry.PropertyDescriptor.Converter
			is not AbstractBoolConverter)
		{
			// Diag.Trace($"Aborting OnGridEditBoxMouseDown() Not AbstractEnumConverter or AbstractBoolConverter.");
			return;
		}

		if (enumConverter == null)
		{
			_EventActive = true;

			int row = SelectedRow;

			// Diag.Trace($"OnGridEditBoxMouseDown: Invoking DoubleClickRow().");

			InvokeMethod(GridView, "DoubleClickRow", BindingFlags.Public | BindingFlags.Instance,
				[row, false, 2], true);

			_EventActive = false;

			return;
		}

		_EventActive = true;

		// Diag.Trace($"OnGridEditBoxMouseDown: Invoking OnBtnClick().");

		InvokeMethod(GridView, "OnBtnClick", BindingFlags.NonPublic | BindingFlags.Instance,
			[this, new EventArgs()]);

		_EventActive = false;

		// Sample for using async calls.
		// _ = Task.Run(() => InvokeMethodAsync("OnBtnClick",
		// 					BindingFlags.NonPublic | BindingFlags.Instance,
		// 					new object[] { this, new EventArgs() }));

	}


	protected void OnPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
	{
		GridItem gridEntry = e.ChangedItem;

		if (gridEntry == null || gridEntry.PropertyDescriptor == null
			|| gridEntry.PropertyDescriptor.Converter == null)
		{
			// Diag.Trace($"Aborting OnGridEditBoxMouseDown() SelectedGridItem or SelectedGridItem.PropertyDescriptor is null.");
			return;
		}

		GridItemValueChangedEventArgs evt = new(e.ChangedItem, e.OldValue);
		_Model.OnGridItemValueChanged(sender, evt);

		if (evt.ReadOnlyChanged)
			_Window?.Refresh();
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Automation model reset setting event handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnResetSettings(object sender, EventArgs e)
	{
		ResetSettings();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Selected grid item changed handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnSelectedItemChanged(object sender, SelectedGridItemChangedEventArgs e)
	{
		ExposeEventDelegates();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Dummy PropertyGrid event handler for testing events.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnTraceEvent(string evt)
	{
		lock (_LockLocal)
		{
			ExposeEventDelegates();
			GridItem item = _Window.SelectedGridItem;
			string name = item != null && item.PropertyDescriptor != null ? item.PropertyDescriptor.Name : "Null";

			// Diag.Trace($"{evt} item: {name}.");
		}
	}


	#endregion Event Handling


}
