// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.Extensibility.ExportableDescriptor<T>

using System;
using System.Collections.Generic;
using System.Linq;

using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Core.Model;

public abstract class ExportableDescriptor<T> where T : IBExportable
{
	public string DisplayName
	{
		get
		{
			if (Metadata == null)
			{
				return string.Empty;
			}

			return Metadata.DisplayMember;
		}
	}

	public abstract T Exportable { get; }

	public abstract IBExportableMetadata Metadata { get; }

	public string ExportableId => Metadata.Id;

	internal abstract ExtensionDescriptor<T, IBExportableMetadata> ExtensionDescriptor { get; }

	public bool Match(IBServerDefinition other)
	{
		if (other == null)
		{
			return false;
		}

		if (MatchMetaData(Metadata.EngineProduct, other.EngineProduct))
		{
			return MatchMetaData(Metadata.EngineType, other.EngineType);
		}

		return false;
	}

	public bool Match(string engineProduct, EnEngineType engineType)
	{
		if (MatchMetaData(Metadata.EngineProduct, engineProduct))
		{
			return MatchMetaData(Metadata.EngineType, engineType);
		}

		return false;
	}

	private bool MatchMetaData(string metaData, string requestedMetaData)
	{
		if (string.IsNullOrEmpty(metaData) || string.IsNullOrEmpty(requestedMetaData))
		{
			return true;
		}

		return metaData.Equals(requestedMetaData, StringComparison.OrdinalIgnoreCase);
	}

	private bool MatchMetaData(EnEngineType metaData, EnEngineType requestedMetaData)
	{

		return metaData == requestedMetaData;
	}

}

public static class ExportableDescriptorExtensionMembers
{
	public static ExportableDescriptor<T> FindMatchedDescriptor<T>(this IEnumerable<ExportableDescriptor<T>> exportableDescriptors, IBServerDefinition serverDefinition = null) where T : IBExportable
	{
		ExportableDescriptor<T> exportableDescriptor = null;
		IEnumerable<ExportableDescriptor<T>> enumerable = exportableDescriptors.FilterDescriptors(serverDefinition);
		IList<ExportableDescriptor<T>> source = enumerable == null ? new List<ExportableDescriptor<T>>() : enumerable.ToList();
		if (source.Count() > 1)
		{
			exportableDescriptor = source.OrderByDescending((x) => x.Metadata.Priority).FirstOrDefault();
		}
		return exportableDescriptor ?? source.FirstOrDefault();
	}

	public static IEnumerable<ExportableDescriptor<T>> FilterDescriptors<T>(this IEnumerable<ExportableDescriptor<T>> exportableDescriptors, IBServerDefinition serverDefinition = null) where T : IBExportable
	{
		if (exportableDescriptors == null)
		{
			return null;
		}
		IEnumerable<ExportableDescriptor<T>> source;
		if (serverDefinition == null)
		{
			source = exportableDescriptors;
		}
		else
		{
			IEnumerable<ExportableDescriptor<T>> enumerable = exportableDescriptors.Where((x) => x.Match(serverDefinition)).ToList();
			source = enumerable;
		}
		IList<ExportableDescriptor<T>> list = source.ToList();
		IList<ExportableDescriptor<T>> list2 = list.Where((x) => serverDefinition.HasSameEngine(x.Metadata)).ToList();
		if (list2.Any())
		{
			list = list2;
		}
		IList<ExportableDescriptor<T>> list3 = list.Where((x) => serverDefinition.HasSameType(x.Metadata)).ToList();
		if (list3.Any())
		{
			list = list3;
		}
		return list;
	}

}
