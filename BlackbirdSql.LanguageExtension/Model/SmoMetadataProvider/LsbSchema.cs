// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Schema

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using static Microsoft.SqlServer.Management.SqlParser.MetadataProvider.MetadataProviderUtils.Names;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbSchema Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Schema for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbSchema : LsbDatabaseOwnedObject<Microsoft.SqlServer.Management.Smo.Schema>, ISchema, IDatabaseOwnedObject, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbSchema
	// ---------------------------------------------------------------------------------


	public LsbSchema(Microsoft.SqlServer.Management.Smo.Schema smoMetadataObject, LsbDatabase parent)
		: base(smoMetadataObject, parent)
	{
		m_extendedStoredProcedures = new ExtendedStoredProcedureCollectionHelperI(this);
		m_scalarValuedFunctions = new ScalarValuedFunctionCollectionHelperI(this);
		m_storedProcedures = new StoredProcedureCollectionHelperI(this);
		m_synonyms = new SynonymCollectionHelperI(this);
		m_tables = new TableCollectionHelperI(this);
		m_tableValuedFunctions = new TableValuedFunctionCollectionHelperI(this);
		m_userDefinedAggregates = new UserDefinedAggregateCollectionHelperI(this);
		m_userDefinedClrTypes = new UserDefinedClrTypeCollectionHelperI(this);
		m_userDefinedDataTypes = new UserDefinedDataTypeCollectionHelperI(this);
		m_userDefinedTableTypes = new UserDefinedTableTypeCollectionHelperI(this);
		m_views = new ViewCollectionHelperI(this);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbSchema
	// =========================================================================================================


	private readonly ExtendedStoredProcedureCollectionHelperI m_extendedStoredProcedures;

	private readonly ScalarValuedFunctionCollectionHelperI m_scalarValuedFunctions;

	private readonly StoredProcedureCollectionHelperI m_storedProcedures;

	private readonly SynonymCollectionHelperI m_synonyms;

	private readonly TableCollectionHelperI m_tables;

	private readonly TableValuedFunctionCollectionHelperI m_tableValuedFunctions;

	private readonly UserDefinedAggregateCollectionHelperI m_userDefinedAggregates;

	private readonly UserDefinedClrTypeCollectionHelperI m_userDefinedClrTypes;

	private readonly UserDefinedDataTypeCollectionHelperI m_userDefinedDataTypes;

	private readonly UserDefinedTableTypeCollectionHelperI m_userDefinedTableTypes;

	private readonly ViewCollectionHelperI m_views;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbSchema
	// =========================================================================================================


	public override int Id => m_smoMetadataObject.ID;

	public override bool IsSystemObject
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)m_smoMetadataObject, "IsSystemObject", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsSysSchema => m_parent.SysSchema == this;

	public IDatabasePrincipal Owner
	{
		get
		{
			Cmd.TryGetPropertyObject<string>(m_smoMetadataObject, "Owner", out var value);
			if (value == null)
			{
				return null;
			}
			return Cmd.GetDatabasePrincipal(base.Parent, value);
		}
	}

	public IMetadataCollection<ITable> Tables => m_tables.MetadataCollection;

	public IMetadataCollection<IView> Views => m_views.MetadataCollection;

	public IMetadataCollection<IUserDefinedAggregate> UserDefinedAggregates => m_userDefinedAggregates.MetadataCollection;

	public IMetadataCollection<ITableValuedFunction> TableValuedFunctions => m_tableValuedFunctions.MetadataCollection;

	public IMetadataCollection<IScalarValuedFunction> ScalarValuedFunctions => m_scalarValuedFunctions.MetadataCollection;

	public IMetadataCollection<IStoredProcedure> StoredProcedures => m_storedProcedures.MetadataCollection;

	public IMetadataCollection<ISynonym> Synonyms => m_synonyms.MetadataCollection;

	public IMetadataCollection<IExtendedStoredProcedure> ExtendedStoredProcedures => m_extendedStoredProcedures.MetadataCollection;

	public IMetadataCollection<IUserDefinedDataType> UserDefinedDataTypes => m_userDefinedDataTypes.MetadataCollection;

	public IMetadataCollection<IUserDefinedTableType> UserDefinedTableTypes => m_userDefinedTableTypes.MetadataCollection;

	public IMetadataCollection<IUserDefinedClrType> UserDefinedClrTypes => m_userDefinedClrTypes.MetadataCollection;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbSchema
	// =========================================================================================================


	public override T Accept<T>(IDatabaseOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - LsbSchema
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Database.SchemaCollectionHelper<T, S>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : LsbDatabase.CollectionHelperI<ISchema, Microsoft.SqlServer.Management.Smo.Schema>
	{
		public CollectionHelperI(LsbDatabase database)
			: base(database)
		{
		}
		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.Schema> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<Microsoft.SqlServer.Management.Smo.Schema>(m_database.Server, m_database.SmoMetadataObject.Schemas);
		}

		protected override IMutableMetadataCollection<ISchema> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.SchemaCollection(initialCapacity, collationInfo);
		}

		protected override ISchema CreateMetadataObject(Microsoft.SqlServer.Management.Smo.Schema smoObject)
		{
			return new LsbSchema(smoObject, m_database);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: CollectionHelper<T, S>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private abstract class CollectionHelperI<T, S> : UnorderedCollectionHelperBaseI<T, S> where T : class, ISchemaOwnedObject where S : ScriptSchemaObjectBase
	{
		protected readonly LsbSchema m_schema;

		protected override LsbMetadataServer Server => m_schema.Database.Server;

		public CollectionHelperI(LsbSchema schema)
		{
			m_schema = schema;
		}

		protected override Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo GetCollationInfo()
		{
			return m_schema.Database.CollationInfo;
		}
	}

	private sealed class ExtendedStoredProcedureCollectionHelperI : CollectionHelperI<IExtendedStoredProcedure, Microsoft.SqlServer.Management.Smo.ExtendedStoredProcedure>
	{
		public ExtendedStoredProcedureCollectionHelperI(LsbSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.ExtendedStoredProcedure> RetrieveSmoMetadataList()
		{
			return m_schema.m_parent.MetadataExtendedStoredProcedures[m_schema.Name];
		}

		protected override IMutableMetadataCollection<IExtendedStoredProcedure> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.ExtendedStoredProcedureCollection(initialCapacity, collationInfo);
		}

		protected override IExtendedStoredProcedure CreateMetadataObject(Microsoft.SqlServer.Management.Smo.ExtendedStoredProcedure smoObject)
		{
			return new LsbExtendedStoredProcedure(smoObject, m_schema);
		}
	}

	private sealed class ScalarValuedFunctionCollectionHelperI : CollectionHelperI<IScalarValuedFunction, Microsoft.SqlServer.Management.Smo.UserDefinedFunction>
	{
		public ScalarValuedFunctionCollectionHelperI(LsbSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.UserDefinedFunction> RetrieveSmoMetadataList()
		{
			return m_schema.m_parent.MetadataUdfs[m_schema.Name, IsScalarValuedFunction];
		}

		protected override IMutableMetadataCollection<IScalarValuedFunction> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new ScalarValuedFunctionCollection(initialCapacity, collationInfo);
		}

		protected override IScalarValuedFunction CreateMetadataObject(Microsoft.SqlServer.Management.Smo.UserDefinedFunction smoObject)
		{
			return new LsbScalarValuedFunction(smoObject, m_schema);
		}

		private static bool IsScalarValuedFunction(Microsoft.SqlServer.Management.Smo.UserDefinedFunction function)
		{
			return function.FunctionType == UserDefinedFunctionType.Scalar;
		}
	}

	private sealed class StoredProcedureCollectionHelperI : CollectionHelperI<IStoredProcedure, Microsoft.SqlServer.Management.Smo.StoredProcedure>
	{
		public StoredProcedureCollectionHelperI(LsbSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.StoredProcedure> RetrieveSmoMetadataList()
		{
			return m_schema.m_parent.MetadataStoredProcedures[m_schema.Name];
		}

		protected override IMutableMetadataCollection<IStoredProcedure> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.StoredProcedureCollection(initialCapacity, collationInfo);
		}

		protected override IStoredProcedure CreateMetadataObject(Microsoft.SqlServer.Management.Smo.StoredProcedure smoObject)
		{
			return new LsbStoredProcedure(smoObject, m_schema);
		}
	}

	private sealed class SynonymCollectionHelperI : CollectionHelperI<ISynonym, Microsoft.SqlServer.Management.Smo.Synonym>
	{
		public SynonymCollectionHelperI(LsbSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.Synonym> RetrieveSmoMetadataList()
		{
			return m_schema.m_parent.MetadataSynonyms[m_schema.Name];
		}

		protected override IMutableMetadataCollection<ISynonym> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.SynonymCollection(initialCapacity, collationInfo);
		}

		protected override ISynonym CreateMetadataObject(Microsoft.SqlServer.Management.Smo.Synonym smoObject)
		{
			return new LsbSynonym(smoObject, m_schema);
		}
	}

	private sealed class TableCollectionHelperI : CollectionHelperI<ITable, Microsoft.SqlServer.Management.Smo.Table>
	{
		public TableCollectionHelperI(LsbSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.Table> RetrieveSmoMetadataList()
		{
			return m_schema.m_parent.MetadataTables[m_schema.Name];
		}

		protected override IMutableMetadataCollection<ITable> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.TableCollection(initialCapacity, collationInfo);
		}

		protected override ITable CreateMetadataObject(Microsoft.SqlServer.Management.Smo.Table smoObject)
		{
			return new LsbTable(smoObject, m_schema);
		}
	}

	private sealed class TableValuedFunctionCollectionHelperI : CollectionHelperI<ITableValuedFunction, Microsoft.SqlServer.Management.Smo.UserDefinedFunction>
	{
		public TableValuedFunctionCollectionHelperI(LsbSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.UserDefinedFunction> RetrieveSmoMetadataList()
		{
			return m_schema.m_parent.MetadataUdfs[m_schema.Name, IsTableValuedFunction];
		}

		protected override IMutableMetadataCollection<ITableValuedFunction> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new TableValuedFunctionCollection(initialCapacity, collationInfo);
		}

		protected override ITableValuedFunction CreateMetadataObject(Microsoft.SqlServer.Management.Smo.UserDefinedFunction smoObject)
		{
			return new TableValuedFunction(smoObject, m_schema);
		}

		private static bool IsTableValuedFunction(Microsoft.SqlServer.Management.Smo.UserDefinedFunction function)
		{
			if (function.FunctionType != UserDefinedFunctionType.Inline)
			{
				return function.FunctionType == UserDefinedFunctionType.Table;
			}
			return true;
		}
	}

	private sealed class UserDefinedAggregateCollectionHelperI : CollectionHelperI<IUserDefinedAggregate, Microsoft.SqlServer.Management.Smo.UserDefinedAggregate>
	{
		public UserDefinedAggregateCollectionHelperI(LsbSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.UserDefinedAggregate> RetrieveSmoMetadataList()
		{
			return m_schema.m_parent.MetadataUserDefinedAggregates[m_schema.Name];
		}

		protected override IMutableMetadataCollection<IUserDefinedAggregate> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.UserDefinedAggregateCollection(initialCapacity, collationInfo);
		}

		protected override IUserDefinedAggregate CreateMetadataObject(Microsoft.SqlServer.Management.Smo.UserDefinedAggregate smoObject)
		{
			return new Microsoft.SqlServer.Management.SmoMetadataProvider.UserDefinedAggregate(smoObject, m_schema);
		}
	}

	private sealed class UserDefinedClrTypeCollectionHelperI : CollectionHelperI<IUserDefinedClrType, UserDefinedType>
	{
		public UserDefinedClrTypeCollectionHelperI(LsbSchema schema)
			: base(schema)
		{
		}

		protected override IMetadataListI<UserDefinedType> RetrieveSmoMetadataList()
		{
			return m_schema.m_parent.MetadataUserDefinedClrTypes[m_schema.Name];
		}

		protected override IMutableMetadataCollection<IUserDefinedClrType> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new UserDefinedClrTypeCollection(initialCapacity, collationInfo);
		}

		protected override IUserDefinedClrType CreateMetadataObject(UserDefinedType smoObject)
		{
			return new UserDefinedClrType(smoObject, m_schema);
		}
	}

	private sealed class UserDefinedDataTypeCollectionHelperI : CollectionHelperI<IUserDefinedDataType, Microsoft.SqlServer.Management.Smo.UserDefinedDataType>
	{
		public UserDefinedDataTypeCollectionHelperI(LsbSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.UserDefinedDataType> RetrieveSmoMetadataList()
		{
			return m_schema.m_parent.MetadataUserDefinedDataTypes[m_schema.Name];
		}

		protected override IMutableMetadataCollection<IUserDefinedDataType> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.UserDefinedDataTypeCollection(initialCapacity, collationInfo);
		}

		protected override IUserDefinedDataType CreateMetadataObject(Microsoft.SqlServer.Management.Smo.UserDefinedDataType smoObject)
		{
			return new Microsoft.SqlServer.Management.SmoMetadataProvider.UserDefinedDataType(smoObject, m_schema);
		}
	}

	private sealed class UserDefinedTableTypeCollectionHelperI : CollectionHelperI<IUserDefinedTableType, Microsoft.SqlServer.Management.Smo.UserDefinedTableType>
	{
		public UserDefinedTableTypeCollectionHelperI(LsbSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.UserDefinedTableType> RetrieveSmoMetadataList()
		{
			return m_schema.m_parent.MetadataUserDefinedTableTypes[m_schema.Name];
		}

		protected override IMutableMetadataCollection<IUserDefinedTableType> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.UserDefinedTableTypeCollection(initialCapacity, collationInfo);
		}

		protected override IUserDefinedTableType CreateMetadataObject(Microsoft.SqlServer.Management.Smo.UserDefinedTableType smoObject)
		{
			return new Microsoft.SqlServer.Management.SmoMetadataProvider.UserDefinedTableType(smoObject, m_schema);
		}
	}

	private sealed class ViewCollectionHelperI : CollectionHelperI<IView, Microsoft.SqlServer.Management.Smo.View>
	{
		public ViewCollectionHelperI(LsbSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.View> RetrieveSmoMetadataList()
		{
			return m_schema.m_parent.MetadataViews[m_schema.Name];
		}

		protected override IMutableMetadataCollection<IView> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.ViewCollection(initialCapacity, collationInfo);
		}

		protected override IView CreateMetadataObject(Microsoft.SqlServer.Management.Smo.View smoObject)
		{
			return new Microsoft.SqlServer.Management.SmoMetadataProvider.View(smoObject, m_schema);
		}
	}


	#endregion Nested types

}
