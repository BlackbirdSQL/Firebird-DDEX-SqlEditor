# BlackbirdSQL DDEX 2.0 .NET Data Provider for Firebird

(*This package is in pre-release. Note that in preparation for release the stock Firebird libraries have been copied to their respective Release output folders and the Release configuration has all Firebird library builds disabled.*)

The BlackbirdSQL DDEX 2.0 .NET Data Provider tool, `BlackbirdSql.VisualStudio.Ddex`, implements all the core DDEX 2.0 interfaces prevalent in the SqlServer DDEX provider, but currently excludes DDL functionality.

*If you're noticing a degradation in performance in the IDE after installing an extension then it's advantages are lost, so the first tenet of this package is `small footprint, low overhead`.*

The intention is to maintain a small footprint. We're not going to start altering VS menus and taking over your Visual Studio IDE workspace. It is a data source UI provider for Firebird and the benchmark is the SqlServer provider, so whatever UI functionality is available for SqlServer is on the todo list for Firebird provided it does not directly interfere with the developer's active UI.

The goal is that you don't have to do any configuring of the .csproj, app.config, machine.config or any legacy edmx models, and eliminate using the GAC.</br>
The once-off validation features can be disabled in the Visual Studio options but each individual task in the validation process is spawned asynchronously so the overhead is miniscual.</br>
See [Extended Description](#extended-description) below.

### FlameRobin
The connection dialog now lists any `FlameRobin` Server hosts (DataSources) and Server host Databases. As everyone familiar with Firebird will know, this makes a huge difference when entering remote Firebird server database paths.

### A Note on AutoIncrement
The logic for auto-increment detection of a trigger (which avoids interrogating the dml) appears to work correctly. There is no documentation to show otherwise, but there is info on the web implying it cannot be done. If you have a case where this logic fails, please pop us a mail.</br>
(We assert a trigger with TRIGGER_TYPE == 1, SEQUENCENO == 1 and FLAGS == 1 with a single dependency column and with a unique single index segment on a 'PRIMARY KEY' index constitutes an auto-increment trigger. You can create an auto-increment without meeting these conditions but then it won't fall into the scope of a SqlServer PrimaryKey with AutoIncrement, and that's what we're after.)


## Known issues
* If on startup of the Visual Studio IDE, and only on startup, an attempt is made to access a data object before the BlackbirdSql DDEX provider has been given the IDE shell context, Visual Studio will flag the provider as unavailable for the duration of the session. As it stands Visual Studio will have to be restarted to clear the flag.</br>
This is true for both the SE and any other data objects in a solution. In particular this issue can consistently be replicated in Server Explorer if a connection node for Firebird is open and the SE is pinned on startup of the IDE within a solution context.</br> 
Unless we're missing a trick here this seems to be unavoidable. Loading is asynchronous, so the provider needs time to register and load.</br>
We want to be as unobtrusive as possible so load delays are just a reality if we go the asynchronous route. (*Loading is initiated at the earliest possible, which is as soon as the IDE shell context is available.*)
* If you have a huge number of triggers then rendering of the triggers in the SE may take some time because an IS_AUTOINCREMENT flag is derived for each
trigger.
* If you use a Server Explorer Database Connection as the connection for an EDMX model and choose to exclude sensitive data there may be instances where the password will also be stripped from the SE connection and you will be prompted for a password through the BlackbirdSql IVsDataConnectionPromptDialog implementation the next time you access it in the SE.</br>
I have been unable to replicate this bug for some time now so it may have been resolved since updating the IVsDataConnectionPromptDialog logic.
* There seems to be an issue with drag and drop on procedures and functions which I haven't looked at. It's likely something trivial but not sure if this functionality is available to SqlServer so may be another rabbit hole.</br>
The same applies to drag and drop from the SE directly into the edmx, not available on SqlServer but I don't see why it cannot be done.
* The enhanced localized new query command is working but has not yet been placed in the views or functions/procedures nodes of the SE, which still use the built-in new query command.</br>
The BlackbirdSql new query command simply filters the table selection list (based on whether you initiated it from a System Table node context or User Table node context) and then passes it on to the native Visual Studio command.</br>
Refreshing the table selection list will include both System and User tables.


## Documentation

* [ADO.NET provider](ado-net.md)
* [Using ADO.NET edmx with .NET](edmx-NET.md)


## Packages

| NuGet | Version | Downloads |
|-------|---------|-----------|
| [FirebirdSql.Data.FireClient](https://www.nuget.org/packages/FirebirdSql.Data.FireClient) | ![FirebirdSql.Data.FireClient](https://img.shields.io/nuget/v/FirebirdSql.Data.FireClient.svg) | ![FirebirdSql.Data.FireClient](https://img.shields.io/nuget/dt/FirebirdSql.Data.FireClient.svg) |
| [EntityFramework.Firebird](https://www.nuget.org/packages/EntityFramework.Firebird) | ![EntityFramework.Firebird](https://img.shields.io/nuget/v/EntityFramework.Firebird.svg) | ![EntityFramework.Firebird](https://img.shields.io/nuget/dt/EntityFramework.Firebird.svg) |
| [FirebirdSql.EntityFrameworkCore.Firebird](https://www.nuget.org/packages/FirebirdSql.EntityFrameworkCore.Firebird) | ![FirebirdSql.EntityFrameworkCore.Firebird](https://img.shields.io/nuget/v/FirebirdSql.EntityFrameworkCore.Firebird.svg) | ![FirebirdSql.EntityFrameworkCore.Firebird](https://img.shields.io/nuget/dt/FirebirdSql.EntityFrameworkCore.Firebird.svg) |


## Resources

* [Downloads](https://github.com/BlackbirdSQL/NETProvider-DDEX/releases)
* [Issue tracker](https://github.com/BlackbirdSQL/NETProvider-DDEX/issues)


## Builds

If you're planning on testing this solution (Blackbird.sln) preferably DO NOT use test projects within Blackbird.sln.</br>
Rather fire up the experimental instance of Visual Studio with Blackbird.sln remaining open and test within the experimental instance. If you have successfully built Blackbird.sln you won't need to install the vsix. VS will automatically detect it in the experimental instance.</br>
To fire up an experimental instance of Visual Studio create a shortcut of Visual Studio with the experimental suffix. eg. `"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe" /RootSuffix Exp`

## Misc

BlackbirdSql.Data.DslClient.dll, EntityFramework.BlackbirdSql.dll and BlackbirdSql.EntityFrameworkCore.dll (Blackbird[Experimental]) were for initial debugging purposes only in order to have full control over the client and modify it where necessary.


## Extended Description

Although the original DDEX 1.0 DataTools package, *BlackbirdSql.VisualStudio.DataTools* was revived, the DDEX 2.0 package, BlackbirdSql.VisualStudio.Ddex is click and go using VSIX and autoload, and requires no additional setup either in the app.config, csproj or machine.config.</br>
The  prupose of *BlackbirdSql.VisualStudio.DataTools* was only to get DDEX functioning for Firebird before implementing DDEX 2.0, so this tool can be considered obsolete.

If the option is enabled there will be a once-off validation of a solution's projects' app.config's and edmx models. Legacy edmx models won't work with Firebird's latest EntityFramework version so an update is required.</br>
This is a once off validation on each `existing` solution the first time it is opened after installing the VSIX. If the app.config is open or any edmx models are open you will need to close them first and then reopen your solution for the once-off validation to complete.

If the option is enabled and you add Firebird.Data.FirebirdClient or EntityFramework.Firebird to a project it will be validated and the app.config updated correctly if required. If the app.config is open the update will be skipped and you will need to reopen your solution for the validation to complete.

__Note:__ For the debug build the once-off validation flags are not persistent between loads of solutions and are repeated.

As it stands right now the code is littered with diagnostics calls with writes to a log file set to c:\bin\vsdiag.log.</br>
These can all be disabled or the log file output path changed in Visual Studio's options.

If you're planning on using EF Core and/or .NET, VS does not have wizard support for edmx which makes no sense to me.
This roadblock is easily overcome by creating a separate project using .NET Framework for your data models and then linking your .NET / EF Core projects to those edmx models.

If there's any magic you feel should be included here, you're welcome to pop me a mail.</br>
On the priority list are DDL commands... Create, Alter etc.
