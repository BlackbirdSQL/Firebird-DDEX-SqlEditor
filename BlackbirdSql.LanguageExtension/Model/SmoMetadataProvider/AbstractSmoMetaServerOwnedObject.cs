// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.ServerOwnedObject<S>

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


// =========================================================================================================
//
//											AbstractSmoMetaServerOwnedObject Class
//
/// <summary>
/// The connection impersonating an SQL Server Smo ServerOwnedObject for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractSmoMetaServerOwnedObject<S> : AbstractSmoMetaDatabaseObject<S, SmoMetaServer>, IServerOwnedObject, IDatabaseObject, IMetadataObject where S : NamedSmoObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractSmoMetaServerOwnedObject
	// ---------------------------------------------------------------------------------


	protected AbstractSmoMetaServerOwnedObject(S smoMetadataObject, SmoMetaServer parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property accessors - AbstractSmoMetaServerOwnedObject
	// =========================================================================================================


	public SmoMetaServer Server => _Parent;

	IServer IServerOwnedObject.Server => _Parent;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractSmoMetaServerOwnedObject
	// =========================================================================================================


	public sealed override T Accept<T>(IDatabaseObjectVisitor<T> visitor)
	{
		return Accept(visitor);
	}

	public sealed override void AssertNameMatches(string name, string detailedMessage)
	{
		_ = _Parent.CollationInfo;
	}

	public abstract T Accept<T>(IServerOwnedObjectVisitor<T> visitor);


	#endregion Methods

}
