using System.Collections;


namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IBEnumerableEquivalencyDescriptors
{
	IBEnumerableEquivalencyDescriptors Equivalency { get; }

	IEnumerator GetEnumerator();
}
