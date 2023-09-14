// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.ServerDefinition

using System.Globalization;

using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Core.Ctl;

public sealed class ServerDefinition : IBServerDefinition
{

	private static readonly ServerDefinition _Default = new ServerDefinition("Firebird", EnEngineType.Unknown);

	public string EngineProduct { get; set; }

	public EnEngineType EngineType { get; set; }

	public string EngineTypeName => EngineType.ToString();

	public static ServerDefinition Default => _Default;

	public ServerDefinition(string engineProduct, EnEngineType engineType)
	{
		EngineProduct = engineProduct;
		EngineType = engineType;
	}

	public string Key =>
		string.Format(CultureInfo.InvariantCulture, "{0}", EngineType.ToString().ToUpperInvariant());

}
