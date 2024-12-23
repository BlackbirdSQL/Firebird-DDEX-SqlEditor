// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.DataConnectionAdvancedDlg
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.ComponentModel.Com2Interop;
using BlackbirdSql.VisualStudio.Ddex.Enums;
using BlackbirdSql.VisualStudio.Ddex.Events;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using Microsoft.Data.ConnectionUI;


namespace BlackbirdSql.VisualStudio.Ddex.Controls.DataTools;

public partial class VxbConnectionAdvancedDialog : Form
{
	public class VxiSpecializedPropertyGrid : PropertyGrid
	{
		private readonly ContextMenuStrip _contextMenu;

		public VxiSpecializedPropertyGrid()
		{
			_contextMenu = new ContextMenuStrip();
			_contextMenu.Items.AddRange(
			[
				new ToolStripMenuItem(),
				new ToolStripSeparator(),
				new ToolStripMenuItem(),
				new ToolStripMenuItem(),
				new ToolStripSeparator(),
				new ToolStripMenuItem()
			]);
			_contextMenu.Items[0].Text = ControlsResources.TDataConnectionAdvancedDlg_Reset;
			_contextMenu.Items[0].Click += ResetProperty;
			_contextMenu.Items[2].Text = ControlsResources.TDataConnectionAdvancedDlg_Add;
			_contextMenu.Items[2].Click += AddProperty;
			_contextMenu.Items[3].Text = ControlsResources.TDataConnectionAdvancedDlg_Remove;
			_contextMenu.Items[3].Click += RemoveProperty;
			_contextMenu.Items[5].Text = ControlsResources.TDataConnectionAdvancedDlg_Description;
			_contextMenu.Items[5].Click += ToggleDescription;
			(_contextMenu.Items[5] as ToolStripMenuItem).Checked = HelpVisible;
			_contextMenu.Opened += SetupContextMenu;
			ContextMenuStrip = _contextMenu;
			base.DrawFlatToolbar = true;
			base.Size = new Size(270, 250);
			MinimumSize = base.Size;
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			ProfessionalColorTable professionalColorTable = ((base.ParentForm != null && base.ParentForm.Site != null) ? (base.ParentForm.Site.GetService(typeof(ProfessionalColorTable)) as ProfessionalColorTable) : null);
			if (professionalColorTable != null)
			{
				base.ToolStripRenderer = new ToolStripProfessionalRenderer(professionalColorTable);
			}
			base.OnHandleCreated(e);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			base.LargeButtons = (double)Font.SizeInPoints >= 15.0;
		}

		protected override void WndProc(ref Message m)
		{
			int msg = m.Msg;
			if (msg == 7)
			{
				Focus();
				((IComPropertyBrowser)this).HandleF4();
			}
			base.WndProc(ref m);
		}

		private void SetupContextMenu(object sender, EventArgs e)
		{
			_contextMenu.Items[0].Enabled = base.SelectedGridItem.GridItemType == GridItemType.Property;
			bool enabled;
			if (_contextMenu.Items[0].Enabled && base.SelectedGridItem.PropertyDescriptor != null)
			{
				object component = base.SelectedObject;
				if (base.SelectedObject is ICustomTypeDescriptor)
				{
					component = (base.SelectedObject as ICustomTypeDescriptor).GetPropertyOwner(base.SelectedGridItem.PropertyDescriptor);
				}
				ToolStripItem toolStripItem = _contextMenu.Items[0];
				enabled = (_contextMenu.Items[3].Enabled = base.SelectedGridItem.PropertyDescriptor.CanResetValue(component));
				toolStripItem.Enabled = enabled;
			}
			ToolStripItem toolStripItem2 = _contextMenu.Items[2];
			enabled = (_contextMenu.Items[3].Visible = (base.SelectedObject as IDataConnectionProperties).IsExtensible);
			toolStripItem2.Visible = enabled;
			if (_contextMenu.Items[3].Visible)
			{
				_contextMenu.Items[3].Enabled = base.SelectedGridItem.GridItemType == GridItemType.Property;
				if (_contextMenu.Items[3].Enabled && base.SelectedGridItem.PropertyDescriptor != null)
				{
					_contextMenu.Items[3].Enabled = !base.SelectedGridItem.PropertyDescriptor.IsReadOnly;
				}
			}
			_contextMenu.Items[1].Visible = _contextMenu.Items[2].Visible || _contextMenu.Items[3].Visible;
		}

		private void ResetProperty(object sender, EventArgs e)
		{
			object value = base.SelectedGridItem.Value;
			object component = base.SelectedObject;
			if (base.SelectedObject is ICustomTypeDescriptor)
			{
				component = (base.SelectedObject as ICustomTypeDescriptor).GetPropertyOwner(base.SelectedGridItem.PropertyDescriptor);
			}
			base.SelectedGridItem.PropertyDescriptor.ResetValue(component);
			Refresh();
			OnPropertyValueChanged(new PropertyValueChangedEventArgs(base.SelectedGridItem, value));
		}

