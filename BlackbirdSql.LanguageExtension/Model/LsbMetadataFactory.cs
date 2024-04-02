// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.SmoMetadataFactory
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model;


internal class LsbMetadataFactory : MetadataFactory
{
	private static class Singleton
	{
		public static LsbMetadataFactory Instance;

		static Singleton()
		{
			Instance = new LsbMetadataFactory();
		}
	}

	public static LsbMetadataFactory Instance => Singleton.Instance;

	private LsbMetadataFactory()
	{
	}
}
