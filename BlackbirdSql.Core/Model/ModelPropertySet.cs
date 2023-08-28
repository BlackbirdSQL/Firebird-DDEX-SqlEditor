/*
 *	Replica to expose the FirebirdClient ConnectionString as well as additional members
 *	to support SourceInformation, Root ObjectSelector and SqlServer style ConnectionInfo classes
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Enums;

using FirebirdSql.Data.FirebirdClient;

using static BlackbirdSql.Core.Model.ModelConstants;




namespace BlackbirdSql.Core.Model;


// =========================================================================================================
//										ModelPropertySet Class
//
/// <summary>
/// PropertySet for the base class (AbstractModelPropertyAgent) for replicating the Firebird ConnectionString
/// class and additional members to support SourceInformation, Root ObjectSelector and SqlServer style
/// ConnectionInfo classes. Descendent AbstractModelPropertyAgent classes may or may not have a seperate
/// PropertySet class dependent on the number of additional describers.
/// </summary>
// =========================================================================================================
public abstract class ModelPropertySet : CorePropertySet
{

	#region Constants and Statics


	public static int _EquivalencyDescriberCount = -1;

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// FbStringBuilder Descriptor/Parameter Properties.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static readonly new Describer[] Describers
		= new Describer[23]
	{
		new Describer("PacketSize", C_KeyPacketSize, typeof(int), C_DefaultPacketSize, true),
		new Describer("Role", C_KeyRoleName, typeof(string), C_DefaultRoleName, true, false, true, false, true),
		new Describer("Dialect", C_KeyDialect, typeof(int), C_DefaultDialect, true, false, true, false, true),
		new Describer("Charset", C_KeyCharacterSet, typeof(string), C_DefaultCharacterSet, true, false, true, false, true),
		new Describer("ConnectionTimeout", C_KeyConnectionTimeout, typeof(int), C_DefaultConnectionTimeout, true),
		new Describer("Pooling", C_KeyPooling, typeof(bool), C_DefaultPooling, true),
		new Describer("ConnectionLifeTime", C_KeyConnectionLifetime, typeof(int), C_DefaultConnectionLifetime, true),
		new Describer("MinPoolSize", C_KeyMinPoolSize, typeof(int), C_DefaultMinPoolSize, true),
		new Describer("MaxPoolSize", C_KeyMaxPoolSize, typeof(int), C_DefaultMaxPoolSize, true),
		new Describer("FetchSize", C_KeyFetchSize, typeof(int), C_DefaultFetchSize, true),
		new Describer("IsolationLevel", C_KeyIsolationLevel, typeof(IsolationLevel), C_DefaultIsolationLevel, true),
		new Describer("ReturnRecordsAffected", C_KeyRecordsAffected, typeof(bool), C_DefaultRecordsAffected, true),
		new Describer("Enlist", C_KeyEnlist, typeof(bool), C_DefaultEnlist, true),
		new Describer("ClientLibrary", C_KeyClientLibrary, typeof(string), C_DefaultClientLibrary, true),
		new Describer("DbCachePages", C_KeyDbCachePages, typeof(int), C_DefaultDbCachePages, true),
		new Describer("NoDatabaseTriggers", C_KeyNoDbTriggers, typeof(bool), C_DefaultNoDbTriggers, true, true, true, false, true),
		new Describer("NoGarbageCollect", C_KeyNoGarbageCollect, typeof(bool), C_DefaultNoGarbageCollect, true),
		new Describer("Compression", C_KeyCompression, typeof(bool), C_DefaultCompression, true),
		new Describer("CryptKey", C_KeyCryptKey, typeof(byte[]), C_DefaultCryptKey, true),
		new Describer("WireCrypt", C_KeyWireCrypt, typeof(FbWireCrypt), C_DefaultWireCrypt, true),
		new Describer("ApplicationName", C_KeyApplicationName, typeof(string), C_DefaultApplicationName, true),
		new Describer("CommandTimeout", C_KeyCommandTimeout, typeof(int), C_DefaultCommandTimeout, true),
		new Describer("ParallelWorkers", C_KeyParallelWorkers, typeof(int), C_DefaultParallelWorkers, true),
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Universal lookup of all possible synonyms of the DbConnectionString property
	/// descriptors.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static readonly new KeyValuePair<string, string>[] Synonyms
		= new KeyValuePair<string, string>[11]
	{
		StringPair( "timeout", "ConnectionTimeout" ),
		StringPair( "cachepages", "DbCachePages" ),
		StringPair( "pagebuffers", "DbCachePages" ),
		StringPair( "page buffers", "DbCachePages" ),
		StringPair( "nodbtriggers", "NoDatabaseTriggers" ),
		StringPair( "no dbtriggers", "NoDatabaseTriggers" ),
		StringPair( "no database triggers", "NoDatabaseTriggers" ),
		StringPair( "wire compression", "Compression" ),
		StringPair( "wirecrypt", "CryptKey" ),
		StringPair( "app", "ApplicationName" ),
		StringPair( "parallel", "ParallelWorkers" )
};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Source Information column describers with Parameter Property Descriptor name in
	/// Parameter field.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static readonly Describer[] SourceInformationDescribers
		= new Describer[11]
	{
		new Describer(C_KeySIDataSourceName, "DataSource", typeof(string)),
		new Describer(C_KeySIDataSourceProduct, typeof(string)),
		new Describer(C_KeySIDataSourceVersion, typeof(string)), 

		// SourceInformation additional titlecased connection properties for Root
		new Describer(C_KeySICatalog, "Database", typeof(string)),
		new Describer(C_KeySIPortNumber, "Port", typeof(int), CoreConstants.C_DefaultPortNumber),
		new Describer(C_KeySIServerType, "ServerType" , typeof(EnDbServerType), CoreConstants.C_DefaultServerType),
		new Describer(C_KeySIUserId, "UserID", typeof(string)),
		new Describer(C_KeySIPassword, "Password", typeof(string)),

		// SourceInformation additional connection derived properties for Root
		new Describer( C_KeySIDataset,  typeof(string)),

		new Describer( C_KeySIMemoryUsage, typeof(int), C_DefaultSIMemoryUsage ),
		new Describer( C_KeySIActiveUsers, typeof(int), C_DefaultSIActiveUsers )
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Universal lookup of all possible synonyms of Source Information specific column
	/// names.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static readonly KeyValuePair<string, string>[] SourceInformationSynonyms
		= new KeyValuePair<string, string>[2]
	{
		StringPair( "DataSourceProductName", C_KeySIDataSourceProduct ), // FbMetaData
		StringPair( "DataSourceProductVersion", C_KeySIDataSourceVersion ), // FbMetaData
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Translation table for the Root object from the DataSourceInformation schema
	/// column names.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static readonly KeyValuePair<string, string>[] RootTranslations = new KeyValuePair<string, string>[2]
	{
		StringPair( C_KeySIDataSourceName, C_KeyRootDataSourceName ),
		StringPair( C_KeySIDataset, C_KeyRootDataset )
	};


	#endregion Constants and Statics





	// =========================================================================================================
	#region Static Methods
	// =========================================================================================================

	public static new void CreateAndPopulatePropertySetFromStatic(DescriberDictionary describers)
	{
		CorePropertySet.CreateAndPopulatePropertySetFromStatic(describers);

		describers.AddRange(Describers);
		describers.AddSynonyms(Synonyms);
	}



	public static int EquivalencyDescriberCount
	{
		get
		{
			if (_EquivalencyDescriberCount != -1)
				return _EquivalencyDescriberCount;


			int count = 0;

			foreach (Describer describer in CorePropertySet.Describers)
			{
				if (describer.IsEquivalency)
					count++;
			}


			foreach (Describer describer in Describers)
			{
				if (describer.IsEquivalency)
					count++;
			}

			_EquivalencyDescriberCount = count;

			return _EquivalencyDescriberCount;
		}
	}



	/// <summary>
	/// Get the describer for a source information column name.
	/// </summary>
	public static Describer GetSourceInformationDescriber(string name)
	{
		return Array.Find(SourceInformationDescribers,
			(obj) => obj.Name == name);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a Source Information column's type given it's column name.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Type GetSourceInformationType(string name)
	{
		Describer describer = GetSourceInformationDescriber(name);

		if (describer == null)
			return null;

		return describer.PropertyType;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the source information column name given a schema column.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Describer GetSchemaColumnSourceInformationDescriber(string name)
	{
		KeyValuePair<string, string> pair = Array.Find(SourceInformationSynonyms,
			(obj) => obj.Key.Equals(name, StringComparison.OrdinalIgnoreCase));

		if (pair.Key != null)
			return GetSourceInformationDescriber(pair.Value);

		return GetSourceInformationDescriber(name);
	}



	public static Describer RecursiveGetDescriber(string name)
	{
		Describer describer = Array.Find(CorePropertySet.Describers,
			(obj) => obj.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

		if (describer != null)
			return describer;

		return Array.Find(Describers,
			(obj) => obj.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
	}



	public static Describer RecursiveGetSynonymDescriber(string name)
	{
		Describer describer = Array.Find(CorePropertySet.Describers,
			(obj) => obj.SynonymMatches(name));

		if (describer != null)
			return describer;

		describer = Array.Find(Describers,
			(obj) => obj.SynonymMatches(name));

		if (describer != null)
			return describer;

		KeyValuePair<string, string> pair = Array.Find(CorePropertySet.Synonyms,
			(obj) => obj.Key.Equals(name, StringComparison.OrdinalIgnoreCase));

		if (pair.Key != null)
		{
			describer = Array.Find(CorePropertySet.Describers,
			(obj) => obj.SynonymMatches(pair.Value));

			if (describer != null)
				return describer;
		}


		pair = Array.Find(Synonyms,
			(obj) => obj.Key.Equals(name, StringComparison.OrdinalIgnoreCase));

		if (pair.Key == null)
			return null;

		describer = Array.Find(Describers,
			(obj) => obj.SynonymMatches(pair.Value));

		return describer;
	}



	#endregion Static Methods


}
