
using System;



namespace BlackbirdSql.Sys;


public class DbInfoMessageEventArgs : EventArgs
{
	#region Constructors

	public DbInfoMessageEventArgs(EventArgs internalEvent)
	{
		_InternalEventArgs = internalEvent;
	}

	#endregion





	#region Fields

	private readonly EventArgs _InternalEventArgs;

	#endregion

	#region Properties

	public EventArgs InternalEventArgs
	{
		get { return _InternalEventArgs; }
	}

	#endregion
}
