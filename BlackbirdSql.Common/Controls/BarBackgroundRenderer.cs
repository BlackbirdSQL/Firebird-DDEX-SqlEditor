// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.BarBackgroundRenderer
using System;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl;



namespace BlackbirdSql.Common.Controls;

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
		base.RoundedEdges = false;
		base.ColorTable.UseSystemColors = true;
		_Provider = provider;
		_ = _Provider;
	}

	protected override void Initialize(ToolStrip toolStrip)
	{
		base.Initialize(toolStrip);
	}

	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
	{
		_VSCOLOR_FILETAB_GRADIENTHIGHLIGHT = VsColorUtilities.GetShellColor(-213);
		_VSCOLOR_FILETAB_GRADIENTDARK = VsColorUtilities.GetShellColor(-213);
		_VSCOLOR_FILETAB_SELECTEDBACKGROUND = VsColorUtilities.GetShellColor(-202);
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
		using Brush brush = new SolidBrush(VsColorUtilities.GetShellColor(-213));
		graphics.FillRectangle(brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	private void RenderVerticalBackground(ToolStripRenderEventArgs e)
	{
		Graphics graphics = e.Graphics;
		Rectangle rectangle = new Rectangle(Point.Empty, e.ToolStrip.Size);
		using Brush brush = new SolidBrush(VsColorUtilities.GetShellColor(-213));
		graphics.FillRectangle(brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}
}
