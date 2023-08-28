#region Assembly Microsoft.Data.Tools.Utilities, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\Microsoft.Data.Tools.Utilities.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Diagnostics.Eventing;
using System.Runtime.InteropServices;

// using Microsoft.Data.Tools.Schema.Common.Diagnostics;
// using Ns = Microsoft.Data.Tools.Schema.Common.Diagnostics;


// namespace Microsoft.Data.Tools.Schema.Common.Diagnostics
namespace BlackbirdSql.Core.Diagnostics
{
	public class SqlEventProvider : EventProvider
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

		internal SqlEventProvider(Guid id)
			: base(id)
		{
		}

		internal bool TemplateEventDescriptor(ref EventDescriptor eventDescriptor)
		{
			if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				return WriteEvent(ref eventDescriptor, 0, IntPtr.Zero);
			}

			return true;
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

		internal unsafe bool TemplatePopulatorMessage(ref EventDescriptor eventDescriptor, bool IsStart, string PopulatorName, int numberOfElements)
		{
			int num = 3;
			bool result = true;
			if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* ptr = stackalloc byte[(int)(uint)(sizeof(EventData) * num)];
				EventData* ptr2 = (EventData*)ptr;
				int num2 = IsStart ? 1 : 0;
				ptr2->DataPointer = (ulong)&num2;
				ptr2->Size = 4u;
				ptr2[1].Size = (uint)(((PopulatorName ?? string.Empty).Length + 1) * 2);
				ptr2[2].DataPointer = (ulong)&numberOfElements;
				ptr2[2].Size = 4u;
				fixed (char* ptr3 = PopulatorName ?? string.Empty)
				{
					ptr2[1].DataPointer = (ulong)ptr3;
					result = WriteEvent(ref eventDescriptor, num, (IntPtr)ptr);
				}
			}

			return result;
		}
	}
}
