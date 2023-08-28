// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.ResultPane.StatisticsTry

using System;
using System.Collections;

namespace BlackbirdSql.Common.Controls.ResultsPane;


public class StatisticsTry
{
	private readonly IDictionary m_TryData;

	private IDictionaryEnumerator m_TryDataEnumerator;

	private readonly DateTime m_TimeOfExecution;

	public IDictionary TryData => m_TryData;

	public IDictionaryEnumerator TryDataEnumerator
	{
		get
		{
			if (m_TryDataEnumerator == null && m_TryData != null)
			{
				m_TryDataEnumerator = m_TryData.GetEnumerator();
			}
			return m_TryDataEnumerator;
		}
	}

	public DateTime TimeOfExecution => m_TimeOfExecution;

	public StatisticsTry(IDictionary tryData)
	{
		m_TryData = tryData;
		m_TimeOfExecution = DateTime.Now;
	}
}
