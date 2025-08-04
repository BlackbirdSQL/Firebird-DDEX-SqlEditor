// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.ForeignKeyConstraint

using System;
using BlackbirdSql.LanguageExtension.Interfaces;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbForeignKeyConstraint Class
//
/// <summary>
/// Impersonation of an SQL Server Smo ForeignKeyConstraint for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbForeignKeyConstraint : IForeignKeyConstraint, IConstraint, IMetadataObject, IBsSmoDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbForeignKeyConstraint
	// ---------------------------------------------------------------------------------


	public LsbForeignKeyConstraint(LsbDatabase database, ITable table, ForeignKey smoForeignKey)
	{
		m_table = table;
		m_smoForeignKey = smoForeignKey;
		columnCollection = new ForeignKeyColumnCollectionHelperI(database, table, ReferencedTable, smoForeignKey.Columns);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbForeignKeyConstraint
	// =========================================================================================================


	private readonly ITable m_table;

	private readonly ForeignKey m_smoForeignKey;

	private readonly ForeignKeyColumnCollectionHelperI columnCollection;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbForeignKeyConstraint
	// =========================================================================================================


	public ITabular Parent => m_table;

	public bool IsSystemNamed => m_smoForeignKey.IsSystemNamed;

	public ConstraintType Type => ConstraintType.ForeignKey;

	public IMetadataOrderedCollection<IForeignKeyColumn> Columns => columnCollection.MetadataCollection;

	public Microsoft.SqlServer.Management.SqlParser.Metadata.ForeignKeyAction DeleteAction => ConvertSmoForeignKeyAction(m_smoForeignKey.DeleteAction);

	public bool IsEnabled => m_smoForeignKey.IsEnabled;

	public bool IsChecked => m_smoForeignKey.IsChecked;

	public bool NotForReplication => m_smoForeignKey.NotForReplication;

	public ITable ReferencedTable => m_table.Schema.Database.Schemas[m_smoForeignKey.ReferencedTableSchema].Tables[m_smoForeignKey.ReferencedTable];

	public Microsoft.SqlServer.Management.SqlParser.Metadata.ForeignKeyAction UpdateAction => ConvertSmoForeignKeyAction(m_smoForeignKey.UpdateAction);

	public string Name => m_smoForeignKey.Name;

	public SqlSmoObject SmoObject => m_smoForeignKey;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbForeignKeyConstraint
	// =========================================================================================================


	public T Accept<T>(IMetadataObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}

	private Microsoft.SqlServer.Management.SqlParser.Metadata.ForeignKeyAction ConvertSmoForeignKeyAction(Microsoft.SqlServer.Management.Smo.ForeignKeyAction smoForeignKeyAction)
	{
		return smoForeignKeyAction switch
		{
			Microsoft.SqlServer.Management.Smo.ForeignKeyAction.Cascade => Microsoft.SqlServer.Management.SqlParser.Metadata.ForeignKeyAction.Cascade, 
			Microsoft.SqlServer.Management.Smo.ForeignKeyAction.NoAction => Microsoft.SqlServer.Management.SqlParser.Metadata.ForeignKeyAction.NoAction, 
			Microsoft.SqlServer.Management.Smo.ForeignKeyAction.SetDefault => Microsoft.SqlServer.Management.SqlParser.Metadata.ForeignKeyAction.SetDefault, 
			Microsoft.SqlServer.Management.Smo.ForeignKeyAction.SetNull => Microsoft.SqlServer.Management.SqlParser.Metadata.ForeignKeyAction.SetNull, 
			_ => Microsoft.SqlServer.Management.SqlParser.Metadata.ForeignKeyAction.NoAction, 
		};
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - LsbForeignKeyConstraint
	// =========================================================================================================

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Utils.ForeignKeyColumnCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class ForeignKeyColumnCollectionHelperI : Cmd.OrderedCollectionHelperI<IForeignKeyColumn, ForeignKeyColumn>
	{
		public ForeignKeyColumnCollectionHelperI(LsbDatabase database, ITable table, ITable refTable, Microsoft.SqlServer.Management.Smo.ForeignKeyColumnCollection smoCollection)
			: base(database)
		{
			this.table = table;
			this.refTable = refTable;
			this.smoCollection = smoCollection;
		}



		private readonly Microsoft.SqlServer.Management.Smo.ForeignKeyColumnCollection smoCollection;

		private readonly ITable table;

		private readonly ITable refTable;


		protected override AbstractDatabaseObject.IMetadataListI<ForeignKeyColumn> RetrieveSmoMetadataList()
		{
			return new AbstractDatabaseObject.SmoCollectionMetadataListI<ForeignKeyColumn>(m_database.Server, smoCollection);
		}

		protected override IForeignKeyColumn CreateMetadataObject(ForeignKeyColumn smoObject)
		{
			IConstraintFactory constraint = LsbMetadataFactory.Instance.Constraint;
			IMetadataOrderedCollection<IColumn> columns = table.Columns;
			IMetadataCollection<IColumn> columns2 = refTable.Columns;
			IColumn referencingColumn = columns[smoObject.Name];
			IColumn referencedColumn = columns2[smoObject.ReferencedColumn];
			return constraint.CreateForeignKeyColumn(referencingColumn, referencedColumn);
		}
	}

	#endregion Nested types

}
