namespace TailorApp
{
    // Fully qualified: the project's Clean Architecture "Application" layer
    // (TailorApp.Application) otherwise shadows Microsoft.Maui.Controls.Application here.
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "Tailor App" };
        }
    }
}
