namespace Symptum.UI.Markdown;

[TemplatePart(Name = QuoteContainerName, Type = typeof(ContentControl))]
public sealed partial class QuoteControl : Control
{
    private const string QuoteContainerName = "QuoteContainer";
    private ContentControl? _container;

    #region Properties

    #region Alert Kind

    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind),
        typeof(AlertKind),
        typeof(QuoteControl),
        new PropertyMetadata(AlertKind.None));

    public AlertKind Kind
    {
        get => (AlertKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    #endregion

    #region Alert Foreground

    public static readonly DependencyProperty AlertForegroundProperty = DependencyProperty.Register(
        nameof(AlertForeground),
        typeof(Brush),
        typeof(QuoteControl),
        new PropertyMetadata(null));

    public Brush AlertForeground
    {
        get => (Brush)GetValue(AlertForegroundProperty);
        set => SetValue(AlertForegroundProperty, value);
    }

    #endregion

    #region Content

    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content),
        typeof(object),
        typeof(QuoteControl),
        new PropertyMetadata(null));

    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    #endregion

    #endregion

    public QuoteControl()
    {
        DefaultStyleKey = typeof(QuoteControl);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _container = (ContentControl)GetTemplateChild(QuoteContainerName);
    }
}

public enum AlertKind
{
    None,
    Note,
    Tip,
    Important,
    Warning,
    Caution,
}
