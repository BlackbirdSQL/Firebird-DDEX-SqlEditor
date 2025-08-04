// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Column

using System;
using BlackbirdSql.LanguageExtension.Interfaces;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbColumn Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Column for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbColumn : IColumn, IScalar, IMetadataObject, IBsSmoDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbColumn
	// ---------------------------------------------------------------------------------


	private LsbColumn(ISchemaOwnedObject parent, Microsoft.SqlServer.Management.Smo.Column smoColumn)
	{
		m_parent = parent;
		m_smoColumn = smoColumn;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbColumn
	// =========================================================================================================


	private readonly ISchemaOwnedObject m_parent;

	private readonly Microsoft.SqlServer.Management.Smo.Column m_smoColumn;

	private LsbDefaultConstraint m_defaultConstraint;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbColumn
	// =========================================================================================================


	public ITabular Parent => (ITabular)m_parent;

	public bool InPrimaryKey
	{
		get
		{
			bool? propertyValue = Cmd.GetPropertyValue<bool>(m_smoColumn, "InPrimaryKey");
			if (!propertyValue.HasValue)
			{
				return false;
			}
			return propertyValue.Value;
		}
	}

	public bool Nullable => m_smoColumn.Nullable;

	public ICollation Collation
	{
		get
		{
			string propertyObject = Cmd.GetPropertyObject<string>(m_smoColumn, "Collation");
			if (!string.IsNullOrEmpty(propertyObject))
			{
				return Cmd.GetCollation(propertyObject);
			}
			return null;
		}
	}

	public virtual ComputedColumnInfo ComputedColumnInfo => null;

	public virtual IDefaultConstraint DefaultValue
	{
		get
		{
			if (m_defaultConstraint == null)
			{
				Microsoft.SqlServer.Management.Smo.DefaultConstraint defaultConstraint = m_smoColumn.DefaultConstraint;
				if (defaultConstraint != null)
				{
					m_defaultConstraint = new LsbDefaultConstraint(this, defaultConstraint);
				}
			}
			return m_defaultConstraint;
		}
	}

	public virtual IdentityColumnInfo IdentityColumnInfo => null;

	public bool RowGuidCol => Cmd.GetPropertyValue<bool>(m_smoColumn, "RowGuidCol").GetValueOrDefault();

	public bool IsSparse
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)m_smoColumn, "IsSparse", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsColumnSet
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)m_smoColumn, "IsColumnSet", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsGeneratedAlwaysAsRowStart
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)m_smoColumn, "IsGeneratedAlwaysAsRowStart", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsGeneratedAlwaysAsRowEnd
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)m_smoColumn, "IsGeneratedAlwaysAsRowEnd", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsGeneratedAlwaysAsTransactionIdStart
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)m_smoColumn, "IsGeneratedAlwaysAsTransactionIdStart", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsGeneratedAlwaysAsTransactionIdEnd
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)m_smoColumn, "IsGeneratedAlwaysAsTransactionIdEnd", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsGeneratedAlwaysAsSequenceNumberStart
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)m_smoColumn, "IsGeneratedAlwaysAsSequenceNumberStart", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsGeneratedAlwaysAsSequenceNumberEnd
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)m_smoColumn, "IsGeneratedAlwaysAsSequenceNumberEnd", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public ScalarType ScalarType => ScalarType.Column;

	public IScalarDataType DataType => Cmd.GetDataType(m_parent.Schema.Database, m_smoColumn.DataType) as IScalarDataType;

	public IColumn AsColumn => this;

	public string Name => m_smoColumn.Name;

	public SqlSmoObject SmoObject => m_smoColumn;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbColumn
	// =========================================================================================================


	public static LsbColumn Create(ISchemaOwnedObject parent, Microsoft.SqlServer.Management.Smo.Column smoColumn)
	{
		if (smoColumn.Identity)
		{
			return new IdentityColumnI(parent, smoColumn);
		}
		if (smoColumn.Computed)
		{
			return new ComputedColumnI(parent, smoColumn);
		}
		return new (parent, smoColumn);
	}



	public T Accept<T>(IMetadataObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - LsbColumn
	// =========================================================================================================

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Utils.ColumnCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : Cmd.OrderedCollectionHelperI<IColumn, Microsoft.SqlServer.Management.Smo.Column>
	{
		public CollectionHelperI(LsbDatabase database, ISchemaOwnedObject parent, Microsoft.SqlServer.Management.Smo.ColumnCollection smoCollection)
			: base(database)
		{
			this.parent = parent;
			this.smoCollection = smoCollection;
		}


		private readonly Microsoft.SqlServer.Management.Smo.ColumnCollection smoCollection;

		private readonly ISchemaOwnedObject parent;


		protected override AbstractDatabaseObject.IMetadataListI<Microsoft.SqlServer.Management.Smo.Column> RetrieveSmoMetadataList()
		{
			return new AbstractDatabaseObject.SmoCollectionMetadataListI

				<Microsoft.SqlServer.Management.Smo.Column>(m_database.Server, smoCollection);
		}

		protected override Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo GetCollationInfo()
		{
			return ((IDatabaseTable)parent).CollationInfo;
		}

		protected override IColumn CreateMetadataObject(Microsoft.SqlServer.Management.Smo.Column smoObject)
		{
			return LsbColumn.Create(parent, smoObject);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: ComputedColumn.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private sealed class ComputedColumnI : LsbColumn
	{
		public ComputedColumnI(ISchemaOwnedObject parent, Microsoft.SqlServer.Management.Smo.Column smoColumn)
			: base(parent, smoColumn)
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)smoColumn, "IsPersisted", out bool? value);
			m_computedColumnInfo = new ComputedColumnInfo(smoColumn.ComputedText, value.GetValueOrDefault());
		}


		private readonly ComputedColumnInfo m_computedColumnInfo;

		public override ComputedColumnInfo ComputedColumnInfo => m_computedColumnInfo;

		public override IDefaultConstraint DefaultValue => null;

	}



	private sealed class IdentityColumnI : LsbColumn
	{
		public IdentityColumnI(ISchemaOwnedObject parent, Microsoft.SqlServer.Management.Smo.Column smoColumn)
			: base(parent, smoColumn)
		{
			m_identityColumnInfo = new IdentityColumnInfo(smoColumn.IdentitySeed, smoColumn.IdentityIncrement, smoColumn.NotForReplication);
		}


		private readonly IdentityColumnInfo m_identityColumnInfo;

		public override IdentityColumnInfo IdentityColumnInfo => m_identityColumnInfo;

		public override IDefaultConstraint DefaultValue => null;

	}


	#endregion Nested types

}
