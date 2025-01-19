using System;

namespace LottoBreaker.Models // Adjust the namespace as per your project structure
{
    public class UnclaimedPrize
    {
        // Based on the data structure from the API, you might need to adjust these properties
        public string draw_date { get; set; }
        public string game { get; set; }
        public string prize { get; set; }
        public string value { get; set; }
        public string location { get; set; }
        public string claim_deadline { get; set; }

        // Add or remove properties as needed to match the JSON structure
    }
}