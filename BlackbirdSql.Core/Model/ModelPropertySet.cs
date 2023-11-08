/*
 *	Replica to expose the FirebirdClient ConnectionString as well as additional members
 *	to support SourceInformation, Root ObjectSelector and SqlServer style ConnectionInfo classes
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Properties;
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
	/// FbStringBuilder Descriptor/Parameter Properties.
	/// </summary>
	/// <remarks>
	/// Equivalency describers are tagged with *.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static readonly new Describer[] Describers
		= new Describer[23]
	{
		new Describer("PacketSize", C_KeyPacketSize, typeof(int), C_DefaultPacketSize, true),
		new Describer("Role", C_KeyRoleName, typeof(string), C_DefaultRoleName, true, false, true, false, true), // *
		new Describer("Dialect", C_KeyDialect, typeof(int), C_DefaultDialect, true, false, true, false, true), // *
		new Describer("Charset", C_KeyCharacterSet, typeof(string), C_DefaultCharacterSet, true, false, true, false, true), // *
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
		new Describer("NoDatabaseTriggers", C_KeyNoDbTriggers, typeof(bool), C_DefaultNoDbTriggers, true, true, true, false, true), // *
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
	public static readonly Describer[] RootNodeDescribers
		= new Describer[1]
	{
		new Describer(C_KeyNodeDatasetKey, typeof(string))
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Source Information column describers with Parameter Property Descriptor name in
	/// Parameter field.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static readonly Describer[] ConnectionNodeDescribers
		= new Describer[14]
	{
		new Describer(C_KeyNodeDatasetKey,  typeof(string)),
		new Describer(C_KeyNodeDataSource, "DataSource", typeof(string)),
		new Describer(C_KeyNodePort, "Port", typeof(int), CoreConstants.C_DefaultPortNumber),
		new Describer(C_KeyNodeServerType, "ServerType", typeof(EnDbServerType), CoreConstants.C_DefaultServerType),
		new Describer(C_KeyNodeDatabase, "Database", typeof(string)),
		new Describer(C_KeyNodeDisplayMember,  typeof(string)),
		new Describer(C_KeyNodeUserId, "UserID", typeof(string), CoreConstants.C_DefaultUserId),
		new Describer(C_KeyNodePassword, "Password", typeof(string), CoreConstants.C_DefaultPassword, false, true, false),
		new Describer(C_KeyNodeRole, "Role", typeof(string)),
		new Describer(C_KeyNodeCharset, "Charset", typeof(string)),
		new Describer(C_KeyNodeDialect, "Dialect", typeof(int)),
		new Describer(C_KeyNodeNoDbTriggers, "NoDatabaseTriggers", typeof(bool)),
		new Describer(C_KeyNodeMemoryUsage, typeof(string), C_DefaultNodeMemoryUsage ),
		new Describer(C_KeyNodeActiveUsers, typeof(int), C_DefaultNodeActiveUsers )
	};



	#endregion Constants and Statics





	// =========================================================================================================
	#region Static Methods
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether or not connection property/parameter objects are equivalent
	/// </summary>
	/// <remarks>
	/// We consider connections equivalent if they will produce the same results. The connection properties
	/// that determine this equivalency are defined in <see cref="CoreProperties.EquivalencyKeys"/>.
	/// </remarks>
	public static bool AreEquivalent(DbConnectionStringBuilder csb1, DbConnectionStringBuilder csb2)
	{
		// Tracer.Trace(typeof(ModelPropertySet), "AreEquivalent()");

		int equivalencyValueCount = 0;
		int equivalencyKeyCount = ModelPropertySet.EquivalencyDescriberCount;
		object value1, value2;
		Describer describer;

		try
		{
			// Keep it simple. Loop thorugh each connection string and compare
			// If it's not in the other or null use default

			foreach (KeyValuePair<string, object> param in csb1)
			{
				// If all equivalency keys have been checked, break
				if (equivalencyValueCount == equivalencyKeyCount)
					break;

				// Get the correct key for the parameter in connection 1
				if ((describer = ModelPropertySet.RecursiveGetSynonymDescriber(param.Key)) == null)
				{
					ArgumentException ex = new(Resources.ExceptionParameterDescriberNotFound.FmtRes(param.Key));
					Diag.Dug(ex);
					throw ex;
				}

				// Exclude non-applicable connection values.
				// Typically we may require a password and if it's already in, for example, the SE we have rights to it.
				// There would be no point ignoring that password just because some spurious value differs. For example 'Connection Lifetime'.

				if (!describer.IsEquivalency)
					continue;

				equivalencyValueCount++;

				// For both connections we set the value to default if it's null or doesn't exist
				if (param.Value != null)
					value1 = param.Value;
				else
					value1 = describer.DefaultValue;

				// We can't do a straight lookup on the second string because it may be a synonym so we have to loop
				// through the parameters, find the real key, and use that

				value2 = FindKeyValueInConnection(describer, csb2);

				value2 ??= describer.DefaultValue;

				if (!AreEquivalent(describer.DerivedConnectionParameter, value1, value2))
				{
					Tracer.Trace(typeof(ModelPropertySet), "AreEquivalent(IDictionary, IDictionary)",
						"Connection parameter '{0}' mismatch: '{1}' : '{2}.",
						param.Key, value1 != null ? value1.ToString() : "null", value2 != null ? value2.ToString() : "null");
					return false;
				}
			}

			if (equivalencyValueCount < equivalencyKeyCount)
			{

				foreach (KeyValuePair<string, object> param in csb2)
				{
					// If all equivalency keys have been checked, break
					if (equivalencyValueCount == equivalencyKeyCount)
						break;

					// Get the correct key for the parameter in connection 2
					if ((describer = ModelPropertySet.RecursiveGetSynonymDescriber(param.Key)) == null)
					{
						ArgumentException ex = new($"Could not locate Describer for connection parameter '{param.Key}'.");
						Diag.Dug(ex);
						throw ex;
					}



					// Exclude non-applicable connection values.
					// Typically we may require a password and if it's already in, for example, the SE we have rights to it.
					// There would be no point ignoring that password just because some spurious value differs. For example 'Connection Lifetime'. 

					if (!describer.IsEquivalency)
						continue;

					equivalencyValueCount++;

					// For both connections we set the value to default if it's null or doesn't exist
					if (param.Value != null)
						value2 = param.Value;
					else
						value2 = describer.DefaultValue;

					// We can't do a straight lookup on the first connection because it may be a synonym so we have to loop
					// through the parameters, find the real key, and use that
					value1 = FindKeyValueInConnection(describer, csb1);

					value1 ??= describer.DefaultValue;

					if (!AreEquivalent(describer.DerivedConnectionParameter, value2, value1))
					{
						Tracer.Trace(typeof(ModelPropertySet), "AreEquivalent(IDictionary, IDictionary)",
							"Connection2 parameter '{0}' mismatch: '{1}' : '{2}.",
							param.Key, value2 != null ? value2.ToString() : "null", value1 != null ? value1.ToString() : "null");
						return false;
					}
					// Diag.Trace("Connection2 parameter '" + key + "' equivalent: '" + (value2 != null ? value2.ToString() : "null") + "' : '" + (value1 != null ? value1.ToString() : "null"));
				}
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		// Tracer.Trace(typeof(TConnectionEquivalencyComparer),
		// 	"TConnectionEquivalencyComparer.AreEquivalent(IDictionary, IDictionary)", "Connections are equivalent");

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an equivalency comparison of to values of the connection
	/// property/parameter 'key'.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value1"></param>
	/// <param name="value2"></param>
	/// <returns>true if equivalent else false</returns>
	// ---------------------------------------------------------------------------------
	protected static bool AreEquivalent(string key, object value1, object value2)
	{
		// Diag.Trace();
		string text1 = value1 as string;
		string text2 = value2 as string;

		/*
		if (key == "Data Source")
		{
			if (!string.Equals(text1, text2, StringComparison.OrdinalIgnoreCase))
			{
				if (text1 != null)
					text1 = StandardizeDataSource(text1);

				if (text2 != null)
					text2 = StandardizeDataSource(text2);

			}
		}
		*/

		if (!string.Equals(text1, text2, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		return true;
	}



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



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds the value of the connection property/parameter 'key' in a connection
	/// properties list given that the property key used in the list may be a synonym.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="connectionProperties"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	// ---------------------------------------------------------------------------------
	private static object FindKeyValueInConnection(Describer describer, DbConnectionStringBuilder csb)
	{
		// First try for matching keys
		object value;

		if (describer.ConnectionParameter != null)
		{
			if (csb.TryGetValue(describer.ConnectionParameter, out value))
				return value;
		}

		if (describer.ConnectionParameter == null || describer.Name != describer.ConnectionParameter)
		{
			if (csb.TryGetValue(describer.Name, out value))
				return value;
		}


		Describer connectionDescriber;


		foreach (KeyValuePair<string, object> parameter in csb)
		{
			if ((connectionDescriber = ModelPropertySet.RecursiveGetSynonymDescriber(parameter.Key)) == null)
			{
				ArgumentException ex = new(Resources.ExceptionParameterDescriberNotFound.FmtRes(parameter.Key));
				Diag.Dug(ex);
				throw ex;
			}


			if (connectionDescriber.Name == describer.Name)
				return parameter.Value;
		}

		return null;
	}



	/// <summary>
	/// Gets the describer for a root or connection node column name.
	/// </summary>
	public static Describer GetConnectionNodeDescriber(string name)
	{
		return Array.Find(ConnectionNodeDescribers,
			(obj) => obj.Name == name);
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
