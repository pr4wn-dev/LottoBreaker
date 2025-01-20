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
                        foreach (var node in prizeNodes.Skip(1)) // Skip the header row
                        {
                            var cells = node.SelectNodes("td");
                            if (cells != null && cells.Count >= 7)
                            {
                                var pricePoint = cells[0].InnerText.Trim();
                                if (!string.IsNullOrEmpty(pricePoint) && pricePoint != " ") // New game
                                {
                                    var newGame = new UnclaimedPrize
                                    {
                                        PricePoint = pricePoint,
                                        GameNumber = cells[1].InnerText.Trim(),
                                        GameName = cells[2].InnerText.Trim(),
                                        PercentUnsold = cells[3].InnerText.Trim(),
                                        TotalUnclaimed = cells[4].InnerText.Trim(),
                                    };
                                    UnclaimedPrizes.Add(newGame);
                                }
                            }
                        }

                        // Calculate winning chance for each game
                        foreach (var prize in UnclaimedPrizes)
                        {
                            // Ensure we're always calculating a chance, even if data might be missing
                            if (double.TryParse(prize.PercentUnsold.Trim('%'), out double unsoldPercent))
                            {
                                int totalTickets = EstimateTotalTickets(prize.PricePoint);
                                double unsoldTickets = totalTickets * (unsoldPercent / 100.0);

                                // Use TryParse to handle potential parsing errors
                                if (int.TryParse(prize.TotalUnclaimed, out int totalUnclaimedPrizes) && totalUnclaimedPrizes > 0)
                                {
                                    double chance = unsoldTickets / totalUnclaimedPrizes;
                                    prize.WinningChance = string.Format("{0:N2} to 1", chance);
                                }
                                else
                                {
                                    // If TotalUnclaimed can't be parsed or is zero, we'll use a very high number to avoid division by zero
                                    // This effectively gives a very low chance of winning
                                    prize.WinningChance = string.Format("{0:N2} to 1", double.MaxValue);
                                }
                            }
                            else
                            {
                                // If PercentUnsold can't be parsed, we'll set a default value
                                prize.WinningChance = "N/A";
                                System.Diagnostics.Debug.WriteLine($"No chance calculated for {prize.GameName}: PercentUnsold: {prize.PercentUnsold}, TotalUnclaimed: {prize.TotalUnclaimed}");
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
        public string WinningChance { get; set; }
    }

    public class TopPrizeInfo
    {
        public string PrizeLevel { get; set; }
        public int Unclaimed { get; set; }
    }
}