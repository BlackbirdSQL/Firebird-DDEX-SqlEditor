// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.UserDefinedFunction

using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbUserDefinedFunction Class
//
/// <summary>
/// Impersonation of an SQL Server Smo UserDefinedFunction for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class LsbUserDefinedFunction : LsbSchemaOwnedModule<Microsoft.SqlServer.Management.Smo.UserDefinedFunction>, IUserDefinedFunction, IFunction, IMetadataObject, IFunctionModuleBase, IUserDefinedFunctionModuleBase, ISchemaOwnedObject, IDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbUserDefinedFunction
	// ---------------------------------------------------------------------------------


	protected LsbUserDefinedFunction(Microsoft.SqlServer.Management.Smo.UserDefinedFunction function, LsbSchema schema)
		: base(function, schema)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbUserDefinedFunction
	// =========================================================================================================


	private IExecutionContext m_executionContext;

	private string bodyText;

	private bool isBodyTextSet;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbUserDefinedFunction
	// =========================================================================================================


	public override int Id => m_smoMetadataObject.ID;

	public override bool IsSystemObject => m_smoMetadataObject.IsSystemObject;

	public bool IsTableValuedFunction
	{
		get
		{
			if (m_smoMetadataObject.FunctionType != UserDefinedFunctionType.Table)
			{
				return m_smoMetadataObject.FunctionType == UserDefinedFunctionType.Inline;
			}
			return true;
		}
	}

	public bool IsScalarValuedFunction => m_smoMetadataObject.FunctionType == UserDefinedFunctionType.Scalar;

	public string BodyText
	{
		get
		{
			if (HasBodyText() && !isBodyTextSet)
			{
				if (Cmd.TryGetPropertyObject<string>(m_smoMetadataObject, "Text", out var value))
				{
					bodyText = Cmd.RetrieveFunctionBody(value, IsQuotedIdentifierOn);
				}
				else
				{
					bodyText = null;
				}
				isBodyTextSet = true;
			}
			return bodyText;
		}
	}

	public bool IsSchemaBound
	{
		get
		{
			if (!IsSqlClr)
			{
				return m_smoMetadataObject.IsSchemaBound;
			}
			return false;
		}
	}

	public bool IsSqlClr
	{
		get
		{
			ISfcSupportsDesignMode smoMetadataObject = m_smoMetadataObject;
			if (smoMetadataObject != null && smoMetadataObject.IsDesignMode)
			{
				return false;
			}
			return m_smoMetadataObject.ImplementationType == ImplementationType.SqlClr;
		}
	}

	public override bool IsQuotedIdentifierOn
	{
		get
		{
			if (!IsSqlClr)
			{
				return m_smoMetadataObject.QuotedIdentifierStatus;
			}
			return false;
		}
	}

	public bool IsEncrypted => m_smoMetadataObject.IsEncrypted;

	public IExecutionContext ExecutionContext
	{
		get
		{
			if (m_executionContext == null)
			{
				LsbDatabase parent = m_parent.Parent;
				m_executionContext = Cmd.GetExecutionContext(parent, m_smoMetadataObject);
			}
			return m_executionContext;
		}
	}

	public abstract IMetadataOrderedCollection<IParameter> Parameters { get; }


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbUserDefinedFunction
	// =========================================================================================================


	private bool HasBodyText()
	{
		if (!IsSqlClr)
		{
			return !IsEncrypted;
		}
		return false;
	}


	#endregion Methods

}
