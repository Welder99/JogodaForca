namespace JogodaForca
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Console.WriteLine("App initialized successfully.");
            MainPage = new AppShell();
        }
    }
}
