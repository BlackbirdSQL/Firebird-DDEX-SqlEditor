/*
 *	Replica to expose the FirebirdClient ConnectionString as well as additional members
 *	to support SourceInformation, Root ObjectSelector and SqlServer style ConnectionInfo classes
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data;
using BlackbirdSql.Core.Ctl;
using FirebirdSql.Data.FirebirdClient;

using static BlackbirdSql.Core.Model.ModelConstants;


namespace BlackbirdSql.Core.Model;

// =========================================================================================================
//										ModelPropertySet Class
//
/// <summary>
/// Static PropertySet for the base class AbstractModelPropertyAgent for replicating the Firebird
/// ConnectionString class and additional members to support SourceInformation, Root ObjectSelector and
/// SqlServer style ConnectionInfo classes. Descendent AbstractModelPropertyAgent classes may or may not
/// have a separate PropertySet class dependent on the number of additional describers.
/// Refer to the <seealso cref="PropertySet"/> class for more information.
/// </summary>
// =========================================================================================================
public abstract class ModelPropertySet : CorePropertySet
{

	#region Constants and Statics


	public static int _EquivalencyDescriberCount = -1;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// PropertyAgent Descriptor/Parameter Properties.
	/// </summary>
	/// <remarks>
	/// Equivalency describers are tagged with *.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static readonly new Describer[] Describers =
		[
			new Describer(C_KeyRole, C_KeyFbRole, typeof(string), C_DefaultRole, true, false), // *
			new Describer(C_KeyDialect, C_KeyFbDialect, typeof(int), C_DefaultDialect, true, false), // *
			new Describer(C_KeyCharset, C_KeyFbCharset, typeof(string), C_DefaultCharset, true, false), // *
			new Describer(C_KeyNoDatabaseTriggers, C_KeyFbNoDatabaseTriggers, typeof(bool), C_DefaultNoDatabaseTriggers, true, true), // *
			new Describer(C_KeyPacketSize, C_KeyFbPacketSize, typeof(int), C_DefaultPacketSize, true),
			new Describer(C_KeyConnectionTimeout, C_KeyFbConnectionTimeout, typeof(int), C_DefaultConnectionTimeout, true),
			new Describer(C_KeyPooling, C_KeyFbPooling, typeof(bool), C_DefaultPooling, true),
			new Describer(C_KeyConnectionLifeTime, C_KeyFbConnectionLifeTime, typeof(int), C_DefaultConnectionLifeTime, true),
			new Describer(C_KeyMinPoolSize, C_KeyFbMinPoolSize, typeof(int), C_DefaultMinPoolSize, true),
			new Describer(C_KeyMaxPoolSize, C_KeyFbMaxPoolSize, typeof(int), C_DefaultMaxPoolSize, true),
			new Describer(C_KeyFetchSize, C_KeyFbFetchSize, typeof(int), C_DefaultFetchSize, true),
			new Describer(C_KeyIsolationLevel, C_KeyFbIsolationLevel, typeof(IsolationLevel), C_DefaultIsolationLevel, true),
			new Describer(C_KeyReturnRecordsAffected, C_KeyFbReturnRecordsAffected, typeof(bool), C_DefaultReturnRecordsAffected, true),
			new Describer(C_KeyEnlist, C_KeyFbEnlist, typeof(bool), C_DefaultEnlist, true),
			new Describer(C_KeyClientLibrary, C_KeyFbClientLibrary, typeof(string), C_DefaultClientLibrary, true),
			new Describer(C_KeyDbCachePages, C_KeyFbDbCachePages, typeof(int), C_DefaultDbCachePages, true),
			new Describer(C_KeyNoGarbageCollect, C_KeyFbNoGarbageCollect, typeof(bool), C_DefaultNoGarbageCollect, true),
			new Describer(C_KeyCompression, C_KeyFbCompression, typeof(bool), C_DefaultCompression, true),
			new Describer(C_KeyCryptKey, C_KeyFbCryptKey, typeof(byte[]), C_DefaultCryptKey, true),
			new Describer(C_KeyWireCrypt, C_KeyFbWireCrypt, typeof(FbWireCrypt), C_DefaultWireCrypt, true),
			new Describer(C_KeyApplicationName, C_KeyFbApplicationName, typeof(string), C_DefaultApplicationName, true),
			new Describer(C_KeyCommandTimeout, C_KeyFbCommandTimeout, typeof(int), C_DefaultCommandTimeout, true),
			new Describer(C_KeyParallelWorkers, C_KeyFbParallelWorkers, typeof(int), C_DefaultParallelWorkers, true),

			new Describer(C_KeyExClientVersion, typeof(Version), C_DefaultExClientVersion, false, false)
		];
	public static readonly new KeyValuePair<string, string>[] Synonyms =
		[
			StringPair("no triggers", C_KeyNoDatabaseTriggers),
			StringPair("nodbtriggers", C_KeyNoDatabaseTriggers),
			StringPair("no dbtriggers", C_KeyNoDatabaseTriggers),
			StringPair("no database triggers", C_KeyNoDatabaseTriggers),
			StringPair("timeout", C_KeyConnectionTimeout),
			StringPair("db cache pages", C_KeyDbCachePages),
			StringPair("cachepages", C_KeyDbCachePages),
			StringPair("pagebuffers", C_KeyDbCachePages),
			StringPair("page buffers", C_KeyDbCachePages),
			StringPair("wire compression", C_KeyCompression),
			StringPair("app", C_KeyApplicationName),
			StringPair("parallel", C_KeyParallelWorkers)
		];



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



	#endregion Static Methods


}
