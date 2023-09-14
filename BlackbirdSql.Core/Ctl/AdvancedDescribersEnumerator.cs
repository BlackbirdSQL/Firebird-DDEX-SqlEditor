using BlackbirdSql.Core.Ctl.Extensions;

namespace BlackbirdSql.Core.Ctl;


internal class AdvancedDescribersEnumerator : AbstractDescribersEnumerator
{

	public AdvancedDescribersEnumerator(PublicValueCollection<string, Describer> values)
		: base(values)
	{
	}


	public override bool IsValid(Describer descriptor)
	{
		return descriptor.IsAdvanced;
	}

}
