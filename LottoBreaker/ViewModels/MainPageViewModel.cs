using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Windows.Input;

namespace LottoBreaker.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TicketGame> _ticketGame = new ObservableCollection<TicketGame>();
        public ObservableCollection<TicketGame> TicketGame
        {
            get => _ticketGame;
            set
            {
                if (_ticketGame != value)
                {
                    _ticketGame = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand LoadDataCommand { get; }

        public MainPageViewModel()
        {
            LoadDataCommand = new Command(async () => await LoadDataAsync());
            System.Diagnostics.Debug.WriteLine("Initial TicketGame count: " + TicketGame.Count);
        }

        public async Task LoadDataAsync()
        {
            System.Diagnostics.Debug.WriteLine("LoadDataAsync method started.");
            System.Diagnostics.Debug.WriteLine("Before Clear: TicketGame count: " + TicketGame.Count);
            TicketGame.Clear();
            System.Diagnostics.Debug.WriteLine("After Clear: TicketGame count: " + TicketGame.Count);

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

                    var tables = doc.DocumentNode.SelectNodes("//table[@class='tbstriped']");
                    if (tables != null && tables.Count > 0)
                    {
                        var allGames = new ObservableCollection<TicketGame>();

                        foreach (var table in tables)
                        {
                            var prizeNodes = table.SelectNodes("tr");
                            if (prizeNodes != null)
                            {
                                TicketGame currentGame = null;

                                foreach (var node in prizeNodes.Skip(1)) // Skip the header row
                                {
                                    var cells = node.SelectNodes("td");
                                    if (cells != null && cells.Count >= 7)
                                    {
                                        var pricePoint = cells[0].InnerText.Trim();
                                        var gameName = cells[2].InnerText.Trim();
                                        var percentUnsold = cells[3].InnerText.Trim();
                                        var unclaimedPrizesStr = cells[6].InnerText.Trim();

                                        if (!string.IsNullOrEmpty(pricePoint) && pricePoint != " ")
                                        {
                                            // Start of a new game or continue if game name is the same
                                            if (currentGame == null || currentGame.GameName != gameName)
                                            {
                                                currentGame = new TicketGame
                                                {
                                                    PricePoint = pricePoint,
                                                    GameNumber = cells[1].InnerText.Trim(),
                                                    GameName = gameName,
                                                    PercentUnsold = percentUnsold,
                                                    TotalUnclaimedTopPrizes = "",
                                                    UnclaimedPrizes = new ObservableCollection<string>()
                                                };
                                                allGames.Add(currentGame);
                                            }
                                        }

                                        // Collect unclaimed prizes as strings
                                        currentGame.UnclaimedPrizes.Add(unclaimedPrizesStr);
                                    }
                                }
                            }
                        }

                        // Convert all unclaimed prizes after collecting all data
                        foreach (var game in allGames)
                        {
                            int totalUnclaimed = 0;
                            foreach (var unclaimedPrize in game.UnclaimedPrizes)
                            {
                                if (int.TryParse(unclaimedPrize, out int unclaimed))
                                {
                                    totalUnclaimed += unclaimed;
                                    System.Diagnostics.Debug.WriteLine($"Parsed {unclaimedPrize} to {unclaimed} for {game.GameName}, Total: {totalUnclaimed}");
                                }
                                else
                                {
                                    string cleanedValue = unclaimedPrize.Trim().Replace(",", "");
                                    if (int.TryParse(cleanedValue, out int parsedValue))
                                    {
                                        totalUnclaimed += parsedValue;
                                        System.Diagnostics.Debug.WriteLine($"Parsed after cleaning {unclaimedPrize} to {parsedValue} for {game.GameName}, Total: {totalUnclaimed}");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Failed to parse {unclaimedPrize} for {game.GameName}");
                                    }
                                }
                            }
                            game.TotalUnclaimedTopPrizes = totalUnclaimed.ToString();

                            // Calculate tickets made and winning chance
                            if (double.TryParse(game.PricePoint.TrimStart('$'), out double price) &&
                                double.TryParse(game.PercentUnsold.TrimEnd('%'), out double unsoldPercent))
                            {
                                long ticketsMade = CalculateTicketsMade(price);
                                long ticketsLeft = (long)(ticketsMade * (unsoldPercent / 100.0));

                                if (totalUnclaimed > 0 && ticketsLeft > 0)
                                {
                                    double winningChance = (double)totalUnclaimed / ticketsLeft;
                                    game.WinningChance = (winningChance * 100).ToString("F2") + "%";
                                }
                                else
                                {
                                    game.WinningChance = "0.00%"; // If no top prizes or no tickets left
                                }
                                System.Diagnostics.Debug.WriteLine($"For {game.GameName}, Tickets Made: {ticketsMade}, Tickets Left: {ticketsLeft}, Winning Chance: {game.WinningChance}");
                            }
                            else
                            {
                                game.WinningChance = "Data Error";
                                System.Diagnostics.Debug.WriteLine($"Data error for {game.GameName}");
                            }
                        }

                        TicketGame = allGames;

                        // Ensure all data operations are complete before notifying UI
                        await Task.Run(() =>
                        {
                            System.Diagnostics.Debug.WriteLine("After Loading: TicketGame count: " + TicketGame.Count);
                            foreach (var game in TicketGame)
                            {
                                System.Diagnostics.Debug.WriteLine($"Game: {game.GameName}, TotalUnclaimedTopPrizes: {game.TotalUnclaimedTopPrizes}");
                            }
                        });

                        // Notify UI of changes on the main thread
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            OnPropertyChanged(nameof(TicketGame));
                        });
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

        // Implement this method according to your logic for ticket production
        private long CalculateTicketsMade(double pricePoint)
        {
            switch (pricePoint)
            {
                case 1.00:
                    return 1_500_000; // Lower priced games might have more tickets
                case 2.00:
                    return 1_000_000;
                case 3.00:
                    return 800_000;
                case 5.00:
                    return 700_000;
                case 10.00:
                    return 600_000;
                case 20.00:
                    return 500_000;
                default:
                    return 1_000_000; // Default case for unknown price points
            }
        }
    }

    public class TicketGame
    {
        public string PricePoint { get; set; }
        public string GameNumber { get; set; }
        public string GameName { get; set; }
        public string PercentUnsold { get; set; }
        public string TotalUnclaimedTopPrizes { get; set; }
        public string WinningChance { get; set; }
        public ObservableCollection<string> UnclaimedPrizes { get; set; } = new ObservableCollection<string>();
    }

    public class TopPrizeInfo
    {
        public string PrizeLevel { get; set; }
        public int Unclaimed { get; set; }
    }
}