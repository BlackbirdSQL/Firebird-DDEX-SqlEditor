
using Microsoft.VisualStudio.TaskStatusCenter;


namespace BlackbirdSql.Common.Extensions;


public interface ITaskHandlerClient
{
	ITaskHandler GetTaskHandler();
	TaskProgressData GetProgressData();
}
