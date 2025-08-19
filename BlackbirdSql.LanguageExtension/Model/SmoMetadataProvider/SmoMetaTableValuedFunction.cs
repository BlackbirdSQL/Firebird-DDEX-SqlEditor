// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.TableValuedFunction

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											SmoMetaTableValuedFunction Class
//
/// <summary>
/// Impersonation of an SQL Server Smo TableValuedFunction for providing metadata.
/// </summary>
// =========================================================================================================
internal sealed class SmoMetaTableValuedFunction : AbstractSmoMetaUserDefinedFunction, ITableValuedFunction, IDatabaseTable, ITabular, IMetadataObject, IUserDefinedFunction, IFunction, IFunctionModuleBase, IUserDefinedFunctionModuleBase, ISchemaOwnedObject, IDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaTableValuedFunction
	// ---------------------------------------------------------------------------------


	public SmoMetaTableValuedFunction(Microsoft.SqlServer.Management.Smo.UserDefinedFunction function, SmoMetaSchema schema)
		: base(function, schema)
	{
		columnCollection = new SmoMetaColumn.CollectionHelperI(base.Parent.Database, this, _SmoMetadataObject.Columns);
		constraintCollection = new SmoMetaConstraint.CollectionHelperI(base.Parent.Database, this, _SmoMetadataObject);
		indexCollection = new AbstractSmoMetaIndex.CollectionHelperI(base.Parent.Database, this, _SmoMetadataObject.Indexes);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaTableValuedFunction
	// =========================================================================================================


	private IMetadataOrderedCollection<IParameter> _Parameters;

	private readonly SmoMetaColumn.CollectionHelperI columnCollection;

	private readonly SmoMetaConstraint.CollectionHelperI constraintCollection;

	private readonly AbstractSmoMetaIndex.CollectionHelperI indexCollection;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaTableValuedFunction
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

	public TabularType TabularType => TabularType.TableValuedFunction;

	public IMetadataOrderedCollection<IColumn> Columns => columnCollection.MetadataCollection;

	public ITabular Unaliased => this;

	public IMetadataCollection<IConstraint> Constraints => constraintCollection.MetadataCollection;

	public IMetadataCollection<IIndex> Indexes => indexCollection.MetadataCollection;

	public IMetadataCollection<IStatistics> Statistics => Collection<IStatistics>.Empty;

	public bool IsInline => _SmoMetadataObject.FunctionType == UserDefinedFunctionType.Inline;

	public string TableVariableName
	{
		get
		{
			if (IsInline)
			{
				return null;
			}
			return _SmoMetadataObject.TableVariableName;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaTableValuedFunction
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
