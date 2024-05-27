// Microsoft.Data.Tools.Components, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Components.Diagnostics.ISqlTraceTelemetryProvider
using System;
using System.Diagnostics;


namespace BlackbirdSql.Sys;

public interface IBSqlTraceTelemetryProvider
{
	void PostEvent(TraceEventType eventType, EnSqlTraceId traceId, Exception exception, int lineNumber = 0, string fileName = "", string memberName = "");
}
