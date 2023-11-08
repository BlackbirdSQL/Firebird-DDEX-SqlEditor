#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Enums;
using BlackbirdSql.Common.Controls.Interfaces;

using Microsoft.Win32;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Grid
{
	public abstract class AbstractGridColumn : IDisposable
	{
		protected int m_myColType;

		protected HorizontalAlignment m_myAlign;

		protected bool m_withRightGridLine;

		protected bool m_withSelectionBk;

		protected SolidBrush m_myBackgroundBrush;

		protected SolidBrush m_myTextBrush;

		protected int m_myWidthInPixels;

		protected int m_myColumnIndex;

		protected EnTextBitmapLayout m_myTextBmpLayout;

		protected bool m_myWidthInChars;

		private int m_origWidthInPixels;

		protected static readonly int s_defaultWidthInPixels;

		protected static bool s_defaultRTL;

		public static readonly int CELL_CONTENT_OFFSET;

		[ThreadStatic]
		private static SolidBrush _SDisabledCellBKBrush;

		[ThreadStatic]
		private static SolidBrush _SDisabledCellForeBrush;

		protected static SolidBrush SDisabledCellBKBrush
		{
			get
			{
				return _SDisabledCellBKBrush ??= new SolidBrush(SystemColors.Control);
			}
			set
			{
				_SDisabledCellBKBrush?.Dispose();

				_SDisabledCellBKBrush = value;
			}
		}

		protected static SolidBrush SDisabledCellForeBrush
		{
			get
			{
				return _SDisabledCellForeBrush ??= new SolidBrush(SystemColors.GrayText);
			}
			set
			{
				_SDisabledCellForeBrush?.Dispose();

				_SDisabledCellForeBrush = value;
			}
		}

		public bool IsWidthInChars => m_myWidthInChars;

		public int WidthInPixels
		{
			get
			{
				return m_myWidthInPixels;
			}
			set
			{
				if (value < 0)
				{
					m_myWidthInPixels = s_defaultWidthInPixels;
				}
				else
				{
					m_myWidthInPixels = value;
				}
			}
		}

		public int ColumnIndex
		{
			get
			{
				return m_myColumnIndex;
			}
			set
			{
				m_myColumnIndex = value;
			}
		}

		public int ColumnType => m_myColType;

		public bool RightGridLine => m_withRightGridLine;

		public SolidBrush BackgroundBrush
		{
			get
			{
				return m_myBackgroundBrush;
			}
			set
			{
				m_myBackgroundBrush = value;
			}
		}

		public SolidBrush TextBrush
		{
			get
			{
				return m_myTextBrush;
			}
			set
			{
				m_myTextBrush = value;
			}
		}

		public HorizontalAlignment TextAlign => m_myAlign;

		public bool WithSelectionBk => m_withSelectionBk;

		public EnTextBitmapLayout TextBitmapLayout => m_myTextBmpLayout;

		public int OrigWidthInPixelsDuringResize
		{
			get
			{
				return m_origWidthInPixels;
			}
			set
			{
				m_origWidthInPixels = value;
			}
		}

		public static Brush DisabledBackgroundBrush => SDisabledCellBKBrush;

		static AbstractGridColumn()
		{
			s_defaultWidthInPixels = 20;
			s_defaultRTL = false;
			CELL_CONTENT_OFFSET = 4;
			SystemEvents.UserPreferenceChanged += OnUserPrefChanged;
		}

		private static void OnUserPrefChanged(object sender, UserPreferenceChangedEventArgs pref)
		{
			SDisabledCellBKBrush = null;
			SDisabledCellForeBrush = null;
		}

		protected AbstractGridColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
		{
			m_myColType = ci.ColumnType;
			m_myAlign = ci.ColumnAlignment;
			m_withRightGridLine = ci.IsWithRightGridLine;
			m_myBackgroundBrush = new SolidBrush(ci.BackgroundColor);
			m_myTextBrush = new SolidBrush(ci.TextColor);
			m_myWidthInPixels = nWidthInPixels >= 0 ? nWidthInPixels : s_defaultWidthInPixels;
			m_myColumnIndex = colIndex;
			m_myTextBmpLayout = ci.TextBmpCellsLayout;
			m_withSelectionBk = ci.IsWithSelectionBackground;
			m_myWidthInChars = ci.WidthType == EnGridColumnWidthType.InAverageFontChar;
		}

		public static void SetDisabledColors(Color bkColor, Color frColor)
		{
			SDisabledCellBKBrush = new SolidBrush(bkColor);
			SDisabledCellForeBrush = new SolidBrush(frColor);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}

		public virtual void DrawCell(Graphics g, Brush bkBrush, Brush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex)
		{
			DrawCell(g, bkBrush, (SolidBrush)textBrush, textFont, rect, storage, nRowIndex);
		}

		public virtual void DrawCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex)
		{
		}

		public virtual void PrintCell(Graphics g, Brush bkBrush, Brush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex)
		{
			PrintCell(g, bkBrush, (SolidBrush)textBrush, textFont, rect, storage, nRowIndex);
		}

		public virtual void PrintCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex)
		{
			DrawCell(g, bkBrush, textBrush, textFont, rect, storage, nRowIndex);
		}

		public virtual void DrawDisabledCell(Graphics g, Font textFont, Rectangle rect, IBGridStorage storage, long nRowIndex)
		{
			DrawCell(g, SDisabledCellBKBrush, SDisabledCellForeBrush, textFont, rect, storage, nRowIndex);
		}

		public virtual void SetRTL(bool bRightToLeft)
		{
		}

		public virtual void ProcessNewGridFont(Font newFont)
		{
		}

		public virtual bool IsPointOverTextInCell(Point pt, Rectangle cellRect, IBGridStorage storage, long row, Graphics g, Font f)
		{
			return true;
		}

		public virtual AccessibleStates GetAccessibleState(long nRowIndex, IBGridStorage storage)
		{
			return AccessibleStates.None;
		}

		public virtual string GetAccessibleValue(long nRowIndex, IBGridStorage storage)
		{
			return "";
		}

		protected AbstractGridColumn()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
			if (m_myBackgroundBrush != null)
			{
				m_myBackgroundBrush.Dispose();
				m_myBackgroundBrush = null;
			}

			if (m_myTextBrush != null)
			{
				m_myTextBrush.Dispose();
				m_myTextBrush = null;
			}
		}
	}
}
