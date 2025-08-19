// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.CheckConstraint

using System;
using BlackbirdSql.LanguageExtension.Interfaces;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											SmoMetaCheckConstraint Class
//
/// <summary>
/// Impersonation of an SQL Server Smo CheckConstraint for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaCheckConstraint : ICheckConstraint, IConstraint, IMetadataObject, IBsSmoDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaCheckConstraint
	// ---------------------------------------------------------------------------------


	public SmoMetaCheckConstraint(IDatabaseTable parent, Check smoCheckConstraint)
	{
		_Parent = parent;
		_SmoCheckConstraint = smoCheckConstraint;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaCheckConstraint
	// =========================================================================================================


	private readonly IDatabaseTable _Parent;

	private readonly Check _SmoCheckConstraint;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaCheckConstraint
	// =========================================================================================================


	public ITabular Parent => _Parent;

	public bool IsSystemNamed => _SmoCheckConstraint.IsSystemNamed;

	public ConstraintType Type => ConstraintType.Check;

	public bool IsEnabled => _SmoCheckConstraint.IsEnabled;

	public bool IsChecked => _SmoCheckConstraint.IsChecked;

	public bool NotForReplication => _SmoCheckConstraint.NotForReplication;

	public string Text => _SmoCheckConstraint.Text;

	public string Name => _SmoCheckConstraint.Name;

	public SqlSmoObject SmoObject => _SmoCheckConstraint;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaCheckConstraint
	// =========================================================================================================


	public T Accept<T>(IMetadataObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}


	#endregion Methods

}
