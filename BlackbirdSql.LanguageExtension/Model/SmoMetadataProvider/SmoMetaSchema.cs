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
//											SmoMetaSchema Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Schema for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaSchema : AbstractSmoMetaDatabaseOwnedObject<Microsoft.SqlServer.Management.Smo.Schema>, ISchema, IDatabaseOwnedObject, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaSchema
	// ---------------------------------------------------------------------------------


	public SmoMetaSchema(Microsoft.SqlServer.Management.Smo.Schema smoMetadataObject, SmoMetaDatabase parent)
		: base(smoMetadataObject, parent)
	{
		_ExtendedStoredProcedures = new ExtendedStoredProcedureCollectionHelperI(this);
		_ScalarValuedFunctions = new ScalarValuedFunctionCollectionHelperI(this);
		_StoredProcedures = new StoredProcedureCollectionHelperI(this);
		_Synonyms = new SynonymCollectionHelperI(this);
		_Tables = new TableCollectionHelperI(this);
		_TableValuedFunctions = new TableValuedFunctionCollectionHelperI(this);
		_UserDefinedAggregates = new UserDefinedAggregateCollectionHelperI(this);
		_UserDefinedClrTypes = new UserDefinedClrTypeCollectionHelperI(this);
		_UserDefinedDataTypes = new UserDefinedDataTypeCollectionHelperI(this);
		_UserDefinedTableTypes = new UserDefinedTableTypeCollectionHelperI(this);
		_Views = new ViewCollectionHelperI(this);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaSchema
	// =========================================================================================================


	private readonly ExtendedStoredProcedureCollectionHelperI _ExtendedStoredProcedures;

	private readonly ScalarValuedFunctionCollectionHelperI _ScalarValuedFunctions;

	private readonly StoredProcedureCollectionHelperI _StoredProcedures;

	private readonly SynonymCollectionHelperI _Synonyms;

	private readonly TableCollectionHelperI _Tables;

	private readonly TableValuedFunctionCollectionHelperI _TableValuedFunctions;

	private readonly UserDefinedAggregateCollectionHelperI _UserDefinedAggregates;

	private readonly UserDefinedClrTypeCollectionHelperI _UserDefinedClrTypes;

	private readonly UserDefinedDataTypeCollectionHelperI _UserDefinedDataTypes;

	private readonly UserDefinedTableTypeCollectionHelperI _UserDefinedTableTypes;

	private readonly ViewCollectionHelperI _Views;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaSchema
	// =========================================================================================================


	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoMetadataObject, "IsSystemObject", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsSysSchema => _Parent.SysSchema == this;

	public IDatabasePrincipal Owner
	{
		get
		{
			Cmd.TryGetPropertyObject<string>(_SmoMetadataObject, "Owner", out var value);
			if (value == null)
			{
				return null;
			}
			return Cmd.GetDatabasePrincipal(base.Parent, value);
		}
	}

	public IMetadataCollection<ITable> Tables => _Tables.MetadataCollection;

	public IMetadataCollection<IView> Views => _Views.MetadataCollection;

	public IMetadataCollection<IUserDefinedAggregate> UserDefinedAggregates => _UserDefinedAggregates.MetadataCollection;

	public IMetadataCollection<ITableValuedFunction> TableValuedFunctions => _TableValuedFunctions.MetadataCollection;

	public IMetadataCollection<IScalarValuedFunction> ScalarValuedFunctions => _ScalarValuedFunctions.MetadataCollection;

	public IMetadataCollection<IStoredProcedure> StoredProcedures => _StoredProcedures.MetadataCollection;

	public IMetadataCollection<ISynonym> Synonyms => _Synonyms.MetadataCollection;

	public IMetadataCollection<IExtendedStoredProcedure> ExtendedStoredProcedures => _ExtendedStoredProcedures.MetadataCollection;

	public IMetadataCollection<IUserDefinedDataType> UserDefinedDataTypes => _UserDefinedDataTypes.MetadataCollection;

	public IMetadataCollection<IUserDefinedTableType> UserDefinedTableTypes => _UserDefinedTableTypes.MetadataCollection;

	public IMetadataCollection<IUserDefinedClrType> UserDefinedClrTypes => _UserDefinedClrTypes.MetadataCollection;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaSchema
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
	#region									Nested types - SmoMetaSchema
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Database.SchemaCollectionHelper<T, S>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : SmoMetaDatabase.CollectionHelperI<ISchema, Microsoft.SqlServer.Management.Smo.Schema>
	{
		public CollectionHelperI(SmoMetaDatabase database)
			: base(database)
		{
		}
		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.Schema> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<Microsoft.SqlServer.Management.Smo.Schema>(_Database.Server, _Database.SmoMetadataObject.Schemas);
		}

		protected override IMutableMetadataCollection<ISchema> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.SchemaCollection(initialCapacity, collationInfo);
		}

		protected override ISchema CreateMetadataObject(Microsoft.SqlServer.Management.Smo.Schema smoObject)
		{
			return new SmoMetaSchema(smoObject, _Database);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: CollectionHelper<T, S>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private abstract class CollectionHelperI<T, S> : UnorderedCollectionHelperBaseI<T, S> where T : class, ISchemaOwnedObject where S : ScriptSchemaObjectBase
	{
		protected readonly SmoMetaSchema _Schema;

		protected override SmoMetaServer Server => _Schema.Database.Server;

		public CollectionHelperI(SmoMetaSchema schema)
		{
			_Schema = schema;
		}

		protected override Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo GetCollationInfo()
		{
			return _Schema.Database.CollationInfo;
		}
	}

	private sealed class ExtendedStoredProcedureCollectionHelperI : CollectionHelperI<IExtendedStoredProcedure, Microsoft.SqlServer.Management.Smo.ExtendedStoredProcedure>
	{
		public ExtendedStoredProcedureCollectionHelperI(SmoMetaSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.ExtendedStoredProcedure> RetrieveSmoMetadataList()
		{
			return _Schema._Parent.MetadataExtendedStoredProcedures[_Schema.Name];
		}

		protected override IMutableMetadataCollection<IExtendedStoredProcedure> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.ExtendedStoredProcedureCollection(initialCapacity, collationInfo);
		}

		protected override IExtendedStoredProcedure CreateMetadataObject(Microsoft.SqlServer.Management.Smo.ExtendedStoredProcedure smoObject)
		{
			return new SmoMetaExtendedStoredProcedure(smoObject, _Schema);
		}
	}

	private sealed class ScalarValuedFunctionCollectionHelperI : CollectionHelperI<IScalarValuedFunction, Microsoft.SqlServer.Management.Smo.UserDefinedFunction>
	{
		public ScalarValuedFunctionCollectionHelperI(SmoMetaSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.UserDefinedFunction> RetrieveSmoMetadataList()
		{
			return _Schema._Parent.MetadataUdfs[_Schema.Name, IsScalarValuedFunction];
		}

		protected override IMutableMetadataCollection<IScalarValuedFunction> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new ScalarValuedFunctionCollection(initialCapacity, collationInfo);
		}

		protected override IScalarValuedFunction CreateMetadataObject(Microsoft.SqlServer.Management.Smo.UserDefinedFunction smoObject)
		{
			return new SmoMetaScalarValuedFunction(smoObject, _Schema);
		}

		private static bool IsScalarValuedFunction(Microsoft.SqlServer.Management.Smo.UserDefinedFunction function)
		{
			return function.FunctionType == UserDefinedFunctionType.Scalar;
		}
	}

	private sealed class StoredProcedureCollectionHelperI : CollectionHelperI<IStoredProcedure, Microsoft.SqlServer.Management.Smo.StoredProcedure>
	{
		public StoredProcedureCollectionHelperI(SmoMetaSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.StoredProcedure> RetrieveSmoMetadataList()
		{
			return _Schema._Parent.MetadataStoredProcedures[_Schema.Name];
		}

		protected override IMutableMetadataCollection<IStoredProcedure> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.StoredProcedureCollection(initialCapacity, collationInfo);
		}

		protected override IStoredProcedure CreateMetadataObject(Microsoft.SqlServer.Management.Smo.StoredProcedure smoObject)
		{
			return new SmoMetaStoredProcedure(smoObject, _Schema);
		}
	}

	private sealed class SynonymCollectionHelperI : CollectionHelperI<ISynonym, Microsoft.SqlServer.Management.Smo.Synonym>
	{
		public SynonymCollectionHelperI(SmoMetaSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.Synonym> RetrieveSmoMetadataList()
		{
			return _Schema._Parent.MetadataSynonyms[_Schema.Name];
		}

		protected override IMutableMetadataCollection<ISynonym> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.SynonymCollection(initialCapacity, collationInfo);
		}

		protected override ISynonym CreateMetadataObject(Microsoft.SqlServer.Management.Smo.Synonym smoObject)
		{
			return new SmoMetaSynonym(smoObject, _Schema);
		}
	}

	private sealed class TableCollectionHelperI : CollectionHelperI<ITable, Microsoft.SqlServer.Management.Smo.Table>
	{
		public TableCollectionHelperI(SmoMetaSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.Table> RetrieveSmoMetadataList()
		{
			return _Schema._Parent.MetadataTables[_Schema.Name];
		}

		protected override IMutableMetadataCollection<ITable> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.TableCollection(initialCapacity, collationInfo);
		}

		protected override ITable CreateMetadataObject(Microsoft.SqlServer.Management.Smo.Table smoObject)
		{
			return new SmoMetaTable(smoObject, _Schema);
		}
	}

	private sealed class TableValuedFunctionCollectionHelperI : CollectionHelperI<ITableValuedFunction, Microsoft.SqlServer.Management.Smo.UserDefinedFunction>
	{
		public TableValuedFunctionCollectionHelperI(SmoMetaSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.UserDefinedFunction> RetrieveSmoMetadataList()
		{
			return _Schema._Parent.MetadataUdfs[_Schema.Name, IsTableValuedFunction];
		}

		protected override IMutableMetadataCollection<ITableValuedFunction> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new TableValuedFunctionCollection(initialCapacity, collationInfo);
		}

		protected override ITableValuedFunction CreateMetadataObject(Microsoft.SqlServer.Management.Smo.UserDefinedFunction smoObject)
		{
			return new SmoMetaTableValuedFunction(smoObject, _Schema);
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
		public UserDefinedAggregateCollectionHelperI(SmoMetaSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.UserDefinedAggregate> RetrieveSmoMetadataList()
		{
			return _Schema._Parent.MetadataUserDefinedAggregates[_Schema.Name];
		}

		protected override IMutableMetadataCollection<IUserDefinedAggregate> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.UserDefinedAggregateCollection(initialCapacity, collationInfo);
		}

		protected override IUserDefinedAggregate CreateMetadataObject(Microsoft.SqlServer.Management.Smo.UserDefinedAggregate smoObject)
		{
			return new SmoMetaUserDefinedAggregate(smoObject, _Schema);
		}
	}

	private sealed class UserDefinedClrTypeCollectionHelperI : CollectionHelperI<IUserDefinedClrType, UserDefinedType>
	{
		public UserDefinedClrTypeCollectionHelperI(SmoMetaSchema schema)
			: base(schema)
		{
		}

		protected override IMetadataListI<UserDefinedType> RetrieveSmoMetadataList()
		{
			return _Schema._Parent.MetadataUserDefinedClrTypes[_Schema.Name];
		}

		protected override IMutableMetadataCollection<IUserDefinedClrType> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new UserDefinedClrTypeCollection(initialCapacity, collationInfo);
		}

		protected override IUserDefinedClrType CreateMetadataObject(UserDefinedType smoObject)
		{
			return new SmoMetaUserDefinedClrType(smoObject, _Schema);
		}
	}

	private sealed class UserDefinedDataTypeCollectionHelperI : CollectionHelperI<IUserDefinedDataType, Microsoft.SqlServer.Management.Smo.UserDefinedDataType>
	{
		public UserDefinedDataTypeCollectionHelperI(SmoMetaSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.UserDefinedDataType> RetrieveSmoMetadataList()
		{
			return _Schema._Parent.MetadataUserDefinedDataTypes[_Schema.Name];
		}

		protected override IMutableMetadataCollection<IUserDefinedDataType> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.UserDefinedDataTypeCollection(initialCapacity, collationInfo);
		}

		protected override IUserDefinedDataType CreateMetadataObject(Microsoft.SqlServer.Management.Smo.UserDefinedDataType smoObject)
		{
			return new SmoMetaUserDefinedDataType(smoObject, _Schema);
		}
	}

	private sealed class UserDefinedTableTypeCollectionHelperI : CollectionHelperI<IUserDefinedTableType, Microsoft.SqlServer.Management.Smo.UserDefinedTableType>
	{
		public UserDefinedTableTypeCollectionHelperI(SmoMetaSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.UserDefinedTableType> RetrieveSmoMetadataList()
		{
			return _Schema._Parent.MetadataUserDefinedTableTypes[_Schema.Name];
		}

		protected override IMutableMetadataCollection<IUserDefinedTableType> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.UserDefinedTableTypeCollection(initialCapacity, collationInfo);
		}

		protected override IUserDefinedTableType CreateMetadataObject(Microsoft.SqlServer.Management.Smo.UserDefinedTableType smoObject)
		{
			return new SmoMetaUserDefinedTableType(smoObject, _Schema);
		}
	}

	private sealed class ViewCollectionHelperI : CollectionHelperI<IView, Microsoft.SqlServer.Management.Smo.View>
	{
		public ViewCollectionHelperI(SmoMetaSchema schema)
		: base(schema)
		{
		}

		protected override IMetadataListI<Microsoft.SqlServer.Management.Smo.View> RetrieveSmoMetadataList()
		{
			return _Schema._Parent.MetadataViews[_Schema.Name];
		}

		protected override IMutableMetadataCollection<IView> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.ViewCollection(initialCapacity, collationInfo);
		}

		protected override IView CreateMetadataObject(Microsoft.SqlServer.Management.Smo.View smoObject)
		{
			return new SmoMetaView(smoObject, _Schema);
		}
	}


	#endregion Nested types

}
