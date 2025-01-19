using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using HtmlAgilityPack;
using System.Net.Http;

namespace LottoBreaker.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        public ObservableCollection<string> UnclaimedPrizes { get; } = new ObservableCollection<string>();

        [RelayCommand]
        async Task LoadDataAsync()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://www.mainelottery.com/games/instant-games/unclaimed-prizes/");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(content);
                // Parse the HTML to get unclaimed prizes. This is very basic and needs refinement based on actual HTML structure.
                var nodes = doc.DocumentNode.SelectNodes("//div[@class='prize-list']");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        UnclaimedPrizes.Add(node.InnerText.Trim());
                    }
                }
            }
        }
    }
}