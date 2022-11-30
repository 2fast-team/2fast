using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


// based on https://github.com/muak/SvgImageSource/blob/922a0dbbbf0b175d6a5ee5766d2891ebb5b93fdf/SvgImageSource/SvgImageSource.cs
namespace Project2FA.MAUI.Controls
{
    /// <summary>
    /// Svg image source.
    /// </summary>
    public class SvgImageSource : ImageSource
    {
        internal static float ScreenScale;

        /// <summary>
        /// The stream func property.
        /// </summary>
        public static BindableProperty StreamFuncProperty =
            BindableProperty.Create(
                nameof(StreamFunc),
                typeof(Func<CancellationToken, Task<Stream>>),
                typeof(SvgImageSource),
                default(Func<CancellationToken, Task<Stream>>),
                defaultBindingMode: BindingMode.OneWay
            );

        /// <summary>
        /// Gets or sets the stream func.
        /// </summary>
        /// <value>The stream func.</value>
        public Func<CancellationToken, Task<Stream>> StreamFunc
        {
            get { return (Func<CancellationToken, Task<Stream>>)GetValue(StreamFuncProperty); }
            set { SetValue(StreamFuncProperty, value); }
        }

        /// <summary>
        /// The source property.
        /// </summary>
        public static BindableProperty SourceProperty =
            BindableProperty.Create(
                nameof(Source),
                typeof(string),
                typeof(SvgImageSource),
                default(string),
                defaultBindingMode: BindingMode.OneWay
            );

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// The width property.
        /// </summary>
        public static BindableProperty WidthProperty =
            BindableProperty.Create(
                nameof(Width),
                typeof(double),
                typeof(SvgImageSource),
                default(double),
                defaultBindingMode: BindingMode.OneWay
            );

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        /// <summary>
        /// The height property.
        /// </summary>
        public static BindableProperty HeightProperty =
            BindableProperty.Create(
                nameof(Height),
                typeof(double),
                typeof(SvgImageSource),
                default(double),
                defaultBindingMode: BindingMode.OneWay
            );

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        /// <summary>
        /// The color property.
        /// </summary>
        public static BindableProperty ColorProperty =
            BindableProperty.Create(
                nameof(Color),
                typeof(Color),
                typeof(SvgImageSource),
                default(Color),
                defaultBindingMode: BindingMode.OneWay
            );

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        static Assembly AssemblyCache;

        static Lazy<HttpClient> _lazyClient = new Lazy<HttpClient>();
        static HttpClient _httpClient => _lazyClient.Value;

        /// <summary>
        /// Registers the assembly.
        /// </summary>
        /// <param name="typeHavingResource">Type having resource.</param>
        public static void RegisterAssembly(Type typeHavingResource = null)
        {
            if (typeHavingResource == null)
            {
                AssemblyCache = Assembly.GetCallingAssembly();
            }
            else
            {
                AssemblyCache = typeHavingResource.GetTypeInfo().Assembly;
            }
        }

        /// <summary>
        /// Froms the svg.
        /// </summary>
        /// <returns>The svg.</returns>
        /// <param name="resource">Resource.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="color">Color.</param>
        public static ImageSource FromSvgResource(string resource, double width, double height, Color color = default)
        {

            AssemblyCache = AssemblyCache ?? Assembly.GetCallingAssembly();

            if (AssemblyCache == null)
            {
                return null;
            }

            return new SvgImageSource { StreamFunc = GetResourceStreamFunc(resource), Source = resource, Width = width, Height = height, Color = color };
        }

        /// <summary>
        /// Froms the svg.
        /// </summary>
        /// <returns>The svg.</returns>
        /// <param name="resource">Resource.</param>
        /// <param name="color">Color.</param>
        public static ImageSource FromSvgResource(string resource, Color color = default)
        {
            AssemblyCache = AssemblyCache ?? Assembly.GetCallingAssembly();

            if (AssemblyCache == null)
            {
                return null;
            }

            return new SvgImageSource { StreamFunc = GetResourceStreamFunc(resource), Source = resource, Color = color };

        }

