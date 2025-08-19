// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.View

using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Smo;
// using Microsoft.SqlServer.Management.SmoMetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.Parser;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


internal class SmoMetaView : AbstractSmoMetaTableView<Microsoft.SqlServer.Management.Smo.View>, IView, ITableViewBase, IDatabaseTable, ITabular, IMetadataObject, ISchemaOwnedObject, IDatabaseObject
{
	public SmoMetaView(Microsoft.SqlServer.Management.Smo.View smoMetadataObject, SmoMetaSchema parent)
		: base(smoMetadataObject, parent)
	{
		constraintCollection = new SmoMetaConstraint.CollectionHelperI(base.Parent.Database, this, _SmoMetadataObject);
		indexCollection = new AbstractSmoMetaIndex.CollectionHelperI(base.Parent.Database, this, _SmoMetadataObject.Indexes);
		statisticsCollection = new Cmd.StatisticsCollectionHelperI(base.Parent.Database, this, _SmoMetadataObject.Statistics);
		triggerCollection = new SmoMetaDmlTrigger.CollectionHelperI(base.Parent.Database, this, _SmoMetadataObject.Triggers);
	}




	private readonly SmoMetaConstraint.CollectionHelperI constraintCollection;

	private readonly AbstractSmoMetaIndex.CollectionHelperI indexCollection;

	private readonly Cmd.StatisticsCollectionHelperI statisticsCollection;

	private readonly SmoMetaDmlTrigger.CollectionHelperI triggerCollection;

	private bool viewInfoRetrieved;

	private IDictionary<string, object> viewInfo;

	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => _SmoMetadataObject.IsSystemObject;

	public override TabularType TabularType => TabularType.View;

	protected override SmoMetaConstraint.CollectionHelperI ConstraintCollection => constraintCollection;

	protected override AbstractSmoMetaIndex.CollectionHelperI IndexCollection => indexCollection;

	protected override Cmd.StatisticsCollectionHelperI StatisticsCollection => statisticsCollection;

	protected override SmoMetaDmlTrigger.CollectionHelperI TriggerCollection => triggerCollection;

	public bool HasCheckOption
	{
		get
		{
			IDictionary<string, object> dictionary = GetViewInfo();
			if (dictionary != null)
			{
				return (bool)dictionary[PropertyKeys.HasCheckOption];
			}
			return false;
		}
	}

	public bool IsEncrypted => _SmoMetadataObject.IsEncrypted;

	public bool IsSchemaBound => _SmoMetadataObject.IsSchemaBound;

	public string QueryText
	{
		get
		{
			IDictionary<string, object> dictionary = GetViewInfo();
			if (dictionary == null)
			{
				return null;
			}
			return (string)dictionary[PropertyKeys.QueryDefinition];
		}
	}

	public bool ReturnsViewMetadata
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoMetadataObject, "ReturnsViewMetadata", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public override bool IsQuotedIdentifierOn
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoMetadataObject, "QuotedIdentifierStatus", out bool? value);
			return value.GetValueOrDefault();
		}
	}

	public bool HasColumnSpecification => _SmoMetadataObject.HasColumnSpecification;

	public override T Accept<T>(ISchemaOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}



	private IDictionary<string, object> GetViewInfo()
	{
		if (!viewInfoRetrieved)
		{
			string definitionTest = Cmd.ModuleI.GetDefinitionTest(_SmoMetadataObject);
			if (!string.IsNullOrEmpty(definitionTest))
			{
				viewInfo = ParseUtils.RetrieveViewDefinition(definitionTest, new ParseOptions(string.Empty, IsQuotedIdentifierOn));
			}
			viewInfoRetrieved = true;
		}
		return viewInfo;
	}
}
