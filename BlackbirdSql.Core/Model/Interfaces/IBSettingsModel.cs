//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using BlackbirdSql.Core.Controls.Events;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Interfaces;

namespace BlackbirdSql.Core.Model.Interfaces;

public interface IBSettingsModel : IComponent
{
	// delegate void SelectedItemChangedEventHandler(object sender, SelectedGridItemChangedEventArgs e);
	delegate void SelectedItemFocusEventHandler(object sender, SelectedGridItemFocusEventArgs e);
	delegate void GridItemValueChangedEventHandler(object sender, GridItemValueChangedEventArgs e);

	event AutomationVerbEventHandler SettingsResetEvent;
	// event SelectedItemChangedEventHandler SelectedItemChangedEvent;
	event SelectedItemFocusEventHandler GridEditBoxGotFocusEvent;
	event SelectedItemFocusEventHandler GridEditBoxLostFocusEvent;
	event GridItemValueChangedEventHandler GridItemValueChangedEvent;

	PropertyWrapper this[string propertyName] { get; }

	string LivePrefix { get; }

	object Owner { get; set; }

	List<IBSettingsModelPropertyWrapper> PropertyWrappers { get; }

	string GetPackage();
	string GetGroup();

	// void OnSelectedItemChanged(object sender, SelectedGridItemChangedEventArgs e);
	void OnGridEditBoxGotFocus(object sender, SelectedGridItemFocusEventArgs e);
	void OnGridEditBoxLostFocus(object sender, SelectedGridItemFocusEventArgs e);
	void OnGridItemValueChanged(object sender, GridItemValueChangedEventArgs e);
}
