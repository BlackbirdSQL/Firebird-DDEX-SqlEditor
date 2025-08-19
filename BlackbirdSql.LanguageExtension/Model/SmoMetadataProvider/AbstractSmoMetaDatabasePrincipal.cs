// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.DatabasePrincipal<S>

using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


// =========================================================================================================
//
//											AbstractSmoMetaDatabasePrincipal Class
//
/// <summary>
/// Impersonation of an SQL Server Smo DatabasePrincipal for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractSmoMetaDatabasePrincipal<S> : AbstractSmoMetaDatabaseOwnedObject<S>, IDatabasePrincipal, IDatabaseOwnedObject, IDatabaseObject, IMetadataObject where S : NamedSmoObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractSmoMetaDatabasePrincipal
	// ---------------------------------------------------------------------------------


	protected AbstractSmoMetaDatabasePrincipal(S smoMetadataObject, SmoMetaDatabase parent)
		: base(smoMetadataObject, parent)
	{
	}

	static AbstractSmoMetaDatabasePrincipal()
	{
		allPermissionTypes = (DatabasePermissionType[])Enum.GetValues(typeof(DatabasePermissionType));
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractSmoMetaDatabasePrincipal
	// =========================================================================================================


	private static readonly DatabasePermissionType[] allPermissionTypes;

	private IMetadataCollection<IDatabaseRole> memberOfRoles;

	private IMetadataCollection<IDatabasePermission> permissions;



	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractSmoMetaDatabasePrincipal
	// =========================================================================================================


	public IMetadataCollection<IDatabaseRole> MemberOfRoles
	{
		get
		{
			memberOfRoles ??= CreateMemberOfRolesCollection();
			return memberOfRoles;
		}
	}

	public IMetadataCollection<IDatabasePermission> Permissions
	{
		get
		{
			permissions ??= CreatePermissionCollection();
			return permissions;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractSmoMetaDatabasePrincipal
	// =========================================================================================================


	protected abstract IEnumerable<string> GetMemberOfRoleNames();

	private IMetadataCollection<IDatabaseRole> CreateMemberOfRolesCollection()
	{
		IEnumerable<string> memberOfRoleNames = GetMemberOfRoleNames();
		Microsoft.SqlServer.Management.SqlParser.MetadataProvider.DatabaseRoleCollection databaseRoleCollection = new Microsoft.SqlServer.Management.SqlParser.MetadataProvider.DatabaseRoleCollection(base.Database.CollationInfo);
		foreach (string item2 in memberOfRoleNames)
		{
			IDatabaseRole item = base.Database.Roles[item2];
			databaseRoleCollection.Add(item);
		}
		return databaseRoleCollection;
	}

	private IMetadataCollection<IDatabasePermission> CreatePermissionCollection()
	{
		ObjectPermissionInfo[] array = base.Parent.SmoObject.EnumObjectPermissions(_SmoMetadataObject.Name);
		IEnumerable<DatabasePermissionInfo> enumerable = base.Parent.SmoObject.EnumDatabasePermissions(_SmoMetadataObject.Name);
		DatabasePermissionCollection databasePermissionCollection = new DatabasePermissionCollection(base.Database.CollationInfo);
		foreach (ObjectPermissionInfo item3 in (IEnumerable<ObjectPermissionInfo>)array)
		{
			Microsoft.SqlServer.Management.SqlParser.Metadata.PermissionState permissionState = GetPermissionState(item3.PermissionState);
			foreach (DatabasePermissionType permissionType in GetPermissionTypes(item3.PermissionType))
			{
				IDatabasePermission item = new SmoMetaDatabasePermission(this, item3, permissionType, permissionState, null);
				databasePermissionCollection.Add(item);
			}
		}
		foreach (DatabasePermissionInfo item4 in enumerable)
		{
			Microsoft.SqlServer.Management.SqlParser.Metadata.PermissionState permissionState2 = GetPermissionState(item4.PermissionState);
			foreach (DatabasePermissionType permissionType2 in GetPermissionTypes(item4.PermissionType))
			{
				IDatabasePermission item2 = new SmoMetaDatabasePermission(this, item4, permissionType2, permissionState2, null);
				databasePermissionCollection.Add(item2);
			}
		}
		return databasePermissionCollection;
	}

	private static IEnumerable<DatabasePermissionType> GetPermissionTypes(ObjectPermissionSet permissionSet)
	{
		DatabasePermissionType[] array = allPermissionTypes;
		foreach (DatabasePermissionType databasePermissionType in array)
		{
			if (ContainsPermissionType(permissionSet, databasePermissionType))
			{
				yield return databasePermissionType;
			}
		}
	}

	private static IEnumerable<DatabasePermissionType> GetPermissionTypes(DatabasePermissionSet permissionSet)
	{
		DatabasePermissionType[] array = allPermissionTypes;
		foreach (DatabasePermissionType databasePermissionType in array)
		{
			if (ContainsPermissionType(permissionSet, databasePermissionType))
			{
				yield return databasePermissionType;
			}
		}
	}

	private static bool ContainsPermissionType(ObjectPermissionSet permissionSet, DatabasePermissionType permissionType)
	{
		return permissionType switch
		{
			DatabasePermissionType.Alter => permissionSet.Alter, 
			DatabasePermissionType.Connect => permissionSet.Connect, 
			DatabasePermissionType.Control => permissionSet.Control, 
			DatabasePermissionType.CreateSequence => permissionSet.CreateSequence, 
			DatabasePermissionType.Delete => permissionSet.Delete, 
			DatabasePermissionType.Execute => permissionSet.Execute, 
			DatabasePermissionType.Impersonate => permissionSet.Impersonate, 
			DatabasePermissionType.Insert => permissionSet.Insert, 
			DatabasePermissionType.Receive => permissionSet.Receive, 
			DatabasePermissionType.References => permissionSet.References, 
			DatabasePermissionType.Select => permissionSet.Select, 
			DatabasePermissionType.Send => permissionSet.Send, 
			DatabasePermissionType.TakeOwnership => permissionSet.TakeOwnership, 
			DatabasePermissionType.Unmask => permissionSet.Unmask, 
			DatabasePermissionType.Update => permissionSet.Update, 
			DatabasePermissionType.ViewChangeTracking => permissionSet.ViewChangeTracking, 
			DatabasePermissionType.ViewDefinition => permissionSet.ViewDefinition, 
			_ => false, 
		};
	}

	private static bool ContainsPermissionType(DatabasePermissionSet permissionSet, DatabasePermissionType permissionType)
	{
		return permissionType switch
		{
			DatabasePermissionType.AdministerDatabaseBulkOperations => permissionSet.AdministerDatabaseBulkOperations, 
			DatabasePermissionType.Alter => permissionSet.Alter, 
			DatabasePermissionType.AlterAnyApplicationRole => permissionSet.AlterAnyApplicationRole, 
			DatabasePermissionType.AlterAnyAssembly => permissionSet.AlterAnyAssembly, 
			DatabasePermissionType.AlterAnyAsymmetricKey => permissionSet.AlterAnyAsymmetricKey, 
			DatabasePermissionType.AlterAnyCertificate => permissionSet.AlterAnyCertificate, 
			DatabasePermissionType.AlterAnyColumnEncryptionKey => permissionSet.AlterAnyColumnEncryptionKey, 
			DatabasePermissionType.AlterAnyColumnMasterKey => permissionSet.AlterAnyColumnMasterKey, 
			DatabasePermissionType.AlterAnyContract => permissionSet.AlterAnyContract, 
			DatabasePermissionType.AlterAnyDatabaseAudit => permissionSet.AlterAnyDatabaseAudit, 
			DatabasePermissionType.AlterAnyDatabaseDdlTrigger => permissionSet.AlterAnyDatabaseDdlTrigger, 
			DatabasePermissionType.AlterAnyDatabaseEventNotification => permissionSet.AlterAnyDatabaseEventNotification, 
			DatabasePermissionType.AlterAnyDatabaseEventSession => permissionSet.AlterAnyDatabaseEventSession, 
			DatabasePermissionType.AlterAnyDatabaseEventSessionAddEvent => permissionSet.AlterAnyDatabaseEventSessionAddEvent, 
			DatabasePermissionType.AlterAnyDatabaseEventSessionAddTarget => permissionSet.AlterAnyDatabaseEventSessionAddTarget, 
			DatabasePermissionType.AlterAnyDatabaseEventSessionDisable => permissionSet.AlterAnyDatabaseEventSessionDisable, 
			DatabasePermissionType.AlterAnyDatabaseEventSessionDropEvent => permissionSet.AlterAnyDatabaseEventSessionDropEvent, 
			DatabasePermissionType.AlterAnyDatabaseEventSessionDropTarget => permissionSet.AlterAnyDatabaseEventSessionDropTarget, 
			DatabasePermissionType.AlterAnyDatabaseEventSessionEnable => permissionSet.AlterAnyDatabaseEventSessionEnable, 
			DatabasePermissionType.AlterAnyDatabaseEventSessionOption => permissionSet.AlterAnyDatabaseEventSessionOption, 
			DatabasePermissionType.AlterAnyDatabaseScopedConfiguration => permissionSet.AlterAnyDatabaseScopedConfiguration, 
			DatabasePermissionType.AlterAnyDataspace => permissionSet.AlterAnyDataspace, 
			DatabasePermissionType.AlterAnyExternalDataSource => permissionSet.AlterAnyExternalDataSource, 
			DatabasePermissionType.AlterAnyExternalFileFormat => permissionSet.AlterAnyExternalFileFormat, 
			DatabasePermissionType.AlterAnyExternalJob => permissionSet.AlterAnyExternalJob, 
			DatabasePermissionType.AlterAnyExternalLanguage => permissionSet.AlterAnyExternalLanguage, 
			DatabasePermissionType.AlterAnyExternalLibrary => permissionSet.AlterAnyExternalLibrary, 
			DatabasePermissionType.AlterAnyExternalStream => permissionSet.AlterAnyExternalStream, 
			DatabasePermissionType.AlterAnyFulltextCatalog => permissionSet.AlterAnyFulltextCatalog, 
			DatabasePermissionType.AlterAnyMask => permissionSet.AlterAnyMask, 
			DatabasePermissionType.AlterAnyMessageType => permissionSet.AlterAnyMessageType, 
			DatabasePermissionType.AlterAnyRemoteServiceBinding => permissionSet.AlterAnyRemoteServiceBinding, 
			DatabasePermissionType.AlterAnyRole => permissionSet.AlterAnyRole, 
			DatabasePermissionType.AlterAnyRoute => permissionSet.AlterAnyRoute, 
			DatabasePermissionType.AlterAnySchema => permissionSet.AlterAnySchema, 
			DatabasePermissionType.AlterAnySecurityPolicy => permissionSet.AlterAnySecurityPolicy, 
			DatabasePermissionType.AlterAnySensitivityClassification => permissionSet.AlterAnySensitivityClassification, 
			DatabasePermissionType.AlterAnyService => permissionSet.AlterAnyService, 
			DatabasePermissionType.AlterAnySymmetricKey => permissionSet.AlterAnySymmetricKey, 
			DatabasePermissionType.AlterAnyUser => permissionSet.AlterAnyUser, 
			DatabasePermissionType.AlterLedger => permissionSet.AlterLedger, 
			DatabasePermissionType.AlterLedgerConfiguration => permissionSet.AlterLedgerConfiguration, 
			DatabasePermissionType.Authenticate => permissionSet.Authenticate, 
			DatabasePermissionType.BackupDatabase => permissionSet.BackupDatabase, 
			DatabasePermissionType.BackupLog => permissionSet.BackupLog, 
			DatabasePermissionType.Checkpoint => permissionSet.Checkpoint, 
			DatabasePermissionType.Connect => permissionSet.Connect, 
			DatabasePermissionType.ConnectReplication => permissionSet.ConnectReplication, 
			DatabasePermissionType.Control => permissionSet.Control, 
			DatabasePermissionType.CreateAggregate => permissionSet.CreateAggregate, 
			DatabasePermissionType.CreateAnyDatabaseEventSession => permissionSet.CreateAnyDatabaseEventSession, 
			DatabasePermissionType.CreateAssembly => permissionSet.CreateAssembly, 
			DatabasePermissionType.CreateAsymmetricKey => permissionSet.CreateAsymmetricKey, 
			DatabasePermissionType.CreateCertificate => permissionSet.CreateCertificate, 
			DatabasePermissionType.CreateContract => permissionSet.CreateContract, 
			DatabasePermissionType.CreateDatabase => permissionSet.CreateDatabase, 
			DatabasePermissionType.CreateDatabaseDdlEventNotification => permissionSet.CreateDatabaseDdlEventNotification, 
			DatabasePermissionType.CreateDefault => permissionSet.CreateDefault, 
			DatabasePermissionType.CreateExternalLanguage => permissionSet.CreateExternalLanguage, 
			DatabasePermissionType.CreateExternalLibrary => permissionSet.CreateExternalLibrary, 
			DatabasePermissionType.CreateFulltextCatalog => permissionSet.CreateFulltextCatalog, 
			DatabasePermissionType.CreateFunction => permissionSet.CreateFunction, 
			DatabasePermissionType.CreateMessageType => permissionSet.CreateMessageType, 
			DatabasePermissionType.CreateProcedure => permissionSet.CreateProcedure, 
			DatabasePermissionType.CreateQueue => permissionSet.CreateQueue, 
			DatabasePermissionType.CreateRemoteServiceBinding => permissionSet.CreateRemoteServiceBinding, 
			DatabasePermissionType.CreateRole => permissionSet.CreateRole, 
			DatabasePermissionType.CreateRoute => permissionSet.CreateRoute, 
			DatabasePermissionType.CreateRule => permissionSet.CreateRule, 
			DatabasePermissionType.CreateSchema => permissionSet.CreateSchema, 
			DatabasePermissionType.CreateService => permissionSet.CreateService, 
			DatabasePermissionType.CreateSymmetricKey => permissionSet.CreateSymmetricKey, 
			DatabasePermissionType.CreateSynonym => permissionSet.CreateSynonym, 
			DatabasePermissionType.CreateTable => permissionSet.CreateTable, 
			DatabasePermissionType.CreateType => permissionSet.CreateType, 
			DatabasePermissionType.CreateUser => permissionSet.CreateUser, 
			DatabasePermissionType.CreateView => permissionSet.CreateView, 
			DatabasePermissionType.CreateXmlSchemaCollection => permissionSet.CreateXmlSchemaCollection, 
			DatabasePermissionType.Delete => permissionSet.Delete, 
			DatabasePermissionType.DropAnyDatabaseEventSession => permissionSet.DropAnyDatabaseEventSession, 
			DatabasePermissionType.EnableLedger => permissionSet.EnableLedger, 
			DatabasePermissionType.Execute => permissionSet.Execute, 
			DatabasePermissionType.ExecuteAnyExternalEndpoint => permissionSet.ExecuteAnyExternalEndpoint, 
			DatabasePermissionType.ExecuteAnyExternalScript => permissionSet.ExecuteAnyExternalScript, 
			DatabasePermissionType.Insert => permissionSet.Insert, 
			DatabasePermissionType.KillDatabaseConnection => permissionSet.KillDatabaseConnection, 
			DatabasePermissionType.References => permissionSet.References, 
			DatabasePermissionType.Select => permissionSet.Select, 
			DatabasePermissionType.Showplan => permissionSet.Showplan, 
			DatabasePermissionType.SubscribeQueryNotifications => permissionSet.SubscribeQueryNotifications, 
			DatabasePermissionType.TakeOwnership => permissionSet.TakeOwnership, 
			DatabasePermissionType.Unmask => permissionSet.Unmask, 
			DatabasePermissionType.Update => permissionSet.Update, 
			DatabasePermissionType.ViewAnyColumnEncryptionKeyDefinition => permissionSet.ViewAnyColumnEncryptionKeyDefinition, 
			DatabasePermissionType.ViewAnyColumnMasterKeyDefinition => permissionSet.ViewAnyColumnMasterKeyDefinition, 
			DatabasePermissionType.ViewAnySensitivityClassification => permissionSet.ViewAnySensitivityClassification, 
			DatabasePermissionType.ViewCryptographicallySecuredDefinition => permissionSet.ViewCryptographicallySecuredDefinition, 
			DatabasePermissionType.ViewDatabasePerformanceState => permissionSet.ViewDatabasePerformanceState, 
			DatabasePermissionType.ViewDatabaseSecurityAudit => permissionSet.ViewDatabaseSecurityAudit, 
			DatabasePermissionType.ViewDatabaseSecurityState => permissionSet.ViewDatabaseSecurityState, 
			DatabasePermissionType.ViewDatabaseState => permissionSet.ViewDatabaseState, 
			DatabasePermissionType.ViewDefinition => permissionSet.ViewDefinition, 
			DatabasePermissionType.ViewLedgerContent => permissionSet.ViewLedgerContent, 
			DatabasePermissionType.ViewPerformanceDefinition => permissionSet.ViewPerformanceDefinition, 
			DatabasePermissionType.ViewSecurityDefinition => permissionSet.ViewSecurityDefinition, 
			_ => false, 
		};
	}

	private Microsoft.SqlServer.Management.SqlParser.Metadata.PermissionState GetPermissionState(Microsoft.SqlServer.Management.Smo.PermissionState permissionState)
	{
		return permissionState switch
		{
			Microsoft.SqlServer.Management.Smo.PermissionState.Deny => Microsoft.SqlServer.Management.SqlParser.Metadata.PermissionState.Deny, 
			Microsoft.SqlServer.Management.Smo.PermissionState.Grant => Microsoft.SqlServer.Management.SqlParser.Metadata.PermissionState.Grant, 
			Microsoft.SqlServer.Management.Smo.PermissionState.GrantWithGrant => Microsoft.SqlServer.Management.SqlParser.Metadata.PermissionState.GrantWithGrant, 
			Microsoft.SqlServer.Management.Smo.PermissionState.Revoke => Microsoft.SqlServer.Management.SqlParser.Metadata.PermissionState.Revoke, 
			_ => Microsoft.SqlServer.Management.SqlParser.Metadata.PermissionState.Deny, 
		};
	}


	#endregion Methods

}
