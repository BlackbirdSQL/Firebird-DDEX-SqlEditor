// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// Microsoft.VisualStudio.Shell.Framework, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.VSConstants
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;

public sealed class VSConstants
{
	public static class WellKnownOldVersionValues
	{
		public const string LowestMajor = "LowestMajor";

		public const string LowestMajorMinor = "LowestMajorMinor";

		public const string Current = "Current";
	}

	public static class WellKnownToolboxStringMaps
	{
		public const string MultiTargeting = "MultiTargeting:{FBB22D27-7B21-42ac-88C8-595F94BDBCA5}";
	}

	public static class ToolboxMultitargetingFields
	{
		public const string TypeName = "TypeName";

		public const string AssemblyName = "AssemblyName";

		public const string Frameworks = "Frameworks";

		public const string ItemProvider = "ItemProvider";

		public const string UseProjectTargetFrameworkVersionInTooltip = "UseProjectTargetFrameworkVersionInTooltip";
	}

	public static class CMDSETID
	{
		public const string StandardCommandSet97_string = "{5EFC7975-14BC-11CF-9B2B-00AA00573819}";

		public static readonly Guid StandardCommandSet97_guid = new Guid("{5EFC7975-14BC-11CF-9B2B-00AA00573819}");

		public const string StandardCommandSet2K_string = "{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}";

		public static readonly Guid StandardCommandSet2K_guid = new Guid("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}");

		public const string StandardCommandSet2010_string = "{5DD0BB59-7076-4C59-88D3-DE36931F63F0}";

		public static readonly Guid StandardCommandSet2010_guid = new Guid("{5DD0BB59-7076-4C59-88D3-DE36931F63F0}");

		public const string StandardCommandSet11_string = "{D63DB1F0-404E-4B21-9648-CA8D99245EC3}";

		public static readonly Guid StandardCommandSet11_guid = new Guid("{D63DB1F0-404E-4B21-9648-CA8D99245EC3}");

		public const string StandardCommandSet12_string = "{2A8866DC-7BDE-4dc8-A360-A60679534384}";

		public static readonly Guid StandardCommandSet12_guid = new Guid("{2A8866DC-7BDE-4dc8-A360-A60679534384}");

		public const string StandardCommandSet14_string = "{4C7763BF-5FAF-4264-A366-B7E1F27BA958}";

		public static readonly Guid StandardCommandSet14_guid = new Guid("{4C7763BF-5FAF-4264-A366-B7E1F27BA958}");

		public const string StandardCommandSet15_string = "{712C6C80-883B-4AAD-B430-BBCA5256FA9D}";

		public static readonly Guid StandardCommandSet15_guid = new Guid("{712C6C80-883B-4AAD-B430-BBCA5256FA9D}");

		public const string StandardCommandSet16_string = "{8F380902-6040-4097-9837-D3F40E66F908}";

		public static readonly Guid StandardCommandSet16_guid = new Guid("{8F380902-6040-4097-9837-D3F40E66F908}");

		public const string StandardCommandSet17_string = "{43F755C7-7916-454D-81A9-90D4914019DD}";

		public static readonly Guid StandardCommandSet17_guid = new Guid("{43F755C7-7916-454D-81A9-90D4914019DD}");

		public const string ShellMainMenu_string = "{D309F791-903F-11D0-9EFC-00A0C911004F}";

		public static readonly Guid ShellMainMenu_guid = new Guid("{D309F791-903F-11D0-9EFC-00A0C911004F}");

		public const string UIHierarchyWindowCommandSet_string = "{60481700-078B-11D1-AAF8-00A0C9055A90}";

		public static readonly Guid UIHierarchyWindowCommandSet_guid = new Guid("{60481700-078B-11D1-AAF8-00A0C9055A90}");

		public const string VsDocOutlinePackageCommandSet_string = "{21AF45B0-FFA5-11D0-B63F-00A0C922E851}";

		public static readonly Guid VsDocOutlinePackageCommandSet_guid = new Guid("{21AF45B0-FFA5-11D0-B63F-00A0C922E851}");

		public const string SolutionExplorerPivotList_string = "{afe48dbb-c199-46ce-ba09-adbd5e933ea3}";

		public static readonly Guid SolutionExplorerPivotList_guid = new Guid("{afe48dbb-c199-46ce-ba09-adbd5e933ea3}");

		public const string CSharpGroup_string = "{5D7E7F65-A63F-46ee-84F1-990B2CAB23F9}";

		public static readonly Guid CSharpGroup_guid = new Guid("{5D7E7F65-A63F-46ee-84F1-990B2CAB23F9}");
	}

	public static class NewDocumentStateReason
	{
		public static readonly Guid FindSymbolResults = StandardToolWindows.FindSymbolResults;

		public static readonly Guid FindResults = StandardToolWindows.Find1;

		public static readonly Guid Navigation = new Guid("8d57e022-9e44-4efd-8e4e-230284f86376");

		public static readonly Guid SolutionExplorer = StandardToolWindows.SolutionExplorer;

		public static readonly Guid TeamExplorer = StandardToolWindows.TeamExplorer;
	}

	[Guid("12F1A339-02B9-46e6-BDAF-1071F76056BF")]
	public enum AppCommandCmdID
	{
		BrowserBackward = 1,
		BrowserForward,
		BrowserRefresh,
		BrowserStop,
		BrowserSearch,
		BrowserFavorites,
		BrowserHome,
		VolumeMute,
		VolumeDown,
		VolumeUp,
		MediaNextTrack,
		MediaPreviousTrack,
		MediaStop,
		MediaPlayPause,
		LaunchMail,
		LaunchMediaSelect,
		LaunchApp1,
		LaunchApp2,
		BassDown,
		BassBoost,
		BassUp,
		TrebleDown,
		TrebleUp,
		MicrophoneVolumeMute,
		MicrophoneVolumeDown,
		MicrophoneVolumeUp
	}

	[Guid("5EFC7975-14BC-11CF-9B2B-00AA00573819")]
	public enum VSStd97CmdID
	{
		AlignBottom = 1,
		AlignHorizontalCenters = 2,
		AlignLeft = 3,
		AlignRight = 4,
		AlignToGrid = 5,
		AlignTop = 6,
		AlignVerticalCenters = 7,
		ArrangeBottom = 8,
		ArrangeRight = 9,
		BringForward = 10,
		BringToFront = 11,
		CenterHorizontally = 12,
		CenterVertically = 13,
		Code = 14,
		Copy = 15,
		Cut = 16,
		Delete = 17,
		FontName = 18,
		FontNameGetList = 500,
		FontSize = 19,
		FontSizeGetList = 501,
		Group = 20,
		HorizSpaceConcatenate = 21,
		HorizSpaceDecrease = 22,
		HorizSpaceIncrease = 23,
		HorizSpaceMakeEqual = 24,
		LockControls = 369,
		InsertObject = 25,
		Paste = 26,
		Print = 27,
		Properties = 28,
		Redo = 29,
		MultiLevelRedo = 30,
		SelectAll = 31,
		SendBackward = 32,
		SendToBack = 33,
		ShowTable = 34,
		SizeToControl = 35,
		SizeToControlHeight = 36,
		SizeToControlWidth = 37,
		SizeToFit = 38,
		SizeToGrid = 39,
		SnapToGrid = 40,
		TabOrder = 41,
		Toolbox = 42,
		Undo = 43,
		MultiLevelUndo = 44,
		Ungroup = 45,
		VertSpaceConcatenate = 46,
		VertSpaceDecrease = 47,
		VertSpaceIncrease = 48,
		VertSpaceMakeEqual = 49,
		ZoomPercent = 50,
		BackColor = 51,
		Bold = 52,
		BorderColor = 53,
		BorderDashDot = 54,
		BorderDashDotDot = 55,
		BorderDashes = 56,
		BorderDots = 57,
		BorderShortDashes = 58,
		BorderSolid = 59,
		BorderSparseDots = 60,
		BorderWidth1 = 61,
		BorderWidth2 = 62,
		BorderWidth3 = 63,
		BorderWidth4 = 64,
		BorderWidth5 = 65,
		BorderWidth6 = 66,
		BorderWidthHairline = 67,
		Flat = 68,
		ForeColor = 69,
		Italic = 70,
		JustifyCenter = 71,
		JustifyGeneral = 72,
		JustifyLeft = 73,
		JustifyRight = 74,
		Raised = 75,
		Sunken = 76,
		Underline = 77,
		Chiseled = 78,
		Etched = 79,
		Shadowed = 80,
		CompDebug1 = 81,
		CompDebug2 = 82,
		CompDebug3 = 83,
		CompDebug4 = 84,
		CompDebug5 = 85,
		CompDebug6 = 86,
		CompDebug7 = 87,
		CompDebug8 = 88,
		CompDebug9 = 89,
		CompDebug10 = 90,
		CompDebug11 = 91,
		CompDebug12 = 92,
		CompDebug13 = 93,
		CompDebug14 = 94,
		CompDebug15 = 95,
		ExistingSchemaEdit = 96,
		Find = 97,
		GetZoom = 98,
		QueryOpenDesign = 99,
		QueryOpenNew = 100,
		SingleTableDesign = 101,
		SingleTableNew = 102,
		ShowGrid = 103,
		NewTable = 104,
		CollapsedView = 105,
		FieldView = 106,
		VerifySQL = 107,
		HideTable = 108,
		PrimaryKey = 109,
		Save = 110,
		SaveAs = 111,
		SortAscending = 112,
		SortDescending = 113,
		AppendQuery = 114,
		CrosstabQuery = 115,
		DeleteQuery = 116,
		MakeTableQuery = 117,
		SelectQuery = 118,
		UpdateQuery = 119,
		Parameters = 120,
		Totals = 121,
		ViewCollapsed = 122,
		ViewFieldList = 123,
		ViewKeys = 124,
		ViewGrid = 125,
		InnerJoin = 126,
		RightOuterJoin = 127,
		LeftOuterJoin = 128,
		FullOuterJoin = 129,
		UnionJoin = 130,
		ShowSQLPane = 131,
		ShowGraphicalPane = 132,
		ShowDataPane = 133,
		ShowQBEPane = 134,
		SelectAllFields = 135,
		OLEObjectMenuButton = 136,
		ObjectVerbList0 = 137,
		ObjectVerbList1 = 138,
		ObjectVerbList2 = 139,
		ObjectVerbList3 = 140,
		ObjectVerbList4 = 141,
		ObjectVerbList5 = 142,
		ObjectVerbList6 = 143,
		ObjectVerbList7 = 144,
		ObjectVerbList8 = 145,
		ObjectVerbList9 = 146,
		ConvertObject = 147,
		CustomControl = 148,
		CustomizeItem = 149,
		Rename = 150,
		Import = 151,
		NewPage = 152,
		Move = 153,
		Cancel = 154,
		Font = 155,
		ExpandLinks = 156,
		ExpandImages = 157,
		ExpandPages = 158,
		RefocusDiagram = 159,
		TransitiveClosure = 160,
		CenterDiagram = 161,
		ZoomIn = 162,
		ZoomOut = 163,
		RemoveFilter = 164,
		HidePane = 165,
		DeleteTable = 166,
		DeleteRelationship = 167,
		Remove = 168,
		JoinLeftAll = 169,
		JoinRightAll = 170,
		AddToOutput = 171,
		OtherQuery = 172,
		GenerateChangeScript = 173,
		SaveSelection = 174,
		AutojoinCurrent = 175,
		AutojoinAlways = 176,
		EditPage = 177,
		ViewLinks = 178,
		Stop = 179,
		Pause = 180,
		Resume = 181,
		FilterDiagram = 182,
		ShowAllObjects = 183,
		ShowApplications = 184,
		ShowOtherObjects = 185,
		ShowPrimRelationships = 186,
		Expand = 187,
		Collapse = 188,
		Refresh = 189,
		Layout = 190,
		ShowResources = 191,
		InsertHTMLWizard = 192,
		ShowDownloads = 193,
		ShowExternals = 194,
		ShowInBoundLinks = 195,
		ShowOutBoundLinks = 196,
		ShowInAndOutBoundLinks = 197,
		Preview = 198,
		Open = 261,
		OpenWith = 199,
		ShowPages = 200,
		RunQuery = 201,
		ClearQuery = 202,
		RecordFirst = 203,
		RecordLast = 204,
		RecordNext = 205,
		RecordPrevious = 206,
		RecordGoto = 207,
		RecordNew = 208,
		InsertNewMenu = 209,
		InsertSeparator = 210,
		EditMenuNames = 211,
		DebugExplorer = 212,
		DebugProcesses = 213,
		ViewThreadsWindow = 214,
		WindowUIList = 215,
		NewProject = 216,
		OpenProject = 217,
		OpenProjectFromWeb = 450,
		OpenSolution = 218,
		CloseSolution = 219,
		FileNew = 221,
		NewProjectFromExisting = 385,
		FileOpen = 222,
		FileOpenFromWeb = 451,
		FileClose = 223,
		SaveSolution = 224,
		SaveSolutionAs = 225,
		SaveProjectItemAs = 226,
		PageSetup = 227,
		PrintPreview = 228,
		Exit = 229,
		Replace = 230,
		Goto = 231,
		PropertyPages = 232,
		FullScreen = 233,
		ProjectExplorer = 234,
		PropertiesWindow = 235,
		TaskListWindow = 236,
		OutputWindow = 237,
		ObjectBrowser = 238,
		DocOutlineWindow = 239,
		ImmediateWindow = 240,
		WatchWindow = 241,
		LocalsWindow = 242,
		CallStack = 243,
		AutosWindow = 747,
		ThisWindow = 748,
		AddNewItem = 220,
		AddExistingItem = 244,
		NewFolder = 245,
		SetStartupProject = 246,
		ProjectSettings = 247,
		ProjectReferences = 367,
		StepInto = 248,
		StepOver = 249,
		StepOut = 250,
		RunToCursor = 251,
		AddWatch = 252,
		EditWatch = 253,
		QuickWatch = 254,
		ToggleBreakpoint = 255,
		ClearBreakpoints = 256,
		ShowBreakpoints = 257,
		SetNextStatement = 258,
		ShowNextStatement = 259,
		EditBreakpoint = 260,
		DetachDebugger = 262,
		CustomizeKeyboard = 263,
		ToolsOptions = 264,
		NewWindow = 265,
		Split = 266,
		Cascade = 267,
		TileHorz = 268,
		TileVert = 269,
		TechSupport = 270,
		About = 271,
		DebugOptions = 272,
		DeleteWatch = 274,
		CollapseWatch = 275,
		PbrsToggleStatus = 282,
		PropbrsHide = 283,
		DockingView = 284,
		HideActivePane = 285,
		PaneNextPane = 316,
		PanePrevPane = 317,
		PaneNextTab = 286,
		PanePrevTab = 287,
		PaneCloseToolWindow = 288,
		PaneActivateDocWindow = 289,
		DockingViewMDI = 290,
		DockingViewFloater = 291,
		AutoHideWindow = 292,
		MoveToDropdownBar = 293,
		FindCmd = 294,
		Start = 295,
		Restart = 296,
		AddinManager = 297,
		MultiLevelUndoList = 298,
		MultiLevelRedoList = 299,
		ToolboxAddTab = 300,
		ToolboxDeleteTab = 301,
		ToolboxRenameTab = 302,
		ToolboxTabMoveUp = 303,
		ToolboxTabMoveDown = 304,
		ToolboxRenameItem = 305,
		ToolboxListView = 306,
		WindowUIGetList = 308,
		InsertValuesQuery = 309,
		ShowProperties = 310,
		ThreadSuspend = 311,
		ThreadResume = 312,
		ThreadSetFocus = 313,
		DisplayRadix = 314,
		OpenProjectItem = 315,
		ClearPane = 318,
		GotoErrorTag = 319,
		TaskListSortByCategory = 320,
		TaskListSortByFileLine = 321,
		TaskListSortByPriority = 322,
		TaskListSortByDefaultSort = 323,
		TaskListShowTooltip = 324,
		TaskListFilterByNothing = 325,
		CancelEZDrag = 326,
		TaskListFilterByCategoryCompiler = 327,
		TaskListFilterByCategoryComment = 328,
		ToolboxAddItem = 329,
		ToolboxReset = 330,
		SaveProjectItem = 331,
		SaveOptions = 959,
		ViewForm = 332,
		ViewCode = 333,
		PreviewInBrowser = 334,
		BrowseWith = 336,
		SearchSetCombo = 307,
		SearchCombo = 337,
		EditLabel = 338,
		Exceptions = 339,
		DefineViews = 340,
		ToggleSelMode = 341,
		ToggleInsMode = 342,
		LoadUnloadedProject = 343,
		UnloadLoadedProject = 344,
		ElasticColumn = 345,
		HideColumn = 346,
		TaskListPreviousView = 347,
		ZoomDialog = 348,
		FindHiddenText = 349,
		FindMatchCase = 350,
		FindWholeWord = 351,
		FindSimplePattern = 276,
		FindRegularExpression = 352,
		FindBackwards = 353,
		FindInSelection = 354,
		FindStop = 355,
		FindInFiles = 277,
		ReplaceInFiles = 278,
		NextLocation = 279,
		PreviousLocation = 280,
		GotoQuick = 281,
		TaskListNextError = 357,
		TaskListPrevError = 358,
		TaskListFilterByCategoryUser = 359,
		TaskListFilterByCategoryShortcut = 360,
		TaskListFilterByCategoryHTML = 361,
		TaskListFilterByCurrentFile = 362,
		TaskListFilterByChecked = 363,
		TaskListFilterByUnchecked = 364,
		TaskListSortByDescription = 365,
		TaskListSortByChecked = 366,
		StartNoDebug = 368,
		FindNext = 370,
		FindPrev = 371,
		FindSelectedNext = 372,
		FindSelectedPrev = 373,
		SearchGetList = 374,
		InsertBreakpoint = 375,
		EnableBreakpoint = 376,
		F1Help = 377,
		MoveToNextEZCntr = 384,
		UpdateMarkerSpans = 386,
		MoveToPreviousEZCntr = 393,
		ProjectProperties = 396,
		PropSheetOrProperties = 397,
		TshellStep = 398,
		TshellRun = 399,
		MarkerCmd0 = 400,
		MarkerCmd1 = 401,
		MarkerCmd2 = 402,
		MarkerCmd3 = 403,
		MarkerCmd4 = 404,
		MarkerCmd5 = 405,
		MarkerCmd6 = 406,
		MarkerCmd7 = 407,
		MarkerCmd8 = 408,
		MarkerCmd9 = 409,
		MarkerLast = 409,
		MarkerEnd = 410,
		ReloadProject = 412,
		UnloadProject = 413,
		NewBlankSolution = 414,
		SelectProjectTemplate = 415,
		DetachAttachOutline = 420,
		ShowHideOutline = 421,
		SyncOutline = 422,
		RunToCallstCursor = 423,
		NoCmdsAvailable = 424,
		ContextWindow = 427,
		Alias = 428,
		GotoCommandLine = 429,
		EvaluateExpression = 430,
		ImmediateMode = 431,
		EvaluateStatement = 432,
		FindResultWindow1 = 433,
		FindResultWindow2 = 434,
		ViewFindResultWindow1 = 435,
		ViewFindResultWindow2 = 436,
		ViewFindResultWindow3 = 437,
		ViewFindResultWindow4 = 438,
		ViewFindResultWindow5 = 439,
		ViewFindReferencesWindow1 = 440,
		ViewFindReferencesWindow2 = 441,
		ViewFindReferencesWindow3 = 442,
		ViewFindReferencesWindow4 = 443,
		ViewFindReferencesWindow5 = 444,
		RenameBookmark = 559,
		ToggleBookmark = 560,
		DeleteBookmark = 561,
		BookmarkWindowGoToBookmark = 562,
		EnableBookmark = 564,
		NewBookmarkFolder = 565,
		NextBookmarkFolder = 568,
		PrevBookmarkFolder = 569,
		Window1 = 570,
		Window2 = 571,
		Window3 = 572,
		Window4 = 573,
		Window5 = 574,
		Window6 = 575,
		Window7 = 576,
		Window8 = 577,
		Window9 = 578,
		Window10 = 579,
		Window11 = 580,
		Window12 = 581,
		Window13 = 582,
		Window14 = 583,
		Window15 = 584,
		Window16 = 585,
		Window17 = 586,
		Window18 = 587,
		Window19 = 588,
		Window20 = 589,
		Window21 = 590,
		Window22 = 591,
		Window23 = 592,
		Window24 = 593,
		Window25 = 594,
		MoreWindows = 595,
		AutoHideAllWindows = 597,
		TaskListTaskHelp = 598,
		ClassView = 599,
		MRUProj1 = 600,
		MRUProj2 = 601,
		MRUProj3 = 602,
		MRUProj4 = 603,
		MRUProj5 = 604,
		MRUProj6 = 605,
		MRUProj7 = 606,
		MRUProj8 = 607,
		MRUProj9 = 608,
		MRUProj10 = 609,
		MRUProj11 = 610,
		MRUProj12 = 611,
		MRUProj13 = 612,
		MRUProj14 = 613,
		MRUProj15 = 614,
		MRUProj16 = 615,
		MRUProj17 = 616,
		MRUProj18 = 617,
		MRUProj19 = 618,
		MRUProj20 = 619,
		MRUProj21 = 620,
		MRUProj22 = 621,
		MRUProj23 = 622,
		MRUProj24 = 623,
		MRUProj25 = 624,
		SplitNext = 625,
		SplitPrev = 626,
		CloseAllDocuments = 627,
		NextDocument = 628,
		PrevDocument = 629,
		Tool1 = 630,
		Tool2 = 631,
		Tool3 = 632,
		Tool4 = 633,
		Tool5 = 634,
		Tool6 = 635,
		Tool7 = 636,
		Tool8 = 637,
		Tool9 = 638,
		Tool10 = 639,
		Tool11 = 640,
		Tool12 = 641,
		Tool13 = 642,
		Tool14 = 643,
		Tool15 = 644,
		Tool16 = 645,
		Tool17 = 646,
		Tool18 = 647,
		Tool19 = 648,
		Tool20 = 649,
		Tool21 = 650,
		Tool22 = 651,
		Tool23 = 652,
		Tool24 = 653,
		ExternalCommands = 654,
		PasteNextTBXCBItem = 655,
		ToolboxShowAllTabs = 656,
		ProjectDependencies = 657,
		CloseDocument = 658,
		ToolboxSortItems = 659,
		ViewBarView1 = 660,
		ViewBarView2 = 661,
		ViewBarView3 = 662,
		ViewBarView4 = 663,
		ViewBarView5 = 664,
		ViewBarView6 = 665,
		ViewBarView7 = 666,
		ViewBarView8 = 667,
		ViewBarView9 = 668,
		ViewBarView10 = 669,
		ViewBarView11 = 670,
		ViewBarView12 = 671,
		ViewBarView13 = 672,
		ViewBarView14 = 673,
		ViewBarView15 = 674,
		ViewBarView16 = 675,
		ViewBarView17 = 676,
		ViewBarView18 = 677,
		ViewBarView19 = 678,
		ViewBarView20 = 679,
		ViewBarView21 = 680,
		ViewBarView22 = 681,
		ViewBarView23 = 682,
		ViewBarView24 = 683,
		SolutionCfg = 684,
		SolutionCfgGetList = 685,
		ManageIndexes = 675,
		ManageRelationships = 676,
		ManageConstraints = 677,
		TaskListCustomView1 = 678,
		TaskListCustomView2 = 679,
		TaskListCustomView3 = 680,
		TaskListCustomView4 = 681,
		TaskListCustomView5 = 682,
		TaskListCustomView6 = 683,
		TaskListCustomView7 = 684,
		TaskListCustomView8 = 685,
		TaskListCustomView9 = 686,
		TaskListCustomView10 = 687,
		TaskListCustomView11 = 688,
		TaskListCustomView12 = 689,
		TaskListCustomView13 = 690,
		TaskListCustomView14 = 691,
		TaskListCustomView15 = 692,
		TaskListCustomView16 = 693,
		TaskListCustomView17 = 694,
		TaskListCustomView18 = 695,
		TaskListCustomView19 = 696,
		TaskListCustomView20 = 697,
		TaskListCustomView21 = 698,
		TaskListCustomView22 = 699,
		TaskListCustomView23 = 700,
		TaskListCustomView24 = 701,
		TaskListCustomView25 = 702,
		TaskListCustomView26 = 703,
		TaskListCustomView27 = 704,
		TaskListCustomView28 = 705,
		TaskListCustomView29 = 706,
		TaskListCustomView30 = 707,
		TaskListCustomView31 = 708,
		TaskListCustomView32 = 709,
		TaskListCustomView33 = 710,
		TaskListCustomView34 = 711,
		TaskListCustomView35 = 712,
		TaskListCustomView36 = 713,
		TaskListCustomView37 = 714,
		TaskListCustomView38 = 715,
		TaskListCustomView39 = 716,
		TaskListCustomView40 = 717,
		TaskListCustomView41 = 718,
		TaskListCustomView42 = 719,
		TaskListCustomView43 = 720,
		TaskListCustomView44 = 721,
		TaskListCustomView45 = 722,
		TaskListCustomView46 = 723,
		TaskListCustomView47 = 724,
		TaskListCustomView48 = 725,
		TaskListCustomView49 = 726,
		TaskListCustomView50 = 727,
		WhiteSpace = 728,
		CommandWindow = 729,
		CommandWindowMarkMode = 730,
		LogCommandWindow = 731,
		Shell = 732,
		SingleChar = 733,
		ZeroOrMore = 734,
		OneOrMore = 735,
		BeginLine = 736,
		EndLine = 737,
		BeginWord = 738,
		EndWord = 739,
		CharInSet = 740,
		CharNotInSet = 741,
		Or = 742,
		Escape = 743,
		TagExp = 744,
		PatternMatchHelp = 745,
		RegExList = 746,
		DebugReserved1 = 747,
		DebugReserved2 = 748,
		DebugReserved3 = 749,
		WildZeroOrMore = 754,
		WildSingleChar = 755,
		WildSingleDigit = 756,
		WildCharInSet = 757,
		WildCharNotInSet = 758,
		FindWhatText = 759,
		TaggedExp1 = 760,
		TaggedExp2 = 761,
		TaggedExp3 = 762,
		TaggedExp4 = 763,
		TaggedExp5 = 764,
		TaggedExp6 = 765,
		TaggedExp7 = 766,
		TaggedExp8 = 767,
		TaggedExp9 = 768,
		EditorWidgetClick = 769,
		CmdWinUpdateAC = 770,
		SlnCfgMgr = 771,
		AddNewProject = 772,
		AddExistingProject = 773,
		AddExistingProjFromWeb = 774,
		AutoHideContext1 = 776,
		AutoHideContext2 = 777,
		AutoHideContext3 = 778,
		AutoHideContext4 = 779,
		AutoHideContext5 = 780,
		AutoHideContext6 = 781,
		AutoHideContext7 = 782,
		AutoHideContext8 = 783,
		AutoHideContext9 = 784,
		AutoHideContext10 = 785,
		AutoHideContext11 = 786,
		AutoHideContext12 = 787,
		AutoHideContext13 = 788,
		AutoHideContext14 = 789,
		AutoHideContext15 = 790,
		AutoHideContext16 = 791,
		AutoHideContext17 = 792,
		AutoHideContext18 = 793,
		AutoHideContext19 = 794,
		AutoHideContext20 = 795,
		AutoHideContext21 = 796,
		AutoHideContext22 = 797,
		AutoHideContext23 = 798,
		AutoHideContext24 = 799,
		AutoHideContext25 = 800,
		AutoHideContext26 = 801,
		AutoHideContext27 = 802,
		AutoHideContext28 = 803,
		AutoHideContext29 = 804,
		AutoHideContext30 = 805,
		AutoHideContext31 = 806,
		AutoHideContext32 = 807,
		AutoHideContext33 = 808,
		ShellNavBackward = 809,
		ShellNavForward = 810,
		ShellNavigate1 = 811,
		ShellNavigate2 = 812,
		ShellNavigate3 = 813,
		ShellNavigate4 = 814,
		ShellNavigate5 = 815,
		ShellNavigate6 = 816,
		ShellNavigate7 = 817,
		ShellNavigate8 = 818,
		ShellNavigate9 = 819,
		ShellNavigate10 = 820,
		ShellNavigate11 = 821,
		ShellNavigate12 = 822,
		ShellNavigate13 = 823,
		ShellNavigate14 = 824,
		ShellNavigate15 = 825,
		ShellNavigate16 = 826,
		ShellNavigate17 = 827,
		ShellNavigate18 = 828,
		ShellNavigate19 = 829,
		ShellNavigate20 = 830,
		ShellNavigate21 = 831,
		ShellNavigate22 = 832,
		ShellNavigate23 = 833,
		ShellNavigate24 = 834,
		ShellNavigate25 = 835,
		ShellNavigate26 = 836,
		ShellNavigate27 = 837,
		ShellNavigate28 = 838,
		ShellNavigate29 = 839,
		ShellNavigate30 = 840,
		ShellNavigate31 = 841,
		ShellNavigate32 = 842,
		ShellNavigate33 = 843,
		ShellWindowNavigate1 = 844,
		ShellWindowNavigate2 = 845,
		ShellWindowNavigate3 = 846,
		ShellWindowNavigate4 = 847,
		ShellWindowNavigate5 = 848,
		ShellWindowNavigate6 = 849,
		ShellWindowNavigate7 = 850,
		ShellWindowNavigate8 = 851,
		ShellWindowNavigate9 = 852,
		ShellWindowNavigate10 = 853,
		ShellWindowNavigate11 = 854,
		ShellWindowNavigate12 = 855,
		ShellWindowNavigate13 = 856,
		ShellWindowNavigate14 = 857,
		ShellWindowNavigate15 = 858,
		ShellWindowNavigate16 = 859,
		ShellWindowNavigate17 = 860,
		ShellWindowNavigate18 = 861,
		ShellWindowNavigate19 = 862,
		ShellWindowNavigate20 = 863,
		ShellWindowNavigate21 = 864,
		ShellWindowNavigate22 = 865,
		ShellWindowNavigate23 = 866,
		ShellWindowNavigate24 = 867,
		ShellWindowNavigate25 = 868,
		ShellWindowNavigate26 = 869,
		ShellWindowNavigate27 = 870,
		ShellWindowNavigate28 = 871,
		ShellWindowNavigate29 = 872,
		ShellWindowNavigate30 = 873,
		ShellWindowNavigate31 = 874,
		ShellWindowNavigate32 = 875,
		ShellWindowNavigate33 = 876,
		OBSDoFind = 877,
		OBSMatchCase = 878,
		OBSMatchSubString = 879,
		OBSMatchWholeWord = 880,
		OBSMatchPrefix = 881,
		BuildSln = 882,
		RebuildSln = 883,
		DeploySln = 884,
		CleanSln = 885,
		BuildSel = 886,
		RebuildSel = 887,
		DeploySel = 888,
		CleanSel = 889,
		CancelBuild = 890,
		BatchBuildDlg = 891,
		BuildCtx = 892,
		RebuildCtx = 893,
		DeployCtx = 894,
		CleanCtx = 895,
		QryManageIndexes = 896,
		PrintDefault = 897,
		BrowseDoc = 898,
		ShowStartPage = 899,
		MRUFile1 = 900,
		MRUFile2 = 901,
		MRUFile3 = 902,
		MRUFile4 = 903,
		MRUFile5 = 904,
		MRUFile6 = 905,
		MRUFile7 = 906,
		MRUFile8 = 907,
		MRUFile9 = 908,
		MRUFile10 = 909,
		MRUFile11 = 910,
		MRUFile12 = 911,
		MRUFile13 = 912,
		MRUFile14 = 913,
		MRUFile15 = 914,
		MRUFile16 = 915,
		MRUFile17 = 916,
		MRUFile18 = 917,
		MRUFile19 = 918,
		MRUFile20 = 919,
		MRUFile21 = 920,
		MRUFile22 = 921,
		MRUFile23 = 922,
		MRUFile24 = 923,
		MRUFile25 = 924,
		ExtToolsCurPath = 925,
		ExtToolsCurDir = 926,
		ExtToolsCurFileName = 927,
		ExtToolsCurExtension = 928,
		ExtToolsProjDir = 929,
		ExtToolsProjFileName = 930,
		ExtToolsSlnDir = 931,
		ExtToolsSlnFileName = 932,
		GotoDefn = 935,
		GotoDecl = 936,
		BrowseDefn = 937,
		SyncClassView = 938,
		ShowMembers = 939,
		ShowBases = 940,
		ShowDerived = 941,
		ShowDefns = 942,
		ShowRefs = 943,
		ShowCallers = 944,
		ShowCallees = 945,
		AddClass = 946,
		AddNestedClass = 947,
		AddInterface = 948,
		AddMethod = 949,
		AddProperty = 950,
		AddEvent = 951,
		AddVariable = 952,
		ImplementInterface = 953,
		Override = 954,
		AddFunction = 955,
		AddConnectionPoint = 956,
		AddIndexer = 957,
		BuildOrder = 958,
		OBShowHidden = 960,
		OBEnableGrouping = 961,
		OBSetGroupingCriteria = 962,
		OBBack = 963,
		OBForward = 964,
		OBShowPackages = 965,
		OBSearchCombo = 966,
		OBSearchOptWholeWord = 967,
		OBSearchOptSubstring = 968,
		OBSearchOptPrefix = 969,
		OBSearchOptCaseSensitive = 970,
		CVGroupingNone = 971,
		CVGroupingSortOnly = 972,
		CVGroupingGrouped = 973,
		CVShowPackages = 974,
		CVNewFolder = 975,
		CVGroupingSortAccess = 976,
		ObjectSearch = 977,
		ObjectSearchResults = 978,
		Build1 = 979,
		Build2 = 980,
		Build3 = 981,
		Build4 = 982,
		Build5 = 983,
		Build6 = 984,
		Build7 = 985,
		Build8 = 986,
		Build9 = 987,
		BuildLast = 988,
		Rebuild1 = 989,
		Rebuild2 = 990,
		Rebuild3 = 991,
		Rebuild4 = 992,
		Rebuild5 = 993,
		Rebuild6 = 994,
		Rebuild7 = 995,
		Rebuild8 = 996,
		Rebuild9 = 997,
		RebuildLast = 998,
		Clean1 = 999,
		Clean2 = 1000,
		Clean3 = 1001,
		Clean4 = 1002,
		Clean5 = 1003,
		Clean6 = 1004,
		Clean7 = 1005,
		Clean8 = 1006,
		Clean9 = 1007,
		CleanLast = 1008,
		Deploy1 = 1009,
		Deploy2 = 1010,
		Deploy3 = 1011,
		Deploy4 = 1012,
		Deploy5 = 1013,
		Deploy6 = 1014,
		Deploy7 = 1015,
		Deploy8 = 1016,
		Deploy9 = 1017,
		DeployLast = 1018,
		BuildProjPicker = 1019,
		RebuildProjPicker = 1020,
		CleanProjPicker = 1021,
		DeployProjPicker = 1022,
		ResourceView = 1023,
		ShowHomePage = 1024,
		EditMenuIDs = 1025,
		LineBreak = 1026,
		CPPIdentifier = 1027,
		QuotedString = 1028,
		SpaceOrTab = 1029,
		Integer = 1030,
		CustomizeToolbars = 1036,
		MoveToTop = 1037,
		WindowHelp = 1038,
		ViewPopup = 1039,
		CheckMnemonics = 1040,
		PRSortAlphabeticaly = 1041,
		PRSortByCategory = 1042,
		ViewNextTab = 1043,
		CheckForUpdates = 1044,
		Browser1 = 1045,
		Browser2 = 1046,
		Browser3 = 1047,
		Browser4 = 1048,
		Browser5 = 1049,
		Browser6 = 1050,
		Browser7 = 1051,
		Browser8 = 1052,
		Browser9 = 1053,
		Browser10 = 1054,
		Browser11 = 1055,
		OpenDropDownOpen = 1058,
		OpenDropDownOpenWith = 1059,
		ToolsDebugProcesses = 1060,
		PaneNextSubPane = 1062,
		PanePrevSubPane = 1063,
		MoveFileToProject1 = 1070,
		MoveFileToProject2 = 1071,
		MoveFileToProject3 = 1072,
		MoveFileToProject4 = 1073,
		MoveFileToProject5 = 1074,
		MoveFileToProject6 = 1075,
		MoveFileToProject7 = 1076,
		MoveFileToProject8 = 1077,
		MoveFileToProject9 = 1078,
		MoveFileToProjectLast = 1079,
		MoveFileToProjectPick = 1081,
		DefineSubset = 1095,
		SubsetCombo = 1096,
		SubsetGetList = 1097,
		OBSortObjectsAlpha = 1098,
		OBSortObjectsType = 1099,
		OBSortObjectsAccess = 1100,
		OBGroupObjectsType = 1101,
		OBGroupObjectsAccess = 1102,
		OBSortMembersAlpha = 1103,
		OBSortMembersType = 1104,
		OBSortMembersAccess = 1105,
		PopBrowseContext = 1106,
		GotoRef = 1107,
		OBSLookInReferences = 1108,
		ExtToolsTargetPath = 1109,
		ExtToolsTargetDir = 1110,
		ExtToolsTargetFileName = 1111,
		ExtToolsTargetExtension = 1112,
		ExtToolsCurLine = 1113,
		ExtToolsCurCol = 1114,
		ExtToolsCurText = 1115,
		BrowseNext = 1116,
		BrowsePrev = 1117,
		BrowseUnload = 1118,
		QuickObjectSearch = 1119,
		ExpandAll = 1120,
		ExtToolsBinDir = 1121,
		BookmarkWindow = 1122,
		CodeExpansionWindow = 1123,
		NextDocumentNav = 1124,
		PrevDocumentNav = 1125,
		ForwardBrowseContext = 1126,
		StandardMax = 1500,
		FindReferences = 1915,
		FormsFirst = 24576,
		FormsLast = 28671,
		VBEFirst = 32768,
		Zoom200 = 32770,
		Zoom150 = 32771,
		Zoom100 = 32772,
		Zoom75 = 32773,
		Zoom50 = 32774,
		Zoom25 = 32775,
		Zoom10 = 32784,
		VBELast = 40959,
		SterlingFirst = 40960,
		SterlingLast = 49151,
		uieventidFirst = 49152,
		uieventidSelectRegion = 49153,
		uieventidDrop = 49154,
		uieventidLast = 57343
	}

