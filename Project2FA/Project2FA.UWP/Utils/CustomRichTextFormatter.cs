using Project2FA.UWP.Controls;
using Project2FA.UWP.Controls.TextToolbarButtons;
using Project2FA.UWP.Controls.TextToolbarButtons.Common;
using Project2FA.UWP.Controls.TextToolbarFormats;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Utils
{
    /// <summary>
    /// Rudimentary showcase of RichText and Toggleable Toolbar Buttons.
    /// </summary>
    public class CustomRichTextFormatter : Formatter
    {
        /// <inheritdoc/>
        public override void SetModel(TextToolbar model)
        {
            base.SetModel(model);

            CommonButtons = new CommonButtons(model);
            ButtonActions = new CustomRichTextButtonActions(this);
        }

        /// <inheritdoc/>
        public override void OnSelectionChanged()
        {
            if (Selected.CharacterFormat.Bold == FormatEffect.On)
            {
                BoldButton.IsToggled = true;
            }
            else
            {
                BoldButton.IsToggled = false;
            }

            if (Selected.CharacterFormat.Italic == FormatEffect.On)
            {
                ItalicButton.IsToggled = true;
            }
            else
            {
                ItalicButton.IsToggled = false;
            }

            if (Selected.CharacterFormat.Strikethrough == FormatEffect.On)
            {
                StrikeButton.IsToggled = true;
            }
            else
            {
                StrikeButton.IsToggled = false;
            }

            if (Selected.CharacterFormat.Underline != UnderlineType.None)
            {
                Underline.IsToggled = true;
            }
            else
            {
                Underline.IsToggled = false;
            }

            switch (Selected.ParagraphFormat.ListType)
            {
                case MarkerType.Bullet:
                    ListButton.IsToggled = true;
                    OrderedListButton.IsToggled = false;
                    break;

                default:
                    OrderedListButton.IsToggled = true;
                    ListButton.IsToggled = false;
                    break;

                case MarkerType.Undefined:
                case MarkerType.None:
                    ListButton.IsToggled = false;
                    OrderedListButton.IsToggled = false;
                    break;
            }

            base.OnSelectionChanged();
        }

        private CommonButtons CommonButtons { get; set; }

        /// <inheritdoc/>
        public override string Text
        {
            get
            {
                Model.Editor.Document.GetText(TextGetOptions.FormatRtf, out var currentvalue);
                return currentvalue;
            }
        }

        internal ToolbarButton BoldButton { get; set; }

        internal ToolbarButton ItalicButton { get; set; }

        internal ToolbarButton StrikeButton { get; set; }

        internal ToolbarButton Underline { get; set; }

        internal ToolbarButton ListButton { get; set; }

        internal ToolbarButton OrderedListButton { get; set; }

        /// <inheritdoc/>
        public override ButtonMap DefaultButtons
        {
            get
            {
                BoldButton = CommonButtons.Bold;
                BoldButton.ToolTip = Strings.Resources.TextFormatterBoldToolTip;
                ItalicButton = CommonButtons.Italics;
                ItalicButton.ToolTip = Strings.Resources.TextFormatterItalicToolTip;
                StrikeButton = CommonButtons.Strikethrough;
                StrikeButton.ToolTip = Strings.Resources.TextFormatterStrikeToolTip;
                Underline = new ToolbarButton
                {
                    ToolTip = Strings.Resources.TextFormatterUnderlineToolTip,
                    Icon = new SymbolIcon { Symbol = Symbol.Underline },
                    ShortcutKey = VirtualKey.U,
                    Activation = ((CustomRichTextButtonActions)ButtonActions).FormatUnderline
                };
                ListButton = CommonButtons.List;
                ListButton.ToolTip = Strings.Resources.TextFormatterListToolTip;
                OrderedListButton = CommonButtons.OrderedList;
                OrderedListButton.ToolTip = Strings.Resources.TextFormatterOrderedToolTip;

                return new ButtonMap
                {
                    BoldButton,
                    ItalicButton,
                    Underline,

                    new ToolbarSeparator(),

                    StrikeButton,

                    new ToolbarSeparator(),

                    ListButton,
                    OrderedListButton
                };
            }
        }

        /// <summary>
        /// Gets or sets format used for formatting selection in editor
        /// </summary>
        public ITextCharacterFormat SelectionFormat
        {
            get { return Selected.CharacterFormat; }
            set { Selected.CharacterFormat = value; }
        }

        /// <inheritdoc/>
        public override string NewLineChars => "\r\n";
    }
}
