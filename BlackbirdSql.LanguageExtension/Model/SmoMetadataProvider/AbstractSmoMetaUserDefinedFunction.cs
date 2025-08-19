// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.UserDefinedFunction

using System.Collections.Generic;
using BlackbirdSql.LanguageExtension.Ctl.Config;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;


namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											AbstractSmoMetaUserDefinedFunction Class
//
/// <summary>
/// Impersonation of an SQL Server Smo UserDefinedFunction for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractSmoMetaUserDefinedFunction : AbstractSmoMetaSchemaOwnedModule<Microsoft.SqlServer.Management.Smo.UserDefinedFunction>, IUserDefinedFunction, IFunction, IMetadataObject, IFunctionModuleBase, IUserDefinedFunctionModuleBase, ISchemaOwnedObject, IDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractSmoMetaUserDefinedFunction
	// ---------------------------------------------------------------------------------


	protected AbstractSmoMetaUserDefinedFunction(Microsoft.SqlServer.Management.Smo.UserDefinedFunction function, SmoMetaSchema schema)
		: base(function, schema)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractSmoMetaUserDefinedFunction
	// =========================================================================================================


	private IExecutionContext _ExecutionContext;

	private string _BodyText;

	private bool _IsBodyTextSet;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractSmoMetaUserDefinedFunction
	// =========================================================================================================


	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => _SmoMetadataObject.IsSystemObject;

	public bool IsTableValuedFunction
	{
		get
		{
			if (_SmoMetadataObject.FunctionType != UserDefinedFunctionType.Table)
			{
				return _SmoMetadataObject.FunctionType == UserDefinedFunctionType.Inline;
			}
			return true;
		}
	}

	public bool IsScalarValuedFunction => _SmoMetadataObject.FunctionType == UserDefinedFunctionType.Scalar;

	public string BodyText
	{
		get
		{
			if (HasBodyText() && !_IsBodyTextSet)
			{
				if (Cmd.TryGetPropertyObject<string>(_SmoMetadataObject, "Text", out var value))
				{
					_BodyText = Cmd.RetrieveFunctionBody(value, IsQuotedIdentifierOn);
				}
				else
				{
					_BodyText = null;
				}
				_IsBodyTextSet = true;
			}
			return _BodyText;
		}
	}

	public bool IsSchemaBound
	{
		get
		{
			if (!IsSqlClr)
			{
				return _SmoMetadataObject.IsSchemaBound;
			}
			return false;
		}
	}

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

	public abstract IMetadataOrderedCollection<IParameter> Parameters { get; }


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractSmoMetaUserDefinedFunction
	// =========================================================================================================


	public static Microsoft.SqlServer.Management.SqlParser.MetadataProvider.ParameterCollection CreateParameterCollection(SmoMetaDatabase database, ParameterCollectionBase metadataCollection, IDictionary<string, object> moduleInfo)
	{
		_ = database.Parent;
		IParameterFactory parameter = SmoMetaSmoMetadataFactory.Instance.Parameter;
		database.Parent.TryRefreshSmoCollection(metadataCollection, SmoConfig.SmoInitFields.GetInitFields(typeof(UserDefinedFunctionParameter)));
		ParameterCollection parameterCollection = new ParameterCollection(metadataCollection.Count, database.CollationInfo);
		IList<IDictionary<string, object>> parametersInfo = ((moduleInfo != null) ? ((IList<IDictionary<string, object>>)moduleInfo[Microsoft.SqlServer.Management.SqlParser.Parser.PropertyKeys.Parameters]) : null);

		foreach (ParameterBase item in metadataCollection)
		{
			IDataType dataType;
			try
			{
				dataType = Cmd.GetDataType(database, item.DataType);
			}
			catch (SmoException)
			{
				dataType = SmoMetaSmoMetadataFactory.Instance.DataType.UnknownScalar;
			}
			IParameter parameter2;
			if (dataType is IScalarDataType dataType2)
			{
				IDictionary<string, object> parameterInfo = GetParameterInfo(parametersInfo, item.Name);
				string text = ((parameterInfo != null) ? ((string)parameterInfo[Microsoft.SqlServer.Management.SqlParser.Parser.PropertyKeys.DefaultValue]) : null);
				parameter2 = ((!string.IsNullOrEmpty(text)) ? parameter.CreateScalarParameter(item.Name, dataType2, isOutput: false, text) : parameter.CreateScalarParameter(item.Name, dataType2));
			}
			else
			{
				ITableDataType dataType3 = dataType as ITableDataType;
				parameter2 = parameter.CreateTableParameter(item.Name, dataType3);
			}
			parameterCollection.Add(parameter2);
		}
		return parameterCollection;
	}



	private static IDictionary<string, object> GetParameterInfo(IList<IDictionary<string, object>> parametersInfo, string parameterName)
	{
		if (parametersInfo != null)
		{
			foreach (IDictionary<string, object> item in parametersInfo)
			{
				if ((string)item[Microsoft.SqlServer.Management.SqlParser.Parser.PropertyKeys.Name] == parameterName)
				{
					return item;
				}
			}
		}
		return null;
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
