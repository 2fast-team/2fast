using Markdig.Syntax;

namespace Symptum.UI.Markdown;
public partial class MarkdownTextBlock
{
    #region Configuration

    public static readonly DependencyProperty ConfigurationProperty = DependencyProperty.Register(
        nameof(Configuration),
        typeof(MarkdownConfiguration),
        typeof(MarkdownTextBlock),
        new PropertyMetadata(MarkdownConfiguration.Default, OnConfigChanged));

    public MarkdownConfiguration Configuration
    {
        get => (MarkdownConfiguration)GetValue(ConfigurationProperty);
        set => SetValue(ConfigurationProperty, value);
    }

    private static void OnConfigChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MarkdownTextBlock self && e.NewValue != null)
        {
            self.ApplyConfig(self.Configuration);
        }
    }

    #endregion

    #region Text

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(MarkdownTextBlock),
        new PropertyMetadata(string.Empty, OnTextChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MarkdownTextBlock self && e.NewValue is string text)
        {
            self.ApplyText(true);
        }
    }

    #endregion

    #region MarkdownDocument

    public static readonly DependencyProperty MarkdownDocumentProperty = DependencyProperty.Register(
        nameof(MarkdownDocument),
        typeof(MarkdownDocument),
        typeof(MarkdownTextBlock),
        new PropertyMetadata(null));

    public MarkdownDocument? MarkdownDocument
    {
        get => (MarkdownDocument)GetValue(MarkdownDocumentProperty);
        private set => SetValue(MarkdownDocumentProperty, value);
    }

    #endregion

    public DocumentOutline DocumentOutline { get; }

    public ImportsHandler ImportsHandler { get; }
}
