// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.DatabaseChangesManager
// Split into two for brevity.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Shared.Ctl;


// =========================================================================================================
//
//										AbstractDesignerServices Class
//
/// <summary>
/// Base class service for handling open query commands.
/// </summary>
// =========================================================================================================
public abstract class AbstractDesignerServices
{

	// ----------------------------------------------------------
	#region Constructors / Destructors - AbstractDesignerServices
	// ----------------------------------------------------------


	public AbstractDesignerServices()
	{
	}


	#endregion Constructors / Destructors





	// =====================================================================================================
	#region Fields - AbstractDesignerServices
	// =====================================================================================================


	private static event EventHandler<BeforeOpenDocumentEventArgs> _S_BeforeOpenDocumentEvent;


	#endregion Fields





	// =====================================================================================================
	#region Property accessors - AbstractDesignerServices
	// =====================================================================================================


	public static event EventHandler<BeforeOpenDocumentEventArgs> SBeforeOpenDocumentEvent
	{
		add { _S_BeforeOpenDocumentEvent += value; }
		remove { _S_BeforeOpenDocumentEvent -= value; }
	}


	protected static EventHandler<BeforeOpenDocumentEventArgs> S_BeforeOpenDocumentHandler => _S_BeforeOpenDocumentEvent;


	#endregion Property accessors





	// =====================================================================================================
	#region Methods - AbstractDesignerServices
	// =====================================================================================================


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);



	// Microsoft.Data.Tools.Utilities, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.Data.Tools.Schema.Common.Threading.EventMarshaler:Invoke
	protected static void ExecuteDocumentLoadedCallback(Delegate method, params object[] args)
	{
		if (method == null)
			return;

		try
		{
			method.DynamicInvoke(args);
		}
		catch (MemberAccessException ex)
		{
			Diag.Dug(ex);
			Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "EventMarshaler.Invoke(...): Could not access target member {0}", ex);
			throw;
		}
		catch (TargetException ex2)
		{
			Diag.Dug(ex2);
			Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "EventMarshaler.Invoke(...): Attempted to invoke on null target or the member does not exist {0}", ex2);
			throw;
		}
		catch (TargetInvocationException ex3)
		{
			Diag.Dug(ex3);
			Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "EventMarshaler.Invoke(...): The target threw an exception.  {0}", ex3);
			if (ex3.InnerException != null)
			{
				Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "EventMarshaler.Invoke(...): The target threw an exception.  Inner exception {0}", ex3.InnerException);
				Exception innerException = ex3.InnerException;
				innerException.Data.Add("TargetSiteCallstack", innerException.StackTrace);
				innerException.Data.Add("OriginalException", ex3);
				ExceptionDispatchInfo.Capture(innerException).Throw();
			}
			throw;
		}
		catch (Exception ex4)
		{
			Diag.Dug(ex4);
			Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "Exception in EventMarshaler.Invoke(...): " + ex4.ToString());
			throw;
		}
	}



	public static void SetDocumentReadOnly(uint docCookie, bool readOnly)
	{
		object docData = RdtManager.GetDocDataFromCookie(docCookie);

		if (docData != null)
		{
			((IVsPersistDocData2)docData).SetDocDataReadOnly(readOnly ? 1 : 0);

			if (RdtManager.ServiceAvailable)
			{
				uint grfDocChanged = (uint)(readOnly
					? _VSRDTFLAGS4.RDT_PendingInitialization
					: _VSRDTFLAGS4.RDT_PendingHierarchyInitialization);

				RdtManager.NotifyDocumentChanged(docCookie, grfDocChanged);
			}
		}
	}


	#endregion Methods





	// =========================================================================================================
	#region Event handling - AbstractDesignerServices
	// =========================================================================================================


	protected static void RaiseBeforeOpenDocument(string mkDocument, DbConnectionStringBuilder csb,
		IList<string> identifierList, EnModelObjectType elementType, EnModelTargetType targetType,
		EventHandler<BeforeOpenDocumentEventArgs> handlers)
	{
		if (handlers == null)
			return;

		BeforeOpenDocumentEventArgs e = new BeforeOpenDocumentEventArgs(mkDocument, csb,
			identifierList, elementType, targetType);

		handlers(null, e);
	}


	#endregion Event handling
}
