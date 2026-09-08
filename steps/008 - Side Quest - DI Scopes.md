# Side-Quest - DI Scopes

Just to help drill in the notes here, it is important to quickly understand how things work for DI.

>[!Note]
>The example contained here was adapted from the Microsoft Documentation here - https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/dependency-injection?view=aspnetcore-10.0#owningcomponentbase

## Create a simple TimeTravel Service

Within `ProjectHub.Application` create a new folder `TimeTravel` and add the following two files

`ITimeTravel.cs`
```` csharp
namespace ProjectHub.Application.TimeTravel;

public interface ITimeTravel
{
    public DateTime DT { get; set; }
}
````

`TimeTravel.cs`
```` csharp
namespace ProjectHub.Application.TimeTravel;

[RegisterScoped]
internal class TimeTravel : ITimeTravel
{
    public DateTime DT { get; set; } = DateTime.Now;
}
````

## Create a TimeTravel Demo Page

Within `ProjectHub.Web` create a new folder `TimeTravel` and add a new Razor Component `Index.razor` with the following content

```` razor
@page "/time-travel"

@using ProjectHub.Application.TimeTravel

@inject ITimeTravel TimeTravel1

@inherits OwningComponentBase //Allow for the creatin of Custom Scoped Services in the Component

<h1><code>OwningComponentBase</code> Example &amp; Circuit Lifetime Demos</h1>

<ul>
    <li>TimeTravel1.DT: @TimeTravel1?.DT</li>
    <li>TimeTravel2.DT: @TimeTravel2?.DT</li>
</ul>

@code {
    private ITimeTravel TimeTravel2 { get; set; } = default!;

    protected override void OnInitialized()
    {
        TimeTravel2 = ScopedServices.GetRequiredService<ITimeTravel>();
    }
}
````

## Lastly Add it to the Menu

```` razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="/time-travel">
        <span class="bi bi-house-door-fill-nav-menu" aria-hidden="true"></span> Time Travel
    </NavLink>
</div>
````

## Discussion Points & Testing

What does this do for us?  Try it out to see.

* Navigate to the `Time Travel` menu item, note the times
* Use the menu to navigate to `Projects` then back to `Time Travel` and notice the times.
* Hit F5 and notice the times

Each time a circuit is recreated, you get new values for the first one (Injected) and each initialization you get others.

Why do we care?