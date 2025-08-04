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
//											LsbDatabaseObject Class
//
/// <summary>
/// Impersonation of an SQL Server Smo DatabaseObject<S,P> for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class LsbDatabaseObject<S, P> : AbstractDatabaseObject, IDatabaseObject, IMetadataObject, IBsSmoDatabaseObject where S : NamedSmoObject where P : class, IDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbDatabaseObject
	// ---------------------------------------------------------------------------------


	protected LsbDatabaseObject(S smoMetadataObject, P parent)
	{
		m_smoMetadataObject = smoMetadataObject;
		m_parent = parent;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbDatabaseObject
	// =========================================================================================================


	protected readonly S m_smoMetadataObject;

	protected readonly P m_parent;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbDatabaseObject
	// =========================================================================================================


	public S SmoMetadataObject => m_smoMetadataObject;

	public P Parent => m_parent;

	public S SmoObject => m_smoMetadataObject;

	public abstract int Id { get; }

	IDatabaseObject IDatabaseObject.Parent => m_parent;

	public abstract bool IsSystemObject { get; }

	public bool IsVolatile => false;

	public string Name => m_smoMetadataObject.Name;

	SqlSmoObject IBsSmoDatabaseObject.SmoObject => m_smoMetadataObject;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbDatabaseObject
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
