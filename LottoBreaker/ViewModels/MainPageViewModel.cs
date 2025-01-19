using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using HtmlAgilityPack;
using System.Net.Http;
using System.ComponentModel;
using System.Windows.Input;

namespace LottoBreaker.ViewModels
{

    public class MainPageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _unclaimedPrizes = new ObservableCollection<string>();

        public ObservableCollection<string> UnclaimedPrizes
        {
            get => _unclaimedPrizes;
            set
            {
                if (_unclaimedPrizes != value)
                {
                    _unclaimedPrizes = value;
                    OnPropertyChanged(nameof(UnclaimedPrizes));
                }
            }
        }

        public ICommand LoadDataCommand { get; }

        public MainPageViewModel()
        {
            LoadDataCommand = new Command(async () => await LoadDataAsync());
        }

        public async Task LoadDataAsync()
        {
            Console.WriteLine("boo");
            // Simulate fetching data, replace this with your actual data loading logic

            var client = new HttpClient();
            var response = await client.GetAsync("https://www.mainelottery.com/players_info/unclaimed_prizes.html");
          
            if (response.IsSuccessStatusCode)
            {
               
                var content = await response.Content.ReadAsStringAsync();
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(content);
                // Parse the HTML to get unclaimed prizes. This is very basic and needs refinement based on actual HTML structure.
                //UnclaimedPrizes.Clear();
                var nodes = doc.DocumentNode.SelectNodes("//div[@class='maincontent1']");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                       
                        UnclaimedPrizes.Add(node.InnerText.Trim());
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}














































