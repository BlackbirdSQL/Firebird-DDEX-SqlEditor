using BlackbirdSql.Core.Ctl.Extensions;

namespace BlackbirdSql.Core.Ctl;


internal class EquivalencyDescribersEnumerator(PublicValueCollection<string, Describer> values)
	: AbstractDescribersEnumerator(values)
{
	public override bool IsValid(Describer descriptor)
	{
		return descriptor.IsEquivalency;
	}

}
