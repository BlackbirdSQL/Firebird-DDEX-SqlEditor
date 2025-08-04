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
//											LsbCheckConstraint Class
//
/// <summary>
/// Impersonation of an SQL Server Smo CheckConstraint for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbCheckConstraint : ICheckConstraint, IConstraint, IMetadataObject, IBsSmoDatabaseObject
{
	public LsbCheckConstraint(IDatabaseTable parent, Check smoCheckConstraint)
	{
		m_parent = parent;
		m_smoCheckConstraint = smoCheckConstraint;
	}



	private readonly IDatabaseTable m_parent;

	private readonly Check m_smoCheckConstraint;

	public ITabular Parent => m_parent;

	public bool IsSystemNamed => m_smoCheckConstraint.IsSystemNamed;

	public ConstraintType Type => ConstraintType.Check;

	public bool IsEnabled => m_smoCheckConstraint.IsEnabled;

	public bool IsChecked => m_smoCheckConstraint.IsChecked;

	public bool NotForReplication => m_smoCheckConstraint.NotForReplication;

	public string Text => m_smoCheckConstraint.Text;

	public string Name => m_smoCheckConstraint.Name;

	public SqlSmoObject SmoObject => m_smoCheckConstraint;


	public T Accept<T>(IMetadataObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}
}
