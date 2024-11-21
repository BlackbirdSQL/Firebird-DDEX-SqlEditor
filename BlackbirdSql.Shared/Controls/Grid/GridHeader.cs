// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridHeader

using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Properties;



namespace BlackbirdSql.Shared.Controls.Grid;


public sealed class GridHeader : IDisposable
{
	public class HeaderItem
	{
		private readonly EnGridColumnHeaderType m_Type;

		private readonly HorizontalAlignment m_Align;

		private readonly EnTextBitmapLayout m_textBmpLayout;

		private readonly bool m_bResizable;

		private readonly bool m_bMergedWithRight;

		private float m_mergedHeaderResizeProportion;

		private readonly bool m_bClickable;

		private Bitmap m_Bmp;

		private string m_strText;

		private string m_accessibleName;

		private EnGridCheckBoxState m_checkboxState = EnGridCheckBoxState.None;

		private bool m_bPushed;

		public EnGridColumnHeaderType Type => m_Type;

		public HorizontalAlignment Align => m_Align;

		public EnTextBitmapLayout TextBmpLayout => m_textBmpLayout;

		public bool Resizable => m_bResizable;

		public bool Clickable => m_bClickable;

		public bool MergedWithRight => m_bMergedWithRight;

		public float MergedHeaderResizeProportion
		{
			get
			{
				return m_mergedHeaderResizeProportion;
			}
			set
			{
				m_mergedHeaderResizeProportion = value;
			}
		}

		public Bitmap Bmp
		{
			get
			{
				if (m_Type == EnGridColumnHeaderType.CheckBox || m_Type == EnGridColumnHeaderType.TextAndCheckBox)
				{
					return m_checkboxState switch
					{
						EnGridCheckBoxState.Checked => GridConstants.CheckedCheckBoxBitmap,
						EnGridCheckBoxState.Unchecked => GridConstants.UncheckedCheckBoxBitmap,
						EnGridCheckBoxState.Indeterminate => GridConstants.IntermediateCheckBoxBitmap,
						EnGridCheckBoxState.Disabled => GridConstants.DisabledCheckBoxBitmap,
						EnGridCheckBoxState.None => null,
						_ => null,
					};
				}

				return m_Bmp;
			}
			set
			{
				m_Bmp = value;
			}
		}

		public string Text
		{
			get
			{
				return m_strText;
			}
			set
			{
				m_strText = value;
			}
		}

		public string AccessibleName
		{
			get
			{
				return m_accessibleName;
			}
			set
			{
				m_accessibleName = value;
			}
		}

		public EnGridCheckBoxState CheckboxState
		{
			get
			{
				return m_checkboxState;
			}
			set
			{
				m_checkboxState = value;
			}
		}

		public bool Pushed
		{
			get
			{
				return m_bPushed;
			}
			set
			{
				m_bPushed = value;
			}
		}

		public HeaderItem(GridColumnInfo ci)
		{
			m_Type = ci.HeaderType;
			m_Align = ci.HeaderAlignment;
			m_bResizable = ci.IsUserResizable;
			m_bMergedWithRight = ci.IsHeaderMergedWithRight;
			m_textBmpLayout = ci.TextBmpHeaderLayout;
			m_mergedHeaderResizeProportion = ci.MergedHeaderResizeProportion;
			m_bClickable = ci.IsHeaderClickable;
		}

		private HeaderItem()
		{
		}
	}

	public class HeaderItemCollection : CollectionBase
	{
		public HeaderItem this[int index]
		{
			get
			{
				return (HeaderItem)List[index];
			}
			set
			{
				List[index] = value;
			}
		}

		public HeaderItemCollection()
		{
		}

		public HeaderItemCollection(HeaderItemCollection value)
		{
			AddRange(value);
		}

		public HeaderItemCollection(HeaderItem[] value)
		{
			AddRange(value);
		}

		public int Add(HeaderItem node)
		{
			return List.Add(node);
		}

		public void AddRange(HeaderItem[] nodes)
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				Add(nodes[i]);
			}
		}

		public void AddRange(HeaderItemCollection value)
		{
			for (int i = 0; i < value.Count; i++)
			{
				Add(value[i]);
			}
		}

		public bool Contains(HeaderItem node)
		{
			return List.Contains(node);
		}

		public void CopyTo(HeaderItem[] array, int index)
		{
			List.CopyTo(array, index);
		}

		public int IndexOf(HeaderItem node)
		{
			return List.IndexOf(node);
		}

		public void Insert(int index, HeaderItem node)
		{
			List.Insert(index, node);
		}

		public void Remove(HeaderItem node)
		{
			List.Remove(node);
		}

		public void Move(int fromIndex, int toIndex)
		{
			HeaderItem node = this[fromIndex];
			RemoveAt(fromIndex);
			Insert(toIndex, node);
		}
	}

	private readonly HeaderItemCollection m_Items = [];

	private Font m_headerFont;

	private readonly GridButton m_cachedButton = new GridButton();

	public HeaderItem this[int nIndex] => m_Items[nIndex];

	public GridButton HeaderGridButton => m_cachedButton;

	public Font Font
	{
		get
		{
			return m_headerFont;
		}
		set
		{
			m_headerFont = value;
		}
	}

	public void Reset()
	{
		m_Items.Clear();
	}

	public void InsertHeaderItem(int nIndex, GridColumnInfo info)
	{
		HeaderItem node = new HeaderItem(info);
		m_Items.Insert(nIndex, node);
	}

	public void SetHeaderItemInfo(int nIndex, string strText, Bitmap bmp, EnGridCheckBoxState checkboxState)
	{
		HeaderItem headerItem = m_Items[nIndex];
		if (headerItem.MergedWithRight)
		{
			ArgumentException ex = new(ControlsResources.ExShouldBeNoDataForMergedColumHeader, "nIndex");
			Diag.Ex(ex);
			throw ex;
		}

		headerItem.Bmp = bmp;
		headerItem.Text = strText;
		headerItem.CheckboxState = checkboxState;
	}

	public void SetHeaderItemState(int nIndex, bool bPushed)
	{
		HeaderItem headerItem = m_Items[nIndex];
		if (headerItem.MergedWithRight)
		{
			ArgumentException ex = new(ControlsResources.ExCannotSetMergeItemState, "nIndex");
			Diag.Ex(ex);
			throw ex;
		}

		headerItem.Pushed = bPushed;
	}

	public void DeleteItem(int nIndex)
	{
		m_Items.RemoveAt(nIndex);
	}

	public void Move(int fromIndex, int toIndex)
	{
		m_Items.Move(fromIndex, toIndex);
	}

	public void Dispose()
	{
	}
}
