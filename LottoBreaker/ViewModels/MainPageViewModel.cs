using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using HtmlAgilityPack;
using Microsoft.Maui.Controls;

namespace LottoBreaker.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<UnclaimedPrize> UnclaimedPrizes { get; set; } = new ObservableCollection<UnclaimedPrize>();

        public ICommand LoadDataCommand { get; }

        public MainPageViewModel()
        {
            LoadDataCommand = new Command(async () => await LoadDataAsync());
        }

        public async Task LoadDataAsync()
        {
            System.Diagnostics.Debug.WriteLine("LoadDataAsync method started.");

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://www.mainelottery.com/players_info/unclaimed_prizes.html")
            };

            try
            {
                using (var response = await client.SendAsync(request))
                {
                    System.Diagnostics.Debug.WriteLine("HTTP Response Status Code: " + response.StatusCode);
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine("Response body length: " + body.Length);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(body);

                    var prizeNodes = doc.DocumentNode.SelectNodes("//table[@class='tbstriped']/tr");
                    if (prizeNodes != null)
                    {
                        UnclaimedPrizes.Clear();
                        UnclaimedPrize currentGame = null;
                        foreach (var node in prizeNodes.Skip(1)) // Skip the header row
                        {
                            var cells = node.SelectNodes("td");
                            if (cells != null && cells.Count >= 7)
                            {
                                var pricePoint = cells[0].InnerText.Trim();
                                if (!string.IsNullOrEmpty(pricePoint) && pricePoint != " ") // New game
                                {
                                    // Start of new game
                                    currentGame = new UnclaimedPrize
                                    {
                                        PricePoint = pricePoint,
                                        GameNumber = cells[1].InnerText.Trim(),
                                        GameName = cells[2].InnerText.Trim(),
                                        PercentUnsold = cells[3].InnerText.Trim(),
                                        TotalUnclaimed = cells[4].InnerText.Trim(),
                                        TopPrizes = new List<TopPrizeInfo>()
                                    };
                                    UnclaimedPrizes.Add(currentGame);
                                }

                                // Add prize level information, whether it's a new game or not
                                if (currentGame != null && int.TryParse(cells[6].InnerText.Trim(), out int unclaimed))
                                {
                                    currentGame.TopPrizes.Add(new TopPrizeInfo
                                    {
                                        PrizeLevel = cells[5].InnerText.Trim(),
                                        Unclaimed = unclaimed
                                    });
                                }
                                System.Diagnostics.Debug.WriteLine($"Parsing Game: {currentGame?.GameName ?? "New Game"}, Prize Levels: {currentGame?.TopPrizes.Count ?? 0}");
                            }
                        }

                        // Calculate winning chance for each game
                        foreach (var prize in UnclaimedPrizes)
                        {
                            if (double.TryParse(prize.PercentUnsold.Trim('%'), out double unsoldPercent) && prize.TopPrizes.Any())
                            {
                                int totalTickets = EstimateTotalTickets(prize.PricePoint);
                                double unsoldTickets = totalTickets * (unsoldPercent / 100.0);

                                // Calculation should only consider the number of unclaimed prizes, not dollar values
                                int totalUnclaimedPrizes = prize.TopPrizes.Sum(p => p.Unclaimed);
                                if (totalUnclaimedPrizes > 0)
                                {
                                    double chance = unsoldTickets / totalUnclaimedPrizes;
                                    prize.WinningChance = string.Format("{0:N2} to 1", chance);
                                }
                                else
                                {
                                    prize.WinningChance = "No Prizes Left";
                                }

                                // Display logic for TopPrizeLevel
                                prize.TopPrizeLevel = prize.TopPrizes.Count > 1 ? $"{prize.TopPrizes.Count} Levels" : "1 Level";
                            }
                            else
                            {
                                if (prize.TopPrizes == null || prize.TopPrizes.Count == 0)
                                {
                                    prize.WinningChance = "No Prizes Left";
                                    prize.TopPrizeLevel = "No Levels";
                                }
                                else
                                {
                                    prize.WinningChance = "N/A";
                                    prize.TopPrizeLevel = $"{prize.TopPrizes.Count} Levels";
                                }
                                System.Diagnostics.Debug.WriteLine($"No chance calculated for {prize.GameName}: PercentUnsold: {prize.PercentUnsold}, TopPrizes Count: {prize.TopPrizes?.Count ?? 0}");
                            }
                        }
                        OnPropertyChanged(nameof(UnclaimedPrizes));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No prize data found or table structure not as expected.");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine("HTTP Request Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("An error occurred: " + ex.Message);
            }
            System.Diagnostics.Debug.WriteLine("LoadDataAsync method completed.");
        }

        private int EstimateTotalTickets(string pricePoint)
        {
            switch (pricePoint)
            {
                case "$1.00": return 1_500_000;
                case "$2.00": return 1_000_000;
                case "$3.00": return 800_000;
                case "$5.00": return 700_000;
                case "$10.00": return 600_000;
                case "$20.00": return 500_000;
                default: return 1_000_000;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class UnclaimedPrize
    {
        public string PricePoint { get; set; }
        public string GameNumber { get; set; }
        public string GameName { get; set; }
        public string PercentUnsold { get; set; }
        public string TotalUnclaimed { get; set; }
        public List<TopPrizeInfo> TopPrizes { get; set; } = new List<TopPrizeInfo>();
        public string WinningChance { get; set; }
        public string TopPrizeLevel { get; set; } // Used to display the number of levels
    }

    public class TopPrizeInfo
    {
        public string PrizeLevel { get; set; }
        public int Unclaimed { get; set; }
    }
}