	[Guid("1496A755-94DE-11D0-8C3F-00C04FC2AAE2")]
	public enum VSStd2KCmdID
	{
		TYPECHAR = 1,
		BACKSPACE = 2,
		RETURN = 3,
		TAB = 4,
		ECMD_TAB = 4,
		BACKTAB = 5,
		DELETE = 6,
		LEFT = 7,
		LEFT_EXT = 8,
		RIGHT = 9,
		RIGHT_EXT = 10,
		UP = 11,
		UP_EXT = 12,
		DOWN = 13,
		DOWN_EXT = 14,
		HOME = 15,
		HOME_EXT = 16,
		END = 17,
		END_EXT = 18,
		BOL = 19,
		BOL_EXT = 20,
		FIRSTCHAR = 21,
		FIRSTCHAR_EXT = 22,
		EOL = 23,
		EOL_EXT = 24,
		LASTCHAR = 25,
		LASTCHAR_EXT = 26,
		PAGEUP = 27,
		PAGEUP_EXT = 28,
		PAGEDN = 29,
		PAGEDN_EXT = 30,
		TOPLINE = 31,
		TOPLINE_EXT = 32,
		BOTTOMLINE = 33,
		BOTTOMLINE_EXT = 34,
		SCROLLUP = 35,
		SCROLLDN = 36,
		SCROLLPAGEUP = 37,
		SCROLLPAGEDN = 38,
		SCROLLLEFT = 39,
		SCROLLRIGHT = 40,
		SCROLLBOTTOM = 41,
		SCROLLCENTER = 42,
		SCROLLTOP = 43,
		SELECTALL = 44,
		SELTABIFY = 45,
		SELUNTABIFY = 46,
		SELLOWCASE = 47,
		SELUPCASE = 48,
		SELTOGGLECASE = 49,
		SELTITLECASE = 50,
		SELSWAPANCHOR = 51,
		GOTOLINE = 52,
		GOTOBRACE = 53,
		GOTOBRACE_EXT = 54,
		GOBACK = 55,
		SELECTMODE = 56,
		TOGGLE_OVERTYPE_MODE = 57,
		CUT = 58,
		COPY = 59,
		PASTE = 60,
		CUTLINE = 61,
		DELETELINE = 62,
		DELETEBLANKLINES = 63,
		DELETEWHITESPACE = 64,
		DELETETOEOL = 65,
		DELETETOBOL = 66,
		OPENLINEABOVE = 67,
		OPENLINEBELOW = 68,
		INDENT = 69,
		UNINDENT = 70,
		UNDO = 71,
		UNDONOMOVE = 72,
		REDO = 73,
		REDONOMOVE = 74,
		DELETEALLTEMPBOOKMARKS = 75,
		TOGGLETEMPBOOKMARK = 76,
		GOTONEXTBOOKMARK = 77,
		GOTOPREVBOOKMARK = 78,
		FIND = 79,
		REPLACE = 80,
		REPLACE_ALL = 81,
		FINDNEXT = 82,
		FINDNEXTWORD = 83,
		FINDPREV = 84,
		FINDPREVWORD = 85,
		FINDAGAIN = 86,
		TRANSPOSECHAR = 87,
		TRANSPOSEWORD = 88,
		TRANSPOSELINE = 89,
		SELECTCURRENTWORD = 90,
		DELETEWORDRIGHT = 91,
		DELETEWORDLEFT = 92,
		WORDPREV = 93,
		WORDPREV_EXT = 94,
		WORDNEXT = 96,
		WORDNEXT_EXT = 97,
		COMMENTBLOCK = 98,
		UNCOMMENTBLOCK = 99,
		SETREPEATCOUNT = 100,
		WIDGETMARGIN_LBTNDOWN = 101,
		SHOWCONTEXTMENU = 102,
		CANCEL = 103,
		PARAMINFO = 104,
		TOGGLEVISSPACE = 105,
		TOGGLECARETPASTEPOS = 106,
		COMPLETEWORD = 107,
		SHOWMEMBERLIST = 108,
		FIRSTNONWHITEPREV = 109,
		FIRSTNONWHITENEXT = 110,
		HELPKEYWORD = 111,
		FORMATSELECTION = 112,
		OPENURL = 113,
		INSERTFILE = 114,
		TOGGLESHORTCUT = 115,
		QUICKINFO = 116,
		LEFT_EXT_COL = 117,
		RIGHT_EXT_COL = 118,
		UP_EXT_COL = 119,
		DOWN_EXT_COL = 120,
		TOGGLEWORDWRAP = 121,
		ISEARCH = 122,
		ISEARCHBACK = 123,
		BOL_EXT_COL = 124,
		EOL_EXT_COL = 125,
		WORDPREV_EXT_COL = 126,
		WORDNEXT_EXT_COL = 127,
		OUTLN_HIDE_SELECTION = 128,
		OUTLN_TOGGLE_CURRENT = 129,
		OUTLN_TOGGLE_ALL = 130,
		OUTLN_STOP_HIDING_ALL = 131,
		OUTLN_STOP_HIDING_CURRENT = 132,
		OUTLN_COLLAPSE_TO_DEF = 133,
		DOUBLECLICK = 134,
		EXTERNALLY_HANDLED_WIDGET_CLICK = 135,
		COMMENT_BLOCK = 136,
		UNCOMMENT_BLOCK = 137,
		OPENFILE = 138,
		NAVIGATETOURL = 139,
		HANDLEIMEMESSAGE = 140,
		SELTOGOBACK = 141,
		COMPLETION_HIDE_ADVANCED = 142,
		FORMATDOCUMENT = 143,
		OUTLN_START_AUTOHIDING = 144,
		FINAL = 145,
		ECMD_DECREASEFILTER = 146,
		ECMD_COPYTIP = 148,
		ECMD_PASTETIP = 149,
		ECMD_LEFTCLICK = 150,
		ECMD_GOTONEXTBOOKMARKINDOC = 151,
		ECMD_GOTOPREVBOOKMARKINDOC = 152,
		ECMD_INVOKESNIPPETFROMSHORTCUT = 154,
		AUTOCOMPLETE = 155,
		ECMD_INVOKESNIPPETPICKER2 = 156,
		ECMD_DELETEALLBOOKMARKSINDOC = 157,
		ECMD_CONVERTTABSTOSPACES = 158,
		ECMD_CONVERTSPACESTOTABS = 159,
		ECMD_FINAL = 160,
		STOP = 220,
		REVERSECANCEL = 221,
		SLNREFRESH = 222,
		SAVECOPYOFITEMAS = 223,
		NEWELEMENT = 224,
		NEWATTRIBUTE = 225,
		NEWCOMPLEXTYPE = 226,
		NEWSIMPLETYPE = 227,
		NEWGROUP = 228,
		NEWATTRIBUTEGROUP = 229,
		NEWKEY = 230,
		NEWRELATION = 231,
		EDITKEY = 232,
		EDITRELATION = 233,
		MAKETYPEGLOBAL = 234,
		PREVIEWDATASET = 235,
		GENERATEDATASET = 236,
		CREATESCHEMA = 237,
		LAYOUTINDENT = 238,
		LAYOUTUNINDENT = 239,
		REMOVEHANDLER = 240,
		EDITHANDLER = 241,
		ADDHANDLER = 242,
		STYLE = 243,
		STYLEGETLIST = 244,
		FONTSTYLE = 245,
		FONTSTYLEGETLIST = 246,
		PASTEASHTML = 247,
		VIEWBORDERS = 248,
		VIEWDETAILS = 249,
		EXPANDCONTROLS = 250,
		COLLAPSECONTROLS = 251,
		SHOWSCRIPTONLY = 252,
		INSERTTABLE = 253,
		INSERTCOLLEFT = 254,
		INSERTCOLRIGHT = 255,
		INSERTROWABOVE = 256,
		INSERTROWBELOW = 257,
		DELETETABLE = 258,
		DELETECOLS = 259,
		DELETEROWS = 260,
		SELECTTABLE = 261,
		SELECTTABLECOL = 262,
		SELECTTABLEROW = 263,
		SELECTTABLECELL = 264,
		MERGECELLS = 265,
		SPLITCELL = 266,
		INSERTCELL = 267,
		DELETECELLS = 268,
		SEAMLESSFRAME = 269,
		VIEWFRAME = 270,
		DELETEFRAME = 271,
		SETFRAMESOURCE = 272,
		NEWLEFTFRAME = 273,
		NEWRIGHTFRAME = 274,
		NEWTOPFRAME = 275,
		NEWBOTTOMFRAME = 276,
		SHOWGRID = 277,
		SNAPTOGRID = 278,
		BOOKMARK = 279,
		HYPERLINK = 280,
		IMAGE = 281,
		INSERTFORM = 282,
		INSERTSPAN = 283,
		DIV = 284,
		HTMLCLIENTSCRIPTBLOCK = 285,
		HTMLSERVERSCRIPTBLOCK = 286,
		BULLETEDLIST = 287,
		NUMBEREDLIST = 288,
		EDITSCRIPT = 289,
		EDITCODEBEHIND = 290,
		DOCOUTLINEHTML = 291,
		DOCOUTLINESCRIPT = 292,
		RUNATSERVER = 293,
		WEBFORMSVERBS = 294,
		WEBFORMSTEMPLATES = 295,
		ENDTEMPLATE = 296,
		EDITDEFAULTEVENT = 297,
		SUPERSCRIPT = 298,
		SUBSCRIPT = 299,
		EDITSTYLE = 300,
		ADDIMAGEHEIGHTWIDTH = 301,
		REMOVEIMAGEHEIGHTWIDTH = 302,
		LOCKELEMENT = 303,
		VIEWSTYLEORGANIZER = 304,
		ECMD_AUTOCLOSEOVERRIDE = 305,
		NEWANY = 306,
		NEWANYATTRIBUTE = 307,
		DELETEKEY = 308,
		AUTOARRANGE = 309,
		VALIDATESCHEMA = 310,
		NEWFACET = 311,
		VALIDATEXMLDATA = 312,
		DOCOUTLINETOGGLE = 313,
		VALIDATEHTMLDATA = 314,
		VIEWXMLSCHEMAOVERVIEW = 315,
		SHOWDEFAULTVIEW = 316,
		EXPAND_CHILDREN = 317,
		COLLAPSE_CHILDREN = 318,
		TOPDOWNLAYOUT = 319,
		LEFTRIGHTLAYOUT = 320,
		INSERTCELLRIGHT = 321,
		EDITMASTER = 322,
		INSERTSNIPPET = 323,
		FORMATANDVALIDATION = 324,
		COLLAPSETAG = 325,
		SELECT_TAG = 329,
		SELECT_TAG_CONTENT = 330,
		CHECK_ACCESSIBILITY = 331,
		UNCOLLAPSETAG = 332,
		GENERATEPAGERESOURCE = 333,
		SHOWNONVISUALCONTROLS = 334,
		RESIZECOLUMN = 335,
		RESIZEROW = 336,
		MAKEABSOLUTE = 337,
		MAKERELATIVE = 338,
		MAKESTATIC = 339,
		INSERTLAYER = 340,
		UPDATEDESIGNVIEW = 341,
		UPDATESOURCEVIEW = 342,
		INSERTCAPTION = 343,
		DELETECAPTION = 344,
		MAKEPOSITIONNOTSET = 345,
		AUTOPOSITIONOPTIONS = 346,
		EDITIMAGE = 347,
		COMPILE = 350,
		PROJSETTINGS = 352,
		LINKONLY = 353,
		REMOVE = 355,
		PROJSTARTDEBUG = 356,
		PROJSTEPINTO = 357,
		ECMD_UPDATEMGDRES = 358,
		UPDATEWEBREF = 360,
		ADDRESOURCE = 362,
		WEBDEPLOY = 363,
		ECMD_PROJTOOLORDER = 367,
		ECMD_PROJECTTOOLFILES = 368,
		ECMD_OTB_PGO_INSTRUMENT = 369,
		ECMD_OTB_PGO_OPT = 370,
		ECMD_OTB_PGO_UPDATE = 371,
		ECMD_OTB_PGO_RUNSCENARIO = 372,
		ADDHTMLPAGE = 400,
		ADDHTMLPAGECTX = 401,
		ADDMODULE = 402,
		ADDMODULECTX = 403,
		ADDWFCFORM = 406,
		ADDWEBFORM = 410,
		ECMD_ADDMASTERPAGE = 411,
		ADDUSERCONTROL = 412,
		ECMD_ADDCONTENTPAGE = 413,
		ADDDHTMLPAGE = 426,
		ADDIMAGEGENERATOR = 432,
		ADDINHERWFCFORM = 434,
		ADDINHERCONTROL = 436,
		ADDWEBUSERCONTROL = 438,
		BUILDANDBROWSE = 439,
		ADDTBXCOMPONENT = 442,
		ADDWEBSERVICE = 444,
		ECMD_ADDSTYLESHEET = 445,
		ECMD_SETBROWSELOCATION = 446,
		ECMD_REFRESHFOLDER = 447,
		ECMD_SETBROWSELOCATIONCTX = 448,
		ECMD_VIEWMARKUP = 449,
		ECMD_NEXTMETHOD = 450,
		ECMD_PREVMETHOD = 451,
		ECMD_RENAMESYMBOL = 452,
		ECMD_SHOWREFERENCES = 453,
		ECMD_CREATESNIPPET = 454,
		ECMD_CREATEREPLACEMENT = 455,
		ECMD_INSERTCOMMENT = 456,
		VIEWCOMPONENTDESIGNER = 457,
		GOTOTYPEDEF = 458,
		SHOWSNIPPETHIGHLIGHTING = 459,
		HIDESNIPPETHIGHLIGHTING = 460,
		ADDVFPPAGE = 500,
		SETBREAKPOINT = 501,
		SHOWALLFILES = 600,
		ADDTOPROJECT = 601,
		ADDBLANKNODE = 602,
		ADDNODEFROMFILE = 603,
		CHANGEURLFROMFILE = 604,
		EDITTOPIC = 605,
		EDITTITLE = 606,
		MOVENODEUP = 607,
		MOVENODEDOWN = 608,
		MOVENODELEFT = 609,
		MOVENODERIGHT = 610,
		ADDOUTPUT = 700,
		ADDFILE = 701,
		MERGEMODULE = 702,
		ADDCOMPONENTS = 703,
		LAUNCHINSTALLER = 704,
		LAUNCHUNINSTALL = 705,
		LAUNCHORCA = 706,
		FILESYSTEMEDITOR = 707,
		REGISTRYEDITOR = 708,
		FILETYPESEDITOR = 709,
		USERINTERFACEEDITOR = 710,
		CUSTOMACTIONSEDITOR = 711,
		LAUNCHCONDITIONSEDITOR = 712,
		EDITOR = 713,
		EXCLUDE = 714,
		REFRESHDEPENDENCIES = 715,
		VIEWOUTPUTS = 716,
		VIEWDEPENDENCIES = 717,
		VIEWFILTER = 718,
		KEY = 750,
		STRING = 751,
		BINARY = 752,
		DWORD = 753,
		KEYSOLO = 754,
		IMPORT = 755,
		FOLDER = 756,
		PROJECTOUTPUT = 757,
		FILE = 758,
		ADDMERGEMODULES = 759,
		CREATESHORTCUT = 760,
		LARGEICONS = 761,
		SMALLICONS = 762,
		LIST = 763,
		DETAILS = 764,
		ADDFILETYPE = 765,
		ADDACTION = 766,
		SETASDEFAULT = 767,
		MOVEUP = 768,
		MOVEDOWN = 769,
		ADDDIALOG = 770,
		IMPORTDIALOG = 771,
		ADDFILESEARCH = 772,
		ADDREGISTRYSEARCH = 773,
		ADDCOMPONENTSEARCH = 774,
		ADDLAUNCHCONDITION = 775,
		ADDCUSTOMACTION = 776,
		OUTPUTS = 777,
		DEPENDENCIES = 778,
		FILTER = 779,
		COMPONENTS = 780,
		ENVSTRING = 781,
		CREATEEMPTYSHORTCUT = 782,
		ADDFILECONDITION = 783,
		ADDREGISTRYCONDITION = 784,
		ADDCOMPONENTCONDITION = 785,
		ADDURTCONDITION = 786,
		ADDIISCONDITION = 787,
		SPECIALFOLDERBASE = 800,
		USERSAPPLICATIONDATAFOLDER = 800,
		COMMONFILES64FOLDER = 801,
		COMMONFILESFOLDER = 802,
		CUSTOMFOLDER = 803,
		USERSDESKTOP = 804,
		USERSFAVORITESFOLDER = 805,
		FONTSFOLDER = 806,
		GLOBALASSEMBLYCACHEFOLDER = 807,
		MODULERETARGETABLEFOLDER = 808,
		USERSPERSONALDATAFOLDER = 809,
		PROGRAMFILES64FOLDER = 810,
		PROGRAMFILESFOLDER = 811,
		USERSPROGRAMSMENU = 812,
		USERSSENDTOMENU = 813,
		SHAREDCOMPONENTSFOLDER = 814,
		USERSSTARTMENU = 815,
		USERSSTARTUPFOLDER = 816,
		SYSTEM64FOLDER = 817,
		SYSTEMFOLDER = 818,
		APPLICATIONFOLDER = 819,
		USERSTEMPLATEFOLDER = 820,
		WEBCUSTOMFOLDER = 821,
		WINDOWSFOLDER = 822,
		SPECIALFOLDERLAST = 823,
		EXPORTEVENTS = 900,
		IMPORTEVENTS = 901,
		VIEWEVENT = 902,
		VIEWEVENTLIST = 903,
		VIEWCHART = 904,
		VIEWMACHINEDIAGRAM = 905,
		VIEWPROCESSDIAGRAM = 906,
		VIEWSOURCEDIAGRAM = 907,
		VIEWSTRUCTUREDIAGRAM = 908,
		VIEWTIMELINE = 909,
		VIEWSUMMARY = 910,
		APPLYFILTER = 911,
		CLEARFILTER = 912,
		STARTRECORDING = 913,
		STOPRECORDING = 914,
		PAUSERECORDING = 915,
		ACTIVATEFILTER = 916,
		SHOWFIRSTEVENT = 917,
		SHOWPREVIOUSEVENT = 918,
		SHOWNEXTEVENT = 919,
		SHOWLASTEVENT = 920,
		REPLAYEVENTS = 921,
		STOPREPLAY = 922,
		INCREASEPLAYBACKSPEED = 923,
		DECREASEPLAYBACKSPEED = 924,
		ADDMACHINE = 925,
		ADDREMOVECOLUMNS = 926,
		SORTCOLUMNS = 927,
		SAVECOLUMNSETTINGS = 928,
		RESETCOLUMNSETTINGS = 929,
		SIZECOLUMNSTOFIT = 930,
		AUTOSELECT = 931,
		AUTOFILTER = 932,
		AUTOPLAYTRACK = 933,
		GOTOEVENT = 934,
		ZOOMTOFIT = 935,
		ADDGRAPH = 936,
		REMOVEGRAPH = 937,
		CONNECTMACHINE = 938,
		DISCONNECTMACHINE = 939,
		EXPANDSELECTION = 940,
		COLLAPSESELECTION = 941,
		ADDFILTER = 942,
		ADDPREDEFINED0 = 943,
		ADDPREDEFINED1 = 944,
		ADDPREDEFINED2 = 945,
		ADDPREDEFINED3 = 946,
		ADDPREDEFINED4 = 947,
		ADDPREDEFINED5 = 948,
		ADDPREDEFINED6 = 949,
		ADDPREDEFINED7 = 950,
		ADDPREDEFINED8 = 951,
		TIMELINESIZETOFIT = 952,
		FIELDVIEW = 1000,
		SELECTEXPERT = 1001,
		TOPNEXPERT = 1002,
		SORTORDER = 1003,
		PROPPAGE = 1004,
		HELP = 1005,
		SAVEREPORT = 1006,
		INSERTSUMMARY = 1007,
		INSERTGROUP = 1008,
		INSERTSUBREPORT = 1009,
		INSERTCHART = 1010,
		INSERTPICTURE = 1011,
		SETASSTARTPAGE = 1100,
		RECALCULATELINKS = 1101,
		WEBPERMISSIONS = 1102,
		COMPARETOMASTER = 1103,
		WORKOFFLINE = 1104,
		SYNCHRONIZEFOLDER = 1105,
		SYNCHRONIZEALLFOLDERS = 1106,
		COPYPROJECT = 1107,
		IMPORTFILEFROMWEB = 1108,
		INCLUDEINPROJECT = 1109,
		EXCLUDEFROMPROJECT = 1110,
		BROKENLINKSREPORT = 1111,
		ADDPROJECTOUTPUTS = 1112,
		ADDREFERENCE = 1113,
		ADDWEBREFERENCE = 1114,
		ADDWEBREFERENCECTX = 1115,
		UPDATEWEBREFERENCE = 1116,
		RUNCUSTOMTOOL = 1117,
		SETRUNTIMEVERSION = 1118,
		VIEWREFINOBJECTBROWSER = 1119,
		PUBLISH = 1120,
		PUBLISHCTX = 1121,
		STARTOPTIONS = 1124,
		ADDREFERENCECTX = 1125,
		STARTOPTIONSCTX = 1127,
		DETACHLOCALDATAFILECTX = 1128,
		ADDSERVICEREFERENCE = 1129,
		ADDSERVICEREFERENCECTX = 1130,
		UPDATESERVICEREFERENCE = 1131,
		CONFIGURESERVICEREFERENCE = 1132,
		DRAG_MOVE = 1140,
		DRAG_COPY = 1141,
		DRAG_CANCEL = 1142,
		TESTDIALOG = 1200,
		SPACEACROSS = 1201,
		SPACEDOWN = 1202,
		TOGGLEGRID = 1203,
		TOGGLEGUIDES = 1204,
		SIZETOTEXT = 1205,
		CENTERVERT = 1206,
		CENTERHORZ = 1207,
		FLIPDIALOG = 1208,
		SETTABORDER = 1209,
		BUTTONRIGHT = 1210,
		BUTTONBOTTOM = 1211,
		AUTOLAYOUTGROW = 1212,
		AUTOLAYOUTNORESIZE = 1213,
		AUTOLAYOUTOPTIMIZE = 1214,
		GUIDESETTINGS = 1215,
		RESOURCEINCLUDES = 1216,
		RESOURCESYMBOLS = 1217,
		OPENBINARY = 1218,
		RESOURCEOPEN = 1219,
		RESOURCENEW = 1220,
		RESOURCENEWCOPY = 1221,
		INSERT = 1222,
		EXPORT = 1223,
		CTLMOVELEFT = 1224,
		CTLMOVEDOWN = 1225,
		CTLMOVERIGHT = 1226,
		CTLMOVEUP = 1227,
		CTLSIZEDOWN = 1228,
		CTLSIZEUP = 1229,
		CTLSIZELEFT = 1230,
		CTLSIZERIGHT = 1231,
		NEWACCELERATOR = 1232,
		CAPTUREKEYSTROKE = 1233,
		INSERTACTIVEXCTL = 1234,
		INVERTCOLORS = 1235,
		FLIPHORIZONTAL = 1236,
		FLIPVERTICAL = 1237,
		ROTATE90 = 1238,
		SHOWCOLORSWINDOW = 1239,
		NEWSTRING = 1240,
		NEWINFOBLOCK = 1241,
		DELETEINFOBLOCK = 1242,
		ADJUSTCOLORS = 1243,
		LOADPALETTE = 1244,
		SAVEPALETTE = 1245,
		CHECKMNEMONICS = 1246,
		DRAWOPAQUE = 1247,
		TOOLBAREDITOR = 1248,
		GRIDSETTINGS = 1249,
		NEWDEVICEIMAGE = 1250,
		OPENDEVICEIMAGE = 1251,
		DELETEDEVICEIMAGE = 1252,
		VIEWASPOPUP = 1253,
		CHECKMENUMNEMONICS = 1254,
		SHOWIMAGEGRID = 1255,
		SHOWTILEGRID = 1256,
		MAGNIFY = 1257,
		ResProps = 1258,
		IMPORTICONIMAGE = 1259,
		EXPORTICONIMAGE = 1260,
		OPENEXTERNALEDITOR = 1261,
		PICKRECTANGLE = 1300,
		PICKREGION = 1301,
		PICKCOLOR = 1302,
		ERASERTOOL = 1303,
		FILLTOOL = 1304,
		PENCILTOOL = 1305,
		BRUSHTOOL = 1306,
		AIRBRUSHTOOL = 1307,
		LINETOOL = 1308,
		CURVETOOL = 1309,
		TEXTTOOL = 1310,
		RECTTOOL = 1311,
		OUTLINERECTTOOL = 1312,
		FILLEDRECTTOOL = 1313,
		ROUNDRECTTOOL = 1314,
		OUTLINEROUNDRECTTOOL = 1315,
		FILLEDROUNDRECTTOOL = 1316,
		ELLIPSETOOL = 1317,
		OUTLINEELLIPSETOOL = 1318,
		FILLEDELLIPSETOOL = 1319,
		SETHOTSPOT = 1320,
		ZOOMTOOL = 1321,
		ZOOM1X = 1322,
		ZOOM2X = 1323,
		ZOOM6X = 1324,
		ZOOM8X = 1325,
		TRANSPARENTBCKGRND = 1326,
		OPAQUEBCKGRND = 1327,
		ERASERSMALL = 1328,
		ERASERMEDIUM = 1329,
		ERASERLARGE = 1330,
		ERASERLARGER = 1331,
		CIRCLELARGE = 1332,
		CIRCLEMEDIUM = 1333,
		CIRCLESMALL = 1334,
		SQUARELARGE = 1335,
		SQUAREMEDIUM = 1336,
		SQUARESMALL = 1337,
		LEFTDIAGLARGE = 1338,
		LEFTDIAGMEDIUM = 1339,
		LEFTDIAGSMALL = 1340,
		RIGHTDIAGLARGE = 1341,
		RIGHTDIAGMEDIUM = 1342,
		RIGHTDIAGSMALL = 1343,
		SPLASHSMALL = 1344,
		SPLASHMEDIUM = 1345,
		SPLASHLARGE = 1346,
		LINESMALLER = 1347,
		LINESMALL = 1348,
		LINEMEDIUM = 1349,
		LINELARGE = 1350,
		LINELARGER = 1351,
		LARGERBRUSH = 1352,
		LARGEBRUSH = 1353,
		STDBRUSH = 1354,
		SMALLBRUSH = 1355,
		SMALLERBRUSH = 1356,
		ZOOMIN = 1357,
		ZOOMOUT = 1358,
		PREVCOLOR = 1359,
		PREVECOLOR = 1360,
		NEXTCOLOR = 1361,
		NEXTECOLOR = 1362,
		IMG_OPTIONS = 1363,
		STARTWEBADMINTOOL = 1400,
		NESTRELATEDFILES = 1401,
		CANCELDRAG = 1500,
		DEFAULTACTION = 1501,
		CTLMOVEUPGRID = 1502,
		CTLMOVEDOWNGRID = 1503,
		CTLMOVELEFTGRID = 1504,
		CTLMOVERIGHTGRID = 1505,
		CTLSIZERIGHTGRID = 1506,
		CTLSIZEUPGRID = 1507,
		CTLSIZELEFTGRID = 1508,
		CTLSIZEDOWNGRID = 1509,
		NEXTCTL = 1510,
		PREVCTL = 1511,
		RENAME = 1550,
		EXTRACTMETHOD = 1551,
		ENCAPSULATEFIELD = 1552,
		EXTRACTINTERFACE = 1553,
		PROMOTELOCAL = 1554,
		REMOVEPARAMETERS = 1555,
		REORDERPARAMETERS = 1556,
		GENERATEMETHODSTUB = 1557,
		IMPLEMENTINTERFACEIMPLICIT = 1558,
		IMPLEMENTINTERFACEEXPLICIT = 1559,
		IMPLEMENTABSTRACTCLASS = 1560,
		SURROUNDWITH = 1561,
		QUICKOBJECTSEARCH = 1119,
		ToggleWordWrapOW = 1600,
		GotoNextLocationOW = 1601,
		GotoPrevLocationOW = 1602,
		BuildOnlyProject = 1603,
		RebuildOnlyProject = 1604,
		CleanOnlyProject = 1605,
		SetBuildStartupsOnlyOnRun = 1606,
		UnhideAll = 1607,
		HideFolder = 1608,
		UnhideFolders = 1609,
		CopyFullPathName = 1610,
		SaveFolderAsSolution = 1611,
		ManageUserSettings = 1612,
		NewSolutionFolder = 1613,
		ClearPaneOW = 1615,
		GotoErrorTagOW = 1616,
		GotoNextErrorTagOW = 1617,
		GotoPrevErrorTagOW = 1618,
		ClearPaneFR1 = 1619,
		GotoErrorTagFR1 = 1620,
		GotoNextErrorTagFR1 = 1621,
		GotoPrevErrorTagFR1 = 1622,
		ClearPaneFR2 = 1623,
		GotoErrorTagFR2 = 1624,
		GotoNextErrorTagFR2 = 1625,
		GotoPrevErrorTagFR2 = 1626,
		OutputPaneCombo = 1627,
		OutputPaneComboList = 1628,
		DisableDockingChanges = 1629,
		ToggleFloat = 1630,
		ResetLayout = 1631,
		EditProjectFile = 1632,
		OpenInFormView = 1633,
		OpenInCodeView = 1634,
		ExploreFolderInWindows = 1635,
		NewSolutionFolderBar = 1638,
		DataShortcut = 1639,
		NextToolWindow = 1640,
		PrevToolWindow = 1641,
		BrowseToFileInExplorer = 1642,
		ShowEzMDIFileMenu = 1643,
		PrevToolWindowNav = 1645,
		StaticAnalysisOnlyProject = 1646,
		ECMD_RUNFXCOPSEL = 1647,
		CloseAllButThis = 1650,
		CVShowInheritedMembers = 1651,
		CVShowBaseTypes = 1652,
		CVShowDerivedTypes = 1653,
		CVShowHidden = 1654,
		CVBack = 1655,
		CVForward = 1656,
		CVSearchCombo = 1657,
		CVSearch = 1658,
		CVSortObjectsAlpha = 1659,
		CVSortObjectsType = 1660,
		CVSortObjectsAccess = 1661,
		CVGroupObjectsType = 1662,
		CVSortMembersAlpha = 1663,
		CVSortMembersType = 1664,
		CVSortMembersAccess = 1665,
		CVTypeBrowserSettings = 1666,
		CVViewMembersAsImplementor = 1667,
		CVViewMembersAsSubclass = 1668,
		CVViewMembersAsUser = 1669,
		CVReserved1 = 1670,
		CVReserved2 = 1671,
		CVShowProjectReferences = 1672,
		CVGroupMembersType = 1673,
		CVClearSearch = 1674,
		CVFilterToType = 1675,
		CVSortByBestMatch = 1676,
		CVSearchMRUList = 1677,
		CVViewOtherMembers = 1678,
		CVSearchCmd = 1679,
		CVGoToSearchCmd = 1680,
		ControlGallery = 1700,
		OBShowInheritedMembers = 1711,
		OBShowBaseTypes = 1712,
		OBShowDerivedTypes = 1713,
		OBShowHidden = 1714,
		OBBack = 1715,
		OBForward = 1716,
		OBSearchCombo = 1717,
		OBSearch = 1718,
		OBSortObjectsAlpha = 1719,
		OBSortObjectsType = 1720,
		OBSortObjectsAccess = 1721,
		OBGroupObjectsType = 1722,
		OBSortMembersAlpha = 1723,
		OBSortMembersType = 1724,
		OBSortMembersAccess = 1725,
		OBTypeBrowserSettings = 1726,
		OBViewMembersAsImplementor = 1727,
		OBViewMembersAsSubclass = 1728,
		OBViewMembersAsUser = 1729,
		OBNamespacesView = 1730,
		OBContainersView = 1731,
		OBReserved1 = 1732,
		OBGroupMembersType = 1733,
		OBClearSearch = 1734,
		OBFilterToType = 1735,
		OBSortByBestMatch = 1736,
		OBSearchMRUList = 1737,
		OBViewOtherMembers = 1738,
		OBSearchCmd = 1739,
		OBGoToSearchCmd = 1740,
		OBShowExtensionMembers = 1741,
		FullScreen2 = 1775,
		FSRSortObjectsAlpha = 1776,
		FSRSortByBestMatch = 1777,
		NavigateBack = 1800,
		NavigateForward = 1801,
		ECMD_CORRECTION_1 = 1900,
		ECMD_CORRECTION_2 = 1901,
		ECMD_CORRECTION_3 = 1902,
		ECMD_CORRECTION_4 = 1903,
		ECMD_CORRECTION_5 = 1904,
		ECMD_CORRECTION_6 = 1905,
		ECMD_CORRECTION_7 = 1906,
		ECMD_CORRECTION_8 = 1907,
		ECMD_CORRECTION_9 = 1908,
		ECMD_CORRECTION_10 = 1909,
		OBAddReference = 1914,
		[Obsolete("VSStd2KCmdID.FindReferences has been deprecated; please use VSStd97CmdID.FindReferences instead.", false)]
		FindReferences = 1915,
		CodeDefView = 1926,
		CodeDefViewGoToPrev = 1927,
		CodeDefViewGoToNext = 1928,
		CodeDefViewEditDefinition = 1929,
		CodeDefViewChooseEncoding = 1930,
		ViewInClassDiagram = 1931,
		ECMD_ADDDBTABLE = 1950,
		ECMD_ADDDATATABLE = 1951,
		ECMD_ADDFUNCTION = 1952,
		ECMD_ADDRELATION = 1953,
		ECMD_ADDKEY = 1954,
		ECMD_ADDCOLUMN = 1955,
		ECMD_CONVERT_DBTABLE = 1956,
		ECMD_CONVERT_DATATABLE = 1957,
		ECMD_GENERATE_DATABASE = 1958,
		ECMD_CONFIGURE_CONNECTIONS = 1959,
		ECMD_IMPORT_XMLSCHEMA = 1960,
		ECMD_SYNC_WITH_DATABASE = 1961,
		ECMD_CONFIGURE = 1962,
		ECMD_CREATE_DATAFORM = 1963,
		ECMD_CREATE_ENUM = 1964,
		ECMD_INSERT_FUNCTION = 1965,
		ECMD_EDIT_FUNCTION = 1966,
		ECMD_SET_PRIMARY_KEY = 1967,
		ECMD_INSERT_COLUMN = 1968,
		ECMD_AUTO_SIZE = 1969,
		ECMD_SHOW_RELATION_LABELS = 1970,
		VSDGenerateDataSet = 1971,
		VSDPreview = 1972,
		VSDConfigureAdapter = 1973,
		VSDViewDatasetSchema = 1974,
		VSDDatasetProperties = 1975,
		VSDParameterizeForm = 1976,
		VSDAddChildForm = 1977,
		ECMD_EDITCONSTRAINT = 1978,
		ECMD_DELETECONSTRAINT = 1979,
		ECMD_EDITDATARELATION = 1980,
		CloseProject = 1982,
		ReloadCommandBars = 1983,
		SolutionPlatform = 1990,
		SolutionPlatformGetList = 1991,
		ECMD_DATAACCESSOR = 2000,
		ECMD_ADD_DATAACCESSOR = 2001,
		ECMD_QUERY = 2002,
		ECMD_ADD_QUERY = 2003,
		ECMD_PUBLISHSELECTION = 2005,
		ECMD_PUBLISHSLNCTX = 2006,
		CallBrowserShowCallsTo = 2010,
		CallBrowserShowCallsFrom = 2011,
		CallBrowserShowNewCallsTo = 2012,
		CallBrowserShowNewCallsFrom = 2013,
		CallBrowser1ShowCallsTo = 2014,
		CallBrowser2ShowCallsTo = 2015,
		CallBrowser3ShowCallsTo = 2016,
		CallBrowser4ShowCallsTo = 2017,
		CallBrowser5ShowCallsTo = 2018,
		CallBrowser6ShowCallsTo = 2019,
		CallBrowser7ShowCallsTo = 2020,
		CallBrowser8ShowCallsTo = 2021,
		CallBrowser9ShowCallsTo = 2022,
		CallBrowser10ShowCallsTo = 2023,
		CallBrowser11ShowCallsTo = 2024,
		CallBrowser12ShowCallsTo = 2025,
		CallBrowser13ShowCallsTo = 2026,
		CallBrowser14ShowCallsTo = 2027,
		CallBrowser15ShowCallsTo = 2028,
		CallBrowser16ShowCallsTo = 2029,
		CallBrowser1ShowCallsFrom = 2030,
		CallBrowser2ShowCallsFrom = 2031,
		CallBrowser3ShowCallsFrom = 2032,
		CallBrowser4ShowCallsFrom = 2033,
		CallBrowser5ShowCallsFrom = 2034,
		CallBrowser6ShowCallsFrom = 2035,
		CallBrowser7ShowCallsFrom = 2036,
		CallBrowser8ShowCallsFrom = 2037,
		CallBrowser9ShowCallsFrom = 2038,
		CallBrowser10ShowCallsFrom = 2039,
		CallBrowser11ShowCallsFrom = 2040,
		CallBrowser12ShowCallsFrom = 2041,
		CallBrowser13ShowCallsFrom = 2042,
		CallBrowser14ShowCallsFrom = 2043,
		CallBrowser15ShowCallsFrom = 2044,
		CallBrowser16ShowCallsFrom = 2045,
		CallBrowser1ShowFullNames = 2046,
		CallBrowser2ShowFullNames = 2047,
		CallBrowser3ShowFullNames = 2048,
		CallBrowser4ShowFullNames = 2049,
		CallBrowser5ShowFullNames = 2050,
		CallBrowser6ShowFullNames = 2051,
		CallBrowser7ShowFullNames = 2052,
		CallBrowser8ShowFullNames = 2053,
		CallBrowser9ShowFullNames = 2054,
		CallBrowser10ShowFullNames = 2055,
		CallBrowser11ShowFullNames = 2056,
		CallBrowser12ShowFullNames = 2057,
		CallBrowser13ShowFullNames = 2058,
		CallBrowser14ShowFullNames = 2059,
		CallBrowser15ShowFullNames = 2060,
		CallBrowser16ShowFullNames = 2061,
		CallBrowser1Settings = 2062,
		CallBrowser2Settings = 2063,
		CallBrowser3Settings = 2064,
		CallBrowser4Settings = 2065,
		CallBrowser5Settings = 2066,
		CallBrowser6Settings = 2067,
		CallBrowser7Settings = 2068,
		CallBrowser8Settings = 2069,
		CallBrowser9Settings = 2070,
		CallBrowser10Settings = 2071,
		CallBrowser11Settings = 2072,
		CallBrowser12Settings = 2073,
		CallBrowser13Settings = 2074,
		CallBrowser14Settings = 2075,
		CallBrowser15Settings = 2076,
		CallBrowser16Settings = 2077,
		CallBrowser1SortAlpha = 2078,
		CallBrowser2SortAlpha = 2079,
		CallBrowser3SortAlpha = 2080,
		CallBrowser4SortAlpha = 2081,
		CallBrowser5SortAlpha = 2082,
		CallBrowser6SortAlpha = 2083,
		CallBrowser7SortAlpha = 2084,
		CallBrowser8SortAlpha = 2085,
		CallBrowser9SortAlpha = 2086,
		CallBrowser10SortAlpha = 2087,
		CallBrowser11SortAlpha = 2088,
		CallBrowser12SortAlpha = 2089,
		CallBrowser13SortAlpha = 2090,
		CallBrowser14SortAlpha = 2091,
		CallBrowser15SortAlpha = 2092,
		CallBrowser16SortAlpha = 2093,
		CallBrowser1SortAccess = 2094,
		CallBrowser2SortAccess = 2095,
		CallBrowser3SortAccess = 2096,
		CallBrowser4SortAccess = 2097,
		CallBrowser5SortAccess = 2098,
		CallBrowser6SortAccess = 2099,
		CallBrowser7SortAccess = 2100,
		CallBrowser8SortAccess = 2101,
		CallBrowser9SortAccess = 2102,
		CallBrowser10SortAccess = 2103,
		CallBrowser11SortAccess = 2104,
		CallBrowser12SortAccess = 2105,
		CallBrowser13SortAccess = 2106,
		CallBrowser14SortAccess = 2107,
		CallBrowser15SortAccess = 2108,
		CallBrowser16SortAccess = 2109,
		ShowCallBrowser = 2120,
		CallBrowser1 = 2121,
		CallBrowser2 = 2122,
		CallBrowser3 = 2123,
		CallBrowser4 = 2124,
		CallBrowser5 = 2125,
		CallBrowser6 = 2126,
		CallBrowser7 = 2127,
		CallBrowser8 = 2128,
		CallBrowser9 = 2129,
		CallBrowser10 = 2130,
		CallBrowser11 = 2131,
		CallBrowser12 = 2132,
		CallBrowser13 = 2133,
		CallBrowser14 = 2134,
		CallBrowser15 = 2135,
		CallBrowser16 = 2136,
		CallBrowser17 = 2137,
		GlobalUndo = 2138,
		GlobalRedo = 2139,
		CallBrowserShowCallsToCmd = 2140,
		CallBrowserShowCallsFromCmd = 2141,
		CallBrowserShowNewCallsToCmd = 2142,
		CallBrowserShowNewCallsFromCmd = 2143,
		CallBrowser1Search = 2145,
		CallBrowser2Search = 2146,
		CallBrowser3Search = 2147,
		CallBrowser4Search = 2148,
		CallBrowser5Search = 2149,
		CallBrowser6Search = 2150,
		CallBrowser7Search = 2151,
		CallBrowser8Search = 2152,
		CallBrowser9Search = 2153,
		CallBrowser10Search = 2154,
		CallBrowser11Search = 2155,
		CallBrowser12Search = 2156,
		CallBrowser13Search = 2157,
		CallBrowser14Search = 2158,
		CallBrowser15Search = 2159,
		CallBrowser16Search = 2160,
		CallBrowser1Refresh = 2161,
		CallBrowser2Refresh = 2162,
		CallBrowser3Refresh = 2163,
		CallBrowser4Refresh = 2164,
		CallBrowser5Refresh = 2165,
		CallBrowser6Refresh = 2166,
		CallBrowser7Refresh = 2167,
		CallBrowser8Refresh = 2168,
		CallBrowser9Refresh = 2169,
		CallBrowser10Refresh = 2170,
		CallBrowser11Refresh = 2171,
		CallBrowser12Refresh = 2172,
		CallBrowser13Refresh = 2173,
		CallBrowser14Refresh = 2174,
		CallBrowser15Refresh = 2175,
		CallBrowser16Refresh = 2176,
		CallBrowser1SearchCombo = 2180,
		CallBrowser2SearchCombo = 2181,
		CallBrowser3SearchCombo = 2182,
		CallBrowser4SearchCombo = 2183,
		CallBrowser5SearchCombo = 2184,
		CallBrowser6SearchCombo = 2185,
		CallBrowser7SearchCombo = 2186,
		CallBrowser8SearchCombo = 2187,
		CallBrowser9SearchCombo = 2188,
		CallBrowser10SearchCombo = 2189,
		CallBrowser11SearchCombo = 2190,
		CallBrowser12SearchCombo = 2191,
		CallBrowser13SearchCombo = 2192,
		CallBrowser14SearchCombo = 2193,
		CallBrowser15SearchCombo = 2194,
		CallBrowser16SearchCombo = 2195,
		TaskListProviderCombo = 2200,
		TaskListProviderComboList = 2201,
		CreateUserTask = 2202,
		ErrorListShowErrors = 2210,
		ErrorListShowWarnings = 2211,
		ErrorListShowMessages = 2212,
		Registration = 2214,
		CallBrowser1SearchComboList = 2215,
		CallBrowser2SearchComboList = 2216,
		CallBrowser3SearchComboList = 2217,
		CallBrowser4SearchComboList = 2218,
		CallBrowser5SearchComboList = 2219,
		CallBrowser6SearchComboList = 2220,
		CallBrowser7SearchComboList = 2221,
		CallBrowser8SearchComboList = 2222,
		CallBrowser9SearchComboList = 2223,
		CallBrowser10SearchComboList = 2224,
		CallBrowser11SearchComboList = 2225,
		CallBrowser12SearchComboList = 2226,
		CallBrowser13SearchComboList = 2227,
		CallBrowser14SearchComboList = 2228,
		CallBrowser15SearchComboList = 2229,
		CallBrowser16SearchComboList = 2230,
		SnippetProp = 2240,
		SnippetRef = 2241,
		SnippetRepl = 2242,
		StartPage = 2245,
		EditorLineFirstColumn = 2250,
		EditorLineFirstColumnExtend = 2251,
		SEServerExplorer = 2260,
		SEDataExplorer = 2261,
		ViewCallHierarchy = 2301,
		ToggleConsumeFirstCompletionMode = 2303,
		ECMD_VALIDATION_TARGET = 11281,
		ECMD_VALIDATION_TARGET_GET_LIST = 11282,
		ECMD_CSS_TARGET = 11283,
		ECMD_CSS_TARGET_GET_LIST = 11284,
		Design = 12288,
		DesignOn = 12289,
		SEDesign = 12291,
		NewDiagram = 12292,
		NewTable = 12294,
		NewDBItem = 12302,
		NewTrigger = 12304,
		Debug = 12306,
		NewProcedure = 12307,
		NewQuery = 12308,
		RefreshLocal = 12309,
		DbAddDataConnection = 12311,
		DBDefDBRef = 12312,
		RunCmd = 12313,
		RunOn = 12314,
		NewDBRef = 12315,
		SetAsDef = 12316,
		CreateCmdFile = 12317,
		Cancel = 12318,
		NewDatabase = 12320,
		NewUser = 12321,
		NewRole = 12322,
		ChangeLogin = 12323,
		NewView = 12324,
		ModifyConnection = 12325,
		Disconnect = 12326,
		CopyScript = 12327,
		AddSCC = 12328,
		RemoveSCC = 12329,
		GetLatest = 12336,
		CheckOut = 12337,
		CheckIn = 12338,
		UndoCheckOut = 12339,
		AddItemSCC = 12340,
		NewPackageSpec = 12341,
		NewPackageBody = 12342,
		InsertSQL = 12343,
		RunSelection = 12344,
		UpdateScript = 12345,
		NewScript = 12348,
		NewFunction = 12349,
		NewTableFunction = 12350,
		NewInlineFunction = 12351,
		AddDiagram = 12352,
		AddTable = 12353,
		AddSynonym = 12354,
		AddView = 12355,
		AddProcedure = 12356,
		AddFunction = 12357,
		AddTableFunction = 12358,
		AddInlineFunction = 12359,
		AddPkgSpec = 12360,
		AddPkgBody = 12361,
		AddTrigger = 12362,
		ExportData = 12363,
		DbnsVcsAdd = 12364,
		DbnsVcsRemove = 12365,
		DbnsVcsCheckout = 12366,
		DbnsVcsUndoCheckout = 12367,
		DbnsVcsCheckin = 12368,
		SERetrieveData = 12384,
		SEEditTextObject = 12385,
		DesignSQLBlock = 12388,
		RegisterSQLInstance = 12389,
		UnregisterSQLInstance = 12390,
		CommandWindowSaveScript = 12550,
		CommandWindowRunScript = 12551,
		CommandWindowCursorUp = 12552,
		CommandWindowCursorDown = 12553,
		CommandWindowCursorLeft = 12554,
		CommandWindowCursorRight = 12555,
		CommandWindowHistoryUp = 12556,
		CommandWindowHistoryDown = 12557
	}

