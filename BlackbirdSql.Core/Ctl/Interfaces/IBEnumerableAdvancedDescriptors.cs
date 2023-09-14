using System.Collections;


namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IBEnumerableAdvancedDescriptors
{
	IBEnumerableAdvancedDescriptors Advanced { get; }

	IEnumerator GetEnumerator();
}
