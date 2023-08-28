
using BlackbirdSql.Core.Extensions;




namespace BlackbirdSql.Core;


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
