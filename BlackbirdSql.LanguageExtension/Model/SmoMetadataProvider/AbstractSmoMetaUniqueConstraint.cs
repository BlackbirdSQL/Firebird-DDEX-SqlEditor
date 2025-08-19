// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.UniqueConstraintBase

using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											AbstractSmoMetaUniqueConstraint Class
//
/// <summary>
/// Impersonation of an SQL Server Smo UniqueConstraintBase for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractSmoMetaUniqueConstraint : IUniqueConstraintBase, IConstraint, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractSmoMetaUniqueConstraint
	// ---------------------------------------------------------------------------------


	protected AbstractSmoMetaUniqueConstraint(IDatabaseTable parent, IRelationalIndex index)
	{
		_Parent = parent;
		_Index = index;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractSmoMetaUniqueConstraint
	// =========================================================================================================


	private readonly IDatabaseTable _Parent;

	private readonly IRelationalIndex _Index;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractSmoMetaUniqueConstraint
	// =========================================================================================================


	public ITabular Parent => _Parent;

	public bool IsSystemNamed => _Index.IsSystemNamed;

	public abstract ConstraintType Type { get; }

	public IRelationalIndex AssociatedIndex => _Index;

	public string Name => _Index.Name;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractSmoMetaUniqueConstraint
	// =========================================================================================================


	public abstract T Accept<T>(IMetadataObjectVisitor<T> visitor);


	#endregion Methods

}
