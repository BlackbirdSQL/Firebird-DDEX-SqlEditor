// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.DatabaseRole
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using static BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaDatabase;
using static Microsoft.SqlServer.Management.SqlParser.MetadataProvider.MetadataProviderUtils.Names;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											SmoMetaDatabaseRole Class
//
/// <summary>
/// Impersonation of an SQL Server Smo DatabaseRole for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaDatabaseRole : AbstractSmoMetaDatabasePrincipal<Microsoft.SqlServer.Management.Smo.DatabaseRole>, IDatabaseRole, IDatabasePrincipal, IDatabaseOwnedObject, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaDatabaseRole
	// ---------------------------------------------------------------------------------


	public SmoMetaDatabaseRole(Microsoft.SqlServer.Management.Smo.DatabaseRole smoMetadataObject, SmoMetaDatabase parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaDatabaseRole
	// =========================================================================================================


	private bool? isSystemObject;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaDatabaseRole
	// =========================================================================================================


	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject
	{
		get
		{
			if (!isSystemObject.HasValue)
			{
				isSystemObject = IsFixedRole || base.SmoObject == base.Parent.SmoObject.Roles["public"];
			}
			return isSystemObject.Value;
		}
	}

	public bool IsFixedRole => _SmoMetadataObject.IsFixedRole;

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


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaDatabaseRole
	// =========================================================================================================


	public override T Accept<T>(IDatabaseOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}

	protected override IEnumerable<string> GetMemberOfRoleNames()
	{
		return _SmoMetadataObject.EnumRoles().Cast<string>();
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - SmoMetaDatabaseRole
	// =========================================================================================================

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Database.DatabaseRoleCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : CollectionHelperI<IDatabaseRole, DatabaseRole>
	{
		public CollectionHelperI(SmoMetaDatabase database)
		: base(database)
		{
		}

		protected override IMetadataListI<DatabaseRole> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<DatabaseRole>(_Database.Server, _Database.SmoMetadataObject.Roles);
		}

		protected override IMutableMetadataCollection<IDatabaseRole> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.DatabaseRoleCollection(initialCapacity, collationInfo);
		}

		protected override IDatabaseRole CreateMetadataObject(DatabaseRole smoObject)
		{
			return new SmoMetaDatabaseRole(smoObject, _Database);
		}
	}


	#endregion Nested types

}
