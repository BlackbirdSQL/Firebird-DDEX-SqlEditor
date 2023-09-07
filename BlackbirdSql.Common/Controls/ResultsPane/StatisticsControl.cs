// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsControl

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using BlackbirdSql.Common.Model.QueryExecution;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Services;
using Microsoft.VisualStudio.LanguageServer.Client;
using static Microsoft.ServiceHub.Framework.ServiceBrokerClient;

namespace BlackbirdSql.Common.Controls.ResultsPane;


public class StatisticsControl : CollectionBase
{
	private string m_Text;


	public string Text
	{
		get
		{
			return m_Text;
		}
		set
		{
			m_Text = value;
		}
	}



	public StatisticsConnection this[int index]
	{
		get
		{
			return (StatisticsConnection)List[index];
		}
		set
		{
			List[index] = value;
		}
	}

	public StatisticsControl()
	{
	}

	public void RetrieveStatisticsIfNeeded(QueryExecutor executor)
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				StatisticsConnection statisticsConnection = (StatisticsConnection)enumerator.Current;

				FbDatabaseInfo info = new(statisticsConnection.InternalConnection);
				Dictionary<string, object> stats = new(12);


				long value = 1L;
				stats.Add("IduCount", value);

				value = info.GetInsertCount() + info.GetUpdateCount() + info.GetDeleteCount();
				stats.Add("IduRows", value);

				value = 1L;
				stats.Add("SelectCount", value);

				value = 0L;
				stats.Add("SelectRows", value);

				value = info.GetActiveTransactionsCount();
				stats.Add("Transactions", value);

				value = info.GetFetches();
				stats.Add("ServerRoundtrips", value);

				long buffers = info.GetNumBuffers();

				value = info.GetWrites() * buffers;
				stats.Add("BuffersSent", value);

				value *= statisticsConnection.InternalConnection.PacketSize;
				stats.Add("BytesSent", value);

				value = info.GetReads() * buffers;
				stats.Add("BuffersReceived", value);

				value *= statisticsConnection.InternalConnection.PacketSize;
				stats.Add("BytesReceived", value);

				string str = string.Empty;

				if (executor.QueryExecutionStartTime.HasValue && executor.QueryExecutionEndTime.HasValue)
				{
					str = GetElapsedTime(executor.QueryExecutionStartTime.Value, executor.QueryExecutionEndTime.Value);
				}

				stats.Add("ExecutionTime", str);

				stats.Add("NetworkServerTime", str);

				statisticsConnection.Insert(0, new StatisticsTry(stats));

				// statisticsConnection.InternalConnection.ResetStatistics();
			}
		}
		finally
		{
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}

	public StatisticsControl(StatisticsControl value)
	{
		AddRange(value);
	}

	public StatisticsControl(StatisticsConnection[] value)
	{
		AddRange(value);
	}

	public int Add(StatisticsConnection node)
	{
		return List.Add(node);
	}

	public void AddRange(StatisticsConnection[] nodes)
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			Add(nodes[i]);
		}
	}

	public void AddRange(StatisticsControl value)
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

	public void CopyTo(StatisticsConnection[] array, int index)
	{
		List.CopyTo(array, index);
	}

	public int IndexOf(StatisticsConnection node)
	{
		return List.IndexOf(node);
	}

	private string GetElapsedTime(DateTime start, DateTime end)
	{
		string empty = (end - start).ToString();
		if (!string.IsNullOrEmpty(empty))
		{
			string numberDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			int num = empty.LastIndexOf(numberDecimalSeparator, StringComparison.Ordinal);
			if (num != -1)
			{
				int num2 = num + 4;
				if (num2 <= empty.Length)
				{
					empty = empty[..num2];
				}
			}
		}

		return empty;
	}

	public void Insert(int index, StatisticsConnection node)
	{
		List.Insert(index, node);
	}

	public void Remove(StatisticsConnection node)
	{
		List.Remove(node);
	}
}
