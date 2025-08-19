// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.SpatialIndex

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											SmoMetaSpatialIndex Class
//
/// <summary>
/// Impersonation of an SQL Server Smo SpatialIndex for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaSpatialIndex : AbstractSmoMetaIndex, ISpatialIndex, IIndex, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaSpatialIndex
	// ---------------------------------------------------------------------------------


	public SmoMetaSpatialIndex(IDatabaseTable parent, Microsoft.SqlServer.Management.Smo.Index smoIndex)
		: base(parent, smoIndex)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Property accessors - SmoMetaSpatialIndex
	// =========================================================================================================


	public double BoundingBoxXMax => GetBoundingBoxValue("BoundingBoxXMax");

	public double BoundingBoxXMin => GetBoundingBoxValue("BoundingBoxXMin");

	public double BoundingBoxYMax => GetBoundingBoxValue("BoundingBoxYMax");

	public double BoundingBoxYMin => GetBoundingBoxValue("BoundingBoxYMin");

	public int CellsPerObject
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoIndex, "CellsPerObject", out int? value);
			if (!value.HasValue)
			{
				return 16;
			}
			return value.Value;
		}
	}

	public IColumn IndexedColumn
	{
		get
		{
			string name = _SmoIndex.IndexedColumns[0].Name;
			return _Parent.Columns[name];
		}
	}

	public GridDensity Level1Density => GetGridDensity(_SmoIndex.Level1Grid);

	public GridDensity Level2Density => GetGridDensity(_SmoIndex.Level2Grid);

	public GridDensity Level3Density => GetGridDensity(_SmoIndex.Level3Grid);

	public GridDensity Level4Density => GetGridDensity(_SmoIndex.Level4Grid);

	public bool NoAutomaticRecomputation
	{
		get
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)_SmoIndex, "NoAutomaticRecomputation", out bool? value);
			return value.GetValueOrDefault();
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaSpatialIndex
	// =========================================================================================================


	public override T Accept<T>(IMetadataObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}

	private static GridDensity GetGridDensity(SpatialGeoLevelSize spatialGeoLevelSize)
	{
		switch (spatialGeoLevelSize)
		{
		case SpatialGeoLevelSize.High:
			return GridDensity.High;
		case SpatialGeoLevelSize.Low:
			return GridDensity.Low;
		case SpatialGeoLevelSize.None:
		case SpatialGeoLevelSize.Medium:
			return GridDensity.Medium;
		default:
			return GridDensity.Medium;
		}
	}

	private double GetBoundingBoxValue(string propertyName)
	{
		Cmd.TryGetPropertyValue((SqlSmoObject)_SmoIndex, propertyName, out double? value);
		return value.GetValueOrDefault();
	}


	#endregion Methods

}
