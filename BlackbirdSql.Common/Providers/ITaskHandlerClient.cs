using Microsoft.VisualStudio.TaskStatusCenter;


namespace BlackbirdSql.Common.Providers;


public interface ITaskHandlerClient
{
	ITaskHandler GetTaskHandler();
	TaskProgressData GetProgressData();
}
