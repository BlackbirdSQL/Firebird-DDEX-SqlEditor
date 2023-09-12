// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsPanel

using System.Globalization;
using System.Resources;

using BlackbirdSql.Common.Properties;


namespace BlackbirdSql.Common.Controls.ResultsPane;

public class StatisticsPropertySet
{
	public enum EnStatisticSpecialAction
	{
		NoAction,
		ClientProcessingTimeAction,
		ElapsedTimeFormat,
		DateTimeFormat,
		ByteFormat,
		SIFormat
	}



	public struct StatisticEntity
	{
		public static ResourceManager ResMgr => AttributeResources.ResourceManager;

		public string Name;

		public readonly string DisplayName => ResMgr.GetString("StatisticsPanelStat" + Name);

		public EnStatisticSpecialAction SpecialAction;

		public bool CalculateAverage;

		public StatisticEntity(string name, EnStatisticSpecialAction specialAction, bool calculateAverage = true)
		{
			Name = name;
			SpecialAction = specialAction;
			CalculateAverage = calculateAverage;
		}
	}



	public delegate string GetCategoryValueDelegate(StatisticsSnapshot snapshot);



	/// <summary>
	/// Array of statistics categories
	/// </summary>
	public static readonly string[] SCategoryNames = new string[5]
	{
		AttributeResources.StatisticsPanelCategorySnapshotTimestamp,
		AttributeResources.StatisticsPanelCategoryQueryProfileStats,
		AttributeResources.StatisticsPanelCategoryNetworkStats,
		AttributeResources.StatisticsPanelCategoryTimeStats,
		AttributeResources.StatisticsPanelCategoryServerStats
	};


	/// <summary>
	/// Delegates for generating Statistic category values.
	/// </summary>
	public static readonly GetCategoryValueDelegate[] SCategoryValueDelegates = new GetCategoryValueDelegate[5]
	{
		new GetCategoryValueDelegate(GetTimeOfExecution),
		null,
		null,
		null,
		null,
	};


	/// <summary>
	/// Statistic entities to be output by category.
	/// These were originally output explicity for each cell.
	/// By creating a multi-dim array, adding or changing a statistic requires:
	/// 1. Including it here in SStatisticEntities.
	/// 2. Adding the property accessor to StatisticsConnection.
	/// 3. Including it in the StatisticsConnection.Load() method, and
	/// 4. Including it in StatisticsControl.RetrieveStatisticsIfNeeded()
	/// </summary>
	public static readonly StatisticEntity[][] SStatisticEntities = new StatisticEntity[5][]
	{
		// ClientExecutionTime
		new StatisticEntity[0],

		// QueryProfileStatistics
		new StatisticEntity[6]
		{
			new StatisticEntity("IduRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("InsRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("UpdRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("DelRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("SelectRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("Transactions", EnStatisticSpecialAction.SIFormat)
		},

		// NetworkStatistics
		new StatisticEntity[10]
		{
			new StatisticEntity("ServerRoundtrips", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("BufferCount", EnStatisticSpecialAction.SIFormat, false),
			new StatisticEntity("ReadCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("WriteCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("ReadIdxCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("ReadSeqCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("PurgeCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("ExpungeCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("Marks", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("PacketSize", EnStatisticSpecialAction.ByteFormat, false)
		},

		// TimeStatistics
		new StatisticEntity[3]
		{
			new StatisticEntity("ExecutionStartTimeEpoch", EnStatisticSpecialAction.DateTimeFormat, false),
			new StatisticEntity("ExecutionEndTimeEpoch", EnStatisticSpecialAction.DateTimeFormat, false),
			new StatisticEntity("ExecutionTimeTicks", EnStatisticSpecialAction.ElapsedTimeFormat),
		},

		// ServerStatistics
		new StatisticEntity[6]
		{
			new StatisticEntity("AllocationPages", EnStatisticSpecialAction.SIFormat, false),
			new StatisticEntity("CurrentMemory", EnStatisticSpecialAction.ByteFormat),
			new StatisticEntity("MaxMemory", EnStatisticSpecialAction.ByteFormat),
			new StatisticEntity("DatabaseSizeInPages", EnStatisticSpecialAction.SIFormat, false),
			new StatisticEntity("PageSize", EnStatisticSpecialAction.ByteFormat, false),
			new StatisticEntity("ActiveUserCount", EnStatisticSpecialAction.SIFormat)
		}
	};



	public static string GetTimeOfExecution(StatisticsSnapshot snapshot)
	{
		return snapshot.TimeOfExecution.ToString("T", CultureInfo.InvariantCulture);
	}

}
