# 002 - Improving the Solution Structure

Now that we have a working initial project, we want to make some changes for long-term support and to better start dividing our application into a more managable state.

## Move "Data" to its own solution

Just by getting EF out of the mix, we can make some big improvements!

* Right click on the solution and select "Add Project"
* Select "Class LIbrary"
* Name it "ProjectHub.Data", you can use the default location
* Select .NET 10 as the framework
* Delete the auto-added class
* Add a reference to `ProjectHub.Data` from `ProjectHub.Web`
* Open "Tools" -> "NuGet Package Manager" -> "Manage Packages for Solution"
* Install `Microsoft.EntityFrameworkCore.SqlServer` to the new data project
* Install `Microsoft.AspNetCore.Identity.EntityFrameworkCore` to the new data project
* Move `Data/ApplicationDbContext.cs` to the root of the `Data` Project
* Move `Data/ApplicationUser.cs` to a new folder `Models` within the `Data` project
* Delete the `Migrations` folder, we will recreate it
* Delete the `Data` folder
* Modify the namespace for `AppliationUser` to be `ProjectHub.Data.Models`
* Modify the namespace for `ApplicationDbContext.cs` to be `ProjectHub.Data` and add `ProjectHub.Data.Models` as a using to the file
* Do a global rename of `@using ProjectHub.Web.Data;` to `@using ProjectHub.Data.Models;` to get the project to a close state.
* Open `ProjectHub.Web.Program.cs` and replace the existing `using ProjectHub.Web.Data` with 2 statements `using ProjectHub.Data;` and `using ProjectHub.Data.Models`

Once done with the above, ensure that the project build successfully.

>[!Tip]
>See the `Enhancement Steps` for a way to ensure that the moved files appear as moves, rather than add/delete.  These were used in the creation of the sample within this repository

## Ensure You Have the latest EF Core Tools

We will use the command-line tools for EF Core, just to get used to how we can use those within other environments.

Ensure that you have the latest version installed by running the following command.

````
dotnet tool install --global dotnet-ef
````

## Recreate Initial Migration & Setup For Future Notes

To re-create the migrations, from the command line when locatated in the `ProjectHub.Data` folder execute the following command

````
dotnet ef migrations --startup-project '../ProjectHub.Web/ProjectHub.Web.csproj' add Initial
````

## Check DB Connection

We are ok without the DB working at the moment, but run the following command to attempt to apply migrations to see if your machine is setup for local DB.

````
dotnet ef database update --startup-project '../ProjectHub.Web/ProjectHub.Web.csproj'
````

If you have local DB you will get success, otherwise it will be an error

## Results & Next-Steps

The goal of this process was to start improving our structure.  We will continue to enhance upon this in the coming steps.



