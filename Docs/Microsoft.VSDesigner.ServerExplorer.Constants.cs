// Microsoft.VisualStudio.ServerExplorer, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VSDesigner.ServerExplorer.Constants
using System;
using System.ComponentModel.Design;

internal sealed class Constants
{
	public static readonly Guid GUID_VsUIHierarchyWindowCmds = new Guid("{60481700-078B-11d1-AAF8-00A0C9055A90}");

	public static readonly Guid guidIFCmdId = new Guid("{74d21311-2aee-11d1-8bfb-00a0c90f26f7}");

	public static readonly Guid guidIFGrpId = new Guid("{74d21310-2aee-11d1-8bfb-00a0c90f26f7}");

	public static readonly Guid VSStandardCommandSet97 = new Guid("{5efc7975-14bc-11cf-9b2b-00aa00573819}");

	public static readonly Guid VSStandardCommandSet2K = new Guid("{1496a755-94de-11d0-8c3f-00c04fc2aae2}");

	public static readonly Guid guid_VSD_CommandID = new Guid("{746910da-3a87-4255-a4ae-d1083b0572a0}");

	public static readonly Guid guidDataCmdId = new Guid("{501822E1-B5AF-11d0-B4DC-00A0C91506EF}");

	public const int cmdidRefresh = 189;

	public static readonly CommandID StandardRefreshCommand = new CommandID(VSStandardCommandSet97, 189);

	public static readonly CommandID StandardPropertySheetCommand = new CommandID(VSStandardCommandSet97, 397);

	public const int ECMD_STOP = 220;

	public static readonly CommandID StandardStopRefreshCommand = new CommandID(VSStandardCommandSet2K, 220);

	public const int cmdIFDataCommandUpdateScript = 12345;

	public const int cmdIFDataCommandCreateScript = 12346;

	public const int UIHWCMDID_RightClick = 1;

	// GUID_VsUIHierarchyWindowCmds
	public const int UIHWCMDID_DoubleClick = 2;

	public const int UIHWCMDID_EnterKey = 3;

	public const int UIHWCMDID_StartLabelEdit = 4;

	public const int UIHWCMDID_CommitLabelEdit = 5;

	public const int UIHWCMDID_CancelLabelEdit = 6;

	public const int IDM_IF_CTXT_SERVEXP = 1283;

	public const int cmdIFServExpNodeMenu = 8449;

	public const int cmdIFServExpNodeMenuLast = 8704;

	public const int cmdIFServExpRefresh = 12292;

	public const int cmdIFServExpStopRefresh = 12293;

	public const int cmdIFServExpSaveView = 12298;

	public const int cmdIFServExpSaveViewAs = 12294;

	public const int cmdIFServExpDeleteView = 12295;

	public const int cmdIFServExpViewList = 61440;

	public const int cmdIFServExpAddToForm = 12297;

	public const int cmdIFServExpDeleteNode = 12299;

	public const int OLECMDF_SUPPORTED = 1;

	public const int OLECMDF_ENABLED = 2;

	public const int OLECMDF_LATCHED = 4;

	public const int OLECMDF_NINCHED = 8;

	public const int OLECMDF_INVISIBLE = 16;

	public const int OLECMDTEXTF_NONE = 0;

	public const int OLECMDTEXTF_NAME = 1;

	public const int OLECMDTEXTF_STATUS = 2;

	public const int OLECMDEXECOPT_DODEFAULT = 0;

	public const int OLECMDEXECOPT_PROMPTUSER = 1;

	public const int OLECMDEXECOPT_DONTPROMPTUSER = 2;

	public const int OLECMDEXECOPT_SHOWHELP = 3;

	public static readonly Guid SID_SOleComponentUIManager = new Guid("{5efc7974-14bc-11cf-9b2b-00aa00573819}");

	public const int EXPF_ExpandFolder = 0;

	public const int EXPF_CollapseFolder = 1;

	public const int EXPF_ExpandFolderRecursively = 2;

	public const int EXPF_ExpandParentsToShowItem = 3;

	public const int EXPF_SelectItem = 4;

	public const int EXPF_BoldItem = 5;

	public const int EXPF_ExtendSelectItem = 6;

	public const int EXPF_AddSelectItem = 7;

	public const int EXPF_UnSelectItem = 8;

