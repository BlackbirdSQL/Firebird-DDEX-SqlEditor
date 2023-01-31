/*
 *  Visual Studio DDEX Provider for FirebirdClient
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2023 GA Christos
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    GA Christos
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Common;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;




namespace BlackbirdSql.VisualStudio.Ddex;



public partial class DdexConnectionPromptDialog : DataConnectionPromptDialog
{


	public bool IsComplete
	{
		get
		{
			try
			{
				foreach (string property in Schema.DslConnectionString.MandatoryProperties)
				{
					if (!ConnectionUIProperties.ContainsKey(property))
						return false;
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


	#region · Constructors ·

	public DdexConnectionPromptDialog()
	{
		Diag.Trace();

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

	#endregion

	#region · Methods ·

	public new string ShowDialog(IVsDataConnectionSupport dataConnectionSupport)
	{
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

	protected override void LoadProperties()
	{
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
			serverTextBox.Text = ConnectionUIProperties["Data Source"] as string;
			databaseTextBox.Text = ConnectionUIProperties["Initial Catalog"] as string;

			userNameTextBox.Text = ConnectionUIProperties["User ID"] as string;
			if (ConnectionUIProperties.TryGetValue("Password", out object value))
				passwordTextBox.Text = value as string;
			// if (ConnectionUIProperties.TryGetValue("Persist Security Info", out value))
			//	savePasswordCheckBox.Checked = Convert.ToBoolean(value);


			if (IsComplete)
			{
				Diag.Trace("prompt dialog connection is complete");
				serverTextBox.ReadOnly = true;
				databaseTextBox.ReadOnly = true;
				userNameTextBox.ReadOnly = true;
			}
			else
				Diag.Trace("prompt dialog connection is NOT complete");

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

	}

	protected override void SaveProperties()
	{
		try
		{
			if (serverTextBox.Enabled)
			{
				ConnectionUIProperties["Data Source"] = serverTextBox.Text.Trim();
			}

			if (databaseTextBox.Enabled)
			{
				ConnectionUIProperties["Initial Catalog"] = databaseTextBox.Text.Trim();
			}

			if (userNameTextBox.Enabled)
			{
				ConnectionUIProperties["User ID"] = userNameTextBox.Text.Trim();
			}

			ConnectionUIProperties["Password"] = passwordTextBox.Text;
			// ConnectionUIProperties["No Garbage Collect"] = savePasswordCheckBox.Checked;
			ConnectionSupport.ConnectionString = ConnectionUIProperties.ToString();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}

	#endregion

	#region · Event Handlers ·

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
			if (IsComplete)
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
		okButton.Enabled = userNameTextBox.Text.Length > 0;
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
	#endregion
}