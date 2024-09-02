// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.QueryExecutor.StatusType


using System;

namespace BlackbirdSql.Shared.Enums;

[Flags]
public enum EnQueryState
{
	None = 0,
	// -----------------------
	// Virtual states
	// -----------------------
	// All states before the Virtual marker are Virtual / dynamic:
	//	1. They are dynamic and used for creating a temporary conditional
	//		during an Operation/Action, similar to an event.
	//	2. They never generate a state change event irrelevant of their
	//		previous value.
	//	3. They do not get pushed onto or popped off of the syncronicity stack.
	//	4. A state change event cannot be cloned for them after an antecedent
	//		state has been popped off of the stack.
	//	5. They become volatile when there are operations/actions on the stack.
	//		ie. Until there are operations on the stack they are non-volatile.
	//	6. After, and only after, an Action or the Operation is popped off of
	//		the stack, and there are no more Actions, then all virtual states
	//		will be reset. IOW, on the hypothetical 'OnAfterLastActionRemoved'
	//		or za-'OnOperationRemovedAndNoActionsExist" events.
	DatabaseChanged = 0x1,
	Faulted = 0x2,

	VirtualMarker = 0x8,

	// -----------------------
	// Fixed states
	// -----------------------
	Online = 0x10,
	Connected = 0x20,

	OperationMarker = 0x80,

	// -----------------------
	// Operations
	// -----------------------
	// An Operation/Action cannot stack before a fixed state. Inversely, fixed
	// states are pushed onto the stack ahead of any existing operations.
	// There should only ever be no more than one pure Operation on the stack,
	// but there can be multiple Actions.
	Executing = 0x100,

	ActionMarker = 0x800,
	// -----------------------
	// Actions
	// -----------------------
	// Actions are a subset of Operations.
	// Once the last action is popped and the current state flags are assigned to its
	// payload, all virtual states are cleared.
	Cancelling = 0x1000,
	Connecting = 0x2000,
	Prompting = 0x4000

	// Operations vs Actions
	// Both make virtual states volatile but Actions differ subtly in
	// functionality to pure Operations in that popping the last Action
	// always clears virtual flags, even if a pure Operation is on the
	// stack, but popping a pure Operation requires that there be no
	// Actions on the stack before virtual flags will be cleared.
	// So in the case where there is an Operation and one Action on the stack,
	// for example Executing + Connecting, then if Executing is popped, Virtual
	// states remain; but if Connecting is popped, Virtual states are cleared.
}
