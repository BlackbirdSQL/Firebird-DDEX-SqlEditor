// System.Windows.Forms, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// System.Windows.Forms.PropertyValueChangedEventArgs
using System;
using System.Windows.Forms;


namespace BlackbirdSql.Core.Controls.Events;

public class GridItemValueChangedEventArgs(GridItem changedItem, object oldValue) : EventArgs
{
	public GridItem ChangedItem { get; } = changedItem;

	public bool ReadOnlyChanged { get; set; } = false;

	/// <summary>The value of the grid item before it was changed.</summary>
	/// <returns>A object representing the old value of the property.</returns>
	/// <filterpriority>1</filterpriority>
	public object OldValue { get; } = oldValue;
}