        /// <summary>
        /// Froms the svg URI.
        /// </summary>
        /// <returns>The svg URI.</returns>
        /// <param name="uri">URI.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="color">Color.</param>
        public static ImageSource FromSvgUri(string uri, double width, double height, Color color)
        {
            return new SvgImageSource { StreamFunc = GethttpStreamFunc(uri), Source = uri, Width = width, Height = height, Color = color };
        }

        //public Task<ImageSource> SetSourceAsync(Stream contentStream, double width, double height, Color color)
        //{
        //    return new SvgImageSource { StreamFunc = token => GetResourceStreamFunc(contentStream), Width = width, Height = height, Color = color };
        //}

        /// <summary>
        /// Froms the svg stream.
        /// </summary>
        /// <returns>The svg stream.</returns>
        /// <param name="streamFunc">Stream func.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="color">Color.</param>
        /// <param name="key">Key.</param>
        public static ImageSource FromSvgStream(Func<Stream> streamFunc, double width, double height, Color color, string key = null)
        {
            key = key ?? streamFunc.GetHashCode().ToString();
            return new SvgImageSource { StreamFunc = token => Task.Run(streamFunc), Width = width, Height = height, Color = color };
        }

        static Func<CancellationToken, Task<Stream>> GetResourceStreamFunc(string resource)
        {
            //var realResource = GetRealResource(resource);
            //if (realResource == null)
            //{
            //    return null;
            //}
            return token => Task.Run(() => AssemblyCache.GetManifestResourceStream(resource), token);

        }

        static Func<CancellationToken, Task<Stream>> GetResourceStreamFunc(Stream contentStream)
        {
            return token => Task.Run(async() => await SvgUtility.CreateImageTask(contentStream, 68, 68, Colors.Transparent), token);
        }

        static Func<CancellationToken, Task<Stream>> GethttpStreamFunc(string uri)
        {
            return token => Task.Run(async () =>
            {
                var response = await _httpClient.GetAsync(uri, token);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("HTTP Request", $"Could not retrieve {uri}, status code {response.StatusCode}");
                    return null;
                }
                // the HttpResponseMessage needs to be disposed of after the calling code is done with the stream 
                // otherwise the stream may get disposed before the caller can use it
                return new StreamWrapper(await response.Content.ReadAsStreamAsync().ConfigureAwait(false), response) as Stream;
            }, token);
        }

        static string GetRealResource(string resource)
        {
            return AssemblyCache.GetManifestResourceNames()
                              .FirstOrDefault(x => x.EndsWith(resource, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Ons the property changed.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected override void OnPropertyChanged(string propertyName)
        {
            if (propertyName == StreamFuncProperty.PropertyName)
            {
                OnSourceChanged();
            }
            else if (propertyName == SourceProperty.PropertyName)
            {
                if (string.IsNullOrEmpty(Source))
                {
                    return;
                }

                if (Uri.TryCreate(Source, UriKind.Absolute, out var uri))
                {
                    StreamFunc = GethttpStreamFunc(Source);
                }
                else
                {
                    StreamFunc = GetResourceStreamFunc(Source);
                }

                OnSourceChanged();
            }
            base.OnPropertyChanged(propertyName);
        }

        internal virtual async Task<Stream> GetImageStreamAsync(CancellationToken userToken)
        {
            //OnSourceChanged();
            //OnLoadingStarted();
            userToken.Register(new CancellationTokenSource().Cancel);

            Stream imageStream = null;
            try
            {
                using (var stream = await StreamFunc(new CancellationTokenSource().Token).ConfigureAwait(false))
                {
                    if (stream == null)
                    {
                        //OnLoadingCompleted(false);
                        return null;
                    }
                    imageStream = await SvgUtility.CreateImageTask(stream, Width, Height, Color);
                }

                //OnLoadingCompleted(false);
            }
            catch (OperationCanceledException oex)
            {
                //OnLoadingCompleted(true);
                System.Diagnostics.Debug.WriteLine($"cancel exception {oex.Message}");
                throw;
            }

            return imageStream;
        }
    }
}
