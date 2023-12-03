// System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// System.Windows.Forms.SelectedGridItemChangedEventArgs
using System;
using System.Windows.Forms;

namespace BlackbirdSql.Core.Controls.Events;

public class SelectedGridItemFocusEventArgs(GridItem selectionItem) : EventArgs
{
	private bool _ValidateValue = false;
	private string _Value = null;
	private readonly GridItem _SelectionItem = selectionItem;


	public GridItem SelectionItem => _SelectionItem;

	public bool ValidateValue
	{
		get { return _ValidateValue; }
		set { _ValidateValue = value; }
	}

	public string Value
	{
		get { return _Value; }
		set { _Value = value; }
	}
}
