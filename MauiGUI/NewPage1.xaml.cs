using AudioCollectionApi;
using System.Collections.ObjectModel;

namespace MauiGUI;

public partial class NewPage1 : ContentPage
{
    IMediaRepository MediaRepository { get; set; }

    MediaVm MediaVm { get { return (MediaVm)(BindingContext); } }


	public NewPage1()
	{
		InitializeComponent();
		MediaRepository = ((App)App.Current).Services.GetService<IMediaRepository>();
		_ = MediaRepository.LoadAllAsync("");

        MediaVm.cdcat = MediaRepository.GetCdCategories();

    }

    private void ListView_ItemTapped(object sender, ItemTappedEventArgs e) {
        MediaVm.cdcat.Add(new MediaCategory("myid") { Name = "lökölköl" });
    }
}