using BlackbirdSql.Core.Ctl.Extensions;

namespace BlackbirdSql.Core.Ctl;


internal class ConnectionDescribersEnumerator : AbstractDescribersEnumerator
{

	public ConnectionDescribersEnumerator(PublicValueCollection<string, Describer> values)
		: base(values)
	{
	}


	public override bool IsValid(Describer descriptor)
	{
		return descriptor.IsConnectionParameter;
	}

}
