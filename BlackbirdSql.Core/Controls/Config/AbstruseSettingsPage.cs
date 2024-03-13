//
// Plagiarized from Community.VisualStudio.Toolkit extension
//
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BlackbirdSql.Core.Controls.Events;
using BlackbirdSql.Core.Controls.Interfaces;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Properties;

using Microsoft.VisualStudio.Shell;

using Control = System.Windows.Forms.Control;
using TextBox = System.Windows.Forms.TextBox;


namespace BlackbirdSql.Core.Controls.Config;

// =========================================================================================================
//										AbstruseSettingsPage Class
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
public abstract class AbstruseSettingsPage : DialogPage, IBSettingsPage
{

	// ---------------------------------------------------------------------------------
	#region Fields - AbstruseSettingsPage
	// ---------------------------------------------------------------------------------


	// A protected 'this' object lock
	protected readonly object _LockObject = new object();

	private bool _Exposed = false;
	private bool _Initialized = false;
	private int _EventsCardinal = 0;

	protected static object _LockGlobal = new object();
	private PropertyGrid _Window = null;
	private bool _ValidFocusCell = false;
	private bool _ValidMouseEventCell = false;



	#endregion Fields




	// =========================================================================================================
	#region Exposing Property Accessors - AbstruseSettingsPage
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
			lock (_LockObject)
			{
				Control gridView = GridView;

				if (gridView == null)
					return null;

				return Reflect.GetField(gridView, "edit", BindingFlags.NonPublic | BindingFlags.Instance)
					as TextBox;
			}
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
			lock (_LockObject)
			{
				if (_Window == null)
					return null;

				return (Control)Reflect.GetFieldValue(_Window, "gridView",
					BindingFlags.NonPublic | BindingFlags.Instance);
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
			lock (_LockObject)
			{
				Control gridview = GridView;
				if (gridview != null)
				{
					return (string)Reflect.GetFieldValue(gridview, "originalTextValue",
						BindingFlags.NonPublic | BindingFlags.Instance);
				}
				return null;
			}
		}
		set
		{
			lock (_LockObject)
			{
				Control gridview = GridView;
				if (gridview != null)
				{
					Reflect.SetFieldValue(gridview, "originalTextValue",
						BindingFlags.NonPublic | BindingFlags.Instance, value);
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
			lock (_LockObject)
			{
				Control gridView = GridView;

				if (gridView == null)
					return -1;

				return (int)Reflect.GetFieldValue(gridView, "selectedRow",
					BindingFlags.NonPublic | BindingFlags.Instance);
			}
		}
	}


	#endregion Exposing Property Accessors




	// =========================================================================================================
	#region Property Accessors - AbstruseSettingsPage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if when execution has entered an event handler that may cause recursion
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool EventsDisabled => _EventsCardinal > 0;


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public PropertyGrid Grid => _Window ?? (PropertyGrid)Window;


	[Browsable(false)] // For brevity.
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	private bool SortedByCategories => _Window != null && (_Window.PropertySort & PropertySort.Categorized) != 0;



	/// <summary>
	/// The settings page PropertyGrid window.
	/// </summary>
	[Browsable(false)] // For brevity.
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	protected override IWin32Window Window
	{
		get
		{
			lock (_LockObject)
			{
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
				_Window.VisibleChanged += (sender, args) => OnTraceEvent(sender, args, $"VisibleChanged[Visible: {_Window.Visible}]");
				_Window.Invalidated += (sender, args) => OnTraceEvent(sender, args, "Invalidated");
				_Window.Enter += (sender, args) => OnTraceEvent(sender, args, "Enter");
				_Window.Leave += (sender, args) => OnTraceEvent(sender, args, "Leave");
				_Window.LostFocus += (sender, args) => OnTraceEvent(sender, args, "LostFocus");
				_Window.Validated += (sender, args) => OnTraceEvent(sender, args, "Validated");
				_Window.MouseDown += (sender, args) => OnTraceEvent(sender, args, "MouseDown");
				_Window.Click += (sender, args) => OnTraceEvent(sender, args, "Clicked");
				_Window.MouseClick += (sender, args) => OnTraceEvent(sender, args, "MouseClick");
				_Window.PropertyValueChanged += OnPropertyValueChanged;

				return _Window;
			}
		}
	}


	public event IBSettingsPage.EditControlFocusEventHandler EditControlGotFocusEvent;
	public event IBSettingsPage.EditControlFocusEventHandler EditControlLostFocusEvent;
	public event IBSettingsPage.AutomationPropertyValueChangedEventHandler AutomationPropertyValueChangedEvent;

	#endregion Property Accessors




	// =========================================================================================================
	#region Constructors / Destructors - AbstruseSettingsPage
	// =========================================================================================================


	public AbstruseSettingsPage()
	{
		// Tracer.Trace(GetType(), ".ctor");
	}




	protected override void Dispose(bool disposing)
	{
		// Tracer.Trace(GetType(), "Dispose()");
		if (disposing)
		{
			if (_Window != null)
			{
				_Window.SelectedGridItemChanged -= OnSelectedItemChanged;
				_Window.GotFocus -= OnGotFocus;
				_Window.VisibleChanged -= (sender, args) => OnTraceEvent(sender, args, $"VisibleChanged[Visible: {_Window.Visible}]");
				_Window.Invalidated -= (sender, args) => OnTraceEvent(sender, args, "Invalidated");
				_Window.Enter -= (sender, args) => OnTraceEvent(sender, args, "Enter");
				_Window.Leave -= (sender, args) => OnTraceEvent(sender, args, "Leave");
				_Window.LostFocus -= (sender, args) => OnTraceEvent(sender, args, "LostFocus");
				_Window.Validated -= (sender, args) => OnTraceEvent(sender, args, "Validated");
				_Window.MouseDown -= (sender, args) => OnTraceEvent(sender, args, "MouseDown");
				_Window.Click -= (sender, args) => OnTraceEvent(sender, args, "Clicked");
				_Window.MouseClick -= (sender, args) => OnTraceEvent(sender, args, "MouseClick");
				_Window.PropertyValueChanged -= OnPropertyValueChanged;
				_Window = null;
			}
		}

		base.Dispose(disposing);
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Methods - AbstruseSettingsPage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Increments the <see cref="EventsDisabled"/> counter when execution enters an event handler
	/// to prevent recursion
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void DisableEvents()
	{
		_EventsCardinal++;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Decrements the <see cref="EventsDisabled"/> counter that was previously incremented by
	/// <see cref="DisableEvents"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void EnableEvents()
	{
		if (_EventsCardinal == 0)
			Diag.Dug(new InvalidOperationException(Resources.ExceptionEventsAlreadyEnabled));
		else
			_EventsCardinal--;
	}


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
		lock (_LockObject)
		{
			if (_Exposed)
				return;

			// Tracer.Trace(GetType(), "ExposeEventDelegates()", "Events NOT exposed. Exposing...");

			TextBox editCtl = EditField;

			if (editCtl == null)
				return;

			_Exposed = true;

			editCtl.GotFocus += OnEditControlGotFocus;
			editCtl.LostFocus += OnEditControlLostFocus;
			editCtl.MouseDown += OnEditControlMouseDown;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads the settings from live storage. TBC!!!
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract void LoadSettings();



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Saves settings to live storage. TBC!!!
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract void SaveSettings();


	public void ActivatePage()
	{
		if (_Window != null)
		{

			CancelEventArgs e = new();
			OnActivate(e);

			// Control gridView = GridView;
			// gridView?.Focus();
		}
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Override of the DialogPage ResetSettings method.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void ResetSettings()
	{
		_Window?.Refresh();
	}


	#endregion Methods




	// =========================================================================================================
	#region Event Handling - AbstruseSettingsPage
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// *** Exposes the hidden GridView SetScrollOffset() and SelectRow() methods.
	/// OnActivate handler. Fixes a glitch in VS initial display - incorrect scroll and
	/// focus.
	/// This method and OnEditControlMouseDown are currently the only methods that invoke
	/// inaccessible methods.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override void OnActivate(CancelEventArgs e)
	{
		base.OnActivate(e);

		// Sanity check
		if (_Window == null)
			return;

		if (EventsDisabled || _Initialized)
			return;

		_Initialized = true;

		// Fix glitch.

		DisableEvents();

		try
		{
			ExposeEventDelegates();

			Control gridView = GridView;

			if (gridView == null)
				return;

			if (Reflect.InvokeMethod(gridView, "SetScrollOffset", BindingFlags.Public | BindingFlags.Instance, [0]) != null)
			{
				Reflect.InvokeMethod(gridView, "SelectRow", BindingFlags.NonPublic | BindingFlags.Instance,
					[SortedByCategories ? 1 : 0]);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			EnableEvents();
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// PropertyGrid GotFocus handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnGotFocus(object sender, EventArgs e)
	{
		// Sanity check
		if (_Window == null)
			return;

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
	private void OnEditControlGotFocus(object sender, EventArgs e)
	{
		// Sanity check
		if (_Window == null)
			return;

		if (EventsDisabled || !_ValidFocusCell)
			return;


		lock (_LockObject)
		{
			GridItem gridEntry = _Window.SelectedGridItem;

			if (gridEntry == null || gridEntry.PropertyDescriptor == null
				|| string.IsNullOrWhiteSpace(gridEntry.PropertyDescriptor.Name))
			{
				return;
			}

			DisableEvents();


			EditControlFocusEventArgs args = new(_Window.SelectedGridItem);
			EditControlGotFocusEvent?.Invoke(sender, args);

			EnableEvents();
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

		// Tracer.Trace("Done OnEditBoxGotFocus().");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Edit textbox LostFocus handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnEditControlLostFocus(object sender, EventArgs e)
	{
		// Sanity check
		if (_Window == null)
			return;

		if (EventsDisabled || !_ValidFocusCell)
			return;


		DisableEvents();

		try
		{
			lock (_LockObject)
			{
				GridItem gridEntry = _Window.SelectedGridItem;

				if (gridEntry == null || gridEntry.PropertyDescriptor == null
					|| string.IsNullOrWhiteSpace(gridEntry.PropertyDescriptor.Name))
				{
					throw new InvalidOperationException($"Aborting OnEditControlLostFocus() Invalid GridEntry.");
				}


				EditControlFocusEventArgs args = new(gridEntry);
				EditControlLostFocusEvent?.Invoke(sender, args);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			EnableEvents();
		}

		// Tracer.Trace("Done OnEditControlLostFocus().");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// *** Exposes the hidden GridView DoubleClickRow() and OnBtnClick() methods.
	/// Overload for PropertyGridView.OnEditMouseDown. Invokes double click for boolean
	/// converters and drop down for enum converters.
	/// We're keeping any access or updating of inaccessible members that are exposed
	/// using Reflection contained within this class so that the rest of the extension
	/// does not break any coding protocols.
	/// This method and OnActivate are currently the only methods that invoke
	/// inaccessible methods.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnEditControlMouseDown(object sender, MouseEventArgs e)
	{
		// Tracer.Trace(GetType(), "OnEditControlMouseDown()", "Sender type: {0}", sender.GetType().FullName);

		// Sanity check
		if (_Window == null)
			return;

		if (EventsDisabled || !_ValidMouseEventCell)
			return;

		if (e.Clicks % 2 == 0 || e.Button != MouseButtons.Left)
			return;

		GridItem gridEntry = _Window.SelectedGridItem;

		DisableEvents();

		try
		{
			Control gridView = GridView;
			if (gridView == null)
				return;

			if (gridEntry.PropertyDescriptor.Converter is BooleanConverter)
			{
				int row = SelectedRow;

				Reflect.InvokeMethod(gridView, "DoubleClickRow",
					BindingFlags.Public | BindingFlags.Instance, [row, false, 2]);

				return;
			}


			Reflect.InvokeMethod(gridView, "OnBtnClick", BindingFlags.NonPublic | BindingFlags.Instance,
				[this, new EventArgs()]);

			// Sample for using async calls.
			// _ = Task.Run(() => InvokeMethodAsync("OnBtnClick",
			// 					BindingFlags.NonPublic | BindingFlags.Instance,
			// 					new object[] { this, new EventArgs() }));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			EnableEvents();
		}
	}


	private void OnPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
	{
		// Sanity check
		if (_Window == null)
			return;

		if (!_ValidMouseEventCell)
			return;


		DisableEvents();

		AutomationPropertyValueChangedEventArgs evt = null;

		try
		{
			GridItem gridEntry = e.ChangedItem;
			evt = new(e.ChangedItem, e.OldValue);

			AutomationPropertyValueChangedEvent?.Invoke(sender, evt);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}
		finally
		{
			EnableEvents();
		}

		if (evt.ReadOnlyChanged)
			_Window?.Refresh();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Selected grid item changed handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnSelectedItemChanged(object sender, SelectedGridItemChangedEventArgs e)
	{
		// Sanity check
		if (_Window == null)
			return;

		_ValidFocusCell = false;
		_ValidMouseEventCell = false;

		if (e.NewSelection != null && e.NewSelection.PropertyDescriptor != null
			&& e.NewSelection.PropertyDescriptor.Converter != null)
		{
			if (e.NewSelection.PropertyDescriptor.Converter is IBEditConverter)
			{
				_ValidFocusCell = true;
			}
			else if (e.NewSelection.PropertyDescriptor.Converter is IBAutomationConverter)
			{
				_ValidMouseEventCell = true;
			}
		}

		ExposeEventDelegates();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Dummy PropertyGrid event handler for testing events.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnTraceEvent(object sender, EventArgs e, string evt)
	{
		lock (_LockObject)
		{
			// Sanity check
			if (_Window == null)
				return;

			DisableEvents();

			try
			{
				ExposeEventDelegates();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
			finally
			{
				EnableEvents();
			}
		}
	}



	#endregion Event Handling


}
