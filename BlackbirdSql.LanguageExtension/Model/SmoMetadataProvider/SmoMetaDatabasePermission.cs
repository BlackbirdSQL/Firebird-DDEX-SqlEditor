// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.DatabasePermission

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


// =========================================================================================================
//
//											SmoMetaDatabasePermission Class
//
/// <summary>
/// Impersonation of an SQL Server Smo DatabasePermission for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaDatabasePermission : IDatabasePermission, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaDatabasePermission
	// ---------------------------------------------------------------------------------


	public SmoMetaDatabasePermission(IDatabasePrincipal databasePrincipal, PermissionInfo permissionInfo, DatabasePermissionType permissionType, Microsoft.SqlServer.Management.SqlParser.Metadata.PermissionState permissionState, IDatabasePrincipal grantor)
	{
		name = Guid.NewGuid().ToString();
		this.databasePrincipal = databasePrincipal;
		this.permissionInfo = permissionInfo;
		this.permissionType = permissionType;
		this.permissionState = permissionState;
		this.grantor = grantor;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaDatabasePermission
	// =========================================================================================================


	private readonly string name;

	private readonly IDatabasePrincipal databasePrincipal;

	private readonly IDatabasePrincipal grantor;

	private readonly PermissionInfo permissionInfo;

	private readonly DatabasePermissionType permissionType;

	private readonly Microsoft.SqlServer.Management.SqlParser.Metadata.PermissionState permissionState;

	private IMetadataObject targetObject;

	private bool targetObjectSet;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaDatabasePermission
	// =========================================================================================================


	public IDatabasePrincipal DatabasePrincipal => databasePrincipal;

	public IDatabasePrincipal Grantor => grantor;

	public Microsoft.SqlServer.Management.SqlParser.Metadata.PermissionState PermissionState => permissionState;

	public DatabasePermissionType PermissionType => permissionType;

	public IMetadataObject TargetObject
	{
		get
		{
			if (!targetObjectSet)
			{
				targetObject = FindTargetObject();
				targetObjectSet = true;
			}
			return targetObject;
		}
	}

	public string Name => name;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaDatabasePermission
	// =========================================================================================================


	public T Accept<T>(IMetadataObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}

	private IMetadataObject FindTargetObject()
	{
		IDatabase database = databasePrincipal.Database;
		switch (permissionInfo.ObjectClass)
		{
		case ObjectClass.Database:
			return database.Server.Databases[permissionInfo.ObjectName];
		case ObjectClass.ObjectOrColumn:
		{
			IMetadataObject metadataObject = database.Schemas[permissionInfo.ObjectSchema].ExtendedStoredProcedures[permissionInfo.ObjectName];
			object obj2 = metadataObject;
			if (obj2 == null)
			{
				metadataObject = database.Schemas[permissionInfo.ObjectSchema].ScalarValuedFunctions[permissionInfo.ObjectName];
				obj2 = metadataObject;
				if (obj2 == null)
				{
					metadataObject = database.Schemas[permissionInfo.ObjectSchema].StoredProcedures[permissionInfo.ObjectName];
					obj2 = metadataObject;
					if (obj2 == null)
					{
						metadataObject = database.Schemas[permissionInfo.ObjectSchema].Tables[permissionInfo.ObjectName];
						obj2 = metadataObject;
						if (obj2 == null)
						{
							metadataObject = database.Schemas[permissionInfo.ObjectSchema].TableValuedFunctions[permissionInfo.ObjectName];
							obj2 = metadataObject;
							if (obj2 == null)
							{
								metadataObject = database.Schemas[permissionInfo.ObjectSchema].UserDefinedAggregates[permissionInfo.ObjectName];
								obj2 = metadataObject;
								if (obj2 == null)
								{
									metadataObject = database.Schemas[permissionInfo.ObjectSchema].Views[permissionInfo.ObjectName];
									obj2 = metadataObject ?? database.Schemas[permissionInfo.ObjectSchema].Synonyms[permissionInfo.ObjectName];
								}
							}
						}
					}
				}
			}
			IMetadataObject metadataObject2 = (IMetadataObject)obj2;
			if (!string.IsNullOrEmpty(permissionInfo.ColumnName))
			{
				metadataObject2 = (metadataObject2 as ITabular).Columns[permissionInfo.ColumnName];
			}
			return metadataObject2;
		}
		case ObjectClass.Schema:
			return database.Schemas[permissionInfo.ObjectName];
		case ObjectClass.UserDefinedType:
		{
			IMetadataObject metadataObject = database.Schemas[permissionInfo.ObjectSchema].UserDefinedDataTypes[permissionInfo.ObjectName];
			object obj = metadataObject;
			if (obj == null)
			{
				metadataObject = database.Schemas[permissionInfo.ObjectSchema].UserDefinedTableTypes[permissionInfo.ObjectName];
				obj = metadataObject ?? database.Schemas[permissionInfo.ObjectSchema].UserDefinedClrTypes[permissionInfo.ObjectName];
			}
			return (IMetadataObject)obj;
		}
		case ObjectClass.Certificate:
			return database.Certificates[permissionInfo.ObjectName];
		case ObjectClass.AsymmetricKey:
			return database.AsymmetricKeys[permissionInfo.ObjectName];
		case ObjectClass.ApplicationRole:
			return database.ApplicationRoles[permissionInfo.ObjectName];
		case ObjectClass.User:
			return database.Users[permissionInfo.ObjectName];
		case ObjectClass.DatabaseRole:
			return database.Roles[permissionInfo.ObjectName];
		case ObjectClass.SqlAssembly:
		case ObjectClass.SecurityExpression:
		case ObjectClass.XmlNamespace:
		case ObjectClass.MessageType:
		case ObjectClass.ServiceContract:
		case ObjectClass.Service:
		case ObjectClass.RemoteServiceBinding:
		case ObjectClass.ServiceRoute:
		case ObjectClass.FullTextCatalog:
		case ObjectClass.SymmetricKey:
		case ObjectClass.FullTextStopList:
		case ObjectClass.SearchPropertyList:
		case ObjectClass.AvailabilityGroup:
			return null;
		case ObjectClass.Server:
		case ObjectClass.Login:
		case ObjectClass.Endpoint:
		case ObjectClass.ServerPrincipal:
		case ObjectClass.ServerRole:
			return null;
		default:
			return null;
		}
	}


	#endregion Methods

}
