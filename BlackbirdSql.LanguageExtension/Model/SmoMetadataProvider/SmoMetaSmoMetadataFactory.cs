// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.SmoMetadataFactory

using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


/// <summary>
/// Placeholder. Under development.
/// </summary>
internal class SmoMetaSmoMetadataFactory : MetadataFactory
{

	internal static SmoMetaSmoMetadataFactory Instance => SingletonI.Instance;

	private SmoMetaSmoMetadataFactory()
	{
		// Evs.Trace(typeof(SmoMetaSmoMetadataFactory), ".ctor");
	}



	private static class SingletonI
	{
		internal static SmoMetaSmoMetadataFactory Instance;

		static SingletonI()
		{
			Instance = new SmoMetaSmoMetadataFactory();
		}
	}
}
