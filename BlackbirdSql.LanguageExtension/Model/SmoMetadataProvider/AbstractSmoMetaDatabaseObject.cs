// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.DatabaseObject<S,P>

using System.Diagnostics;
using BlackbirdSql.LanguageExtension.Interfaces;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											AbstractSmoMetaDatabaseObject Class
//
/// <summary>
/// Impersonation of an SQL Server Smo DatabaseObject<S,P> for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractSmoMetaDatabaseObject<S, P> : AbstractSmoMetaDatabaseObjectBase, IDatabaseObject, IMetadataObject, IBsSmoDatabaseObject where S : NamedSmoObject where P : class, IDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractSmoMetaDatabaseObject
	// ---------------------------------------------------------------------------------


	protected AbstractSmoMetaDatabaseObject(S smoMetadataObject, P parent)
	{
		_SmoMetadataObject = smoMetadataObject;
		_Parent = parent;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractSmoMetaDatabaseObject
	// =========================================================================================================


	protected readonly S _SmoMetadataObject;

	protected readonly P _Parent;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractSmoMetaDatabaseObject
	// =========================================================================================================


	public S SmoMetadataObject => _SmoMetadataObject;

	public P Parent => _Parent;

	public S SmoObject => _SmoMetadataObject;

	public abstract int Id { get; }

	IDatabaseObject IDatabaseObject.Parent => _Parent;

	public abstract bool IsSystemObject { get; }

	public bool IsVolatile => false;

	public string Name => _SmoMetadataObject.Name;

	SqlSmoObject IBsSmoDatabaseObject.SmoObject => _SmoMetadataObject;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractSmoMetaDatabaseObject
	// =========================================================================================================


	public abstract T Accept<T>(IDatabaseObjectVisitor<T> visitor);

	public T Accept<T>(IMetadataObjectVisitor<T> visitor)
	{
		return Accept((IDatabaseObjectVisitor<T>)visitor);
	}

	[Conditional("DEBUG")]
	public abstract void AssertNameMatches(string name, string detailedMessage);


	#endregion Methods
}
