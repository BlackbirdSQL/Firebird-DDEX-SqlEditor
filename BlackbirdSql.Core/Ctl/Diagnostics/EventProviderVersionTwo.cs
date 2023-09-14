// Microsoft.Data.Tools.Components, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Components.Diagnostics.EventProviderVersionTwo
using System;
using System.Diagnostics.Eventing;
using System.Runtime.InteropServices;

namespace BlackbirdSql.Core.Ctl.Diagnostics;

internal class EventProviderVersionTwo : EventProvider
{
	[StructLayout(LayoutKind.Explicit, Size = 16)]
	private struct EventData
	{
		[FieldOffset(0)]
		internal ulong DataPointer;

		[FieldOffset(8)]
		internal uint Size;

		[FieldOffset(12)]
		internal int Reserved;
	}

	internal EventProviderVersionTwo(Guid id)
		: base(id)
	{
	}

	internal unsafe bool TemplateGenericBeginEndMessage(ref EventDescriptor eventDescriptor, bool IsStart, string EventContext)
	{
		int num = 2;
		bool result = true;
		if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
		{
			byte* ptr = stackalloc byte[(int)(uint)(sizeof(EventData) * num)];
			EventData* ptr2 = (EventData*)ptr;
			int num2 = IsStart ? 1 : 0;
			ptr2->DataPointer = (ulong)&num2;
			ptr2->Size = 4u;
			ptr2[1].Size = (uint)(((EventContext ?? string.Empty).Length + 1) * 2);
			fixed (char* ptr3 = EventContext ?? string.Empty)
			{
				ptr2[1].DataPointer = (ulong)ptr3;
				result = WriteEvent(ref eventDescriptor, num, (IntPtr)ptr);
			}
		}
		return result;
	}

	internal unsafe bool TemplateEmptyBeginEndMessage(ref EventDescriptor eventDescriptor, bool IsStart)
	{
		int num = 1;
		bool result = true;
		if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
		{
			byte* ptr = stackalloc byte[(int)(uint)(sizeof(EventData) * num)];
			EventData* ptr2 = (EventData*)ptr;
			int num2 = IsStart ? 1 : 0;
			ptr2->DataPointer = (ulong)&num2;
			ptr2->Size = 4u;
			result = WriteEvent(ref eventDescriptor, num, (IntPtr)ptr);
		}
		return result;
	}

	internal unsafe bool TemplateLoggingMessage(ref EventDescriptor eventDescriptor, uint traceId, string message)
	{
		int num = 2;
		bool result = true;
		if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
		{
			byte* ptr = stackalloc byte[(int)(uint)(sizeof(EventData) * num)];
			EventData* ptr2 = (EventData*)ptr;
			ptr2->DataPointer = (ulong)&traceId;
			ptr2->Size = 4u;
			ptr2[1].Size = (uint)(((message ?? string.Empty).Length + 1) * 2);
			fixed (char* ptr3 = message ?? string.Empty)
			{
				ptr2[1].DataPointer = (ulong)ptr3;
				result = WriteEvent(ref eventDescriptor, num, (IntPtr)ptr);
			}
		}
		return result;
	}

	internal bool TemplateEventDescriptor(ref EventDescriptor eventDescriptor)
	{
		if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
		{
			return WriteEvent(ref eventDescriptor, 0, IntPtr.Zero);
		}
		return true;
	}
}
