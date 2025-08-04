// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Database

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Common;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbDatabase Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Database for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbDatabase : LsbServerOwnedObject<Database>, IDatabase, IServerOwnedObject, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbDatabase
	// ---------------------------------------------------------------------------------


	public LsbDatabase(Database smoMetadataObject, LsbMetadataServer parent)
		: base(smoMetadataObject, parent)
	{
		m_applicationRoles = new LsbApplicationRole.CollectionHelperI(this);
		m_asymmetricKeys = new LsbAsymmetricKey.CollectionHelperI(this);
		m_certificates = new LsbCertificate.CollectionHelperI(this);
		m_databaseRoles = new LsbDatabaseRole.CollectionHelperI(this);
		m_schemaBasedSchemas = new LsbSchema.CollectionHelperI(this);
		m_roleBasedSchemas = new RoleBasedSchemaCollectionHelperI(this);
		m_userBasedSchemas = new UserBasedSchemaCollectionHelperI(this);
		m_triggers = new LsbDatabaseDdlTrigger.CollectionHelperI(this);
		m_users = new UserCollectionHelperI(this);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - LsbDatabase
	// =========================================================================================================


	private const string SysSchemaName = "sys";


	#endregion Constants





	// =========================================================================================================
	#region Fields - LsbDatabase
	// =========================================================================================================


	private readonly LsbApplicationRole.CollectionHelperI m_applicationRoles;

	private readonly LsbAsymmetricKey.CollectionHelperI m_asymmetricKeys;

	private readonly LsbCertificate.CollectionHelperI m_certificates;

	private readonly LsbDatabaseRole.CollectionHelperI m_databaseRoles;

	private readonly LsbSchema.CollectionHelperI m_schemaBasedSchemas;

	private readonly RoleBasedSchemaCollectionHelperI m_roleBasedSchemas;

	private readonly UserBasedSchemaCollectionHelperI m_userBasedSchemas;

	private readonly LsbDatabaseDdlTrigger.CollectionHelperI m_triggers;

	private readonly UserCollectionHelperI m_users;

	private Microsoft.SqlServer.Management.SqlParser.MetadataProvider.SchemaCollection m_schemaCollection;

	private Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo m_collationInfo;

	private string m_defaultSchemaName;

	private bool m_defaultSchemaNameRetrieved;

	private ISchema m_sysSchema;

	private bool m_sysSchemaRetrieved;

	private DatabaseCompatibilityLevel? m_compatibilityLevel;

	private TableMetadataListI m_metadataTables;

	private ViewMetadataListI m_metadataViews;

	private UserDefinedFunctionMetadataListI m_metadataUdfs;

	private UserDefinedAggregateMetadataListI m_metadataUserDefinedAggregates;

	private StoredProcedureMetadataListI m_metadataStoredProcedures;

	private SynonymMetadataListI m_metadataSynonyms;

	private ExtendedStoredProcedureMetadataListI m_metadataExtendedStoredProcedures;

	private UserDefinedDataTypeMetadataListI m_metadataUserDefinedDataTypes;

	private UserDefinedTableTypeMetadataListI m_metadataUserDefinedTableTypes;

	private UserDefinedTypeMetadataListI m_metadataUserDefinedClrTypes;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbDatabase
	// =========================================================================================================


	public override int Id => m_smoMetadataObject.ID;

	public override bool IsSystemObject => m_smoMetadataObject.IsSystemObject;

	public ISchema SysSchema
	{
		get
		{
			if (!m_sysSchemaRetrieved)
			{
				m_sysSchema = Schemas[SysSchemaName];
				m_sysSchemaRetrieved = true;
			}
			return m_sysSchema;
		}
	}

	public IMetadataCollection<IApplicationRole> ApplicationRoles => m_applicationRoles.MetadataCollection;

	public IMetadataCollection<IAsymmetricKey> AsymmetricKeys => m_asymmetricKeys.MetadataCollection;

	public IMetadataCollection<ICertificate> Certificates => m_certificates.MetadataCollection;

	public Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo CollationInfo
	{
		get
		{
			if (m_collationInfo == null)
			{
				Cmd.TryGetPropertyObject<string>(m_smoMetadataObject, "Collation", out var value);
				m_collationInfo = value != null ? Cmd.GetCollationInfo(value) : Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo.Default;
			}
			return m_collationInfo;
		}
	}

	public DatabaseCompatibilityLevel CompatibilityLevel
	{
		get
		{
			if (!m_compatibilityLevel.HasValue)
			{
				if (Cmd.TryGetPropertyValue(m_smoMetadataObject, "CompatibilityLevel", out CompatibilityLevel? value) && value.HasValue)
				{
					switch (value.Value)
					{
						case Microsoft.SqlServer.Management.Smo.CompatibilityLevel.Version80:
							m_compatibilityLevel = DatabaseCompatibilityLevel.Version80;
							break;
						case Microsoft.SqlServer.Management.Smo.CompatibilityLevel.Version90:
							m_compatibilityLevel = DatabaseCompatibilityLevel.Version90;
							break;
						case Microsoft.SqlServer.Management.Smo.CompatibilityLevel.Version100:
							m_compatibilityLevel = DatabaseCompatibilityLevel.Version100;
							break;
						case Microsoft.SqlServer.Management.Smo.CompatibilityLevel.Version110:
							m_compatibilityLevel = DatabaseCompatibilityLevel.Version110;
							break;
						default:
							m_compatibilityLevel = DatabaseCompatibilityLevel.Current;
							break;
					}
				}
				else
				{
					m_compatibilityLevel = DatabaseCompatibilityLevel.Current;
				}
			}
			return m_compatibilityLevel.Value;
		}
	}

	public string DefaultSchemaName
	{
		get
		{
			if (!m_defaultSchemaNameRetrieved)
			{
				try
				{
					m_defaultSchemaName = m_smoMetadataObject.DefaultSchema;
				}
				catch (ConnectionException)
				{
					m_defaultSchemaName = null;
				}
				catch (SmoException)
				{
					m_defaultSchemaName = null;
				}
				m_defaultSchemaNameRetrieved = true;
			}
			return m_defaultSchemaName;
		}
	}

	public IUser Owner
	{
		get
		{
			Cmd.TryGetPropertyObject<string>(m_smoMetadataObject, "UserName", out var value);
			if (!string.IsNullOrEmpty(value))
			{
				return Users[value];
			}
			return null;
		}
	}

	public IMetadataCollection<IDatabaseRole> Roles => m_databaseRoles.MetadataCollection;

	public IMetadataCollection<ISchema> Schemas
	{
		get
		{
			if (m_schemaCollection == null)
			{
				m_schemaCollection = new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.SchemaCollection(CollationInfo);
				if (Cmd.IsShilohDatabase(m_smoMetadataObject))
				{
					m_schemaCollection.AddRange(m_userBasedSchemas.MetadataCollection.Union(m_roleBasedSchemas.MetadataCollection));
				}
				else
				{
					m_schemaCollection.AddRange(m_schemaBasedSchemas.MetadataCollection);
				}
			}
			return m_schemaCollection;
		}
	}

	public IMetadataCollection<IDatabaseDdlTrigger> Triggers => m_triggers.MetadataCollection;

	public IMetadataCollection<IUser> Users => m_users.MetadataCollection;

	internal TableMetadataListI MetadataTables
	{
		get

		{
			m_metadataTables ??= new TableMetadataListI(m_smoMetadataObject.Tables, Schemas.Count, this);
			return m_metadataTables;
		}
	}

	internal ViewMetadataListI MetadataViews
	{
		get
		{
			m_metadataViews ??= new ViewMetadataListI(m_smoMetadataObject.Views, Schemas.Count, this);
			return m_metadataViews;
		}
	}

	internal UserDefinedFunctionMetadataListI MetadataUdfs
	{
		get
		{
			m_metadataUdfs ??= new UserDefinedFunctionMetadataListI(m_smoMetadataObject.UserDefinedFunctions, Schemas.Count, this);
			return m_metadataUdfs;
		}
	}

	internal UserDefinedAggregateMetadataListI MetadataUserDefinedAggregates
	{
		get
		{
			m_metadataUserDefinedAggregates ??= new UserDefinedAggregateMetadataListI(m_smoMetadataObject.UserDefinedAggregates, Schemas.Count, this);
			return m_metadataUserDefinedAggregates;
		}
	}

	internal StoredProcedureMetadataListI MetadataStoredProcedures
	{
		get
		{
			m_metadataStoredProcedures ??= new StoredProcedureMetadataListI(m_smoMetadataObject.StoredProcedures, Schemas.Count, this);
			return m_metadataStoredProcedures;
		}
	}

	internal SynonymMetadataListI MetadataSynonyms
	{
		get
		{
			m_metadataSynonyms ??= new SynonymMetadataListI(m_smoMetadataObject.Synonyms, Schemas.Count, this);
			return m_metadataSynonyms;
		}
	}

	internal ExtendedStoredProcedureMetadataListI MetadataExtendedStoredProcedures
	{
		get
		{
			m_metadataExtendedStoredProcedures ??= new ExtendedStoredProcedureMetadataListI(m_smoMetadataObject.ExtendedStoredProcedures, Schemas.Count, this);
			return m_metadataExtendedStoredProcedures;
		}
	}

	internal UserDefinedDataTypeMetadataListI MetadataUserDefinedDataTypes
	{
		get
		{
			m_metadataUserDefinedDataTypes ??= new UserDefinedDataTypeMetadataListI(m_smoMetadataObject.UserDefinedDataTypes, Schemas.Count, this);
			return m_metadataUserDefinedDataTypes;
		}
	}

	internal UserDefinedTableTypeMetadataListI MetadataUserDefinedTableTypes
	{
		get
		{
			m_metadataUserDefinedTableTypes ??= new UserDefinedTableTypeMetadataListI(m_smoMetadataObject.UserDefinedTableTypes, Schemas.Count, this);
			return m_metadataUserDefinedTableTypes;
		}
	}

	internal UserDefinedTypeMetadataListI MetadataUserDefinedClrTypes
	{
		get
		{
			m_metadataUserDefinedClrTypes ??= new UserDefinedTypeMetadataListI(m_smoMetadataObject.UserDefinedTypes, Schemas.Count, this);
			return m_metadataUserDefinedClrTypes;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbDatabase
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
	#region									Nested types - LsbDatabase
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: CollectionHelper<T, S>.
	/// </summary>
	// ---------------------------------------------------------------------------------


	public abstract class CollectionHelperI<T, S> : UnorderedCollectionHelperBaseI<T, S> where T : class, IDatabaseOwnedObject where S : NamedSmoObject
	{
		protected readonly LsbDatabase m_database;

		protected override LsbMetadataServer Server => m_database.Server;

		public CollectionHelperI(LsbDatabase database)
		{
			m_database = database;
		}

		protected override Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo GetCollationInfo()
		{
			return m_database.CollationInfo;
		}
	}



	private class UserBasedSchemaCollectionHelperI : CollectionHelperI<ISchema, User>
	{
		public UserBasedSchemaCollectionHelperI(LsbDatabase database)
			: base(database)
		{
		}

		protected override IMetadataListI<User> RetrieveSmoMetadataList()
		{
			return new EnumerableMetadataListI<User>(new SmoCollectionMetadataListI<User>(m_database.Server, m_database.m_smoMetadataObject.Users).Where(Cmd.IsUserConvertableToSchema));
		}

		protected override IMutableMetadataCollection<ISchema> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.SchemaCollection(initialCapacity, collationInfo);
		}

		protected override ISchema CreateMetadataObject(User smoObject)
		{
			return new LsbSchema(new Schema(smoObject.Parent, smoObject.Name), m_database);
		}
	}

	private class RoleBasedSchemaCollectionHelperI : CollectionHelperI<ISchema, DatabaseRole>
	{
		public RoleBasedSchemaCollectionHelperI(LsbDatabase database)
			: base(database)
		{
		}

		protected override IMetadataListI<DatabaseRole> RetrieveSmoMetadataList()
		{
			return new EnumerableMetadataListI<DatabaseRole>(new SmoCollectionMetadataListI<DatabaseRole>(m_database.Server, m_database.m_smoMetadataObject.Roles).Where(Cmd.IsRoleConvertableToSchema));
		}

		protected override IMutableMetadataCollection<ISchema> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.SchemaCollection(initialCapacity, collationInfo);
		}

		protected override ISchema CreateMetadataObject(DatabaseRole smoObject)
		{
			return new LsbSchema(new Schema(smoObject.Parent, smoObject.Name), m_database);
		}
	}



	private class UserCollectionHelperI : CollectionHelperI<IUser, User>
	{
		public UserCollectionHelperI(LsbDatabase database)
			: base(database)
		{
		}

		protected override IMetadataListI<User> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<User>(m_database.Server, m_database.m_smoMetadataObject.Users);
		}

		protected override IMutableMetadataCollection<IUser> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.UserCollection(initialCapacity, collationInfo);
		}

		protected override IUser CreateMetadataObject(User smoObject)
		{
			return LsbUser.CreateUser(smoObject, m_database);
		}
	}

	private class ArrayRangeI<T> : IMetadataListI<T>, IEnumerable<T>, IEnumerable where T : ScriptSchemaObjectBase
	{
		private readonly T[] m_data;

		private readonly int m_startIndex;

		private readonly int m_count;

		public int Count => m_count;

		public T this[int index] => m_data[m_startIndex + index];

		public ArrayRangeI(T[] data, int startIndex, int count)
		{
			m_data = data;
			m_startIndex = startIndex;
			m_count = count;
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < m_count; i++)
			{
				yield return m_data[m_startIndex + i];
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

		private readonly T[] m_data;

		private readonly SortedList<string, ArrayRangeI<T>> m_rangeLookup;

		public IMetadataListI<T> this[string schemaName]
		{
			get
			{
				if (m_rangeLookup.TryGetValue(schemaName, out var value))
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

		protected MetadataListI(U smoCollection, int schemaCount, LsbDatabase database)
		{
			IMetadataListI<T> metadataList = new SmoCollectionMetadataListI<T>(database.Server, smoCollection);
			int count = metadataList.Count;
			m_data = new T[count];
			m_rangeLookup = new SortedList<string, ArrayRangeI<T>>(schemaCount, database.CollationInfo.Comparer);
			if (count != 0)
			{
				int num = 0;
				foreach (T item in metadataList)
				{
					m_data[num] = item;
					num++;
				}
			}
			int comparison(T x, T y) => database.CollationInfo.Comparer.Compare(x.Schema, y.Schema);
			Array.Sort(m_data, comparison);
			int num2 = 0;
			while (num2 < count)
			{
				string schema = m_data[num2].Schema;
				int i;
				for (i = num2 + 1; i < count && m_data[i].Schema == schema; i++)
				{
				}
				AddSchemaRange(schema, num2, i - num2);
				num2 = i;
			}
		}

		private void AddSchemaRange(string schemaName, int startIndex, int count)
		{
			m_rangeLookup.Add(schemaName, new ArrayRangeI<T>(m_data, startIndex, count));
		}
	}

	internal class TableMetadataListI : MetadataListI<Table, Microsoft.SqlServer.Management.Smo.TableCollection>
	{
		public TableMetadataListI(Microsoft.SqlServer.Management.Smo.TableCollection collection, int schemaCount, LsbDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class ViewMetadataListI : MetadataListI<View, Microsoft.SqlServer.Management.Smo.ViewCollection>
	{
		public ViewMetadataListI(Microsoft.SqlServer.Management.Smo.ViewCollection collection, int schemaCount, LsbDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class UserDefinedFunctionMetadataListI : MetadataListI<UserDefinedFunction, UserDefinedFunctionCollection>
	{
		public UserDefinedFunctionMetadataListI(UserDefinedFunctionCollection collection, int schemaCount, LsbDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class UserDefinedAggregateMetadataListI : MetadataListI<UserDefinedAggregate, Microsoft.SqlServer.Management.Smo.UserDefinedAggregateCollection>
	{
		public UserDefinedAggregateMetadataListI(Microsoft.SqlServer.Management.Smo.UserDefinedAggregateCollection collection, int schemaCount, LsbDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class StoredProcedureMetadataListI : MetadataListI<StoredProcedure, Microsoft.SqlServer.Management.Smo.StoredProcedureCollection>
	{
		public StoredProcedureMetadataListI(Microsoft.SqlServer.Management.Smo.StoredProcedureCollection collection, int schemaCount, LsbDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class SynonymMetadataListI : MetadataListI<Synonym, Microsoft.SqlServer.Management.Smo.SynonymCollection>
	{
		public SynonymMetadataListI(Microsoft.SqlServer.Management.Smo.SynonymCollection collection, int schemaCount, LsbDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class ExtendedStoredProcedureMetadataListI : MetadataListI<ExtendedStoredProcedure, Microsoft.SqlServer.Management.Smo.ExtendedStoredProcedureCollection>
	{
		public ExtendedStoredProcedureMetadataListI(Microsoft.SqlServer.Management.Smo.ExtendedStoredProcedureCollection collection, int schemaCount, LsbDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class UserDefinedDataTypeMetadataListI : MetadataListI<UserDefinedDataType, Microsoft.SqlServer.Management.Smo.UserDefinedDataTypeCollection>
	{
		public UserDefinedDataTypeMetadataListI(Microsoft.SqlServer.Management.Smo.UserDefinedDataTypeCollection collection, int schemaCount, LsbDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class UserDefinedTableTypeMetadataListI : MetadataListI<UserDefinedTableType, Microsoft.SqlServer.Management.Smo.UserDefinedTableTypeCollection>
	{
		public UserDefinedTableTypeMetadataListI(Microsoft.SqlServer.Management.Smo.UserDefinedTableTypeCollection collection, int schemaCount, LsbDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}

	internal class UserDefinedTypeMetadataListI : MetadataListI<UserDefinedType, UserDefinedTypeCollection>
	{
		public UserDefinedTypeMetadataListI(UserDefinedTypeCollection collection, int schemaCount, LsbDatabase database)
			: base(collection, schemaCount, database)
		{
		}
	}


	#endregion Nested types

}
