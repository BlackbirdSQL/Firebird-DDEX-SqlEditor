// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.AsymmetricKey

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using static Microsoft.SqlServer.Management.SqlParser.MetadataProvider.MetadataProviderUtils.Names;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbAsymmetricKey Class
//
/// <summary>
/// Impersonation of an SQL Server Smo AsymmetricKey for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbAsymmetricKey : LsbDatabaseOwnedObject<Microsoft.SqlServer.Management.Smo.AsymmetricKey>, IAsymmetricKey, IDatabaseOwnedObject, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbAsymmetricKey
	// ---------------------------------------------------------------------------------


	public LsbAsymmetricKey(Microsoft.SqlServer.Management.Smo.AsymmetricKey smoMetadataObject, LsbDatabase parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property accessors - LsbAsymmetricKey
	// =========================================================================================================


	public override int Id => m_smoMetadataObject.ID;

	public override bool IsSystemObject => false;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbAsymmetricKey
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
	#region									Nested types - LsbAsymmetricKey
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: AsymmetricKeyCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : LsbDatabase.CollectionHelperI<IAsymmetricKey, AsymmetricKey>
	{
		public CollectionHelperI(LsbDatabase database)
			: base(database)
		{
		}
		protected override IMetadataListI<AsymmetricKey> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<AsymmetricKey>(m_database.Server, m_database.SmoMetadataObject.AsymmetricKeys);
		}

		protected override IMutableMetadataCollection<IAsymmetricKey> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.AsymmetricKeyCollection(initialCapacity, collationInfo);
		}

		protected override IAsymmetricKey CreateMetadataObject(AsymmetricKey smoObject)
		{
			return new LsbAsymmetricKey(smoObject, m_database);
		}
	}


	#endregion	Nested types

}
