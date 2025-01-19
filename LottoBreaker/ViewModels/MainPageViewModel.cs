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
                        foreach (var node in prizeNodes.Skip(1)) // Skip the header row
                        {
                            var cells = node.SelectNodes("td");
                            if (cells != null && cells.Count >= 7) // Ensure we have at least 7 cells to match all fields
                            {
                                UnclaimedPrizes.Add(new UnclaimedPrize
                                {
                                    PricePoint = cells[0].InnerText.Trim(),
                                    GameNumber = cells[1].InnerText.Trim(),
                                    GameName = cells[2].InnerText.Trim(),
                                    PercentUnsold = cells[3].InnerText.Trim(),
                                    TotalUnclaimed = cells[4].InnerText.Trim(),
                                    TopPrizeLevel = cells[5].InnerText.Trim(),
                                    TopPrizeUnclaimed = cells[6].InnerText.Trim()
                                });
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}