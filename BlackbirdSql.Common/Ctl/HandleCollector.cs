// Microsoft.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Common.Win32.HandleCollector
using System;
using System.Threading;
using BlackbirdSql.Common.Ctl.Events;



namespace BlackbirdSql.Common.Ctl;

public sealed class HandleCollector
{
	private class HandleType
	{
		public readonly string name;

		private readonly int initialThreshHold;

		private int threshHold;

		private int handleCount;

		private readonly int deltaPercent;

		public HandleType(string name, int expense, int initialThreshHold)
		{
			this.name = name;
			this.initialThreshHold = initialThreshHold;
			threshHold = initialThreshHold;
			deltaPercent = 100 - expense;
		}

		public void Add(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				return;
			}
			bool flag = false;
			int currentHandleCount = 0;
			lock (this)
			{
				handleCount++;
				flag = NeedCollection();
				currentHandleCount = handleCount;
			}
			lock (_LockClass)
			{
				HandleAddedEvent?.Invoke(name, handle, currentHandleCount);
			}
			if (flag && flag)
			{
				GC.Collect();
				Thread.Sleep((100 - deltaPercent) / 4);
			}
		}

		public int GetHandleCount()
		{
			lock (this)
			{
				return handleCount;
			}
		}

		public bool NeedCollection()
		{
			if (suspendCount > 0)
			{
				return false;
			}
			if (handleCount > threshHold)
			{
				threshHold = handleCount + handleCount * deltaPercent / 100;
				return true;
			}
			int num = 100 * threshHold / (100 + deltaPercent);
			if (num >= initialThreshHold && handleCount < (int)((float)num * 0.9f))
			{
				threshHold = num;
			}
			return false;
		}

		public IntPtr Remove(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				return handle;
			}
			int currentHandleCount = 0;
			lock (this)
			{
				handleCount--;
				if (handleCount < 0)
				{
					handleCount = 0;
				}
				currentHandleCount = handleCount;
			}
			lock (_LockClass)
			{
				HandleRemovedEvent?.Invoke(name, handle, currentHandleCount);
			}
			return handle;
		}
	}

	private static HandleType[] handleTypes;

	private static int handleTypeCount;

	private static int suspendCount;

	// A static class lock
	private static readonly object _LockClass = new object();

	public static event HandleChangeEventHandler HandleAddedEvent;

	public static event HandleChangeEventHandler HandleRemovedEvent;

	public static IntPtr Add(IntPtr handle, int type)
	{
		handleTypes[type - 1].Add(handle);
		return handle;
	}

	public static void SuspendCollect()
	{
		lock (_LockClass)
		{
			suspendCount++;
		}
	}

	public static void ResumeCollect()
	{
		bool flag = false;
		lock (_LockClass)
		{
			if (suspendCount > 0)
			{
				suspendCount--;
			}
			if (suspendCount == 0)
			{
				for (int i = 0; i < handleTypeCount; i++)
				{
					lock (handleTypes[i])
					{
						if (handleTypes[i].NeedCollection())
						{
							flag = true;
						}
					}
				}
			}
		}
		if (flag)
		{
			GC.Collect();
		}
	}

	public static int RegisterType(string typeName, int expense, int initialThreshold)
	{
		lock (_LockClass)
		{
			if (handleTypeCount == 0 || handleTypeCount == handleTypes.Length)
			{
				HandleType[] destinationArray = new HandleType[handleTypeCount + 10];
				if (handleTypes != null)
				{
					Array.Copy(handleTypes, 0, destinationArray, 0, handleTypeCount);
				}
				handleTypes = destinationArray;
			}
			handleTypes[handleTypeCount++] = new HandleType(typeName, expense, initialThreshold);
			return handleTypeCount;
		}
	}

	public static IntPtr Remove(IntPtr handle, int type)
	{
		return handleTypes[type - 1].Remove(handle);
	}
}
