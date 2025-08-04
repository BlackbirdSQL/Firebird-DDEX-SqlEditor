// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.ExtendedStoredProcedure

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbExtendedStoredProcedure Class
//
/// <summary>
/// Impersonation of an SQL Server Smo ExtendedStoredProcedure for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbExtendedStoredProcedure : LsbSchemaOwnedObject<Microsoft.SqlServer.Management.Smo.ExtendedStoredProcedure>, IExtendedStoredProcedure, ICallableModule, IUserDefinedFunctionModuleBase, ISchemaOwnedObject, IDatabaseObject, IMetadataObject, IFunctionModuleBase
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbExtendedStoredProcedure
	// ---------------------------------------------------------------------------------


	public LsbExtendedStoredProcedure(Microsoft.SqlServer.Management.Smo.ExtendedStoredProcedure smoMetadataObject, LsbSchema parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property accessors - LsbExtendedStoredProcedure
	// =========================================================================================================


	public override int Id => m_smoMetadataObject.ID;

	public override bool IsSystemObject => m_smoMetadataObject.IsSystemObject;

	public CallableModuleType ModuleType => CallableModuleType.ExtendedStoredProcedure;

	public IScalarDataType ReturnType => LsbSmoSystemDataTypeLookup.Instance.Int;

	public IMetadataOrderedCollection<IParameter> Parameters => Collection<IParameter>.EmptyOrdered;

	public bool IsEncrypted => false;

	public IExecutionContext ExecutionContext => null;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbExtendedStoredProcedure
	// =========================================================================================================


	public override T Accept<T>(ISchemaOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}


	#endregion Methods

}
