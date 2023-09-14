#region Assembly SqlWorkbench.Interfaces, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\SqlWorkbench.Interfaces.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBTextSpan
{
	string Text { get; }

	int AnchorLine { get; }

	int AnchorCol { get; }

	int EndLine { get; }

	int EndCol { get; }

	int LineWithinTextSpan { get; }

	object TextView { get; }
}
