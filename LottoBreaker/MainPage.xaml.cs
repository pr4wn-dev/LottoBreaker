using LottoBreaker.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections;
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
                await RefreshUIAsync();
                isLoading = false;
            }
        }


        private async Task RefreshUIAsync()
        {
            if (BindingContext is MainPageViewModel viewModel)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    System.Diagnostics.Debug.WriteLine("Before Refresh: collectionView.ItemsSource count: " + (collectionView.ItemsSource as ICollection)?.Count);

                    // Create a new collection to force a refresh
                    var newCollection = new ObservableCollection<TicketGame>(viewModel.TicketGame);

                    // Clear and set new collection
                    collectionView.ItemsSource = null;
                    collectionView.ItemsSource = newCollection;

                    System.Diagnostics.Debug.WriteLine("After Refresh: collectionView.ItemsSource count: " + (collectionView.ItemsSource as ICollection)?.Count);
                });
            }
        }
    }
}