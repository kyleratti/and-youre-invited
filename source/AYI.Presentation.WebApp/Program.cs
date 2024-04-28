using AYI.Core.Contracts;
using AYI.Core.Contracts.Options;
using AYI.Core.Services;
using AYI.Presentation.WebApp.Components;
using AYI.Presentation.WebApp.Options;
using DataAccess;
using Invitations = AYI.Core.Services.Invitations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IScheduledEventService, ScheduledEvents.ScheduledEventService>();
builder.Services.AddScoped<ILocationService, Locations.LocationService>();
builder.Services.AddScoped<IPeopleService, People.PeopleService>();
builder.Services.AddScoped<IInvitationService, Invitations.InvitationService>();
builder.Services.AddScoped<INotificationService, Notifications.NotificationService>();
builder.Services.AddScoped<IEmailSender, EmailSender.EmailSenderService>();
builder.Services.AddSqliteConnectionFactory();

builder.Services.Configure<EmailNotificationOptions>(builder.Configuration.GetSection("EmailNotifications"));

builder.Services.Configure<DisplayOptions>(builder.Configuration.GetSection("Display"));

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
