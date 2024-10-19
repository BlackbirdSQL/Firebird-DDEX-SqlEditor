using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackbirdSql.Sys.Interfaces;

public interface IBsEventSource
{
	void Debug(Type type, string eventType, string functionName, string format, params object[] args);
}
