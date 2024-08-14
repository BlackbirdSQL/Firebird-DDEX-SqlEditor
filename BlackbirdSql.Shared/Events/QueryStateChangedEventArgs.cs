// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.QueryExecutor.StatusChangedEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.QueryExecutor.StatusChangedEventArgs

using System;
using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql.Shared.Events;


public delegate void QueryStateChangedEventHandler(object sender, QueryStateChangedEventArgs args);


public class QueryStateChangedEventArgs : EventArgs, ICloneable
{
	public QueryStateChangedEventArgs(uint states, EnQueryState state, bool value,
		EnQueryState prevState, EnQueryState prevPrevState) : base()
	{
		_States = states;
		_State = state;
		_Value = value;
		_PrevState = prevState;
		_PrevPrevState = prevPrevState;
	}




	private uint _States;
	private EnQueryState _State;
	private bool _Value;
	private EnQueryState _PrevState;
	private readonly EnQueryState _PrevPrevState;



	public EnQueryState State => _State;
	public bool Value => _Value;
	public EnQueryState PrevState =>
		_PrevState != EnQueryState.None ? _PrevState : _PrevPrevState;

	public bool CanProcreate => _PrevState != EnQueryState.None
		&& _State > EnQueryState.VirtualMarker && !_Value;


	public bool Connected => GetValue(EnQueryState.Connected);
	public bool Executing => GetValue(EnQueryState.Executing);
	public bool Connecting => GetValue(EnQueryState.Connecting);
	public bool Cancelling => GetValue(EnQueryState.Cancelling);
	public bool Prompting => GetValue(EnQueryState.Prompting);
	public bool DatabaseChanged => GetValue(EnQueryState.DatabaseChanged);
	public bool Online => GetValue(EnQueryState.Online);

	public bool IsStateConnected => GetIsChangedState(EnQueryState.Connected);
	public bool IsStateExecuting => GetIsChangedState(EnQueryState.Executing);
	public bool IsStateConnecting => GetIsChangedState(EnQueryState.Connecting);
	public bool IsStateCancelling => GetIsChangedState(EnQueryState.Cancelling);
	public bool IsStatePrompting => GetIsChangedState(EnQueryState.Prompting);
	public bool IsStateDatabaseChanged => GetIsChangedState(EnQueryState.DatabaseChanged);
	public bool IsStateOnline => GetIsChangedState(EnQueryState.Online);

	private bool GetIsChangedState(EnQueryState status) => (_State & status) > EnQueryState.None;
	private bool GetValue(EnQueryState statusFlag) => (_States & (uint)statusFlag) > 0;


	object ICloneable.Clone() => Clone();


	public QueryStateChangedEventArgs Clone()
	{
		return new(_States, _State, _Value, _PrevState, _PrevPrevState);
	}

	public QueryStateChangedEventArgs PopClone()
	{
		if (_PrevState == EnQueryState.None)
		{
			string str = $"No clone to pop. Current state: {_Value}.";
			Diag.ThrowException(new ApplicationException(str));
		}


		QueryStateChangedEventArgs clone = new(_States, _State, _Value, _PrevState, _PrevPrevState);

		EnQueryState prevState = _PrevState;

		clone._States &= ~(uint)clone._State;
		clone._State = prevState;
		clone._Value = true;
		clone._PrevState = EnQueryState.None;


		return clone;
	}

}
