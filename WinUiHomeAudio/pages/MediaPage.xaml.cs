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

        private IPlayerProxy? Player;

        public MediaPage() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (e.Parameter is NavContext ctx) {
                ListOfMedia = App.Host.Services.GetRequiredService<IMediaRepository>().GetMediaRepository(ctx.category);
                if (!IsSorted.Contains(ctx.category)) {
                    var sorted = ListOfMedia.OrderBy(x => x.Name).ToList();
                    for (int i = 0; i < sorted.Count(); i++) {
                        ListOfMedia.Move(ListOfMedia.IndexOf(sorted[i]), i);
                    }
                    IsSorted.Add(ctx.category);
                }
                Player = ctx.player;
            }
            base.OnNavigatedTo(e);
        }

        private void ItemsView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args) {
            IMedia? item = (args.InvokedItem as IMedia);
            if (item != null) {
                Debug.WriteLine(item.Name);
                if ((item.IsCollection) && (item is Cd cd)) {
                    Player?.PlayCd(cd);
                } else if (item is NamedUrl radio) {
                    Player?.PlayRadio(radio);
                } else if (item is LmsObject lmsObj) {
                    if (lmsObj.IsCollection) {
                        Player?.PlayCd(lmsObj);
                    } else {
                        Player?.PlayRadio(lmsObj);
                    }
                }
            }
        }
    }
}
