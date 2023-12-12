﻿using System;
using System.Runtime.InteropServices;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model.Config;
using Microsoft.VisualStudio.Shell;

namespace BlackbirdSql.Core.Controls.Config
{
	/// <summary>
	/// A base class for a DialogPage to show in Tools -> Options.
	/// </summary>
	[ComVisible(true)]
	public class BaseOptionPage<T> : DialogPage where T : AbstractSettingsModel<T>, new()
	{
		private readonly AbstractSettingsModel<T> _model;

		/// <summary>
		/// Creates a new instance of the options page.
		/// </summary>
		public BaseOptionPage()
		{
#pragma warning disable VSTHRD104 // Offer async methods
			_model = ThreadHelper.JoinableTaskFactory.Run(AbstractSettingsModel<T>.CreateAsync);
#pragma warning restore VSTHRD104 // Offer async methods
		}

		/// <summary>The model object to load and store.</summary>
		public override object AutomationObject => _model;

		/// <summary>Loads the settings from the internal storage.</summary>
		public override void LoadSettingsFromStorage()
		{
			_model.Load();
		}

		/// <summary>Saves settings to the internal storage.</summary>
		public override void SaveSettingsToStorage()
		{
			_model.Save();
		}
	}
}