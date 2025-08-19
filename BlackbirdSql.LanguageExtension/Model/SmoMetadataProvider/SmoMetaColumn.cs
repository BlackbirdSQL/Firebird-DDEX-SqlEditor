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
//											SmoMetaColumn Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Column for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaColumn : IColumn, IScalar, IMetadataObject, IBsSmoDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaColumn
	// ---------------------------------------------------------------------------------


	private SmoMetaColumn(ISchemaOwnedObject parent, Microsoft.SqlServer.Management.Smo.Column smoColumn)
	{
		_Parent = parent;
		_SmoColumn = smoColumn;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaColumn
	// =========================================================================================================


	private readonly ISchemaOwnedObject _Parent;

	private readonly Microsoft.SqlServer.Management.Smo.Column _SmoColumn;

	private SmoMetaDefaultConstraint _DefaultConstraint;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaColumn
	// =========================================================================================================


	public ITabular Parent => (ITabular)_Parent;

	public bool InPrimaryKey
	{
		get
		{
			bool? propertyValue = Cmd.GetPropertyValue<bool>(_SmoColumn, "InPrimaryKey");
			if (!propertyValue.HasValue)
			{
				return false;
			}
			return propertyValue.Value;
		}
	}

	public bool Nullable => _SmoColumn.Nullable;

	public ICollation Collation
	{
		get
		{
			string propertyObject = Cmd.GetPropertyObject<string>(_SmoColumn, "Collation");
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
			if (_DefaultConstraint == null)
			{
				Microsoft.SqlServer.Management.Smo.DefaultConstraint defaultConstraint = _SmoColumn.DefaultConstraint;
				if (defaultConstraint != null)
				{
					_DefaultConstraint = new SmoMetaDefaultConstraint(this, defaultConstraint);
				}
			}
			return _DefaultConstraint;
		}
	}

	public virtual IdentityColumnInfo IdentityColumnInfo => null;

	public bool RowGuidCol => Cmd.GetPropertyValue<bool>(_SmoColumn, "RowGuidCol").GetValueOrDefault();

	public bool IsSparse
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoColumn, "IsSparse", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsColumnSet
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoColumn, "IsColumnSet", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsGeneratedAlwaysAsRowStart
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoColumn, "IsGeneratedAlwaysAsRowStart", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsGeneratedAlwaysAsRowEnd
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoColumn, "IsGeneratedAlwaysAsRowEnd", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsGeneratedAlwaysAsTransactionIdStart
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoColumn, "IsGeneratedAlwaysAsTransactionIdStart", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsGeneratedAlwaysAsTransactionIdEnd
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoColumn, "IsGeneratedAlwaysAsTransactionIdEnd", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsGeneratedAlwaysAsSequenceNumberStart
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoColumn, "IsGeneratedAlwaysAsSequenceNumberStart", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool IsGeneratedAlwaysAsSequenceNumberEnd
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoColumn, "IsGeneratedAlwaysAsSequenceNumberEnd", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public ScalarType ScalarType => ScalarType.Column;

	public IScalarDataType DataType => Cmd.GetDataType(_Parent.Schema.Database, _SmoColumn.DataType) as IScalarDataType;

	public IColumn AsColumn => this;

	public string Name => _SmoColumn.Name;

	public SqlSmoObject SmoObject => _SmoColumn;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaColumn
	// =========================================================================================================


	public static SmoMetaColumn Create(ISchemaOwnedObject parent, Microsoft.SqlServer.Management.Smo.Column smoColumn)
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
	#region									Nested types - SmoMetaColumn
	// =========================================================================================================

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Utils.ColumnCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : Cmd.OrderedCollectionHelperI<IColumn, Microsoft.SqlServer.Management.Smo.Column>
	{
		public CollectionHelperI(SmoMetaDatabase database, ISchemaOwnedObject parent, Microsoft.SqlServer.Management.Smo.ColumnCollection smoCollection)
			: base(database)
		{
			this.parent = parent;
			this.smoCollection = smoCollection;
		}


		private readonly Microsoft.SqlServer.Management.Smo.ColumnCollection smoCollection;

		private readonly ISchemaOwnedObject parent;


		protected override AbstractSmoMetaDatabaseObjectBase.IMetadataListI<Microsoft.SqlServer.Management.Smo.Column> RetrieveSmoMetadataList()
		{
			return new AbstractSmoMetaDatabaseObjectBase.SmoCollectionMetadataListI

				<Microsoft.SqlServer.Management.Smo.Column>(_Database.Server, smoCollection);
		}

		protected override Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo GetCollationInfo()
		{
			return ((IDatabaseTable)parent).CollationInfo;
		}

		protected override IColumn CreateMetadataObject(Microsoft.SqlServer.Management.Smo.Column smoObject)
		{
			return SmoMetaColumn.Create(parent, smoObject);
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: ComputedColumn.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private sealed class ComputedColumnI : SmoMetaColumn
	{
		public ComputedColumnI(ISchemaOwnedObject parent, Microsoft.SqlServer.Management.Smo.Column smoColumn)
			: base(parent, smoColumn)
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)smoColumn, "IsPersisted", out bool? value);
			_ComputedColumnInfo = new ComputedColumnInfo(smoColumn.ComputedText, value.GetValueOrDefault());
		}


		private readonly ComputedColumnInfo _ComputedColumnInfo;

		public override ComputedColumnInfo ComputedColumnInfo => _ComputedColumnInfo;

		public override IDefaultConstraint DefaultValue => null;

	}



	private sealed class IdentityColumnI : SmoMetaColumn
	{
		public IdentityColumnI(ISchemaOwnedObject parent, Microsoft.SqlServer.Management.Smo.Column smoColumn)
			: base(parent, smoColumn)
		{
			_IdentityColumnInfo = new IdentityColumnInfo(smoColumn.IdentitySeed, smoColumn.IdentityIncrement, smoColumn.NotForReplication);
		}


		private readonly IdentityColumnInfo _IdentityColumnInfo;

		public override IdentityColumnInfo IdentityColumnInfo => _IdentityColumnInfo;

		public override IDefaultConstraint DefaultValue => null;

	}


	#endregion Nested types

}
