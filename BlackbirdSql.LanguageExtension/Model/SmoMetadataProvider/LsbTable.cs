// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Table

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbTable Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Table for providing metadata.
/// </summary>
// =========================================================================================================
internal sealed class LsbTable : AbstractTableView<Microsoft.SqlServer.Management.Smo.Table>, ITable, ITableViewBase, IDatabaseTable, ITabular, IMetadataObject, ISchemaOwnedObject, IDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbTable
	// ---------------------------------------------------------------------------------


	public LsbTable(Microsoft.SqlServer.Management.Smo.Table smoMetadataObject, LsbSchema parent)
		: base(smoMetadataObject, parent)
	{
		constraintCollection = new LsbConstraintCollection.ConstraintCollectionHelperI(base.Parent.Database, this, m_smoMetadataObject);
		indexCollection = new LsbIndex.IndexCollectionHelperI(base.Parent.Database, this, m_smoMetadataObject.Indexes);
		statisticsCollection = new Cmd.StatisticsCollectionHelperI(base.Parent.Database, this, m_smoMetadataObject.Statistics);
		triggerCollection = new LsbDmlTrigger.DmlTriggerCollectionHelperI(base.Parent.Database, this, m_smoMetadataObject.Triggers);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbTable
	// =========================================================================================================


	private readonly LsbConstraintCollection.ConstraintCollectionHelperI constraintCollection;

	private readonly LsbIndex.IndexCollectionHelperI indexCollection;

	private readonly Cmd.StatisticsCollectionHelperI statisticsCollection;

	private readonly LsbDmlTrigger.DmlTriggerCollectionHelperI triggerCollection;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbTable
	// =========================================================================================================


	public override int Id => m_smoMetadataObject.ID;

	public override bool IsSystemObject => m_smoMetadataObject.IsSystemObject;

	public override TabularType TabularType => TabularType.Table;

	public override bool IsQuotedIdentifierOn => m_smoMetadataObject.QuotedIdentifierStatus;

	protected override LsbConstraintCollection.ConstraintCollectionHelperI ConstraintCollection => constraintCollection;

	protected override LsbIndex.IndexCollectionHelperI IndexCollection => indexCollection;

	protected override Cmd.StatisticsCollectionHelperI StatisticsCollection => statisticsCollection;

	protected override LsbDmlTrigger.DmlTriggerCollectionHelperI TriggerCollection => triggerCollection;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbTable
	// =========================================================================================================


	public override T Accept<T>(ISchemaOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}


	#endregion Methods

}
