using Windows.System;

namespace Symptum.UI.Markdown;

public sealed class DefaultLinkHandler : ILinkHandler
{
    private DocumentOutline _documentOutline;

    public DefaultLinkHandler(DocumentOutline documentOutline)
    {
        _documentOutline = documentOutline;
    }

    public async void HandleNavigation(string? url, string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        if (url.StartsWith('#'))
        {
            if (_documentOutline.IdNavigateCollection.TryGetValue(url.Remove(0, 1), out Action? navigate))
                navigate!();
        }
        else
        {
            Uri uri = Helper.GetUri(url, baseUrl);
            await Launcher.LaunchUriAsync(uri);
        }
    }
}
