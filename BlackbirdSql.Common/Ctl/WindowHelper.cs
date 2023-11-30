#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Common.Ctl.Structs;

namespace BlackbirdSql.Common.Ctl;


public static class WindowHelper
{
	public static void RemoveIcon(Window window)
	{
		IntPtr handle = new WindowInteropHelper(window).Handle;
		if (!(handle == IntPtr.Zero))
		{
			int windowLong = Native.GetWindowLongInt(handle, -20);
			Native.SetWindowLong(handle, -20, windowLong | 1);
			Native.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 35);
		}
	}

	public static bool? ShowModal(Window window, IntPtr parent)
	{
		try
		{
			// Tracer.Trace(typeof(WindowHelper), "ShowModal()");

			WindowInteropHelper helper = new WindowInteropHelper(window)
			{
				Owner = parent
			};
			if (window.WindowStartupLocation == WindowStartupLocation.CenterOwner)
			{
				window.SourceInitialized += delegate
				{
					if (Native.GetWindowRect(parent, out UIRECT parentRect))
					{
						HwndSource hwndSource = HwndSource.FromHwnd(helper.Handle);
						if (hwndSource != null)
						{
							Point point = hwndSource.CompositionTarget.TransformToDevice.Transform(new Point(window.ActualWidth, window.ActualHeight));
							UIRECT rECT = CenterRectOnSingleMonitor(parentRect, (int)point.X, (int)point.Y);
							Point point2 = hwndSource.CompositionTarget.TransformFromDevice.Transform(new Point(rECT.Left, rECT.Top));
							window.WindowStartupLocation = WindowStartupLocation.Manual;
							window.Left = point2.X;
							window.Top = point2.Y;
						}
					}
				};
			}

			return window.ShowDialog();
		}
		catch (Exception exception)
		{
			UiTracer.TraceSource.AssertTraceException2(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, exception, "Failed to load the UI", 71, "WindowHelper.cs", "ShowModal");
			return false;
		}
	}

	private static UIRECT CenterRectOnSingleMonitor(UIRECT parentRect, int childWidth, int childHeight)
	{
		Native.FindMaximumSingleMonitorRectangle(parentRect, out var screenSubRect, out var monitorRect);
		return CenterInRect(screenSubRect, childWidth, childHeight, monitorRect);
	}

	private static UIRECT CenterInRect(UIRECT parentRect, int childWidth, int childHeight, UIRECT monitorClippingRect)
	{
		UIRECT result = default;
		result.Left = parentRect.Left + (parentRect.Width - childWidth) / 2;
		result.Top = parentRect.Top + (parentRect.Height - childHeight) / 2;
		result.Width = childWidth;
		result.Height = childHeight;
		result.Left = Math.Min(result.Right, monitorClippingRect.Right) - result.Width;
		result.Top = Math.Min(result.Bottom, monitorClippingRect.Bottom) - result.Height;
		result.Left = Math.Max(result.Left, monitorClippingRect.Left);
		result.Top = Math.Max(result.Top, monitorClippingRect.Top);
		return result;
	}
}
