// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.PropertyViewForm
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Properties;

namespace BlackbirdSql.Shared.Controls.Graphing;

public class PropertyViewDlg : Form
{
	private TextBox viewTextBox;

	private Button closeButton;

#pragma warning disable CS0649 // Field 'PropertyViewForm.components' is never assigned to, and will always have its default value null
	private readonly Container components;
#pragma warning restore CS0649 // Field 'PropertyViewForm.components' is never assigned to, and will always have its default value null

	public string DisplayText
	{
		get
		{
			return viewTextBox.Text;
		}
		set
		{
			viewTextBox.Text = value;
		}
	}

	public PropertyViewDlg()
	{
		InitializeComponent();
		Icon = ControlsResources.IconActualExecutionPlan;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlackbirdSql.Shared.Controls.Graphing.PropertyViewDlg));
		this.viewTextBox = new System.Windows.Forms.TextBox();
		this.closeButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		resources.ApplyResources(this.viewTextBox, "viewTextBox");
		this.viewTextBox.Name = "viewTextBox";
		this.viewTextBox.ReadOnly = true;
		resources.ApplyResources(this.closeButton, "closeButton");
		this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.closeButton.Name = "closeButton";
		resources.ApplyResources(this, "$this");
		base.CancelButton = this.closeButton;
		base.Controls.Add(this.closeButton);
		base.Controls.Add(this.viewTextBox);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "PropertyViewForm";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
