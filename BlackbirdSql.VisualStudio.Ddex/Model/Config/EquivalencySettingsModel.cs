// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model.Config;
using BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;



namespace BlackbirdSql.VisualStudio.Ddex.Model.Config;


// =========================================================================================================
//										EquivalencySettingsModel Class
//
/// <summary>
/// Settings Model for Connection equivalency keys options
/// </summary>
// =========================================================================================================
public class EquivalencySettingsModel : AbstractSettingsModel<EquivalencySettingsModel>
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - EquivalencySettingsModel
	// ---------------------------------------------------------------------------------


	public EquivalencySettingsModel(IBTransientSettings transientSettings)
		: base(C_Package, C_Group, C_LivePrefix, transientSettings)
	{

	}


	public EquivalencySettingsModel() : this(null)
	{
	}


	#endregion Constructors / Destructors





	// =====================================================================================================
	#region Constants - EquivalencySettingsModel
	// =====================================================================================================


	private const string C_Package = "Ddex";
	private const string C_Group = "Equivalency";
	private const string C_LivePrefix = "DdexEquivalency";


	#endregion Constants




	// =====================================================================================================
	#region Property Accessors - EquivalencySettingsModel
	// =====================================================================================================


	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string CollectionName { get; } = "\\BlackbirdSql\\Ddex.EquivalencySettings";



	[GlobalizedCategory("OptionCategoryEquivalencyMandatory")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyDataSource")]
	[GlobalizedDescription("OptionDescriptionEquivalencyDataSource")]
	[TypeConverter(typeof(GlobalIncludedExcludedConverter))]
	[ReadOnly(true)]
	[DefaultValue(true)]
	public bool DataSource { get; set; } = true;

	[GlobalizedCategory("OptionCategoryEquivalencyMandatory")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyPort")]
	[GlobalizedDescription("OptionDescriptionEquivalencyPort")]
	[TypeConverter(typeof(GlobalIncludedExcludedConverter))]
	[ReadOnly(true)]
	[DefaultValue(true)]
	public bool Port { get; set; } = true;

	[GlobalizedCategory("OptionCategoryEquivalencyMandatory")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyDatabase")]
	[GlobalizedDescription("OptionDescriptionEquivalencyDatabase")]
	[TypeConverter(typeof(GlobalIncludedExcludedConverter))]
	[ReadOnly(true)]
	[DefaultValue(true)]
	public bool Database { get; set; } = true;

	[GlobalizedCategory("OptionCategoryEquivalencyMandatory")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyUserID")]
	[GlobalizedDescription("OptionDescriptionEquivalencyUserID")]
	[TypeConverter(typeof(GlobalIncludedExcludedConverter))]
	[ReadOnly(true)]
	[DefaultValue(true)]
	public bool UserID { get; set; } = true;

	[GlobalizedCategory("OptionCategoryEquivalencyMandatory")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyServerType")]
	[GlobalizedDescription("OptionDescriptionEquivalencyServerType")]
	[TypeConverter(typeof(GlobalIncludedExcludedConverter))]
	[ReadOnly(true)]
	[DefaultValue(true)]
	public bool ServerType { get; set; } = true;

	[GlobalizedCategory("OptionCategoryEquivalencyMandatory")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyRole")]
	[GlobalizedDescription("OptionDescriptionEquivalencyRole")]
	[TypeConverter(typeof(GlobalIncludedExcludedConverter))]
	[ReadOnly(true)]
	[DefaultValue(true)]
	public bool Role { get; set; } = true;

	[GlobalizedCategory("OptionCategoryEquivalencyMandatory")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyCharset")]
	[GlobalizedDescription("OptionDescriptionEquivalencyCharset")]
	[TypeConverter(typeof(GlobalIncludedExcludedConverter))]
	[ReadOnly(true)]
	[DefaultValue(true)]
	public bool Charset { get; set; } = true;

	[GlobalizedCategory("OptionCategoryEquivalencyMandatory")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyDialect")]
	[GlobalizedDescription("OptionDescriptionEquivalencyDialect")]
	[TypeConverter(typeof(GlobalIncludedExcludedConverter))]
	[ReadOnly(true)]
	[DefaultValue(true)]
	public bool Dialect { get; set; } = true;

	[GlobalizedCategory("OptionCategoryEquivalencyMandatory")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyNoDatabaseTriggers")]
	[GlobalizedDescription("OptionDescriptionEquivalencyNoDatabaseTriggers")]
	[TypeConverter(typeof(GlobalIncludedExcludedConverter))]
	[ReadOnly(true)]
	[DefaultValue(true)]
	public bool NoDatabaseTriggers { get; set; } = true;




	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyPacketSize")]
	[GlobalizedDescription("OptionDescriptionEquivalencyPacketSize")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool PacketSize { get; set; } = false;


	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyConnectionTimeout")]
	[GlobalizedDescription("OptionDescriptionEquivalencyConnectionTimeout")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool ConnectionTimeout { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyPooling")]
	[GlobalizedDescription("OptionDescriptionEquivalencyPooling")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool Pooling { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyConnectionLifeTime")]
	[GlobalizedDescription("OptionDescriptionEquivalencyConnectionLifeTime")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool ConnectionLifeTime { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyMinPoolSize")]
	[GlobalizedDescription("OptionDescriptionEquivalencyMinPoolSize")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool MinPoolSize { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyMaxPoolSize")]
	[GlobalizedDescription("OptionDescriptionEquivalencyMaxPoolSize")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool MaxPoolSize { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyFetchSize")]
	[GlobalizedDescription("OptionDescriptionEquivalencyFetchSize")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool FetchSize { get; set; } = false;


	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyIsolationLevel")]
	[GlobalizedDescription("OptionDescriptionEquivalencyIsolationLevel")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool IsolationLevel { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyReturnRecordsAffected")]
	[GlobalizedDescription("OptionDescriptionEquivalencyReturnRecordsAffected")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool ReturnRecordsAffected { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyEnlist")]
	[GlobalizedDescription("OptionDescriptionEquivalencyEnlist")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool Enlist { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyClientLibrary")]
	[GlobalizedDescription("OptionDescriptionEquivalencyClientLibrary")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool ClientLibrary { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyDbCachePages")]
	[GlobalizedDescription("OptionDescriptionEquivalencyDbCachePages")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool DbCachePages { get; set; } = false;


	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyNoGarbageCollect")]
	[GlobalizedDescription("OptionDescriptionEquivalencyNoGarbageCollect")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool NoGarbageCollect { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyCompression")]
	[GlobalizedDescription("OptionDescriptionEquivalencyCompression")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool Compression { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyCryptKey")]
	[GlobalizedDescription("OptionDescriptionEquivalencyCryptKey")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool CryptKey { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyWireCrypt")]
	[GlobalizedDescription("OptionDescriptionEquivalencyWireCrypt")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool WireCrypt { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyApplicationName")]
	[GlobalizedDescription("OptionDescriptionEquivalencyApplicationName")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(true)]
	public bool ApplicationName { get; set; } = true;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyCommandTimeout")]
	[GlobalizedDescription("OptionDescriptionEquivalencyCommandTimeout")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool CommandTimeout { get; set; } = false;

	[GlobalizedCategory("OptionCategoryEquivalencyOptional")]
	[GlobalizedDisplayName("OptionDisplayEquivalencyParallelWorkers")]
	[GlobalizedDescription("OptionDescriptionEquivalencyParallelWorkers")]
	[TypeConverter(typeof(GlobalIncludeExcludeConverter))]
	[DefaultValue(false)]
	public bool ParallelWorkers { get; set; } = false;



	#endregion Property Accessors

}
