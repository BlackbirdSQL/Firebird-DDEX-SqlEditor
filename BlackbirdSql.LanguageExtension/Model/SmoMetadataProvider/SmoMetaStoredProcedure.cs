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
//											SmoMetaStoredProcedure Class
//
/// <summary>
/// Impersonation of an SQL Server Smo StoredProcedure for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaStoredProcedure : AbstractSmoMetaSchemaOwnedModule<Microsoft.SqlServer.Management.Smo.StoredProcedure>, IStoredProcedure, ICallableModule, IUserDefinedFunctionModuleBase, ISchemaOwnedObject, IDatabaseObject, IMetadataObject, IFunctionModuleBase
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaStoredProcedure
	// ---------------------------------------------------------------------------------


	public SmoMetaStoredProcedure(Microsoft.SqlServer.Management.Smo.StoredProcedure smoMetadataObject, SmoMetaSchema parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaStoredProcedure
	// =========================================================================================================


	private IMetadataOrderedCollection<IParameter> _Parameters;

	private IExecutionContext _ExecutionContext;

	private string bodyText;

	private bool isBodyTextSet;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaStoredProcedure
	// =========================================================================================================


	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => _SmoMetadataObject.IsSystemObject;

	public CallableModuleType ModuleType => CallableModuleType.StoredProcedure;

	public IScalarDataType ReturnType => SmoMetaSmoSystemDataTypeLookup.Instance.Int;

	public IMetadataOrderedCollection<IParameter> Parameters
	{
		get
		{
			if (_Parameters == null)
			{
				SmoMetaDatabase parent = base.Parent.Parent;
				_Parameters = Cmd.StoredProcedureI.CreateParameterCollection(parent, _SmoMetadataObject);
			}
			return _Parameters;
		}
	}

	public bool IsEncrypted => _SmoMetadataObject.IsEncrypted;

	public IExecutionContext ExecutionContext
	{
		get
		{
			if (_ExecutionContext == null)
			{
				SmoMetaDatabase parent = _Parent.Parent;
				_ExecutionContext = Cmd.GetExecutionContext(parent, _SmoMetadataObject);
			}
			return _ExecutionContext;
		}
	}

	public string BodyText
	{
		get
		{
			if (HasBodyText() && !isBodyTextSet)
			{
				if (Cmd.TryGetPropertyObject<string>(_SmoMetadataObject, "Text", out var value))
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

	public bool ForReplication => _SmoMetadataObject.ForReplication;

	public bool IsSqlClr
	{
		get
		{
			ISfcSupportsDesignMode smoMetadataObject = _SmoMetadataObject;
			if (smoMetadataObject != null && smoMetadataObject.IsDesignMode)
			{
				return false;
			}
			return _SmoMetadataObject.ImplementationType == ImplementationType.SqlClr;
		}
	}

	public bool IsRecompiled
	{
		get
		{
			if (!IsSqlClr)
			{
				return _SmoMetadataObject.Recompile;
			}
			return false;
		}
	}

	public bool Startup => _SmoMetadataObject.Startup;

	public override bool IsQuotedIdentifierOn
	{
		get
		{
			if (!IsSqlClr)
			{
				return _SmoMetadataObject.QuotedIdentifierStatus;
			}
			return false;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaStoredProcedure
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
