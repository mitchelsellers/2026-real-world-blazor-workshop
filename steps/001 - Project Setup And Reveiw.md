# 001 - Project Setup & Review

The goal of this set of activities is to get the project created, and start with the default strucuture.

Our goal is to include a /src folder and include the project within the folder, with a solution and initial aspire setup.

## Create The Default Template

* Launch Visual Studio Create new Solution
* Select "Blazor Web App" as the template and "Next"
  * Project Name: ProjectHub.Web
  * Location: The root folder you want to create it in
  * Solution Name: src (for a silly VS Bug)
  * Press "Next"
* Provide Additional Information
  * Framework: .NET 10
  * Authentication Type: "Individual Accounts"
  * Configure for Https: Checked
  * Interactive Render Mode: Server
  * Interactivity Location: Global
  * Include Sample Pages: Checked
  * Use the ".dev.* TLD: Checked
  * Enlist in Aspire Orchestration: Checked
    * Ensure version is "13.5" if not, go back out and update
* Click "Create"
* VS is silly so do a bit of renaming
  * Solution "src" to "ProjectHub"
  * Project "src.AppHost" to "ProjectHub.AppHost"
  * Project "src.ServiceDefaults" to "ProjectHub.ServiceDefaults"
* Close Visual Studio
* Navigate to your folder and also rename the folders similar to the above


### Correct `.slnx` and Projects

Patch the paths, it should look like this

```` xml
<Solution>
  <Project Path="ProjectHub.Web/ProjectHub.Web.csproj" />
  <Project Path="ProjectHub.AppHost/ProjectHub.AppHost.csproj" />
  <Project Path="ProjectHub.ServiceDefaults/ProjectHub.ServiceDefaults.csproj" />
</Solution>
````

Edit the `ProjectHub.Web` project and update the path for the `ServiceDefaults` reference.



### Reopen & Validate Functionality

Re-open the solution and hit "F5" to start debugging to ensure that you are able to build & run the solution, we will do more with this later!


## Next Steps

We are going to review & discuss from here! 

This completes Step 001!