// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Statistics

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											SmoMetaStatistics Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Statistics for providing metadata.
/// </summary>
// =========================================================================================================
internal sealed class SmoMetaStatistics : IStatistics, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaStatistics
	// ---------------------------------------------------------------------------------


	public SmoMetaStatistics(SmoMetaDatabase database, IDatabaseTable parent, Statistic smoStatistic)
	{
		this.parent = parent;
		this.smoStatistic = smoStatistic;
		columnCollection = new Cmd.StatisticsColumnCollectionHelperI(database, parent, this.smoStatistic.StatisticColumns);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaStatistics
	// =========================================================================================================


	private readonly IDatabaseTable parent;

	private readonly Statistic smoStatistic;

	private readonly Cmd.StatisticsColumnCollectionHelperI columnCollection;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaStatistics
	// =========================================================================================================


	public IMetadataOrderedCollection<IColumn> Columns => columnCollection.MetadataCollection;

	public string FilterDefinition
	{
		get
		{
			Cmd.TryGetPropertyObject<string>(smoStatistic, "FilterDefinition", out var value);
			return value;
		}
	}

	public bool NoAutomaticRecomputation => smoStatistic.NoAutomaticRecomputation;

	public ITabular Parent => parent;

	public StatisticsType Type
	{
		get
		{
			if (smoStatistic.IsAutoCreated)
			{
				return StatisticsType.Auto;
			}
			if (smoStatistic.IsFromIndexCreation)
			{
				return StatisticsType.ImplicitViaIndex;
			}
			return StatisticsType.Explicit;
		}
	}

	public string Name => smoStatistic.Name;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaStatistics
	// =========================================================================================================


	public T Accept<T>(IMetadataObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}


	#endregion Methods

}
