// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Certificate
using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using static BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider.LsbDatabase;
using static Microsoft.SqlServer.Management.SqlParser.MetadataProvider.MetadataProviderUtils.Names;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbCertificate Class
//
/// <summary>
/// Impersonation of an SQL Server Smo AsymmetricKey for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbCertificate : LsbDatabaseOwnedObject<Microsoft.SqlServer.Management.Smo.Certificate>, ICertificate, IDatabaseOwnedObject, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbCertificate
	// ---------------------------------------------------------------------------------


	public LsbCertificate(Microsoft.SqlServer.Management.Smo.Certificate smoMetadataObject, LsbDatabase parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property accessors - LsbCertificate
	// =========================================================================================================


	public override int Id => m_smoMetadataObject.ID;

	public override bool IsSystemObject => false;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbCertificate
	// =========================================================================================================


	public override T Accept<T>(IDatabaseOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - LsbCertificate
	// =========================================================================================================

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Database.CertificateCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : CollectionHelperI<ICertificate, Certificate>
	{
		public CollectionHelperI(LsbDatabase database)
		: base(database)
		{
		}

		protected override IMetadataListI<Certificate> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<Certificate>(m_database.Server, m_database.SmoMetadataObject.Certificates);
		}

		protected override IMutableMetadataCollection<ICertificate> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.CertificateCollection(initialCapacity, collationInfo);
		}

		protected override ICertificate CreateMetadataObject(Certificate smoObject)
		{
			return new LsbCertificate(smoObject, m_database);
		}
	}


	#endregion Nested types

}