	[Guid("5DD0BB59-7076-4C59-88D3-DE36931F63F0")]
	public enum VSStd2010CmdID
	{
		DynamicToolBarListFirst = 1,
		DynamicToolBarListLast = 300,
		WindowFrameDockMenu = 500,
		NextDocumentTab = 600,
		PreviousDocumentTab = 601,
		ShellNavigate1First = 1000,
		ShellNavigate2First = 1033,
		ShellNavigate3First = 1066,
		ShellNavigate4First = 1099,
		ShellNavigate5First = 1132,
		ShellNavigate6First = 1165,
		ShellNavigate7First = 1198,
		ShellNavigate8First = 1231,
		ShellNavigate9First = 1264,
		ShellNavigate10First = 1297,
		ShellNavigate11First = 1330,
		ShellNavigate12First = 1363,
		ShellNavigate13First = 1396,
		ShellNavigate14First = 1429,
		ShellNavigate15First = 1462,
		ShellNavigate16First = 1495,
		ShellNavigate17First = 1528,
		ShellNavigate18First = 1561,
		ShellNavigate19First = 1594,
		ShellNavigate20First = 1627,
		ShellNavigate21First = 1660,
		ShellNavigate22First = 1693,
		ShellNavigate23First = 1726,
		ShellNavigate24First = 1759,
		ShellNavigate25First = 1792,
		ShellNavigate26First = 1825,
		ShellNavigate27First = 1858,
		ShellNavigate28First = 1891,
		ShellNavigate29First = 1924,
		ShellNavigate30First = 1957,
		ShellNavigate31First = 1990,
		ShellNavigate32First = 2023,
		ShellNavigateLast = 2055,
		ZoomIn = 2100,
		ZoomOut = 2101,
		OUTLN_EXPAND_ALL = 2500,
		OUTLN_COLLAPSE_ALL = 2501,
		OUTLN_EXPAND_CURRENT = 2502,
		OUTLN_COLLAPSE_CURRENT = 2503,
		ExtensionManager = 3000
	}

