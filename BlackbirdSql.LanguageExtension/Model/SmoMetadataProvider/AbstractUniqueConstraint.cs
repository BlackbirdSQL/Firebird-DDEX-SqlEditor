// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.UniqueConstraintBase

using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											AbstractUniqueConstraint Class
//
/// <summary>
/// Impersonation of an SQL Server Smo UniqueConstraintBase for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractUniqueConstraint : IUniqueConstraintBase, IConstraint, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractUniqueConstraint
	// ---------------------------------------------------------------------------------


	protected AbstractUniqueConstraint(IDatabaseTable parent, IRelationalIndex index)
	{
		m_parent = parent;
		m_index = index;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractUniqueConstraint
	// =========================================================================================================


	private readonly IDatabaseTable m_parent;

	private readonly IRelationalIndex m_index;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractUniqueConstraint
	// =========================================================================================================


	public ITabular Parent => m_parent;

	public bool IsSystemNamed => m_index.IsSystemNamed;

	public abstract ConstraintType Type { get; }

	public IRelationalIndex AssociatedIndex => m_index;

	public string Name => m_index.Name;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractUniqueConstraint
	// =========================================================================================================


	public abstract T Accept<T>(IMetadataObjectVisitor<T> visitor);


	#endregion Methods

}
