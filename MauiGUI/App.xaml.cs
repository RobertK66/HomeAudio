namespace MauiGUI {
    public partial class App : Application {

        public IServiceProvider Services;
        public App(IServiceProvider provider) {
            InitializeComponent();
            Services = provider;
            MainPage = new AppShell();
        }
    }
}