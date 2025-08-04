using System.Collections.Generic;
using BlackbirdSql.Sys.Ctl;


namespace BlackbirdSql.Sys.Interfaces;

internal interface IBsEnumerableDescribers<T> : IEnumerable<Describer> where T : EnumeratorDescribers
{
	// IEnumerable<Describer> DescriberEnumerator { get; }
}
