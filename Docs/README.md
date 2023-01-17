# BlackbirdSQL DDEX 2.0 .NET Data Provider

This package is still under development.

The original DDEX 1.0 data tool has been re-implemented as BlackbirdSql.VisualStudio.DataTools. The purpose was only to get DDEX functioning for Firebird before implementing DDEX 2.0, so this tool can be considered obsolete.</br>
BlackbirdSql.Data.DslClient.dll, EntityFramework.BlackbirdSql.dll and BlackbirdSql.EntityFrameworkCore.dll were for debugging purposes only.</br>
A new data tool implementing DDEX 2.0, `BlackbirdSql.VisualStudio.Ddex`, has been added.</br>
The goal is that you don't have to do any configuring of .csproj, app.config, machine.config and any legacy edmx models. The validation features can be disabled in the Visual Studio but the tool has a small footprint and low overhead.
See [Extended Description](#extended-description) below.


## Documentation

* [ADO.NET provider](ado-net.md)

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

TBC

## Misc

### Notable supporters

### 3rd party code

## Extended Description

Although the original DDEX 1.0 DataTools package was revived, the DDEX 2.0 package, BlackbirdSql.VisualStudio.Ddex is click and go using VSIX and autoload, and requires no additional setup either in the app.config, csproj or machine.config.</br>

Loading is asynchronous so the provider needs time to register and load. A database node in server explorer can simply be refreshed if an attempt was made to access it before it was fully loaded.</br>
We want to be as unobtrusive as possible so load delays are just a reality if we go the asynchronous route. (*Loading is initiated at the earliest possible, which is as soon as the IDE shell context is available.*)

If the option is enabled a solution's projects will be checked for correct configuration of the app.config and edmx models. Legacy edmx models won't work with Firebird's latest EntityFramework version.</br>
This is a once off validation on each `existing` solution the first time it is opened after installing the VSIX. If the app.config is open or any edmx models are open you will need to close them first and then reopen your solution for the once-off validation to complete.

If you add Firebird.Data.FirebirdClient or EntityFramework.Firebird to a project it will be validated and the app.config updated correctly if required.

As it stands right now the code is littered with diagnostics calls with writes to a log file set to c:\bin\vsdiag.log.</br>
These can all be disabled in Visual Studio's options.

There seems to be an issue with drag and drop on procedures and functions which I haven't looked at. It's likely something trivial but this functionality isn't available to SqlServer so may be another rabbit hole. The same applies to drag and drop from the SE directly into the edmx, also not available on SqlServer but I don't see why it cannot be done.</br>
Also, Index and Function columns are not appearing in the SE.

The enhanced localized new query command is working but has not yet been placed in the views or functions/procedures nodes of the SE.</br>
The BlackbirdSql new query command simply filters the table selection list (based on whether you initiated it from a System Table node context or User Table node context) and then passes it on to the native Visual Studio command.</br>
Refreshing the table selection list will include both System and User tables.

If you're planning on using EF Core and/or .NET, VS does not have wizard support for either of these in the edmx or xsd which makes no sense to me.
This roadblock is easily overcome by creating a separate project using .NET Framework for your data models and then linking your .NET / EF Core projects to those datasets.

I'm not a purist. If you're like me then you see coding as an art form and the focus is on being creative. Taking the code first route is to me pretty pointless, so we're going to build on this project and develop this package into something really neat, eliminating unecessary coding where we can.

The intention is to maintain a small footprint. We're not going to start altering VS menus and taking over the Visual Studio IDE. It is a data source UI provider for Firebird and the benchmark is the SqlServer provider, so whatever UI functionality is available for SqlServer is on the ToDo list for Firebird.

If there's any magic you feel should be included here, pop me a mail.</br>
Scanning for preset FlameRobin databases and including them in a dropdown in the VS connection dialog is on the priority ToDo list. That's a simple enumeration of the FlameRobin db xml file, if installed.</br>
Also on the priority list are DML commands... Create, Alter etc.
