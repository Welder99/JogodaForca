using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views; // Necessário para acessar Window e SetStatusBarColor

namespace JogodaForca
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Define a cor da Status Bar
            Window.SetStatusBarColor(Android.Graphics.Color.Black); // Altere para a cor desejada, por exemplo, Color.Black
        }
    }
}
