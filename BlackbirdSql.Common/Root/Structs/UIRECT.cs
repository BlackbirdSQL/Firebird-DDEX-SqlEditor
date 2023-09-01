// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.PlatformUI.RECT

using System;
using System.Runtime.InteropServices;
using System.Windows;

using Microsoft.VisualStudio.OLE.Interop;




namespace BlackbirdSql.Common.Structs
{
	[Serializable]
	public struct UIRECT
	{
		// Microsoft.SqlServer.ConnectionDlg.UI.WPF.PlatformUI.RECT

		[ComAliasName("Microsoft.VisualStudio.OLE.Interop.LONG")]
		public int Left;

		[ComAliasName("Microsoft.VisualStudio.OLE.Interop.LONG")]
		public int Top;

		[ComAliasName("Microsoft.VisualStudio.OLE.Interop.LONG")]
		public int Right;

		[ComAliasName("Microsoft.VisualStudio.OLE.Interop.LONG")]
		public int Bottom;

		public readonly Point Position => new Point(Left, Top);

		public Size Size => new Size(Width, Height);

		public int Height
		{
			readonly get
			{
				return Bottom - Top;
			}
			set
			{
				Bottom = Top + value;
			}
		}

		public int Width
		{
#pragma warning disable IDE0251 // Make member 'readonly'
			get
#pragma warning restore IDE0251 // Make member 'readonly'
			{
				return Right - Left;
			}
			set
			{
				Right = Left + value;
			}
		}

		public UIRECT(int left, int top, int right, int bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public UIRECT(Rect rect)
		{
			Left = (int)rect.Left;
			Top = (int)rect.Top;
			Right = (int)rect.Right;
			Bottom = (int)rect.Bottom;
		}

		public UIRECT(RECT rect)
		{
			Left = rect.left;
			Top = rect.top;
			Right = rect.right;
			Bottom = rect.bottom;
		}

		public void Offset(int dx, int dy)
		{
			Left += dx;
			Right += dx;
			Top += dy;
			Bottom += dy;
		}

		public Int32Rect ToInt32Rect()
		{
			return new Int32Rect(Left, Top, Width, Height);
		}
	}
}
