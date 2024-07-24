// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridConstants

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;



namespace BlackbirdSql.Shared.Controls.Grid;


public sealed class GridConstants
{
	public const int C_StandardCheckBoxSize = 13;

	[ThreadStatic]
	private static Bitmap SCheckedBitmap;

	[ThreadStatic]
	private static Bitmap SUncheckedBitmap;

	[ThreadStatic]
	private static Bitmap SIntermediateBitmap;

	[ThreadStatic]
	private static Bitmap SDisabledBitmap;

	public static int ActualCheckBoxSize => Convert.ToInt32(ScaleFactor * 13f);

	public static float ScaleFactor { get; set; }

	public static Bitmap CheckedCheckBoxBitmap
	{
		get
		{
			if (SCheckedBitmap == null)
			{
				SCheckedBitmap = new Bitmap(ActualCheckBoxSize, ActualCheckBoxSize);
				GetStdBitmap(SCheckedBitmap, ButtonState.Checked);
			}

			return SCheckedBitmap;
		}
	}

	public static Bitmap UncheckedCheckBoxBitmap
	{
		get
		{
			if (SUncheckedBitmap == null)
			{
				SUncheckedBitmap = new Bitmap(ActualCheckBoxSize, ActualCheckBoxSize);
				GetStdBitmap(SUncheckedBitmap, ButtonState.Normal);
			}

			return SUncheckedBitmap;
		}
	}

	public static Bitmap IntermediateCheckBoxBitmap
	{
		get
		{
			if (SIntermediateBitmap == null)
			{
				SIntermediateBitmap = new Bitmap(ActualCheckBoxSize, ActualCheckBoxSize);
				GetIntermidiateCheckboxBitmap(SIntermediateBitmap);
			}

			return SIntermediateBitmap;
		}
	}

	public static Bitmap DisabledCheckBoxBitmap
	{
		get
		{
			if (SDisabledBitmap == null)
			{
				SDisabledBitmap = new Bitmap(ActualCheckBoxSize, ActualCheckBoxSize);
				GetStdBitmap(SDisabledBitmap, ButtonState.Inactive);
			}

			return SDisabledBitmap;
		}
	}

	public static TextFormatFlags DefaultTextFormatFlags => TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.WordEllipsis | TextFormatFlags.PreserveGraphicsClipping;

	static GridConstants()
	{
		ScaleFactor = 1f;
	}

	public static void RegenerateCheckBoxBitmaps()
	{
		_ = CheckedCheckBoxBitmap;
		_ = UncheckedCheckBoxBitmap;
		_ = IntermediateCheckBoxBitmap;
		_ = DisabledCheckBoxBitmap;
	}

	private static void GetIntermidiateCheckboxBitmap(Bitmap bmp)
	{
		Rectangle rectangle = new Rectangle(0, 0, ActualCheckBoxSize, ActualCheckBoxSize);
		using Graphics graphics = Graphics.FromImage(bmp);
		graphics.Clear(Color.Transparent);

		if (Application.RenderWithVisualStyles)
		{
			VisualStyleElement checkBox = DrawManager.GetCheckBox(ButtonState.Flat);

			if (checkBox != null && VisualStyleRenderer.IsElementDefined(checkBox))
			{
				new VisualStyleRenderer(checkBox).DrawBackground(graphics, rectangle);
				return;
			}
		}

		ControlPaint.DrawMixedCheckBox(graphics, rectangle, ButtonState.Checked);
	}


	private static void GetStdBitmap(Bitmap bmp, ButtonState state)
	{
		Rectangle rectangle = new Rectangle(0, 0, ActualCheckBoxSize, ActualCheckBoxSize);
		using Graphics graphics = Graphics.FromImage(bmp);
		graphics.Clear(Color.Transparent);

		if (Application.RenderWithVisualStyles)
		{

			VisualStyleElement checkBox = DrawManager.GetCheckBox(state);

			if (checkBox != null && VisualStyleRenderer.IsElementDefined(checkBox))
			{
				new VisualStyleRenderer(checkBox).DrawBackground(graphics, rectangle);
				return;
			}
		}

		ControlPaint.DrawCheckBox(graphics, rectangle, state);
	}



	public static void AdjustFormatFlagsForAlignment(ref TextFormatFlags inputFlags, HorizontalAlignment ha)
	{
		switch (ha)
		{
			case HorizontalAlignment.Left:
				inputFlags &= ~TextFormatFlags.Right;
				inputFlags &= ~TextFormatFlags.HorizontalCenter;
				inputFlags |= TextFormatFlags.Default;
				break;
			case HorizontalAlignment.Center:
				inputFlags &= ~TextFormatFlags.Right;
				inputFlags &= (TextFormatFlags)(-1);
				inputFlags |= TextFormatFlags.HorizontalCenter;
				break;
			case HorizontalAlignment.Right:
				inputFlags &= (TextFormatFlags)(-1);
				inputFlags &= ~TextFormatFlags.HorizontalCenter;
				inputFlags |= TextFormatFlags.Right;
				break;
		}
	}
}
