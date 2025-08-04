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
//											LsbSchemaOwnedModule Class
//
/// <summary>
/// Impersonation of an SQL Server Smo SchemaOwnedModule for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class LsbSchemaOwnedModule<S> : LsbSchemaOwnedObject<S> where S : ScriptSchemaObjectBase
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbSchemaOwnedModule
	// ---------------------------------------------------------------------------------


	protected LsbSchemaOwnedModule(S smoMetadataObject, LsbSchema parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbSchemaOwnedModule
	// =========================================================================================================


	private bool moduleInfoRetrieved;

	private IDictionary<string, object> moduleInfo;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbSchemaOwnedModule
	// =========================================================================================================


	public abstract bool IsQuotedIdentifierOn { get; }


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbSchemaOwnedModule
	// =========================================================================================================


	protected IDictionary<string, object> GetModuleInfo()
	{
		if (!moduleInfoRetrieved)
		{
			string definitionTest = Cmd.ModuleI.GetDefinitionTest(m_smoMetadataObject);
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
