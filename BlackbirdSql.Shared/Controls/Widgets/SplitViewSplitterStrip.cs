// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.SplitViewSplitterStrip
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Shared.Controls.Widgets;

public class SplitViewSplitterStrip : ToolStrip
{
	private class SplitBarRenderer(IServiceProvider provider) : BarBackgroundRenderer(provider)
	{
		private const int C_MINGRIPWIDTH = 4;

		private const int C_GRIPHEIGHT = 6;

		private const int C_GRIPLINESPACING = 3;

		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			base.OnRenderToolStripBackground(e);
			if (e.ToolStrip.Orientation == Orientation.Horizontal)
			{
				RenderHorizontalBackgroundTabs(e);
			}
			else
			{
				RenderVerticalBackgroundTabs(e);
			}
			RenderSplitterGrip(e);
		}

		private Rectangle GetHorizontalTabBounds(ToolStripButton button, bool inPrimaryPane, bool showSplitter)
		{
			Rectangle bounds = button.Bounds;
			int height = bounds.Height;
			if (!showSplitter)
			{
				bounds.Height++;
				bounds.Y -= 2;
				bounds.Width += height + C_GRIPHEIGHT;
				bounds.X -= 2;
			}
			else if (inPrimaryPane)
			{
				bounds.Height++;
				bounds.Y -= 2;
				bounds.Width += height + C_GRIPHEIGHT;
				bounds.X -= 2;
			}
			else
			{
				bounds.Inflate(2, 2);
				bounds.Width += height + 2;
				bounds.X -= height;
			}
			return bounds;
		}

		private Rectangle GetVerticalTabBounds(ToolStripButton button, bool inPrimaryPane, bool showSplitter)
		{
			Rectangle bounds = button.Bounds;
			int width = bounds.Width;
			if (!showSplitter)
			{
				bounds.Height += width;
				bounds.Width += 2;
			}
			else if (inPrimaryPane)
			{
				bounds.Height += width;
				bounds.Width += 2;
			}
			else
			{
				bounds.Height += width;
				bounds.Y -= width + 2;
			}
			return bounds;
		}

