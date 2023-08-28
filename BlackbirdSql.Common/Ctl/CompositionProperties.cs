#region Assembly Microsoft.SqlServer.Tools.Extensibility, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\Microsoft.SqlServer.Tools.Extensibility.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace BlackbirdSql.Common.Ctl;


public class CompositionProperties
{
	public bool UseDefaultCatalog { get; set; }

	public string AssemblyLookupPath { get; set; }

	public IEnumerable<ComposablePartCatalog> Catalogs { get; set; }

	public ExportProvider[] Providers { get; set; }

	public CreationPolicy PartCreationPolicy { get; set; }

	public Func<Export, bool> Filter { get; set; }

	public CompositionProperties()
	{
		UseDefaultCatalog = true;
		PartCreationPolicy = CreationPolicy.Any;
	}

	public void AddCatalogs(IEnumerable<ComposablePartCatalog> catalogsToAdd)
	{
		List<ComposablePartCatalog> list = new List<ComposablePartCatalog>();
		if (Catalogs != null)
		{
			list.AddRange(Catalogs);
		}

		if (catalogsToAdd != null)
		{
			list.AddRange(catalogsToAdd);
		}

		Catalogs = list;
	}

	public void AddCatalog(ComposablePartCatalog catalogToAdd)
	{
		AddCatalogs(new ComposablePartCatalog[1] { catalogToAdd });
	}
}
