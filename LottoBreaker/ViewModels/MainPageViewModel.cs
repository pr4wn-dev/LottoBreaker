using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace LottoBreaker.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        public ObservableCollection<string> UnclaimedPrizes { get; } = new ObservableCollection<string>();

        [RelayCommand]
        async Task LoadDataAsync()
        {
            // Here you would fetch data from the Maine Lottery site
            // This is a placeholder
            UnclaimedPrizes.Add("Example Prize 1");
            UnclaimedPrizes.Add("Example Prize 2");
        }
    }
}