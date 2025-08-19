// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.SchemaOwnedModule<S>

using System.Collections.Generic;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Parser;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											AbstractSmoMetaSchemaOwnedModule Class
//
/// <summary>
/// Impersonation of an SQL Server Smo SchemaOwnedModule for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractSmoMetaSchemaOwnedModule<S> : AbstractSmoMetaSchemaOwnedObject<S> where S : ScriptSchemaObjectBase
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractSmoMetaSchemaOwnedModule
	// ---------------------------------------------------------------------------------


	protected AbstractSmoMetaSchemaOwnedModule(S smoMetadataObject, SmoMetaSchema parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractSmoMetaSchemaOwnedModule
	// =========================================================================================================


	private bool moduleInfoRetrieved;

	private IDictionary<string, object> moduleInfo;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractSmoMetaSchemaOwnedModule
	// =========================================================================================================


	public abstract bool IsQuotedIdentifierOn { get; }


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractSmoMetaSchemaOwnedModule
	// =========================================================================================================


	protected IDictionary<string, object> GetModuleInfo()
	{
		if (!moduleInfoRetrieved)
		{
			string definitionTest = Cmd.ModuleI.GetDefinitionTest(_SmoMetadataObject);
			if (!string.IsNullOrEmpty(definitionTest))
			{
				moduleInfo = ParseUtils.RetrieveModuleDefinition(definitionTest, new ParseOptions(string.Empty, IsQuotedIdentifierOn));
			}
			moduleInfoRetrieved = true;
		}
		return moduleInfo;
	}


	#endregion Methods

}