	[Guid("D63DB1F0-404E-4B21-9648-CA8D99245EC3")]
	public enum VSStd11CmdID
	{
		FloatAll = 1,
		MoveAllToNext = 2,
		MoveAllToPrevious = 3,
		MultiSelect = 4,
		PaneNextTabAndMultiSelect = 5,
		PanePrevTabAndMultiSelect = 6,
		PinTab = 7,
		BringFloatingWindowsToFront = 8,
		PromoteTab = 9,
		MoveToMainTabWell = 10,
		ToggleFilter = 11,
		FilterToCurrentProject = 12,
		FilterToCurrentDocument = 13,
		FilterToOpenDocuments = 14,
		WindowSearch = 17,
		GlobalSearch = 18,
		GlobalSearchBack = 19,
		SolutionExplorerSearch = 20,
		StartupProjectProperties = 21,
		CloseAllButPinned = 22,
		ResolveFaultedProjects = 23,
		ExecuteSelectionInInteractive = 24,
		ExecuteLineInInteractive = 25,
		InteractiveSessionInterrupt = 26,
		InteractiveSessionRestart = 27,
		SolutionExplorerCollapseAll = 29,
		SolutionExplorerBack = 30,
		SolutionExplorerHome = 31,
		SolutionExplorerForward = 33,
		SolutionExplorerNewScopedWindow = 34,
		SolutionExplorerToggleSingleClickPreview = 35,
		SolutionExplorerSyncWithActiveDocument = 36,
		NewProjectFromTemplate = 37,
		SolutionExplorerScopeToThis = 38,
		SolutionExplorerFilterOpened = 39,
		SolutionExplorerFilterPendingChanges = 40,
		PasteAsLink = 41,
		LocateFindTarget = 42
	}

	[Guid("2A8866DC-7BDE-4dc8-A360-A60679534384")]
	public enum VSStd12CmdID
	{
		ShowUserNotificationsToolWindow = 1,
		OpenProjectFromScc = 2,
		ShareProject = 3,
		PeekDefinition = 4,
		AccountSettings = 5,
		PeekNavigateForward = 6,
		PeekNavigateBackward = 7,
		RetargetProject = 8,
		RetargetProjectInstallComponent = 9,
		AddReferenceProjectOnly = 10,
		AddWebReferenceProjectOnly = 11,
		AddServiceReferenceProjectOnly = 12,
		AddReferenceNonProjectOnly = 13,
		AddWebReferenceNonProjectOnly = 14,
		AddServiceReferenceNonProjectOnly = 15,
		NavigateTo = 256,
		MoveSelLinesUp = 258,
		MoveSelLinesDown = 259
	}

	[Guid("4C7763BF-5FAF-4264-A366-B7E1F27BA958")]
	public enum VSStd14CmdID
	{
		ShowQuickFixes = 1,
		ShowRefactorings = 2,
		SmartBreakLine = 3,
		ManageWindowLayouts = 4,
		SaveWindowLayout = 5,
		ShowQuickFixesForPosition = 6,
		ShowQuickFixesForPosition2 = 7,
		DeleteFR1 = 10,
		DeleteFR2 = 20,
		ErrorContextComboList = 30,
		ErrorContextComboGetList = 31,
		ErrorBuildContextComboList = 40,
		ErrorBuildContextComboGetList = 41,
		ErrorListClearFilters = 50,
		WindowLayoutList0 = 4096,
		WindowLayoutList1 = 4097,
		WindowLayoutList2 = 4098,
		WindowLayoutList3 = 4099,
		WindowLayoutList4 = 4100,
		WindowLayoutList5 = 4101,
		WindowLayoutList6 = 4102,
		WindowLayoutList7 = 4103,
		WindowLayoutList8 = 4104,
		WindowLayoutList9 = 4105,
		WindowLayoutListFirst = 4096,
		WindowLayoutListDynamicFirst = 4112,
		WindowLayoutListLast = 8191
	}

