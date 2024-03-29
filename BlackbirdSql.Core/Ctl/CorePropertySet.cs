﻿/*
 *	Replica to expose the FirebirdClient ConnectionString as well as additional members
 *	to support SourceInformation, Root ObjectSelector and SqlServer style ConnectionInfo classes
 */

// $OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Model.Enums;
using FirebirdSql.Data.FirebirdClient;

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
	public static readonly Describer[] Describers =
	[
		new Describer(C_KeyExIcon, typeof(object)),

		new Describer(C_KeyDataSource, C_KeyFbDataSource, typeof(string), C_DefaultDataSource, true, false, true, true), // *
		new Describer(C_KeyPort, C_KeyFbPort, typeof(int), C_DefaultPort, true, false), // *
		new Describer(C_KeyServerType, C_KeyFbServerType, typeof(FbServerType), C_DefaultServerType, true, false), // *
		new Describer(C_KeyDatabase, C_KeyFbDatabase, typeof(string), C_DefaultDatabase, true, false, true, true), // *
		new Describer(C_KeyUserID ,C_KeyFbUserID, typeof(string), C_DefaultUserID, true, false, true, true), // *
		new Describer(C_KeyPassword, C_KeyFbPassword, typeof(string), C_DefaultPassword, true, false, false, true),

		new Describer(C_KeyExDataset, typeof(string), C_DefaultExDataset),
		new Describer(C_KeyExDatasetKey, typeof(string), C_DefaultExDatasetKey),
		new Describer(C_KeyExConnectionKey, typeof(string), C_DefaultExConnectionKey),
		new Describer(C_KeyExDatasetId, typeof(string), C_DefaultExDatasetId),
		new Describer(C_KeyExConnectionName, typeof(string), C_DefaultExConnectionName),
		new Describer(C_KeyExConnectionSource, typeof(EnConnectionSource), C_DefaultExConnectionSource),
		new Describer(C_KeyExServerEngine, typeof(EnEngineType), C_DefaultExServerEngine),
		new Describer(C_KeyExServerVersion, typeof(Version), C_DefaultExServerVersion),
		new Describer(C_KeyExPersistPassword, typeof(bool), C_DefaultExPersistPassword, false, false, false),
		new Describer(C_KeyExAdministratorLogin, typeof(string), C_DefaultExAdministratorLogin),
		new Describer(C_KeyExServerFullyQualifiedDomainName, typeof(string), C_DefaultExServerFullyQualifiedDomainName),
		new Describer(C_KeyExOtherParams, typeof(string))
	];
	public static readonly KeyValuePair<string, string>[] Synonyms =
		[
			StringPair("server", C_KeyDataSource),
			StringPair("host", C_KeyDataSource),
			StringPair("uid", C_KeyUserID),
			StringPair("user", C_KeyUserID),
			StringPair("username", C_KeyUserID),
			StringPair("user name", C_KeyUserID),
			StringPair("userpassword", C_KeyPassword),
			StringPair("user password", C_KeyPassword),
		];


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
