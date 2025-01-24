using LottoBreaker.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace LottoBreaker
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainPageViewModel();
        }

        [Obsolete]
        private bool isLoading = false;

        [Obsolete]
        private async void OnLoadDataButtonClicked(object sender, EventArgs e)
        {
            if (!isLoading && BindingContext is MainPageViewModel viewModel)
            {
                isLoading = true;
                await viewModel.LoadDataAsync();
                await Task.Delay(100); // Small delay to ensure data is fully processed
                RefreshUI();
                isLoading = false;
            }
        }

        private void RefreshUI()
        {
            if (BindingContext is MainPageViewModel viewModel)
            {
                // Create a new collection to force a refresh
                var newCollection = new ObservableCollection<TicketGame>(viewModel.TicketGame);
                collectionView.ItemsSource = newCollection;
            }
        }
    }
}