// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.BarBackgroundRenderer
using System;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Common.Controls.Widgets;

public class BarBackgroundRenderer : ToolStripProfessionalRenderer
{
	private readonly IServiceProvider _Provider;

	private Color _VSCOLOR_FILETAB_GRADIENTHIGHLIGHT = SystemColors.Window;

	private Color _VSCOLOR_FILETAB_GRADIENTDARK = SystemColors.Control;

	private Color _VSCOLOR_FILETAB_SELECTEDBACKGROUND = SystemColors.Window;

	protected Color VSCOLOR_FILETAB_GRADIENTHIGHLIGHT => _VSCOLOR_FILETAB_GRADIENTHIGHLIGHT;

	protected Color VSCOLOR_FILETAB_GRADIENTDARK => _VSCOLOR_FILETAB_GRADIENTDARK;

	protected Color VSCOLOR_FILETAB_SELECTEDBACKGROUND => _VSCOLOR_FILETAB_SELECTEDBACKGROUND;

	protected Color HighContrastBackground => SystemColors.ControlDark;

	public BarBackgroundRenderer(IServiceProvider provider)
	{
		RoundedEdges = false;
		ColorTable.UseSystemColors = true;
		_Provider = provider;
		_ = _Provider;
	}

	protected override void Initialize(ToolStrip toolStrip)
	{
		base.Initialize(toolStrip);
	}

	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
	{
		_VSCOLOR_FILETAB_GRADIENTHIGHLIGHT = VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_THREEDFACE);
		_VSCOLOR_FILETAB_GRADIENTDARK = VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_THREEDFACE);
		_VSCOLOR_FILETAB_SELECTEDBACKGROUND = VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_HIGHLIGHT);
		if (e.ToolStrip is ToolStripDropDown)
		{
			base.OnRenderToolStripBackground(e);
		}
		else if (e.ToolStrip.Orientation == Orientation.Horizontal)
		{
			RenderHorizontalBackground(e);
		}
		else
		{
			RenderVerticalBackground(e);
		}
	}

	protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
	{
		if (e.ToolStrip is ToolStripDropDown)
		{
			base.OnRenderToolStripBorder(e);
		}
	}

	private void RenderHorizontalBackground(ToolStripRenderEventArgs e)
	{
		Graphics graphics = e.Graphics;
		Rectangle rectangle = new Rectangle(Point.Empty, e.ToolStrip.Size);
		using Brush brush = new SolidBrush(VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_THREEDFACE));
		graphics.FillRectangle(brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	private void RenderVerticalBackground(ToolStripRenderEventArgs e)
	{
		Graphics graphics = e.Graphics;
		Rectangle rectangle = new Rectangle(Point.Empty, e.ToolStrip.Size);
		using Brush brush = new SolidBrush(VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_THREEDFACE));
		graphics.FillRectangle(brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}
}
