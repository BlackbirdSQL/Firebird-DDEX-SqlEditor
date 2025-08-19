// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Credential

using System;
using Microsoft.SqlServer.Management.Smo;
// using Microsoft.SqlServer.Management.SmoMetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


// =========================================================================================================
//
//											SmoMetaCredential Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Credential for providing metadata.
/// </summary>
// =========================================================================================================

internal class SmoMetaCredential : AbstractSmoMetaServerOwnedObject<Credential>, ICredential, IServerOwnedObject, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaCredential
	// ---------------------------------------------------------------------------------

	public SmoMetaCredential(Credential smoMetadataObject, SmoMetaServer parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property accessors - SmoMetaCredential
	// =========================================================================================================


	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => false;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaCredential
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

}
