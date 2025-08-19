// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.ApplicationRole

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using static BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.SmoMetaDatabase;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


// =========================================================================================================
//
//											SmoMetaApplicationRole Class
//
/// <summary>
/// Impersonation of an SQL Server Smo ApplicationRole for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaApplicationRole :	AbstractSmoMetaDatabasePrincipal<Microsoft.SqlServer.Management.Smo.ApplicationRole>, IApplicationRole, IDatabasePrincipal, IDatabaseOwnedObject, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaApplicationRole
	// ---------------------------------------------------------------------------------


	public SmoMetaApplicationRole(Microsoft.SqlServer.Management.Smo.ApplicationRole smoMetadataObject, SmoMetaDatabase parent)	: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property accessors - SmoMetaApplicationRole
	// =========================================================================================================


	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => false;

	public ISchema DefaultSchema => base.Parent.Schemas[_SmoMetadataObject.DefaultSchema];


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaApplicationRole
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
		return [];
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - SmoMetaApplicationRole
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Datbase.ApplicationRoleCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : CollectionHelperI<IApplicationRole, ApplicationRole>
	{
		public CollectionHelperI(SmoMetaDatabase database)
			: base(database)
		{
		}

		protected override IMetadataListI<ApplicationRole> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<ApplicationRole>(_Database.Server, _Database.SmoMetadataObject.ApplicationRoles);
		}

		protected override IMutableMetadataCollection<IApplicationRole> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.ApplicationRoleCollection(initialCapacity, collationInfo);
		}

		protected override IApplicationRole CreateMetadataObject(ApplicationRole smoObject)
		{
			return new SmoMetaApplicationRole(smoObject, _Database);
		}
	}


	#endregion	Nested types

}
