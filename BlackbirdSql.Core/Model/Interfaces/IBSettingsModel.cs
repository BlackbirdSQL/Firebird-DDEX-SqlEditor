﻿//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System.Collections.Generic;
using System.ComponentModel;
using BlackbirdSql.Core.Controls.Events;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Interfaces;

namespace BlackbirdSql.Core.Model.Interfaces;

public interface IBSettingsModel : IComponent
{
	// delegate void SelectedItemChangedEventHandler(object sender, SelectedGridItemChangedEventArgs e);
	delegate void EditControlFocusEventHandler(object sender, EditControlFocusEventArgs e);
	delegate void AutomationPropertyValueChangedEventHandler(object sender, AutomationPropertyValueChangedEventArgs e);

	event AutomationVerbEventHandler SettingsResetEvent;
	// event SelectedItemChangedEventHandler SelectedItemChangedEvent;
	event EditControlFocusEventHandler EditControlGotFocusEvent;
	event EditControlFocusEventHandler EditControlLostFocusEvent;
	event AutomationPropertyValueChangedEventHandler AutomationPropertyValueChangedEvent;

	IBModelPropertyWrapper this[string propertyName] { get; }

	string LivePrefix { get; }

	List<IBModelPropertyWrapper> PropertyWrappers { get; }
	IEnumerable<IBModelPropertyWrapper> PropertyWrappersEnumeration { get; }

	string GetPackage();
	string GetGroup();

	// void OnSelectedItemChanged(object sender, SelectedGridItemChangedEventArgs e);
	void OnEditControlGotFocus(object sender, EditControlFocusEventArgs e);
	void OnEditControlLostFocus(object sender, EditControlFocusEventArgs e);
	void OnAutomationPropertyValueChanged(object sender, AutomationPropertyValueChangedEventArgs e);
}
