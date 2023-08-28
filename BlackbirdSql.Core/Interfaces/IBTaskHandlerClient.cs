
using Microsoft.VisualStudio.TaskStatusCenter;


namespace BlackbirdSql.Core.Interfaces;


// =========================================================================================================
//										IBTaskHandlerClient Interface
//
/// <summary>
/// Interface for TaskHandler and Statusbar functionality.
/// Provides a callback to a <see cref="Diag.TaskHandlerProgress"/> client to verify that it's
/// TaskHandler is still valid before proceeding with the async call.
/// </summary>
// =========================================================================================================
public interface IBTaskHandlerClient
{
	/// <summary>
	/// The name of the running task if the object is currently using the task handler.
	/// </summary>
	string TaskHandlerTaskName { get; }

	/// <summary>
	/// Provides a callback to the client's ProgressData.
	/// </summary>
	TaskProgressData GetProgressData();

	/// <summary>
	/// Provides a callback to a <see cref="Diag.TaskHandlerProgress"/> client to verify that it's
	/// TaskHandler is still valid before proceeding with the async call.
	/// </summary>
	ITaskHandler GetTaskHandler();

	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar
	/// with updated TaskHandlerTaskName information.
	/// </summary>
	bool TaskHandlerProgress(string text);

	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar.
	/// </summary>
	/// <param name="progress">The % completion of TaskHandlerTaskName.</param>
	/// <param name="elapsed">The time taken to complete the stage.</param>
	bool TaskHandlerProgress(int progress, int elapsed);

	/// <summary>
	/// Updates the VisualStudio status bar.
	/// </summary>
	/// <param name="message">Thye status bar display text.</param>
	/// <param name="complete">Tags this message as a completion message.</param>
	/// <returns></returns>
	bool UpdateStatusBar(string message, bool complete = false);

}