	[Guid("712C6C80-883B-4AAD-B430-BBCA5256FA9D")]
	public enum VSStd15CmdID
	{
		NavigateToFile = 1,
		NavigateToType = 2,
		NavigateToSymbol = 3,
		NavigateToMember = 4,
		NavigateToRecentFile = 5,
		FindAllRefPresetGroupingComboList = 42,
		FindAllRefPresetGroupingComboGetList = 43,
		FindAllRefLockWindow = 44,
		FindAllRefFlatList = 45,
		GetToolsAndFeatures = 60,
		ShowLineAnnotations = 76,
		MoveToNextAnnotation = 77,
		MoveToPreviousAnnotation = 78,
		ShowStructure = 79,
		HelpAccessibility = 112,
		ToggleAutoHideChannels = 256,
		EnableRestoreDocumentsOnSolutionLoad = 512,
		DisableRestoreDocumentsOnSolutionLoad = 513,
		CloseAllButToolWindows = 528
	}

	[Guid("8F380902-6040-4097-9837-D3F40E66F908")]
	public enum VSStd16CmdID
	{
		NewProject2 = 1,
		DocumentTabsLeft = 2,
		DocumentTabsTop = 3,
		DocumentTabsRight = 4,
		DocumentTabSettings = 5,
		DocumentTabsGroupByProject = 6,
		DocumentTabsGroupNone = 7,
		DocumentTabsSortAlpha = 8,
		DocumentTabsSortMroFirst = 9,
		DocumentTabsSortMroLast = 10,
		CopyRelativePath = 128,
		AddAssemblyReference = 512,
		AddComReference = 513,
		AddProjectReference = 514,
		AddSharedProjectReference = 515,
		AddSdkReference = 516,
		LoadProjectDependencies = 1655,
		LoadAllProjectDependencies = 1656,
		RepeatFind = 46
	}

	[Guid("43F755C7-7916-454D-81A9-90D4914019DD")]
	public enum VSStd17CmdID
	{
		[Obsolete("Do not use this cmd id. It has been deprecated.")]
		ColorizeDocumentTabsByProject = 1,
		SelectNoTabColor,
		SelectTabColor1,
		SelectTabColor2,
		SelectTabColor3,
		SelectTabColor4,
		SelectTabColor5,
		SelectTabColor6,
		SelectTabColor7,
		SelectTabColor8,
		SelectTabColor9,
		SelectTabColor10,
		SelectTabColor11,
		SelectTabColor12,
		SelectTabColor13,
		SelectTabColor14,
		SelectTabColor15,
		SelectTabColor16,
		RestoreClosedTab,
		MultiRowTabs,
		RestoreClosedToolWindow,
		ToggleToolWindowVisibility
	}

	[Flags]
	public enum CEF : uint
	{
		CloneFile = 1u,
		OpenFile = 2u,
		Silent = 4u,
		OpenAsNew = 8u
	}

	[Guid("60481700-078b-11d1-aaf8-00a0c9055a90")]
	public enum VsUIHierarchyWindowCmdIds
	{
		UIHWCMDID_RightClick = 1,
		UIHWCMDID_DoubleClick,
		UIHWCMDID_EnterKey,
		UIHWCMDID_StartLabelEdit,
		UIHWCMDID_CommitLabelEdit,
		UIHWCMDID_CancelLabelEdit
	}

	public enum VSSELELEMID
	{
		SEID_UndoManager,
		SEID_WindowFrame,
		SEID_DocumentFrame,
		SEID_StartupProject,
		SEID_PropertyBrowserSID,
		SEID_UserContext,
		SEID_ResultList,
		SEID_LastWindowFrame
	}

	public static class VsPackageGuid
	{
		public const string VsEnvironmentPackage_string = "{DA9FB551-C724-11D0-AE1F-00A0C90FFFC3}";

		public static readonly Guid VsEnvironmentPackage_guid = new Guid("{DA9FB551-C724-11D0-AE1F-00A0C90FFFC3}");

		public const string HtmlEditorPackage_string = "{1B437D20-F8FE-11D2-A6AE-00104BCC7269}";

		public static readonly Guid HtmlEditorPackage_guid = new Guid("{1B437D20-F8FE-11D2-A6AE-00104BCC7269}");

		public const string VsTaskListPackage_string = "{4A9B7E50-AA16-11D0-A8C5-00A0C921A4D2}";

		public static readonly Guid VsTaskListPackage_guid = new Guid("{4A9B7E50-AA16-11D0-A8C5-00A0C921A4D2}");

		public const string VsDocOutlinePackage_string = "{21AF45B0-FFA5-11D0-B63F-00A0C922E851}";

		public static readonly Guid VsDocOutlinePackage_guid = new Guid("{21AF45B0-FFA5-11D0-B63F-00A0C922E851}");
	}

	public static class VsEditorFactoryGuid
	{
		public const string HtmlEditor_string = "{C76D83F8-A489-11D0-8195-00A0C91BBEE3}";

		public static readonly Guid HtmlEditor_guid = new Guid("{C76D83F8-A489-11D0-8195-00A0C91BBEE3}");

		public const string TextEditor_string = "{8B382828-6202-11d1-8870-0000F87579D2}";

		public static readonly Guid TextEditor_guid = new Guid("{8B382828-6202-11d1-8870-0000F87579D2}");

		public const string ExternalEditor_string = "{8B382828-6202-11D1-8870-0000F87579D2}";

		public static readonly Guid ExternalEditor_guid = new Guid("{8B382828-6202-11D1-8870-0000F87579D2}");

		public const string ProjectDesignerEditor_string = "{04B8AB82-A572-4FEF-95CE-5222444B6B64}";

		public static readonly Guid ProjectDesignerEditor_guid = new Guid("{04B8AB82-A572-4FEF-95CE-5222444B6B64}");
	}

	public static class VsLanguageServiceGuid
	{
		public const string HtmlLanguageService_string = "{58E975A0-F8FE-11D2-A6AE-00104BCC7269}";

		public static readonly Guid HtmlLanguageService_guid = new Guid("{58E975A0-F8FE-11D2-A6AE-00104BCC7269}");
	}

	public static class OutputWindowPaneGuid
	{
		public const string BuildOutputPane_string = "{1BD8A850-02D1-11D1-BEE7-00A0C913D1F8}";

		public static readonly Guid BuildOutputPane_guid = new Guid("{1BD8A850-02D1-11D1-BEE7-00A0C913D1F8}");

		public const string SortedBuildOutputPane_string = "{2032B126-7C8D-48AD-8026-0E0348004FC0}";

		public static readonly Guid SortedBuildOutputPane_guid = new Guid("{2032B126-7C8D-48AD-8026-0E0348004FC0}");

		public const string DebugPane_string = "{FC076020-078A-11D1-A7DF-00A0C9110051}";

		public static readonly Guid DebugPane_guid = new Guid("{FC076020-078A-11D1-A7DF-00A0C9110051}");

		public const string GeneralPane_string = "{3C24D581-5591-4884-A571-9FE89915CD64}";

		public static readonly Guid GeneralPane_guid = new Guid("{3C24D581-5591-4884-A571-9FE89915CD64}");

		public const string StoreValidationPane_string = "{54065C74-1B11-4249-9EA7-5540D1A6D528}";

		public static readonly Guid StoreValidationPane_guid = new Guid("{54065C74-1B11-4249-9EA7-5540D1A6D528}");
	}

	public static class ItemTypeGuid
	{
		public const string PhysicalFile_string = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";

		public static readonly Guid PhysicalFile_guid = new Guid("{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}");

		public const string PhysicalFolder_string = "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}";

		public static readonly Guid PhysicalFolder_guid = new Guid("{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}");

		public const string VirtualFolder_string = "{6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C}";

		public static readonly Guid VirtualFolder_guid = new Guid("{6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C}");

		public const string SubProject_string = "{EA6618E8-6E24-4528-94BE-6889FE16485C}";

		public static readonly Guid SubProject_guid = new Guid("{EA6618E8-6E24-4528-94BE-6889FE16485C}");

		public const string SharedProjectReference_string = "{FBA6BD9A-47F3-4C04-BDC0-7F76A9E2E582}";

		public static readonly Guid SharedProjectReference_guid = new Guid("{FBA6BD9A-47F3-4C04-BDC0-7F76A9E2E582}");
	}

	public static class CodeModelLanguage
	{
		public const string VC = "{B5E9BD32-6D3E-4B5D-925E-8A43B79820B4}";

		public const string VB = "{B5E9BD33-6D3E-4B5D-925E-8A43B79820B4}";

		public const string CSharp = "{B5E9BD34-6D3E-4B5D-925E-8A43B79820B4}";

		public const string IDL = "{B5E9BD35-6D3E-4B5D-925E-8A43B79820B4}";

		public const string MC = "{B5E9BD36-6D3E-4B5D-925E-8A43B79820B4}";
	}

	public static class WizardType
	{
		public const string AddSubProject = "{0F90E1D2-4999-11D1-B6D1-00A0C90F2744}";

		public const string AddItem = "{0F90E1D1-4999-11D1-B6D1-00A0C90F2744}";

		public const string NewProject = "{0F90E1D0-4999-11D1-B6D1-00A0C90F2744}";
	}

	public static class VsDependencyTypeGuid
	{
		public const string BuildProject_string = "{707D11B6-91CA-11D0-8A3E-00A0C91E2ACD}";

		public static readonly Guid BuildProject_guid = new Guid("{707D11B6-91CA-11D0-8A3E-00A0C91E2ACD}");
	}

	public static class VsEditorUserDataGuid
	{
		public const string EditorDpiContext_string = "{E80CBA74-D298-4DB5-A28D-6511675031F4}";

		public static readonly Guid EditorDpiContext_guid = new Guid("{E80CBA74-D298-4DB5-A28D-6511675031F4}");
	}

	public static class VsLanguageUserDataGuid
	{
		public const string SupportCF_HTML_string = "{27E97702-589E-11D2-8233-0080C747D9A0}";

		public static readonly Guid SupportCF_HTML_guid = new Guid("{27E97702-589E-11D2-8233-0080C747D9A0}");
	}

	public static class VsTextBufferUserDataGuid
	{
		public const string VsBufferMoniker_string = "{978A8E17-4DF8-432A-9623-D530A26452BC}";

		public static readonly Guid VsBufferMoniker_guid = new Guid("{978A8E17-4DF8-432A-9623-D530A26452BC}");

		public const string VsBufferIsDiskFile_string = "{D9126592-1473-11D3-BEC6-0080C747D9A0}";

		public static readonly Guid VsBufferIsDiskFile_guid = new Guid("{D9126592-1473-11D3-BEC6-0080C747D9A0}");

		public const string VsBufferEncodingVSTFF_string = "{16417F39-A6B7-4C90-89FA-770D2C60440B}";

		public static readonly Guid VsBufferEncodingVSTFF_guid = new Guid("{16417F39-A6B7-4C90-89FA-770D2C60440B}");

		public const string VsBufferEncodingPromptOnLoad_string = "{99EC03F0-C843-4C09-BE74-CDCA5158D36C}";

		public static readonly Guid VsBufferEncodingPromptOnLoad_guid = new Guid("{99EC03F0-C843-4C09-BE74-CDCA5158D36C}");

		public const string VsBufferDetectCharSet_string = "{36358D1F-BF7E-11D1-B03A-00C04FB68006}";

		public static readonly Guid VsBufferDetectCharSet_guid = new Guid("{36358D1F-BF7E-11D1-B03A-00C04FB68006}");

		public const string VsBufferDetectLangSID_string = "{17F375AC-C814-11D1-88AD-0000F87579D2}";

		public static readonly Guid VsBufferDetectLangSID_guid = new Guid("{17F375AC-C814-11D1-88AD-0000F87579D2}");

		public const string PropertyBrowserSID_string = "{CE6DDBBA-8D13-11D1-8889-0000F87579D2}";

		public static readonly Guid PropertyBrowserSID_guid = new Guid("{CE6DDBBA-8D13-11D1-8889-0000F87579D2}");

		public const string UserReadOnlyErrorString_string = "{A3BCFE56-CF1B-11D1-88B1-0000F87579D2}";

		public static readonly Guid UserReadOnlyErrorString_guid = new Guid("{A3BCFE56-CF1B-11D1-88B1-0000F87579D2}");

		public const string BufferStorage_string = "{D97F167A-638E-11D2-88F6-0000F87579D2}";

		public static readonly Guid BufferStorage_guid = new Guid("{D97F167A-638E-11D2-88F6-0000F87579D2}");

		public const string VsBufferExtraFiles_string = "{FD494BF6-1167-4635-A20C-5C24B2D7B33D}";

		public static readonly Guid VsBufferExtraFiles_guid = new Guid("{FD494BF6-1167-4635-A20C-5C24B2D7B33D}");

		public const string VsBufferFileReload_string = "{80D2B881-81A3-4F0B-BCF0-70A0054E672F}";

		public static readonly Guid VsBufferFileReload_guid = new Guid("{80D2B881-81A3-4F0B-BCF0-70A0054E672F}");

		public const string VsInitEncodingDialogFromUserData_string = "{C2382D84-6650-4386-860F-248ECB222FC1}";

		public static readonly Guid VsInitEncodingDialogFromUserData_guid = new Guid("{C2382D84-6650-4386-860F-248ECB222FC1}");

		public const string VsBufferContentType_string = "{1BEB4195-98F4-4589-80E0-480CE32FF059}";

		public static readonly Guid VsBufferContentType_guid = new Guid("{1BEB4195-98F4-4589-80E0-480CE32FF059}");

		public const string VsTextViewRoles_string = "{297078FF-81A2-43D8-9CA3-4489C53C99BA}";

		public static readonly Guid VsTextViewRoles_guid = new Guid("{297078FF-81A2-43D8-9CA3-4489C53C99BA}");
	}

	public static class DocumentMetadataUserDataGuid
	{
		public const string Version_string = "{22acb6fc-6235-401f-a4f5-4f29e7e45049}";

		public static readonly Guid Version_guid = new Guid("{22acb6fc-6235-401f-a4f5-4f29e7e45049}");
	}

	public static class EditPropyCategoryGuid
	{
		public const string TextManagerGlobal_string = "{6BFB60A2-48D8-424E-81A2-040ACA0B1F68}";

		public static readonly Guid TextManagerGlobal_guid = new Guid("{6BFB60A2-48D8-424E-81A2-040ACA0B1F68}");

		public const string ViewMasterSettings_string = "{D1756E7C-B7FD-49A8-B48E-87B14A55655A}";

		public static readonly Guid ViewMasterSettings_guid = new Guid("{D1756E7C-B7FD-49A8-B48E-87B14A55655A}");
	}

	public static class CATID
	{
		public const string CSharpFileProperties_string = "{8D58E6AF-ED4E-48B0-8C7B-C74EF0735451}";

		public static readonly Guid CSharpFileProperties_guid = new Guid("{8D58E6AF-ED4E-48B0-8C7B-C74EF0735451}");

		public const string CSharpFolderProperties_string = "{914FE278-054A-45DB-BF9E-5F22484CC84C}";

		public static readonly Guid CSharpFolderProperties_guid = new Guid("{914FE278-054A-45DB-BF9E-5F22484CC84C}");

		public const string ProjectAutomationObject_string = "{610D4614-D0D5-11D2-8599-006097C68E81}";

		public static readonly Guid ProjectAutomationObject_guid = new Guid("{610D4614-D0D5-11D2-8599-006097C68E81}");

		public const string ProjectItemAutomationObject_string = "{610D4615-D0D5-11D2-8599-006097C68E81}";

		public static readonly Guid ProjectItemAutomationObject_guid = new Guid("{610D4615-D0D5-11D2-8599-006097C68E81}");

		public const string VBAFileProperties_string = "{AC2912B2-50ED-4E62-8DFF-429B4B88FC9E}";

		public static readonly Guid VBAFileProperties_guid = new Guid("{AC2912B2-50ED-4E62-8DFF-429B4B88FC9E}");

		public const string VBAFolderProperties_string = "{79231B36-6213-481D-AA7D-0F931E8F2CF9}";

		public static readonly Guid VBAFolderProperties_guid = new Guid("{79231B36-6213-481D-AA7D-0F931E8F2CF9}");

		public const string VBFileProperties_string = "{EA5BD05D-3C72-40A5-95A0-28A2773311CA}";

		public static readonly Guid VBFileProperties_guid = new Guid("{EA5BD05D-3C72-40A5-95A0-28A2773311CA}");

		public const string VBFolderProperties_string = "{932DC619-2EAA-4192-B7E6-3D15AD31DF49}";

		public static readonly Guid VBFolderProperties_guid = new Guid("{932DC619-2EAA-4192-B7E6-3D15AD31DF49}");

		public const string VBProjectProperties_string = "{E0FDC879-C32A-4751-A3D3-0B3824BD575F}";

		public static readonly Guid VBProjectProperties_guid = new Guid("{E0FDC879-C32A-4751-A3D3-0B3824BD575F}");

		public const string VBReferenceProperties_string = "{2289B812-8191-4E81-B7B3-174045AB0CB5}";

		public static readonly Guid VBReferenceProperties_guid = new Guid("{2289B812-8191-4E81-B7B3-174045AB0CB5}");

		public const string VCProjectNode_string = "{EE8299CB-19B6-4F20-ABEA-E1FD9A33B683}";

		public static readonly Guid VCProjectNode_guid = new Guid("{EE8299CB-19B6-4F20-ABEA-E1FD9A33B683}");

		public const string VCFileGroup_string = "{EE8299CA-19B6-4F20-ABEA-E1FD9A33B683}";

		public static readonly Guid VCFileGroup_guid = new Guid("{EE8299CA-19B6-4F20-ABEA-E1FD9A33B683}");

		public const string VCFileNode_string = "{EE8299C9-19B6-4F20-ABEA-E1FD9A33B683}";

		public static readonly Guid VCFileNode_guid = new Guid("{EE8299C9-19B6-4F20-ABEA-E1FD9A33B683}");

		public const string VCAssemblyReferenceNode_string = "{FE8299C9-19B6-4F20-ABEA-E1FD9A33B683}";

		public static readonly Guid VCAssemblyReferenceNode_guid = new Guid("{FE8299C9-19B6-4F20-ABEA-E1FD9A33B683}");

		public const string VCProjectReferenceNode_string = "{593DCFCE-20A7-48E4-ACA1-49ADE9049887}";

		public static readonly Guid VCProjectReferenceNode_guid = new Guid("{593DCFCE-20A7-48E4-ACA1-49ADE9049887}");

		public const string VCActiveXReferenceNode_string = "{9E8182D3-C60A-44F4-A74B-14C90EF9CACE}";

		public static readonly Guid VCActiveXReferenceNode_guid = new Guid("{9E8182D3-C60A-44F4-A74B-14C90EF9CACE}");

		public const string VCReferences_string = "{FE8299CA-19B6-4F20-ABEA-E1FD9A33B683}";

		public static readonly Guid VCReferences_guid = new Guid("{FE8299CA-19B6-4F20-ABEA-E1FD9A33B683}");
	}

	public static class CLSID
	{
		public const string MiscellaneousFilesProject_string = "{A2FE74E1-B743-11D0-AE1A-00A0C90FFFC3}";

		public static readonly Guid MiscellaneousFilesProject_guid = new Guid("{A2FE74E1-B743-11D0-AE1A-00A0C90FFFC3}");

		public const string SolutionFolderProject_string = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";

		public static readonly Guid SolutionFolderProject_guid = new Guid("{2150E333-8FDC-42A3-9474-1A3956D46DE8}");

		public const string SolutionItemsProject_string = "{D1DCDB85-C5E8-11D2-BFCA-00C04F990235}";

		public static readonly Guid SolutionItemsProject_guid = new Guid("{D1DCDB85-C5E8-11D2-BFCA-00C04F990235}");

		public const string VsTextBuffer_string = "{8E7B96A8-E33D-11d0-A6D5-00C04FB67F6A}";

		public static readonly Guid VsTextBuffer_guid = new Guid("{8E7B96A8-E33D-11d0-A6D5-00C04FB67F6A}");

		public const string UnloadedProject_string = "{76E22BD3-C2EC-47F1-802B-53197756DAE8}";

		public static readonly Guid UnloadedProject_guid = new Guid("{76E22BD3-C2EC-47F1-802B-53197756DAE8}");

		public const string VsCfgProviderEventsHelper_string = "{99913F1F-1EE3-11D1-8A6E-00C04F682E21}";

		public static readonly Guid VsCfgProviderEventsHelper_guid = new Guid("{99913F1F-1EE3-11D1-8A6E-00C04F682E21}");

		public const string VsEnvironmentPackage_string = "{DA9FB551-C724-11D0-AE1F-00A0C90FFFC3}";

		public static readonly Guid VsEnvironmentPackage_guid = new Guid("{DA9FB551-C724-11D0-AE1F-00A0C90FFFC3}");

		public const string VsTaskListPackage_string = "{4A9B7E50-AA16-11D0-A8C5-00A0C921A4D2}";

		public static readonly Guid VsTaskListPackage_guid = new Guid("{4A9B7E50-AA16-11D0-A8C5-00A0C921A4D2}");

		public const string VsUIWpfLoader_string = "{0B127700-143C-4AB5-9D39-BFF47151B563}";

		public static readonly Guid VsUIWpfLoader_guid = new Guid("{0B127700-143C-4AB5-9D39-BFF47151B563}");

		public const string VsSearchQueryParser_string = "{B71B3DF9-7A4A-4D70-8293-3874DB098FDD}";

		public static readonly Guid VsSearchQueryParser_guid = new Guid("{B71B3DF9-7A4A-4D70-8293-3874DB098FDD}");

		public const string HtmDocData_string = "{62C81794-A9EC-11D0-8198-00A0C91BBEE3}";

		public static readonly Guid HtmDocData_guid = new Guid("{62C81794-A9EC-11D0-8198-00A0C91BBEE3}");

		public const string VsUIHierarchyWindow_string = "{7D960B07-7AF8-11D0-8E5E-00A0C911005A}";

		public static readonly Guid VsUIHierarchyWindow_guid = new Guid("{7D960B07-7AF8-11D0-8E5E-00A0C911005A}");

		public const string VsTaskList_string = "{BC5955D5-AA0D-11D0-A8C5-00A0C921A4D2}";

		public static readonly Guid VsTaskList_guid = new Guid("{BC5955D5-AA0D-11D0-A8C5-00A0C921A4D2}");
	}

	public static class DebugEnginesGuids
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly Guid NativeOnly = new Guid("{3B476D35-A401-11D2-AAD4-00C04F990171}");

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly Guid Script = new Guid("{F200A7E7-DEA5-11D0-B854-00A0244A1DE2}");

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly Guid ManagedAndNative = new Guid("{92EF0900-2251-11D2-B72E-0000F87572EF}");

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly Guid SQLLocalEngine = new Guid("{E04BDE58-45EC-48DB-9807-513F78865212}");

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly Guid SqlDebugEngine2 = new Guid("{3B476D30-A401-11D2-AAD4-00C04F990171}");

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly Guid SqlDebugEngine3 = new Guid("{3B476D3A-A401-11D2-AAD4-00C04F990171}");

