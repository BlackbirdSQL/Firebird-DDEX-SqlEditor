// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.StoredProcedure

using System;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbStoredProcedure Class
//
/// <summary>
/// Impersonation of an SQL Server Smo StoredProcedure for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbStoredProcedure : LsbSchemaOwnedModule<Microsoft.SqlServer.Management.Smo.StoredProcedure>, IStoredProcedure, ICallableModule, IUserDefinedFunctionModuleBase, ISchemaOwnedObject, IDatabaseObject, IMetadataObject, IFunctionModuleBase
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbStoredProcedure
	// ---------------------------------------------------------------------------------


	public LsbStoredProcedure(Microsoft.SqlServer.Management.Smo.StoredProcedure smoMetadataObject, LsbSchema parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbStoredProcedure
	// =========================================================================================================


	private IMetadataOrderedCollection<IParameter> m_parameters;

	private IExecutionContext m_executionContext;

	private string bodyText;

	private bool isBodyTextSet;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbStoredProcedure
	// =========================================================================================================


	public override int Id => m_smoMetadataObject.ID;

	public override bool IsSystemObject => m_smoMetadataObject.IsSystemObject;

	public CallableModuleType ModuleType => CallableModuleType.StoredProcedure;

	public IScalarDataType ReturnType => LsbSmoSystemDataTypeLookup.Instance.Int;

	public IMetadataOrderedCollection<IParameter> Parameters
	{
		get
		{
			if (m_parameters == null)
			{
				LsbDatabase parent = base.Parent.Parent;
				m_parameters = Cmd.StoredProcedureI.CreateParameterCollection(parent, m_smoMetadataObject);
			}
			return m_parameters;
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

	public string BodyText
	{
		get
		{
			if (HasBodyText() && !isBodyTextSet)
			{
				if (Cmd.TryGetPropertyObject<string>(m_smoMetadataObject, "Text", out var value))
				{
					bodyText = Cmd.RetrieveStoredProcedureBody(value, IsQuotedIdentifierOn);
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

	public bool ForReplication => m_smoMetadataObject.ForReplication;

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

	public bool IsRecompiled
	{
		get
		{
			if (!IsSqlClr)
			{
				return m_smoMetadataObject.Recompile;
			}
			return false;
		}
	}

	public bool Startup => m_smoMetadataObject.Startup;

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


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbStoredProcedure
	// =========================================================================================================


	public override T Accept<T>(ISchemaOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}

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
