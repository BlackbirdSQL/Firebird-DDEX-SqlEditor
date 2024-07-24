using System.Windows.Forms;
using BlackbirdSql.Sys.Events;

namespace BlackbirdSql.Core.Interfaces;

public interface IBsSettingsPage
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
