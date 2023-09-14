using System.Collections;


namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IBEnumerableMandatoryDescriptors
{
	IBEnumerableMandatoryDescriptors Mandatory { get; }

	IEnumerator GetEnumerator();
}
