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
//											SmoMetaAsymmetricKey Class
//
/// <summary>
/// Impersonation of an SQL Server Smo AsymmetricKey for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaAsymmetricKey : AbstractSmoMetaDatabaseOwnedObject<Microsoft.SqlServer.Management.Smo.AsymmetricKey>, IAsymmetricKey, IDatabaseOwnedObject, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaAsymmetricKey
	// ---------------------------------------------------------------------------------


	public SmoMetaAsymmetricKey(Microsoft.SqlServer.Management.Smo.AsymmetricKey smoMetadataObject, SmoMetaDatabase parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property accessors - SmoMetaAsymmetricKey
	// =========================================================================================================


	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => false;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaAsymmetricKey
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
	#region									Nested types - SmoMetaAsymmetricKey
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: AsymmetricKeyCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : SmoMetaDatabase.CollectionHelperI<IAsymmetricKey, AsymmetricKey>
	{
		public CollectionHelperI(SmoMetaDatabase database)
			: base(database)
		{
		}
		protected override IMetadataListI<AsymmetricKey> RetrieveSmoMetadataList()
		{
			return new SmoCollectionMetadataListI<AsymmetricKey>(_Database.Server, _Database.SmoMetadataObject.AsymmetricKeys);
		}

		protected override IMutableMetadataCollection<IAsymmetricKey> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo)
		{
			return new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.AsymmetricKeyCollection(initialCapacity, collationInfo);
		}

		protected override IAsymmetricKey CreateMetadataObject(AsymmetricKey smoObject)
		{
			return new SmoMetaAsymmetricKey(smoObject, _Database);
		}
	}


	#endregion	Nested types

}
