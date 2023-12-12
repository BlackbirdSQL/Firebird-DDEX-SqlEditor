#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BlackbirdSql.Core;
using Microsoft.Win32;


namespace BlackbirdSql.Common.Controls.Grid;

public static class DrawManager
{
	// Microsoft.SqlServer.Management.UI.Grid.DrawManager

	[ThreadStatic]
	private static Pen borderPen;

	public static Pen BorderPen
	{
		get
		{
			if (Application.RenderWithVisualStyles)
			{
				return borderPen ??= new Pen(VisualStyleInformation.TextControlBorder);
			}

			return SystemPens.ControlDark;
		}
	}

	public static Color BorderColor
	{
		get
		{
			if (Application.RenderWithVisualStyles)
			{
				return VisualStyleInformation.TextControlBorder;
			}

			return SystemColors.ControlDark;
		}
	}

	static DrawManager()
	{
		SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
	}

	private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		borderPen = null;
	}

	public static VisualStyleElement GetButton(ButtonState state)
	{
		VisualStyleElement result = null;
		if (Application.RenderWithVisualStyles)
		{
			result = VisualStyleElement.Button.PushButton.Normal;
			switch (state)
			{
				case ButtonState.Inactive:
					result = VisualStyleElement.Button.PushButton.Disabled;
					break;
				case ButtonState.Pushed:
					result = VisualStyleElement.Button.PushButton.Pressed;
					break;
			}
		}

		return result;
	}

	public static VisualStyleElement GetHeader(ButtonState state)
	{
		VisualStyleElement result = null;
		if (Application.RenderWithVisualStyles)
		{
			result = VisualStyleElement.Header.Item.Normal;
			switch (state)
			{
				case ButtonState.Inactive:
					result = VisualStyleElement.Header.Item.Normal;
					break;
				case ButtonState.Pushed:
					result = VisualStyleElement.Header.Item.Pressed;
					break;
			}
		}

		return result;
	}

	public static VisualStyleElement GetLineIndexButton(ButtonState state)
	{
		VisualStyleElement visualStyleElement = null;
		if (Application.RenderWithVisualStyles)
		{
			visualStyleElement = VisualStyleElement.Header.ItemRight.Normal;
			switch (state)
			{
				case ButtonState.Inactive:
					visualStyleElement = VisualStyleElement.Header.ItemRight.Normal;
					break;
				case ButtonState.Pushed:
					visualStyleElement = VisualStyleElement.Header.ItemRight.Pressed;
					break;
			}

			if (!VisualStyleRenderer.IsElementDefined(visualStyleElement))
			{
				visualStyleElement = VisualStyleElement.Header.ItemLeft.Normal;
				switch (state)
				{
					case ButtonState.Inactive:
						visualStyleElement = VisualStyleElement.Header.ItemLeft.Normal;
						break;
					case ButtonState.Pushed:
						visualStyleElement = VisualStyleElement.Header.ItemLeft.Pressed;
						break;
				}
			}

			if (!VisualStyleRenderer.IsElementDefined(visualStyleElement))
			{
				visualStyleElement = VisualStyleElement.Header.Item.Normal;
				switch (state)
				{
					case ButtonState.Inactive:
						visualStyleElement = VisualStyleElement.Header.Item.Normal;
						break;
					case ButtonState.Pushed:
						visualStyleElement = VisualStyleElement.Header.Item.Pressed;
						break;
				}
			}
		}

		return visualStyleElement;
	}

	public static VisualStyleElement GetCheckBox(ButtonState state)
	{
		VisualStyleElement result = null;
		if (Application.RenderWithVisualStyles)
		{
			CheckState checkState = (state & ButtonState.Flat) == ButtonState.Flat ? CheckState.Indeterminate : (state & ButtonState.Checked) == ButtonState.Checked ? CheckState.Checked : CheckState.Unchecked;
			bool flag = (state & ButtonState.Inactive) == ButtonState.Inactive;
			bool flag2 = (state & ButtonState.Pushed) == ButtonState.Pushed;
			switch (checkState)
			{
				case CheckState.Checked:
					result = !flag ? !flag2 ? VisualStyleElement.Button.CheckBox.CheckedNormal : VisualStyleElement.Button.CheckBox.CheckedPressed : VisualStyleElement.Button.CheckBox.CheckedDisabled;
					break;
				case CheckState.Unchecked:
					result = !flag ? !flag2 ? VisualStyleElement.Button.CheckBox.UncheckedNormal : VisualStyleElement.Button.CheckBox.UncheckedPressed : VisualStyleElement.Button.CheckBox.UncheckedDisabled;
					break;
				case CheckState.Indeterminate:
					result = !flag ? !flag2 ? VisualStyleElement.Button.CheckBox.MixedNormal : VisualStyleElement.Button.CheckBox.MixedPressed : VisualStyleElement.Button.CheckBox.MixedDisabled;
					break;
			}
		}

		return result;
	}

	public static bool DrawNCBorder(ref Message m)
	{
		bool result = false;
		int num = Native.GetWindowLongPtrPtr(m.HWnd, -16).ToInt32();
		if (m.Msg == Native.WM_NCPAINT && ((uint)num & 0x800000u) != 0 && (Native.GetWindowLongPtrPtr(m.HWnd, -20).ToInt32() & 0x200) == 0)
		{
			IntPtr intPtr = m.WParam != (IntPtr)1 ? m.WParam : IntPtr.Zero;
			if (m.HWnd != IntPtr.Zero)
			{
				int num2 = 328705;
				if (intPtr != IntPtr.Zero)
					num2 |= 0x80;

				IntPtr dCEx = UnsafeNative.GetDCEx(m.HWnd, intPtr, num2);
				if (dCEx != IntPtr.Zero)
				{
					Native.GetWindowRect(m.HWnd, out Native.RECT rect);

					rect.right -= rect.left;
					rect.left = 0;
					rect.bottom -= rect.top;
					rect.top = 0;
					if (((uint)num & 0x100000u) != 0 || ((uint)num & 0x200000u) != 0)
					{
						m.Result = UnsafeNative.DefWindowProc(m.HWnd, m.Msg, m.WParam, m.LParam);
					}

					IntPtr handle = Native.CreatePen(0, 1, ColorTranslator.ToWin32(BorderColor));
					IntPtr handle2 = Native.SelectObject(new HandleRef(null, dCEx), new HandleRef(null, handle));
					Native.POINT pt = new();
					Native.MoveToEx(new HandleRef(null, dCEx), rect.left, rect.top, pt);
					Native.LineTo(new HandleRef(null, dCEx), rect.left, rect.bottom - 1);
					Native.LineTo(new HandleRef(null, dCEx), rect.right - 1, rect.bottom - 1);
					Native.LineTo(new HandleRef(null, dCEx), rect.right - 1, rect.top);
					Native.LineTo(new HandleRef(null, dCEx), rect.left, rect.top);
					Native.SelectObject(new HandleRef(null, dCEx), new HandleRef(null, handle2));
					Native.DeleteObject(new HandleRef(null, handle));
					UnsafeNative.ReleaseDC(m.HWnd, dCEx);
					result = true;
				}
			}
		}

		return result;
	}
}
