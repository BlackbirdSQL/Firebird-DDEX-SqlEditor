// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.MultilineToolTip
#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace BlackbirdSql.Shared.Controls.Graphing.Widgets;

internal class MultilineToolTip : ToolTip
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private class LOGFONT
	{
		internal const int LF_FACESIZE = 32;

		internal int lfHeight;

		internal int lfWidth;

		internal int lfEscapement;

		internal int lfOrientation;

		internal int lfWeight;

		internal byte lfItalic;

		internal byte lfUnderline;

		internal byte lfStrikeOut;

		internal byte lfCharSet;

		internal byte lfOutPrecision;

		internal byte lfClipPrecision;

		internal byte lfQuality;

		internal byte lfPitchAndFamily;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		internal string lfFaceName = "";
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private class NONCLIENTMETRICS
	{
		internal uint cbSize = (uint)Marshal.SizeOf(typeof(NONCLIENTMETRICS));

		internal int iBorderWidth;

		internal int iScrollWidth;

		internal int iScrollHeight;

		internal int iCaptionWidth;

		internal int iCaptionHeight;

		internal LOGFONT lfCaptionFont = new LOGFONT();

		internal int iSmCaptionWidth;

		internal int iSmCaptionHeight;

		internal LOGFONT lfSmCaptionFont = new LOGFONT();

		internal int iMenuWidth;

		internal int iMenuHeight;

		internal LOGFONT lfMenuFont = new LOGFONT();

		internal LOGFONT lfStatusFont = new LOGFONT();

		internal LOGFONT lfMessageFont = new LOGFONT();
	}

	internal class ControlData(Control control)
	{
		private readonly Control control = control;



		private string text;

		internal Control Control => control;

		internal string Text
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
			}
		}
	}


	private Font font;

	private Font boldFont;

	private readonly Timer timer;

	private bool isVisible;

	private readonly List<ControlData> controlDataList = new List<ControlData>(1);

	private ControlData current;

	internal static readonly Size Margin = new Size(5, 5);

	internal Font Font
	{
		get
		{
			if (font == null)
			{
				NONCLIENTMETRICS nONCLIENTMETRICS = new()
				{
					cbSize = (uint)Marshal.SizeOf(typeof(NONCLIENTMETRICS))
				};
				IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NONCLIENTMETRICS)));
				try
				{
					Marshal.WriteInt32(intPtr, Marshal.SizeOf(typeof(NONCLIENTMETRICS)));
					Native.SystemParametersInfoW(41u, (uint)Marshal.SizeOf(typeof(NONCLIENTMETRICS)), intPtr, 0u);
					Marshal.PtrToStructure(intPtr, nONCLIENTMETRICS);
				}
				finally
				{
					Marshal.FreeHGlobal(intPtr);
				}
				FontStyle fontStyle = FontStyle.Regular;
				if (nONCLIENTMETRICS.lfStatusFont.lfItalic != 0)
				{
					fontStyle |= FontStyle.Italic;
				}
				font = new Font(nONCLIENTMETRICS.lfStatusFont.lfFaceName, Math.Abs(nONCLIENTMETRICS.lfStatusFont.lfHeight), fontStyle, nONCLIENTMETRICS.lfStatusFont.lfHeight < 0 ? GraphicsUnit.Pixel : GraphicsUnit.Point);
			}
			return font;
		}
	}

	internal Font BoldFont
	{
		get
		{
			boldFont ??= new Font(Font.FontFamily, Font.SizeInPoints, FontStyle.Bold);
			return boldFont;
		}
	}

	internal Control CurrentControl
	{
		get
		{
			if (Current == null)
			{
				return null;
			}
			return Current.Control;
		}
	}

	internal string CurrentText
	{
		get
		{
			if (Current == null)
			{
				return "";
			}
			return Current.Text;
		}
	}

	internal object CurrentTag
	{
		get
		{
			if (Current == null)
			{
				return null;
			}
			return Tag;
		}
	}

	protected Size MaximumSize
	{
		get
		{
			Rectangle workingArea = Screen.GetWorkingArea(Control.MousePosition);
			return new Size(workingArea.Width, workingArea.Height);
		}
	}

	internal ControlData Current
	{
		get
		{
			return current;
		}
		set
		{
			current = value;
		}
	}

	public MultilineToolTip()
	{
		OwnerDraw = true;
		ShowAlways = true;
		UseAnimation = false;
		UseFading = false;
		InitialDelay = 400;
		timer = new Timer();
		timer.Tick += OnTimerTick;
		Popup += OnToolTipPopup;
		Draw += OnToolTipDraw;
	}

	internal new void SetToolTip(Control control, string text)
	{
		ControlData controlData = GetControlData(control);
		controlData.Text = text;
		if (CurrentControl == control && text.Length == 0)
		{
			Hide();
		}
		else if (IsOverControl(control))
		{
			Current = controlData;
			StartInitialDelayTimer();
		}
	}

	internal new string GetToolTip(Control control)
	{
		return GetControlData(control).Text;
	}

	internal new void RemoveAll()
	{
		Hide();
		foreach (ControlData controlData in controlDataList)
		{
			UnsubscribeEvents(controlData);
		}
		controlDataList.RemoveAll((data) => true);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			ResetCachedGdiObjects();
		}
		base.Dispose(disposing);
	}

	protected virtual Size Measure(Graphics g, string text)
	{
		Size size = Size.Ceiling(g.MeasureString(text, Font));
		if (size.Width > MaximumSize.Width)
		{
			size = Size.Ceiling(g.MeasureString(text, Font, MaximumSize.Width));
		}
		return size + Margin + Margin;
	}

	protected virtual void DrawContent(DrawToolTipEventArgs e)
	{
		Rectangle rectangle = Rectangle.Inflate(e.Bounds, -Margin.Width, -Margin.Height);
		StringFormat stringFormat = new()
		{
			FormatFlags = StringFormatFlags.NoClip,
			Trimming = StringTrimming.EllipsisCharacter
		};
		e.Graphics.DrawString(CurrentText, Font, new SolidBrush(ForeColor), rectangle, stringFormat);
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		Control control = sender as Control;
		if (Current == null || Current.Control != control)
		{
			if (Current != null)
			{
				StopTimer();
			}
			ControlData controlData = GetControlData(control);
			if (controlData.Text.Length > 0)
			{
				Current = controlData;
				StartInitialDelayTimer();
			}
		}
	}

	private void OnMouseLeave(object sender, EventArgs e)
	{
		Control control = sender as Control;
		if (Current != null && Current.Control == control && !IsOverControl(Current.Control))
		{
			Hide();
		}
	}

	private void OnControlDestroyed(object sender, EventArgs e)
	{
		Control control = sender as Control;
		if (Current != null && Current.Control == control)
		{
			Hide();
		}
	}

	private void OnTimerTick(object sender, EventArgs e)
	{
		if (Current == null)
		{
			return;
		}
		if (!isVisible)
		{
			if (IsOverControl(Current.Control))
			{
				Show();
			}
		}
		else if (!IsOverControl(Current.Control))
		{
			Hide();
		}
	}

	private void OnToolTipPopup(object sender, PopupEventArgs e)
	{
		Graphics g = e.AssociatedControl.CreateGraphics();
		e.ToolTipSize = Measure(g, CurrentText);
	}

	private void OnToolTipDraw(object sender, DrawToolTipEventArgs e)
	{
		e.DrawBackground();
		Pen pen = new Pen(ForeColor);
		Pen pen2 = SystemInformation.HighContrast ? pen : new Pen(SystemColors.Control);
		e.Graphics.DrawLine(pen2, e.Bounds.Left, e.Bounds.Top, e.Bounds.Right - 2, e.Bounds.Top);
		e.Graphics.DrawLine(pen2, e.Bounds.Left, e.Bounds.Top, e.Bounds.Left, e.Bounds.Bottom - 2);
		e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right - 1, e.Bounds.Bottom - 1);
		e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom - 1);
		DrawContent(e);
		ResetCachedGdiObjects();
	}

	private void StartInitialDelayTimer()
	{
		timer.Interval = InitialDelay;
		timer.Start();
	}

	private void StartTrackingTimer()
	{
		timer.Interval = 100;
		timer.Start();
	}

	private new void StopTimer()
	{
		timer.Stop();
	}

	private void Show()
	{
		StopTimer();
		using (Graphics g = Current.Control.CreateGraphics())
		{
			Point pt = Current.Control.PointToScreen(new Point(0, 0));
			Point point = Control.MousePosition - new Size(pt) + new Size(0, 20);
			Rectangle workingArea = Screen.GetWorkingArea(Control.MousePosition);
			Rectangle r = new Rectangle(point, Measure(g, CurrentText));
			r = Current.Control.RectangleToScreen(r);
			if (r.Left < workingArea.Left)
			{
				point.Offset(workingArea.Left - r.Left, 0);
			}
			if (r.Top < workingArea.Top)
			{
				point.Offset(0, workingArea.Top - r.Top);
			}
			if (r.Right > workingArea.Right)
			{
				point.Offset(workingArea.Right - r.Right, 0);
			}
			if (r.Bottom > workingArea.Bottom)
			{
				point.Offset(0, workingArea.Bottom - r.Bottom);
			}
			Show(CurrentText, Current.Control, point);
		}
		isVisible = true;
		StartTrackingTimer();
	}

	private void Hide()
	{
		if (Current != null)
		{
			StopTimer();
			try
			{
				Hide(Current.Control);
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.Message);
			}
			Current = null;
		}
		isVisible = false;
	}

	private void ResetCachedGdiObjects()
	{
		if (font != null)
		{
			font.Dispose();
			font = null;
		}
		if (boldFont != null)
		{
			boldFont.Dispose();
			boldFont = null;
		}
	}

	private ControlData GetControlData(Control control)
	{
		foreach (ControlData controlData3 in controlDataList)
		{
			if (controlData3.Control == control)
			{
				return controlData3;
			}
		}
		ControlData controlData2 = new ControlData(control);
		SubscribeEvents(controlData2);
		controlDataList.Add(controlData2);
		return controlData2;
	}

	private void SubscribeEvents(ControlData controlData)
	{
		controlData.Control.MouseMove += OnMouseMove;
		controlData.Control.MouseLeave += OnMouseLeave;
		controlData.Control.Disposed += OnControlDestroyed;
		controlData.Control.HandleDestroyed += OnControlDestroyed;
	}

	private void UnsubscribeEvents(ControlData controlData)
	{
		controlData.Control.MouseMove -= OnMouseMove;
		controlData.Control.MouseLeave -= OnMouseLeave;
		controlData.Control.Disposed -= OnControlDestroyed;
		controlData.Control.HandleDestroyed -= OnControlDestroyed;
	}

	private bool IsOverControl(Control control)
	{
		return IsOverControl(control, control.PointToClient(Control.MousePosition));
	}

	private bool IsOverControl(Control control, Point point)
	{
		if (control.Visible && IsControlForeground(control))
		{
			return control.ClientRectangle.Contains(point);
		}
		return false;
	}

	private static IntPtr GetTopLevelParentWindow(IntPtr window)
	{
		IntPtr result = IntPtr.Zero;
		while (window != IntPtr.Zero)
		{
			result = window;
			window = Native.GetParent(window);
		}
		return result;
	}

	private bool IsControlForeground(Control control)
	{
		if (Current == null)
		{
			return false;
		}
		IntPtr topLevelParentWindow = GetTopLevelParentWindow(control.Handle);
		IntPtr topLevelParentWindow2 = GetTopLevelParentWindow(Native.GetForegroundWindow());
		return topLevelParentWindow == topLevelParentWindow2;
	}
}
