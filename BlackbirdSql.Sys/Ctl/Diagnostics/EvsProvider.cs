
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using BlackbirdSql.Sys.Ctl.Config;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using Microsoft.VisualStudio;



namespace BlackbirdSql.Sys.Ctl.Diagnostics;


/// <summary>
/// To be completed.
/// Common EventSource base class. The class family uses a type parameter/specifier of the
/// final descendent class for creating the object so that we can have a separate singleton
/// instance for each assembly using minimal implementation code in the final class.
/// Inclusion of an interface, <see cref="IBsEventSource"/>, is redundant and purely for brevity.
/// </summary>
public class EvsProvider<TEventSource> : EventSource where TEventSource : EvsProvider<TEventSource>
{
	protected EvsProvider()
	{
		_DummyCounter = new EventCounter(nameof(_DummyCounter), this);
	}


	/// <summary>
	/// Creates a new instance of the EventSource class.
	/// </summary>
	public static TEventSource CreateInstance()
	{
		return (TEventSource)Activator.CreateInstance(typeof(TEventSource));
	}

	private readonly EventCounter _DummyCounter;

	private List<EventCounter> _OpCounters = null;
	private List<EventCounter> _TelemCounters = null;
	private List<long> _TelemCardinals = null;
	private List<long> _ElapsedTimes = null;
	private List<long> _StartTimes = null;
	private Stopwatch _ElapsedTimer = null;

	private static TEventSource _Log = null;

	public static TEventSource Log => _Log ??= CreateInstance();

	private List<EventCounter> OpCounters => _OpCounters ??= [];
	private List<EventCounter> TelemCounters => _TelemCounters ??= [];
	protected List<long> TelemCardinals => _TelemCardinals ??= [];
	private List<long> ElapsedTimes => _ElapsedTimes ??= [];
	private List<long> StartTimes => _StartTimes ??= [];
	private Stopwatch ElapsedTimer => _ElapsedTimer ??= Stopwatch.StartNew();



	[NonEvent]
	private int AddCounter(string name)
	{
		OpCounters.Add(new(name, this));
		TelemCounters.Add(new("Telem_" + name, this));
		TelemCardinals.Add(0L);
		ElapsedTimes.Add(0L);
		StartTimes.Add(0L);

		return OpCounters.Count - 1;
	}



	[NonEvent]
	protected static bool CanTrace(EnEventLevel level) =>
		((int)level & (int)PersistentSettings.SourceLevel) > 0;



	[Event((int)EnEvsId.Critical, Level = EventLevel.Critical)]
	protected int CriticalEvent(string msg)
	{
		WriteEvent((int)EnEvsId.Critical, msg);

		return VSConstants.S_OK;
	}



	[Event((int)EnEvsId.Error, Level = EventLevel.Error)]
	protected int ErrorEvent(string msg)
	{
		WriteEvent((int)EnEvsId.Error, msg);

		return VSConstants.S_OK;
	}



	[Event((int)EnEvsId.Warning, Level = EventLevel.Warning)]
	protected int WarnEvent(string msg)
	{
		WriteEvent((int)EnEvsId.Warning, msg);

		return VSConstants.S_OK;
	}



	[Event((int)EnEvsId.Information, Level = EventLevel.Informational)]
	protected int InfoEvent(string msg)
	{
		WriteEvent((int)EnEvsId.Information, msg);

		return VSConstants.S_OK;
	}



	[Event((int)EnEvsId.Trace, Level = EventLevel.Verbose)]
	protected int TraceEvent(string msg)
	{
		WriteEvent((int)EnEvsId.Trace, msg);

		return VSConstants.S_OK;
	}



	[Event((int)EnEvsId.Debug, Level = EventLevel.Verbose)]
	protected int DebugEvent(string msg)
	{
		WriteEvent((int)EnEvsId.Debug, msg);

		return VSConstants.S_OK;
	}



	/// <summary>
	/// Caller is responsible for retaining it's index.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="index"></param>
	/// <returns></returns>
	[Event((int)EnEvsId.OpStart, Level = EventLevel.Informational, Opcode = EventOpcode.Start)]
	protected int StartEvent(string name, int index, string msg)
	{
		if (index < 0)
			index = AddCounter(name);

		long elapsed = ElapsedTimer.ElapsedMilliseconds;

		OpCounters[index].WriteMetric(DateTime.Now.Millisecond);
		
		ElapsedTimes[index] = 0L;
		StartTimes[index] = elapsed;

		WriteEvent((int)EnEvsId.OpStart, name);

		return index;
	}



	[Event((int)EnEvsId.OpStop, Level = EventLevel.Informational, Opcode = EventOpcode.Stop)]
	protected int StopEvent(string name, int index, string msg)
	{
		long elapsed = ElapsedTimer.ElapsedMilliseconds;
		long duration = elapsed - StartTimes[index] + ElapsedTimes[index];

		OpCounters[index].WriteMetric(DateTime.Now.Millisecond);
		TelemCounters[index].WriteMetric(duration);
		TelemCardinals[index] = TelemCardinals[index] + 1;

		WriteEvent((int)EnEvsId.OpStop, name);

		return index;
	}



	[Event((int)EnEvsId.OpResume, Level = EventLevel.Informational, Opcode = EventOpcode.Resume)]
	protected int ResumeEvent(string name, int index, string msg)
	{
		StartTimes[index] = ElapsedTimer.ElapsedMilliseconds;

		WriteEvent((int)EnEvsId.OpResume, name);

		return index;
	}



	[Event((int)EnEvsId.OpSuspend, Level = EventLevel.Informational, Opcode = EventOpcode.Suspend)]
	protected int SuspendEvent(string name, int index, string msg)
	{
		long elapsed = ElapsedTimer.ElapsedMilliseconds;

		ElapsedTimes[index] += elapsed - StartTimes[index];

		WriteEvent((int)EnEvsId.OpSuspend, name);

		return index;
	}

}