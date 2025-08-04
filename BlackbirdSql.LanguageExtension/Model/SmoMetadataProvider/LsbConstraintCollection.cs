// Microsoft.SqlServer.Management.SqlParser, Version=17.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlParser.MetadataProvider.ConstraintCollection

using Microsoft.SqlServer.Management.Smo;
using System.Collections.Generic;
using System.Data;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;

using CollationInfo = Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo;


namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbConstraintCollection Class
//
/// <summary>
/// Impersonation of an SQL Server Smo ConstraintCollection for providing metadata.
/// </summary>
// =========================================================================================================
public class LsbConstraintCollection : SortedListCollection<IConstraint>
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbConstraintCollection
	// ---------------------------------------------------------------------------------


	public LsbConstraintCollection(CollationInfo collationInfo)
		: this(4, collationInfo)
	{
	}

	public LsbConstraintCollection(int initialCapacity, CollationInfo collationInfo)
		: base(initialCapacity, collationInfo)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region									Nested types - LsbConstraintCollection
	// =========================================================================================================

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Utils.ConstraintCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal class CollectionHelperI : AbstractDatabaseObject.CollectionHelperBaseI<IConstraint, IMetadataCollection<IConstraint>>
	{
		public CollectionHelperI(LsbDatabase database, IDatabaseTable dbTable, Microsoft.SqlServer.Management.Smo.Table table)
		{
			this.database = database;
			this.dbTable = dbTable;
			checks = table.Checks;
			foreignKeys = table.ForeignKeys;
		}

		public CollectionHelperI(LsbDatabase database, IDatabaseTable dbTable, Microsoft.SqlServer.Management.Smo.View view)
		{
			this.database = database;
			this.dbTable = dbTable;
			checks = null;
			foreignKeys = null;
		}

		public CollectionHelperI(LsbDatabase database, IDatabaseTable dbTable, Microsoft.SqlServer.Management.Smo.UserDefinedFunction tableValuedFunction)
		{
			this.database = database;
			this.dbTable = dbTable;
			checks = tableValuedFunction.Checks;
			foreignKeys = null;
		}

		public CollectionHelperI(LsbDatabase database, IDatabaseTable dbTable, Microsoft.SqlServer.Management.Smo.UserDefinedTableType tableType)
		{
			this.database = database;
			this.dbTable = dbTable;
			checks = tableType.Checks;
			foreignKeys = null;
		}


		private readonly LsbDatabase database;

		private readonly CheckCollection checks;

		private readonly ForeignKeyCollection foreignKeys;

		private readonly IDatabaseTable dbTable;

		protected override LsbMetadataServer Server => database.Server;


		protected override IMetadataCollection<IConstraint> CreateMetadataCollection()
		{
			LsbMetadataServer parent = database.Parent;
			LsbConstraintCollection constraintCollection = new(database.CollationInfo);
			if (checks != null)
			{
				foreach (Check item in (IEnumerable<Check>)new AbstractDatabaseObject.SmoCollectionMetadataListI<Check>(parent, checks))
				{
					constraintCollection.Add(new LsbCheckConstraint(dbTable, item));
				}
			}
			if (foreignKeys != null)
			{
				AbstractDatabaseObject.SmoCollectionMetadataListI<ForeignKey> smoCollectionMetadataList = new(parent, foreignKeys);
				ITable table = dbTable as ITable;
				foreach (ForeignKey item2 in (IEnumerable<ForeignKey>)smoCollectionMetadataList)
				{
					constraintCollection.Add(new LsbForeignKeyConstraint(database, table, item2));
				}
			}
			foreach (IIndex index in dbTable.Indexes)
			{
				if (index is IRelationalIndex relationalIndex)
				{
					IConstraint indexKey = relationalIndex.IndexKey;
					if (indexKey != null)
					{
						constraintCollection.Add(indexKey);
					}
				}
			}
			return constraintCollection;
		}

		protected override IMetadataCollection<IConstraint> GetEmptyCollection()
		{
			return Collection<IConstraint>.Empty;
		}
	}


	#endregion Nested types

}
