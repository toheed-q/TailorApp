using Android.App;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using AndroidX.Core.View;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace TailorApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        // App page background (cream). Painted behind the system bars so the
        // status/navigation bar regions match the UI instead of showing white/purple.
        private static readonly AColor BarBackground = AColor.ParseColor("#F4ECE0");

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var window = Window;
            if (window is null)
            {
                return;
            }

            // Paint the area behind the (now transparent, edge-to-edge) system bars cream.
            window.SetBackgroundDrawable(new ColorDrawable(BarBackground));

            // Android 15 (SDK 35) forces edge-to-edge: the WebView draws behind the
            // status and navigation bars. Opt in explicitly so behaviour is the same
            // on older API levels too, then re-apply the insets as padding below.
            WindowCompat.SetDecorFitsSystemWindows(window, false);

            // Cream bars are light, so request dark (light-appearance) icons.
            var insetsController = WindowCompat.GetInsetsController(window, window.DecorView);
            if (insetsController is not null)
            {
                insetsController.AppearanceLightStatusBars = true;
                insetsController.AppearanceLightNavigationBars = true;
            }

            // Pad the content view by the system-bar + cutout insets so the WebView
            // never sits underneath the status bar (top) or the gesture/nav bar (bottom).
            var content = window.DecorView.FindViewById(Android.Resource.Id.Content);
            if (content is not null)
            {
                ViewCompat.SetOnApplyWindowInsetsListener(content, new SafeAreaInsetsListener());
                ViewCompat.RequestApplyInsets(content);
            }
        }

        private sealed class SafeAreaInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
        {
            public WindowInsetsCompat OnApplyWindowInsets(AView? view, WindowInsetsCompat? insets)
            {
                if (view is null || insets is null)
                {
                    return insets ?? WindowInsetsCompat.Consumed;
                }

                var bars = insets.GetInsets(
                    WindowInsetsCompat.Type.SystemBars() | WindowInsetsCompat.Type.DisplayCutout());
                view.SetPadding(bars.Left, bars.Top, bars.Right, bars.Bottom);
                return WindowInsetsCompat.Consumed;
            }
        }
    }
}
