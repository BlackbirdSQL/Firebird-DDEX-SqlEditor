using System;
using System.Collections.Generic;



namespace BlackbirdSql.VisualStudio.DataTools.Configuration;


internal static class SupportedObjects
{

	/*
	 * _useProviderObjectFactory
	 * Setting this to false inserts registry values against implementations
	 * Setting this to true only creates the key for each implementation
	*/
	const bool _useProviderObjectFactory = false;

	public static readonly IDictionary<string, int> Implementations = new Dictionary<string, int>()
	{
		{ "DataConnectionProperties", _useProviderObjectFactory ? 0 : 1 },
		{ "DataConnectionSupport", _useProviderObjectFactory ? 0 : 1 },
		{ "DataConnectionUIControl", _useProviderObjectFactory ? 0 : 1 },
		{ "DataObjectSupport", _useProviderObjectFactory ? 0 : 1 },
		{ "DataSourceInformation", _useProviderObjectFactory ? 0 : 1 },
		{ "DataViewSupport", _useProviderObjectFactory ? 0 : 1 }
	};


	public static readonly IDictionary<string, RegistryValue> Values = new Dictionary<string, RegistryValue>()
	{
		{ "DataConnectionProperties:0", new(null, "BlackbirdSql.VisualStudio.DataTools.ConnectionProperties") },
		{ "DataConnectionSupport:0", new (null, "BlackbirdSql.VisualStudio.DataTools.ConnectionSupport") },
		{ "DataConnectionUIControl:0", new(null, "BlackbirdSql.VisualStudio.DataTools.ConnectionUIControl") },
		{ "DataObjectSupport:0", new(null,  "BlackbirdSql.VisualStudio.DataTools.ObjectSupport") },
		{ "DataSourceInformation:0", new(null,  "BlackbirdSql.VisualStudio.DataTools.SourceInformation") },
		{ "DataViewSupport:0", new(null,  "BlackbirdSql.VisualStudio.DataTools.ViewSupport") }
	};

	public class RegistryValue
	{
		public readonly string Name;
		public readonly Object Value;

		public RegistryValue(string name, Object value)
		{
			Name = name;
			Value = value;
		}
	}

}
