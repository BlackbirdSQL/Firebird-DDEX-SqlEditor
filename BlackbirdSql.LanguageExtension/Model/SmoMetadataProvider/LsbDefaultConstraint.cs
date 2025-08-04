// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.DefaultConstraint

using System;
using BlackbirdSql.LanguageExtension.Interfaces;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbDefaultConstraint Class
//
/// <summary>
/// Impersonation of an SQL Server Smo DefaultConstraint for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbDefaultConstraint : IDefaultConstraint, IMetadataObject, IBsSmoDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbDefaultConstraint
	// ---------------------------------------------------------------------------------


	public LsbDefaultConstraint(IColumn parent, Microsoft.SqlServer.Management.Smo.DefaultConstraint smoDefaultConstraint)
	{
		this.parent = parent;
		this.smoDefaultConstraint = smoDefaultConstraint;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbDefaultConstraint
	// =========================================================================================================


	private readonly IColumn parent;

	private readonly Microsoft.SqlServer.Management.Smo.DefaultConstraint smoDefaultConstraint;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbDefaultConstraint
	// =========================================================================================================


	public IColumn Parent => parent;

	public bool IsSystemNamed
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)smoDefaultConstraint, "IsSystemNamed", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public string Text => smoDefaultConstraint.Text;

	public string Name => smoDefaultConstraint.Name;

	public SqlSmoObject SmoObject => smoDefaultConstraint;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbDefaultConstraint
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
