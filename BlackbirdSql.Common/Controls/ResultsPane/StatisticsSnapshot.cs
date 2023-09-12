// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsTry

using System;
using System.Collections;


namespace BlackbirdSql.Common.Controls.ResultsPane;

public class StatisticsSnapshot
{
	private readonly IDictionary _SnapshotData;

	private IDictionaryEnumerator _SnapshotEnumerator;

	private readonly DateTime _TimeOfExecution;

	public IDictionary Snapshot => _SnapshotData;

	public IDictionaryEnumerator SnapshotEnumerator
	{
		get
		{
			if (_SnapshotEnumerator == null && _SnapshotData != null)
			{
				_SnapshotEnumerator = _SnapshotData.GetEnumerator();
			}
			return _SnapshotEnumerator;
		}
	}

	public DateTime TimeOfExecution => _TimeOfExecution;

	public StatisticsSnapshot(IDictionary snapshotData)
	{
		_SnapshotData = snapshotData;
		_TimeOfExecution = DateTime.Now;
	}

}