		private void AddProperty(object sender, EventArgs e)
		{
			VxbConnectionDialog dataConnectionDialog = base.ParentForm as VxbConnectionDialog;
			dataConnectionDialog ??= (base.ParentForm as VxbConnectionAdvancedDialog)._mainDialog;
			VxbAddPropertyDialog addPropertyDialog = new VxbAddPropertyDialog(dataConnectionDialog);
			try
			{
				base.ParentForm.Container?.Add(addPropertyDialog);
				DialogResult dialogResult = addPropertyDialog.ShowDialog(base.ParentForm);
				if (dialogResult == DialogResult.OK)
				{
					(base.SelectedObject as IDataConnectionProperties).Add(addPropertyDialog.PropertyName);
					Refresh();
					GridItem selectedGridItem = base.SelectedGridItem;
					while (selectedGridItem.Parent != null)
					{
						selectedGridItem = selectedGridItem.Parent;
					}
					GridItem gridItem = LocateGridItem(selectedGridItem, addPropertyDialog.PropertyName);
					if (gridItem != null)
					{
						base.SelectedGridItem = gridItem;
					}
				}
			}
			finally
			{
				base.ParentForm.Container?.Remove(addPropertyDialog);
				addPropertyDialog.Dispose();
			}
		}

		private void RemoveProperty(object sender, EventArgs e)
		{
			(base.SelectedObject as IDataConnectionProperties).Remove(base.SelectedGridItem.Label);
			Refresh();
			OnPropertyValueChanged(new PropertyValueChangedEventArgs(null, null));
		}

		private void ToggleDescription(object sender, EventArgs e)
		{
			HelpVisible = !HelpVisible;
			(_contextMenu.Items[5] as ToolStripMenuItem).Checked = !(_contextMenu.Items[5] as ToolStripMenuItem).Checked;
		}

		private GridItem LocateGridItem(GridItem currentItem, string propertyName)
		{
			if (currentItem.GridItemType == GridItemType.Property && currentItem.Label.Equals(propertyName, StringComparison.CurrentCulture))
			{
				return currentItem;
			}
			GridItem gridItem = null;
			foreach (GridItem gridItem2 in currentItem.GridItems)
			{
				gridItem = LocateGridItem(gridItem2, propertyName);
				if (gridItem != null)
				{
					break;
				}
			}
			return gridItem;
		}
	}

	private readonly string _savedConnectionString;

	private readonly VxbConnectionDialog _mainDialog;

	private VxiSpecializedPropertyGrid propertyGrid;

	public VxbConnectionAdvancedDialog()
	{
		InitializeComponent();
		components ??= new Container();
		components.Add(new UserPreferenceChangedHandler(this));
	}

	public VxbConnectionAdvancedDialog(IDataConnectionProperties connectionProperties, VxbConnectionDialog mainDialog)
		: this()
	{
		_savedConnectionString = connectionProperties.ToFullString();
		propertyGrid.SelectedObject = connectionProperties;
		_mainDialog = mainDialog;
	}

	public int GetMaximumWidth(Control c)
	{
		if (c.Parent == null)
		{
			return c.Width;
		}
		return c.Parent.ClientSize.Width - c.Parent.Padding.Right - c.Margin.Right - c.Left;
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		ConfigureTextBox();
		SuspendLayout();
		propertyGrid.Width = GetMaximumWidth(propertyGrid);
		textBox.Width = GetMaximumWidth(textBox);
		buttonsTableLayoutPanel.Left += GetMaximumWidth(buttonsTableLayoutPanel) - buttonsTableLayoutPanel.Width;
		ResumeLayout();
		MinimumSize = base.Size;
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		propertyGrid.Focus();
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		textBox.Width = propertyGrid.Width;
	}

	protected override void OnHelpRequested(HelpEventArgs hevent)
	{
		Control control = this;
		// ContainerControl containerControl = null;
		while (control is ContainerControl containerControl2 && containerControl2 != propertyGrid && containerControl2.ActiveControl != null)
		{
			control = containerControl2.ActiveControl;
		}
		EnDataConnectionDlgContext context = EnDataConnectionDlgContext.Advanced;
		if (control == propertyGrid)
		{
			context = EnDataConnectionDlgContext.AdvancedPropertyGrid;
		}
		if (control == textBox)
		{
			context = EnDataConnectionDlgContext.AdvancedTextBox;
		}
		if (control == okButton)
		{
			context = EnDataConnectionDlgContext.AdvancedOkButton;
		}
		if (control == cancelButton)
		{
			context = EnDataConnectionDlgContext.AdvancedCancelButton;
		}
		ContextHelpEventArgs contextHelpEventArgs = new ContextHelpEventArgs(context, hevent.MousePos);
		_mainDialog.OnContextHelpRequested(contextHelpEventArgs);
		hevent.Handled = contextHelpEventArgs.Handled;
		if (!contextHelpEventArgs.Handled)
		{
			base.OnHelpRequested(hevent);
		}
	}

	protected override void WndProc(ref Message m)
	{
		/*
		if (_mainDialog.TranslateHelpButton && HelpUtils.IsContextHelpMessage(ref m))
		{
			HelpUtils.TranslateContextHelpMessage(this, ref m);
		}
		*/
		base.WndProc(ref m);
	}

	private void SetTextBox(object s, PropertyValueChangedEventArgs e)
	{
		ConfigureTextBox();
	}

	private void ConfigureTextBox()
	{
		if (propertyGrid.SelectedObject is IDataConnectionProperties)
		{
			try
			{
				textBox.Text = (propertyGrid.SelectedObject as IDataConnectionProperties).ToDisplayString();
				return;
			}
			catch
			{
				textBox.Text = null;
				return;
			}
		}
		textBox.Text = null;
	}

	private void RevertProperties(object sender, EventArgs e)
	{
		try
		{
			(propertyGrid.SelectedObject as IDataConnectionProperties).Parse(_savedConnectionString);
		}
		catch
		{
		}
	}

}
