// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.BaseMetadataProviderProvider
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using BlackbirdSql.LanguageExtension.Interfaces;
using Microsoft.SqlServer.Management.SqlParser.Binder;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Parser;



namespace BlackbirdSql.LanguageExtension.Model;

[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]


/// <summary>
/// Placeholder. Under development.
/// </summary>
public abstract class AbstractMetadataProviderProvider : IBsMetadataProviderProvider, IDisposable
{
	protected AbstractMetadataProviderProvider()
	{
		_SchemaModelMetadata = null;
		_MetadataBuildingEvent = new ManualResetEvent(initialState: false);
		IsDisposed = false;
		BinderQueue = new BinderQueueImpl();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			(BinderQueue as IDisposable).Dispose();
			ManualResetEvent metadataBuildingEvent = _MetadataBuildingEvent;
			if (metadataBuildingEvent != null)
			{
				metadataBuildingEvent.Dispose();
				_MetadataBuildingEvent = null;
			}
			IsDisposed = true;
		}
	}





	private class BinderQueueImpl : IBsBinderQueue, IDisposable
	{
		internal BinderQueueImpl()
		{
		}

		public void Dispose()
		{
			Dispose(isDisposing: true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool isDisposing)
		{
			if (!isDisposing)
			{
				return;
			}
			lock (_LockLocal)
			{
				for (int i = 0; i < _Actions.Length; i++)
				{
					if (_Actions[i] != null)
					{
						_Actions[i].Item2.Complete(isCompleted: false);
						_Actions[i] = null;
					}
				}
				_JobsToProcessSignal.Set();
				_JobsToProcessSignal.Dispose();
				_IsDisposed = true;
			}
		}




		private enum Priority
		{
			UIThread,
			Bind,
			RecomputeMetadata
		}



		private class BinderAsyncResult : IAsyncResult
		{
			private readonly ManualResetEvent _AsyncWaitHandle = new ManualResetEvent(initialState: false);

			private volatile bool _IsCompleted;

			private volatile object _AsyncState;

			public object AsyncState
			{
				get { return _AsyncState; }
				set { _AsyncState = value; }
			}

			public WaitHandle AsyncWaitHandle => _AsyncWaitHandle;

			public bool CompletedSynchronously => false;

			public bool IsCompleted
			{
				get { return _IsCompleted; }
				private set { _IsCompleted = value; }
			}

			public void Complete(bool isCompleted)
			{
				IsCompleted = isCompleted;
				_AsyncWaitHandle.Set();
			}
		}


		private static readonly int NumPriorities = 3;

		private Tuple<Func<object>, BinderAsyncResult>[] _Actions = new Tuple<Func<object>, BinderAsyncResult>[NumPriorities];

		private bool _IsDisposed;

		private bool _IsThreadProcessingJobs;

		private ManualResetEvent _JobsToProcessSignal = new ManualResetEvent(initialState: false);

		private readonly object _LockLocal = new object();


		public IAsyncResult EnqueueUIThreadAction(Func<object> a)
		{
			return Enqueue(a, Priority.UIThread);
		}

		public IAsyncResult EnqueueBindAction(Func<object> a)
		{
			return Enqueue(a, Priority.Bind);
		}

		public IAsyncResult EnqueueRecomputeMetadataAction(Func<object> a)
		{
			return Enqueue(a, Priority.RecomputeMetadata);
		}

		private IAsyncResult Enqueue(Func<object> a, Priority priority)
		{
			lock (_LockLocal)
			{
				_Actions[(int)priority]?.Item2.Complete(isCompleted: false);
				_Actions[(int)priority] = new Tuple<Func<object>, BinderAsyncResult>(a, new BinderAsyncResult());

				if (!_IsDisposed)
				{
					if (!_IsThreadProcessingJobs)
					{
						_IsThreadProcessingJobs = true;
						new Action(ProcessJobs).BeginInvoke(null, null);
					}
					else
					{
						_JobsToProcessSignal.Set();
					}
				}
				return _Actions[(int)priority].Item2;
			}
		}

		private void ProcessJobs()
		{
			while (true)
			{
				Tuple<Func<object>, BinderAsyncResult> tuple = null;
				lock (_LockLocal)
				{
					if (_IsDisposed)
					{
						_IsThreadProcessingJobs = false;
						break;
					}

					for (int i = 0; i < _Actions.Length; i++)
					{
						if (tuple != null)
						{
							break;
						}
						tuple = _Actions[i];
						_Actions[i] = null;
					}

					if (tuple == null)
					{
						_JobsToProcessSignal.Reset();
					}
				}
				try
				{
					if (tuple != null)
					{
						try
						{
							tuple.Item2.AsyncState = tuple.Item1();
						}
						catch (Exception e)
						{
							Diag.Ex(e);
						}
						tuple.Item2.Complete(isCompleted: true);
					}
					else
					{
						if (_JobsToProcessSignal.WaitOne(5000))
						{
							continue;
						}
						lock (_LockLocal)
						{
							bool flag = false;
							for (int j = 0; j < _Actions.Length; j++)
							{
								if (flag)
								{
									break;
								}
								flag = _Actions[j] != null;
							}
							if (!flag || _IsDisposed)
							{
								_JobsToProcessSignal.Reset();
								_IsThreadProcessingJobs = false;
								break;
							}
						}
						continue;
					}
				}
				catch (ObjectDisposedException)
				{
				}
				catch (Exception e2)
				{
					Diag.Ex(e2);
				}
			}
		}


	}





	private IMetadataProvider _SchemaModelMetadata;

	private ManualResetEvent _MetadataBuildingEvent;

	public IBsBinderQueue BinderQueue { get; private set; }

	protected bool IsDisposed { get; private set; }

	public IMetadataProvider MetadataProvider
	{
		get
		{
			return _SchemaModelMetadata;
		}
		protected set
		{
			_SchemaModelMetadata = value;
		}
	}

	public IBinder Binder { get; protected set; }

	public abstract string Moniker { get; }

	public ManualResetEvent BuildEvent => _MetadataBuildingEvent;

	protected virtual bool AssertInDestructor => true;


	public abstract ParseOptions CreateParseOptions();
}