		private void RenderHorizontalBackgroundTabs(ToolStripRenderEventArgs e)
		{
			if (e.ToolStrip is not SplitViewSplitterStrip splitViewSplitterStrip)
			{
				return;
			}
			Graphics graphics = e.Graphics;
			ToolStripButton toolStripButton = null;
			bool flag = false;
			if (!splitViewSplitterStrip.ShowSplitter)
			{
				foreach (ToolStripButton primaryPaneButton in splitViewSplitterStrip.PrimaryPaneButtons)
				{
					if (primaryPaneButton.Visible)
					{
						Rectangle horizontalTabBounds = GetHorizontalTabBounds(primaryPaneButton, inPrimaryPane: true, showSplitter: false);
						if (primaryPaneButton.Checked)
						{
							toolStripButton = primaryPaneButton;
							flag = true;
						}
						RenderUpTab(graphics, horizontalTabBounds, primaryPaneButton.Checked);
					}
				}
				foreach (ToolStripButton secondaryPaneButton in splitViewSplitterStrip.SecondaryPaneButtons)
				{
					if (secondaryPaneButton.Visible)
					{
						Rectangle horizontalTabBounds2 = GetHorizontalTabBounds(secondaryPaneButton, inPrimaryPane: false, showSplitter: false);
						if (secondaryPaneButton.Checked)
						{
							toolStripButton = secondaryPaneButton;
							flag = false;
						}
						RenderUpTab(graphics, horizontalTabBounds2, secondaryPaneButton.Checked);
					}
				}
				using (Pen pen = new Pen(VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_THREEDSHADOW)))
				{
					graphics.DrawLine(pen, e.ToolStrip.ClientRectangle.Left, e.ToolStrip.ClientRectangle.Top, e.ToolStrip.ClientRectangle.Right, e.ToolStrip.ClientRectangle.Top);
				}
				if (toolStripButton != null)
				{
					Rectangle horizontalTabBounds3 = GetHorizontalTabBounds(toolStripButton, flag, showSplitter: false);
					RenderUpTab(graphics, horizontalTabBounds3, toolStripButton.Checked);
				}
			}
			else
			{
				if (splitViewSplitterStrip.PrimaryPaneButtons == null || splitViewSplitterStrip.SecondaryPaneButtons == null)
				{
					return;
				}
				using (Pen pen2 = new Pen(VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_THREEDSHADOW)))
				{
					graphics.DrawLine(pen2, e.ToolStrip.ClientRectangle.Left, e.ToolStrip.ClientRectangle.Top, e.ToolStrip.ClientRectangle.Right, e.ToolStrip.ClientRectangle.Top);
				}
				using (Pen pen3 = new Pen(VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_THREEDSHADOW)))
				{
					graphics.DrawLine(pen3, e.ToolStrip.ClientRectangle.Left, e.ToolStrip.ClientRectangle.Bottom - 2, e.ToolStrip.ClientRectangle.Right, e.ToolStrip.ClientRectangle.Bottom - 2);
				}
				foreach (ToolStripButton primaryPaneButton2 in splitViewSplitterStrip.PrimaryPaneButtons)
				{
					if (primaryPaneButton2.Visible)
					{
						Rectangle horizontalTabBounds4 = GetHorizontalTabBounds(primaryPaneButton2, inPrimaryPane: true, showSplitter: true);
						if (primaryPaneButton2.Checked)
						{
							toolStripButton = primaryPaneButton2;
							flag = true;
						}
						RenderUpTab(graphics, horizontalTabBounds4, primaryPaneButton2.Checked);
					}
				}
				foreach (ToolStripButton secondaryPaneButton2 in splitViewSplitterStrip.SecondaryPaneButtons)
				{
					if (secondaryPaneButton2.Visible)
					{
						Rectangle horizontalTabBounds5 = GetHorizontalTabBounds(secondaryPaneButton2, inPrimaryPane: false, showSplitter: true);
						if (secondaryPaneButton2.Checked)
						{
							toolStripButton = secondaryPaneButton2;
							flag = false;
						}
						RenderDownTab(graphics, horizontalTabBounds5, secondaryPaneButton2.Checked);
					}
				}
				if (toolStripButton != null)
				{
					Rectangle horizontalTabBounds6 = GetHorizontalTabBounds(toolStripButton, flag, showSplitter: true);
					if (flag)
					{
						RenderUpTab(graphics, horizontalTabBounds6, toolStripButton.Checked);
					}
					else
					{
						RenderDownTab(graphics, horizontalTabBounds6, toolStripButton.Checked);
					}
				}
			}
		}

		private void RenderVerticalBackgroundTabs(ToolStripRenderEventArgs e)
		{
			if (e.ToolStrip is not SplitViewSplitterStrip splitViewSplitterStrip)
			{
				return;
			}
			Graphics graphics = e.Graphics;
			ToolStripButton toolStripButton = null;
			bool flag = false;
			if (!splitViewSplitterStrip.ShowSplitter)
			{
				foreach (ToolStripButton primaryPaneButton in splitViewSplitterStrip.PrimaryPaneButtons)
				{
					if (primaryPaneButton.Visible)
					{
						Rectangle verticalTabBounds = GetVerticalTabBounds(primaryPaneButton, inPrimaryPane: true, showSplitter: false);
						if (primaryPaneButton.Checked)
						{
							toolStripButton = primaryPaneButton;
							flag = true;
						}
						RenderLeftTab(graphics, verticalTabBounds, primaryPaneButton.Checked);
					}
				}
				foreach (ToolStripButton secondaryPaneButton in splitViewSplitterStrip.SecondaryPaneButtons)
				{
					if (secondaryPaneButton.Visible)
					{
						Rectangle verticalTabBounds2 = GetVerticalTabBounds(secondaryPaneButton, inPrimaryPane: false, showSplitter: false);
						if (secondaryPaneButton.Checked)
						{
							toolStripButton = secondaryPaneButton;
							flag = false;
						}
						RenderLeftTab(graphics, verticalTabBounds2, secondaryPaneButton.Checked);
					}
				}
				using (Pen pen = new Pen(VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_THREEDDARKSHADOW)))
				{
					graphics.DrawLine(pen, e.ToolStrip.ClientRectangle.Left, e.ToolStrip.ClientRectangle.Top, e.ToolStrip.ClientRectangle.Left, e.ToolStrip.ClientRectangle.Bottom);
				}
				if (toolStripButton != null)
				{
					Rectangle verticalTabBounds3 = GetVerticalTabBounds(toolStripButton, flag, showSplitter: false);
					RenderLeftTab(graphics, verticalTabBounds3, toolStripButton.Checked);
				}
			}
			else
			{
				if (splitViewSplitterStrip.PrimaryPaneButtons == null || splitViewSplitterStrip.SecondaryPaneButtons == null)
				{
					return;
				}
				using (Pen pen2 = new Pen(VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_THREEDDARKSHADOW)))
				{
					graphics.DrawLine(pen2, e.ToolStrip.ClientRectangle.Left, e.ToolStrip.ClientRectangle.Top, e.ToolStrip.ClientRectangle.Left, e.ToolStrip.ClientRectangle.Bottom);
				}
				using (Pen pen3 = new Pen(VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_THREEDDARKSHADOW)))
				{
					graphics.DrawLine(pen3, e.ToolStrip.ClientRectangle.Right - 2, e.ToolStrip.ClientRectangle.Top, e.ToolStrip.ClientRectangle.Right - 2, e.ToolStrip.ClientRectangle.Bottom);
				}
				foreach (ToolStripButton primaryPaneButton2 in splitViewSplitterStrip.PrimaryPaneButtons)
				{
					if (primaryPaneButton2.Visible)
					{
						Rectangle verticalTabBounds4 = GetVerticalTabBounds(primaryPaneButton2, inPrimaryPane: true, showSplitter: true);
						if (primaryPaneButton2.Checked)
						{
							toolStripButton = primaryPaneButton2;
							flag = true;
						}
						RenderLeftTab(graphics, verticalTabBounds4, primaryPaneButton2.Checked);
					}
				}
				foreach (ToolStripButton secondaryPaneButton2 in splitViewSplitterStrip.SecondaryPaneButtons)
				{
					if (secondaryPaneButton2.Visible)
					{
						Rectangle verticalTabBounds5 = GetVerticalTabBounds(secondaryPaneButton2, inPrimaryPane: false, showSplitter: true);
						if (secondaryPaneButton2.Checked)
						{
							toolStripButton = secondaryPaneButton2;
							flag = false;
						}
						RenderRightTab(graphics, verticalTabBounds5, secondaryPaneButton2.Checked);
					}
				}
				if (toolStripButton != null)
				{
					Rectangle verticalTabBounds6 = GetVerticalTabBounds(toolStripButton, flag, showSplitter: true);
					if (flag)
					{
						RenderLeftTab(graphics, verticalTabBounds6, toolStripButton.Checked);
					}
					else
					{
						RenderRightTab(graphics, verticalTabBounds6, toolStripButton.Checked);
					}
				}
			}
		}

		private void RenderDownTab(Graphics g, Rectangle tabBounds, bool isActive)
		{
			int height = tabBounds.Height;
			Point point = new Point(tabBounds.Right, tabBounds.Bottom);
			Point point2 = new Point(tabBounds.Right, tabBounds.Top + 5);
			Point point3 = new Point(tabBounds.Right - 2, tabBounds.Top + 3);
			Point point4 = new Point(tabBounds.Left + height - 1, tabBounds.Top + 3);
			Point point5 = new Point(tabBounds.Left + height - S_FINASCENT, tabBounds.Top + S_FINASCENT);
			Point point6 = new Point(tabBounds.Left, tabBounds.Bottom);
			Point point7 = new Point(tabBounds.Left + height, tabBounds.Bottom);
			Point[] points = [point, point2, point3, point4, point5, point6, point7, point];
			if (!isActive)
			{
				using Brush brush = new LinearGradientBrush(tabBounds, VSCOLOR_FILETAB_GRADIENTDARK, VSCOLOR_FILETAB_GRADIENTHIGHLIGHT, LinearGradientMode.Vertical);
				g.FillPolygon(brush, points);
			}
			else
			{
				using Brush brush2 = new SolidBrush(VSCOLOR_FILETAB_SELECTEDBACKGROUND);
				g.FillPolygon(brush2, points);
			}
			g.DrawLine(SystemPens.ControlDark, point, point2);
			g.DrawLine(SystemPens.ControlDark, point3, point4);
			using (Pen pen = new Pen(ColorTable.ImageMarginRevealedGradientEnd))
			{
				g.DrawLine(pen, point.X - 1, point.Y, point2.X - 1, point2.Y);
			}
			using (Pen pen2 = new Pen(VSCOLOR_FILETAB_GRADIENTHIGHLIGHT))
			{
				g.DrawLine(SystemPens.ControlDark, point5.X - 1, point5.Y + 1, point6.X, point6.Y);
				g.DrawLine(pen2, point5.X + 1, point5.Y, point6.X + 1, point6.Y);
				g.DrawLine(pen2, point4.X - 2, point4.Y + 1, point3.X, point3.Y + 1);
				g.DrawLine(pen2, point5.X + 1, point5.Y + 1, point5.X + 2, point5.Y + 1);
				g.DrawLine(SystemPens.ControlDark, point5.X + 1, point5.Y, point5.X + 2, point5.Y);
				g.DrawLine(SystemPens.ControlDark, point5.X - 1, point5.Y + 1, point5.X, point5.Y + 1);
			}
			g.FillRectangle(SystemBrushes.ControlDark, point2.X - 1, point2.Y - 1, 1, 1);
		}

		private void RenderLeftTab(Graphics g, Rectangle tabBounds, bool isActive)
		{
			int width = tabBounds.Width;
			Point point = new Point(0, tabBounds.Top);
			Point point2 = new Point(tabBounds.Right - C_MINGRIPWIDTH, tabBounds.Top);
			Point point3 = new Point(point2.X + 2, tabBounds.Bottom - width - S_FINASCENT + 2);
			Point point4 = new Point(point3.X, tabBounds.Top + 2);
			Point point5 = new Point(point3.X - 2, point3.Y + S_FINASCENT);
			Point point6 = new Point(tabBounds.Left, tabBounds.Bottom - 2);
			Point point7 = new Point(0, tabBounds.Bottom - width);
			Point[] points = [point, point2, point4, point3, point5, point6, point7, point];
			if (!isActive)
			{
				using Brush brush = new LinearGradientBrush(tabBounds, VSCOLOR_FILETAB_GRADIENTHIGHLIGHT, VSCOLOR_FILETAB_GRADIENTDARK, LinearGradientMode.Horizontal);
				g.FillPolygon(brush, points);
			}
			else
			{
				using Brush brush2 = new SolidBrush(VSCOLOR_FILETAB_SELECTEDBACKGROUND);
				g.FillPolygon(brush2, points);
			}
			g.DrawLine(SystemPens.ControlDark, point, point2);
			g.DrawLine(SystemPens.ControlDark, point4, point3);
			using (Pen pen = new Pen(VSCOLOR_FILETAB_GRADIENTHIGHLIGHT))
			{
				g.DrawLine(pen, point.X - 1, point.Y + 1, point2.X - 1, point2.Y + 1);
			}
			using (Pen pen2 = new Pen(ColorTable.ImageMarginRevealedGradientEnd))
			{
				g.DrawLine(SystemPens.ControlDark, point5.X - 1, point5.Y + 1, point6.X, point6.Y);
				g.DrawLine(pen2, point5.X - 1, point5.Y, point6.X - 1, point6.Y);
				g.DrawLine(pen2, point3.X - 1, point3.Y, point3.X - 1, point3.Y + 1);
				g.DrawLine(pen2, point3.X - 2, point3.Y + 2, point3.X - 2, point3.Y + 3);
				g.DrawLine(SystemPens.ControlDark, point3.X - 1, point3.Y + 1, point3.X - 1, point3.Y + 2);
				g.DrawLine(SystemPens.ControlDark, point3.X - 2, point3.Y + 3, point3.X - 2, point3.Y + C_MINGRIPWIDTH);
			}
			g.FillRectangle(SystemBrushes.ControlDark, point2.X + 1, point2.Y + 1, 1, 1);
		}

		private void RenderRightTab(Graphics g, Rectangle tabBounds, bool isActive)
		{
			int width = tabBounds.Width;
			Point point = new Point(tabBounds.Right, tabBounds.Bottom);
			Point point2 = new Point(tabBounds.Left + C_MINGRIPWIDTH, tabBounds.Bottom);
			Point point3 = new Point(point2.X - 2, tabBounds.Top + width + S_FINASCENT - 2);
			Point point4 = new Point(point3.X, tabBounds.Bottom - 2);
			Point point5 = new Point(point3.X + 2, point3.Y - S_FINASCENT);
			Point point6 = new Point(tabBounds.Right, tabBounds.Top + 2);
			Point point7 = new Point(tabBounds.Right, tabBounds.Top + width);
			Point[] points = [point, point2, point4, point3, point5, point6, point7, point];
			if (!isActive)
			{
				using Brush brush = new LinearGradientBrush(tabBounds, VSCOLOR_FILETAB_GRADIENTDARK, VSCOLOR_FILETAB_GRADIENTHIGHLIGHT, LinearGradientMode.Horizontal);
				g.FillPolygon(brush, points);
			}
			else
			{
				using Brush brush2 = new SolidBrush(VSCOLOR_FILETAB_SELECTEDBACKGROUND);
				g.FillPolygon(brush2, points);
			}
			g.DrawLine(SystemPens.ControlDark, point, point2);
			g.DrawLine(SystemPens.ControlDark, point4, point3);
			using (Pen pen = new Pen(ColorTable.ImageMarginRevealedGradientEnd))
			{
				g.DrawLine(pen, point.X - 1, point.Y - 1, point2.X - 1, point2.Y - 1);
			}
			using (Pen pen2 = new Pen(VSCOLOR_FILETAB_GRADIENTHIGHLIGHT))
			{
				g.DrawLine(SystemPens.ControlDark, point5.X, point5.Y, point6.X, point6.Y);
				g.DrawLine(pen2, point5.X - 1, point5.Y + 2, point6.X - 1, point6.Y + 2);
				g.DrawLine(pen2, point3.X + 1, point3.Y, point3.X + 1, point4.Y + 1);
				g.DrawLine(pen2, point3.X + 2, point3.Y - 1, point3.X + 2, point3.Y - 2);
				g.DrawLine(SystemPens.ControlDark, point3.X + 1, point3.Y - 1, point3.X + 1, point3.Y - 2);
				g.DrawLine(SystemPens.ControlDark, point3.X + 2, point3.Y - 3, point3.X + 2, point3.Y - C_MINGRIPWIDTH);
			}
			g.FillRectangle(SystemBrushes.ControlDark, point2.X - 1, point2.Y - 1, 1, 1);
		}

		private static void RenderSplitterGrip(ToolStripRenderEventArgs e)
		{
			if (e.ToolStrip is not SplitViewSplitterStrip splitViewSplitterStrip)
			{
				return;
			}
			Graphics graphics = e.Graphics;
			Rectangle splitterRectangle = splitViewSplitterStrip.SplitterRectangle;
			if (splitViewSplitterStrip.Orientation == Orientation.Horizontal)
			{
				int num = Math.Min(splitterRectangle.Width, 50);
				Point point = new Point(splitterRectangle.X + (splitterRectangle.Width - num) / 2, splitterRectangle.Y + (splitterRectangle.Height / 2 - 3));
				Point point2 = point;
				point2.Offset(0, C_GRIPLINESPACING);
				if (num > 4)
				{
					graphics.DrawLine(SystemPens.ButtonHighlight, point.X + 1, point.Y, point.X + num - 1, point.Y);
					graphics.DrawLine(SystemPens.ButtonHighlight, point.X, point.Y + 1, point.X + num, point.Y + 1);
					graphics.DrawLine(SystemPens.ControlDark, point.X + 1, point.Y + 1, point.X + num - 1, point.Y + 1);
					graphics.DrawLine(SystemPens.ButtonHighlight, point2.X + 1, point2.Y, point2.X + num - 1, point2.Y);
					graphics.DrawLine(SystemPens.ButtonHighlight, point2.X, point2.Y + 1, point2.X + num, point2.Y + 1);
					graphics.DrawLine(SystemPens.ControlDark, point2.X + 1, point2.Y + 1, point2.X + num - 1, point2.Y + 1);
				}
			}
			else
			{
				int num2 = Math.Min(splitterRectangle.Height, 50);
				Point point3 = new Point(splitterRectangle.X + (splitterRectangle.Width / 2 - 3), splitterRectangle.Y + (splitterRectangle.Height - num2) / 2);
				Point point4 = point3;
				point4.Offset(C_MINGRIPWIDTH, 0);
				if (num2 > C_MINGRIPWIDTH)
				{
					graphics.DrawLine(SystemPens.ButtonHighlight, point3.X, point3.Y + 1, point3.X, point3.Y + num2 - 1);
					graphics.DrawLine(SystemPens.ButtonHighlight, point3.X + 1, point3.Y, point3.X + 1, point3.Y + num2);
					graphics.DrawLine(SystemPens.ControlDark, point3.X + 1, point3.Y + 1, point3.X + 1, point3.Y + num2 - 1);
					graphics.DrawLine(SystemPens.ButtonHighlight, point4.X, point4.Y + 1, point4.X, point4.Y + num2 - 1);
					graphics.DrawLine(SystemPens.ButtonHighlight, point4.X + 1, point4.Y, point4.X + 1, point4.Y + num2);
					graphics.DrawLine(SystemPens.ControlDark, point4.X + 1, point4.Y + 1, point4.X + 1, point4.Y + num2 - 1);
				}
			}
		}

		private void RenderUpTab(Graphics g, Rectangle tabBounds, bool isActive)
		{
			int height = tabBounds.Height;
			Point point = new Point(tabBounds.Left, tabBounds.Top);
			Point point2 = new Point(tabBounds.Left, tabBounds.Bottom - 2);
			Point point3 = new Point(tabBounds.Left + 2, tabBounds.Bottom);
			Point point4 = new Point(tabBounds.Right - height - 2, tabBounds.Bottom);
			Point point5 = new Point(tabBounds.Right - height + S_FINASCENT - 1, tabBounds.Bottom - S_FINASCENT + 1);
			Point point6 = new Point(tabBounds.Right, tabBounds.Top);
			Point point7 = new Point(tabBounds.Right - height, tabBounds.Top);
			Point[] points = [point, point2, point3, point4, point5, point6, point7, point];
			if (!isActive)
			{
				using Brush brush = new LinearGradientBrush(tabBounds, VSCOLOR_FILETAB_GRADIENTHIGHLIGHT, VSCOLOR_FILETAB_GRADIENTDARK, LinearGradientMode.Vertical);
				g.FillPolygon(brush, points);
			}
			else
			{
				using Brush brush2 = new SolidBrush(VSCOLOR_FILETAB_SELECTEDBACKGROUND);
				g.FillPolygon(brush2, points);
			}
			g.DrawLine(SystemPens.ControlDark, point, point2);
			g.DrawLine(SystemPens.ControlDark, point3, point4);
			using (Pen pen = new Pen(VSCOLOR_FILETAB_GRADIENTHIGHLIGHT))
			{
				g.DrawLine(pen, point.X + 1, point.Y, point2.X + 1, point2.Y);
			}
			using (Pen pen2 = new Pen(ColorTable.ImageMarginRevealedGradientEnd))
			{
				g.DrawLine(SystemPens.ControlDark, point5, point6);
				g.DrawLine(pen2, point5.X - 1, point5.Y, point6.X - 1, point6.Y);
				g.DrawLine(pen2, point5.X - 2, point5.Y + 1, point5.X - 3, point5.Y + 1);
				g.DrawLine(SystemPens.ControlDark, point5.X - 1, point5.Y + 1, point5.X - 2, point5.Y + 1);
				g.DrawLine(pen2, point5.X - 4, point5.Y + 2, point5.X - 5, point5.Y + 2);
				g.DrawLine(SystemPens.ControlDark, point5.X - 3, point5.Y + 2, point5.X - 4, point5.Y + 2);
			}
			g.FillRectangle(SystemBrushes.ControlDark, point.X + 1, point2.Y + 1, 1, 1);
		}

		protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
		{
			if (e.Item is ToolStripButton toolStripButton && toolStripButton.Owner is SplitViewSplitterStrip splitViewSplitterStrip && toolStripButton != null && !splitViewSplitterStrip.PrimaryPaneButtons.Contains(toolStripButton) && !splitViewSplitterStrip.SecondaryPaneButtons.Contains(toolStripButton))
			{
				base.OnRenderButtonBackground(e);
			}
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			ToolStripItem item = e.Item;
			if (item == null)
			{
				return;
			}
			if (item.Owner is not SplitViewSplitterStrip splitViewSplitterStrip)
			{
				base.OnRenderItemText(e);
				return;
			}
			if (item is ToolStripButton item2 && (splitViewSplitterStrip.PrimaryPaneButtons.Contains(item2) || splitViewSplitterStrip.SecondaryPaneButtons.Contains(item2)))
			{
				e.TextColor = VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_WINDOWTEXT);
				Rectangle textRectangle = e.TextRectangle;
				Rectangle bounds = e.Item.Bounds;
				if (!splitViewSplitterStrip.ShowSplitter)
				{
					Rectangle rectangle = splitViewSplitterStrip.Orientation == Orientation.Horizontal ? Rectangle.FromLTRB(0, Math.Abs(bounds.Y), bounds.Width, bounds.Height) : Rectangle.FromLTRB(Math.Abs(bounds.X), 0, bounds.Width, bounds.Height);
					if (splitViewSplitterStrip.Orientation == Orientation.Horizontal)
					{
						textRectangle.Y = rectangle.Y + (rectangle.Height - textRectangle.Height) / 2 - 1;
						textRectangle.X += 2;
					}
					else
					{
						textRectangle.X = rectangle.X + (rectangle.Width - textRectangle.Width) / 2 + 1;
						textRectangle.Y += 2;
					}
				}
				else if (splitViewSplitterStrip.Orientation == Orientation.Vertical)
				{
					Rectangle rectangle2 = Rectangle.FromLTRB(0, 0, splitViewSplitterStrip.Width, textRectangle.Height);
					textRectangle.X = rectangle2.X + 1 + (rectangle2.Width - textRectangle.Width) / 2;
				}
				if (splitViewSplitterStrip.Orientation == Orientation.Horizontal)
				{
					textRectangle.X += item.Padding.Horizontal;
				}
				else if (e.Item.TextImageRelation == TextImageRelation.ImageAboveText)
				{
					textRectangle.Y += item.Padding.Vertical;
				}
				else
				{
					textRectangle.Y -= item.Padding.Vertical;
				}
				e.TextRectangle = textRectangle;
			}
			base.OnRenderItemText(e);
		}

		protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
		{
			ToolStripItem item = e.Item;
			if (item == null || item.Owner is not SplitViewSplitterStrip splitViewSplitterStrip)
			{
				return;
			}
			bool flag = item is ToolStripButton && splitViewSplitterStrip.PrimaryPaneButtons.Contains(item as ToolStripButton);
			bool flag2 = item is ToolStripButton && splitViewSplitterStrip.SecondaryPaneButtons.Contains(item as ToolStripButton);
			if (flag || flag2)
			{
				Rectangle imageRectangle = e.ImageRectangle;
				Rectangle bounds = e.Item.Bounds;
				Rectangle rectangle = splitViewSplitterStrip.Orientation == Orientation.Horizontal ? Rectangle.FromLTRB(0, Math.Abs(bounds.Y), bounds.Width, bounds.Height) : Rectangle.FromLTRB(Math.Abs(bounds.X), 0, bounds.Width, bounds.Height);
				switch (splitViewSplitterStrip.Orientation)
				{
					case Orientation.Horizontal:
						if (!splitViewSplitterStrip.ShowSplitter)
						{
							imageRectangle.Y = rectangle.Y + (rectangle.Height - imageRectangle.Height) / 2;
							imageRectangle.X += 2;
						}
						else
						{
							imageRectangle.Y += flag ? 1 : 0;
						}
						imageRectangle.X += item.Padding.Horizontal;
						break;
					case Orientation.Vertical:
						if (!splitViewSplitterStrip.ShowSplitter)
						{
							imageRectangle.X = rectangle.X + (rectangle.Width - imageRectangle.Width) / 2;
							imageRectangle.Y += 2;
						}
						else
						{
							Rectangle rectangle2 = Rectangle.FromLTRB(0, 0, splitViewSplitterStrip.Width, bounds.Height);
							imageRectangle.X = rectangle2.X + 1 + (rectangle2.Width - imageRectangle.Width) / 2;
						}
						if (e.Item.TextImageRelation == TextImageRelation.ImageAboveText)
						{
							imageRectangle.Y += item.Padding.Top;
							imageRectangle.X += flag ? 1 : 0;
						}
						else
						{
							imageRectangle.Y -= item.Padding.Bottom;
							imageRectangle.X += flag ? 2 : 0;
						}
						break;
				}
				e = new ToolStripItemImageRenderEventArgs(e.Graphics, e.Item, imageRectangle);
			}
			if (e.Image != null)
			{
				bool flag3 = item is ToolStripButton toolStripButton && toolStripButton.Checked;
				ColorMap[] array =
				[
					new ColorMap()
				];
				array[0].OldColor = Color.Black;
				array[0].NewColor = flag3 && SystemInformation.HighContrast ? VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_HIGHLIGHTTEXT) : VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_WINDOWTEXT);
				using ImageAttributes imageAttributes = new ImageAttributes();
				imageAttributes.SetRemapTable(array, ColorAdjustType.Bitmap);
				e.Graphics.DrawImage(e.Image, e.ImageRectangle, 0, 0, e.ImageRectangle.Width, e.ImageRectangle.Height, GraphicsUnit.Pixel, imageAttributes);
				return;
			}
			base.OnRenderItemImage(e);
		}
	}

	private ToolStripButton _hSplitButton;

	private ToolStripButton _vSplitButton;

	private ToolStripButton _button2;

	private ToolStripButton _button1;

	private ToolStripButton _swapButton;

	private ToolStripButton _chevronButton;

	private List<ToolStripButton> _primaryPaneButtons;

	private List<ToolStripButton> _secondaryPaneButtons;

	private bool _showSplitter = true;

	private int _minDistance;

	private int _minHeight;

	public const int C_GRIPWIDTH = 50;

	public static int S_FINASCENT = 4;

	private Rectangle _splitterRectangle;

	private Orientation _lastOrientation;

	private bool _invalidating;

	private readonly VsFontColorPreferences _VsFontColorPreferences;

	public ToolStripButton Button1
	{
		get
		{
			if (_button1 == null)
			{
				_button1 = CreateButton("button1", ControlsResources.SplitViewContainer_DefaultButton1Text, null);
				_button1.DoubleClickEnabled = true;
				_button1.AutoSize = true;
				_button1.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
				_button1.Padding = new Padding(1);
				_button1.Tag = VSConstants.LOGVIEWID_Designer;
			}
			return _button1;
		}
	}

	public ToolStripButton SecondaryPaneLastVisibleOrDefault
	{
		get
		{
			for (int num = SecondaryPaneButtons.Count - 1; num >= 0; num--)
			{
				if (SecondaryPaneButtons[num].Visible)
				{
					return SecondaryPaneButtons[num];
				}
			}
			return SecondaryPaneButtons[^1];
		}
	}

	public ToolStripButton PrimaryPaneFirstButton => PrimaryPaneButtons[0];

	public ToolStripButton PrimaryPaneLastVisibleButtonOrDefault
	{
		get
		{
			for (int num = PrimaryPaneButtons.Count - 1; num >= 0; num--)
			{
				if (PrimaryPaneButtons[num].Visible)
				{
					return PrimaryPaneButtons[num];
				}
			}
			return PrimaryPaneButtons[^1];
		}
	}

	public List<ToolStripButton> PrimaryPaneButtons
	{
		get
		{
			_primaryPaneButtons ??=
				[
					Button2
				];
			return _primaryPaneButtons;
		}
	}

	public List<ToolStripButton> SecondaryPaneButtons
	{
		get
		{
			_secondaryPaneButtons ??=
				[
					Button1
				];
			return _secondaryPaneButtons;
		}
	}

	public ToolStripButton SwapButton
	{
		get
		{
			if (_swapButton == null)
			{
				_swapButton = CreateButton("swapButton", ControlsResources.TabbedEditor_SwapButton, ControlsResources.swap_horz);
				_swapButton.Margin = new Padding(4, 2, 4 + SecondaryPaneLastVisibleOrDefault.Height, 2);
			}
			return _swapButton;
		}
	}

	public ToolStripButton HSplitButton
	{
		get
		{
			if (_hSplitButton == null)
			{
				_hSplitButton = CreateButton("hsplitButton", ControlsResources.TabbedEditor_HorizontalSplit, ControlsResources.hsplit);
				_hSplitButton.Checked = _showSplitter && Orientation == Orientation.Horizontal;
				_hSplitButton.Alignment = ToolStripItemAlignment.Right;
			}
			return _hSplitButton;
		}
	}

	public ToolStripButton VSplitButton
	{
		get
		{
			if (_vSplitButton == null)
			{
				_vSplitButton = CreateButton("vsplitButton", ControlsResources.TabbedEditor_VerticalSplit, ControlsResources.vsplit);
				_vSplitButton.Checked = _showSplitter && Orientation == Orientation.Vertical;
				_vSplitButton.Alignment = ToolStripItemAlignment.Right;
			}
			return _vSplitButton;
		}
	}

	public ToolStripButton ChevronButton
	{
		get
		{
			if (_chevronButton == null)
			{
				_chevronButton = CreateButton("chevronButton", ControlsResources.TabbedEditor_CollapsePane, ControlsResources.horizontal_collapse);
				_chevronButton.Checked = !_showSplitter;
				_chevronButton.Alignment = ToolStripItemAlignment.Right;
			}
			return _chevronButton;
		}
	}

	public ToolStripButton Button2
	{
		get
		{
			if (_button2 == null)
			{
				_button2 = CreateButton("button2", ControlsResources.SplitViewContainer_DefaultButton2Text, null);
				_button2.DoubleClickEnabled = true;
				_button2.AutoSize = true;
				_button2.DisplayStyle = ToolStripItemDisplayStyle.Text;
				_button2.Padding = new Padding(1);
				_button2.Tag = VSConstants.LOGVIEWID_TextView;
			}
			return _button2;
		}
	}

	protected override Padding DefaultPadding
	{
		get
		{
			Padding result = base.DefaultPadding;
			if (Orientation == Orientation.Horizontal)
			{
				result.Right = 2;
				result.Left = 10;
				result.Top = 0;
				result.Bottom = 1;
			}
			else
			{
				result.Bottom = SystemInformation.HorizontalScrollBarHeight;
				result.Top = 8;
				result.Bottom = 2;
				result.Left = 0;
				result.Right = 1;
			}
			return result;
		}
	}

	public override Size MinimumSize
	{
		get
		{
			if (Orientation == Orientation.Horizontal)
			{
				return new Size(_minDistance, _minHeight);
			}
			return new Size(_minHeight, _minDistance);
		}
		set
		{
			base.MinimumSize = value;
		}
	}

	public bool ShowSplitter
	{
		get
		{
			return _showSplitter;
		}
		set
		{
			if (_showSplitter == value)
			{
				return;
			}
			_showSplitter = value;
			HSplitButton.Checked = _showSplitter && Orientation == Orientation.Horizontal;
			VSplitButton.Checked = _showSplitter && Orientation == Orientation.Vertical;
			UpdateChevronButton();
			SwapButton.Visible = _showSplitter;
			if (ShowSplitterChangedEvent != null)
			{
				_ = Handle;
				BeginInvoke((EventHandler)delegate
				{
					ShowSplitterChangedEvent(this, EventArgs.Empty);
				});
			}
		}
	}

	public Rectangle SplitterRectangle => _splitterRectangle;

	public event EventHandler DesignerXamlDoubleClickEvent;

	public event EventHandler DesignerXamlClickEvent;

	public event EventHandler ShowSplitterChangedEvent;

	public SplitViewSplitterStrip(IServiceProvider serviceProvider)
	{
		SuspendLayout();

		try
		{
			Name = "_tabStrip";
			Text = ControlsResources.TabbedEditor_SplitStrip;
			_lastOrientation = Orientation;
			GripStyle = ToolStripGripStyle.Hidden;
			CanOverflow = false;
			AutoSize = false;
			Size = new Size(20, 20);
			foreach (ToolStripButton secondaryPaneButton in SecondaryPaneButtons)
			{
				Items.Add(secondaryPaneButton);
			}
			Items.Add(SwapButton);
			foreach (ToolStripButton primaryPaneButton in PrimaryPaneButtons)
			{
				Items.Add(primaryPaneButton);
			}
			Items.AddRange([ChevronButton, HSplitButton, VSplitButton]);
			UpdateMinimumSize();
		}
		finally
		{
			ResumeLayout(performLayout: false);
		}

		Renderer = new SplitBarRenderer(serviceProvider);
		Button1.DoubleClick += DesignerXamlButton_DoubleClick;
		Button2.DoubleClick += DesignerXamlButton_DoubleClick;
		foreach (ToolStripButton primaryPaneButton2 in PrimaryPaneButtons)
		{
			primaryPaneButton2.DoubleClick += DesignerXamlButton_DoubleClick;
			primaryPaneButton2.Click += DesignerXamlButton_Click;
		}
		foreach (ToolStripButton secondaryPaneButton2 in SecondaryPaneButtons)
		{
			secondaryPaneButton2.DoubleClick += DesignerXamlButton_DoubleClick;
			secondaryPaneButton2.Click += DesignerXamlButton_Click;
		}
		_VsFontColorPreferences = new VsFontColorPreferences();
		_VsFontColorPreferences.PreferencesChangedEvent += VsFontColorPreferences_PreferencesChanged;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_VsFontColorPreferences.PreferencesChangedEvent -= VsFontColorPreferences_PreferencesChanged;
			_VsFontColorPreferences.Dispose();
		}
		base.Dispose(disposing);
	}

	private void VsFontColorPreferences_PreferencesChanged(object sender, EventArgs args)
	{
		foreach (ToolStripButton item in EnumerateAllButtons())
		{
			item.Font = VsFontColorPreferences.EnvironmentFont;
		}
		UpdateMinimumSize();
	}

	private ToolStripButton CreateButton(string name, string text, Image image)
	{
		SizeF scaleFactor = ControlUtils.GetScaleFactor(this);
		return new ToolStripButton(text, image == null ? null : ControlUtils.ScaleImage(image, scaleFactor))
		{
			Name = name,
			Margin = Padding.Empty,
			Padding = Padding.Empty,
			ImageScaling = ToolStripItemImageScaling.None,
			DisplayStyle = ToolStripItemDisplayStyle.Image,
			ImageTransparentColor = Color.White,
			ToolTipText = text,
			AutoSize = false,
			Size = ControlUtils.GetScaledSize(this, new Size(15, 15)),
			Font = VsFontColorPreferences.EnvironmentFont
		};
	}

	public IEnumerable<ToolStripButton> EnumerateAllButtons(bool visibleOnly = false)
	{
		foreach (ToolStripButton primaryPaneButton in PrimaryPaneButtons)
		{
			if (!visibleOnly || primaryPaneButton.Visible)
			{
				yield return primaryPaneButton;
			}
		}
		foreach (ToolStripButton secondaryPaneButton in SecondaryPaneButtons)
		{
			if (!visibleOnly || secondaryPaneButton.Visible)
			{
				yield return secondaryPaneButton;
			}
		}
	}

	public void SetTabButtonVisibleStatus(Guid buttonTagGuid, bool visible)
	{
		foreach (ToolStripButton item in EnumerateAllButtons())
		{
			if ((Guid)item.Tag == buttonTagGuid)
			{
				item.Visible = visible;
				break;
			}
		}
	}

	public bool GetTabButtonVisibleStatus(Guid buttonTagGuid)
	{
		bool result = false;
		foreach (ToolStripButton item in EnumerateAllButtons())
		{
			if ((Guid)item.Tag == buttonTagGuid)
			{
				result = item.Visible;
				break;
			}
		}
		return result;
	}

	public void EnsureButtonInDesignPane(Guid buttonGuid)
	{
		EnsureButtonInPane(buttonGuid, Button1);
	}

	public void EnsureButtonInXamlPane(Guid buttonGuid)
	{
		EnsureButtonInPane(buttonGuid, Button2);
	}

	private void EnsureButtonInPane(Guid buttonGuid, ToolStripButton referenceButton)
	{
		List<ToolStripButton> list = null;
		if (PrimaryPaneButtons.Contains(referenceButton))
		{
			list = PrimaryPaneButtons;
		}
		else if (SecondaryPaneButtons.Contains(referenceButton))
		{
			list = SecondaryPaneButtons;
		}
		if (list == null)
		{
			return;
		}
		foreach (ToolStripButton item in list)
		{
			if ((Guid)item.Tag == buttonGuid)
			{
				return;
			}
		}
		CreateButtonInPrimaryOrSecondaryPane(list, buttonGuid);
	}

	private void CreateButtonInPrimaryOrSecondaryPane(List<ToolStripButton> primaryOrSecondaryButtons, Guid buttonGuid)
	{
		ToolStripButton toolStripButton = CreateButton("button", ControlsResources.SplitViewContainer_DefaultButtonText, null);
		toolStripButton.DoubleClickEnabled = true;
		toolStripButton.AutoSize = true;
		toolStripButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
		toolStripButton.Padding = new Padding(1);
		toolStripButton.Tag = buttonGuid;
		toolStripButton.Click += DesignerXamlButton_Click;
		toolStripButton.DoubleClick += DesignerXamlButton_DoubleClick;


		SuspendLayout();

		try
		{
			int num = Items.IndexOf(primaryOrSecondaryButtons[^1]);
			ToolStripItem[] array = new ToolStripItem[num + 1];
			ToolStripItem[] array2 = new ToolStripItem[Items.Count - num - 1];
			ToolStripItem[] array3 = new ToolStripItem[Items.Count];
			Items.CopyTo(array3, 0);
			Array.Copy(array3, array, num + 1);
			Array.ConstrainedCopy(array3, num + 1, array2, 0, Items.Count - num - 1);
			Items.Clear();
			Items.AddRange(array);
			Items.Add(toolStripButton);
			Items.AddRange(array2);
			primaryOrSecondaryButtons.Add(toolStripButton);
		}
		finally
		{
			ResumeLayout();
		}
	}

	public ToolStripButton GetButtonInCyclicOrderFromChecked(bool forward)
	{
		IEnumerator<ToolStripButton> enumerator = EnumerateAllButtons(visibleOnly: true).GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return null;
		}
		ToolStripButton toolStripButton = null;
		ToolStripButton toolStripButton3 = null;
		bool flag = false;
		while (!flag)
		{
			ToolStripButton toolStripButton2 = enumerator.Current;
			if (toolStripButton2.Checked)
			{
				flag = true;
				bool flag2 = enumerator.MoveNext();
				toolStripButton3 = enumerator.Current;
				if (!flag2)
				{
					toolStripButton3 = PrimaryPaneButtons[0];
					if (!toolStripButton3.Visible)
					{
						enumerator.Reset();
						_ = enumerator.MoveNext();
						toolStripButton3 = enumerator.Current;
					}
				}
			}
			else
			{
				toolStripButton = toolStripButton2;
				if (!enumerator.MoveNext())
				{
					return null;
				}
			}
		}
		if (flag && !forward && toolStripButton == null)
		{
			while (enumerator.MoveNext())
			{
			}
			toolStripButton = enumerator.Current;
		}
		if (forward)
		{
			return toolStripButton3;
		}
		return toolStripButton;
	}

	public Rectangle GetPrimaryPaneButtonBounds(int index)
	{
		if (PrimaryPaneButtons != null)
		{
			Rectangle bounds = PrimaryPaneButtons[index].Bounds;
			Padding margin = SecondaryPaneLastVisibleOrDefault.Margin;
			bounds.X--;
			if (ShowSplitter)
			{
				if (Orientation == Orientation.Horizontal)
				{
					bounds.Width += margin.Right + 6;
				}
				else
				{
					bounds.Height += margin.Bottom;
				}
			}
			else
			{
				bounds.Size += margin.Size;
			}
			return bounds;
		}
		return Rectangle.Empty;
	}

	public Rectangle GetSecondaryPaneButtonBounds(int index)
	{
		if (SecondaryPaneButtons != null)
		{
			Rectangle bounds = SecondaryPaneButtons[index].Bounds;
			Padding margin = SecondaryPaneLastVisibleOrDefault.Margin;
			if (ShowSplitter)
			{
				if (Orientation == Orientation.Horizontal)
				{
					bounds.X -= margin.Right;
					bounds.Width += margin.Right + 6;
					bounds.Height += 2;
				}
				else
				{
					bounds.Y -= margin.Bottom;
					bounds.Height += margin.Bottom;
				}
			}
			else
			{
				bounds.Height += margin.Bottom;
				bounds.Width += margin.Right;
			}
			return bounds;
		}
		return Rectangle.Empty;
	}

	private void DesignerXamlButton_DoubleClick(object sender, EventArgs e)
	{
		DesignerXamlDoubleClickEvent?.Invoke(sender, EventArgs.Empty);
	}

	private void DesignerXamlButton_Click(object sender, EventArgs e)
	{
		DesignerXamlClickEvent?.Invoke(sender, EventArgs.Empty);
	}

	protected override object GetService(Type service)
	{
		if (Parent != null)
		{
			object service2 = ((IServiceProvider)Parent).GetService(service);
			if (service2 != null)
			{
				return service2;
			}
		}
		return base.GetService(service);
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		Size preferredSize = base.GetPreferredSize(proposedSize);
		if (Orientation == Orientation.Horizontal)
		{
			preferredSize.Width += 52;
		}
		else
		{
			preferredSize.Height += 52;
		}
		return preferredSize;
	}

	private bool IsClickWithinPrimaryTab(int index, MouseEventArgs e)
	{
		Rectangle primaryPaneButtonBounds = GetPrimaryPaneButtonBounds(index);
		if (primaryPaneButtonBounds.Contains(e.Location))
		{
			int num = primaryPaneButtonBounds.Height;
			Point point = new Point(primaryPaneButtonBounds.Right - num, Math.Max(0, primaryPaneButtonBounds.Y));
			if (Orientation == Orientation.Horizontal)
			{
				return e.X - point.X + e.Y - point.Y <= num;
			}
			return e.X - point.X >= e.Y - point.Y;
		}
		return false;
	}

	private bool IsClickWithinSecondaryTab(int index, MouseEventArgs e)
	{
		Rectangle secondaryPaneButtonBounds = GetSecondaryPaneButtonBounds(index);
		if (secondaryPaneButtonBounds.Contains(e.Location))
		{
			Point location = secondaryPaneButtonBounds.Location;
			int num = secondaryPaneButtonBounds.Height;
			if (Orientation == Orientation.Horizontal)
			{
				return e.X - location.X + e.Y - location.Y >= num;
			}
			return e.X - location.X <= e.Y - location.Y;
		}
		return false;
	}

	protected override void OnInvalidated(InvalidateEventArgs e)
	{
		base.OnInvalidated(e);

		if (_invalidating)
			return;


		_invalidating = true;

		try
		{
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < PrimaryPaneButtons.Count; i++)
			{
				Rectangle primaryPaneButtonBounds = GetPrimaryPaneButtonBounds(i);
				if (e.InvalidRect.IntersectsWith(primaryPaneButtonBounds))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < SecondaryPaneButtons.Count; j++)
				{
					Rectangle secondaryPaneButtonBounds = GetSecondaryPaneButtonBounds(j);
					if (e.InvalidRect.IntersectsWith(secondaryPaneButtonBounds))
					{
						flag2 = true;
						break;
					}
				}
			}
			if (ShowSplitter)
			{
				if (flag)
				{
					for (int k = 0; k < PrimaryPaneButtons.Count; k++)
					{
						Rectangle primaryPaneButtonBounds2 = GetPrimaryPaneButtonBounds(k);
						if (e.InvalidRect.IntersectsWith(primaryPaneButtonBounds2))
						{
							Invalidate(primaryPaneButtonBounds2);
						}
					}
				}
				if (!flag2)
				{
					return;
				}
				for (int l = 0; l < SecondaryPaneButtons.Count; l++)
				{
					Rectangle secondaryPaneButtonBounds2 = GetSecondaryPaneButtonBounds(l);
					if (e.InvalidRect.IntersectsWith(secondaryPaneButtonBounds2))
					{
						Invalidate(secondaryPaneButtonBounds2);
					}
				}
			}
			else
			{
				if (!(flag || flag2))
				{
					return;
				}
				for (int m = 0; m < PrimaryPaneButtons.Count; m++)
				{
					Rectangle primaryPaneButtonBounds3 = GetPrimaryPaneButtonBounds(m);
					if (e.InvalidRect.IntersectsWith(primaryPaneButtonBounds3))
					{
						Invalidate(primaryPaneButtonBounds3);
					}
				}
				for (int n = 0; n < SecondaryPaneButtons.Count; n++)
				{
					Rectangle secondaryPaneButtonBounds3 = GetSecondaryPaneButtonBounds(n);
					if (e.InvalidRect.IntersectsWith(secondaryPaneButtonBounds3))
					{
						Invalidate(secondaryPaneButtonBounds3);
					}
				}
			}
		}
		finally
		{
			_invalidating = false;
		}
	}

	protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
	{
		base.OnItemClicked(e);
		e.ClickedItem?.Invalidate();
	}

	protected override void OnItemAdded(ToolStripItemEventArgs e)
	{
		base.OnItemAdded(e);
	}

	protected override void OnItemRemoved(ToolStripItemEventArgs e)
	{
		base.OnItemRemoved(e);
	}

	protected override void OnLayout(LayoutEventArgs e)
	{
		if (_primaryPaneButtons != null && _secondaryPaneButtons != null)
		{
			int num = Items.IndexOf(Button1);
			int num2 = Items.IndexOf(Button2);
			List<ToolStripButton> list;
			List<ToolStripButton> list2;
			if (PrimaryPaneButtons.Contains(Button1))
			{
				list = PrimaryPaneButtons;
				list2 = SecondaryPaneButtons;
			}
			else
			{
				list = SecondaryPaneButtons;
				list2 = PrimaryPaneButtons;
			}
			if (num < num2)
			{
				_primaryPaneButtons = list;
				_secondaryPaneButtons = list2;
			}
			else
			{
				_primaryPaneButtons = list2;
				_secondaryPaneButtons = list;
			}
			int num3 = (int)(10f * ControlUtils.GetScaleFactor(this).Width);
			if (!ShowSplitter)
			{
				PrimaryPaneButtons[0].Margin = Padding.Empty;
				for (int i = 1; i < PrimaryPaneButtons.Count; i++)
				{
					PrimaryPaneButtons[i].Margin = Orientation == Orientation.Horizontal ? new Padding(num3 + S_FINASCENT, 0, 0, 0) : new Padding(0, num3 + S_FINASCENT, 0, 0);
				}
				for (int j = 0; j < SecondaryPaneButtons.Count - 1; j++)
				{
					SecondaryPaneButtons[j].Margin = Orientation == Orientation.Horizontal ? new Padding(num3 + S_FINASCENT, 0, 0, 0) : new Padding(0, num3 + S_FINASCENT, 0, 0);
				}
				SecondaryPaneLastVisibleOrDefault.Margin = Orientation == Orientation.Horizontal ? new Padding(num3 + S_FINASCENT, 0, SecondaryPaneLastVisibleOrDefault.Height, 0) : new Padding(0, num3 + S_FINASCENT, 0, SecondaryPaneLastVisibleOrDefault.Width);
			}
			else
			{
				for (int k = 0; k < PrimaryPaneButtons.Count - 1; k++)
				{
					PrimaryPaneButtons[k].Margin = Orientation == Orientation.Horizontal ? new Padding(0, 0, num3 + S_FINASCENT, 0) : new Padding(0, 0, 0, num3 + S_FINASCENT);
				}
				PrimaryPaneLastVisibleButtonOrDefault.Margin = Orientation == Orientation.Horizontal ? new Padding(0, 0, PrimaryPaneLastVisibleButtonOrDefault.Height, 0) : new Padding(0, 0, 0, PrimaryPaneLastVisibleButtonOrDefault.Width);
				for (int l = 0; l < SecondaryPaneButtons.Count - 1; l++)
				{
					SecondaryPaneButtons[l].Margin = Orientation == Orientation.Horizontal ? new Padding(0, 0, num3 + S_FINASCENT, 0) : new Padding(0, 0, 0, num3 + S_FINASCENT);
				}
				SecondaryPaneLastVisibleOrDefault.Margin = Orientation == Orientation.Horizontal ? new Padding(0, 0, SecondaryPaneLastVisibleOrDefault.Height, 0) : new Padding(0, 0, 0, SecondaryPaneLastVisibleOrDefault.Width);
			}
		}
		else
		{
			_primaryPaneButtons = null;
			_secondaryPaneButtons = null;
		}

		base.OnLayout(e);

		UpdateOrientation();

		if (ShowSplitter)
		{
			if (PrimaryPaneButtons != null)
			{
				foreach (ToolStripButton primaryPaneButton in PrimaryPaneButtons)
				{
					if (primaryPaneButton.Placement == ToolStripItemPlacement.Main)
					{
						Point location = Orientation == Orientation.Horizontal ? new Point(primaryPaneButton.Bounds.X, primaryPaneButton.Bounds.Y - 2) : new Point(primaryPaneButton.Bounds.X - 2, primaryPaneButton.Bounds.Y);
						SetItemLocation(primaryPaneButton, location);
					}
				}
			}
			if (SecondaryPaneButtons == null)
			{
				return;
			}
			{
				foreach (ToolStripButton secondaryPaneButton in SecondaryPaneButtons)
				{
					if (secondaryPaneButton.Placement == ToolStripItemPlacement.Main)
					{
						Point location2 = Orientation == Orientation.Horizontal ? new Point(secondaryPaneButton.Bounds.X, secondaryPaneButton.Bounds.Y + 2) : new Point(secondaryPaneButton.Bounds.X + 2, secondaryPaneButton.Bounds.Y);
						SetItemLocation(secondaryPaneButton, location2);
					}
				}
				return;
			}
		}
		if (PrimaryPaneButtons != null)
		{
			foreach (ToolStripButton primaryPaneButton2 in PrimaryPaneButtons)
			{
				if (primaryPaneButton2.Placement == ToolStripItemPlacement.Main)
				{
					Point location3 = Orientation == Orientation.Horizontal ? new Point(primaryPaneButton2.Bounds.X, primaryPaneButton2.Bounds.Y - 2) : new Point(primaryPaneButton2.Bounds.X - 2, primaryPaneButton2.Bounds.Y);
					SetItemLocation(primaryPaneButton2, location3);
				}
			}
		}
		if (SecondaryPaneButtons == null)
		{
			return;
		}

		foreach (ToolStripButton secondaryPaneButton2 in SecondaryPaneButtons)
		{
			Point location4 = Orientation == Orientation.Horizontal ? new Point(secondaryPaneButton2.Bounds.X, secondaryPaneButton2.Bounds.Y - 2) : new Point(secondaryPaneButton2.Bounds.X - 2, secondaryPaneButton2.Bounds.Y);
			SetItemLocation(secondaryPaneButton2, location4);
		}
	}

	protected override void OnLayoutStyleChanged(EventArgs e)
	{
		base.OnLayoutStyleChanged(e);
		if (_lastOrientation != Orientation)
		{
			_lastOrientation = Orientation;
			UpdateMinimumSize();
			UpdateChevronButton();
			SizeF scaleFactor = ControlUtils.GetScaleFactor(this);
			SwapButton.Image = ControlUtils.ScaleImage(Orientation == Orientation.Horizontal ? ControlsResources.swap_horz : ControlsResources.swap_vert, scaleFactor);
		}
		UpdateOrientation();
	}

	protected override void OnLayoutCompleted(EventArgs e)
	{
		base.OnLayoutCompleted(e);
		ToolStripItem toolStripItem = null;
		ToolStripItem toolStripItem2 = null;
		for (int num = Items.Count - 1; num >= 0; num--)
		{
			ToolStripItem toolStripItem3 = Items[num];
			if (toolStripItem3.Visible && toolStripItem3.Placement != ToolStripItemPlacement.None)
			{
				if (toolStripItem2 == null && toolStripItem3.Alignment == ToolStripItemAlignment.Left)
				{
					toolStripItem2 = toolStripItem3;
				}
				else if (toolStripItem == null && toolStripItem3.Alignment == ToolStripItemAlignment.Right)
				{
					toolStripItem = toolStripItem3;
				}
			}
		}
		if (toolStripItem2 != null && toolStripItem != null)
		{
			if (Orientation == Orientation.Horizontal)
			{
				if (toolStripItem.Bounds.X < toolStripItem2.Bounds.X)
				{
					(toolStripItem2, toolStripItem) = (toolStripItem, toolStripItem2);
				}
				_splitterRectangle = Rectangle.FromLTRB(toolStripItem2.Bounds.Right + 1, 0, toolStripItem.Bounds.Left - 1, Height);
			}
			else
			{
				_splitterRectangle = Rectangle.FromLTRB(0, toolStripItem2.Bounds.Bottom + 1, Width, toolStripItem.Bounds.Top);
			}
		}
		else
		{
			_splitterRectangle = Rectangle.Empty;
		}
	}

	protected override void OnMouseMove(MouseEventArgs mea)
	{
		base.OnMouseMove(mea);

		if (_splitterRectangle.Contains(mea.Location))
		{
			Cursor.Current = Orientation == Orientation.Vertical ? Cursors.VSplit : Cursors.HSplit;
		}
		else if (Cursor.Current != Cursors.Default && !Capture)
		{
			Cursor.Current = Cursors.Default;
		}
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		for (int i = 0; i < PrimaryPaneButtons.Count; i++)
		{
			ToolStripButton toolStripButton = PrimaryPaneButtons[i];
			if (IsClickWithinPrimaryTab(i, e) && !toolStripButton.Bounds.Contains(e.Location))
			{
				toolStripButton.PerformClick();
				return;
			}
		}
		for (int j = 0; j < SecondaryPaneButtons.Count; j++)
		{
			ToolStripButton toolStripButton2 = SecondaryPaneButtons[j];
			if (IsClickWithinSecondaryTab(j, e) && !toolStripButton2.Bounds.Contains(e.Location))
			{
				toolStripButton2.PerformClick();
				break;
			}
		}
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		bool showSplitter = ShowSplitter;
		base.OnMouseDoubleClick(e);
		if (!showSplitter || e.Button != MouseButtons.Left)
		{
			return;
		}
		for (int i = 0; i < PrimaryPaneButtons.Count; i++)
		{
			ToolStripButton toolStripButton = PrimaryPaneButtons[i];
			if (IsClickWithinPrimaryTab(i, e) && !toolStripButton.Bounds.Contains(e.Location))
			{
				DesignerXamlButton_DoubleClick(toolStripButton, EventArgs.Empty);
				return;
			}
		}
		for (int j = 0; j < SecondaryPaneButtons.Count; j++)
		{
			ToolStripButton toolStripButton2 = SecondaryPaneButtons[j];
			if (IsClickWithinSecondaryTab(j, e) && !toolStripButton2.Bounds.Contains(e.Location))
			{
				DesignerXamlButton_DoubleClick(toolStripButton2, EventArgs.Empty);
				break;
			}
		}
	}

	private void UpdateChevronButton()
	{
		SizeF scaleFactor = ControlUtils.GetScaleFactor(this);
		if (ShowSplitter)
		{
			ChevronButton.Image = ControlUtils.ScaleImage(Orientation == Orientation.Horizontal ? ControlsResources.horizontal_collapse : ControlsResources.vertical_collapse, scaleFactor);
			ChevronButton.Text = "-";
		}
		else
		{
			ChevronButton.Image = ControlUtils.ScaleImage(Orientation == Orientation.Horizontal ? ControlsResources.horizontal_expand : ControlsResources.vertical_expand, scaleFactor);
			ChevronButton.Text = "+";
		}
	}

	private void UpdateMinimumSize()
	{
		ResetMinimumSize();
		_minDistance = 0;
		_minHeight = 0;
		_minHeight = Math.Max(TextRenderer.MeasureText(Button1.Text, Button1.Font).Height, TextRenderer.MeasureText(Button2.Text, Button2.Font).Height) + 7;
		_minDistance = Orientation == Orientation.Horizontal ? PreferredSize.Width : PreferredSize.Height;
		Size = MinimumSize;
	}

	private void UpdateOrientation()
	{
		if (Orientation == Orientation.Horizontal)
		{
			foreach (ToolStripButton primaryPaneButton in PrimaryPaneButtons)
			{
				primaryPaneButton.TextDirection = ToolStripTextDirection.Horizontal;
				primaryPaneButton.TextImageRelation = TextImageRelation.ImageBeforeText;
			}
			foreach (ToolStripButton secondaryPaneButton in SecondaryPaneButtons)
			{
				secondaryPaneButton.TextDirection = ToolStripTextDirection.Horizontal;
				secondaryPaneButton.TextImageRelation = TextImageRelation.ImageBeforeText;
			}
			SwapButton.Margin = new Padding(2, 2, 2 + SecondaryPaneLastVisibleOrDefault.Height, 2);
		}
		else
		{
			SwapButton.Margin = new Padding(2, 2, 2, 2 + SecondaryPaneLastVisibleOrDefault.Width);
			if (ShowSplitter)
			{
				foreach (ToolStripButton primaryPaneButton2 in PrimaryPaneButtons)
				{
					primaryPaneButton2.TextDirection = ToolStripTextDirection.Vertical90;
					primaryPaneButton2.TextImageRelation = TextImageRelation.ImageAboveText;
				}
				foreach (ToolStripButton secondaryPaneButton2 in SecondaryPaneButtons)
				{
					secondaryPaneButton2.TextDirection = ToolStripTextDirection.Vertical270;
					secondaryPaneButton2.TextImageRelation = TextImageRelation.TextAboveImage;
				}
			}
			else
			{
				foreach (ToolStripButton primaryPaneButton3 in PrimaryPaneButtons)
				{
					primaryPaneButton3.TextDirection = ToolStripTextDirection.Vertical90;
					primaryPaneButton3.TextImageRelation = TextImageRelation.ImageAboveText;
				}
				foreach (ToolStripButton secondaryPaneButton3 in SecondaryPaneButtons)
				{
					secondaryPaneButton3.TextDirection = ToolStripTextDirection.Vertical90;
					secondaryPaneButton3.TextImageRelation = TextImageRelation.ImageAboveText;
				}
			}
		}
		ToolStripItemDisplayStyle displayStyle = Orientation == Orientation.Horizontal ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image;
		foreach (ToolStripButton primaryPaneButton4 in PrimaryPaneButtons)
		{
			primaryPaneButton4.DisplayStyle = displayStyle;
		}
		foreach (ToolStripButton secondaryPaneButton4 in SecondaryPaneButtons)
		{
			secondaryPaneButton4.DisplayStyle = displayStyle;
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == Native.WM_MOUSEACTIVATE)
		{
			DefWndProc(ref m);
		}
		else
		{
			base.WndProc(ref m);
		}
	}
}
