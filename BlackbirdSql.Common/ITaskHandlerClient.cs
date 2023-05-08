using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TaskStatusCenter;

namespace BlackbirdSql.Common;

public interface ITaskHandlerClient
{
	ITaskHandler GetTaskHandler();
	TaskProgressData GetProgressData();
}
