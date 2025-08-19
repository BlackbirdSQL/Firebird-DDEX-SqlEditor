// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.ScalarValuedFunction

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											SmoMetaScalarValuedFunction Class
//
/// <summary>
/// Impersonation of an SQL Server Smo ScalarValuedFunction for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaScalarValuedFunction : AbstractSmoMetaUserDefinedFunction, IScalarValuedFunction, IUserDefinedFunction, IFunction, IMetadataObject, IFunctionModuleBase, IUserDefinedFunctionModuleBase, ISchemaOwnedObject, IDatabaseObject, ICallableModule, IScalarFunction, IScalar
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaScalarValuedFunction
	// ---------------------------------------------------------------------------------


	public SmoMetaScalarValuedFunction(Microsoft.SqlServer.Management.Smo.UserDefinedFunction function, SmoMetaSchema schema)
		: base(function, schema)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaScalarValuedFunction
	// =========================================================================================================


	private IScalarDataType _DataType;

	private ParameterCollection _Parameters;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaScalarValuedFunction
	// =========================================================================================================


	public override IMetadataOrderedCollection<IParameter> Parameters
	{
		get
		{
			if (_Parameters == null)
			{
				SmoMetaDatabase parent = base.Parent.Parent;
				_Parameters = Cmd.UserDefinedFunctionI.CreateParameterCollection(parent, _SmoMetadataObject.Parameters, GetModuleInfo());
			}
			return _Parameters;
		}
	}

	public bool IsAggregateFunction => false;

	public ScalarType ScalarType => ScalarType.ScalarFunction;

	public IScalarDataType DataType
	{
		get
		{
			if (_DataType == null)
			{
				IDataType dataType = Cmd.GetDataType(base.Parent.Parent, _SmoMetadataObject.DataType);
				_DataType = dataType as IScalarDataType;
			}
			return _DataType;
		}
	}

	public bool Nullable => true;

	public CallableModuleType ModuleType => CallableModuleType.ScalarFunction;

	public IScalarDataType ReturnType => DataType;

	public bool ReturnsNullOnNullInput
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoMetadataObject, "ReturnsNullOnNullInput", out bool? value);
			if (!_Parent.IsSysSchema)
			{
				return value.GetValueOrDefault();
			}
			return false;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaScalarValuedFunction
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
