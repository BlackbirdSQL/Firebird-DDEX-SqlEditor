
using System.Collections;
using BlackbirdSql.Core.Ctl.Extensions;

namespace BlackbirdSql.Core.Ctl;


internal abstract class AbstractDescribersEnumerator : IEnumerator
{
	private readonly PublicValueCollection<string, Describer> _Owner;
	private PublicValueCollection<string, Describer>.Enumerator _Enumerator;

	public AbstractDescribersEnumerator(PublicValueCollection<string, Describer> values)
	{
		_Owner = values;
		_Enumerator = values.GetEnumerator();
		Reset();
	}

	public object Current
	{
		get
		{
			return _Enumerator.Current;
		}
	}

	public bool MoveNext()
	{
		if (_Enumerator.Index >= _Owner.Count)
			return false;

		do
		{
			if (!_Enumerator.MoveNext())
				return false;

		} while (!IsValid(_Enumerator.Current));

		return true;
	}

	public abstract bool IsValid(Describer describer);

	public void Reset()
	{
		_Enumerator.Reset();
	}
}
