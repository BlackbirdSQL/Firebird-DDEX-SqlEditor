#pragma once

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>

#ifndef PCXSTR
typedef wchar_t XCHAR;
typedef LPWSTR PXSTR;
typedef LPCWSTR PCXSTR;
typedef char YCHAR;
typedef LPSTR PYSTR;
typedef LPCSTR PCYSTR;
#endif // !PCXSTR

#ifndef SysStr
#define SysStr System::String
#define SysObj System::Object
#endif // !SysStr

#ifndef ReplicaKeyPair

/// <summary>
/// The ReplicaKey KeyValuePair type used on ReplicaKeyEnumerator. For a Cell&lt;SysStr^&gt;^
/// this would be ReplicaKeyPair(Cell&lt;SysStr^&gt;^).
/// </summary>
#define ReplicaKeyPair(__TYPE__) KeyValuePair<ReplicaKey, __TYPE__>

#endif // !ReplicaKeyPair

