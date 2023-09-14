/*
 *	Replica to expose the FirebirdClient ConnectionString as well as additional members
 *	to support SourceInformation, Root ObjectSelector and SqlServer style ConnectionInfo classes
 */

// $OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data.Common;

using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;

using static BlackbirdSql.Core.Ctl.CoreConstants;


namespace BlackbirdSql.Core.Ctl;

// =========================================================================================================
//										CorePropertySet Class
//
/// <summary>
/// PropertySet's are utilized by an <see cref="IBPropertyAgent"/> class when it is the initiating instance
/// (Initiator), ie. the final descendent child class instance in the instance lineage. A class instance can
/// simultaneousy be an Initiator and a sub-class instance.
/// Each class in the IBPropertyAgent hierarchy must provide for it's own static private cumulative
/// PropertySet through the static <see cref="AbstractPropertyAgent.CreateAndPopulatePropertySet()"/>.
/// Descendents are provided a replica of their parent's cumulative static private property set, to which
/// they can then add their own custom properties.
/// Descendent <see cref="AbstractPropertyAgent"/> classes may or may not have a separate PropertySet class
/// dependent on the number of additional descriptors.
/// </summary>
/// <remarks>
/// PropertySet Accessors:
/// 1. Synonyms <see cref="IDictionary{string, string}"/>: contains the synonyms of all Primary
///		Parameter Keys or non-parameter properties. A Parameter is the primary key used to construct the
///		property string or a <see cref="DbConnectionStringBuilder"/> object. Mutable.
/// 2. ParameterDescriptors KeyValuePair<string, string> array: Contains a reverse lookup of Parameters to
///		Descriptor Properties or PropertyName to self for non-parameter properties. Immutable.
///	3. AdvancedOptions string array: An immutable list all Parameters that will not appear in a
///		property or connection dialog but would be available under 'Advanced' or 'Additional' options.
///	4. ExternalOptions <see cref="List{string}"/>: A mutable list of properties that are not Parameters.
///	5. DefaultValues <see cref="IDictionary{string, object}"/>: A mutable dictionary of Parameters' and
///		non-parameter properties' default values.
///	6. PropertyTypes <see cref="IDictionary{string, Type}"/>: A mutable dictionary of Parameters' and
///		non-parameter properties' Types.
///	7. EquivalencyKeys string array: An immutable list of Parameters that are checked for equivalency for
///		two <see cref="IBPropertyAgent"/> objects to be considered functionally equivalent.
///		a derived property string or <see cref="DbConnectionStringBuilder"/> object will not appear in a
///		property or connection dialog but would be available under 'Advanced' or 'Additional' options.
///	8. PublicMandatoryKeys string array: List of Parameters required to be able to successfully build an
///		safe property string or <see cref="DbConnectionStringBuilder"/> object.
///	8. ProtectedMandatoryKeys string array: List of Parameters required to be able to successfully build an
///		unsafe property string or <see cref="DbConnectionStringBuilder"/> object.
/// </remarks>
// =========================================================================================================
public abstract class CorePropertySet : PropertySet
{

	#region Constants


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Core Descriptors.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static readonly Describer[] Describers
		= new Describer[14]
	{
		new Describer("Icon", typeof(object)),
		new Describer("DataSource", C_KeyDataSource, typeof(string), C_DefaultDataSource, true, false, true, true, true),
		new Describer("Port", C_KeyPortNumber, typeof(int), C_DefaultPortNumber, true, false, true, false, true),
		new Describer("ServerType", C_KeyServerType, typeof(EnDbServerType), C_DefaultServerType, true, false, true, false, true),
		new Describer("Database", C_KeyCatalog, typeof(string), C_DefaultCatalog, true, false, true, true, true),
		new Describer("UserID" ,C_KeyUserId, typeof(string), C_DefaultUserId, true, false, true, true, true),
		new Describer("Password", C_KeyPassword, typeof(string), C_DefaultPassword, true, false, false, true),

		new Describer("DatasetKey", typeof(string), C_DefaultExDatasetKey),
		new Describer("ServerDefinition", typeof(ServerDefinition), C_DefaultExServerDefinition),
		new Describer("ServerVersion", typeof(Version), C_DefaultExServerVersion),
		new Describer("PersistPassword", typeof(bool), C_DefaultExPersistPassword),
		new Describer("AdministratorLogin", typeof(string), C_DefaultExAdministratorLogin),
		new Describer("ServerFullyQualifiedDomainName", typeof(string), C_DefaultExServerFullyQualifiedDomainName),
		new Describer("OtherParams", typeof(string))
	};



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Universal lookup of all possible synonyms of default parameter names.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static readonly KeyValuePair<string, string>[] Synonyms
		= new KeyValuePair<string, string>[8]
	{
		// StringPair( "datasource", "DataSource" ),
		StringPair( "server", "DataSource" ),
		StringPair( "host", "DataSource" ),
		StringPair( "uid", "UserID" ),
		StringPair( "user", "UserID" ),
		StringPair( "user name", "UserID" ),
		StringPair( "username", "UserID" ),
		StringPair( "user password", "Password" ),
		StringPair( "userpassword", "Password" )
	};


	#endregion Constants



	// =========================================================================================================
	#region Static Methods
	// =========================================================================================================


	public static void CreateAndPopulatePropertySetFromStatic(DescriberDictionary describers)
	{
		describers.AddRange(Describers);
		describers.AddSynonyms(Synonyms);
	}


	#endregion Static Methods


}
