using TheRabbitHole.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace TheRabbitHole.Core.Extensions;

public static partial class PublishedContentExtensions
{
    // Navigation
    public static Home GetHomePage(this IPublishedContent content) => content.AncestorOrSelf<Home>()!;
}