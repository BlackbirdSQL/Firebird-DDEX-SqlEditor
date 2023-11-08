// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Converters.ToggleButtonConverterHelper

using System;
using System.Windows;
using System.Windows.Media;
using BlackbirdSql.Core;

namespace BlackbirdSql.Common.Controls.Converters.Wpf;


public class ToggleButtonConverterHelper
{
	public static bool IsButtonFilled(bool isExpanded, bool isMouseover, bool isListItemHighlighted)
	{
		if (isExpanded)
		{
			if (isMouseover && isListItemHighlighted)
			{
				return false;
			}
			return true;
		}
		if (isMouseover && isListItemHighlighted)
		{
			return true;
		}
		return false;
	}

	public static SolidColorBrush GetButtonColorBrush(bool isExpanded, bool isMouseOverToggleButton, bool isListItemSelected, bool isMouseOverListItem, bool isListFocused)
	{
		if (IsListItemHighlighted(isListItemSelected, isListFocused, isMouseOverListItem))
		{
			return GetColorBrush("ItemSelectedTextBrushKey");
		}
		if (isListItemSelected && !isListFocused)
		{
			if (isMouseOverToggleButton)
			{
				return GetColorBrush("ActionLinkItemSelectedNotFocusedBrushKey");
			}
			return GetColorBrush("ItemSelectedTextNotFocusedBrushKey");
		}
		if (isMouseOverListItem)
		{
			if (isMouseOverToggleButton)
			{
				return GetColorBrush("ActionLinkItemHoverBrushKey");
			}
			return GetColorBrush("ItemHoverTextBrushKey");
		}
		if (isMouseOverToggleButton)
		{
			return GetColorBrush("ArrowHighlightBrushKey");
		}
		return GetColorBrush("ArrowBrushKey");
	}




	public static bool IsListItemHighlighted(bool isListItemSelected, bool isListFocused, bool isMouseOverListItem)
	{
		if (isListItemSelected)
		{
			return isListFocused || isMouseOverListItem;
		}
		return false;
	}

	public static SolidColorBrush GetColorBrush(string id)
	{
		Color color;

		switch (id)
		{
			case "ItemSelectedTextBrushKey":
				color = SystemParameters.HighContrast
					? SystemColors.HighlightTextColor
					: Color.FromArgb(byte.MaxValue, 0, 0, 0);
				break;
			case "ActionLinkItemSelectedNotFocusedBrushKey":
				color = SystemParameters.HighContrast
					? SystemColors.HighlightTextColor
					: Color.FromArgb(byte.MaxValue, 0, 112, 192);
				break;
			case "ItemSelectedTextNotFocusedBrushKey":
				color = SystemParameters.HighContrast
					? SystemColors.HighlightTextColor
					: Color.FromArgb(byte.MaxValue, 0, 0, 0);
				break;
			case "ActionLinkItemHoverBrushKey":
				color = SystemParameters.HighContrast
					? SystemColors.HighlightTextColor
					: Color.FromArgb(byte.MaxValue, 0, 112, 192);
				break;
			case "ItemHoverTextBrushKey":
				color = SystemParameters.HighContrast
					? SystemColors.HighlightTextColor
					: Color.FromArgb(byte.MaxValue, 0, 0, 0);
				break;
			case "ArrowHighlightBrushKey":
				color = SystemColors.HotTrackColor;
				break;
			case "ArrowBrushKey":
				color = SystemColors.WindowTextColor;
				break;
			case "TransparentBrushKey":
				return new SolidColorBrush(Color.FromRgb(0, 0, 0)) { Opacity = 0.0 };
			default:
				ArgumentException ex = new("id: " + id);
				Diag.Dug(ex);
				throw ex;
		}

		return new SolidColorBrush(color);
	}


}