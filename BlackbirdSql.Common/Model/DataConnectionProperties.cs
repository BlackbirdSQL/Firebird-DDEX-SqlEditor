

using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;


namespace BlackbirdSql.Common.Model;

// namespace Microsoft.SqlServer.ConnectionDlg.Core
public class DataConnectionProperties : AbstractModelPropertyAgent
{

	public DataConnectionProperties() : base()
	{
	}

	public DataConnectionProperties(AbstractModelPropertyAgent lhs, bool generateNewId) : base(lhs, generateNewId)
	{
	}

	public DataConnectionProperties(AbstractModelPropertyAgent lhs) : base(lhs)
	{
	}




	public override IBPropertyAgent Copy()
	{
		return new DataConnectionProperties(this, generateNewId: true);
	}


}
