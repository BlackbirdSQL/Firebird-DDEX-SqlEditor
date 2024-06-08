// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsControl

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Sys.Enums;

namespace BlackbirdSql.Shared.Controls.ResultsPanels;

public class StatisticsSnapshotCollection : CollectionBase
{
	public StatisticsSnapshotCollection()
	{
	}

	public StatisticsSnapshotCollection(IDbConnection connection)
	{
		_InternalConnection = (DbConnection)connection;
	}


	public StatisticsSnapshotCollection(StatisticsSnapshotCollection value)
	{
		AddRange(value);
	}

	public StatisticsSnapshotCollection(StatisticsSnapshot[] value)
	{
		AddRange(value);
	}


	public const int C_CategoryCount = 4;


	private readonly DbConnection _InternalConnection;

	private StatisticsSnapshot _StatisticsConnectionBase = null;


	public DbConnection InternalConnection => _InternalConnection;

	public StatisticsSnapshot StatisticsConnectionBase => _StatisticsConnectionBase;




	private bool _Initialized = false;
	private string _Text;


	public string Text
	{
		get
		{
			return _Text;
		}
		set
		{
			_Text = value;
		}
	}



	public StatisticsSnapshot this[int index]
	{
		get
		{
			return (StatisticsSnapshot)List[index];
		}
		set
		{
			List[index] = value;
		}
	}



	public int Add(StatisticsSnapshot node)
	{
		return List.Add(node);
	}

	public void AddRange(StatisticsSnapshot[] nodes)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			Add(nodes[i]);
		}
	}

	public void AddRange(StatisticsSnapshotCollection value)
	{
		for (int i = 0; i < value.Count; i++)
		{
			Add(value[i]);
		}
	}


	public bool Contains(ResultSetAndGridContainer node)
	{
		return List.Contains(node);
	}

	public void CopyTo(StatisticsSnapshot[] array, int index)
	{
		List.CopyTo(array, index);
	}

	public int IndexOf(StatisticsSnapshot node)
	{
		return List.IndexOf(node);
	}


	public void Insert(int index, StatisticsSnapshot node)
	{
		List.Insert(index, node);
	}

	public void Remove(StatisticsSnapshot node)
	{
		List.Remove(node);
	}

	public void SetStatisticsBase(StatisticsSnapshot snapshot)
	{
		_StatisticsConnectionBase = snapshot;
	}


	public void ResetStatistics()
	{
		_StatisticsConnectionBase = null;
	}





	public void LoadStatisticsSnapshotBase(QueryManager qryMgr)
	{
		try
		{
			StatisticsSnapshot snapshot = new(InternalConnection);

			QESQLQueryDataEventArgs args = new(qryMgr.LiveSettings.ExecutionType, EnSqlStatementAction.Inactive,
				qryMgr.LiveSettings.EditorResultsOutputMode, qryMgr.IsWithActualPlan, qryMgr.IsWithClientStats,
				0, 0, 0, 0, qryMgr.QueryExecutionStartTime, DateTime.Now);

			snapshot.Load(args);

			SetStatisticsBase(snapshot);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			_Initialized = true;
		}
	}



	public void RetrieveStatisticsIfNeeded(QESQLQueryDataEventArgs args)
	{
		if (!_Initialized)
		{
			InvalidOperationException ex = new InvalidOperationException("StatisticsSnapshotCollection not initialized.");
			Diag.Dug(ex);
			throw ex;
		}


		try
		{
			StatisticsSnapshot snapshot = new(InternalConnection, StatisticsConnectionBase);

			snapshot.Load(args);

			Insert(0, snapshot);

			ResetStatistics();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}




}
