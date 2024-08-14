// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsPanel
// Previously integrated into StatisticsPanel. Implented to parameterize the structure.

using System.Globalization;
using System.Resources;
using BlackbirdSql.Shared.Properties;



namespace BlackbirdSql.Shared.Model.QueryExecution;


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



	public struct StatisticEntity(string name, EnStatisticSpecialAction specialAction, bool calculateAverage = true)
	{
		public string Name = name;
		public EnStatisticSpecialAction SpecialAction = specialAction;
		public bool CalculateAverage = calculateAverage;


		public static ResourceManager ResMgr => AttributeResources.ResourceManager;

		public readonly string DisplayName => ResMgr.GetString("StatisticsPanelStat" + Name);

	}



	public delegate string GetCategoryValueDelegate(StatisticsSnapshot snapshot);



	/// <summary>
	/// Array of statistics categories
	/// </summary>
	public static readonly string[] SCategoryNames =
	[
		/* AttributeResources.StatisticsPanelCategorySnapshotTimestamp, */
		AttributeResources.StatisticsPanelCategoryTimeStats,
		AttributeResources.StatisticsPanelCategoryQueryProfileStats,
		AttributeResources.StatisticsPanelCategoryNetworkStats,
		AttributeResources.StatisticsPanelCategoryServerStats
	];


	/// <summary>
	/// Delegates for generating Statistic category values.
	/// </summary>
	public static readonly GetCategoryValueDelegate[] SCategoryValueDelegates =
	[
		new GetCategoryValueDelegate(GetTimeOfExecution),
		/* null, */
		null,
		null,
		null,
	];


	/// <summary>
	/// Statistic entities to be output by category.
	/// These were originally output explicity for each cell.
	/// By creating a multi-dim array, adding or changing a statistic requires:
	/// 1. Including it here in SStatisticEntities.
	/// 2. Adding the property accessor to ConnectionSnapshotCollection.
	/// 3. Including it in the ConnectionSnapshotCollection.Load() method, and
	/// 4. Including it in StatisticsSnapshotCollection.RetrieveStatisticsIfNeeded()
	/// </summary>
	public static readonly StatisticEntity[][] SStatisticEntities =
	[
		// ClientExecutionTime
		/* new StatisticEntity[0], */

		// TimeStatistics
		[
			new StatisticEntity("ExecutionStartTimeEpoch", EnStatisticSpecialAction.DateTimeFormat, false),
			new StatisticEntity("ExecutionEndTimeEpoch", EnStatisticSpecialAction.DateTimeFormat, false),
			new StatisticEntity("ExecutionTimeTicks", EnStatisticSpecialAction.ElapsedTimeFormat),
		],

		// QueryProfileStatistics
		[
			new StatisticEntity("StatementCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("SelectRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("IduRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("InsRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("UpdRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("DelRowCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("ReadIdxCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("ReadSeqCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("ExpungeCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("PurgeCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("Transactions", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("InsRowEntities", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("UpdRowEntities", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("DelRowEntities", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("ReadIdxEntities", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("ReadSeqEntities", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("ExpungeEntities", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("PurgeEntities", EnStatisticSpecialAction.SIFormat)
		],

		// NetworkStatistics
		[
			new StatisticEntity("ServerCacheReadCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("ServerCacheWriteCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("BufferCount", EnStatisticSpecialAction.SIFormat, false),
			new StatisticEntity("ReadCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("WriteCount", EnStatisticSpecialAction.SIFormat),
			new StatisticEntity("PacketSize", EnStatisticSpecialAction.ByteFormat, false)
		],

		// ServerStatistics
		[
			new StatisticEntity("AllocationPages", EnStatisticSpecialAction.SIFormat, false),
			new StatisticEntity("CurrentMemory", EnStatisticSpecialAction.ByteFormat),
			new StatisticEntity("MaxMemory", EnStatisticSpecialAction.ByteFormat),
			new StatisticEntity("DatabaseSizeInPages", EnStatisticSpecialAction.SIFormat, false),
			new StatisticEntity("PageSize", EnStatisticSpecialAction.ByteFormat, false),
			new StatisticEntity("ActiveUserCount", EnStatisticSpecialAction.SIFormat)
		]
	];



	public static string GetTimeOfExecution(StatisticsSnapshot snapshot)
	{
		return snapshot.TimeOfExecution.ToString("T", CultureInfo.InvariantCulture);
	}

}
