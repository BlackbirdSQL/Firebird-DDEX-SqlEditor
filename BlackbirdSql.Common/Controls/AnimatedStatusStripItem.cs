// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Design.Core.Controls.AnimatedStatusStripItem
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl.Config;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.Common.Controls;

public sealed class AnimatedStatusStripItem : ToolStripStatusLabel
{
	// private static string TName = typeof(AnimatedStatusStripItem).Name;

	private Image[] images;

	private readonly int _Interval;

	private Timer _Timer;

	private int _CurrentImage;

	private StatusStrip parentStatusStrip;

	private Rectangle invalidateRect = new Rectangle(0, 0, 0, 0);

	private bool initializing;

	private const int C_TextImageGap = 2;

	private const int C_BorderOffset = 2;

	private readonly VsFontColorPreferences _VsFontColorPreferences;

	private int CurrentImageIndex
	{
		set
		{
			if (value != _CurrentImage)
			{
				if (value < 0 || value >= images.Length)
				{
					value = 0;
				}
				_CurrentImage = value;
				parentStatusStrip.Invalidate(invalidateChildren: true);
			}
		}
	}

	public override string Text
	{
		get
		{
			return base.Text;
		}
		set
		{
			base.Text = value;
			ToolTipText = value;
		}
	}

	public AnimatedStatusStripItem(int timeInterval)
	{
		_Interval = timeInterval;
		_Timer = new Timer
		{
			Interval = _Interval
		};
		_Timer.Tick += ChangeImage;
		_CurrentImage = 0;
		AutoToolTip = false;
		_VsFontColorPreferences = new VsFontColorPreferences();
		Font = VsFontColorPreferences.EnvironmentFont;
		_VsFontColorPreferences.PreferencesChangedEvent += VsFontColorPreferences_PreferencesChanged;
	}

	public void SetImages(Image[] images)
	{
		SqlTracer.TraceEvent(TraceEventType.Information, EnSqlTraceId.VSShell, "AnimatedStatusStripItem.SetImages");
		this.images = images;
		CurrentImageIndex = 0;
		parentStatusStrip.Invalidate(invalidateChildren: true);
	}

	public void SetOneImage(Image image)
	{
		SqlTracer.TraceEvent(TraceEventType.Information, EnSqlTraceId.VSShell, "AnimatedStatusStripItem.SetOneImage");
		StopAnimate();
		images = new Image[1] { image };
		CurrentImageIndex = 0;
		parentStatusStrip.Invalidate(invalidateChildren: true);
	}

	public void SetParent(StatusStrip parent)
	{
		SqlTracer.TraceEvent(TraceEventType.Information, EnSqlTraceId.VSShell, "AnimatedStatusStripItem.SetParent");
		parentStatusStrip = parent;
		parentStatusStrip.Font = VsFontColorPreferences.EnvironmentFont;
	}

	public void StartAnimate()
	{
		invalidateRect.Height = parentStatusStrip.Height;
		if (images != null && images.Length != 0)
		{
			invalidateRect.Width = images[0].Width + 4 + 2;
		}
		else
		{
			invalidateRect.Width = base.Width;
		}
		_Timer.Start();
	}

	public void StopAnimate()
	{
		SqlTracer.TraceEvent(TraceEventType.Information, EnSqlTraceId.VSShell, "AnimatedStatusStripItem.StopAnimate");
		if (_Timer.Enabled)
		{
			_Timer.Stop();
		}
	}

	public void BeginInit()
	{
		initializing = true;
	}

	public void EndInit()
	{
		initializing = false;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			SqlTracer.TraceEvent(TraceEventType.Information, EnSqlTraceId.VSShell, "AnimatedStatusStripItem.Dispose");
			if (_Timer != null)
			{
				_Timer.Dispose();
				_Timer = null;
			}
			if (parentStatusStrip != null)
			{
				parentStatusStrip = null;
			}
			if (_VsFontColorPreferences != null)
			{
				_VsFontColorPreferences.PreferencesChangedEvent -= VsFontColorPreferences_PreferencesChanged;
				_VsFontColorPreferences.Dispose();
			}
			images = null;
		}
		base.Dispose(disposing);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (initializing)
		{
			return;
		}
		Rectangle contentRectangle = base.ContentRectangle;
		using (SolidBrush brush = new SolidBrush(BackColor))
		{
			e.Graphics.FillRectangle(brush, contentRectangle);
		}
		contentRectangle.Offset(C_BorderOffset, C_BorderOffset);
		contentRectangle.Width -= 4;
		contentRectangle.Height -= 4;
		if (images != null)
		{
			Image image = images[_CurrentImage];
			if (image != null)
			{
				int y = (contentRectangle.Height - image.Size.Height) / 2 + contentRectangle.Top;
				Rectangle rect = new Rectangle(contentRectangle.Left, y, image.Width, image.Height);
				contentRectangle.Offset(rect.Width + C_TextImageGap, 0);
				contentRectangle.Width -= rect.Width + C_TextImageGap;
				e.Graphics.DrawImage(image, rect);
			}
		}
		using SolidBrush brush2 = new SolidBrush(ForeColor);
		using StringFormat stringFormat = new StringFormat();
		stringFormat.Alignment = StringAlignment.Near;
		stringFormat.LineAlignment = StringAlignment.Center;
		stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
		if (parentStatusStrip.RightToLeft == RightToLeft.Yes)
		{
			stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}
		stringFormat.Trimming = StringTrimming.EllipsisCharacter;
		e.Graphics.DrawString(Text, parentStatusStrip.Font, brush2, contentRectangle, stringFormat);
	}

	private void ChangeImage(object sender, EventArgs e)
	{
		_CurrentImage = (_CurrentImage + 1) % images.Length;
		parentStatusStrip.Invalidate(invalidateRect, invalidateChildren: true);
	}

	private void VsFontColorPreferences_PreferencesChanged(object sender, EventArgs args)
	{
		if (parentStatusStrip != null)
		{
			parentStatusStrip.Font = VsFontColorPreferences.EnvironmentFont;
		}
		Font = VsFontColorPreferences.EnvironmentFont;
	}
}
