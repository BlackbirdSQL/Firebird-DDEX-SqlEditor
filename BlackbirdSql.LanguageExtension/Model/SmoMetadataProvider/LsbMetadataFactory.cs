// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.SmoMetadataFactory

using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


/// <summary>
/// Placeholder. Under development.
/// </summary>
internal class LsbMetadataFactory : MetadataFactory
{

	internal static LsbMetadataFactory Instance => SingletonI.Instance;

	private LsbMetadataFactory()
	{
		Evs.Trace(typeof(LsbMetadataFactory), ".ctor");
	}



	private static class SingletonI
	{
		internal static LsbMetadataFactory Instance;

		static SingletonI()
		{
			Instance = new LsbMetadataFactory();
		}
	}
}
