using System;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Events;


public delegate void DatabaseChangeEventHandler(object sender, DatabaseChangeEventArgs args);


public sealed class DatabaseChangeEventArgs : EventArgs
{
	private readonly IBsModelCsb _PreviousCsb;

	private readonly IBsModelCsb _CurrentCsb;



	public IBsModelCsb PreviousCsb => _PreviousCsb;

	public IBsModelCsb CurrentCsb => _CurrentCsb;



	public DatabaseChangeEventArgs(IBsModelCsb currentCsb, IBsModelCsb previousCsb)
	{
		_CurrentCsb = currentCsb;
		_PreviousCsb = previousCsb;
	}
}
