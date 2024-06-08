

using System;
using System.IO;




namespace BlackbirdSql.Core.Interfaces;


public interface IBGlobalsAgent : IDisposable
{

	bool IsConfiguredDbProviderStatus { get; }
	bool IsConfiguredEFStatus { get; }
	bool IsScannedStatus { get; set; }
	bool IsUpdatedEdmxsStatus { get; set; }
	bool IsValidatedStatus { get; }
	bool IsValidStatus { get; set; }
	bool IsValidatedDbProviderStatus { set; }
	bool IsValidatedEFStatus { set; }



	bool IsValidateFailedStatus { get; set; }
	int Value { get; set; }
	bool VariableExists { get; }


	bool Flush();
	bool Flush(Stream stream);


	bool ClearValidateStatus();

}


