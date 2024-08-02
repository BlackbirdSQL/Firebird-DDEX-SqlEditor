
using System;
using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql.Shared.Events;


public delegate bool NotifyConnectionStateEventHandler(object sender, NotifyConnectionStateEventArgs args);


public class NotifyConnectionStateEventArgs : EventArgs
{
	private bool _Islocked = false;
	private readonly EnNotifyConnectionState _State;
	private readonly bool _TtsDiscarded;


	public bool IsLocked
	{
		get { return _Islocked; }
		set { _Islocked = value; }
	}

	public EnNotifyConnectionState State => _State;
	public bool TtsDiscarded => _TtsDiscarded;

	private NotifyConnectionStateEventArgs()
	{
	}

	public NotifyConnectionStateEventArgs(EnNotifyConnectionState state, bool ttsDiscarded)
	{
		_State = state;
		_TtsDiscarded = ttsDiscarded;
	}
}