		public const string ManagedOnly_string = "{449EC4CC-30D2-4032-9256-EE18EB41B62B}";

		public static readonly Guid ManagedOnly_guid = new Guid("{449EC4CC-30D2-4032-9256-EE18EB41B62B}");

		public const string ManagedOnlyEngineV2_string = "{5FFF7536-0C87-462D-8FD2-7971D948E6DC}";

		public static readonly Guid ManagedOnlyEngineV2_guid = new Guid("{5FFF7536-0C87-462D-8FD2-7971D948E6DC}");

		public const string ManagedOnlyEngineV4_string = "{FB0D4648-F776-4980-95F8-BB7F36EBC1EE}";

		public static readonly Guid ManagedOnlyEngineV4_guid = new Guid("{FB0D4648-F776-4980-95F8-BB7F36EBC1EE}");

		public const string CoreSystemClr_string = "{2E36F1D4-B23C-435D-AB41-18E608940038}";

		public static readonly Guid CoreSystemClr_guid = new Guid("{2E36F1D4-B23C-435D-AB41-18E608940038}");

		public const string COMPlusLegacyEngine_string = "{351668CC-8477-4fbf-BFE3-5F1006E4DB1F}";

		public static readonly Guid COMPlusLegacyEngine_guid = new Guid("{351668CC-8477-4fbf-BFE3-5F1006E4DB1F}");

		public const string COMPlusNewArchEngine_string = "{97552AEF-4F41-447a-BCC3-802EAA377343}";

		public static readonly Guid COMPlusNewArchEngine_guid = new Guid("{97552AEF-4F41-447a-BCC3-802EAA377343}");

		public const string NativeOnly_string = "{3B476D35-A401-11D2-AAD4-00C04F990171}";

		public static readonly Guid NativeOnly_guid = new Guid("{3B476D35-A401-11D2-AAD4-00C04F990171}");

		public const string Script_string = "{F200A7E7-DEA5-11D0-B854-00A0244A1DE2}";

		public static readonly Guid Script_guid = new Guid("{F200A7E7-DEA5-11D0-B854-00A0244A1DE2}");

		public const string ManagedAndNative_string = "{92EF0900-2251-11D2-B72E-0000F87572EF}";

		public static readonly Guid ManagedAndNative_guid = new Guid("{92EF0900-2251-11D2-B72E-0000F87572EF}");

		public const string SQLLocalEngine_string = "{E04BDE58-45EC-48DB-9807-513F78865212}";

		public static readonly Guid SQLLocalEngine_guid = new Guid("{E04BDE58-45EC-48DB-9807-513F78865212}");

		public const string SqlDebugEngine2_string = "{3B476D30-A401-11D2-AAD4-00C04F990171}";

		public static readonly Guid SqlDebugEngine2_guid = new Guid("{3B476D30-A401-11D2-AAD4-00C04F990171}");

		public const string SqlDebugEngine3_string = "{3B476D3A-A401-11D2-AAD4-00C04F990171}";

