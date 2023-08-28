using System.Collections;




namespace BlackbirdSql.Core.Interfaces;


public interface IEnumerableMandatoryDescriptors
{
	IEnumerableMandatoryDescriptors Mandatory { get; }

	IEnumerator GetEnumerator();
}
