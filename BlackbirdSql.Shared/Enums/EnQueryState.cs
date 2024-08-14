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
	DatabaseChanged = 0x1,
	Faulted = 0x2,
	// All states before this marker are virtual / dynamic:
	//	1. They are volatile / dynamic and used for creating a
	//		temporary conditional during an action.
	//	2. They never generate a state change event irrelevant
	//		of their previous value.
	//	3. They do not get pushed onto or popped off of the stack.
	//	4. Their state change event cannot be cloned after an
	//		antecedent state has been popped off of the stack.
	//	5. They become volatile when there are actions on the stack.
	//	6. After, and only after, an action is popped off of the stack,
	//		and there are no more actions, then all virtual states will
	//		be reset. IOW, on the imaginery 'OnAfterLastActionRemoved'
	//		event.
	VirtualMarker = 0x8,
	// -----------------------
	// Fixed states
	// -----------------------
	Online = 0x10,
	Connected = 0x20,
	// An action cannot stack before a state. Inversely, fixed states are
	// pushed onto the stack ahead of any existing actions.
	// Once the last action is popped and the current state flags assigned to its
	// payload, all virtual states are cleared.
	ActionMarker = 0x80,
	// -----------------------
	// Actions
	// -----------------------
	Executing = 0x100,
	Cancelling = 0x200,
	Connecting = 0x400,
	Prompting = 0x800
}
