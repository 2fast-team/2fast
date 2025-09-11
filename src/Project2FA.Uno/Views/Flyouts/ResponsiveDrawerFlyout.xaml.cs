namespace Project2FA.Uno.Views.Flyouts
{

    public sealed partial class ResponsiveDrawerFlyout : Flyout
    {
        private const int WideBreakpoint = 800;
        private const int WidestBreakpoint = 1080;

        private FlyoutPresenter? _presenter;

        public ResponsiveDrawerFlyout()
        {
            this.InitializeComponent();
        }

        private void OnOpening(object? sender, object e)
        {
            if (_presenter is { } presenter)
            {
                var width = XamlRoot?.Size.Width ?? 0;
                if (width >= WideBreakpoint)
                {
                    var gridLength = width > WidestBreakpoint ? 0.33 : 0.66;

                    DrawerFlyoutPresenter.SetDrawerLength(presenter, new GridLength(gridLength, GridUnitType.Star));
                    DrawerFlyoutPresenter.SetOpenDirection(presenter, DrawerOpenDirection.Left);
                    DrawerFlyoutPresenter.SetIsGestureEnabled(presenter, false);
                    presenter.CornerRadius = new CornerRadius(20, 0, 0, 20);
                }
                else
                {
                    DrawerFlyoutPresenter.SetDrawerLength(presenter, new GridLength(1, GridUnitType.Star));
                    DrawerFlyoutPresenter.SetIsGestureEnabled(presenter, false);
                }
            }
        }

        protected override Control? CreatePresenter()
        {
            var basePresenter = base.CreatePresenter();

            _presenter = basePresenter as FlyoutPresenter;

            return basePresenter;
        }
    }
}
