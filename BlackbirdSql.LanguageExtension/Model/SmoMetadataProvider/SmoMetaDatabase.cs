// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Database

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BlackbirdSql.LanguageExtension.Ctl.Config;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Common;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Parser;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											SmoMetaDatabase Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Database for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaDatabase : AbstractSmoMetaServerOwnedObject<Database>, IDatabase, IServerOwnedObject, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaDatabase
	// ---------------------------------------------------------------------------------


	public SmoMetaDatabase(Database smoMetadataObject, SmoMetaServer parent)
		: base(smoMetadataObject, parent)
	{
		_ApplicationRoles = new SmoMetaApplicationRole.CollectionHelperI(this);
		_AsymmetricKeys = new SmoMetaAsymmetricKey.CollectionHelperI(this);
		_Certificates = new SmoMetaCertificate.CollectionHelperI(this);
		_DatabaseRoles = new SmoMetaDatabaseRole.CollectionHelperI(this);
		_SchemaBasedSchemas = new SmoMetaSchema.CollectionHelperI(this);
		_RoleBasedSchemas = new RoleBasedSchemaCollectionHelperI(this);
		_UserBasedSchemas = new UserBasedSchemaCollectionHelperI(this);
		_Triggers = new SmoMetaDatabaseDdlTrigger.CollectionHelperI(this);
		_Users = new UserCollectionHelperI(this);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - SmoMetaDatabase
	// =========================================================================================================


	private const string SysSchemaName = "sys";


	#endregion Constants





	// =========================================================================================================
	#region Fields - SmoMetaDatabase
	// =========================================================================================================


	private readonly SmoMetaApplicationRole.CollectionHelperI _ApplicationRoles;

	private readonly SmoMetaAsymmetricKey.CollectionHelperI _AsymmetricKeys;

	private readonly SmoMetaCertificate.CollectionHelperI _Certificates;

	private readonly SmoMetaDatabaseRole.CollectionHelperI _DatabaseRoles;

	private readonly SmoMetaSchema.CollectionHelperI _SchemaBasedSchemas;

	private readonly RoleBasedSchemaCollectionHelperI _RoleBasedSchemas;

	private readonly UserBasedSchemaCollectionHelperI _UserBasedSchemas;

	private readonly SmoMetaDatabaseDdlTrigger.CollectionHelperI _Triggers;

	private readonly UserCollectionHelperI _Users;

	private Microsoft.SqlServer.Management.SqlParser.MetadataProvider.SchemaCollection _SchemaCollection;

	private Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo _CollationInfo;

	private string _DefaultSchemaName;

	private bool _DefaultSchemaNameRetrieved;

	private ISchema _SysSchema;

	private bool _SysSchemaRetrieved;

	private DatabaseCompatibilityLevel? _CompatibilityLevel;

	private TableMetadataListI _MetadataTables;

	private ViewMetadataListI _MetadataViews;

	private UserDefinedFunctionMetadataListI _MetadataUdfs;

	private UserDefinedAggregateMetadataListI _MetadataUserDefinedAggregates;

	private StoredProcedureMetadataListI _MetadataStoredProcedures;

	private SynonymMetadataListI _MetadataSynonyms;

	private ExtendedStoredProcedureMetadataListI _MetadataExtendedStoredProcedures;

	private UserDefinedDataTypeMetadataListI _MetadataUserDefinedDataTypes;

	private UserDefinedTableTypeMetadataListI _MetadataUserDefinedTableTypes;

	private UserDefinedTypeMetadataListI _MetadataUserDefinedClrTypes;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaDatabase
	// =========================================================================================================


	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => _SmoMetadataObject.IsSystemObject;

	public ISchema SysSchema
	{
		get
		{
			if (!_SysSchemaRetrieved)
			{
				_SysSchema = Schemas[SysSchemaName];
				_SysSchemaRetrieved = true;
			}
			return _SysSchema;
		}
	}

	public IMetadataCollection<IApplicationRole> ApplicationRoles => _ApplicationRoles.MetadataCollection;

	public IMetadataCollection<IAsymmetricKey> AsymmetricKeys => _AsymmetricKeys.MetadataCollection;

	public IMetadataCollection<ICertificate> Certificates => _Certificates.MetadataCollection;

	public Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo CollationInfo
	{
		get
		{
			if (_CollationInfo == null)
			{
				Cmd.TryGetPropertyObject<string>(_SmoMetadataObject, "Collation", out var value);
				_CollationInfo = value != null ? Cmd.GetCollationInfo(value) : Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo.Default;
			}
			return _CollationInfo;
		}
	}

	public DatabaseCompatibilityLevel CompatibilityLevel
	{
		get
		{
			if (!_CompatibilityLevel.HasValue)
			{
				if (Cmd.TryGetPropertyValue(_SmoMetadataObject, "CompatibilityLevel", out CompatibilityLevel? value) && value.HasValue)
				{
					switch (value.Value)
					{
						case Microsoft.SqlServer.Management.Smo.CompatibilityLevel.Version80:
							_CompatibilityLevel = DatabaseCompatibilityLevel.Version80;
							break;
						case Microsoft.SqlServer.Management.Smo.CompatibilityLevel.Version90:
							_CompatibilityLevel = DatabaseCompatibilityLevel.Version90;
							break;
						case Microsoft.SqlServer.Management.Smo.CompatibilityLevel.Version100:
							_CompatibilityLevel = DatabaseCompatibilityLevel.Version100;
							break;
						case Microsoft.SqlServer.Management.Smo.CompatibilityLevel.Version110:
							_CompatibilityLevel = DatabaseCompatibilityLevel.Version110;
							break;
						default:
							_CompatibilityLevel = DatabaseCompatibilityLevel.Current;
							break;
					}
				}
				else
				{
					_CompatibilityLevel = DatabaseCompatibilityLevel.Current;
				}
			}
			return _CompatibilityLevel.Value;
		}
	}

	public string DefaultSchemaName
	{
		get
		{
			if (!_DefaultSchemaNameRetrieved)
			{
				try
				{
					_DefaultSchemaName = _SmoMetadataObject.DefaultSchema;
				}
				catch (ConnectionException)
				{
					_DefaultSchemaName = null;
				}
				catch (SmoException)
				{
					_DefaultSchemaName = null;
				}
				_DefaultSchemaNameRetrieved = true;
			}
			return _DefaultSchemaName;
		}
	}

	public IUser Owner
	{
		get
		{
			Cmd.TryGetPropertyObject<string>(_SmoMetadataObject, "UserName", out var value);
			if (!string.IsNullOrEmpty(value))
			{
				return Users[value];
			}
			return null;
		}
	}

	public IMetadataCollection<IDatabaseRole> Roles => _DatabaseRoles.MetadataCollection;

	public IMetadataCollection<ISchema> Schemas
	{
		get
		{
			if (_SchemaCollection == null)
			{
				_SchemaCollection = new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.SchemaCollection(CollationInfo);
				if (Cmd.IsShilohDatabase(_SmoMetadataObject))
				{
					_SchemaCollection.AddRange(_UserBasedSchemas.MetadataCollection.Union(_RoleBasedSchemas.MetadataCollection));
				}
				else
				{
					_SchemaCollection.AddRange(_SchemaBasedSchemas.MetadataCollection);
				}
			}
			return _SchemaCollection;
		}
	}

	public IMetadataCollection<IDatabaseDdlTrigger> Triggers => _Triggers.MetadataCollection;

	public IMetadataCollection<IUser> Users => _Users.MetadataCollection;

	internal TableMetadataListI MetadataTables
	{
		get

		{
			_MetadataTables ??= new TableMetadataListI(_SmoMetadataObject.Tables, Schemas.Count, this);
			return _MetadataTables;
		}
	}

	internal ViewMetadataListI MetadataViews
	{
		get
		{
			_MetadataViews ??= new ViewMetadataListI(_SmoMetadataObject.Views, Schemas.Count, this);
			return _MetadataViews;
		}
	}

	internal UserDefinedFunctionMetadataListI MetadataUdfs
	{
		get
		{
			_MetadataUdfs ??= new UserDefinedFunctionMetadataListI(_SmoMetadataObject.UserDefinedFunctions, Schemas.Count, this);
			return _MetadataUdfs;
		}
	}

	internal UserDefinedAggregateMetadataListI MetadataUserDefinedAggregates
	{
		get
		{
			_MetadataUserDefinedAggregates ??= new UserDefinedAggregateMetadataListI(_SmoMetadataObject.UserDefinedAggregates, Schemas.Count, this);
			return _MetadataUserDefinedAggregates;
		}
	}

	internal StoredProcedureMetadataListI MetadataStoredProcedures
	{
		get
		{
			_MetadataStoredProcedures ??= new StoredProcedureMetadataListI(_SmoMetadataObject.StoredProcedures, Schemas.Count, this);
			return _MetadataStoredProcedures;
		}
	}

	internal SynonymMetadataListI MetadataSynonyms
	{
		get
		{
			_MetadataSynonyms ??= new SynonymMetadataListI(_SmoMetadataObject.Synonyms, Schemas.Count, this);
			return _MetadataSynonyms;
		}
	}

	internal ExtendedStoredProcedureMetadataListI MetadataExtendedStoredProcedures
	{
		get
		{
			_MetadataExtendedStoredProcedures ??= new ExtendedStoredProcedureMetadataListI(_SmoMetadataObject.ExtendedStoredProcedures, Schemas.Count, this);
			return _MetadataExtendedStoredProcedures;
		}
	}

	internal UserDefinedDataTypeMetadataListI MetadataUserDefinedDataTypes
	{
		get
		{
			_MetadataUserDefinedDataTypes ??= new UserDefinedDataTypeMetadataListI(_SmoMetadataObject.UserDefinedDataTypes, Schemas.Count, this);
			return _MetadataUserDefinedDataTypes;
		}
	}

	internal UserDefinedTableTypeMetadataListI MetadataUserDefinedTableTypes
	{
		get
		{
			_MetadataUserDefinedTableTypes ??= new UserDefinedTableTypeMetadataListI(_SmoMetadataObject.UserDefinedTableTypes, Schemas.Count, this);
			return _MetadataUserDefinedTableTypes;
		}
	}

	internal UserDefinedTypeMetadataListI MetadataUserDefinedClrTypes
	{
		get
		{
			_MetadataUserDefinedClrTypes ??= new UserDefinedTypeMetadataListI(_SmoMetadataObject.UserDefinedTypes, Schemas.Count, this);
			return _MetadataUserDefinedClrTypes;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaDatabase
	// =========================================================================================================


	public override T Accept<T>(IServerOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - SmoMetaDatabase
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: CollectionHelper<T, S>.
	/// </summary>
	// ---------------------------------------------------------------------------------


	public abstract class CollectionHelperI<T, S> : UnorderedCollectionHelperBaseI<T, S> where T : class, IDatabaseOwnedObject where S : NamedSmoObject
	{
		protected readonly SmoMetaDatabase _Database;

		protected override SmoMetaServer Server => _Database.Server;

		public CollectionHelperI(SmoMetaDatabase database)
		{
			_Database = database;
		}

		protected override Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo GetCollationInfo()
		{
			return _Database.CollationInfo;
		}
	}



	private class UserBasedSchemaCollectionHelperI : CollectionHelperI<ISchema, User>
	{
		public UserBasedSchemaCollectionHelperI(SmoMetaDatabase database)
			: base(database)
		{
		}

		protected override IMetadataListI<User> RetrieveSmoMetadataList()
		{
			return new EnumerableMetadataListI<User>(new SmoCollectionMetadataListI<User>(_Database.Server, _Database._SmoMetadataObject.Users).Where(Cmd.IsUserConvertableToSchema));
		}

		protected override IMutableMetadataCollection<ISchema> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.SchemaCollection(initialCapacity, collationInfo);
		}

		protected override ISchema CreateMetadataObject(User smoObject)
		{
			return new SmoMetaSchema(new Schema(smoObject.Parent, smoObject.Name), _Database);
		}
	}

	private class RoleBasedSchemaCollectionHelperI : CollectionHelperI<ISchema, DatabaseRole>
	{
		public RoleBasedSchemaCollectionHelperI(SmoMetaDatabase database)
			: base(database)
		{
		}

		protected override IMetadataListI<DatabaseRole> RetrieveSmoMetadataList()
		{
			return new EnumerableMetadataListI<DatabaseRole>(new SmoCollectionMetadataListI<DatabaseRole>(_Database.Server, _Database._SmoMetadataObject.Roles).Where(Cmd.IsRoleConvertableToSchema));
		}

		protected override IMutableMetadataCollection<ISchema> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.SchemaCollection(initialCapacity, collationInfo);
		}

		protected override ISchema CreateMetadataObject(DatabaseRole smoObject)
		{
			return new SmoMetaSchema(new Schema(smoObject.Parent, smoObject.Name), _Database);
		}
	}



	private class UserCollectionHelperI : CollectionHelperI<IUser, User>
	{
		public UserCollectionHelperI(SmoMetaDatabase database)
			: base(database)
		{
		}

		protected override IMetadataListI<User> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<User>(_Database.Server, _Database._SmoMetadataObject.Users);
		}

		protected override IMutableMetadataCollection<IUser> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.UserCollection(initialCapacity, collationInfo);
		}

		protected override IUser CreateMetadataObject(User smoObject)
		{
			return AbstractSmoMetaUser.CreateUser(smoObject, _Database);
		}
	}

	private class ArrayRangeI<T> : IMetadataListI<T>, IEnumerable<T>, IEnumerable where T : ScriptSchemaObjectBase
	{
		private readonly T[] _Data;

		private readonly int _StartIndex;

		private readonly int _Count;

		public int Count => _Count;

		public T this[int index] => _Data[_StartIndex + index];

		public ArrayRangeI(T[] data, int startIndex, int count)
		{
			_Data = data;
			_StartIndex = startIndex;
			_Count = count;
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < _Count; i++)
			{
				yield return _Data[_StartIndex + i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private class EmptyRangeI<T> : IMetadataListI<T>, IEnumerable<T>, IEnumerable where T : ScriptSchemaObjectBase
	{
		public int Count => 0;

		public T this[int index] => null;

		public IEnumerator<T> GetEnumerator()
		{
			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			yield break;
		}
	}

	internal abstract class MetadataListI<T, U> where T : ScriptSchemaObjectBase where U : SmoCollectionBase
	{
		private static readonly IMetadataListI<T> EmptyRange = new EmptyRangeI<T>();

		private readonly T[] _Data;

		private readonly SortedList<string, ArrayRangeI<T>> _RangeLookup;

		public IMetadataListI<T> this[string schemaName]
		{
			get
			{
				if (_RangeLookup.TryGetValue(schemaName, out var value))
				{
					return value;
				}
				return EmptyRange;
			}
		}

		public IMetadataListI<T> this[string schemaName, Predicate<T> filter]
		{
			get
			{
				IMetadataListI<T> metadataList = this[schemaName];
				T[] array = new T[metadataList.Count];
				int num = 0;
				foreach (T item in metadataList)
				{
					if (filter(item))
					{
						array[num++] = item;
					}
				}
				if (num != 0)
				{
					return new ArrayRangeI<T>(array, 0, num);
				}
				return EmptyRange;
			}
		}

		protected MetadataListI(U smoCollection, int schemaCount, SmoMetaDatabase database)
		{
			IMetadataListI<T> metadataList = new SmoCollectionMetadataListI<T>(database.Server, smoCollection);
			int count = metadataList.Count;
			_Data = new T[count];
			_RangeLookup = new SortedList<string, ArrayRangeI<T>>(schemaCount, database.CollationInfo.Comparer);
			if (count != 0)
			{
				int num = 0;
				foreach (T item in metadataList)
				{
					_Data[num] = item;
					num++;
				}
			}
			int comparison(T x, T y) => database.CollationInfo.Comparer.Compare(x.Schema, y.Schema);
			Array.Sort(_Data, comparison);
			int num2 = 0;
			while (num2 < count)
			{
				string schema = _Data[num2].Schema;
				int i;
				for (i = num2 + 1; i < count && _Data[i].Schema == schema; i++)
				{
				}
				AddSchemaRange(schema, num2, i - num2);
				num2 = i;
			}
		}

		private void AddSchemaRange(string schemaName, int startIndex, int count)
		{
			_RangeLookup.Add(schemaName, new ArrayRangeI<T>(_Data, startIndex, count));
		}
	}

	internal class TableMetadataListI : MetadataListI<Table, Microsoft.SqlServer.Management.Smo.TableCollection>
	{
		public TableMetadataListI(Microsoft.SqlServer.Management.Smo.TableCollection collection, int schemaCount, SmoMetaDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class ViewMetadataListI : MetadataListI<View, Microsoft.SqlServer.Management.Smo.ViewCollection>
	{
		public ViewMetadataListI(Microsoft.SqlServer.Management.Smo.ViewCollection collection, int schemaCount, SmoMetaDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class UserDefinedFunctionMetadataListI : MetadataListI<UserDefinedFunction, UserDefinedFunctionCollection>
	{
		public UserDefinedFunctionMetadataListI(UserDefinedFunctionCollection collection, int schemaCount, SmoMetaDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class UserDefinedAggregateMetadataListI : MetadataListI<UserDefinedAggregate, Microsoft.SqlServer.Management.Smo.UserDefinedAggregateCollection>
	{
		public UserDefinedAggregateMetadataListI(Microsoft.SqlServer.Management.Smo.UserDefinedAggregateCollection collection, int schemaCount, SmoMetaDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class StoredProcedureMetadataListI : MetadataListI<StoredProcedure, Microsoft.SqlServer.Management.Smo.StoredProcedureCollection>
	{
		public StoredProcedureMetadataListI(Microsoft.SqlServer.Management.Smo.StoredProcedureCollection collection, int schemaCount, SmoMetaDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class SynonymMetadataListI : MetadataListI<Synonym, Microsoft.SqlServer.Management.Smo.SynonymCollection>
	{
		public SynonymMetadataListI(Microsoft.SqlServer.Management.Smo.SynonymCollection collection, int schemaCount, SmoMetaDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class ExtendedStoredProcedureMetadataListI : MetadataListI<ExtendedStoredProcedure, Microsoft.SqlServer.Management.Smo.ExtendedStoredProcedureCollection>
	{
		public ExtendedStoredProcedureMetadataListI(Microsoft.SqlServer.Management.Smo.ExtendedStoredProcedureCollection collection, int schemaCount, SmoMetaDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class UserDefinedDataTypeMetadataListI : MetadataListI<UserDefinedDataType, Microsoft.SqlServer.Management.Smo.UserDefinedDataTypeCollection>
	{
		public UserDefinedDataTypeMetadataListI(Microsoft.SqlServer.Management.Smo.UserDefinedDataTypeCollection collection, int schemaCount, SmoMetaDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class UserDefinedTableTypeMetadataListI : MetadataListI<UserDefinedTableType, Microsoft.SqlServer.Management.Smo.UserDefinedTableTypeCollection>
	{
		public UserDefinedTableTypeMetadataListI(Microsoft.SqlServer.Management.Smo.UserDefinedTableTypeCollection collection, int schemaCount, SmoMetaDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class UserDefinedTypeMetadataListI : MetadataListI<UserDefinedType, UserDefinedTypeCollection>
	{
		public UserDefinedTypeMetadataListI(UserDefinedTypeCollection collection, int schemaCount, SmoMetaDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}


	#endregion Nested types

}
