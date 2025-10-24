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
        get => _headerVisibility;
        set
        {
            if (_headerVisibility == value) return;
            _headerVisibility = value;
        }
    }
    private Visibility _headerVisibility = Visibility.Visible;
}
