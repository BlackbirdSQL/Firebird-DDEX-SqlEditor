# BlackbirdSQL DDEX 2.0 with SqlEditor Provider for Firebird

### Change log

#### v0.1.0-alpha
Initial release.

#### v0.1.02-alpha
Implemented VSIX installer.

#### v0.1.03-alpha
Streamlined async to sync transitions.

#### v9.1.0.5-alpha
Extended user reporting for IDE StatusBar, TaskHandler and Output window for background and UI tasks and Exceptions.

#### v9.1.0.6-beta
Initial beta release.

#### v9.1.0.74-beta
Fixed potential deadlock on async -> sync (async cancel) -> async restart -> sync (async cancel) scenario.

#### v9.1.0.84-beta
Fixed anomaly where an attached longrunning async task had not yet been launched by a task factory launching task. This caused a deadlock when returning sync tasks attempted a wait on the launcher.

#### v9.1.0.87-prelease
Final pre-release using a release build.

#### v9.1.0.88-prelease
Implemented single click check boxes and radio button into Visual Studio user settings Property grid.
Streamlined and Centralized DocumentMoniker and ConnectionMoniker management.

#### v9.1.0.90-prelease
Centralized connection management and implemented outstanding connection features.

#### v9.1.0.91-prelease
Upgraded the FirebirdClient to 10.0.0.0.

#### v9.9.1.01-prelease
Fixed issue with opening and then cancelling an IVsDataConnectionDialog in a custom context.

#### v9.9.1.02-prelease
Fixed issue where DSRefBuilder used an empty Site.

#### v9.9.1.03-prelease
Implemented SqlEditor text output, enhanced SE context menus and resolved outstanding issues.

#### v10.0.0.101
Version sync.

#### v10.0.1.0-prerelease
Pre-release version for publishing on Visual Sturio Markeyplace.

#### v10.0.1.1-prerelease
Resolved Blob handling exceptions.

#### v10.0.1.2-prerelease
Resolved Blob handling exceptions and name update.

#### v10.0.1.3
Resolved Blob handling exceptions and Readme update.

#### v10.0.1.4
Resolved Generic Blob handling exceptions.
