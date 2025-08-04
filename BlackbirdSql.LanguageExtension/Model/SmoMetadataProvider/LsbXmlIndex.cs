// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.XmlIndex

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbXmlIndex Class
//
/// <summary>
/// Impersonation of an SQL Server Smo XmlIndex for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbXmlIndex : LsbIndex, IXmlIndex, IIndex, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbXmlIndex
	// ---------------------------------------------------------------------------------


	public LsbXmlIndex(IDatabaseTable parent, Microsoft.SqlServer.Management.Smo.Index smoIndex)
		: base(parent, smoIndex)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - LsbXmlIndex
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
