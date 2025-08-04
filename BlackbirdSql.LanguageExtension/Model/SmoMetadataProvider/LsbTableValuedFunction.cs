// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.TableValuedFunction

using System;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											LsbTableValuedFunction Class
//
/// <summary>
/// Impersonation of an SQL Server Smo TableValuedFunction for providing metadata.
/// </summary>
// =========================================================================================================
internal sealed class LsbTableValuedFunction : LsbUserDefinedFunction, ITableValuedFunction, IDatabaseTable, ITabular, IMetadataObject, IUserDefinedFunction, IFunction, IFunctionModuleBase, IUserDefinedFunctionModuleBase, ISchemaOwnedObject, IDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbTableValuedFunction
	// ---------------------------------------------------------------------------------


	public LsbTableValuedFunction(Microsoft.SqlServer.Management.Smo.UserDefinedFunction function, Microsoft.SqlServer.Management.SmoMetadataProvider.Schema schema)
		: base(function, schema)
	{
		columnCollection = new LsbColumn.ColumnCollectionHelperI(base.Parent.Database, this, m_smoMetadataObject.Columns);
		constraintCollection = new LsbConstraintCollection.ConstraintCollectionHelperI(base.Parent.Database, this, m_smoMetadataObject);
		indexCollection = new LsbIndex.IndexCollectionHelperI(base.Parent.Database, this, m_smoMetadataObject.Indexes);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbTableValuedFunction
	// =========================================================================================================


	private IMetadataOrderedCollection<IParameter> m_parameters;

	private readonly Utils.ColumnCollectionHelper columnCollection;

	private readonly Utils.ConstraintCollectionHelper constraintCollection;

	private readonly Utils.IndexCollectionHelper indexCollection;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbTableValuedFunction
	// =========================================================================================================


	public override IMetadataOrderedCollection<IParameter> Parameters
	{
		get
		{
			if (m_parameters == null)
			{
				Microsoft.SqlServer.Management.SmoMetadataProvider.Database parent = base.Parent.Parent;
				m_parameters = Utils.UserDefinedFunction.CreateParameterCollection(parent, m_smoMetadataObject.Parameters, GetModuleInfo());
			}
			return m_parameters;
		}
	}

	public TabularType TabularType => TabularType.TableValuedFunction;

	public IMetadataOrderedCollection<IColumn> Columns => columnCollection.MetadataCollection;

	public ITabular Unaliased => this;

	public IMetadataCollection<IConstraint> Constraints => constraintCollection.MetadataCollection;

	public IMetadataCollection<IIndex> Indexes => indexCollection.MetadataCollection;

	public IMetadataCollection<IStatistics> Statistics => Collection<IStatistics>.Empty;

	public bool IsInline => m_smoMetadataObject.FunctionType == UserDefinedFunctionType.Inline;

	public string TableVariableName
	{
		get
		{
			if (IsInline)
			{
				return null;
			}
			return m_smoMetadataObject.TableVariableName;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbTableValuedFunction
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
