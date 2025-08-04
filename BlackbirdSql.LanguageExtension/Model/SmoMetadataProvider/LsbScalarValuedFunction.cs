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
//											LsbScalarValuedFunction Class
//
/// <summary>
/// Impersonation of an SQL Server Smo ScalarValuedFunction for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbScalarValuedFunction : LsbUserDefinedFunction, IScalarValuedFunction, IUserDefinedFunction, IFunction, IMetadataObject, IFunctionModuleBase, IUserDefinedFunctionModuleBase, ISchemaOwnedObject, IDatabaseObject, ICallableModule, IScalarFunction, IScalar
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbScalarValuedFunction
	// ---------------------------------------------------------------------------------


	public LsbScalarValuedFunction(Microsoft.SqlServer.Management.Smo.UserDefinedFunction function, LsbSchema schema)
		: base(function, schema)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbScalarValuedFunction
	// =========================================================================================================


	private IScalarDataType m_dataType;

	private ParameterCollection m_parameters;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbScalarValuedFunction
	// =========================================================================================================


	public override IMetadataOrderedCollection<IParameter> Parameters
	{
		get
		{
			if (m_parameters == null)
			{
				LsbDatabase parent = base.Parent.Parent;
				m_parameters = Cmd.UserDefinedFunctionI.CreateParameterCollection(parent, m_smoMetadataObject.Parameters, GetModuleInfo());
			}
			return m_parameters;
		}
	}

	public bool IsAggregateFunction => false;

	public ScalarType ScalarType => ScalarType.ScalarFunction;

	public IScalarDataType DataType
	{
		get
		{
			if (m_dataType == null)
			{
				IDataType dataType = Cmd.GetDataType(base.Parent.Parent, m_smoMetadataObject.DataType);
				m_dataType = dataType as IScalarDataType;
			}
			return m_dataType;
		}
	}

	public bool Nullable => true;

	public CallableModuleType ModuleType => CallableModuleType.ScalarFunction;

	public IScalarDataType ReturnType => DataType;

	public bool ReturnsNullOnNullInput
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)m_smoMetadataObject, "ReturnsNullOnNullInput", out bool? value);
			if (!m_parent.IsSysSchema)
			{
				return value.GetValueOrDefault();
			}
			return false;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbScalarValuedFunction
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
