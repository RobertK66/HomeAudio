using AudioCollectionApi.api;
using AudioCollectionApi.model;
using LmsRepositiory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using WinUiHomeAudio.model;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUiHomeAudio.pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MediaPage : VmPage {
        private List<string> IsSorted = new List<string>();

        private ObservableCollection<IMedia> _ListOfMedia = new ObservableCollection<IMedia>();
        public ObservableCollection<IMedia> ListOfMedia { get { return _ListOfMedia; } set { if (_ListOfMedia != value) { _ListOfMedia = value; RaisePropertyChanged(); } } }

        public MediaPage() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            string? cat = e?.Parameter?.ToString();
            if (cat != null) {
                ListOfMedia = App.Host.Services.GetRequiredService<IMediaRepository>().GetMediaRepository(cat);
                if (!IsSorted.Contains(cat)) {
                    var sorted = ListOfMedia.OrderBy(x => x.Name).ToList();
                    for (int i = 0; i < sorted.Count(); i++) {
                        ListOfMedia.Move(ListOfMedia.IndexOf(sorted[i]), i);
                    }
                    IsSorted.Add(cat);
                }
            }
            base.OnNavigatedTo(e);
        }

        private void ItemsView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args) {
            IMedia? item = (args.InvokedItem as IMedia);
            if (item != null) {
                Debug.WriteLine(item.Name);
                if ((item.IsCollection) && (item is Cd cd)) {
                    App.Host.Services.GetRequiredService<IPlayerRepository>().PlayCd(cd);
                } else if (item is NamedUrl radio) {
                    App.Host.Services.GetRequiredService<IPlayerRepository>().PlayRadio(radio);
                } else if (item is LmsObject lmsObj) {
                    if (lmsObj.IsCollection) {
                        App.Host.Services.GetRequiredService<IPlayerRepository>().PlayCd(lmsObj);
                    } else {
                        App.Host.Services.GetRequiredService<IPlayerRepository>().PlayRadio(lmsObj);
                    }
                }
            }
        }
    }
}
