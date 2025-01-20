using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Linq;
using System.Windows.Input;
using LottoBreaker.Models;
using System.ComponentModel;

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

                    // Define prizeNodes here
                    var prizeNodes = doc.DocumentNode.SelectNodes("//table[@class='tbstriped']/tr"); // Adjust this XPath if necessary
                    if (prizeNodes != null)
                    {
                        UnclaimedPrizes.Clear();
                        UnclaimedPrize currentGame = null;
                        foreach (var node in prizeNodes.Skip(1))
                        {

                            var cells = node.SelectNodes("td");
                            System.Diagnostics.Debug.WriteLine($"Parsing Game: {cells[2].InnerText.Trim()}, Prize Levels: {currentGame?.TopPrizes?.Count ?? 0}");

                            if (cells != null && cells.Count >= 7)
                            {
                                var pricePoint = cells[0].InnerText.Trim();
                                // Ensure a new game object is created for each new game
                                if (!string.IsNullOrEmpty(pricePoint) && pricePoint != " ")
                                {
                                    currentGame = new UnclaimedPrize
                                    {
                                        PricePoint = pricePoint,
                                        GameNumber = cells[1].InnerText.Trim(),
                                        GameName = cells[2].InnerText.Trim(),
                                        PercentUnsold = cells[3].InnerText.Trim(),
                                        TotalUnclaimed = cells[4].InnerText.Trim(),
                                        TopPrizes = new List<TopPrizeInfo>() // Always initialize this
                                    };
                                    UnclaimedPrizes.Add(currentGame);
                                }

                                // Add top prize info
                                if (currentGame != null && int.TryParse(cells[6].InnerText.Trim(), out int unclaimed))
                                {
                                    currentGame.TopPrizes.Add(new TopPrizeInfo { PrizeLevel = cells[5].InnerText.Trim(), Unclaimed = unclaimed });
                                }
                            }
                        }

                        // Calculate winning chance for each game
                        foreach (var prize in UnclaimedPrizes)
                        {
                            System.Diagnostics.Debug.WriteLine($"Game: {prize.GameName}, Prize Levels: {prize.TopPrizes.Count}");

                            if (double.TryParse(prize.PercentUnsold.Trim('%'), out double unsoldPercent) && prize.TopPrizes.Any())
                            {
                                int totalTickets = EstimateTotalTickets(prize.PricePoint);
                                double unsoldTickets = totalTickets * (unsoldPercent / 100.0);

                                // Sum up all unclaimed prizes for a total chance calculation
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
                            }
                            if (prize.TopPrizes == null || prize.TopPrizes.Count == 0)
                            {
                                prize.WinningChance = "No Prizes Left";
                            }
                            else
                            {
                                prize.WinningChance = "N/A";
                                System.Diagnostics.Debug.WriteLine($"No chance calculated for {prize.GameName}: PercentUnsold: {prize.PercentUnsold}, TopPrizes Count: {prize.TopPrizes.Count}");
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
            // This is an estimation based on Maine's typical ticket production numbers
            switch (pricePoint)
            {
                case "$1.00": return 1_500_000; // Lower priced games might have more tickets
                case "$2.00": return 1_000_000;
                case "$3.00": return 800_000;
                case "$5.00": return 700_000;
                case "$10.00": return 600_000;
                case "$20.00": return 500_000;
                default: return 1_000_000; // Default case for unknown price points
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }

}