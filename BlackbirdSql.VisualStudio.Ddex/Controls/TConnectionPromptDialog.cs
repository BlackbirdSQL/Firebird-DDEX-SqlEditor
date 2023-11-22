// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Drawing;
using System.Windows.Forms;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model;

using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex.Controls;


// =========================================================================================================
//										TConnectionPromptDialog Class
//
/// <summary>
/// Implementation of the password <see cref="IVsDataConnectionPromptDialog"/> interface
/// </summary>
// =========================================================================================================
public partial class TConnectionPromptDialog : DataConnectionPromptDialog
{

	// ---------------------------------------------------------------------------------
	#region Property Accessors - TConnectionPromptDialog
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Determines if the connection properties for the dialog (exluding password) are
	/// complete (ReadOnly) and returns true if they are.
	/// </summary>
	public bool IsReadOnly
	{
		get
		{
			try
			{
				foreach (Describer descriptor in CorePropertySet.Describers)
				{
					if (!descriptor.IsPublicMandatory)
						continue;
					if (!ConnectionUIProperties.ContainsKey(descriptor.DerivedConnectionParameter))
					{
						Diag.Stack("ConnectionUIProperties public mandatory core property missing: " + descriptor.DerivedConnectionParameter);
						return false;
					}
				}

				foreach (Describer descriptor in ModelPropertySet.Describers)
				{
					if (!descriptor.IsPublicMandatory)
						continue;
					if (!ConnectionUIProperties.ContainsKey(descriptor.DerivedConnectionParameter))
					{
						Diag.Stack("ConnectionUIProperties public mandatory schema property missing: " + descriptor.DerivedConnectionParameter);
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			return true;
		}
	}


	#endregion Property Accessors





	// =========================================================================================================
	#region Constructors / Destructors - TConnectionPromptDialog
	// =========================================================================================================


	public TConnectionPromptDialog()
	{
		// Tracer.Trace(GetType(), "TConnectionPromptDialog.TConnectionPromptDialog");

		try
		{
			InitializeComponent();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		int width = savePasswordCheckBox.PreferredSize.Width;
		if (savePasswordCheckBox.Width < width)
		{
			Width += width - savePasswordCheckBox.Width;
		}

		int preferredHeight = headerLabel.PreferredHeight;
		if (headerLabel.Height > preferredHeight)
		{
			Height += headerLabel.Height - preferredHeight;
		}

		MinimumSize = new Size(Width, Height);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations & Overloads - TConnectionPromptDialog
	// =========================================================================================================


#if DEBUG
	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Overloads the password prompt dialog [For debug exception tracking]
	/// </summary>
	// ---------------------------------------------------------------------------------
	public new string ShowDialog(IVsDataConnectionSupport dataConnectionSupport)
	{
		// Tracer.Trace(GetType(), "ShowDialog()");

		try
		{
			return base.ShowDialog(dataConnectionSupport);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}
#endif


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads the current <see cref="IVsDataConnectionUIProperties"/> connection properties
	/// into the form data
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override void LoadProperties()
	{
		// Tracer.Trace(GetType(), "LoadProperties()");

		try
		{
			ConnectionUIProperties.Parse(ConnectionSupport.ConnectionString);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		try
		{
			serverTextBox.Text = ConnectionUIProperties.ContainsKey("DataSource") ? ConnectionUIProperties["DataSource"] as string : "";
			databaseTextBox.Text = ConnectionUIProperties.ContainsKey("Database") ? ConnectionUIProperties["Database"] as string : "";

			userNameTextBox.Text = ConnectionUIProperties.ContainsKey("User ID") ? ConnectionUIProperties["User ID"] as string : "";
			if (ConnectionUIProperties.TryGetValue("Password", out object value))
				passwordTextBox.Text = value as string;
			else
				okButton.Enabled = false;
			// if (ConnectionUIProperties.TryGetValue("Persist Security Info", out value))
			//	savePasswordCheckBox.Checked = Convert.ToBoolean(value);


			if (IsReadOnly)
			{
				serverTextBox.ReadOnly = true;
				databaseTextBox.ReadOnly = true;
				userNameTextBox.ReadOnly = true;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Assigns the form data to the <see cref="IVsDataConnectionUIProperties"/> connection properties.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override void SaveProperties()
	{
		try
		{
			if (serverTextBox.Enabled)
			{
				// if (serverTextBox.Text.Trim() == "")
				//	ConnectionUIProperties.Remove("DataSource");
				// else
					ConnectionUIProperties["DataSource"] = serverTextBox.Text.Trim();
			}

			if (databaseTextBox.Enabled)
			{
				// if (databaseTextBox.Text.Trim() == "")
				//	ConnectionUIProperties.Remove("Database");
				// else
					ConnectionUIProperties["Database"] = databaseTextBox.Text.Trim();
			}

			if (userNameTextBox.Enabled)
			{
				// if (userNameTextBox.Text.Trim() == "")
				//	ConnectionUIProperties.Remove("User ID");
				// else
					ConnectionUIProperties["User ID"] = userNameTextBox.Text.Trim();
			}

			// if (passwordTextBox.Text.Trim() == "")
			//	ConnectionUIProperties.Remove("Password");
			// else
				ConnectionUIProperties["Password"] = passwordTextBox.Text;

			ConnectionSupport.ConnectionString = ConnectionUIProperties.ToString();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}


	#endregion Method Implementations & Overloads



	// =========================================================================================================
	#region Event Handlers & Implementations - TConnectionPromptDiialog
	// =========================================================================================================


	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (e.Cancel)
			return;

		if (DialogResult == DialogResult.OK)
			base.OnFormClosing(e);
	}


	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		int num = buttonsTableLayoutPanel.Top - buttonsTableLayoutPanel.Margin.Top - loginTableLayoutPanel.Margin.Bottom - loginTableLayoutPanel.Bottom;
		MinimumSize = new Size(MinimumSize.Width, MinimumSize.Height - num);
		Height += -num;
	}


	protected override void OnShown(EventArgs e)
	{
		try
		{
			if (IsReadOnly)
			{
				passwordTextBox.Focus();
			}

			base.OnShown(e);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}


	protected override void OnHelpRequested(HelpEventArgs hevent)
	{
		// Host.ShowHelp("vs.sqlconnectionpromptdialog");
		// hevent.Handled = true;
		base.OnHelpRequested(hevent);
	}


	private void TrimControlText(object sender, EventArgs e)
	{
		Control control = sender as Control;
		control.Text = control.Text.Trim();
	}


	private void ResetControlText(object sender, EventArgs e)
	{
		Control control = sender as Control;
		control.Text = control.Text;
	}


	private void SetOkButtonStatus(object sender, EventArgs e)
	{
		okButton.Enabled = passwordTextBox.Text.Trim().Length > 0
			&& userNameTextBox.Text.Trim().Length > 0
			&& serverTextBox.Text.Trim().Length > 0;
	}


	/*
	 * protected override void WndProc(ref Message m)
	{
		if (Host.TranslateContextHelpMessage((Form)this, ref m))
		{
			DefWndProc(ref m);
		}
		else
		{
			base.WndProc(ref m);
		}
	}
	*/


	#endregion Event Handlers & Implementations

}