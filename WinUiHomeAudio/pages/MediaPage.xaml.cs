using AudioCollectionApi.api;
using AudioCollectionApi.model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WinUiHomeAudio.model;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUiHomeAudio.pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MediaPage : VmPage {

        private ObservableCollection<IMedia> _ListOfMedia = new ObservableCollection<IMedia>();
        public ObservableCollection<IMedia> ListOfMedia { get { return _ListOfMedia; } set { if (_ListOfMedia != value) { _ListOfMedia = value; RaisePropertyChanged(); } } }

        public MediaPage() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            //var p = e.Parameter;
            ListOfMedia = App.Host.Services.GetRequiredService<IMediaRepository>().GetMediaRepository(e?.Parameter?.ToString() ?? "X");
            base.OnNavigatedTo(e);
        }

        private void ItemsView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args) {
            IMedia? item = (args.InvokedItem as IMedia);
            if (item != null) {
                Debug.WriteLine(item.Name);
                if ((item.IsCollection) && (item is Cd cd)) {
                    App.Host.Services.GetRequiredService<ChromeCastRepository>().PlayCed(cd);
                } else if (item is NamedUrl radio) {
                    App.Host.Services.GetRequiredService<ChromeCastRepository>().PlayRadio(radio);
                }
            }
        }
    }
}
