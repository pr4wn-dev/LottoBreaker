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
                        // Clear the collection before repopulating
                        //TicketGame.Clear();

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
                                                    TotalUnclaimedTopPrizes = "0"  // Initialize to zero for a new game
                                                };
                                                TicketGame.Add(currentGame);
                                            }
                                        }

                                        // Parse unclaimed prizes for this level of the game
                                        if (int.TryParse(unclaimedPrizesStr, out int unclaimedPrizes))
                                        {
                                            int currentTotal = int.Parse(currentGame.TotalUnclaimedTopPrizes);
                                            currentGame.TotalUnclaimedTopPrizes = (currentTotal + unclaimedPrizes).ToString();
                                            System.Diagnostics.Debug.WriteLine($"Parsed {unclaimedPrizesStr} to {unclaimedPrizes} for {currentGame.GameName}, Total: {currentGame.TotalUnclaimedTopPrizes}");
                                        }
                                        else
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Failed to parse {unclaimedPrizesStr} for {currentGame.GameName}");
                                        }
                                    }
                                }
                            }
                        }

                        // Ensure all data operations are complete before notifying UI
                        await Task.Run(() =>
                        {
                            foreach (var game in TicketGame)
                            {
                                if (string.IsNullOrEmpty(game.TotalUnclaimedTopPrizes))
                                {
                                    game.TotalUnclaimedTopPrizes = "0"; // Default to 0 if parsing fails
                                }
                            }
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
    }

    public class TicketGame
    {
        public string PricePoint { get; set; }
        public string GameNumber { get; set; }
        public string GameName { get; set; }
        public string PercentUnsold { get; set; }
        public string TotalUnclaimedTopPrizes { get; set; }
        public string WinningChance { get; set; }
    }
    public class TopPrizeInfo
    {
        public string PrizeLevel { get; set; }
        public int Unclaimed { get; set; }
    }
}