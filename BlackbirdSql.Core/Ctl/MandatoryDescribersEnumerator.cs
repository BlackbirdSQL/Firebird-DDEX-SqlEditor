using BlackbirdSql.Core.Ctl.Extensions;

namespace BlackbirdSql.Core.Ctl;


internal class MandatoryDescribersEnumerator : AbstractDescribersEnumerator
{

	public MandatoryDescribersEnumerator(PublicValueCollection<string, Describer> values)
		: base(values)
	{
	}


	public override bool IsValid(Describer descriptor)
	{
		return descriptor.IsMandatory;
	}

}
