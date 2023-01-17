# ADO.NET

### Setup procedure

* Install the DDEX extension `BlackbirdSql.VisualStudio.Ddex.vsix`. </br>*We're still under development so install the debug version located in the output folder `BlackbirdSql.VisualStudio.Ddex\bin\Debug`.*
* On opening a solution for the first time after installation ensure none of your app.config files or edmx models are open in the IDE. If any are the DDEX tool will skip over them during validation, so close them and then reopen your solution.
* Add the applicable Nuget package if you haven't already.
* Rebuild your Project. It should now be able to access any existing client connections, xsd datasets and edmx models you have.
* Create a dataset throught the xsd wizard or an entity data model through the edmx wizard.
<p style="font-size:1.1em;margin-bottom:-8px">For an xsd dataset:</p>

* Install the Nuget FirebirdSql.Data.FirebirdClient package if you haven't already and `rebuild` your project.
* You can add your Firebird database to the server explorer (SE) (*or add a Settings connection string*) using the `Firebird SQL Server` data source and selecting the `BlackbirdSql DDEX 2.0` provider.</br>Using the SE is the easiest. From there you can just drag and drop your Firebird database entities onto the xsd.

<p style="font-size:1.1em;margin-bottom:-8px">For an ADO.NET edmx:</p>

* Install the Nuget EntityFramework.Firebird package if you haven't already and `rebuild` your project. (FirebirdSql.Data.FirebirdClient will be auto installed as a dependency if it's not already installed.)
* Open an existing edmx model. It should work right off the bat, or add a new edmx model through the wizard using the `EF designer from database` option as you normally would, again connecting through the `BlackbirdSql DDEX 2.0` provider.
* With your new edmx open select `Project > Add new data source > Object` to complete the setup of your model.

__Warning__ Operations within the edmx UI can take some time. Even a Cancel request can lock up the IDE. Be patient.</br>
__Warning__ BlackbirdSql.VisualStudio.Ddex.dll is used by the IDE. Do not add a reference to it in your projects.</br>
__Note__ Rebuild your projects after installing the VSIX. This will ensure any legacy edmx models that were updated and any modifications to the app.config are activated.
