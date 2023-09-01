using System.Collections;




namespace BlackbirdSql.Core.Interfaces;


public interface IEnumerableAdvancedDescriptors
{
	IEnumerableAdvancedDescriptors Advanced { get; }

	IEnumerator GetEnumerator();
}
