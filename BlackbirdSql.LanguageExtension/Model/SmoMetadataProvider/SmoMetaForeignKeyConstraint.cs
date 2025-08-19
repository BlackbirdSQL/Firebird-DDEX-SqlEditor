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
//											SmoMetaForeignKeyConstraint Class
//
/// <summary>
/// Impersonation of an SQL Server Smo ForeignKeyConstraint for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaForeignKeyConstraint : IForeignKeyConstraint, IConstraint, IMetadataObject, IBsSmoDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaForeignKeyConstraint
	// ---------------------------------------------------------------------------------


	public SmoMetaForeignKeyConstraint(SmoMetaDatabase database, ITable table, ForeignKey smoForeignKey)
	{
		_Table = table;
		_SmoForeignKey = smoForeignKey;
		columnCollection = new ForeignKeyColumnCollectionHelperI(database, table, ReferencedTable, smoForeignKey.Columns);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaForeignKeyConstraint
	// =========================================================================================================


	private readonly ITable _Table;

	private readonly ForeignKey _SmoForeignKey;

	private readonly ForeignKeyColumnCollectionHelperI columnCollection;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaForeignKeyConstraint
	// =========================================================================================================


	public ITabular Parent => _Table;

	public bool IsSystemNamed => _SmoForeignKey.IsSystemNamed;

	public ConstraintType Type => ConstraintType.ForeignKey;

	public IMetadataOrderedCollection<IForeignKeyColumn> Columns => columnCollection.MetadataCollection;

	public Microsoft.SqlServer.Management.SqlParser.Metadata.ForeignKeyAction DeleteAction => ConvertSmoForeignKeyAction(_SmoForeignKey.DeleteAction);

	public bool IsEnabled => _SmoForeignKey.IsEnabled;

	public bool IsChecked => _SmoForeignKey.IsChecked;

	public bool NotForReplication => _SmoForeignKey.NotForReplication;

	public ITable ReferencedTable => _Table.Schema.Database.Schemas[_SmoForeignKey.ReferencedTableSchema].Tables[_SmoForeignKey.ReferencedTable];

	public Microsoft.SqlServer.Management.SqlParser.Metadata.ForeignKeyAction UpdateAction => ConvertSmoForeignKeyAction(_SmoForeignKey.UpdateAction);

	public string Name => _SmoForeignKey.Name;

	public SqlSmoObject SmoObject => _SmoForeignKey;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaForeignKeyConstraint
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
	#region									Nested types - SmoMetaForeignKeyConstraint
	// =========================================================================================================

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Utils.ForeignKeyColumnCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class ForeignKeyColumnCollectionHelperI : Cmd.OrderedCollectionHelperI<IForeignKeyColumn, ForeignKeyColumn>
	{
		public ForeignKeyColumnCollectionHelperI(SmoMetaDatabase database, ITable table, ITable refTable, Microsoft.SqlServer.Management.Smo.ForeignKeyColumnCollection smoCollection)
			: base(database)
		{
			this.table = table;
			this.refTable = refTable;
			this.smoCollection = smoCollection;
		}



		private readonly Microsoft.SqlServer.Management.Smo.ForeignKeyColumnCollection smoCollection;

		private readonly ITable table;

		private readonly ITable refTable;


		protected override AbstractSmoMetaDatabaseObjectBase.IMetadataListI<ForeignKeyColumn> RetrieveSmoMetadataList()
		{
			return new AbstractSmoMetaDatabaseObjectBase.SmoCollectionMetadataListI<ForeignKeyColumn>(_Database.Server, smoCollection);
		}

		protected override IForeignKeyColumn CreateMetadataObject(ForeignKeyColumn smoObject)
		{
			IConstraintFactory constraint = SmoMetaSmoMetadataFactory.Instance.Constraint;
			IMetadataOrderedCollection<IColumn> columns = table.Columns;
			IMetadataCollection<IColumn> columns2 = refTable.Columns;
			IColumn referencingColumn = columns[smoObject.Name];
			IColumn referencedColumn = columns2[smoObject.ReferencedColumn];
			return constraint.CreateForeignKeyColumn(referencingColumn, referencedColumn);
		}
	}

	#endregion Nested types

}
