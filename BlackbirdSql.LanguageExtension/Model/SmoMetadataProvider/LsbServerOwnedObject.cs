// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.ServerOwnedObject<S>

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbSmoMetadataProviderServerOwnedObject Class
//
/// <summary>
/// The connection impersonating an SQL Server Smo ServerOwnedObject for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class LsbServerOwnedObject<S> : LsbDatabaseObject<S, LsbMetadataServer>, IServerOwnedObject, IDatabaseObject, IMetadataObject where S : NamedSmoObject
{
	protected LsbServerOwnedObject(S smoMetadataObject, LsbMetadataServer parent)
		: base(smoMetadataObject, parent)
	{
	}


	public LsbMetadataServer Server => m_parent;

	IServer IServerOwnedObject.Server => m_parent;


	public sealed override T Accept<T>(IDatabaseObjectVisitor<T> visitor)
	{
		return Accept(visitor);
	}

	public sealed override void AssertNameMatches(string name, string detailedMessage)
	{
		_ = m_parent.CollationInfo;
	}

	public abstract T Accept<T>(IServerOwnedObjectVisitor<T> visitor);
}
