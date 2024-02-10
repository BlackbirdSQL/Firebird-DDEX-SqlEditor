# BlackbirdSQL DDEX 2.0 with SqlEditor Provider for Firebird

### Change log

#### v11.0.0.0
First official full release of BlackbirdSql.</br>
Improved interactive synchronization between Connection Dialog and RunningConnectionTable.
Resolved RunningConnectionTable deadlock red zone issues.</br>
Removed redundant CsbAgent DataSource Case name mangling validations.</br>
Resolved several SE, edmx and RunningConnectionTable sync issues.</br>
Removed ConnectionSource debug messages.</br>
Prevented DatasetId glyph removal in Session connection dialogs when connection has not changed.

#### v10.1.2.3
Resolved several SE, edmx and RunningConnectionTable sync issues.

#### v10.1.2.2
Synchronized edmx with RunningConnectionTable connection management.</br>
Implemented recovery procedures for SE glitch that leaves a sited explorer connection corrupted and dangling after loading another solution.

#### v10.1.2.1
Included handling of IDE `no solution` state and early loading of the connection dialog.</br>
Prevented LinkageParser replication when a weak equivalency has been refreshed.</br>
Handle asynchronous VS load glitch referencing incorrect invariant assembly when an edmx is open on load.</br>
Validation of Solution's configuration settings can now only be launched from an open SE node's context menu.

#### v10.1.2.0
Final preview version. Implemented SqlEditor and Intellisense support for `.fbsql` disk files.

#### v10.1.1.1
Final preview version. Synchronized Intellisense and RdtManager.

#### v10.1.1.0
Final preview version. Implemented wait timeouts on all possible deadlock logic.

#### v10.1.0.2
Prevented transient LinkageParser deletion and relink during SE connection modification.

#### v10.1.0.1
Implemented asynchronous Server Explorer enumerations.</br>
Resolved early load deadlock issue.

#### v10.1.0.0
Full integration of Server Explorer and RunningConnectionTable connection management.</br>
Improved asynchronous loading of trigger linkage tables.</br>
Defaulted ApplicationName as a weak equivalency connection property for loading multiple equivalent connections.</br>
Resolved several Server Explorer synchronization issues.

#### v10.0.3.0
Moved validation Globals flag variable from Solution and Project Globals to user files.</br>
Resolved issues with User Options dialogs.</br>
Resolved issue where SqlEditor text output of DateTime columns threw an exception.

#### v10.0.2.1
Resolved bug where the readonly attribute of dependent properties were not being updated by their automator.

#### v10.0.2.0
Implemented configurable equivalency keys.</br>
Resolved bug in user options when collapsing then expanding a category.

#### v10.0.1.5
Implemented connection naming and resolved centralized connection issues.

#### v10.0.1.4
Resolved Generic Blob handling exceptions.

#### v10.0.1.3
Resolved Blob handling exceptions and Readme update.

#### v10.0.1.2-prerelease
Resolved Blob handling exceptions and name update.

#### v10.0.1.1-prerelease
Resolved Blob handling exceptions.

#### v10.0.1.0-prerelease
Pre-release version for publishing on Visual Sturio Markeyplace.

#### v10.0.0.101
Version sync.

#### v9.9.1.03-prelease
Implemented SqlEditor text output, enhanced SE context menus and resolved outstanding issues.

#### v9.9.1.02-prelease
Fixed issue where DSRefBuilder used an empty Site.

#### v9.9.1.01-prelease
Fixed issue with opening and then cancelling an IVsDataConnectionDialog in a custom context.

#### v9.1.0.91-prelease
Upgraded the FirebirdClient to 10.0.0.0.

#### v9.1.0.90-prelease
Centralized connection management and implemented outstanding connection features.

#### v9.1.0.88-prelease
Implemented single click check boxes and radio button into Visual Studio user settings Property grid.</br>
Streamlined and Centralized DocumentMoniker and ConnectionMoniker management.

#### v9.1.0.87-prelease
Final pre-release using a release build.

#### v9.1.0.84-beta
Fixed anomaly where an attached longrunning async task had not yet been launched by a task factory launching task. This caused a deadlock when returning sync tasks attempted a wait on the launcher.

#### v9.1.0.74-beta
Fixed potential deadlock on async -> sync (async cancel) -> async restart -> sync (async cancel) scenario.

#### v9.1.0.6-beta
Initial beta release.

#### v9.1.0.5-alpha
Extended user reporting for IDE StatusBar, TaskHandler and Output window for background and UI tasks and Exceptions.

#### v0.1.03-alpha
Streamlined async to sync transitions.

#### v0.1.02-alpha
Implemented VSIX installer.

#### v0.1.0-alpha
Initial release.