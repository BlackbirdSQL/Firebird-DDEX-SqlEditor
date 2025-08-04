// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Readability")]
[assembly: SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "Do not like")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Readability")]
// [assembly: SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "Required by Legacy SqlSqerver code")]

[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>")]
// [assembly: SuppressMessage("Usage", "VSTHRD001:Avoid legacy thread switching APIs", Justification = "<Pending>")]
// [assembly: SuppressMessage("Performance", "VSSDK003:Support async tool windows", Justification = "<Pending>")]
[assembly: SuppressMessage("Usage", "VSTHRD104:Offer async methods")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Preferred")]
