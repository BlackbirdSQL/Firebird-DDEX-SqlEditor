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
/// The static constants base class for use by connection classes derived from AbstractPropertyAgent.
/// Refer to the <seealso cref="PropertySet"/> class for more information.
/// </summary>
// =========================================================================================================
public abstract class CorePropertySet : PropertySet
{

	#region Constants


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Core Descriptors.
	/// </summary>
	/// <remarks>
	/// Equivalency describers are tagged with *.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static readonly Describer[] Describers
		= new Describer[16]
	{
		new Describer("Icon", typeof(object)),
		new Describer("DataSource", C_KeyDataSource, typeof(string), C_DefaultDataSource, true, false, true, true, true), // *
		new Describer("Port", C_KeyPortNumber, typeof(int), C_DefaultPortNumber, true, false, true, false, true), // *
		new Describer("ServerType", C_KeyServerType, typeof(EnDbServerType), C_DefaultServerType, true, false, true, false, true), // *
		new Describer("Database", C_KeyCatalog, typeof(string), C_DefaultCatalog, true, false, true, true, true), // *
		new Describer("UserID" ,C_KeyUserId, typeof(string), C_DefaultUserId, true, false, true, true, true), // *
		new Describer("Password", C_KeyPassword, typeof(string), C_DefaultPassword, true, false, false, true),

		new Describer("Dataset", typeof(string), C_DefaultExDataset),
		new Describer("DatasetKey", typeof(string), C_DefaultExDatasetKey),
		new Describer("DisplayMember", typeof(string), C_DefaultExDisplayMember),
		new Describer("ServerDefinition", typeof(ServerDefinition), C_DefaultExServerDefinition),
		new Describer("ServerVersion", typeof(Version), C_DefaultExServerVersion),
		new Describer("PersistPassword", typeof(bool), C_DefaultExPersistPassword, false, false, false),
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
