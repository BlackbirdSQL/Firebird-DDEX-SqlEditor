// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.CustomZoom
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using BlackbirdSql.Common.Exceptions;
using BlackbirdSql.Common.Properties;

namespace BlackbirdSql.Common.Controls.Graphing;

public class CustomZoomDlg : Form
{
	private Label zoomLabel;

	private NumericUpDown zoomNumber;

	private Button zoom;

	private Button close;

	private FlowLayoutPanel flowLayoutPanel;

#pragma warning disable CS0649 // Field 'FileEncodingDialog.components' is never assigned to, and will always have its default value null
	private readonly Container components;
#pragma warning restore CS0649 // Field 'FileEncodingDialog.components' is never assigned to, and will always have its default value null

	public decimal Zoom => zoomNumber.Value;

	public event EventHandler ZoomChangedEvent;

	public CustomZoomDlg(decimal zoom)
	{
		InitializeComponent();
		zoomNumber.Value = zoom;
		zoomNumber.Select(0, zoomNumber.Text.Length);
		zoomNumber.Focus();
		AdjustFormWidth();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlackbirdSql.Common.Controls.Graphing.CustomZoomDlg));
		this.zoomLabel = new System.Windows.Forms.Label();
		this.zoomNumber = new System.Windows.Forms.NumericUpDown();
		this.zoom = new System.Windows.Forms.Button();
		this.close = new System.Windows.Forms.Button();
		this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
		((System.ComponentModel.ISupportInitialize)this.zoomNumber).BeginInit();
		this.flowLayoutPanel.SuspendLayout();
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		resources.ApplyResources(this.zoomLabel, "zoomLabel");
		this.zoomLabel.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
		this.zoomLabel.Name = "zoomLabel";
		resources.ApplyResources(this.zoomNumber, "zoomNumber");
		this.zoomNumber.Margin = new System.Windows.Forms.Padding(0);
		this.zoomNumber.Maximum = new decimal(new int[4] { 200, 0, 0, 0 });
		this.zoomNumber.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.zoomNumber.Name = "zoomNumber";
		this.zoomNumber.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		resources.ApplyResources(this.zoom, "zoom");
		this.zoom.Name = "zoom";
		this.zoom.Click += new System.EventHandler(OnZoomClick);
		resources.ApplyResources(this.close, "close");
		this.close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.close.Name = "close";
		resources.ApplyResources(this.flowLayoutPanel, "flowLayoutPanel");
		this.flowLayoutPanel.Controls.Add(this.zoomLabel);
		this.flowLayoutPanel.Controls.Add(this.zoomNumber);
		this.flowLayoutPanel.Name = "flowLayoutPanel";
		this.flowLayoutPanel.WrapContents = false;
		base.AcceptButton = this.zoom;
		resources.ApplyResources(this, "$this");
		base.CancelButton = this.close;
		base.ControlBox = false;
		base.Controls.Add(this.flowLayoutPanel);
		base.Controls.Add(this.close);
		base.Controls.Add(this.zoom);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "CustomZoom";
		base.ShowInTaskbar = false;
		((System.ComponentModel.ISupportInitialize)this.zoomNumber).EndInit();
		this.flowLayoutPanel.ResumeLayout(false);
		this.flowLayoutPanel.PerformLayout();
		base.ResumeLayout(false);
	}

	private void OnZoomClick(object sender, EventArgs e)
	{
		try
		{
			int num = Convert.ToInt32(zoomNumber.Text, CultureInfo.InvariantCulture);
			if ((decimal)num < zoomNumber.Minimum || (decimal)num > zoomNumber.Maximum)
			{
				throw new OverflowException();
			}
		}
		catch (Exception ex)
		{
			if (ex is OverflowException || ex is InvalidCastException || ex is FormatException)
			{
				ExceptionMessageBoxCtl exceptionMessageBox = new(new ApplicationException(string.Format(ControlsResources.ZoomLevelShouldBeBetween, (int)zoomNumber.Minimum, (int)zoomNumber.Maximum)))
				{
					Caption = ControlsResources.MessageBoxCaption
				};
				exceptionMessageBox.Show(this);
				zoomNumber.Focus();
				zoomNumber.Select(0, zoomNumber.Text.Length);
				return;
			}
			throw;
		}
		ZoomChangedEvent?.Invoke(this, EventArgs.Empty);
	}

	private void AdjustFormWidth()
	{
		if (zoomNumber.Right > flowLayoutPanel.Width)
		{
			base.Width += zoomNumber.Right - flowLayoutPanel.Width;
		}
	}
}
