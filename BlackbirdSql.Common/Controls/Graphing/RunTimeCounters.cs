// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.RunTimeCounters
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using BlackbirdSql.Common.Controls.Graphing.ComponentModel;
using BlackbirdSql.Common.Properties;

namespace BlackbirdSql.Common.Controls.Graphing;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class RunTimeCounters : ICustomTypeDescriptor
{
	protected struct Counter
	{
		public int Thread;

		public int BrickId;

		public bool BrickIdSpecified;

		public ulong Value;

		public Counter(int thread, ulong value)
		{
			Thread = thread;
			BrickIdSpecified = false;
			BrickId = 0;
			Value = value;
		}

		public Counter(int thread, int brickId, ulong value)
		{
			Thread = thread;
			BrickIdSpecified = true;
			BrickId = brickId;
			Value = value;
		}
	}

	private ulong totalCounters;

	private ulong maxCounter;

	protected List<Counter> counters = new List<Counter>();

	public ulong TotalCounters => totalCounters;

	public ulong MaxCounter => maxCounter;

	public bool DisplayTotalCounters { get; set; }

	public int NumOfCounters => counters.Count;

	public RunTimeCounters()
	{
		maxCounter = 0uL;
		DisplayTotalCounters = true;
	}

	public void AddCounter(int thread, ulong counterValue)
	{
		counters.Add(new Counter(thread, counterValue));
		totalCounters += counterValue;
		if (counterValue > maxCounter)
		{
			maxCounter = counterValue;
		}
	}

	public void AddCounter(int thread, int brickId, ulong counterValue)
	{
		counters.Add(new Counter(thread, brickId, counterValue));
		totalCounters += counterValue;
		if (counterValue > maxCounter)
		{
			maxCounter = counterValue;
		}
	}

	public override string ToString()
	{
		if (DisplayTotalCounters)
		{
			return TotalCounters.ToString(CultureInfo.CurrentCulture);
		}
		return MaxCounter.ToString(CultureInfo.CurrentCulture);
	}

	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return TypeDescriptor.GetAttributes(GetType());
	}

	EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(GetType());
	}

	PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
	{
		return TypeDescriptor.GetDefaultProperty(GetType());
	}

	object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(GetType(), editorBaseType);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		return TypeDescriptor.GetEvents(GetType());
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(GetType(), attributes);
	}

	object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor propertyDescriptor)
	{
		return this;
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		PropertyDescriptor[] array = new PropertyDescriptor[counters.Count];
		string perThreadCounterDescription = ControlsResources.PerThreadCounterDescription;
		if (counters.Count == 1)
		{
			PropertyValue propertyValue = ((!counters[0].BrickIdSpecified) ? new PropertyValue(ControlsResources.RuntimeCounterThreadAll, counters[0].Value) : new PropertyValue(string.Format(ControlsResources.RuntimeCounterThreadOnInstance, counters[0].Thread, counters[0].BrickId), counters[0].Value));
			propertyValue.SetDisplayNameAndDescription(propertyValue.Name, perThreadCounterDescription);
			array[0] = propertyValue;
		}
		else
		{
			for (int i = 0; i < counters.Count; i++)
			{
				PropertyValue propertyValue2 = ((!counters[i].BrickIdSpecified) ? new PropertyValue(string.Format(ControlsResources.RuntimeCounterThread, counters[i].Thread), counters[i].Value) : new PropertyValue(string.Format(ControlsResources.RuntimeCounterThreadOnInstance, counters[i].Thread, counters[i].BrickId), counters[i].Value));
				propertyValue2.SetDisplayNameAndDescription(propertyValue2.Name, perThreadCounterDescription);
				array[i] = propertyValue2;
			}
		}
		return new PropertyDescriptorCollection(array);
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		return ((ICustomTypeDescriptor)this).GetProperties();
	}

	string ICustomTypeDescriptor.GetComponentName()
	{
		return null;
	}

	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		return TypeDescriptor.GetConverter(GetType());
	}

	string ICustomTypeDescriptor.GetClassName()
	{
		return GetType().Name;
	}
}