	public const int EXPF_UnBoldItem = 9;

	public const int EXPF_CutHighlightItem = 10;

	public const int EXPF_AddCutHighlightItem = 11;

	public const int EXPF_UnCutHighlightItem = 12;

	public const int EXPF_EditItemLabel = 13;

	public const int OLECMDERR_E_UNKNOWNGROUP = -2147221244;

	public const int OLECMDERR_E_NOTSUPPORTED = -2147221248;

	public const int CTW_RESERVED_MASK = 65535;

	public const int CTW_fInitNew = 65536;

	public const int CTW_fActivateWithProject = 131072;

	public const int CTW_fActivateWithDocument = 262144;

	public const int CTW_fForceCreate = 524288;

	public const int CTW_fHasBorder = 1048576;

	public const int VSFPROPID_NIL = -1;

	public const int VSFPROPID_Type = -3000;

	public const int VSFPROPID_DocView = -3001;

	public const int VSFPROPID_SPFrame = -3002;

	public const int VSFPROPID_SPProjContext = -3003;

	public const int VSFPROPID_Caption = -3004;

	public const int VSFPROPID_LastFind = -3005;

	public const int VSFPROPID_LastFindOptions = -3006;

	public const int VSFPROPID_WindowState = -3007;

	public const int VSFPROPID_FrameMode = -3008;

	public const int VSFPROPID_IsWindowTabbed = -3009;

	public const int VSFPROPID_UserContext = -3010;

	public const int VSFPROPID_DocCookie = -4000;

	public const int VSFPROPID_OwnerCaption = -4001;

	public const int VSFPROPID_EditorCaption = -4002;

	public const int VSFPROPID_pszMkDocument = -4003;

	public const int VSFPROPID_DocData = -4004;

	public const int VSFPROPID_Hierarchy = -4005;

	public const int VSFPROPID_ItemID = -4006;

	public const int VSFPROPID_CmdUIGuid = -4007;

	public const int VSFPROPID_CreateDocWinFlags = -4008;

	public const int VSFPROPID_guidEditorType = -4009;

	public const int VSFPROPID_pszPhysicalView = -4010;

	public const int VSFPROPID_InheritKeyBindings = -4011;

	public const int VSFPROPID_GuidPersistenceSlot = -5000;

	public const int VSFPROPID_GuidAutoActivate = -5001;

	public const int VSFPROPID_CreateToolWinFlags = -5002;

	public const int VSFPROPID_ExtWindowObject = -5003;

	public const int VSSPROPID_AppDataDir = -9021;

	public const int UIHWF_ActAsProjectTypeWin = 1;

	public const int UIHWF_DoNotSortRootNodes = 2;

	public const int UIHWF_SupportToolWindowToolbars = 4;

	public const int UIHWF_ForceSingleSelect = 8;

	public const int UIHWF_InitWithHiddenRootHierarchy = 16;

	public const int UIHWF_UseSolutionAsHiddenRootHierarchy = 32;

	public const int UIHWF_LinesAtRoot = 64;

	public const int UIHWF_SortChildNodes = 128;

	public const int UIHWF_NoStateIcon = 256;

	public const int UIHWF_InitWithHiddenParentRoot = 1024;

	public const int UIHWF_PropagateAltHierarchyItem = 4096;

	public const int UIHWF_RouteCmdidDelete = 8192;

	public const int VSTWT_LEFT = 0;

	public const int VSTWT_TOP = 1;

	public const int VSTWT_RIGHT = 2;

	public const int VSTWT_BOTTOM = 3;

	public static readonly IntPtr HIERARCHY_DONTPROPAGATE = new IntPtr(-2);

	public const int IDM_IF_TOOLBAR_SERVEXP = 1536;

	public static readonly Guid CLSID_CoolPbrsToolWin = new Guid(1955883015, 14240, 4562, new byte[8] { 162, 115, 0, 192, 79, 142, 244, 255 });

	public const int SEID_PropertyBrowserSID = 4;

	public static readonly Guid CLSID_VSUIHIERARCHYWINDOW = new Guid(2106985223, 31480, 4560, new byte[8] { 142, 94, 0, 160, 201, 17, 0, 90 });

	public static readonly Guid ID_DataViewPackage = new Guid("{4F174C20-8C12-11d0-8340-0000F80270F8}");
}
