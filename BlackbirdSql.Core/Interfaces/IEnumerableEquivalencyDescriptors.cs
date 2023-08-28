using System.Collections;




namespace BlackbirdSql.Core.Interfaces;


public interface IEnumerableEquivalencyDescriptors
{
	IEnumerableEquivalencyDescriptors Equivalency { get; }

	IEnumerator GetEnumerator();
}
