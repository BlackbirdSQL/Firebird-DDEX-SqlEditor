// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Design.Core.Controls.AnimatedStatusStripItem

using System;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl.Config;



namespace BlackbirdSql.Common.Controls.Widgets;


public sealed class AnimatedStatusStripItem : ToolStripStatusLabel
{
	// private static string TName = typeof(AnimatedStatusStripItem).Name;

	private Image[] _Images;

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
				if (value < 0 || value >= _Images.Length)
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

	public void SetImages(Image[] _Images)
	{
		this._Images = _Images;
		CurrentImageIndex = 0;
		parentStatusStrip.Invalidate(invalidateChildren: true);
	}

	public void SetOneImage(Image image)
	{
		StopAnimate();
		_Images = [image];
		CurrentImageIndex = 0;
		parentStatusStrip.Invalidate(invalidateChildren: true);
	}

	public void SetParent(StatusStrip parent)
	{
		parentStatusStrip = parent;
		parentStatusStrip.Font = VsFontColorPreferences.EnvironmentFont;
	}

	public void StartAnimate()
	{
		invalidateRect.Height = parentStatusStrip.Height;
		if (_Images != null && _Images.Length != 0)
		{
			invalidateRect.Width = _Images[0].Width + 4 + 2;
		}
		else
		{
			invalidateRect.Width = Width;
		}
		_Timer.Start();
	}

	public void StopAnimate()
	{
		if (_Timer.Enabled)
			_Timer.Stop();
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
			_Images = null;
		}
		base.Dispose(disposing);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (initializing)
		{
			return;
		}
		Rectangle contentRectangle = ContentRectangle;
		using (SolidBrush brush = new SolidBrush(BackColor))
		{
			e.Graphics.FillRectangle(brush, contentRectangle);
		}
		contentRectangle.Offset(C_BorderOffset, C_BorderOffset);
		contentRectangle.Width -= 4;
		contentRectangle.Height -= 4;
		if (_Images != null)
		{
			Image image = _Images[_CurrentImage];
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
		_CurrentImage = (_CurrentImage + 1) % _Images.Length;
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
