using Markdig.Syntax.Inlines;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using HtmlAgilityPack;
using System.Globalization;

namespace Symptum.UI.Markdown.TextElements;

public class ImageElement : IAddChild
{
    private SContainer _container = new();
    private LinkInline? _linkInline;
    private HtmlNode? _htmlNode;
    private Image _image = new();
    private Uri _uri;
    private IImageProvider? _imageProvider;
    private ISVGRenderer _svgRenderer;
    private double _precedentWidth;
    private double _precedentHeight;
    private bool _loaded;

    private TextBlock _altText;
    private static readonly Dictionary<Uri, ImageSource> _imageCache = [];

    public STextElement TextElement => _container;

    public ImageElement(LinkInline linkInline, Uri uri, MarkdownConfiguration config)
    {
        _linkInline = linkInline;
        _uri = uri;
        _imageProvider = config.ImageProvider;
        _svgRenderer = config.SVGRenderer ?? new DefaultSVGRenderer();
        Init(linkInline.Label, config);
        Size size = Helper.GetMarkdownImageSize(linkInline);
        if (size.Width != 0)
        {
            _precedentWidth = size.Width;
        }
        if (size.Height != 0)
        {
            _precedentHeight = size.Height;
        }
    }

    public ImageElement(HtmlNode htmlNode, MarkdownConfiguration config)
    {
        if (Uri.TryCreate(htmlNode.GetAttribute("src", "#"), UriKind.RelativeOrAbsolute, out Uri? uri))
            _uri = uri;

        _htmlNode = htmlNode;
        _imageProvider = config.ImageProvider;
        _svgRenderer = config.SVGRenderer ?? new DefaultSVGRenderer();
        Init(htmlNode.GetAttribute("alt", string.Empty), config);
        int.TryParse(htmlNode.GetAttribute("width", "0"),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var width);

        int.TryParse(htmlNode.GetAttribute("height", "0"),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var height);

        if (width > 0)
        {
            _precedentWidth = width;
        }
        if (height > 0)
        {
            _precedentHeight = height;
        }
    }

    private void Init(string? altText, MarkdownConfiguration config)
    {
        _image.Loaded += LoadImage;
        Grid _grid = new();
        _grid.RowDefinitions.Add(new() { Height = new(0, GridUnitType.Auto) });
        _grid.RowDefinitions.Add(new() { Height = new(0, GridUnitType.Auto) });
        _altText = new()
        {
            Text = altText,
            Style = config.Themes.BodyTextBlockStyle
        };
        _altText.SetValue(Grid.RowProperty, 1);
        _grid.Children.Add(_altText);
        _grid.Children.Add(_image);
        _container.UIElement = _grid;
    }

    private async void LoadImage(object sender, RoutedEventArgs e)
    {
        if (_loaded) return;

        void imageLoaded(ImageSource source)
        {
            _loaded = true;
            _imageCache.TryAdd(_uri, source);
            _altText.Visibility = Visibility.Collapsed;
        }

        if (_imageCache.TryGetValue(_uri, out ImageSource? value))
        {
            _image.Source = value;
            imageLoaded(value);
        }
        else
        {
            try
            {
                if (_imageProvider != null && _imageProvider.ShouldUseThisProvider(_uri.AbsoluteUri))
                {
                    var source = await _imageProvider.GetImageSource(_uri.AbsoluteUri);
                    _image.Source = source;
                    imageLoaded(source);
                }
                else if (_uri.Scheme == "file")
                {
                    StorageFile? file = await StorageFile.GetFileFromPathAsync(_uri.LocalPath);
                    if (file != null)
                    {
                        using IRandomAccessStream? stream = await file.OpenAsync(FileAccessMode.Read);
                        BitmapImage bitmap = new();
                        if (stream != null) await bitmap.SetSourceAsync(stream);
                        _image.Source = bitmap;
                        _image.Width = bitmap.PixelWidth == 0 ? bitmap.DecodePixelWidth : bitmap.PixelWidth;
                        _image.Height = bitmap.PixelHeight == 0 ? bitmap.DecodePixelHeight : bitmap.PixelHeight;
                        imageLoaded(bitmap);
                    }
                }
                else
                {
                    HttpClient client = new();
//#if __WASM__
//                    client.DefaultRequestHeaders.Add("Access-Control-Allow-Origin", "*");
//                    client.DefaultRequestHeaders.Add("Access-Control-Allow-Methods", "*");
//                    client.DefaultRequestHeaders.Add("Access-Control-Allow-Headers", "*");
//                    client.DefaultRequestHeaders.Add("Access-Control-Max-Age", "86400");
//#endif
                    HttpResponseMessage response = await client.GetAsync(_uri);
                    if (response != null)
                    {
                        string? contentType = response.Content.Headers?.ContentType?.MediaType;
                        if (contentType == "image/svg+xml")
                        {
                            string? svgString = await response.Content.ReadAsStringAsync();
                            ImageSource resImage = await _svgRenderer.SvgToImageSource(svgString);
                            if (resImage != null)
                            {
                                _image.Source = resImage;
                                Size size = Helper.GetSvgSize(svgString);
                                if (size.Width > 0) _image.Width = size.Width;
                                if (size.Height > 0) _image.Height = size.Height;
                                imageLoaded(resImage);
                            }
                        }
                        else
                        {
                            using Stream? stream = await response.Content.ReadAsStreamAsync();
                            BitmapImage bitmap = new();

                            if (stream != null) await bitmap.SetSourceAsync(stream.AsRandomAccessStream());

                            _image.Source = bitmap;
                            _image.Width = bitmap.PixelWidth == 0 ? bitmap.DecodePixelWidth : bitmap.PixelWidth;
                            _image.Height = bitmap.PixelHeight == 0 ? bitmap.DecodePixelHeight : bitmap.PixelHeight;
                            imageLoaded(bitmap);
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        if (_precedentWidth != 0)
        {
            _image.Width = _precedentWidth;
        }
        if (_precedentHeight != 0)
        {
            _image.Height = _precedentHeight;
        }
    }

    public void AddChild(IAddChild child)
    {
        if (child != null && child.TextElement is SInline inline)
        {
            _altText.Inlines.Add(inline.Inline);
        }
    }
}
