// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Synonym

using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											SmoMetaSynonym Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Synonym for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaSynonym : AbstractSmoMetaSchemaOwnedObject<Microsoft.SqlServer.Management.Smo.Synonym>, ISynonym, ISchemaOwnedObject, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaSynonym
	// ---------------------------------------------------------------------------------


	public SmoMetaSynonym(Microsoft.SqlServer.Management.Smo.Synonym smoMetadataObject, SmoMetaSchema parent)
		: base(smoMetadataObject, parent)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaSynonym
	// =========================================================================================================


	private string baseObjectName;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaSynonym
	// =========================================================================================================


	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => false;

	public string BaseObjectName
	{
		get
		{
			if (baseObjectName == null)
			{
				List<string> list = new List<string>(4);
				Cmd.TryGetPropertyObject<string>(_SmoMetadataObject, "BaseServer", out var value);
				Cmd.TryGetPropertyObject<string>(_SmoMetadataObject, "BaseDatabase", out var value2);
				Cmd.TryGetPropertyObject<string>(_SmoMetadataObject, "BaseSchema", out var value3);
				Cmd.TryGetPropertyObject<string>(_SmoMetadataObject, "BaseObject", out var value4);
				AddEscapedNamePart(list, value);
				AddEscapedNamePart(list, value2);
				AddEscapedNamePart(list, value3);
				AddEscapedNamePart(list, value4);
				baseObjectName = string.Join(".", [.. list]);
			}
			return baseObjectName;
		}
	}

	public Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType BaseType => GetSynonymBaseType(_SmoMetadataObject.BaseType);


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaSynonym
	// =========================================================================================================


	public override T Accept<T>(ISchemaOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}

	private static Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType GetSynonymBaseType(Microsoft.SqlServer.Management.Smo.SynonymBaseType smoSynonymBaseType)
	{
		return smoSynonymBaseType switch
		{
			Microsoft.SqlServer.Management.Smo.SynonymBaseType.None => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.None, 
			Microsoft.SqlServer.Management.Smo.SynonymBaseType.Table => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.Table, 
			Microsoft.SqlServer.Management.Smo.SynonymBaseType.View => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.View, 
			Microsoft.SqlServer.Management.Smo.SynonymBaseType.SqlStoredProcedure => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.SqlStoredProcedure, 
			Microsoft.SqlServer.Management.Smo.SynonymBaseType.SqlScalarFunction => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.SqlScalarFunction, 
			Microsoft.SqlServer.Management.Smo.SynonymBaseType.SqlTableValuedFunction => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.SqlTableValuedFunction, 
			Microsoft.SqlServer.Management.Smo.SynonymBaseType.SqlInlineTableValuedFunction => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.SqlInlineTableValuedFunction, 
			Microsoft.SqlServer.Management.Smo.SynonymBaseType.ExtendedStoredProcedure => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.ExtendedStoredProcedure, 
			Microsoft.SqlServer.Management.Smo.SynonymBaseType.ReplicationFilterProcedure => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.ReplicationFilterProcedure, 
			Microsoft.SqlServer.Management.Smo.SynonymBaseType.ClrStoredProcedure => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.ClrStoredProcedure, 
			Microsoft.SqlServer.Management.Smo.SynonymBaseType.ClrScalarFunction => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.ClrScalarFunction, 
			Microsoft.SqlServer.Management.Smo.SynonymBaseType.ClrTableValuedFunction => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.ClrTableValuedFunction, 
			Microsoft.SqlServer.Management.Smo.SynonymBaseType.ClrAggregateFunction => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.ClrAggregateFunction, 
			_ => Microsoft.SqlServer.Management.SqlParser.Metadata.SynonymBaseType.None, 
		};
	}

	private static void AddEscapedNamePart(List<string> nameParts, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			nameParts.Add(Cmd.EscapeSqlIdentifier(value));
		}
		else if (nameParts.Count > 0)
		{
			nameParts.Add(string.Empty);
		}
	}


	#endregion Methods

}
