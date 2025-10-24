using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MSAVA_App.Presentation;

public sealed partial class Shell : UserControl, IContentControlProvider
{
    public Shell()
    {
        this.InitializeComponent();
    }

    public ContentControl ContentControl => Splash;

    public Visibility HeaderVisibility
    {
        get => (Visibility)GetValue(HeaderVisibilityProperty);
        set => SetValue(HeaderVisibilityProperty, value);
    }

    public static readonly DependencyProperty HeaderVisibilityProperty =
        DependencyProperty.Register(
            nameof(HeaderVisibility),
            typeof(Visibility),
            typeof(Shell),
            new PropertyMetadata(Visibility.Visible));
}
