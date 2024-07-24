
using System;



namespace BlackbirdSql.Core.Interfaces;


public interface IBsDataConnectionPromptDialogHandler : IDisposable
{
	string CompleteConnectionString { get; }
	string PublicConnectionString { set; }

	bool ShowDialog();
}
