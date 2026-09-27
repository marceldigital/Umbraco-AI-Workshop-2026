using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using TheRabbitHole.Core.Integrations.HubSpot;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace TheRabbitHole.Core;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddTheRabbitHole(this IUmbracoBuilder builder)
    {
        // Register services for the podcast episode queue and transcriber
        builder.Services.AddSingleton<PodcastEpisodeQueue>();
        builder.Services.AddHostedService<PodcastEpisodeTranscriber>();
        builder.AddNotificationAsyncHandler<ContentSavedNotification, PodcastEpisodeSavedHandler>();

        // HubSpot CRM client (Integrations/HubSpot) — looks up guest bios + socials by email
        builder.Services.AddOptions<HubSpotOptions>()
            .BindConfiguration(HubSpotOptions.SectionName);
        builder.Services.AddHttpClient<HubSpotGuestClient>();

        // SignalR hub for real-time updates in the backoffice
        builder.Services.AddSignalR();
        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter("TheRabbitHole")
            {
                Endpoints = app => app.UseEndpoints(endpoints =>
                    endpoints.MapHub<PodcastHub>("/umbraco/backoffice/hubs/podcast"))
            });
        });

        return builder;
    }
}
