// Microsoft.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Common.Win32.HandleChangeEventHandler

using System;

namespace BlackbirdSql.Shared.Events;

public delegate void HandleChangeEventHandler(string handleType, IntPtr handleValue, int currentHandleCount);
