using System;
using Microsoft.VisualStudio.Data.Services;


namespace BlackbirdSql.Core.Interfaces;

public interface IBsRctEventSink : IDisposable
{
	bool Initialized { get; }
}
