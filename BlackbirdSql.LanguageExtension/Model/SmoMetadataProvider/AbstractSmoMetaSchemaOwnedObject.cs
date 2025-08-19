// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.SchemaOwnedObject<S>

using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											AbstractSmoMetaSchemaOwnedObject Class
//
/// <summary>
/// Impersonation of an SQL Server Smo SchemaOwnedObject for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractSmoMetaSchemaOwnedObject<S> : AbstractSmoMetaDatabaseObject<S, SmoMetaSchema>, ISchemaOwnedObject, IDatabaseObject, IMetadataObject where S : ScriptSchemaObjectBase
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractSmoMetaSchemaOwnedObject
	// ---------------------------------------------------------------------------------


	protected AbstractSmoMetaSchemaOwnedObject(S smoMetadataObject, SmoMetaSchema parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property accessors - AbstractSmoMetaSchemaOwnedObject
	// =========================================================================================================


	public Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo CollationInfo => _Parent.Database.CollationInfo;

	public ISchema Schema => _Parent;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractSmoMetaSchemaOwnedObject
	// =========================================================================================================


	public override void AssertNameMatches(string name, string detailedMessage)
	{
		_ = _Parent.Database.CollationInfo;
	}

	public sealed override T Accept<T>(IDatabaseObjectVisitor<T> visitor)
	{
		return Accept(visitor);
	}

	public abstract T Accept<T>(ISchemaOwnedObjectVisitor<T> visitor);


	#endregion Methods

}
