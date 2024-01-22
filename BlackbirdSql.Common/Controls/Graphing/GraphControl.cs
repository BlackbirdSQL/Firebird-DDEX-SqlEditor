// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.GraphControl
#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

using BlackbirdSql.Common.Controls.Graphing.ComponentModel;
using BlackbirdSql.Common.Controls.Graphing.Enums;
using BlackbirdSql.Common.Controls.Graphing.Interfaces;
using BlackbirdSql.Common.Controls.Widgets;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Ctl.Exceptions;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core;

using Microsoft.AnalysisServices.Graphing;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Common.Controls.Graphing;

public class GraphControl : GraphCtrl
{
	private struct PointD
	{
		public double X;

		public double Y;

		public PointD(double x, double y)
		{
			X = x;
			Y = y;
		}
	}

	internal class GraphNodeLayoutHelper
	{
		private readonly List<Point> layoutPoints = new List<Point>();

		public void CheckInvariant()
		{
			Point point = new Point(0, 0);
			for (int i = 0; i < layoutPoints.Count; i++)
			{
				if (point.X <= layoutPoints[i].X)
				{
					_ = point.Y;
					_ = layoutPoints[i].Y;
				}
				point = layoutPoints[i];
			}
		}

		public void UpdateNodeLayout(int xPosition, int yPosition)
		{
			CheckInvariant();
			if (!layoutPoints.Any())
			{
				layoutPoints.Add(new Point(xPosition, yPosition));
				return;
			}
			if (layoutPoints.Count == 1)
			{
				if (xPosition < layoutPoints[0].X)
				{
					layoutPoints.Insert(0, new Point(xPosition, yPosition));
				}
				else if (xPosition == layoutPoints[0].X)
				{
					layoutPoints[0] = new Point(layoutPoints[0].X, Math.Max(layoutPoints[0].Y, yPosition));
				}
				else
				{
					layoutPoints.Add(new Point(xPosition, yPosition));
				}
				return;
			}
			if (xPosition < layoutPoints[0].X && yPosition < layoutPoints[0].Y)
			{
				layoutPoints.Insert(0, new Point(xPosition, yPosition));
				return;
			}
			if (layoutPoints.Last().X < xPosition && yPosition > layoutPoints.Last().Y)
			{
				layoutPoints.Add(new Point(xPosition, yPosition));
				return;
			}
			if (layoutPoints.Last().X == xPosition)
			{
				layoutPoints[^1] = new Point(xPosition, Math.Max(layoutPoints.Last().Y, yPosition));
				return;
			}
			int num = 0;
			for (int i = 0; i < layoutPoints.Count; i++)
			{
				if (xPosition <= layoutPoints[i].X)
				{
					num = i;
					break;
				}
			}
			if (xPosition == layoutPoints[num].X)
			{
				layoutPoints[num] = new Point(xPosition, Math.Max(layoutPoints[num].Y, yPosition));
			}
			else
			{
				layoutPoints.Insert(num, new Point(xPosition, yPosition));
			}
			for (int j = num; j < layoutPoints.Count; j++)
			{
				if (layoutPoints[j].Y > yPosition)
				{
					if (j - num - 1 > 1)
					{
						layoutPoints.RemoveRange(num + 1, j - num - 1);
					}
					else
					{
						layoutPoints.RemoveRange(num + 1, j - num - 1);
					}
					return;
				}
			}
			layoutPoints.RemoveRange(num + 1, layoutPoints.Count - num - 1);
		}

		public int GetYPositionForXPosition(int rowX)
		{
			CheckInvariant();
			int num = 0;
			foreach (Point layoutPoint in layoutPoints)
			{
				if (rowX < layoutPoint.X)
				{
					break;
				}
				num = Math.Max(layoutPoint.Y, num);
			}
			return num;
		}
	}

	private sealed class Renderer : RendererBase
	{
		public static readonly Renderer Default = new Renderer();

		protected override bool OnRender(object item, Graphics graphics)
		{
			try
			{
				if (item is IRenderable renderable)
				{
					renderable.Render(graphics);
					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.Message);
				return false;
			}
		}
	}

	private sealed class ToolTip : MultilineToolTip
	{
		public const string NoTitle = "|";

		private PropertyDescriptor _Description;

		private PropertyDescriptorCollection _Properties;

		private object previousTag;

		private static readonly int spaceBetweenColumns = 4 * Margin.Width;

		private static readonly int maxPropertyLength = 500;

		private float propertyNameWidth;

		private float propertyValueWidth;

		private float descriptionHeight;

		private string additionalText;

		private PropertyDescriptor Description
		{
			get
			{
				ResetCachedValuesIfTagChanged();
				_Description ??= GetProperty("Description");
				return _Description;
			}
		}

