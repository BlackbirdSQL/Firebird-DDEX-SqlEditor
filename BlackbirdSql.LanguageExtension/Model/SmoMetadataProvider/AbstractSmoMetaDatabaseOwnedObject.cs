// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.DatabaseOwnedObject<S>

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


// =========================================================================================================
//
//											AbstractSmoMetaDatabaseOwnedObject Class
//
/// <summary>
/// Impersonation of an SQL Server Smo DatabaseOwnedObject for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractSmoMetaDatabaseOwnedObject<S> : AbstractSmoMetaDatabaseObject<S, SmoMetaDatabase>, IDatabaseOwnedObject, IDatabaseObject, IMetadataObject where S : NamedSmoObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractSmoMetaDatabaseOwnedObject
	// ---------------------------------------------------------------------------------


	protected AbstractSmoMetaDatabaseOwnedObject(S smoMetadataObject, SmoMetaDatabase parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property accessors - AbstractSmoMetaDatabaseOwnedObject
	// =========================================================================================================


	public SmoMetaDatabase Database => _Parent;

	IDatabase IDatabaseOwnedObject.Database => _Parent;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractSmoMetaDatabaseOwnedObject
	// =========================================================================================================


	public sealed override T Accept<T>(IDatabaseObjectVisitor<T> visitor)
	{
		return Accept(visitor);
	}

	public sealed override void AssertNameMatches(string name, string detailedMessage)
	{
		_ = _Parent.CollationInfo;
	}

	public abstract T Accept<T>(IDatabaseOwnedObjectVisitor<T> visitor);


	#endregion Methods

}
