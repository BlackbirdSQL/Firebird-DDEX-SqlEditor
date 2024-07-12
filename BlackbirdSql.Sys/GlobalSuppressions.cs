// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Readability")]
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Readability")]
[assembly: SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "Do not like")]
[assembly: SuppressMessage("Usage", "VSTHRD104:Offer async methods")]
[assembly: SuppressMessage("Reliability", "VSSDK006:Check services exist", Justification = "<Pending>")]
[assembly: SuppressMessage("Build", "VSSDK007:Await/join tasks created from ThreadHelper.JoinableTaskFactory.RunAsync.")]

