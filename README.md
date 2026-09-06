# 2026-real-world-blazor-workshop

This is a demonstration repository designed to work with Mitchel's 2026 "Real World Blazor" workshop that was provided at Kansas City Developer Conference (KCDC) in September 2026.

## Using this Repository

This repository was built with a "Follow-Along" mindset, allowing users to progressively build along, just like what was done as part of the initial setup within the in-person training.  

### Commits

Individual commits exist to provide an easy-to-view 'delta' between the various steps in the progression of the project.  Commits are the most granular progression, and the commit messages that are provided help to understand what was done, where the project is, and otherwise.

### Tags (Eg: 001 - Start Here)

Rather than using a versioned structure, the tags within the repository utilize the `XXX - DESCRIPTION` format.  This allows you to follow along through the steps & progressions.

For example, if you wanted to start out as we did in person, you would pull the repository and then check out the tag `000 - Start Here,` as that will give you the starting point.

At each tag, you will expect to see a few things.

* `/steps` will contain a file that outlines the steps necessary to progress to the next step.  For example, when starting out, there will be a `001 Instructions.md` file that contains all of the steps to make the progress from 000 -> 001.  This way you can either follow along, or you can do other things
* `/presentations` will contain any "time-specific" presentations that may exist for a particular step or stage of the demonstration
* `/alternative-steps` MAY contain additional alternative steps, or tips/tricks that you may want to try on your own.  For example, steps necessary to utilize GitHub Copilot to complete tasks, rather than manually doing them


## Target Solution Structure

At the end of the day, we are working towards creating a larger, robust application using Blazor (Interactive Server), Entity Framework, Redis, Aspire, and more!

## Required Items to Start

* Visual Studio 2026 - Current Version of either the normal edition or the Insiders edition
  * ASP.NET & Web Development workload
* Aspire Templates
  * `dotnet new install Aspire.ProjectTemplates` Should be version 13.5.4
* Docker Desktop
* SQL Server 2025 Image for Docker

>[!Note]
>If you do not have Docker or the needed image, simply skip the Aspire SQL configuration steps, and you will be able to continue the examples.
