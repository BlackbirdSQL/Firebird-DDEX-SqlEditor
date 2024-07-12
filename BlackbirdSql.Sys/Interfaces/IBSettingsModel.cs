//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System.Collections.Generic;
using System.ComponentModel;
using BlackbirdSql.Sys.Events;


namespace BlackbirdSql.Sys.Interfaces;

public interface IBSettingsModel : IComponent
{
	// delegate void SelectedItemChangedEventHandler(object sender, SelectedGridItemChangedEventArgs e);
	delegate void EditControlFocusEventHandler(object sender, EditControlFocusEventArgs e);
	delegate void AutomatorPropertyValueChangedEventHandler(object sender, AutomatorPropertyValueChangedEventArgs e);

	event AutomationVerbEventHandler SettingsResetEvent;
	// event SelectedItemChangedEventHandler SelectedItemChangedEvent;
	event EditControlFocusEventHandler EditControlGotFocusEvent;
	event EditControlFocusEventHandler EditControlLostFocusEvent;
	event AutomatorPropertyValueChangedEventHandler AutomatorPropertyValueChangedEvent;

	IBModelPropertyWrapper this[string propertyName] { get; }

	string LivePrefix { get; }

	List<IBModelPropertyWrapper> PropertyWrappers { get; }
	IEnumerable<IBModelPropertyWrapper> PropertyWrappersEnumeration { get; }

	string GetPackage();
	string GetGroup();

	// void OnSelectedItemChanged(object sender, SelectedGridItemChangedEventArgs e);
	void OnEditControlGotFocus(object sender, EditControlFocusEventArgs e);
	void OnEditControlLostFocus(object sender, EditControlFocusEventArgs e);
	void OnAutomatorPropertyValueChanged(object sender, AutomatorPropertyValueChangedEventArgs e);
}
