using System.Collections;


namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IBEnumerableConnectionDescriptors
{
	IBEnumerableConnectionDescriptors ConnectionProperties { get; }

	IEnumerator GetEnumerator();
}
