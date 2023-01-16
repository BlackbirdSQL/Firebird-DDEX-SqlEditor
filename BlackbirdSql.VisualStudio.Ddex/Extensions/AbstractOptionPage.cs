using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace BlackbirdSql.VisualStudio.Ddex.Extensions
{
	//
	// Summary:
	//     A base class for a DialogPage to show in Tools -> Options.
	[ComVisible(true)]
	public class AbstractOptionPage<T> : DialogPage where T : AbstractOptionModel<T>, new()
	{
		private readonly AbstractOptionModel<T> _model;

		//
		// Summary:
		//     The model object to load and store.
		public override object AutomationObject => _model;

		//
		// Summary:
		//     Creates a new instance of the options page.
		public AbstractOptionPage()
		{
			_model = ThreadHelper.JoinableTaskFactory.Run(new Func<Task<T>>(AbstractOptionModel<T>.CreateAsync));
		}

		//
		// Summary:
		//     Loads the settings from the internal storage.
		public override void LoadSettingsFromStorage()
		{
			_model.Load();
		}

		//
		// Summary:
		//     Saves settings to the internal storage.
		public override void SaveSettingsToStorage()
		{
			_model.Save();
		}
	}
}
