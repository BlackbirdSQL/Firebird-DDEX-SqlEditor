#pragma once
#include "pch.h"
#include "CPentaCommon.h"

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


#ifdef _DEBUG
using namespace System::Diagnostics;
using namespace System::IO;
using namespace System::Reflection;
#endif // _DEBUG

namespace C5 {


public ref class Diag abstract sealed
{

public:

#pragma region Fields


	// Specify your own trace log file and settings here or override in VS options


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the context the call was made from. Either 'IDE' or 'APP'
	/// </summary>
	// ---------------------------------------------------------------------------------
	static SysStr^ Context = "APP";


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag::Trace()"/> calls are logged
	/// </summary>
	// ---------------------------------------------------------------------------------
	static bool EnableTrace = true;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not Diag::Ex(bool isException, SysStr^ message, StackTrace^ stack) calls are logged
	/// </summary>
	/// <remarks>
	/// Exceptions are alweays logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	static bool EnableDiagnostics = true;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not diagnostics calls are logged to the log file.
	/// </summary>
	/// <remarks>
	/// Only applies to the Debug configuration. Debug Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
#ifdef _DEBUG
	static bool EnableDiagnosticsLog = true;
#else // _DEBUG
	static bool EnableDiagnosticsLog = false;
#endif // else _DEBUG


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The log file path
	/// </summary>
	// ---------------------------------------------------------------------------------
	static SysStr^ LogFile = "/temp/vsdiag.log";


#pragma endregion Fields





	// =========================================================================================================
#pragma region Property Accessors - Diag
// =========================================================================================================




#pragma endregion Property accessors





// =========================================================================================================
#pragma region Methods - Diag
// =========================================================================================================


private:


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The common Diag diagnostics method
	/// </summary>
	// ---------------------------------------------------------------------------------
#ifdef _DEBUG
	static void Ex(bool isException, SysStr^ message, StackTrace^ stack)
	{
#else // _DEBUG
	static void Ex(bool isException, SysStr ^ message, SysObj ^ stack)
	{
#endif // else _DEBUG

		if (!isException && !EnableDiagnostics && !EnableTrace)
			return;

		int sourceLineNumber = -1;
		SysStr^ methodName = "[Release: MemberName Unavailable]", ^ sourcePath = "[Release: SourcePath Unavailable]";

#ifdef _DEBUG
		StackFrame^ frame = frame = stack->GetFrame(0);
		MethodInfo^ methodInfo = static_cast<MethodInfo^>(frame->GetMethod());;

		methodName = methodInfo->Name;
		sourcePath = frame->GetFileName();
		sourceLineNumber = frame->GetFileLineNumber();
#endif // _DEBUG


		int pos;
		SysStr^ logfile = LogFile;


		if ((pos = sourcePath->IndexOf("\\BlackbirdSql")) == -1)
			pos = sourcePath->IndexOf("\\BlackbirdDsl");

		if (pos != -1)
			sourcePath = sourcePath->Substring(pos + 1);


		SysStr^ str = Context + ":" + (isException ? ":EXCEPTION: " : " ") + (System::DateTime::Now).ToString("hh.mm.ss.ffffff") + ":   "
			+ methodName + " :: " + sourcePath + " :: " + sourceLineNumber +
			(message == "" ? "" : System::Environment::NewLine + "\t" + message) + System::Environment::NewLine;

#ifdef _DEBUG
		// Remove conditional for a full trace
		try
		{
			if (EnableDiagnosticsLog)
			{

				StreamWriter^ sw = File::AppendText(logfile);

				sw->WriteLine(str);

				sw->Close();
			}
		}
		catch (System::Exception^) {}
#endif // _DEBUG

	}



public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method that can flag a call as an System::Exception even if no System::Exception
	/// </summary>
	// ---------------------------------------------------------------------------------
	static void Ex(bool isException, SysStr^ message)
	{
		if (!isException && !EnableDiagnostics && !EnableTrace)
			return;

#ifdef _DEBUG
		StackTrace^ stack = gcnew StackTrace(true);
#else // _DEBUG
		SysObj^ stack = nullptr;
#endif // else _DEBUG


		Ex(isException, message, stack);

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for Exceptions only
	/// </summary>
	// ---------------------------------------------------------------------------------
	static void Ex(System::Exception^ ex)
	{

#ifdef _DEBUG
		StackTrace^ stack = gcnew StackTrace(true);
#else // _DEBUG
		SysObj^ stack = nullptr;
#endif // else _DEBUG

		Ex(true, ex->Message + ":" + System::Environment::NewLine
			+ (ex->StackTrace != nullptr ? ex->StackTrace->ToString() : ""),
			stack);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for Exceptions with message
	/// </summary>
	// ---------------------------------------------------------------------------------
	static void Ex(System::Exception^ ex, SysStr^ message)
	{

#ifdef _DEBUG
		StackTrace^ stack = gcnew StackTrace(true);
#else // _DEBUG
 		SysObj^ stack = nullptr;
#endif // else _DEBUG

		Ex(true, ex->Message
			+ (message != "" ? " " + message : "")
			+ ":" + System::Environment::NewLine
			+ (ex != nullptr && ex->StackTrace != nullptr ? ex->StackTrace->ToString() : ""),
			stack);

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for full stack trace
	/// </summary>
	// ---------------------------------------------------------------------------------
	static void Stack()
	{

#ifdef _DEBUG
		StackTrace^ stack = gcnew StackTrace(true);
#else // _DEBUG
		SysObj^ stack = nullptr;
#endif // else _DEBUG

		SysStr^ message = "TRACE: " + (System::Environment::StackTrace)->ToString();

		Ex(true, message, stack);

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for full stack trace with message
	/// </summary>
	// ---------------------------------------------------------------------------------
	static void Stack(SysStr^ message)
	{
#ifdef _DEBUG
		StackTrace^ stack = gcnew StackTrace(true);
#else // _DEBUG
 		SysObj^ stack = nullptr;
#endif // else _DEBUG

		message += ":" + System::Environment::NewLine + "TRACE: " + (System::Environment::StackTrace)->ToString();

		Ex(true, message, stack);

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Trace method for trace breadcrumbs during debug
	/// </summary>
	// ---------------------------------------------------------------------------------
	static void Trace()
	{
		if (!EnableTrace)
			return;

#ifdef _DEBUG
		StackTrace^ stack = gcnew StackTrace(true);
#else // _DEBUG
		SysObj^ stack = nullptr;
#endif // else _DEBUG

		Ex(false, "", stack);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Trace method for trace breadcrumbs with message during debug
	/// </summary>
	// ---------------------------------------------------------------------------------
	static void Trace(SysStr^ message)
	{
		if (!EnableTrace)
			return;

#ifdef _DEBUG
		StackTrace^ stack = gcnew StackTrace(true);
#else // _DEBUG
 		SysObj^ stack = nullptr;
#endif // else _DEBUG

		Ex(false, message, stack);
	}


#pragma endregion Methods


};

}