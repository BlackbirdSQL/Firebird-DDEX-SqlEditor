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
//											SmoMetaTable Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Table for providing metadata.
/// </summary>
// =========================================================================================================
internal sealed class SmoMetaTable : AbstractSmoMetaTableView<Microsoft.SqlServer.Management.Smo.Table>, ITable, ITableViewBase, IDatabaseTable, ITabular, IMetadataObject, ISchemaOwnedObject, IDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaTable
	// ---------------------------------------------------------------------------------


	public SmoMetaTable(Microsoft.SqlServer.Management.Smo.Table smoMetadataObject, SmoMetaSchema parent)
		: base(smoMetadataObject, parent)
	{
		constraintCollection = new SmoMetaConstraint.CollectionHelperI(base.Parent.Database, this, _SmoMetadataObject);
		indexCollection = new AbstractSmoMetaIndex.CollectionHelperI(base.Parent.Database, this, _SmoMetadataObject.Indexes);
		statisticsCollection = new Cmd.StatisticsCollectionHelperI(base.Parent.Database, this, _SmoMetadataObject.Statistics);
		triggerCollection = new SmoMetaDmlTrigger.CollectionHelperI(base.Parent.Database, this, _SmoMetadataObject.Triggers);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaTable
	// =========================================================================================================


	private readonly SmoMetaConstraint.CollectionHelperI constraintCollection;

	private readonly AbstractSmoMetaIndex.CollectionHelperI indexCollection;

	private readonly Cmd.StatisticsCollectionHelperI statisticsCollection;

	private readonly SmoMetaDmlTrigger.CollectionHelperI triggerCollection;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaTable
	// =========================================================================================================


	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => _SmoMetadataObject.IsSystemObject;

	public override TabularType TabularType => TabularType.Table;

	public override bool IsQuotedIdentifierOn => _SmoMetadataObject.QuotedIdentifierStatus;

	protected override SmoMetaConstraint.CollectionHelperI ConstraintCollection => constraintCollection;

	protected override AbstractSmoMetaIndex.CollectionHelperI IndexCollection => indexCollection;

	protected override Cmd.StatisticsCollectionHelperI StatisticsCollection => statisticsCollection;

	protected override SmoMetaDmlTrigger.CollectionHelperI TriggerCollection => triggerCollection;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaTable
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
