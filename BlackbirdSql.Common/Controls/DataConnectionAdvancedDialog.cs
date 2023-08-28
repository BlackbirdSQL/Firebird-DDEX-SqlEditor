// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.DataConnectionAdvancedDialog
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.ComponentModel.Com2Interop;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Common.Properties;



namespace BlackbirdSql.Common.Controls;


public class DataConnectionAdvancedDialog : Form
{
	public class SpecializedPropertyGrid : PropertyGrid
	{
		private readonly ContextMenuStrip _ContextMenu;

		public SpecializedPropertyGrid()
		{
			_ContextMenu = new ContextMenuStrip();
			_ContextMenu.Items.AddRange(new ToolStripItem[6]
			{
				new ToolStripMenuItem(),
				new ToolStripSeparator(),
				new ToolStripMenuItem(),
				new ToolStripMenuItem(),
				new ToolStripSeparator(),
				new ToolStripMenuItem()
			});
			_ContextMenu.Items[0].Text = SharedResx.DataConnectionAdvancedDialog_Reset;
			_ContextMenu.Items[0].Click += ResetProperty;
			_ContextMenu.Items[3].Text = SharedResx.DataConnectionAdvancedDialog_Remove;
			_ContextMenu.Items[3].Click += RemoveProperty;
			_ContextMenu.Items[5].Text = SharedResx.DataConnectionAdvancedDialog_Description;
			_ContextMenu.Items[5].Click += ToggleDescription;
			(_ContextMenu.Items[5] as ToolStripMenuItem).Checked = HelpVisible;
			_ContextMenu.Opened += SetupContextMenu;
			ContextMenuStrip = _ContextMenu;
			DrawFlatToolbar = true;
			Size = new Size(270, 250);
			MinimumSize = Size;
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			ProfessionalColorTable professionalColorTable = ParentForm != null && ParentForm.Site != null ? ParentForm.Site.GetService(typeof(ProfessionalColorTable)) as ProfessionalColorTable : null;
			if (professionalColorTable != null)
			{
				ToolStripRenderer = new ToolStripProfessionalRenderer(professionalColorTable);
			}
			base.OnHandleCreated(e);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			LargeButtons = (double)Font.SizeInPoints >= 15.0;
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 7)
			{
				Focus();
				((IComPropertyBrowser)this).HandleF4();
			}
			base.WndProc(ref m);
		}

		private void SetupContextMenu(object sender, EventArgs e)
		{
			_ContextMenu.Items[0].Enabled = SelectedGridItem.GridItemType == GridItemType.Property;
			bool enabled;
			if (_ContextMenu.Items[0].Enabled && SelectedGridItem.PropertyDescriptor != null)
			{
				object component = SelectedObject;
				if (SelectedObject is ICustomTypeDescriptor)
				{
					component = (SelectedObject as ICustomTypeDescriptor).GetPropertyOwner(SelectedGridItem.PropertyDescriptor);
				}
				ToolStripItem toolStripItem = _ContextMenu.Items[0];
				enabled = _ContextMenu.Items[3].Enabled = SelectedGridItem.PropertyDescriptor.CanResetValue(component);
				toolStripItem.Enabled = enabled;
			}
			ToolStripItem toolStripItem2 = _ContextMenu.Items[2];
			enabled = _ContextMenu.Items[3].Visible = (SelectedObject as IBPropertyAgent).IsExtensible;
			toolStripItem2.Visible = enabled;
			if (_ContextMenu.Items[3].Visible)
			{
				_ContextMenu.Items[3].Enabled = SelectedGridItem.GridItemType == GridItemType.Property;
				if (_ContextMenu.Items[3].Enabled && SelectedGridItem.PropertyDescriptor != null)
				{
					_ContextMenu.Items[3].Enabled = !SelectedGridItem.PropertyDescriptor.IsReadOnly;
				}
			}
			_ContextMenu.Items[1].Visible = _ContextMenu.Items[2].Visible || _ContextMenu.Items[3].Visible;
		}

		private void ResetProperty(object sender, EventArgs e)
		{
			object value = SelectedGridItem.Value;
			object component = SelectedObject;
			if (SelectedObject is ICustomTypeDescriptor)
			{
				component = (SelectedObject as ICustomTypeDescriptor).GetPropertyOwner(SelectedGridItem.PropertyDescriptor);
			}
			SelectedGridItem.PropertyDescriptor.ResetValue(component);
			Refresh();
			OnPropertyValueChanged(new PropertyValueChangedEventArgs(SelectedGridItem, value));
		}

		private void RemoveProperty(object sender, EventArgs e)
		{
			(SelectedObject as IBPropertyAgent).Remove(SelectedGridItem.Label);
			Refresh();
			OnPropertyValueChanged(new PropertyValueChangedEventArgs(null, null));
		}

