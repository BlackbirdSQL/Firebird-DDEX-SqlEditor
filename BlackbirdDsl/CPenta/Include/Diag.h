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

	#pragma region Variables


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
	/// Flag indicating whether or not <see cref="Diag::Dug(bool isException, SysStr^ message, StackTrace^ stack)"/> calls are logged
	/// </summary>
	/// <remarks>
	/// Exceptions are alweays logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	static bool EnableDiagnostics = true;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not Firebird debug library diagnostics calls are logged
	/// </summary>
	/// <remarks>
	/// Only applies to the Debug configuration. Debug Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	static bool EnableFbDiagnostics = true;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not diagnostics calls are logged to the log file.
	/// </summary>
	/// <remarks>
	/// Only applies to the Debug configuration. Debug Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	static bool EnableDiagnosticsLog = false;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The log file path
	/// </summary>
	// ---------------------------------------------------------------------------------
	static SysStr^ LogFile = "/temp/vsdiag.log";


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The Firebird log file path
	/// </summary>
	// ---------------------------------------------------------------------------------
	static SysStr^ FbLogFile = "/bin/vsdiagfb.log";


	#pragma endregion Variables





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
	static void Dug(bool isException, SysStr^ message, StackTrace^ stack)
#else
	static void Dug(bool isException, SysStr^ message, SysObj^ stack)
#endif // _DEBUG
	{
		if (!isException && !EnableDiagnostics && !EnableTrace)
			return;

		int sourceLineNumber = 0;
		SysStr^ methodName = "Release:Unavailable", ^ sourceFilePath = "Release:Unavailable";

#ifdef _DEBUG
		StackFrame^ frame = frame = stack->GetFrame(0);
		MethodInfo^ methodInfo = static_cast<MethodInfo^>(frame->GetMethod());;

		methodName = methodInfo->Name;
		sourceFilePath = frame->GetFileName();
		sourceLineNumber = frame->GetFileLineNumber();
#endif

		int pos;
		SysStr^ logfile = LogFile;


		if ((pos = sourceFilePath->IndexOf("\\BlackbirdSql")) == -1)
		{
			if ((pos = sourceFilePath->IndexOf("\\FirebirdSql")) == -1)
				pos = sourceFilePath->IndexOf("\\EntityFramework.Firebird");

			if (pos != -1)
			{
				if (!isException && !EnableFbDiagnostics)
					return;
				logfile = FbLogFile;
			}
		}


		if (pos != -1)
			sourceFilePath = sourceFilePath->Substring(pos + 1);


		SysStr^ str = Context + ":" + (isException ? ":EXCEPTION: " : " ") + (System::DateTime::Now).ToString("hh.mm.ss.ffffff") + ":   "
			+ methodName + " :: " + sourceFilePath + " :: " + sourceLineNumber +
			(message == "" ? "" : System::Environment::NewLine + "\t" + message) + System::Environment::NewLine;

#if _DEBUG
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
#endif

		// System::Diagnostics.Debug::WriteLine(str, "BlackbirSql");
	}



public:

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method that can flag a call as an System::Exception even if no System::Exception
	/// </summary>
	// ---------------------------------------------------------------------------------
	static void Dug(bool isException, SysStr^ message)
	{
		if (!isException && !EnableDiagnostics && !EnableTrace)
			return;

#ifdef _DEBUG
		StackTrace^ stack = gcnew StackTrace(true);
#else
		SysObj^ stack = nullptr;
#endif // _DEBUG


		Dug(isException, message, stack);

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for Exceptions only
	/// </summary>
	// ---------------------------------------------------------------------------------
	static void Dug(System::Exception^ ex)
	{

#ifdef _DEBUG
		StackTrace^ stack = gcnew StackTrace(true);
#else
		SysObj^ stack = nullptr;
#endif // _DEBUG

		Dug(true, ex->Message + ":" + System::Environment::NewLine
			+ (ex->StackTrace != nullptr ? ex->StackTrace->ToString() : ""),
			stack);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Diagnostics method for Exceptions with message
	/// </summary>
	// ---------------------------------------------------------------------------------
	static void Dug(System::Exception^ ex, SysStr^ message)
	{

#ifdef _DEBUG
		StackTrace^ stack = gcnew StackTrace(true);
#else
		SysObj^ stack = nullptr;
#endif // _DEBUG

		Dug(true, ex->Message
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
#else
		SysObj^ stack = nullptr;
#endif // _DEBUG

		SysStr^ message = "TRACE: " + (System::Environment::StackTrace)->ToString();

		Dug(true, message, stack);

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
#else
		SysObj^ stack = nullptr;
#endif // _DEBUG

		message += ":" + System::Environment::NewLine + "TRACE: " + (System::Environment::StackTrace)->ToString();

		Dug(true, message, stack);

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
#else
		SysObj^ stack = nullptr;
#endif // _DEBUG

		Dug(false, "", stack);
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
#else
		SysObj^ stack = nullptr;
#endif // _DEBUG

		Dug(false, message, stack);
	}


	#pragma endregion Methods


};

}