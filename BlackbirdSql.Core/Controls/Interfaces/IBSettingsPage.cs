using System.Windows.Forms;
using BlackbirdSql.Core.Controls.Events;


namespace BlackbirdSql.Core.Controls.Interfaces;

public interface IBSettingsPage
{
	delegate void EditControlFocusEventHandler(object sender, EditControlFocusEventArgs e);
	delegate void AutomationPropertyValueChangedEventHandler(object sender, AutomationPropertyValueChangedEventArgs e);


	event EditControlFocusEventHandler EditControlGotFocusEvent;
	event EditControlFocusEventHandler EditControlLostFocusEvent;
	event AutomationPropertyValueChangedEventHandler AutomationPropertyValueChangedEvent;


	PropertyGrid Grid { get; }

	public void LoadSettings();
	public void SaveSettings();
}
