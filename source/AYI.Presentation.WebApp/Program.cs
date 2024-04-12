using AYI.Core.Contracts;
using AYI.Core.Services;
using AYI.Presentation.WebApp.Components;
using DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IScheduledEventService, ScheduledEvents.ScheduledEventService>();
builder.Services.AddScoped<ILocationService, Locations.LocationService>();
builder.Services.AddScoped<IInvitationService, Invitations.InvitationService>();
builder.Services.AddScoped<IPeopleService, People.PeopleService>();
builder.Services.AddSqliteConnectionFactory();

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();
