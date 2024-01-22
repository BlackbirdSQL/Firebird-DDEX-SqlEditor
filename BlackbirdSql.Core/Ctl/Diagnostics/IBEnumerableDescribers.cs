using System.Collections;
using System.Collections.Generic;


namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IBEnumerableDescribers<T> : IEnumerable<Describer> where T: EnumeratorDescribers
{
	// IEnumerable<Describer> DescriberEnumerator { get; }
}
