// Microsoft.SqlServer.ConnectionInfo, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.Common.SqlConnectionInfo

using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;


namespace BlackbirdSql.Common.Model;

public class DslConnectionInfo : AbstractModelPropertyAgent
{
	public DslConnectionInfo() : base()
	{
	}

	public DslConnectionInfo(DslConnectionInfo lhs, bool generateNewId) : base(lhs, generateNewId)
	{
	}

	public DslConnectionInfo(DslConnectionInfo lhs) : base(lhs)
	{
	}


	public DslConnectionInfo(string server, int port, string database, string userName, string password)
		: base(server, port, database, userName, password)
	{
	}



	public override IBPropertyAgent Copy()
	{
		return new DslConnectionInfo(this, generateNewId: true);
	}


}
