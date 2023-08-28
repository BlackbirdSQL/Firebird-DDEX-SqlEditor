
using BlackbirdSql.Core.Extensions;




namespace BlackbirdSql.Core;


internal class EquivalencyDescribersEnumerator : AbstractDescribersEnumerator
{

	public EquivalencyDescribersEnumerator(PublicValueCollection<string, Describer> values)
		: base(values)
	{
	}


	public override bool IsValid(Describer descriptor)
	{
		return descriptor.IsEquivalency;
	}

}
