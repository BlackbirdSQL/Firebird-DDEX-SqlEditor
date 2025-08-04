//
// Original code plagiarized from Community.VisualStudio.Toolkit extension
//
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BlackbirdSql.Core.Model.Config;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.Core.Controls.Config;

// =========================================================================================================
//										AbstractPersistentSettingsPage Class
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
public abstract class AbstractPersistentSettingsPage<TPage, T> : AbstractSettingsPage<T>
	where TPage : AbstractSettingsPage<T> where T : AbstractSettingsModel<T>, new()
{

	// ----------------------------------------------------------------
	#region Constructors / Destructors - AbstractPersistentSettingsPage
	// ----------------------------------------------------------------


	public AbstractPersistentSettingsPage()
	{
		_Model = ThreadHelper.JoinableTaskFactory.Run(new Func<Task<T>>(AbstractSettingsModel<T>.CreateInstanceAsync));
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

	#endregion Constructors / Destructors





	// =====================================================================================================
	#region Fields - AbstractPersistentSettingsPage
	// =====================================================================================================


	#endregion Fields




	// =========================================================================================================
	#region Exposing Property Accessors - AbstractPersistentSettingsPage
	// =========================================================================================================


	#endregion Exposing Property Accessors




	// =========================================================================================================
	#region Property Accessors - AbstractPersistentSettingsPage
	// =========================================================================================================


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - AbstractPersistentSettingsPage
	// =========================================================================================================



	#endregion Methods


}
