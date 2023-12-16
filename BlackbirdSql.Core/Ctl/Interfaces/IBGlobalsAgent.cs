

using System;
using System.IO;
using BlackbirdSql.Core.Ctl.Events;
using EnvDTE;

using Microsoft.VisualStudio.Shell.Interop;




namespace BlackbirdSql.Core.Ctl.Interfaces;


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