		private PropertyDescriptorCollection Properties
		{
			get
			{
				ResetCachedValuesIfTagChanged();
				if (_Properties == null)
				{
					_Properties = new PropertyDescriptorCollection(new PropertyDescriptor[0]);
					foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(Tag))
					{
						if (property.Attributes[typeof(ShowInToolTipAttribute)] is ShowInToolTipAttribute showInToolTipAttribute && showInToolTipAttribute.Value && GetPropertyText(property).Length > 0)
						{
							_Properties.Add(property);
						}
					}
					_Properties = _Properties.Sort(PropertyValue.OrderComparer.Default);
				}
				return _Properties;
			}
		}

		private int LineHeight => BoldFont.Height + 1;

		protected override Size Measure(Graphics g, string caption)
		{
			float width = g.MeasureString(caption, BoldFont).Width;
			float num = caption != "|" ? LineHeight : 0;
			float val = additionalText != null ? g.MeasureString(additionalText, BoldFont).Width : 0f;
			float num2 = additionalText != null && additionalText != string.Empty ? LineHeight : 0;
			propertyNameWidth = 0f;
			propertyValueWidth = 0f;
			int num3 = -1;
			for (int i = 0; i < Properties.Count; i++)
			{
				PropertyDescriptor propertyDescriptor = Properties[i];
				if (IsLongStringProperty(propertyDescriptor))
				{
					num3 = i;
					break;
				}
				string text = TruncateString(GetPropertyText(propertyDescriptor));
				propertyNameWidth = Math.Max(propertyNameWidth, g.MeasureString(propertyDescriptor.DisplayName, BoldFont).Width);
				propertyValueWidth = Math.Max(propertyValueWidth, g.MeasureString(text, Font).Width);
			}
			int num4 = (int)Math.Max(width, propertyNameWidth + propertyValueWidth + spaceBetweenColumns);
			num4 = (int)Math.Max(num4, val);
			if (num4 > MaximumSize.Width)
			{
				num4 = MaximumSize.Width;
				float num5 = (num4 - spaceBetweenColumns) / (propertyNameWidth + propertyValueWidth);
				propertyNameWidth *= num5;
				propertyValueWidth *= num5;
			}
			string text2 = Description != null ? GetPropertyText(Description) : string.Empty;
			descriptionHeight = text2.Length > 0 ? (float)Math.Ceiling(g.MeasureString(text2, Font, num4).Height) : 0f;
			float num6 = num + descriptionHeight + num2;
			if (Properties.Count > 0)
			{
				if (num6 > 0f)
				{
					num6 += LineHeight;
				}
				num6 += Properties.Count * LineHeight;
			}
			if (num3 != -1)
			{
				num6 += LineHeight;
				_ = MaximumSize.Height;
				float num7;
				while (true)
				{
					num7 = 0f;
					for (int j = num3; j < Properties.Count; j++)
					{
						string text3 = TruncateString(GetPropertyText(Properties[j]));
						if (text3.Length > 0)
						{
							num7 += (float)Math.Ceiling(g.MeasureString(text3, Font, num4).Height);
						}
						if (num6 + num7 > MaximumSize.Height)
						{
							break;
						}
					}
					if (num6 + num7 <= MaximumSize.Height || num4 >= MaximumSize.Width)
					{
						break;
					}
					num4 = Math.Min(num4 * 6 / 5, MaximumSize.Width);
				}
				num6 += num7;
			}
			if (num6 > MaximumSize.Height)
			{
				num6 = MaximumSize.Height;
			}
			return new Size(num4, (int)num6) + Margin + Margin;
		}

