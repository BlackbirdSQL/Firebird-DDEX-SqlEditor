namespace BlackbirdSql.Sys.Enums;

/// <summary>
/// The current state of the payload launcher task.
/// </summary>
public enum EnLauncherPayloadLaunchState
{
	/// <summary>
	/// The launcher task is null or Completed.
	/// </summary>
	Inactive = 0,

	/// <summary>
	/// The launcher task is active but has not started launching it's payload.
	/// </summary>
	Pending,

	/// <summary>
	/// The launcher task is active and is busy launching it's payload.
	/// </summary>
	Launching,

	Discarded,

	/// <summary>
	/// A solution is closing or the ide is shutting down.
	/// </summary>
	Shutdown

}
