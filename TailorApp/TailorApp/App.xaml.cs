using TailorApp.Application.Interfaces;

namespace TailorApp
{
    // Fully qualified: the project's Clean Architecture "Application" layer
    // (TailorApp.Application) otherwise shadows Microsoft.Maui.Controls.Application here.
    public partial class App : Microsoft.Maui.Controls.Application
    {
        private readonly ISyncService _sync;

        public App(ISyncService sync)
        {
            _sync = sync;
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new MainPage()) { Title = "Tailor App" };

            // Back up again whenever the shop reopens the app. SyncAsync signs in
            // silently, skips when offline, and ignores overlapping runs — so this
            // is safe to fire and forget.
            window.Resumed += (_, _) => _ = _sync.SyncAsync();

            return window;
        }
    }
}