		protected override void DrawContent(DrawToolTipEventArgs e)
		{
			Brush brush = new SolidBrush(ForeColor);
			Pen pen = new Pen(ControlPaint.Dark(BackColor));
			StringFormat stringFormat = new()
			{
				LineAlignment = StringAlignment.Center,
				Trimming = StringTrimming.EllipsisCharacter,
				FormatFlags = StringFormatFlags.NoClip
			};
			float num = e.Bounds.Left + Margin.Width;
			float num2 = e.Bounds.Right - Margin.Width;
			float num3 = e.Bounds.Top + Margin.Height;
			float num4 = e.Bounds.Bottom - Margin.Height;
			if (CurrentText != "|")
			{
				RectangleF layoutRectangle = RectangleF.FromLTRB(num, num3, num2, num3 + LineHeight);
				stringFormat.Alignment = StringAlignment.Center;
				e.Graphics.DrawString(CurrentText, BoldFont, brush, layoutRectangle, stringFormat);
				num3 = layoutRectangle.Bottom;
			}
			if (descriptionHeight > 0f)
			{
				RectangleF layoutRectangle2 = RectangleF.FromLTRB(num, num3, num2, num3 + descriptionHeight);
				stringFormat.Alignment = StringAlignment.Near;
				e.Graphics.DrawString(GetPropertyText(Description), Font, brush, layoutRectangle2, stringFormat);
				num3 = layoutRectangle2.Bottom;
			}
			if (additionalText != null && additionalText != string.Empty)
			{
				RectangleF layoutRectangle3 = RectangleF.FromLTRB(num, num3, num2, num3 + LineHeight);
				stringFormat.Alignment = StringAlignment.Near;
				e.Graphics.DrawString(additionalText, BoldFont, brush, layoutRectangle3, stringFormat);
				num3 = layoutRectangle3.Bottom;
			}
			if (Properties.Count > 0)
			{
				if (CurrentText != "|")
				{
					num3 += LineHeight;
				}
				float val = (e.Bounds.Width - spaceBetweenColumns) / (propertyNameWidth + propertyValueWidth);
				val = Math.Max(val, 0f);
				propertyNameWidth *= val;
				propertyValueWidth *= val;
				bool flag = false;
				foreach (PropertyDescriptor property in Properties)
				{
					string text = TruncateString(GetPropertyText(property));
					if (!flag && IsLongStringProperty(property))
					{
						flag = true;
						num3 += LineHeight;
					}
					RectangleF layoutRectangle4 = RectangleF.FromLTRB(num, num3, num + propertyNameWidth, num3 + LineHeight);
					if (layoutRectangle4.Bottom > num4)
					{
						break;
					}
					stringFormat.Alignment = StringAlignment.Near;
					e.Graphics.DrawString(property.DisplayName, BoldFont, brush, layoutRectangle4, stringFormat);
					RectangleF layoutRectangle5;
					if (flag)
					{
						float num5 = (float)Math.Ceiling(e.Graphics.MeasureString(text, Font, (int)(num2 - num)).Height);
						layoutRectangle5 = RectangleF.FromLTRB(num, layoutRectangle4.Bottom - 1f, num2, layoutRectangle4.Bottom + num5);
					}
					else
					{
						layoutRectangle5 = RectangleF.FromLTRB(num2 - propertyValueWidth, layoutRectangle4.Top, num2, layoutRectangle4.Bottom);
						stringFormat.Alignment = StringAlignment.Far;
					}
					e.Graphics.DrawString(text, Font, brush, layoutRectangle5, stringFormat);
					if (!flag)
					{
						float num6 = layoutRectangle5.Bottom - 1f;
						if (num6 < e.Bounds.Bottom - Margin.Height - 2)
						{
							e.Graphics.DrawLine(pen, num, num6, num2 - 1f, num6);
						}
					}
					num3 = layoutRectangle5.Bottom;
				}
			}
			pen.Dispose();
			brush.Dispose();
		}

		private string GetPropertyText(PropertyDescriptor property)
		{
			try
			{
				object value = property.GetValue(Tag);
				if (value == null)
				{
					return string.Empty;
				}
				return property.Converter.ConvertToString(value).Trim();
			}
			catch (TargetInvocationException ex)
			{
				Trace.TraceError(ex.Message);
				return string.Empty;
			}
		}

		private string TruncateString(string srcString)
		{
			if (srcString.Length <= maxPropertyLength)
			{
				return srcString;
			}
			return srcString[..(maxPropertyLength - 3)] + "...";
		}

		private void ResetCachedValuesIfTagChanged()
		{
			if (Tag != previousTag)
			{
				previousTag = Tag;
				_Description = null;
				_Properties = null;
			}
		}

		private PropertyDescriptor GetProperty(string propertyName)
		{
			return TypeDescriptor.GetProperties(Tag)[propertyName];
		}

		private static bool IsLongStringProperty(PropertyDescriptor property)
		{
			if (property is PropertyValue propertyValue)
			{
				return propertyValue.IsLongString;
			}
			return false;
		}

