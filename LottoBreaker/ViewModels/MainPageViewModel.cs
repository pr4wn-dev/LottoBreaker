using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Linq;
using System.Windows.Input;

namespace LottoBreaker.ViewModels
{
    public class MainPageViewModel
    {
        public ObservableCollection<string> UnclaimedPrizes { get; set; } = new ObservableCollection<string>();

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
                        foreach (var node in prizeNodes)
                        {
                            var cells = node.SelectNodes("td"); // This can return null if there are no td elements
                            if (cells != null)
                            {
                                // Joining cells with a separator, but check if cells has elements
                                var prizeInfo = cells.Any() ? string.Join(" - ", cells.Select(c => c.InnerText.Trim())) : string.Empty;
                                if (!string.IsNullOrEmpty(prizeInfo))
                                {
                                    UnclaimedPrizes.Add(prizeInfo);
                                    System.Diagnostics.Debug.WriteLine("Added prize: " + prizeInfo);
                                }
                            }
                        }
                        System.Diagnostics.Debug.WriteLine("Number of unclaimed prizes added: " + UnclaimedPrizes.Count);
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
}