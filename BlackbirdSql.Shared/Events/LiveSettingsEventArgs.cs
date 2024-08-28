
using System;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Events;



public class LiveSettingsEventArgs(IBsEditorTransientSettings liveSettings) : EventArgs
{
	public IBsEditorTransientSettings LiveSettings { get; private set; } = liveSettings;
}
