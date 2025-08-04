// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.DatabaseOwnedObject<S>

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


// =========================================================================================================
//
//											LsbDatabaseOwnedObject Class
//
/// <summary>
/// Impersonation of an SQL Server Smo DatabaseOwnedObject for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class LsbDatabaseOwnedObject<S> : LsbDatabaseObject<S, LsbDatabase>, IDatabaseOwnedObject, IDatabaseObject, IMetadataObject where S : NamedSmoObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbDatabaseOwnedObject
	// ---------------------------------------------------------------------------------


	protected LsbDatabaseOwnedObject(S smoMetadataObject, LsbDatabase parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property accessors - LsbDatabaseOwnedObject
	// =========================================================================================================


	public LsbDatabase Database => m_parent;

	IDatabase IDatabaseOwnedObject.Database => m_parent;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbDatabaseOwnedObject
	// =========================================================================================================


	public sealed override T Accept<T>(IDatabaseObjectVisitor<T> visitor)
	{
		return Accept(visitor);
	}

	public sealed override void AssertNameMatches(string name, string detailedMessage)
	{
		_ = m_parent.CollationInfo;
	}

	public abstract T Accept<T>(IDatabaseOwnedObjectVisitor<T> visitor);


	#endregion Methods

}
