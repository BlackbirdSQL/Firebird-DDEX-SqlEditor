// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Utils

using Microsoft.SqlServer.Management.Smo;
using System.Collections.Generic;
using System.Data;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;

using CollationInfo = Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


// =========================================================================================================
//
//											SmoMetaConstraint Class
//
/// <summary>
/// Smo MetadataProvider Constraint Helper.
/// </summary>
// =========================================================================================================
internal static class SmoMetaConstraint
{

	// ---------------------------------------------------------------------------------
	#region Methods - SmoMetaConstraint
	// ---------------------------------------------------------------------------------



	#endregion Methods





	// =========================================================================================================
	#region									Nested types - SmoMetaConstraint
	// =========================================================================================================

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Utils.ConstraintCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal class CollectionHelperI : AbstractSmoMetaDatabaseObjectBase.CollectionHelperBaseI<IConstraint, IMetadataCollection<IConstraint>>
	{
		public CollectionHelperI(SmoMetaDatabase database, IDatabaseTable dbTable, Microsoft.SqlServer.Management.Smo.Table table)
		{
			this.database = database;
			this.dbTable = dbTable;
			checks = table.Checks;
			foreignKeys = table.ForeignKeys;
		}

		public CollectionHelperI(SmoMetaDatabase database, IDatabaseTable dbTable, Microsoft.SqlServer.Management.Smo.View view)
		{
			this.database = database;
			this.dbTable = dbTable;
			checks = null;
			foreignKeys = null;
		}

		public CollectionHelperI(SmoMetaDatabase database, IDatabaseTable dbTable, Microsoft.SqlServer.Management.Smo.UserDefinedFunction tableValuedFunction)
		{
			this.database = database;
			this.dbTable = dbTable;
			checks = tableValuedFunction.Checks;
			foreignKeys = null;
		}

		public CollectionHelperI(SmoMetaDatabase database, IDatabaseTable dbTable, Microsoft.SqlServer.Management.Smo.UserDefinedTableType tableType)
		{
			this.database = database;
			this.dbTable = dbTable;
			checks = tableType.Checks;
			foreignKeys = null;
		}


		private readonly SmoMetaDatabase database;

		private readonly CheckCollection checks;

		private readonly ForeignKeyCollection foreignKeys;

		private readonly IDatabaseTable dbTable;

		protected override SmoMetaServer Server => database.Server;


		protected override IMetadataCollection<IConstraint> CreateMetadataCollection()
		{
			SmoMetaServer parent = database.Parent;
			// LsbSmoMetaHelper constraintCollection = new(database.CollationInfo);
			Microsoft.SqlServer.Management.SqlParser.MetadataProvider.ConstraintCollection constraintCollection = new(database.CollationInfo);

			if (checks != null)
			{
				foreach (Check item in (IEnumerable<Check>)new AbstractSmoMetaDatabaseObjectBase.SmoCollectionMetadataListI<Check>(parent, checks))
				{
					constraintCollection.Add(new SmoMetaCheckConstraint(dbTable, item));
				}
			}
			if (foreignKeys != null)
			{
				AbstractSmoMetaDatabaseObjectBase.SmoCollectionMetadataListI<ForeignKey> smoCollectionMetadataList = new(parent, foreignKeys);
				ITable table = dbTable as ITable;
				foreach (ForeignKey item2 in (IEnumerable<ForeignKey>)smoCollectionMetadataList)
				{
					constraintCollection.Add(new SmoMetaForeignKeyConstraint(database, table, item2));
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
