#region Assembly Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;

namespace BlackbirdSql.Common.Ctl;


public class ExtensionProperties : CompositionProperties
{
	private IList<string> _SupportedEngineProducts;

	private IList<EnEngineType> _SupportedEngineTypes;

	private static readonly IList<string> SDefaultEngineProducts = new List<string> { "Firebird" };

	private static readonly IList<EnEngineType> SDefaultEngineTypes
		= new List<EnEngineType> { EnEngineType.LocalClassicServer, EnEngineType.EmbeddedDatabase };

	public IEnumerable<string> SupportedEngineProducts
	{
		get
		{
			return _SupportedEngineProducts ??= SDefaultEngineProducts;
		}
	}

	public IEnumerable<EnEngineType> SupportedEngineTypes
	{
		get
		{
			return _SupportedEngineTypes ??= SDefaultEngineTypes;
		}
	}

	public ExtensionProperties(IEnumerable<string> supportedEngineProducts = null, IEnumerable<EnEngineType> supportedEngineTypes = null)
	{
		PartCreationPolicy = CreationPolicy.NonShared;
		UseDefaultCatalog = true;
		Catalogs = new ComposablePartCatalog[0];
		_SupportedEngineProducts = new List<string>(supportedEngineProducts ?? SDefaultEngineProducts);
		_SupportedEngineTypes = new List<EnEngineType>(supportedEngineTypes ?? SDefaultEngineTypes);
	}

	public ExtensionProperties(bool useDefaultLocations, IEnumerable<string> supportedEngineProducts = null,
		IEnumerable<EnEngineType> supportedEngineTypes = null) : this(supportedEngineProducts, supportedEngineTypes)
	{
		UseDefaultCatalog = useDefaultLocations;
	}

	public bool HasNoExtensionsLookupSet()
	{
		if (!UseDefaultCatalog && !HasCatalogs() && !HasExportProviders())
		{
			return string.IsNullOrEmpty(AssemblyLookupPath);
		}

		return false;
	}

	private bool HasCatalogs()
	{
		if (Catalogs != null)
		{
			return Catalogs.Any();
		}

		return false;
	}

	private bool HasExportProviders()
	{
		if (Providers != null)
		{
			return Providers.Any();
		}

		return false;
	}

	public void AddEngineProduct(string engineProduct)
	{
		if (!_SupportedEngineProducts.Contains(engineProduct))
		{
			_SupportedEngineProducts.Add(engineProduct);
		}
	}

	public void AddEngineType(EnEngineType engineType)
	{
		if (!_SupportedEngineTypes.Contains(engineType))
		{
			_SupportedEngineTypes.Add(engineType);
		}
	}

	public bool SupportsMetadata(IBExportableMetadata extensionMetadata)
	{
		if (string.IsNullOrEmpty(extensionMetadata.EngineProduct) || SupportedEngineProducts != null && SupportedEngineProducts.Any((x) => x.Equals(extensionMetadata.EngineProduct, StringComparison.OrdinalIgnoreCase)))
		{
			if (!string.IsNullOrEmpty(extensionMetadata.EngineProduct))
			{
				if (SupportedEngineTypes != null)
				{
					return SupportedEngineTypes.Any((x) => x == extensionMetadata.EngineType);
				}

				return false;
			}

			return true;
		}

		return false;
	}
}
