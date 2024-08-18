//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System.Collections.Generic;
using System.ComponentModel;
using BlackbirdSql.Sys.Events;


namespace BlackbirdSql.Sys.Interfaces;

public interface IBsSettingsModel : IComponent
{
	IBsModelPropertyWrapper this[string propertyName] { get; }

	string PropertyPrefix { get; }
	List<IBsModelPropertyWrapper> PropertyWrappers { get; }
	IEnumerable<IBsModelPropertyWrapper> PropertyWrappersEnumeration { get; }
	string SettingsGroup { get; }
	string SettingsPackage { get; }



	delegate void EditControlFocusEventHandler(object sender, EditControlFocusEventArgs e);
	delegate void AutomatorPropertyValueChangedEventHandler(object sender, AutomatorPropertyValueChangedEventArgs e);

	event AutomationVerbEventHandler SettingsResetEvent;
	event EditControlFocusEventHandler EditControlGotFocusEvent;
	event EditControlFocusEventHandler EditControlLostFocusEvent;
	event AutomatorPropertyValueChangedEventHandler AutomatorPropertyValueChangedEvent;



	void VerbMethodReset();


	void OnAutomatorPropertyValueChanged(object sender, AutomatorPropertyValueChangedEventArgs e);
	void OnEditControlGotFocus(object sender, EditControlFocusEventArgs e);
	void OnEditControlLostFocus(object sender, EditControlFocusEventArgs e);
}
