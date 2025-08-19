// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.PrimaryKeyConstraint

using System;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											SmoMetaPrimaryKeyConstraint Class
//
/// <summary>
/// Impersonation of an SQL Server Smo PrimaryKeyConstraint for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaPrimaryKeyConstraint : AbstractSmoMetaUniqueConstraint, IPrimaryKeyConstraint, IUniqueConstraintBase, IConstraint, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaPrimaryKeyConstraint
	// ---------------------------------------------------------------------------------


	public SmoMetaPrimaryKeyConstraint(IDatabaseTable parent, IRelationalIndex index)
		: base(parent, index)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property accessors - SmoMetaPrimaryKeyConstraint
	// =========================================================================================================


	public override ConstraintType Type => ConstraintType.PrimaryKey;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaPrimaryKeyConstraint
	// =========================================================================================================


	public override T Accept<T>(IMetadataObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}


	#endregion Methods

}
