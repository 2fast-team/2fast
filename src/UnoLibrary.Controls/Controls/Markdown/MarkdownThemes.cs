namespace Symptum.UI.Markdown;

public sealed partial class MarkdownThemes : DependencyObject
{
    internal static MarkdownThemes Default { get; } = new();

    #region Common

    public Thickness Padding { get; set; } = new(36);

    public double Spacing { get; set; } = 12.0;

    public CornerRadius CornerRadius { get; set; } = new(4);

    public Style? BodyTextBlockStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultBodyTextBlockStyle") ?
        Application.Current.Resources["DefaultBodyTextBlockStyle"] as Style : null;


    #endregion

    #region Heading

    public Style? H1TextBlockStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultH1TextBlockStyle") ?
        Application.Current.Resources["DefaultH1TextBlockStyle"] as Style : null;

    public Style? H2TextBlockStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultH2TextBlockStyle") ?
        Application.Current.Resources["DefaultH2TextBlockStyle"] as Style : null;

    public Style? H3TextBlockStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultH3TextBlockStyle") ?
        Application.Current.Resources["DefaultH3TextBlockStyle"] as Style : null;

    public Style? H4TextBlockStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultH4TextBlockStyle") ?
        Application.Current.Resources["DefaultH4TextBlockStyle"] as Style : null;

    public Style? H5TextBlockStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultH5TextBlockStyle") ?
        Application.Current.Resources["DefaultH5TextBlockStyle"] as Style : null;

    public Style? H6TextBlockStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultH6TextBlockStyle") ?
        Application.Current.Resources["DefaultH6TextBlockStyle"] as Style : null;

    #endregion

    #region Code

    public Style? CodeTextBlockStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultCodeTextBlockStyle") ?
        Application.Current.Resources["DefaultCodeTextBlockStyle"] as Style : null;

    public Style? CodeInlineBorderStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultCodeInlineBorderStyle") ?
        Application.Current.Resources["DefaultCodeInlineBorderStyle"] as Style : null;

    public Style? CodeBlockBorderStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultCodeBlockBorderStyle") ?
        Application.Current.Resources["DefaultCodeBlockBorderStyle"] as Style : null;

    #endregion

    #region Quote

    public Style? DefaultQuoteControlStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultQuoteControlStyle") ?
        Application.Current.Resources["DefaultQuoteControlStyle"] as Style : null;

    public Style? NoteQuoteControlStyle { get; set; } = Application.Current.Resources.ContainsKey("NoteQuoteControlStyle") ?
        Application.Current.Resources["NoteQuoteControlStyle"] as Style : null;

    public Style? TipQuoteControlStyle { get; set; } = Application.Current.Resources.ContainsKey("TipQuoteControlStyle") ?
        Application.Current.Resources["TipQuoteControlStyle"] as Style : null;

    public Style? ImportantQuoteControlStyle { get; set; } = Application.Current.Resources.ContainsKey("ImportantQuoteControlStyle") ?
        Application.Current.Resources["ImportantQuoteControlStyle"] as Style : null;

    public Style? WarningQuoteControlStyle { get; set; } = Application.Current.Resources.ContainsKey("WarningQuoteControlStyle") ?
        Application.Current.Resources["WarningQuoteControlStyle"] as Style : null;

    public Style? CautionQuoteControlStyle { get; set; } = Application.Current.Resources.ContainsKey("CautionQuoteControlStyle") ?
        Application.Current.Resources["CautionQuoteControlStyle"] as Style : null;

    #endregion

    #region List

    public Thickness ListMargin { get; set; } = new(16, 8, 0, 8);

    public double ListBulletSpacing { get; set; } = 12;

    #endregion

    #region Table

    public Style? TableCellGridStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultTableCellGridStyle") ?
        Application.Current.Resources["DefaultTableCellGridStyle"] as Style : null;

    public Style? AltTableCellGridStyle { get; set; } = Application.Current.Resources.ContainsKey("AltTableCellGridStyle") ?
        Application.Current.Resources["AltTableCellGridStyle"] as Style : null;

    public Style? TableHeaderCellGridStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultTableHeaderCellGridStyle") ?
        Application.Current.Resources["DefaultTableHeaderCellGridStyle"] as Style : null;

    public Style? TableHeaderTextBlockStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultTableHeaderTextBlockStyle") ?
        Application.Current.Resources["DefaultTableHeaderTextBlockStyle"] as Style : null;

    #endregion

    #region Thematic Break

    public Style? ThematicBreakBorderStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultThematicBreakBorderStyle") ?
        Application.Current.Resources["DefaultThematicBreakBorderStyle"] as Style : null;

    #endregion

    #region Address Block

    public Style? AddressBlockTextBlockStyle { get; set; } = Application.Current.Resources.ContainsKey("DefaultAddressBlockTextBlockStyle") ?
        Application.Current.Resources["DefaultAddressBlockTextBlockStyle"] as Style : null;

    #endregion

    public MarkdownThemes() { }
}