		internal void SetAdditionalText(string textDisplay)
		{
			additionalText = textDisplay;
		}
	}

	private sealed class PrintDocument : System.Drawing.Printing.PrintDocument
	{
		private Point currentPosition;

		private Rectangle pageBounds;

		private readonly GraphControl showPlan;

		private Rectangle worldRectangle;

		private Point worldViewportOrigin;

		private bool showPlanIsActive;

		public PrintDocument(GraphControl showPlan)
		{
			this.showPlan = showPlan;
		}

		protected override void OnBeginPrint(PrintEventArgs e)
		{
			pageBounds = new Rectangle(DefaultPageSettings.Margins.Left, DefaultPageSettings.Margins.Top, DefaultPageSettings.Bounds.Width - DefaultPageSettings.Margins.Left - DefaultPageSettings.Margins.Right, DefaultPageSettings.Bounds.Height - DefaultPageSettings.Margins.Top - DefaultPageSettings.Margins.Bottom);
			worldRectangle = showPlan.graphBoundingRectangle;
			worldViewportOrigin = showPlan.WorldViewportOrigin;
			currentPosition = worldRectangle.Location;
			showPlanIsActive = showPlan.IsActive;
			if (showPlanIsActive)
			{
				showPlan.Deactivate();
			}
			base.OnBeginPrint(e);
		}

		protected override void OnPrintPage(PrintPageEventArgs e)
		{
			float viewScale = showPlan.ViewScale;
			PaintEventArgs e2 = new PaintEventArgs(e.Graphics, new Rectangle((int)((currentPosition.X - worldViewportOrigin.X) * viewScale), (int)((currentPosition.Y - worldViewportOrigin.Y) * viewScale), (int)(pageBounds.Width * viewScale), (int)(pageBounds.Height * viewScale)));
			e.Graphics.SetClip(new Rectangle(e.MarginBounds.Left, e.MarginBounds.Top, pageBounds.Width, pageBounds.Height));
			e.Graphics.TranslateTransform(e.MarginBounds.Left - currentPosition.X + worldViewportOrigin.X, e.MarginBounds.Top - currentPosition.Y + worldViewportOrigin.Y, MatrixOrder.Append);
			e.Graphics.ScaleTransform(1f / viewScale, 1f / viewScale);
			showPlan.InvokePaint(e2);
			currentPosition.X += pageBounds.Width;
			if (currentPosition.X > worldRectangle.Right)
			{
				currentPosition.X = worldViewportOrigin.X;
				currentPosition.Y += pageBounds.Height;
				if (currentPosition.Y > worldRectangle.Bottom)
				{
					e.HasMorePages = false;
				}
				else
				{
					e.HasMorePages = true;
				}
			}
			else
			{
				e.HasMorePages = true;
			}
			base.OnPrintPage(e);
		}

		protected override void OnEndPrint(PrintEventArgs e)
		{
			base.OnEndPrint(e);
			if (showPlanIsActive)
			{
				showPlan.Activate();
			}
		}
	}

	private bool draggingActive;

	private Point initialMouseDragScreenCoord;

	private Point initialMouseDragViewCoord;

	private const int C_NodeToolHideTipThreshold = 6;

	private const int C_PaddingX = 48;

	private const int C_PaddingY = 16;

	private ToolTip toolTip = new ToolTip();

	private bool isActive;

	private IDisplay lastActiveObject;

	private Rectangle graphBoundingRectangle;

	internal virtual bool RequestFocusOnActivate => true;

	public bool IsActive => isActive;

	public BlackbirdSql.Common.Controls.Graphing.NodeDisplay RootNode => NodesZOrder[0] as BlackbirdSql.Common.Controls.Graphing.NodeDisplay;

	public double Cost => RootNode.NodeOriginal.SubtreeCost;

	public string Statement
	{
		get
		{
			string text = RootNode.NodeOriginal["StatementText"] as string ?? RootNode.NodeOriginal["ProcName"] as string;
			if (text == null)
			{
				return "";
			}
			return text;
		}
	}

	internal Rectangle GraphBoundingRectangle => graphBoundingRectangle;

	public event EventHandler IsActiveChangedEvent;

	public event GraphEventHandler SelectionChangedEvent;

	public GraphControl()
	{
		((IGraphDisplay)this).NodeRenderer.RendererOverride = Renderer.Default;
		((IGraphDisplay)this).EdgeRenderer.RendererOverride = Renderer.Default;
		IsNodeMoveAllowed = false;
		IsMouseWheelZoomAllowed = true;
		IsBoundingRectanglePadded = false;
		BackColor = SystemColors.Window;
		ControlTextRenderingHint = TextRenderingHint.SystemDefault;
		ControlSmoothingMode = SmoothingMode.Default;
		AllowDrop = false;
		ScaleToFitViewScaleMax = 2f;
		OnSelChange += OnSelectionChanged;
	}

	private void BeginMouseDrag()
	{
		draggingActive = true;
		initialMouseDragScreenCoord = MousePosition;
		initialMouseDragViewCoord = WorldViewportOrigin;
	}

	private void EndMouseDrag()
	{
		draggingActive = false;
	}

	public void SetGraph(IGraph graph)
	{
		try
		{
			Initialize(graph);
			LayoutGraphElements();
		}
		catch (Exception ex)
		{
			Trace.TraceError(ex.Message);
			ShowExceptionMessage(ex);
		}
	}

	private new bool OnMouseWheel(MouseEventArgs e)
	{
		if (IsMouseWheelZoomAllowed && (ModifierKeys & Keys.Control) == Keys.Control)
		{
			Point point = PointToClient(MousePosition);
			if (point.X < 0 || point.Y < 0 || point.X > Size.Width || point.Y > Size.Height)
			{
				return true;
			}
			Point worldViewportOrigin = WorldViewportOrigin;
			PointD pointD = new PointD(point.X / ViewScale, point.Y / ViewScale);
			if (e.Delta > 0)
			{
				ViewScale *= 1.25f;
			}
			else
			{
				ViewScale *= 0.8f;
			}
			PointD pointD2 = new PointD(point.X / ViewScale, point.Y / ViewScale);
			Point viewportOrigin = new Point((int)(pointD.X - pointD2.X), (int)(pointD.Y - pointD2.Y));
			viewportOrigin.Offset(worldViewportOrigin);
			SetViewportOrigin(viewportOrigin);
			return true;
		}
		base.OnMouseWheel(e);
		return false;
	}

	protected override void WndProc(ref Message m)
	{
		Diag.ThrowIfNotOnUIThread();

		if (m.Msg == 522)
		{
			long num = (long)m.LParam & 0xFFFFFFFFu;
			int num2 = (short)(num & 0xFFFF);
			int num3 = (short)((num >>> C_PaddingY) & 0xFFFF);
			long num4 = (long)m.WParam & 0xFFFFFFFFu;
			int num5 = (int)(num4 & 0xFFFF);
			int delta = (short)(num4 >>> C_PaddingY);
			if (OnMouseWheel(new MouseEventArgs(num5 switch
			{
				1 => MouseButtons.Left,
				2 => MouseButtons.Right,
				16 => MouseButtons.Middle,
				_ => MouseButtons.None,
			}, 0, num2, num3, delta)))
			{
				return;
			}
		}
		else if (m.Msg == 1 && Package.GetGlobalService(typeof(SVsShell)) is IVsShell vsShell)
		{
			vsShell.GetProperty((int)__VSSPROPID5.VSSPROPID_NativeScrollbarThemeModePropName, out var pvar);
			if (!string.IsNullOrEmpty(pvar as string))
			{
				Native.SetProp(new HandleRef(this, Handle), (string)pvar, new HandleRef(this, (IntPtr)2));
			}
		}
		base.WndProc(ref m);
	}

	public void ReadNodes(byte[] dataSource)
	{
		ReadNodesInternal(dataSource, EnExecutionPlanType.Unknown);
	}

	public void ReadNodes(string dataSource)
	{
		ReadNodesInternal(dataSource, EnExecutionPlanType.Unknown);
	}

	public void ReadNodes(string dataSource, EnExecutionPlanType executionPlanType)
	{
		ReadNodesInternal(dataSource, executionPlanType);
	}

	public void Activate()
	{
		if (!isActive)
		{
			isActive = true;
			if (RequestFocusOnActivate)
			{
				Focus();
			}
			lastActiveObject ??= RootNode;
			SelectIDisplayObject(lastActiveObject);
			IsActiveChangedEvent?.Invoke(this, EventArgs.Empty);
		}
	}

	public void Deactivate()
	{
		if (isActive)
		{
			isActive = false;
			SelectIDisplayObject(null);
			IsActiveChangedEvent?.Invoke(this, EventArgs.Empty);
		}
	}

	public void ZoomIn()
	{
		try
		{
			ViewScale = Math.Min(ViewScale * 1.25f, 2f);
		}
		catch (Exception ex)
		{
			Trace.TraceError(ex.Message);
			ShowExceptionMessage(ex);
		}
	}

	public void ZoomOut()
	{
		try
		{
			ViewScale = Math.Max(ViewScale * 0.8f, 0.01f);
		}
		catch (Exception ex)
		{
			Trace.TraceError(ex.Message);
			ShowExceptionMessage(ex);
		}
	}

	public float GetZoom()
	{
		return ViewScale;
	}

	public void SetCustomZoom(float viewScale)
	{
		try
		{
			ViewScale = viewScale;
		}
		catch (Exception ex)
		{
			Trace.TraceError(ex.Message);
			ShowExceptionMessage(ex);
		}
	}

	public void ShowCustomZoomDialog()
	{
		try
		{
			using CustomZoomDlg customZoom = new CustomZoomDlg((decimal)(ViewScale * 100f));
			customZoom.ZoomChangedEvent += OnZoomChanged;
			customZoom.Location = customZoom.PointToClient(MousePosition);
			customZoom.ShowDialog();
		}
		catch (Exception ex)
		{
			Trace.TraceError(ex.Message);
			ShowExceptionMessage(ex);
		}
	}

	public System.Drawing.Printing.PrintDocument GetPrintDocument()
	{
		return new PrintDocument(this);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		try
		{
			base.OnKeyDown(e);
			switch (e.KeyCode)
			{
				case Keys.Subtract:
					if (e.Shift)
					{
						ZoomOut();
					}
					break;
				case Keys.Add:
					if (e.Shift)
					{
						ZoomIn();
					}
					break;
			}
		}
		catch (ArgumentException ex)
		{
			Trace.TraceError(ex.Message);
		}
	}

	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override bool ProcessCmdKey(ref Message message, Keys keyData)
	{
		try
		{
			switch (keyData)
			{
				case Keys.Right:
					MoveSelectionRight();
					return true;
				case Keys.Left:
					MoveSelectionLeft();
					return true;
				case Keys.Down:
					MoveSelectionDown();
					return true;
				case Keys.Up:
					MoveSelectionUp();
					return true;
				case Keys.F1:
					OnHelpRequested();
					return true;
			}
		}
		catch (Exception ex)
		{
			Trace.TraceError(ex.Message);
			ShowExceptionMessage(ex);
		}
		return base.ProcessCmdKey(ref message, keyData);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (!IsActive)
		{
			Activate();
		}
		Focus();
		BeginMouseDrag();
		IDisplay display = HitTest(new Point(e.X, e.Y));
		if (display != null)
		{
			SelectIDisplayObject(display);
			Update();
		}
		if (e.Button == MouseButtons.Right && e.Clicks == 1)
		{
			base.OnMouseDown(e);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		EndMouseDrag();
		Cursor = Cursors.Arrow;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		try
		{
			if (draggingActive && (MouseButtons & MouseButtons.Left) != 0)
			{
				Cursor = Cursors.Hand;
				Point mousePosition = MousePosition;
				Point viewportOrigin = initialMouseDragScreenCoord;
				viewportOrigin.Offset(-mousePosition.X, -mousePosition.Y);
				viewportOrigin.X = (int)(viewportOrigin.X / ViewScale);
				viewportOrigin.Y = (int)(viewportOrigin.Y / ViewScale);
				viewportOrigin.Offset(initialMouseDragViewCoord);
				SetViewportOrigin(viewportOrigin);
				if (Math.Abs(-mousePosition.X + initialMouseDragScreenCoord.X) + Math.Abs(-mousePosition.Y + initialMouseDragScreenCoord.Y) > C_NodeToolHideTipThreshold)
				{
					toolTip.RemoveAll();
				}
			}
			else
			{
				if (_tooltip != null)
				{
					_tooltip.RemoveAll();
					_tooltip.Dispose();
					_tooltip = null;
				}
				base.OnMouseMove(e);
				IDisplay displayObject = HitTest(new Point(e.X, e.Y));
				HandleToolTip(displayObject);
			}
		}
		catch (SystemException ex)
		{
			Trace.TraceError(ex.Message);
			if (ex is StackOverflowException || ex is OutOfMemoryException)
			{
				throw;
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (toolTip != null)
			{
				toolTip.Dispose();
				toolTip = null;
			}
			OnSelChange -= OnSelectionChanged;
		}
		base.Dispose(disposing);
	}

	private void ReadNodesInternal(object dataSource, EnExecutionPlanType executionPlanType)
	{
		IGraph[] array = NodeBuilderFactory.Create(dataSource, executionPlanType).Execute(dataSource);
		IGraph[] array2 = array;
		if (array2.Length >= 1)
		{
			SetGraph(array2[0]);
		}
	}

	private void OnSelectionChanged(object sender, GraphEventArgs e)
	{
		if (e.DisplayObject != null)
		{
			lastActiveObject = e.DisplayObject;
		}
		SelectionChangedEvent?.Invoke(this, e);
	}

	private void OnZoomChanged(object sender, EventArgs e)
	{
		try
		{
			CustomZoomDlg customZoom = sender as CustomZoomDlg;
			ViewScale = (float)customZoom.Zoom / 100f;
		}
		catch (Exception ex)
		{
			Trace.TraceError(ex.Message);
			ShowExceptionMessage(ex);
		}
	}

	private void LayoutGraphElements()
	{
		int num = 0;
		foreach (BlackbirdSql.Common.Controls.Graphing.NodeDisplay item in NodesZOrder.Cast<NodeDisplay>())
		{
			num = Math.Max(num, item.BoundingRect.Height);
		}
		num += C_PaddingY;
		int num2 = (C_PaddingX + RootNode.BoundingRect.Width) / 2;
		int num3 = (C_PaddingY + RootNode.BoundingRect.Height) / 2;
		SetNodePositionRecursive(RootNode, num, num2, num3);
		ViewScale = 0.8f;
		graphBoundingRectangle = BoundingRect();
		SetViewportOrigin(graphBoundingRectangle.Location);
		BeginInvoke((Action)delegate
		{
			if (hScrollBar.Visible)
			{
				int value = hScrollBar.Value;
				hScrollBar.Value = Math.Min(value + hScrollBar.SmallChange, hScrollBar.Maximum);
				hScrollBar.Value = value;
			}
		});
	}

	private void SetNodePositionRecursive(BlackbirdSql.Common.Controls.Graphing.NodeDisplay node, int spacingY, int x, int y)
	{
		SetNodeXPostitionRecursive(node, x);
		GraphNodeLayoutHelper layoutHelper = new GraphNodeLayoutHelper();
		SetNodeYPositionRecursive(node, layoutHelper, spacingY, y);
	}

	private void SetNodeXPostitionRecursive(BlackbirdSql.Common.Controls.Graphing.NodeDisplay node, int x)
	{
		node.Position = new Point(x, 0);
		int num = node.InitializeEdges() + C_PaddingX;
		x += num;
		node.MaxChildrenXPosition = node.BoundingRect.Right;
		foreach (BlackbirdSql.Common.Controls.Graphing.NodeDisplay child in node.Children)
		{
			SetNodeXPostitionRecursive(child, x);
			node.MaxChildrenXPosition = Math.Max(node.MaxChildrenXPosition, child.MaxChildrenXPosition);
		}
	}

	private void SetNodeYPositionRecursive(BlackbirdSql.Common.Controls.Graphing.NodeDisplay node, GraphNodeLayoutHelper layoutHelper, int spacingY, int y)
	{
		int num = Math.Max(y, layoutHelper.GetYPositionForXPosition(node.MaxChildrenXPosition));
		node.Position = new Point(node.Position.X, num);
		int yPosition = num + spacingY;
		foreach (BlackbirdSql.Common.Controls.Graphing.NodeDisplay child in node.Children)
		{
			SetNodeYPositionRecursive(child, layoutHelper, spacingY, num);
			num += spacingY;
		}
		int xPosition = node.Position.X;
		if (node.Parent != null && node.Parent.Position.Y != node.Position.Y)
		{
			int num2 = int.MaxValue;
			IEdgeEnumerator edgesRelated = node.EdgesRelated;
			while (edgesRelated.MoveNext())
			{
				num2 = Math.Min(num2, (edgesRelated.Current as BlackbirdSql.Common.Controls.Graphing.EdgeDisplay).GetMidpoint());
			}
			xPosition = num2;
		}
		layoutHelper.UpdateNodeLayout(xPosition, yPosition);
	}

	protected override Microsoft.AnalysisServices.Graphing.NodeDisplay CreateNodeDisplay(INode originalNode)
	{
		return (Microsoft.AnalysisServices.Graphing.NodeDisplay)Activator.CreateInstance(((BlackbirdSql.Common.Controls.Graphing.Node)originalNode).Operation.DisplayNodeType, this, this, NodeDisplayPropertiesDefault, NodesZOrder, originalNode);
	}

	protected override IEdgeDisplay CreateEdgeDisplay(IEdge iedgeOrig, INodeDisplay inodedisplayFrom, INodeDisplay inodedisplayTo)
	{
		return new BlackbirdSql.Common.Controls.Graphing.EdgeDisplay(this, this, iedgeOrig, EdgeDisplayPropertiesDefault, EdgesZOrder, inodedisplayFrom, inodedisplayTo);
	}

	private void MoveSelectionRight()
	{
		if (CurrentSelection == null)
		{
			return;
		}
		if (CurrentSelection is BlackbirdSql.Common.Controls.Graphing.NodeDisplay)
		{
			BlackbirdSql.Common.Controls.Graphing.NodeDisplay nodeDisplay = CurrentSelection as BlackbirdSql.Common.Controls.Graphing.NodeDisplay;
			if (nodeDisplay.Children.Count <= 0)
			{
				return;
			}
			IEdgeEnumerator edgesRelated = nodeDisplay.Children[0].EdgesRelated;
			while (edgesRelated.MoveNext())
			{
				if (edgesRelated.Current.NodeFrom == nodeDisplay)
				{
					SelectEdge(edgesRelated.Current as BlackbirdSql.Common.Controls.Graphing.EdgeDisplay);
					break;
				}
			}
		}
		else if (CurrentSelection is BlackbirdSql.Common.Controls.Graphing.EdgeDisplay)
		{
			BlackbirdSql.Common.Controls.Graphing.EdgeDisplay edgeDisplay = CurrentSelection as BlackbirdSql.Common.Controls.Graphing.EdgeDisplay;
			SelectNode(edgeDisplay.NodeTo);
		}
	}

	private void MoveSelectionLeft()
	{
		if (CurrentSelection == null)
		{
			return;
		}
		if (CurrentSelection is BlackbirdSql.Common.Controls.Graphing.NodeDisplay)
		{
			BlackbirdSql.Common.Controls.Graphing.NodeDisplay nodeDisplay = CurrentSelection as BlackbirdSql.Common.Controls.Graphing.NodeDisplay;
			IEdgeEnumerator edgesRelated = nodeDisplay.EdgesRelated;
			while (edgesRelated.MoveNext())
			{
				if (edgesRelated.Current.NodeTo == nodeDisplay)
				{
					SelectEdge(edgesRelated.Current as BlackbirdSql.Common.Controls.Graphing.EdgeDisplay);
					break;
				}
			}
		}
		else if (CurrentSelection is BlackbirdSql.Common.Controls.Graphing.EdgeDisplay)
		{
			BlackbirdSql.Common.Controls.Graphing.EdgeDisplay edgeDisplay = CurrentSelection as BlackbirdSql.Common.Controls.Graphing.EdgeDisplay;
			SelectNode(edgeDisplay.NodeFrom);
		}
	}

	private void MoveSelectionDown()
	{
		if (CurrentSelection == null)
		{
			return;
		}
		if (CurrentSelection is BlackbirdSql.Common.Controls.Graphing.NodeDisplay)
		{
			BlackbirdSql.Common.Controls.Graphing.NodeDisplay nodeDisplay = CurrentSelection as BlackbirdSql.Common.Controls.Graphing.NodeDisplay;
			if (nodeDisplay.Parent != null)
			{
				nodeDisplay = nodeDisplay.Parent.Children.GetNext(nodeDisplay);
				if (nodeDisplay != null)
				{
					SelectNode(nodeDisplay);
				}
			}
		}
		else
		{
			if (CurrentSelection is not BlackbirdSql.Common.Controls.Graphing.EdgeDisplay)
			{
				return;
			}
			BlackbirdSql.Common.Controls.Graphing.EdgeDisplay edgeDisplay = CurrentSelection as BlackbirdSql.Common.Controls.Graphing.EdgeDisplay;
			BlackbirdSql.Common.Controls.Graphing.NodeDisplay nodeFrom = edgeDisplay.NodeFrom;
			IEdgeEnumerator edgesRelated = nodeFrom.EdgesRelated;
			while (edgesRelated.MoveNext())
			{
				if (edgesRelated.Current == edgeDisplay)
				{
					if (edgesRelated.MoveNext() && edgesRelated.Current.NodeFrom == nodeFrom)
					{
						SelectEdge(edgesRelated.Current as BlackbirdSql.Common.Controls.Graphing.EdgeDisplay);
					}
					break;
				}
			}
		}
	}

	private void MoveSelectionUp()
	{
		if (CurrentSelection == null)
		{
			return;
		}
		if (CurrentSelection is BlackbirdSql.Common.Controls.Graphing.NodeDisplay)
		{
			BlackbirdSql.Common.Controls.Graphing.NodeDisplay nodeDisplay = CurrentSelection as BlackbirdSql.Common.Controls.Graphing.NodeDisplay;
			if (nodeDisplay.Parent != null)
			{
				nodeDisplay = nodeDisplay.Parent.Children.GetPrevious(nodeDisplay);
				if (nodeDisplay != null)
				{
					SelectNode(nodeDisplay);
				}
			}
		}
		else
		{
			if (CurrentSelection is not BlackbirdSql.Common.Controls.Graphing.EdgeDisplay)
			{
				return;
			}
			BlackbirdSql.Common.Controls.Graphing.EdgeDisplay edgeDisplay = CurrentSelection as BlackbirdSql.Common.Controls.Graphing.EdgeDisplay;
			BlackbirdSql.Common.Controls.Graphing.NodeDisplay nodeFrom = edgeDisplay.NodeFrom;
			IEdgeEnumerator edgesRelated = nodeFrom.EdgesRelated;
			IEdge edge = null;
			while (edgesRelated.MoveNext())
			{
				if (edgesRelated.Current == edgeDisplay)
				{
					if (edge != null)
					{
						SelectEdge(edge as BlackbirdSql.Common.Controls.Graphing.EdgeDisplay);
					}
					break;
				}
				if (edgesRelated.Current.NodeFrom == nodeFrom)
				{
					edge = edgesRelated.Current;
				}
			}
		}
	}

	private void HandleToolTip(object displayObject)
	{
		if (displayObject != toolTip.CurrentTag)
		{
			toolTip.RemoveAll();
			toolTip.Tag = displayObject;
			if (displayObject is BlackbirdSql.Common.Controls.Graphing.NodeDisplay nodeDisplay)
			{
				string text = nodeDisplay.DisplayName.Split('\n')[0];
				toolTip.SetToolTip(this, text);
			}
			else if (displayObject is BlackbirdSql.Common.Controls.Graphing.EdgeDisplay)
			{
				toolTip.SetToolTip(this, "|");
			}
			string additionalTextOnToolTip = GetAdditionalTextOnToolTip(displayObject);
			toolTip.SetAdditionalText(additionalTextOnToolTip);
		}
	}

	protected virtual string GetAdditionalTextOnToolTip(object displayObject)
	{
		return string.Empty;
	}

	private void InvokePaint(PaintEventArgs e)
	{
		InvokePaint(this, e);
	}

	private void ShowExceptionMessage(Exception exception)
	{
		((IBMessageBoxProvider)new DefaultMessageBoxProvider(this)).ShowMessage(exception, (string)null, EnExceptionMessageBoxButtons.OK, EnExceptionMessageBoxSymbol.Error, (IWin32Window)this);
	}

	private void OnHelpRequested()
	{
		if (CurrentSelection is BlackbirdSql.Common.Controls.Graphing.NodeDisplay nodeDisplay && !string.IsNullOrEmpty(nodeDisplay.HelpKeyword))
		{
			// HelpViewer.DisplayTopicFromF1Keyword(nodeDisplay.HelpKeyword);
		}
	}
}