		private void ToggleDescription(object sender, EventArgs e)
		{
			HelpVisible = !HelpVisible;
			(_ContextMenu.Items[5] as ToolStripMenuItem).Checked = !(_ContextMenu.Items[5] as ToolStripMenuItem).Checked;
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

	private IContainer components;

	private TextBox textBox;

	private TableLayoutPanel buttonsTableLayoutPanel;

	private Button okButton;

	private Button cancelButton;

	private SpecializedPropertyGrid propertyGrid;

	private TableLayoutPanel tableLayoutPanel1;

	public DataConnectionAdvancedDialog()
	{
		InitializeComponent();
		components ??= new Container();
	}

	public DataConnectionAdvancedDialog(IBPropertyAgent connectionProperties)
		: this()
	{
		UiTracer.TraceSource.AssertTraceEvent(connectionProperties != null, TraceEventType.Error, EnUiTraceId.Connection, "connectionProperties is null");
		_savedConnectionString = connectionProperties.ToFullString();
		propertyGrid.SelectedObject = connectionProperties;
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
		MinimumSize = Size;
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
		while (control is ContainerControl containerControl2
			&& containerControl2 != propertyGrid && containerControl2.ActiveControl != null)
		{
			control = containerControl2.ActiveControl;
		}
	}

	private void SetTextBox(object s, PropertyValueChangedEventArgs e)
	{
		ConfigureTextBox();
	}

	private void ConfigureTextBox()
	{
		if (propertyGrid.SelectedObject is IBPropertyAgent)
		{
			try
			{
				textBox.Text = (propertyGrid.SelectedObject as IBPropertyAgent).ToDisplayString();
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
			(propertyGrid.SelectedObject as IBPropertyAgent).Parse(_savedConnectionString);
		}
		catch
		{
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		components = new Container();
		ComponentResourceManager resources = new ComponentResourceManager(typeof(DataConnectionAdvancedDialog));
		propertyGrid = new SpecializedPropertyGrid();
		textBox = new TextBox();
		buttonsTableLayoutPanel = new TableLayoutPanel();
		okButton = new Button();
		cancelButton = new Button();
		tableLayoutPanel1 = new TableLayoutPanel();
		buttonsTableLayoutPanel.SuspendLayout();
		tableLayoutPanel1.SuspendLayout();
		SuspendLayout();
		resources.ApplyResources(propertyGrid, "propertyGrid");
		propertyGrid.CategoryForeColor = SystemColors.InactiveCaptionText;
		propertyGrid.CommandsActiveLinkColor = SystemColors.ActiveCaption;
		propertyGrid.CommandsDisabledLinkColor = SystemColors.ControlDark;
		propertyGrid.CommandsLinkColor = SystemColors.ActiveCaption;
		propertyGrid.Name = "propertyGrid";
		propertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(SetTextBox);
		resources.ApplyResources(textBox, "textBox");
		textBox.Name = "textBox";
		textBox.ReadOnly = true;
		resources.ApplyResources(buttonsTableLayoutPanel, "buttonsTableLayoutPanel");
		buttonsTableLayoutPanel.Controls.Add(okButton, 1, 0);
		buttonsTableLayoutPanel.Controls.Add(cancelButton, 2, 0);
		buttonsTableLayoutPanel.Name = "buttonsTableLayoutPanel";
		resources.ApplyResources(okButton, "okButton");
		okButton.DialogResult = DialogResult.OK;
		okButton.Name = "okButton";
		resources.ApplyResources(cancelButton, "cancelButton");
		cancelButton.DialogResult = DialogResult.Cancel;
		cancelButton.Name = "cancelButton";
		cancelButton.Click += new EventHandler(RevertProperties);
		resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
		tableLayoutPanel1.Controls.Add(propertyGrid, 0, 0);
		tableLayoutPanel1.Controls.Add(buttonsTableLayoutPanel, 0, 2);
		tableLayoutPanel1.Controls.Add(textBox, 0, 1);
		tableLayoutPanel1.Name = "tableLayoutPanel1";
		AcceptButton = okButton;
		resources.ApplyResources(this, "$this");
		AutoScaleMode = AutoScaleMode.Font;
		CancelButton = cancelButton;
		Controls.Add(tableLayoutPanel1);
		MaximizeBox = false;
		MinimizeBox = false;
		Name = "DataConnectionAdvancedDialog";
		ShowIcon = false;
		ShowInTaskbar = false;
		buttonsTableLayoutPanel.ResumeLayout(false);
		buttonsTableLayoutPanel.PerformLayout();
		tableLayoutPanel1.ResumeLayout(false);
		tableLayoutPanel1.PerformLayout();
		ResumeLayout(false);
	}


	public static int GetMaximumWidth(Control c)
	{
		if (c.Parent == null)
		{
			return c.Width;
		}
		return c.Parent.ClientSize.Width - c.Parent.Padding.Right - c.Margin.Right - c.Left;
	}

}