		public static readonly Guid SqlDebugEngine3_guid = new Guid("{3B476D3A-A401-11D2-AAD4-00C04F990171}");
	}

	public static class DebugPortSupplierGuids
	{
		public const string NoAuth_string = "{3b476d38-a401-11d2-aad4-00c04f990171}";

		public static readonly Guid NoAuth_guid = new Guid("{3b476d38-a401-11d2-aad4-00c04f990171}");
	}

	public static class ProjectTargets
	{
		public static readonly Guid AppContainer_Win8 = new Guid("676F25AF-340F-41F0-986E-6BDD00C6DD63");

		public static readonly Guid AppContainer_Win8_1 = new Guid("239B20B0-620F-4789-B69A-D1C2223AAE11");

		public static readonly Guid WindowsPhone_80SL = new Guid("DE9F6B31-C1E5-B965-95F3-1885AF956FC9");

		public static readonly Guid WindowsPhone_81SL = new Guid("FAC6B224-1737-05C3-1859-1DC6BF8C3D9E");

		public static readonly Guid OneCore_1 = new Guid("08172433-85AA-424E-A9F1-C140350D8FB5");
	}

	public static class SetupDrivers
	{
		public static readonly Guid SetupDriver_VS = new Guid("4736F7FF-3C17-4848-A5A3-DE37828E8EAD");

		public static readonly Guid SetupDriver_WebPI = new Guid("114414D4-597D-4C7B-8421-9B49C54E5302");

		public static readonly Guid SetupDriver_OOBFeed = new Guid("3F40C28E-C61D-4137-9C22-C0DD553C6344");
	}

	public static class UICONTEXT
	{
		public const string RESXEditor_string = "{FEA4DCC9-3645-44CD-92E7-84B55A16465C}";

		public static readonly Guid RESXEditor_guid = new Guid("{FEA4DCC9-3645-44CD-92E7-84B55A16465C}");

		public const string SettingsDesigner_string = "{515231AD-C9DC-4AA3-808F-E1B65E72081C}";

		public static readonly Guid SettingsDesigner_guid = new Guid("{515231AD-C9DC-4AA3-808F-E1B65E72081C}");

		public const string PropertyPageDesigner_string = "{86670EFA-3C28-4115-8776-A4D5BB1F27CC}";

		public static readonly Guid PropertyPageDesigner_guid = new Guid("{86670EFA-3C28-4115-8776-A4D5BB1F27CC}");

		public const string ApplicationDesigner_string = "{D06CD5E3-D961-44DC-9D80-C89A1A8D9D56}";

		public static readonly Guid ApplicationDesigner_guid = new Guid("{D06CD5E3-D961-44DC-9D80-C89A1A8D9D56}");

		public const string VBProjOpened_string = "{9DA22B82-6211-11d2-9561-00600818403B}";

		public static readonly Guid VBProjOpened_guid = new Guid("{9DA22B82-6211-11d2-9561-00600818403B}");

		public const string CodeWindow_string = "{8FE2DF1D-E0DA-4EBE-9D5C-415D40E487B5}";

		public static readonly Guid CodeWindow_guid = new Guid("{8FE2DF1D-E0DA-4EBE-9D5C-415D40E487B5}");

		public const string DataSourceWindowAutoVisible_string = "{2E78870D-AC7C-4460-A4A1-3FE37D00EF81}";

		public static readonly Guid DataSourceWindowAutoVisible_guid = new Guid("{2E78870D-AC7C-4460-A4A1-3FE37D00EF81}");

		public const string DataSourceWizardSuppressed_string = "{5705AD15-40EE-4426-AD3E-BA750610D599}";

		public static readonly Guid DataSourceWizardSuppressed_guid = new Guid("{5705AD15-40EE-4426-AD3E-BA750610D599}");

		public const string DataSourceWindowSupported_string = "{95C314C4-660B-4627-9F82-1BAF1C764BBF}";

		public static readonly Guid DataSourceWindowSupported_guid = new Guid("{95C314C4-660B-4627-9F82-1BAF1C764BBF}");

		public const string Debugging_string = "{ADFC4E61-0397-11D1-9F4E-00A0C911004F}";

		public static readonly Guid Debugging_guid = new Guid("{ADFC4E61-0397-11D1-9F4E-00A0C911004F}");

		public const string DesignMode_string = "{ADFC4E63-0397-11D1-9F4E-00A0C911004F}";

		public static readonly Guid DesignMode_guid = new Guid("{ADFC4E63-0397-11D1-9F4E-00A0C911004F}");

		public const string Dragging_string = "{B706F393-2E5B-49E7-9E2E-B1825F639B63}";

		public static readonly Guid Dragging_guid = new Guid("{B706F393-2E5B-49E7-9E2E-B1825F639B63}");

		public const string EmptySolution_string = "{ADFC4E65-0397-11D1-9F4E-00A0C911004F}";

		public static readonly Guid EmptySolution_guid = new Guid("{ADFC4E65-0397-11D1-9F4E-00A0C911004F}");

		public const string FirstLaunchSetup_string = "{E7B2B2DB-973B-4CE9-A8D7-8498895DEA73}";

		public static readonly Guid FirstLaunchSetup_guid = new Guid("{E7B2B2DB-973B-4CE9-A8D7-8498895DEA73}");

		public const string FullScreenMode_string = "{ADFC4E62-0397-11D1-9F4E-00A0C911004F}";

		public static readonly Guid FullScreenMode_guid = new Guid("{ADFC4E62-0397-11D1-9F4E-00A0C911004F}");

		public const string MinimalMode_string = "{8AED84FC-BA6C-4233-8D76-5BA42B0EE91D}";

		public static readonly Guid MinimalMode_guid = new Guid("{8AED84FC-BA6C-4233-8D76-5BA42B0EE91D}");

		public const string MainToolBarVisible_string = "{206F83B1-2911-4CDF-95DF-EAB51E21F938}";

		public static readonly Guid MainToolBarVisible_guid = new Guid("{206F83B1-2911-4CDF-95DF-EAB51E21F938}");

		public const string MainToolBarInvisible_string = "{C70BC0E0-343C-486E-963C-5E08EFA0FC8D}";

		public static readonly Guid MainToolBarInvisible_guid = new Guid("{C70BC0E0-343C-486E-963C-5E08EFA0FC8D}");

		public const string HistoricalDebugging_string = "{D1B1E38F-1A7E-4236-AF55-6FA8F5FA76E6}";

		public static readonly Guid HistoricalDebugging_guid = new Guid("{D1B1E38F-1A7E-4236-AF55-6FA8F5FA76E6}");

		public const string CloudDebugging_string = "{C22BCF10-E1EB-42C6-95A5-E01418C08A29}";

		public static readonly Guid CloudDebugging_guid = new Guid("{C22BCF10-E1EB-42C6-95A5-E01418C08A29}");

		public const string NoSolution_string = "{ADFC4E64-0397-11D1-9F4E-00A0C911004F}";

		public static readonly Guid NoSolution_guid = new Guid("{ADFC4E64-0397-11D1-9F4E-00A0C911004F}");

		public const string SolutionClosing_string = "{DA9F8018-6EA4-48DF-BDB6-B85ABD8FC51E}";

		public static readonly Guid SolutionClosing_guid = new Guid("{DA9F8018-6EA4-48DF-BDB6-B85ABD8FC51E}");

		public const string NotBuildingAndNotDebugging_string = "{48EA4A80-F14E-4107-88FA-8D0016F30B9C}";

		public static readonly Guid NotBuildingAndNotDebugging_guid = new Guid("{48EA4A80-F14E-4107-88FA-8D0016F30B9C}");

		public const string OsWindows8OrHigher_string = "{67CFF80C-0863-4202-A4E4-CE80FDF8506E}";

		public static readonly Guid OsWindows8OrHigher_guid = new Guid("{67CFF80C-0863-4202-A4E4-CE80FDF8506E}");

		public const string ToolboxVisible_string = "{643905EE-DAE9-4F52-A343-6A5A7349D52C}";

		public static readonly Guid ToolboxVisible_guid = new Guid("{643905EE-DAE9-4F52-A343-6A5A7349D52C}");

		public const string ProjectRetargeting_string = "{DE039A0E-C18F-490C-944A-888B8E86DA4B}";

		public static readonly Guid ProjectRetargeting_guid = new Guid("{DE039A0E-C18F-490C-944A-888B8E86DA4B}");

		public const string RepositoryOpen_string = "{D8CDD15A-D1F0-4AD5-B0F4-2DE654546D5B}";

		public static readonly Guid RepositoryOpen_guid = new Guid("{D8CDD15A-D1F0-4AD5-B0F4-2DE654546D5B}");

		public const string SolutionBuilding_string = "{ADFC4E60-0397-11D1-9F4E-00A0C911004F}";

		public static readonly Guid SolutionBuilding_guid = new Guid("{ADFC4E60-0397-11D1-9F4E-00A0C911004F}");

		public const string SolutionExists_string = "{F1536EF8-92EC-443C-9ED7-FDADF150DA82}";

		public static readonly Guid SolutionExists_guid = new Guid("{F1536EF8-92EC-443C-9ED7-FDADF150DA82}");

		public const string SolutionExistsAndFullyLoaded_string = "{10534154-102D-46E2-ABA8-A6BFA25BA0BE}";

		public static readonly Guid SolutionExistsAndFullyLoaded_guid = new Guid("{10534154-102D-46E2-ABA8-A6BFA25BA0BE}");

		public const string SolutionExistsAndNotBuildingAndNotDebugging_string = "{D0E4DEEC-1B53-4CDA-8559-D454583AD23B}";

		public static readonly Guid SolutionExistsAndNotBuildingAndNotDebugging_guid = new Guid("{D0E4DEEC-1B53-4CDA-8559-D454583AD23B}");

		public const string SolutionHasMultipleProjects_string = "{93694FA0-0397-11D1-9F4E-00A0C911004F}";

		public static readonly Guid SolutionHasMultipleProjects_guid = new Guid("{93694FA0-0397-11D1-9F4E-00A0C911004F}");

		public const string SolutionHasSingleProject_string = "{ADFC4E66-0397-11D1-9F4E-00A0C911004F}";

		public static readonly Guid SolutionHasSingleProject_guid = new Guid("{ADFC4E66-0397-11D1-9F4E-00A0C911004F}");

		public const string SolutionHasAppContainerProject_string = "{7CAC4AE1-2E6B-4B02-A91C-71611E86F273}";

		public static readonly Guid SolutionHasAppContainerProject_guid = new Guid("{7CAC4AE1-2E6B-4B02-A91C-71611E86F273}");

		public const string SolutionOpening_string = "{D2567162-F94F-4091-8798-A096E61B8B50}";

		public static readonly Guid SolutionOpening_guid = new Guid("{D2567162-F94F-4091-8798-A096E61B8B50}");

		public const string FullSolutionLoading_string = "{164FD4DC-B2A4-448E-BB60-0583CD343D3B}";

		public static readonly Guid FullSolutionLoading_guid = new Guid("{164FD4DC-B2A4-448E-BB60-0583CD343D3B}");

		public const string BulkFileOperation_string = "{1F45BEB3-297F-49D9-9C18-069695B9031F}";

		public static readonly Guid BulkFileOperation_guid = new Guid("{1F45BEB3-297F-49D9-9C18-069695B9031F}");

		public const string SolutionOrProjectUpgrading_string = "{EF4F870B-7B85-4F29-9D15-CE1ABFBE733B}";

		public static readonly Guid SolutionOrProjectUpgrading_guid = new Guid("{EF4F870B-7B85-4F29-9D15-CE1ABFBE733B}");

		[Obsolete("Do not use this field.  The UIContext to which it refers will never be activated.")]
		public const string ProjectCreating_string = "{03BDEAC4-7186-458B-A2B0-941605D9917F}";

		[Obsolete("Do not use this field.  The UIContext to which it refers will never be activated.")]
		public static readonly Guid ProjectCreating_guid = new Guid("{03BDEAC4-7186-458B-A2B0-941605D9917F}");

		public const string ToolboxInitialized_string = "{DC5DB425-F0FD-4403-96A1-F475CDBA9EE0}";

		public static readonly Guid ToolboxInitialized_guid = new Guid("{DC5DB425-F0FD-4403-96A1-F475CDBA9EE0}");

		public const string ToolboxChooseItemsDataSourceInitialized_string = "{99E78B3D-93FF-4EDB-927F-79FF19F598D6}";

		public static readonly Guid ToolboxChooseItemsDataSourceInitialized_guid = new Guid("{99E78B3D-93FF-4EDB-927F-79FF19F598D6}");

		public const string VBProject_string = "{164B10B9-B200-11D0-8C61-00A0C91E29D5}";

		public static readonly Guid VBProject_guid = new Guid("{164B10B9-B200-11D0-8C61-00A0C91E29D5}");

		public const string CSharpProject_string = "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}";

		public static readonly Guid CSharpProject_guid = new Guid("{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}");

		public const string VCProject_string = "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";

		public static readonly Guid VCProject_guid = new Guid("{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}");

		public const string FSharpProject_string = "{F2A71F9B-5D33-465A-A702-920D77279786}";

		public static readonly Guid FSharpProject_guid = new Guid("{F2A71F9B-5D33-465A-A702-920D77279786}");

		public const string VBCodeAttribute_string = "{C28E28CA-E6DC-446F-BE1A-D496BEF8340A}";

		public static readonly Guid VBCodeAttribute_guid = new Guid("{C28E28CA-E6DC-446F-BE1A-D496BEF8340A}");

		public const string VBCodeClass_string = "{C28E28CA-E6DC-446F-BE1A-D496BEF83401}";

		public static readonly Guid VBCodeClass_guid = new Guid("{C28E28CA-E6DC-446F-BE1A-D496BEF83401}");

		public const string VBCodeDelegate_string = "{C28E28CA-E6DC-446F-BE1A-D496BEF83402}";

		public static readonly Guid VBCodeDelegate_guid = new Guid("{C28E28CA-E6DC-446F-BE1A-D496BEF83402}");

		public const string VBCodeEnum_string = "{C28E28CA-E6DC-446F-BE1A-D496BEF83408}";

		public static readonly Guid VBCodeEnum_guid = new Guid("{C28E28CA-E6DC-446F-BE1A-D496BEF83408}");

		public const string VBCodeFunction_string = "{C28E28CA-E6DC-446F-BE1A-D496BEF83400}";

		public static readonly Guid VBCodeFunction_guid = new Guid("{C28E28CA-E6DC-446F-BE1A-D496BEF83400}");

		public const string VBCodeInterface_string = "{C28E28CA-E6DC-446F-BE1A-D496BEF83406}";

		public static readonly Guid VBCodeInterface_guid = new Guid("{C28E28CA-E6DC-446F-BE1A-D496BEF83406}");

		public const string VBCodeNamespace_string = "{C28E28CA-E6DC-446F-BE1A-D496BEF83409}";

		public static readonly Guid VBCodeNamespace_guid = new Guid("{C28E28CA-E6DC-446F-BE1A-D496BEF83409}");

		public const string VBCodeParameter_string = "{C28E28CA-E6DC-446F-BE1A-D496BEF83405}";

		public static readonly Guid VBCodeParameter_guid = new Guid("{C28E28CA-E6DC-446F-BE1A-D496BEF83405}");

		public const string VBCodeProperty_string = "{C28E28CA-E6DC-446F-BE1A-D496BEF83404}";

		public static readonly Guid VBCodeProperty_guid = new Guid("{C28E28CA-E6DC-446F-BE1A-D496BEF83404}");

		public const string VBCodeStruct_string = "{C28E28CA-E6DC-446F-BE1A-D496BEF83407}";

		public static readonly Guid VBCodeStruct_guid = new Guid("{C28E28CA-E6DC-446F-BE1A-D496BEF83407}");

		public const string VBCodeVariable_string = "{C28E28CA-E6DC-446F-BE1A-D496BEF83403}";

		public static readonly Guid VBCodeVariable_guid = new Guid("{C28E28CA-E6DC-446F-BE1A-D496BEF83403}");

		[Obsolete("Obsolete as of Visual Studio 2019.")]
		public const string BackgroundProjectLoad_string = "{dc769521-31a2-41a5-9bbb-210b5d63568d}";

		[Obsolete("Obsolete as of Visual Studio 2019.")]
		public static readonly Guid BackgroundProjectLoad_guid = new Guid("{dc769521-31a2-41a5-9bbb-210b5d63568d}");

		public const string StandardPreviewerConfigurationChanging_string = "{6D3CAD8E-9129-4ec0-929E-69B6F5D4400D}";

		public static readonly Guid StandardPreviewerConfigurationChanging_guid = new Guid("{6D3CAD8E-9129-4ec0-929E-69B6F5D4400D}");

		public const string IdeUserSignedIn_string = "{6FB82950-B2F8-4F94-9417-506703704DB2}";

		public static readonly Guid IdeUserSignedIn_guid = new Guid("{6FB82950-B2F8-4F94-9417-506703704DB2}");

		public const string SolutionHasSilverlightWindowsPhoneProject_string = "781D1330-8DE9-429D-BF73-C74F19E4FCB1";

		public const string WizardOpen_string = "{C3DA54E0-794F-440C-8655-DA03CD0DD05E}";

		public static readonly Guid WizardOpen_guid = new Guid("{C3DA54E0-794F-440C-8655-DA03CD0DD05E}");

		public const string SolutionHasWindowsPhone80NativeProject_string = "de9f6b31-c1e5-b965-95f3-1885af956fc9";

		public const string SynchronousSolutionOperation_string = "{30315F71-BB05-436B-8CC1-6A62B368C842}";

		public static readonly Guid SynchronousSolutionOperation_guid = new Guid("{30315F71-BB05-436B-8CC1-6A62B368C842}");

		public const string SharedMSBuildFilesManagerHierarchyLoaded_string = "{22912BB2-3FF9-4D55-B4DB-D210B6035D4C}";

		public static readonly Guid SharedMSBuildFilesManagerHierarchyLoaded_guid = new Guid("{22912BB2-3FF9-4D55-B4DB-D210B6035D4C}");

		public const string ShellInitialized_string = "{E80EF1CB-6D64-4609-8FAA-FEACFD3BC89F}";

		public static readonly Guid ShellInitialized_guid = new Guid("{E80EF1CB-6D64-4609-8FAA-FEACFD3BC89F}");

		public const string FolderOpened_string = "{4646B819-1AE0-4E79-97F4-8A8176FDD664}";

		public static readonly Guid FolderOpened_guid = new Guid("{4646B819-1AE0-4E79-97F4-8A8176FDD664}");

		public const string DocumentWindowActive_string = "{8F0F3ED3-2241-4638-95CE-D8D5C5222C1D}";

		public static readonly Guid DocumentWindowActive_guid = new Guid("{8F0F3ED3-2241-4638-95CE-D8D5C5222C1D}");

		public const string ToolWindowActive_string = "{30840431-1832-47A3-94A4-64E5EC71B0CD}";

		public static readonly Guid ToolWindowActive_guid = new Guid("{30840431-1832-47A3-94A4-64E5EC71B0CD}");

		public const string CloudEnvironmentConnected_string = "{CE73BF3D-D614-438A-9B93-24E9E9D7453A}";

		public static readonly Guid CloudEnvironmentConnected_guid = new Guid("{CE73BF3D-D614-438A-9B93-24E9E9D7453A}");

		public const string OutputWindowCreated_string = "{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}";

		public static readonly Guid OutputWindowCreated_guid = StandardToolWindows.Output;

		public const string XamlDesignerContext_string = "{E9B8485C-1217-4277-9ED6-C825A5AC1968}";

		public static readonly Guid XamlDesignerContext_guid = new Guid("{E9B8485C-1217-4277-9ED6-C825A5AC1968}");
	}

	public static class VsTaskListView
	{
		public static readonly Guid All = new Guid("{1880202e-fc20-11d2-8bb1-00c04f8ec28c}");

		public static readonly Guid UserTasks = new Guid("{1880202f-fc20-11d2-8bb1-00c04f8ec28c}");

		public static readonly Guid ShortcutTasks = new Guid("{18802030-fc20-11d2-8bb1-00c04f8ec28c}");

		public static readonly Guid HTMLTasks = new Guid("{36ac1c0d-fe86-11d2-8bb1-00c04f8ec28c}");

		public static readonly Guid CompilerTasks = new Guid("{18802033-fc20-11d2-8bb1-00c04f8ec28c}");

		public static readonly Guid CommentTasks = new Guid("{18802034-fc20-11d2-8bb1-00c04f8ec28c}");

		public static readonly Guid CurrentFileTasks = new Guid("{18802035-fc20-11d2-8bb1-00c04f8ec28c}");

		public static readonly Guid CheckedTasks = new Guid("{18802036-fc20-11d2-8bb1-00c04f8ec28c}");

		public static readonly Guid UncheckedTasks = new Guid("{18802037-fc20-11d2-8bb1-00c04f8ec28c}");
	}

	public static class StandardToolWindows
	{
		public static readonly Guid ApplicationVerifier = new Guid("{637792AA-F332-4BB5-BE6C-066B0E88ECED}");

		public static readonly Guid Autos = new Guid("{F2E84780-2AF1-11D1-A7FA-00A0C9110051}");

		public static readonly Guid Behaviors = new Guid("{56B32054-DE4D-4de3-8396-BCB6F98BD246}");

		public static readonly Guid Bookmarks = new Guid("{A0C5197D-0AC7-4B63-97CD-8872A789D233}");

		public static readonly Guid Breakpoints = new Guid("{BE4D7042-BA3F-11D2-840E-00C04F9902C1}");

		public static readonly Guid CSSApplyStyles = new Guid("{402DC223-D700-4029-866F-ACEE803F3F0C}");

		public static readonly Guid CSSManageStyles = new Guid("{38ED9834-0C97-445b-BD1D-F78F3E08AFAC}");

		public static readonly Guid CSSProperties = new Guid("{A9B00010-7308-415c-95C6-EED62C1B9788}");

		public static readonly Guid CSSPropertyGrid = new Guid("{1CBA9826-3184-4799-A184-784E41B56398}");

		public static readonly Guid CallBrowser = new Guid("{5415EA3A-D813-4948-B51E-562082CE0887}");

		public static readonly Guid CallBrowserSecondary = new Guid("{F78BCC56-71F7-4E7D-8215-F690CAE4F452}");

		public static readonly Guid CallHierarchy = new Guid("{3822E751-EB69-4B0E-B301-595A9E4C74D5}");

		public static readonly Guid CallStack = new Guid("{0504FF91-9D61-11D0-A794-00A0C9110051}");

		public static readonly Guid ClassDetails = new Guid("{778B5376-AD77-4751-ACDC-F3D18343F8DD}");

		public static readonly Guid ClassView = new Guid("{C9C0AE26-AA77-11d2-B3F0-0000F87570EE}");

		public static readonly Guid CodeCoverageResults = new Guid("{905DA7D1-18FD-4A46-8D0F-A5FF58ADA9DE}");

		public static readonly Guid CodeDefinition = new Guid("{588470CC-84F8-4a57-9AC4-86BCA0625FF4}");

		public static readonly Guid CodeMetrics = new Guid("{9A7CEBBB-DC5C-4986-BC49-962DA46AA506}");

		public static readonly Guid ColorPalette = new Guid("{5B6781C0-E99D-11D0-9954-00A0C91BC8E5}");

		public static readonly Guid Command = new Guid("{28836128-FC2C-11D2-A433-00C04F72D18A}");

		public static readonly Guid ConditionalFormatting = new Guid("{6FB4A4D9-0C08-4663-AF7B-2ECBDF7A20EC}");

		public static readonly Guid ConsoleIO = new Guid("{FC29E0C0-C1AB-4B30-B5DF-24AA452B9661}");

		public static readonly Guid DBProEventMonitor = new Guid("{F16E7758-BFD9-4360-A45F-6DEEAE786164}");

		public static readonly Guid DataCollectionControl = new Guid("{47A7D881-D3CF-4036-B57C-0444E12DF881}");

		public static readonly Guid DataGenerationDetails = new Guid("{E3369CF0-996F-45BA-881E-2AF696FBE27B}");

		public static readonly Guid DataGenerationPreview = new Guid("{F044F2C2-3D99-4787-A492-6B09A19DF7C0}");

		public static readonly Guid DataSource = new Guid("{873151D0-CF2E-48cc-B4BF-AD0394F6A3C3}");

		public static readonly Guid DatabaseSchemaView = new Guid("{9C7D10E9-0147-4363-BF48-917F0426CD03}");

		public static readonly Guid DebugHistory = new Guid("{ed485b08-5acf-4ce9-8e13-699174ea0201}");

		public static readonly Guid DeviceSecurityManager = new Guid("{E5C2CCE5-61D0-4CD8-A946-13EC76CFDB01}");

		public static readonly Guid Disassembly = new Guid("{CF577B8C-4134-11D2-83E5-00C04F9902C1}");

		public static readonly Guid DocumentOutline = new Guid("{25F7E850-FFA1-11D0-B63F-00A0C922E851}");

		public static readonly Guid EntityMappingDetails = new Guid("{cdbdee54-b399-484b-b763-db2c3393d646}");

		public static readonly Guid EntityModelBrowser = new Guid("{A34B1C5D-6D37-4A0C-A8B0-99F8E8158B48}");

		public static readonly Guid ErrorList = new Guid("{D78612C7-9962-4B83-95D9-268046DAD23A}");

		public static readonly Guid Find1 = new Guid("{0F887920-C2B6-11d2-9375-0080C747D9A0}");

		public static readonly Guid Find2 = new Guid("{0F887921-C2B6-11d2-9375-0080C747D9A0}");

		public static readonly Guid FindInFiles = new Guid("{E830EC50-C2B5-11d2-9375-0080C747D9A0}");

		public static readonly Guid FindReplace = new Guid("{CF2DDC32-8CAD-11d2-9302-005345000000}");

		public static readonly Guid FindSymbol = new Guid("{53024D34-0EF5-11d3-87E0-00C04F7971A5}");

		public static readonly Guid FindSymbolResults = new Guid("{68487888-204A-11d3-87EB-00C04F7971A5}");

		public static readonly Guid HTMLPropertyGrid = new Guid("{F62AF5AD-1276-46dd-AE7B-D07AB54D1081}");

		public static readonly Guid Immediate = new Guid("{ECB7191A-597B-41F5-9843-03A4CF275DDE}");

		public static readonly Guid Layers = new Guid("{7B8C4981-13EC-4c56-9F24-ABE5FAAA9440}");

		public static readonly Guid LoadTest = new Guid("{CB4D394C-6408-4607-8C42-0910D3147A4E}");

		public static readonly Guid LoadTestPostRun = new Guid("{93A69444-E846-4571-9E03-A8433AD9DDF9}");

		public static readonly Guid LocalChanges = new Guid("{53544C4D-5C18-11d3-AB71-0050040AE094}");

		public static readonly Guid Locals = new Guid("{4A18F9D0-B838-11D0-93EB-00A0C90F2734}");

		public static readonly Guid MacroExplorer = new Guid("{07CD18B4-3BA1-11d2-890A-0060083196C6}");

		public static readonly Guid ManualTestExecution = new Guid("{3ADDF8E2-81CC-41A0-9785-DBD2D86064BF}");

		public static readonly Guid Modules = new Guid("{37ABA9BE-445A-11D3-9949-00C04F68FD0A}");

		public static readonly Guid ObjectBrowser = new Guid("{269A02DC-6AF8-11D3-BDC4-00C04F688E50}");

		public static readonly Guid ObjectTestBench = new Guid("{FDFFCCF2-5F63-404F-86AD-33693F544948}");

		public const string Output_string = "{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}";

		public static readonly Guid Output = new Guid("{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}");

		public static readonly Guid ParallelStacks = new Guid("{B9A151CE-EF7C-4fe1-A6AA-4777E6E518F3}");

		public static readonly Guid ParallelTasks = new Guid("{8D263989-FF4B-4a78-90C8-B2BA3FA69311}");

		public static readonly Guid PendingCheckIn = new Guid("{2456BD12-ECF7-4988-A4A6-67D49173F564}");

		public static readonly Guid PerformanceExplorer = new Guid("{099CA9EA-0AE4-4E31-A7E4-FE09BD1715CC}");

		public static readonly Guid Processes = new Guid("{51C76317-9037-4CF2-A20A-6206FD30B4A1}");

		public static readonly Guid Properties = new Guid("{EEFA5220-E298-11D0-8F78-00A0C9110057}");

		public static readonly Guid PropertyManager = new Guid("{6B8E94B5-0949-4d9c-A81F-C1B9B744185C}");

		public static readonly Guid Registers = new Guid("{CA4B8FF5-BFC7-11D2-9929-00C04F68FDAF}");

		public static readonly Guid ResourceView = new Guid("{2D7728C2-DE0A-45b5-99AA-89B609DFDE73}");

		public static readonly Guid RunningDocuments = new Guid("{ECDD9EE0-AC6B-11D0-89F9-00A0C9110055}");

		public static readonly Guid SQLSchemaUpdateScript = new Guid("{F2C4BE33-CA39-41a6-A69A-F4ED439D4178}");

		public static readonly Guid ServerExplorer = new Guid("{74946827-37a0-11d2-a273-00c04f8ef4ff}");

		public static readonly Guid SolutionExplorer = new Guid("{3AE79031-E1BC-11D0-8F78-00A0C9110057}");

		public static readonly Guid SourceControlExplorer = new Guid("{99B8FA2F-AB90-4F57-9C32-949F146F1914}");

		public static readonly Guid SourceHistory = new Guid("{2456BD12-ECF7-4988-A4A6-67D49173F565}");

		public static readonly Guid StartPage = new Guid("{387cb18d-6153-4156-9257-9ac3f9207bbe}");

		public static readonly Guid StyleOrganizer = new Guid("{A764E899-518D-11d2-9A89-00C04F79EFC3}");

		public static readonly Guid TaskList = new Guid("{4A9B7E51-AA16-11D0-A8C5-00A0C921A4D2}");

		public static readonly Guid TeamExplorer = new Guid("{131369F2-062D-44A2-8671-91FF31EFB4F4}");

		public static readonly Guid TestImpactView = new Guid("{0DB31CC8-2322-4f59-A610-1FDC8423DF77}");

		public static readonly Guid TestManager = new Guid("{C79B74FF-F1D7-4C94-AEFA-4D22BFE1B1F9}");

		public static readonly Guid TestResults = new Guid("{519E8A32-1C95-4A42-956F-2CEE2F28EB0F}");

		public static readonly Guid TestRunQueue = new Guid("{92547016-2BD0-4DFE-BD4F-5B52BDCE0037}");

		public static readonly Guid TestView = new Guid("{3ADDF8E2-81CC-41A0-9785-DBD2D86064BD}");

		public static readonly Guid Threads = new Guid("{E62CE6A0-B439-11D0-A79D-00A0C9110051}");

		public static readonly Guid Toolbox = new Guid("{B1E99781-AB81-11D0-B683-00AA00A3EE26}");

		public static readonly Guid UAMSynchronizations = new Guid("{A94C758F-EFB0-4975-BF86-C87B59FDB45D}");

		public static readonly Guid UserNotifications = new Guid("{c93a910a-0fa6-4307-93a4-f2bd61ec7828}");

		public static readonly Guid VCPPPropertyManager = new Guid("{DE1FC918-F32E-4DD7-A915-1792A051F26B}");

		public static readonly Guid VSMDPropertyBrowser = new Guid("{74946810-37a0-11d2-a273-00c04f8ef4ff}");

		public static readonly Guid VSTOAddBookmark = new Guid("{FF863E2F-29C9-4686-95D8-5A2D5B4D72CE}");

		public static readonly Guid Watch = new Guid("{90243340-BD7A-11D0-93EF-00A0C90F2734}");

		public static readonly Guid WebBrowser = new Guid("{e8b06f52-6d01-11d2-aa7d-00c04f990343}");

		public static readonly Guid WebBrowserPreview = new Guid("{e8b06f53-6d01-11d2-aa7d-00c04f990343}");

		public static readonly Guid WebPartGallery = new Guid("{A693A243-4743-4034-AED4-BEC4E79E0B3B}");

		public static readonly Guid XMLSchemaExplorer = new Guid("{DD1DDD20-D59B-11DA-A94D-0800200C9A66}");
	}

	public static class ReferenceManagerHandler
	{
		public const string guidRecentMenuCmdSetString = "8206e3a8-09d6-4f97-985f-7b980b672a97";

		public static readonly Guid guidRecentMenuCmdSet = new Guid("8206e3a8-09d6-4f97-985f-7b980b672a97");

		public const uint cmdidClearRecentReferences = 256u;

		public const uint cmdidRemoveFromRecentReferences = 512u;
	}

	public static class ComponentSelectorPageGuid
	{
		public const string ManagedAssemblyPage_string = "{9A341D95-5A64-11D3-BFF9-00C04F990235}";

		public static readonly Guid ManagedAssemblyPage_guid = new Guid("{9A341D95-5A64-11D3-BFF9-00C04F990235}");

		public const string COMPage_string = "{9A341D96-5A64-11D3-BFF9-00C04F990235}";

		public static readonly Guid COMPage_guid = new Guid("{9A341D96-5A64-11D3-BFF9-00C04F990235}");

		public const string ProjectsPage_string = "{9A341D97-5A64-11D3-BFF9-00C04F990235}";

		public static readonly Guid ProjectsPage_guid = new Guid("{9A341D97-5A64-11D3-BFF9-00C04F990235}");
	}

	public static class LOGVIEWID
	{
		public const string Any_string = "{FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF}";

		public static readonly Guid Any_guid = new Guid("{FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF}");

		public const string Code_string = "{7651A701-06E5-11D1-8EBD-00A0C90F26EA}";

		public static readonly Guid Code_guid = new Guid("{7651A701-06E5-11D1-8EBD-00A0C90F26EA}");

		public const string Debugging_string = "{7651A700-06E5-11D1-8EBD-00A0C90F26EA}";

		public static readonly Guid Debugging_guid = new Guid("{7651A700-06E5-11D1-8EBD-00A0C90F26EA}");

		public const string Designer_string = "{7651A702-06E5-11D1-8EBD-00A0C90F26EA}";

		public static readonly Guid Designer_guid = new Guid("{7651A702-06E5-11D1-8EBD-00A0C90F26EA}");

		public const string ProjectSpecificEditor_string = "{80A3471A-6B87-433E-A75A-9D461DE0645F}";

		public static readonly Guid ProjectSpecificEditor_guid = new Guid("{80A3471A-6B87-433E-A75A-9D461DE0645F}");

		public static readonly Guid Primary_guid = Guid.Empty;

		public const string TextView_string = "{7651A703-06E5-11D1-8EBD-00A0C90F26EA}";

		public static readonly Guid TextView_guid = new Guid("{7651A703-06E5-11D1-8EBD-00A0C90F26EA}");

		public const string UserChooseView_string = "{7651A704-06E5-11D1-8EBD-00A0C90F26EA}";

		public static readonly Guid UserChooseView_guid = new Guid("{7651A704-06E5-11D1-8EBD-00A0C90F26EA}");
	}

	public static class StandardNavigateToFilterShortcuts
	{
		public const string Help = "Navigate To Help";

		public const string Line = "Navigate To Lines";

		public const string Files = "Navigate To Files";

		public const string CurrentProjectFiles = "Navigate To Current Project Files";

		public const string Symbols = "Navigate To Symbols";

		public const string CurrentProjectSymbols = "Navigate To Current Project Symbols";

		public const string CurrentDocumentSymbols = "Navigate To Current Document Symbols";

		public const string TypeSymbols = "Navigate To Type Symbols";

		public const string Members = "Navigate To Members";

		public const string RecentFiles = "Navigate To Recent Files";
	}

	public static class StandardNavigateToKindFilters
	{
		public const string Line = "Navigate To Line";

		public const string File = "Navigate To File";

		public const string Class = "Navigate To Class";

		public const string Structure = "Navigate To Structure";

		public const string Interface = "Navigate To Interface";

		public const string Delegate = "Navigate To Delegate";

		public const string Enum = "Navigate To Enum";

		public const string Module = "Navigate To Module";

		public const string Constant = "Navigate To Constant";

		public const string EnumItem = "Navigate To EnumItem";

		public const string Field = "Navigate To Field";

		public const string Method = "Navigate To Method";

		public const string Property = "Navigate To Property";

		public const string Event = "Navigate To Event";

		public const string OtherSymbol = "Navigate To Other Symbol";

		public const string RecentFile = "Navigate To Recent File";
	}

	public static class StandardNavigateToDocumentScopeFilters
	{
		public const string CurrentDocument = "Navigate To Current Document";

		public const string CurrentProject = "Navigate To Current Project";

		public const string OpenDocuments = "Navigate To Open Documents";
	}

	public static class DebugTargetHandler
	{
		public const string guidDebugTargetHandlerCmdSetString = "6E87CFAD-6C05-4adf-9CD7-3B7943875B7C";

		public static readonly Guid guidDebugTargetHandlerCmdSet = new Guid("6E87CFAD-6C05-4adf-9CD7-3B7943875B7C");

		public const uint cmdidDebugTargetAnchorItem = 257u;

		public const uint cmdidDebugTargetAnchorItemNoAttachToProcess = 258u;

		public const uint cmdidGenericDebugTarget = 512u;

		public const uint cmdidDebugTypeCombo = 16u;

		public const uint cmdidDebugTypeItemHandler = 17u;
	}

	public static class AppPackageDebugTargets
	{
		public const string guidAppPackageDebugTargetCmdSetString = "FEEA6E9D-77D8-423F-9EDE-3970CBB76125";

		public static readonly Guid guidAppPackageDebugTargetCmdSet = new Guid("FEEA6E9D-77D8-423F-9EDE-3970CBB76125");

		public const uint cmdidAppPackage_Simulator = 256u;

		public const uint cmdidAppPackage_LocalMachine = 512u;

		public const uint cmdidAppPackage_TetheredDevice = 768u;

		public const uint cmdidAppPackage_RemoteMachine = 1024u;

		public const uint cmdidAppPackage_Emulator = 1280u;
	}

	public enum VSITEMID : uint
	{
		Nil = uint.MaxValue,
		Root = 4294967294u,
		Selection = 4294967293u
	}

	public enum SelectionElement : uint
	{
		UndoManager,
		WindowFrame,
		DocumentFrame,
		StartupProject,
		PropertyBrowserSID,
		UserContext,
		ResultList,
		LastWindowFrame
	}

	[Flags]
	public enum VsUIAccelModifiers : uint
	{
		VSAM_None = 0u,
		VSAM_Shift = 1u,
		VSAM_Control = 2u,
		VSAM_Alt = 4u,
		VSAM_Windows = 8u
	}

	public enum VsSearchNavigationKeys : uint
	{
		SNK_Enter,
		SNK_Down,
		SNK_Up,
		SNK_PageDown,
		SNK_PageUp,
		SNK_Home,
		SNK_End,
		SNK_Escape
	}

	public enum VsSearchTaskStatus : uint
	{
		Created,
		Started,
		Completed,
		Stopped,
		Error
	}

	public enum MessageBoxResult
	{
		IDOK = 1,
		IDCANCEL,
		IDABORT,
		IDRETRY,
		IDIGNORE,
		IDYES,
		IDNO,
		IDCLOSE,
		IDHELP,
		IDTRYAGAIN,
		IDCONTINUE
	}

	public static class MruList
	{
		public const string Projects_string = "A9C4A31F-F9CB-47A9-ABC0-49CE82D0B3AC";

		public static readonly Guid Projects = new Guid("A9C4A31F-F9CB-47A9-ABC0-49CE82D0B3AC");

		public const string Files_string = "01235AAD-8F1B-429F-9D02-61A0101EA275";

		public static readonly Guid Files = new Guid("01235AAD-8F1B-429F-9D02-61A0101EA275");

		public const string SolutionFiles_string = "335041A8-B61A-4E9F-B0FE-D42DFA193855";

		public static readonly Guid SolutionFiles = new Guid("335041A8-B61A-4E9F-B0FE-D42DFA193855");
	}

	public static class SearchProviderNames
	{
		public const string HierarchySearchProviderName = "HierarchySearchProvider";

		public const string GraphSearchProviderName = "GraphSearchProvider";

		public const string DependenciesTreeSearchProviderName = "DependenciesTreeSearchProvider";
	}

	public static class WellKnownWindowReferences
	{
		public const string DocumentWell = "DocumentWell";
	}

	public const int cmdidToolsOptions = 264;

	public static readonly Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");

	public static readonly Guid GUID_AppCommand = new Guid("{12F1A339-02B9-46E6-BDAF-1071F76056BF}");

	public static readonly Guid GUID_VSStandardCommandSet97 = new Guid("{5EFC7975-14BC-11CF-9B2B-00AA00573819}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid VSStd2K = new Guid("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid VsStd2010 = new Guid("{5DD0BB59-7076-4C59-88D3-DE36931F63F0}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid VsStd11 = new Guid("{D63DB1F0-404E-4B21-9648-CA8D99245EC3}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid VsStd12 = new Guid("{2A8866DC-7BDE-4dc8-A360-A60679534384}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid VsStd14 = new Guid("{4C7763BF-5FAF-4264-A366-B7E1F27BA958}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid VsStd15 = new Guid("{712C6C80-883B-4AAD-B430-BBCA5256FA9D}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid VsStd16 = CMDSETID.StandardCommandSet16_guid;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid VsStd17 = CMDSETID.StandardCommandSet17_guid;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public const uint CEF_CLONEFILE = 1u;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public const uint CEF_OPENFILE = 2u;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public const uint CEF_SILENT = 4u;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public const uint CEF_OPENASNEW = 8u;

	[EditorBrowsable(EditorBrowsableState.Never)]
	// FOR DOUBLECLICK
	public static readonly Guid GUID_VsUIHierarchyWindowCmds = new Guid("{60481700-078B-11D1-AAF8-00A0C9055A90}");

	public static readonly IntPtr HIERARCHY_DONTCHANGE = new IntPtr(-1);

	public static readonly IntPtr SELCONTAINER_DONTCHANGE = new IntPtr(-1);

	public static readonly IntPtr HIERARCHY_DONTPROPAGATE = new IntPtr(-2);

	public static readonly IntPtr SELCONTAINER_DONTPROPAGATE = new IntPtr(-2);

	public const string MiscFilesProjectUniqueName = "<MiscFiles>";

	public const string SolutionItemsProjectUniqueName = "<SolnItems>";

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid CLSID_HtmDocData = new Guid("{62C81794-A9EC-11D0-8198-00A0C91BBEE3}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid CLSID_HtmedPackage = new Guid("{1B437D20-F8FE-11D2-A6AE-00104BCC7269}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid CLSID_HtmlLanguageService = new Guid("{58E975A0-F8FE-11D2-A6AE-00104BCC7269}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_HtmlEditorFactory = new Guid("{C76D83F8-A489-11D0-8195-00A0C91BBEE3}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_TextEditorFactory = new Guid("{8B382828-6202-11D1-8870-0000F87579D2}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_HTMEDAllowExistingDocData = new Guid("{5742D216-8071-4779-BF5F-A24D5F3142BA}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid CLSID_VsEnvironmentPackage = new Guid("{DA9FB551-C724-11D0-AE1F-00A0C90FFFC3}");

	public static readonly Guid GUID_VsNewProjectPseudoFolder = new Guid("{DCF2A94A-45B0-11D1-ADBF-00C04FB6BE4C}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid CLSID_MiscellaneousFilesProject = new Guid("{A2FE74E1-B743-11D0-AE1A-00A0C90FFFC3}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid CLSID_SolutionItemsProject = new Guid("{D1DCDB85-C5E8-11D2-BFCA-00C04F990235}");

	public static readonly Guid SID_SVsGeneralOutputWindowPane = new Guid("{65482C72-DEFA-41B7-902C-11C091889C83}");

	public static readonly Guid SID_SUIHostCommandDispatcher = new Guid("{E69CD190-1276-11D1-9F64-00A0C911004F}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid CLSID_VsUIHierarchyWindow = new Guid("{7D960B07-7AF8-11D0-8E5E-00A0C911005A}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_DefaultEditor = new Guid("{6AC5EF80-12BF-11D1-8E9B-00A0C911005A}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_ExternalEditor = new Guid("{8137C9E8-35FE-4AF2-87B0-DE3C45F395FD}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_BuildOutputWindowPane = new Guid("{1BD8A850-02D1-11d1-BEE7-00A0C913D1F8}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_OutWindowDebugPane = new Guid("{FC076020-078A-11D1-A7DF-00A0C9110051}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_OutWindowGeneralPane = new Guid("{3C24D581-5591-4884-A571-9FE89915CD64}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid BuildOrder = new Guid("2032B126-7C8D-48AD-8026-0E0348004FC0");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid BuildOutput = new Guid("1BD8A850-02D1-11D1-BEE7-00A0C913D1F8");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid DebugOutput = new Guid("FC076020-078A-11D1-A7DF-00A0C9110051");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_ItemType_PhysicalFile = new Guid("{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_ItemType_PhysicalFolder = new Guid("{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_ItemType_VirtualFolder = new Guid("{6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_ItemType_SubProject = new Guid("{EA6618E8-6E24-4528-94BE-6889FE16485C}");

	public static readonly Guid GUID_BrowseFilePage = new Guid("2483F435-673D-4FA3-8ADD-B51442F65349");

	public static readonly Guid guidCOMPLUSLibrary = new Guid(516370391u, 51232, 17011, 154, 33, 119, 122, 92, 82, 46, 3);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid CLSID_ComPlusOnlyDebugEngine = new Guid("449EC4CC-30D2-4032-9256-EE18EB41B62B");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_ItemType_SharedProjectReference = new Guid("{FBA6BD9A-47F3-4C04-BDC0-7F76A9E2E582}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_VS_DEPTYPE_BUILD_PROJECT = new Guid("707d11b6-91ca-11d0-8a3e-00a0c91e2acd");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_ProjectDesignerEditor = new Guid("04b8ab82-a572-4fef-95ce-5222444b6b64");

	public const uint VS_BUILDABLEPROJECTCFGOPTS_REBUILD = 1u;

	public const uint VS_BUILDABLEPROJECTCFGOPTS_BUILD_SELECTION_ONLY = 2u;

	public const uint VS_BUILDABLEPROJECTCFGOPTS_BUILD_ACTIVE_DOCUMENT_ONLY = 4u;

	public const uint VS_BUILDABLEPROJECTCFGOPTS_PACKAGE = 8u;

	public const uint VS_BUILDABLEPROJECTCFGOPTS_PRIVATE = 4294901760u;

	public const uint VSUTDCF_DTEEONLY = 1u;

	public const uint VSUTDCF_REBUILD = 2u;

	public const uint VSUTDCF_PACKAGE = 4u;

	public const uint VSUTDCF_PRIVATE = 4294901760u;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid UICONTEXT_SolutionBuilding = new Guid("{adfc4e60-0397-11d1-9f4e-00a0c911004f}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid UICONTEXT_Debugging = new Guid("{adfc4e61-0397-11d1-9f4e-00a0c911004f}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid UICONTEXT_Dragging = new Guid("{b706f393-2e5b-49e7-9e2e-b1825f639b63}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid UICONTEXT_FullScreenMode = new Guid("{adfc4e62-0397-11d1-9f4e-00a0c911004f}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid UICONTEXT_DesignMode = new Guid("{adfc4e63-0397-11d1-9f4e-00a0c911004f}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid UICONTEXT_NoSolution = new Guid("{adfc4e64-0397-11d1-9f4e-00a0c911004f}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid UICONTEXT_SolutionExists = new Guid("{f1536ef8-92ec-443c-9ed7-fdadf150da82}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid UICONTEXT_EmptySolution = new Guid("{adfc4e65-0397-11d1-9f4e-00a0c911004f}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid UICONTEXT_SolutionHasSingleProject = new Guid("{adfc4e66-0397-11d1-9f4e-00a0c911004f}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid UICONTEXT_SolutionHasMultipleProjects = new Guid("{93694fa0-0397-11d1-9f4e-00a0c911004f}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid UICONTEXT_CodeWindow = new Guid("{8fe2df1d-e0da-4ebe-9d5c-415d40e487b5}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid UICONTEXT_SolutionHasAppContainerProject = new Guid("{7CAC4AE1-2E6B-4B02-A91C-71611E86F273}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid UIContext_SolutionClosing = new Guid("{DA9F8018-6EA4-48DF-BDB6-B85ABD8FC51E}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_VsTaskListViewAll = new Guid("{1880202e-fc20-11d2-8bb1-00c04f8ec28c}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_VsTaskListViewUserTasks = new Guid("{1880202f-fc20-11d2-8bb1-00c04f8ec28c}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_VsTaskListViewShortcutTasks = new Guid("{18802030-fc20-11d2-8bb1-00c04f8ec28c}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_VsTaskListViewHTMLTasks = new Guid("{36ac1c0d-fe86-11d2-8bb1-00c04f8ec28c}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_VsTaskListViewCompilerTasks = new Guid("{18802033-fc20-11d2-8bb1-00c04f8ec28c}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_VsTaskListViewCommentTasks = new Guid("{18802034-fc20-11d2-8bb1-00c04f8ec28c}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_VsTaskListViewCurrentFileTasks = new Guid("{18802035-fc20-11d2-8bb1-00c04f8ec28c}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_VsTaskListViewCheckedTasks = new Guid("{18802036-fc20-11d2-8bb1-00c04f8ec28c}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_VsTaskListViewUncheckedTasks = new Guid("{18802037-fc20-11d2-8bb1-00c04f8ec28c}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid CLSID_VsTaskList = new Guid("{BC5955D5-aa0d-11d0-a8c5-00a0c921a4d2}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid CLSID_VsTaskListPackage = new Guid("{4A9B7E50-aa16-11d0-a8c5-00a0c921a4d2}");

	public static readonly Guid SID_SVsToolboxActiveXDataProvider = new Guid("{35222106-bb44-11d0-8c46-00c04fc2aae2}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid CLSID_VsDocOutlinePackage = new Guid("{21af45b0-ffa5-11d0-b63f-00a0c922e851}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid CLSID_VsCfgProviderEventsHelper = new Guid("{99913f1f-1ee3-11d1-8a6e-00c04f682e21}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_COMPlusPage = new Guid("{9A341D95-5A64-11d3-BFF9-00C04F990235}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_COMClassicPage = new Guid("{9A341D96-5A64-11d3-BFF9-00C04F990235}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid GUID_SolutionPage = new Guid("{9A341D97-5A64-11d3-BFF9-00C04F990235}");

	public const string AssemblyReferenceProvider_string = "{9A341D95-5A64-11D3-BFF9-00C04F990235}";

	public static readonly Guid AssemblyReferenceProvider_Guid = new Guid("{9A341D95-5A64-11D3-BFF9-00C04F990235}");

	public const string ProjectReferenceProvider_string = "{51ECA6BD-5AE4-43F0-AA76-DD0A7B08F40C}";

	public static readonly Guid ProjectReferenceProvider_Guid = new Guid("{51ECA6BD-5AE4-43F0-AA76-DD0A7B08F40C}");

	public const string ComReferenceProvider_string = "{4560BE15-8871-482A-801D-76AA47F1763A}";

	public static readonly Guid ComReferenceProvider_Guid = new Guid("{4560BE15-8871-482A-801D-76AA47F1763A}");

	public const string PlatformReferenceProvider_string = "{97324595-E3F9-4AA8-85B7-DC941E812152}";

	public static readonly Guid PlatformReferenceProvider_Guid = new Guid("{97324595-E3F9-4AA8-85B7-DC941E812152}");

	public const string FileReferenceProvider_string = "{7B069159-FF02-4752-93E8-96B3CADF441A}";

	public static readonly Guid FileReferenceProvider_Guid = new Guid("{7B069159-FF02-4752-93E8-96B3CADF441A}");

	public const string ConnectedServiceInstanceReferenceProvider_string = "{C18E5D73-E6D1-43AA-AC5E-58D82E44DA9C}";

	public static readonly Guid ConnectedServiceInstanceReferenceProvider_Guid = new Guid("{C18E5D73-E6D1-43AA-AC5E-58D82E44DA9C}");

	public const string SharedProjectReferenceProvider_string = "{88B47069-C019-4EEC-B69C-3C8630F83BA5}";

	public static readonly Guid SharedProjectReferenceProvider_Guid = new Guid("{88B47069-C019-4EEC-B69C-3C8630F83BA5}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid LOGVIEWID_Any = new Guid(uint.MaxValue, ushort.MaxValue, ushort.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid LOGVIEWID_Primary = Guid.Empty;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid LOGVIEWID_Debugging = new Guid("{7651A700-06E5-11D1-8EBD-00A0C90F26EA}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid LOGVIEWID_Code = new Guid("{7651A701-06E5-11D1-8EBD-00A0C90F26EA}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid LOGVIEWID_Designer = new Guid("{7651A702-06E5-11D1-8EBD-00A0C90F26EA}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid LOGVIEWID_TextView = new Guid("{7651A703-06E5-11D1-8EBD-00A0C90F26EA}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly Guid LOGVIEWID_UserChooseView = new Guid("{7651A704-06E5-11D1-8EBD-00A0C90F26EA}");

	[EditorBrowsable(EditorBrowsableState.Never)]
	public const uint VSITEMID_NIL = uint.MaxValue;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public const uint VSITEMID_ROOT = 4294967294u;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public const uint VSITEMID_SELECTION = 4294967293u;

	public const uint VSCOOKIE_NIL = 0u;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public const uint UndoManager = 0u;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public const uint WindowFrame = 1u;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public const uint DocumentFrame = 2u;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public const uint StartupProject = 3u;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public const uint PropertyBrowserSID = 4u;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public const uint UserContext = 5u;

	public const int VS_E_PROJECTALREADYEXISTS = -2147213344;

	public const int VS_E_PACKAGENOTLOADED = -2147213343;

	public const int VS_E_PROJECTNOTLOADED = -2147213342;

	public const int VS_E_SOLUTIONNOTOPEN = -2147213341;

	public const int VS_E_SOLUTIONALREADYOPEN = -2147213340;

	public const int VS_E_PROJECTMIGRATIONFAILED = -2147213339;

	public const int VS_E_INCOMPATIBLEDOCDATA = -2147213334;

	public const int VS_E_UNSUPPORTEDFORMAT = -2147213333;

	public const int VS_E_WIZARDBACKBUTTONPRESS = -2147213313;

	public const int VS_E_EDITORDISABLED = -2147213296;

	public const int VS_S_PROJECTFORWARDED = 270320;

	public const int VS_S_TBXMARKER = 270321;

	public const int VS_E_INCOMPATIBLEPROJECT = -2147213309;

	public const int VS_E_INCOMPATIBLECLASSICPROJECT = -2147213308;

	public const int VS_E_INCOMPATIBLEPROJECT_UNSUPPORTED_OS = -2147213307;

	public const int VS_E_PROMPTREQUIRED = -2147213306;

	public const int VS_E_CIRCULARTASKDEPENDENCY = -2147213305;

	public const int VS_S_PROJECT_SAFEREPAIRREQUIRED = 270322;

	public const int VS_S_PROJECT_UNSAFEREPAIRREQUIRED = 270323;

	public const int VS_S_PROJECT_ONEWAYUPGRADEREQUIRED = 270324;

	public const int VS_S_INCOMPATIBLEPROJECT = 270325;

	public const uint ALL = 1u;

	public const uint SELECTED = 2u;

	public const int OLE_E_OLEVERB = -2147221504;

	public const int OLE_E_ADVF = -2147221503;

	public const int OLE_E_ENUM_NOMORE = -2147221502;

	public const int OLE_E_ADVISENOTSUPPORTED = -2147221501;

	public const int OLE_E_NOCONNECTION = -2147221500;

	public const int OLE_E_NOTRUNNING = -2147221499;

	public const int OLE_E_NOCACHE = -2147221498;

	public const int OLE_E_BLANK = -2147221497;

	public const int OLE_E_CLASSDIFF = -2147221496;

	public const int OLE_E_CANT_GETMONIKER = -2147221495;

	public const int OLE_E_CANT_BINDTOSOURCE = -2147221494;

	public const int OLE_E_STATIC = -2147221493;

	public const int OLE_E_PROMPTSAVECANCELLED = -2147221492;

	public const int OLE_E_INVALIDRECT = -2147221491;

	public const int OLE_E_WRONGCOMPOBJ = -2147221490;

	public const int OLE_E_INVALIDHWND = -2147221489;

	public const int OLE_E_NOT_INPLACEACTIVE = -2147221488;

	public const int OLE_E_CANTCONVERT = -2147221487;

	public const int OLE_E_NOSTORAGE = -2147221486;

	public const int DISP_E_UNKNOWNINTERFACE = -2147352575;

	public const int DISP_E_MEMBERNOTFOUND = -2147352573;

	public const int DISP_E_PARAMNOTFOUND = -2147352572;

	public const int DISP_E_TYPEMISMATCH = -2147352571;

	public const int DISP_E_UNKNOWNNAME = -2147352570;

	public const int DISP_E_NONAMEDARGS = -2147352569;

	public const int DISP_E_BADVARTYPE = -2147352568;

	public const int DISP_E_EXCEPTION = -2147352567;

	public const int DISP_E_OVERFLOW = -2147352566;

	public const int DISP_E_BADINDEX = -2147352565;

	public const int DISP_E_UNKNOWNLCID = -2147352564;

	public const int DISP_E_ARRAYISLOCKED = -2147352563;

	public const int DISP_E_BADPARAMCOUNT = -2147352562;

	public const int DISP_E_PARAMNOTOPTIONAL = -2147352561;

	public const int DISP_E_BADCALLEE = -2147352560;

	public const int DISP_E_NOTACOLLECTION = -2147352559;

	public const int DISP_E_DIVBYZERO = -2147352558;

	public const int DISP_E_BUFFERTOOSMALL = -2147352557;

	public const int RPC_E_CALL_REJECTED = -2147418111;

	public const int RPC_E_CALL_CANCELED = -2147418110;

	public const int RPC_E_CANTPOST_INSENDCALL = -2147418109;

	public const int RPC_E_CANTCALLOUT_INASYNCCALL = -2147418108;

	public const int RPC_E_CANTCALLOUT_INEXTERNALCALL = -2147418107;

	public const int RPC_E_CONNECTION_TERMINATED = -2147418106;

	public const int RPC_E_SERVER_DIED = -2147418105;

	public const int RPC_E_CLIENT_DIED = -2147418104;

	public const int RPC_E_INVALID_DATAPACKET = -2147418103;

	public const int RPC_E_CANTTRANSMIT_CALL = -2147418102;

	public const int RPC_E_CLIENT_CANTMARSHAL_DATA = -2147418101;

	public const int RPC_E_CLIENT_CANTUNMARSHAL_DATA = -2147418100;

	public const int RPC_E_SERVER_CANTMARSHAL_DATA = -2147418099;

	public const int RPC_E_SERVER_CANTUNMARSHAL_DATA = -2147418098;

	public const int RPC_E_INVALID_DATA = -2147418097;

	public const int RPC_E_INVALID_PARAMETER = -2147418096;

	public const int RPC_E_CANTCALLOUT_AGAIN = -2147418095;

	public const int RPC_E_SERVER_DIED_DNE = -2147418094;

	public const int RPC_E_SYS_CALL_FAILED = -2147417856;

	public const int RPC_E_OUT_OF_RESOURCES = -2147417855;

	public const int RPC_E_ATTEMPTED_MULTITHREAD = -2147417854;

	public const int RPC_E_NOT_REGISTERED = -2147417853;

	public const int RPC_E_FAULT = -2147417852;

	public const int RPC_E_SERVERFAULT = -2147417851;

	public const int RPC_E_CHANGED_MODE = -2147417850;

	public const int RPC_E_INVALIDMETHOD = -2147417849;

	public const int RPC_E_DISCONNECTED = -2147417848;

	public const int RPC_E_RETRY = -2147417847;

	public const int RPC_E_SERVERCALL_RETRYLATER = -2147417846;

	public const int RPC_E_SERVERCALL_REJECTED = -2147417845;

	public const int RPC_E_INVALID_CALLDATA = -2147417844;

	public const int RPC_E_CANTCALLOUT_ININPUTSYNCCALL = -2147417843;

	public const int RPC_E_WRONG_THREAD = -2147417842;

	public const int RPC_E_THREAD_NOT_INIT = -2147417841;

	public const int RPC_E_VERSION_MISMATCH = -2147417840;

	public const int RPC_E_INVALID_HEADER = -2147417839;

	public const int RPC_E_INVALID_EXTENSION = -2147417838;

	public const int RPC_E_INVALID_IPID = -2147417837;

	public const int RPC_E_INVALID_OBJECT = -2147417836;

	public const int RPC_S_CALLPENDING = -2147417835;

	public const int RPC_S_WAITONTIMER = -2147417834;

	public const int RPC_E_CALL_COMPLETE = -2147417833;

	public const int RPC_E_UNSECURE_CALL = -2147417832;

	public const int RPC_E_TOO_LATE = -2147417831;

	public const int RPC_E_NO_GOOD_SECURITY_PACKAGES = -2147417830;

	public const int RPC_E_ACCESS_DENIED = -2147417829;

	public const int RPC_E_REMOTE_DISABLED = -2147417828;

	public const int RPC_E_INVALID_OBJREF = -2147417827;

	public const int RPC_E_NO_CONTEXT = -2147417826;

	public const int RPC_E_TIMEOUT = -2147417825;

	public const int RPC_E_NO_SYNC = -2147417824;

	public const int RPC_E_FULLSIC_REQUIRED = -2147417823;

	public const int RPC_E_INVALID_STD_NAME = -2147417822;

	public const int CO_E_FAILEDTOIMPERSONATE = -2147417821;

	public const int CO_E_FAILEDTOGETSECCTX = -2147417820;

	public const int CO_E_FAILEDTOOPENTHREADTOKEN = -2147417819;

	public const int CO_E_FAILEDTOGETTOKENINFO = -2147417818;

	public const int CO_E_TRUSTEEDOESNTMATCHCLIENT = -2147417817;

	public const int CO_E_FAILEDTOQUERYCLIENTBLANKET = -2147417816;

	public const int CO_E_FAILEDTOSETDACL = -2147417815;

	public const int CO_E_ACCESSCHECKFAILED = -2147417814;

	public const int CO_E_NETACCESSAPIFAILED = -2147417813;

	public const int CO_E_WRONGTRUSTEENAMESYNTAX = -2147417812;

	public const int CO_E_INVALIDSID = -2147417811;

	public const int CO_E_CONVERSIONFAILED = -2147417810;

	public const int CO_E_NOMATCHINGSIDFOUND = -2147417809;

	public const int CO_E_LOOKUPACCSIDFAILED = -2147417808;

	public const int CO_E_NOMATCHINGNAMEFOUND = -2147417807;

	public const int CO_E_LOOKUPACCNAMEFAILED = -2147417806;

	public const int CO_E_SETSERLHNDLFAILED = -2147417805;

	public const int CO_E_FAILEDTOGETWINDIR = -2147417804;

	public const int CO_E_PATHTOOLONG = -2147417803;

	public const int CO_E_FAILEDTOGENUUID = -2147417802;

	public const int CO_E_FAILEDTOCREATEFILE = -2147417801;

	public const int CO_E_FAILEDTOCLOSEHANDLE = -2147417800;

	public const int CO_E_EXCEEDSYSACLLIMIT = -2147417799;

	public const int CO_E_ACESINWRONGORDER = -2147417798;

	public const int CO_E_INCOMPATIBLESTREAMVERSION = -2147417797;

	public const int CO_E_FAILEDTOOPENPROCESSTOKEN = -2147417796;

	public const int CO_E_DECODEFAILED = -2147417795;

	public const int CO_E_ACNOTINITIALIZED = -2147417793;

	public const int CO_E_CANCEL_DISABLED = -2147417792;

	public const int COR_E_FILENOTFOUND = -2147024894;

	public const int RPC_E_UNEXPECTED = -2147352577;

	public const int VS_E_BUSY = -2147220992;

	public const int VS_E_SPECIFYING_OUTPUT_UNSUPPORTED = -2147220991;

	public const int S_FALSE = 1;

	public const int S_OK = 0;

	public const int UNDO_E_CLIENTABORT = -2147205119;

	public const int E_OUTOFMEMORY = -2147024882;

	public const int E_INVALIDARG = -2147024809;

	public const int E_FAIL = -2147467259;

	public const int E_NOINTERFACE = -2147467262;

	public const int E_NOTIMPL = -2147467263;

	public const int E_UNEXPECTED = -2147418113;

	public const int E_POINTER = -2147467261;

	public const int E_HANDLE = -2147024890;

	public const int E_ABORT = -2147467260;

	public const int E_ACCESSDENIED = -2147024891;

	public const int E_PENDING = -2147483638;

	internal const int WM_USER = 1024;

	public const int VSM_TOOLBARMETRICSCHANGE = 4178;

	public const int VSM_ENTERMODAL = 4179;

	public const int VSM_EXITMODAL = 4180;

	public const int VSM_VIRTUALMEMORYLOW = 4181;

	public const int VSM_VIRTUALMEMORYCRITICAL = 4182;

	public const int VSM_MEMORYHIGH = 4184;

	public const int VSM_MEMORYEXCESSIVE = 4185;

	public const int CPDN_SELCHANGED = 2304;

	public const int CPDN_SELDBLCLICK = 2305;

	public const int CPPM_INITIALIZELIST = 2309;

	public const int CPPM_QUERYCANSELECT = 2310;

	public const int CPPM_GETSELECTION = 2311;

	public const int CPPM_INITIALIZETAB = 2312;

	public const int CPPM_SETMULTISELECT = 2313;

	public const int CPPM_CLEARSELECTION = 2314;

	public static readonly IntPtr DOCDATAEXISTING_UNKNOWN = new IntPtr(-1);

	private VSConstants()
	{
	}
}
