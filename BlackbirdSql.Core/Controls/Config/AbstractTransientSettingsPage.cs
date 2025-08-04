//
// Original code plagiarized from Community.VisualStudio.Toolkit extension
//
using System.Runtime.InteropServices;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.Core.Controls.Config;

// =========================================================================================================
//										AbstractTransientSettingsPage Class
//
/// <summary>
/// Base class for live user options settings which can be used with an implementation of
/// <see cref="AbstractTransientSettingsDialog"/>.
/// </summary>
// =========================================================================================================
[ComVisible(true)]
public abstract class AbstractTransientSettingsPage<TPage, TModel> : AbstractSettingsPage<TModel>
	where TPage : AbstractSettingsPage<TModel> where TModel : AbstractSettingsModel<TModel>, new()

{
	public AbstractTransientSettingsPage(IBsSettingsProvider transientSettings) : base()
	{
		_Model = ThreadHelper.JoinableTaskFactory.Run(() => AbstractSettingsModel<TModel>.CreateInstanceAsync(transientSettings));

		_Model.SettingsResetEvent += OnResetSettings;

		EditControlGotFocusEvent += _Model.OnEditControlGotFocus;
		EditControlLostFocusEvent += _Model.OnEditControlLostFocus;
		AutomatorPropertyValueChangedEvent += _Model.OnAutomatorPropertyValueChanged;
	}


	protected override void Dispose(bool disposing)
	{
		if (!disposing)
			return;

		if (_Model != null)
		{
			EditControlGotFocusEvent -= _Model.OnEditControlGotFocus;
			EditControlLostFocusEvent -= _Model.OnEditControlLostFocus;
			AutomatorPropertyValueChangedEvent -= _Model.OnAutomatorPropertyValueChanged;
			_Model = null;
		}

		base.Dispose(disposing);
	}

}
