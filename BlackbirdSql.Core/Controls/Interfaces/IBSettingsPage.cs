using System.Windows.Forms;
using BlackbirdSql.Sys;


namespace BlackbirdSql.Core.Controls.Interfaces;

public interface IBSettingsPage
{
	delegate void EditControlFocusEventHandler(object sender, EditControlFocusEventArgs e);
	delegate void AutomatorPropertyValueChangedEventHandler(object sender, AutomatorPropertyValueChangedEventArgs e);


	event EditControlFocusEventHandler EditControlGotFocusEvent;
	event EditControlFocusEventHandler EditControlLostFocusEvent;
	event AutomatorPropertyValueChangedEventHandler AutomatorPropertyValueChangedEvent;


	PropertyGrid Grid { get; }

	void ActivatePage();
	void LoadSettings();
	void SaveSettings();
}
