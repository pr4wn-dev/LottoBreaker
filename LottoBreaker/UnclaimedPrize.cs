using System;

namespace LottoBreaker.Models // Adjust the namespace as per your project structure
{
    public class UnclaimedPrize
    {
        public string PricePoint { get; set; }
        public string GameNumber { get; set; }
        public string GameName { get; set; }
        public string PercentUnsold { get; set; }
        public string TotalUnclaimed { get; set; }
        public List<TopPrizeInfo> TopPrizes { get; set; } = new List<TopPrizeInfo>();
        public string WinningChance { get; set; }
    }

    public class TopPrizeInfo
    {
        public string PrizeLevel { get; set; }
        public int Unclaimed { get; set; }
    }
}