
using BlackbirdSql.Core.Extensions;




namespace BlackbirdSql.Core;


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
