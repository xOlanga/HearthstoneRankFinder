using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static Hearthstone_Rank_Finder.RankLookup;
using System.Media;
using System.IO;

namespace Hearthstone_Rank_Finder
{
    public partial class RankLookup : Form
    {
        private int playerFoundPage = -1; // Initialize to an invalid page number
        private int currentRank = -1; // Initialize to an invalid rank
        private string mode = "Standard";
        private string region = "Europe";
        private string season;
        private string playerName;
        private int currentPage;
        private bool isStalking = false;
        private bool isSearching = false;
        private SoundPlayer playerFoundSound;
        private SoundPlayer playerFoundTTS;
        private SoundPlayer rankChangedTTS;

        public RankLookup()
        {
            InitializeComponent();
            // Add the event handler for the TextChanged event of CurrentRankRichTextBox
            playerFoundSound = new SoundPlayer(Properties.Resources.triple_ping);
            playerFoundTTS = new SoundPlayer(Properties.Resources.playerfoundtts);
            rankChangedTTS = new SoundPlayer(Properties.Resources.RankChangedTTS);
        }

        private async void SearchButton_Click(object sender, EventArgs e)
        {


            if (string.IsNullOrWhiteSpace(SearchPlayerTextBox.Text) ||
                SelectModeComboBox.SelectedItem == null ||
                SelectRegionComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(SelectSeasonTextBox.Text))
            {
                MessageBox.Show("Please fill out all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Exit the method if fields are not filled out
            }
            currentPage = 1; // Reset the current page to 1
            isSearching = true;
            StopSearchingButton.Enabled = true; // Enable the "Stop Searching" button
            StartStalkingButton.Enabled = false;
            SearchButton.Enabled = false;
            season = SelectSeasonTextBox.Text.Trim();
            region = GetRegionCode(SelectRegionComboBox.SelectedItem.ToString());
            mode = GetModeCode(SelectModeComboBox.SelectedItem.ToString());
            playerName = SearchPlayerTextBox.Text.Trim();
            // Clear textboxes before each search
            CurrentRankRichTextBox.Text = "";
            LogRichTextbox.Text = "";
            // Call the GetPlayerRanks method or start the searching process
            // Perform web scraping
            await GetPlayerRanks(season, region, mode, playerName, currentPage);
            // If a player was found, enable the "Start Stalking" button
            if (playerFoundPage > 0)
            {
                StartStalkingButton.Enabled = true;
            }
            else
            {
                StartStalkingButton.Enabled = false;
            }
        }
        private async void StartStalkingButton_Click_1(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(SearchPlayerTextBox.Text.Trim()))
            {
                isStalking = true;

                //SearchingProgressBar.Value = 0;

                LogMessage("Started stalking...");
                StartStalkingButton.Enabled = false;
                StopSearchingButton.Enabled = false;
                SearchButton.Enabled = false;
                // Start stalking indefinitely
                await StalkPlayerRanks();

            }
            else
            {
                MessageBox.Show("Please enter a player name to start stalking.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartStalkingButton_Click(object sender, EventArgs e)
        {

        }
        private async void StalkingTimer_Tick(object sender, EventArgs e)
        {

        }
        private void StopStalkingButton_Click(object sender, EventArgs e)
        {
            isStalking = false;
            StalkingTimer.Stop(); // Stop the timer if you were using it
            LogMessage($"Stalking stopped by user.");
            StartStalkingButton.Enabled = true;
            SearchButton.Enabled = true;
            StopStalkingButton.Enabled = false;
        }
        private async Task StalkPlayerRanks()
        {
            if (!isStalking) return; // Stop stalking if "Stop Stalking" button is pressed

            List<(string Rank, string Link)> ranksWithLinks = new List<(string Rank, string Link)>();
            LeaderboardData leaderboardData = null; // Declare leaderboardData here

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Stalk through three consecutive pages (before, current, and after)
                    int startPage = playerFoundPage; // Set the startPage to playerFoundPage

                    for (int i = startPage - 2; i <= startPage + 2; i++)
                    {
                        if (i <= 0) continue; // Skip if page number is less than or equal to 0

                        string url = $"https://hearthstone.blizzard.com/en-us/api/community/leaderboardsData?region={region}&leaderboardId={mode}&seasonId={season}&page={i}";


                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();

                        string jsonContent = await response.Content.ReadAsStringAsync();

                        leaderboardData = JsonConvert.DeserializeObject<LeaderboardData>(jsonContent);

                        if (leaderboardData != null && leaderboardData.Leaderboard != null && leaderboardData.Leaderboard.Rows.Count > 0)
                        {
                            foreach (var row in leaderboardData.Leaderboard.Rows)
                            {
                                if (row.AccountId == playerName)
                                {
                                    ranksWithLinks.Add((row.Rank.ToString(), url));
                                    CurrentRankRichTextBox.Text = $"Stalking {playerName}...\nCurrent Rank: {row.Rank}";
                                    LogMessage($"Checking for rank changes...");
                                    // Print the current page number where the player was found
                                    //LogMessage($"Player found on page {i}!");
                                    playerFoundPage = i;

                                    // Check for rank changes
                                    if (currentRank == -1)
                                    {
                                        currentRank = row.Rank;
                                    }
                                    else if (currentRank != row.Rank)
                                    {
                                        rankChangedTTS.Play();
                                        Thread.Sleep(500);
                                        LogMessage($"Players Rank changed from {currentRank} to {row.Rank}");
                                        MessageBox.Show($"Rank change detected!\nPrevious Rank: {currentRank}\nCurrent Rank: {row.Rank}",
                                            "Rank Change Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        currentRank = row.Rank;
                                        Thread.Sleep(3000);
                                    }
                                    if (isStalking)
                                    {
                                        StopStalkingButton.Enabled = true;
                                    }
                                    else
                                    {
                                        StopStalkingButton.Enabled = false;
                                    }

                                    // Delay for a few seconds before starting the next iteration
                                    await Task.Delay(0);
                                }
                            }
                        }
                    }

                    // Update the current page for the next iteration
                    currentPage = startPage;

                    // Delay for 10 seconds before starting the next iteration
                    await Task.Delay(0);

                    // Recursively call the method for the next iteration
                    await StalkPlayerRanks();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"An error occurred while stalking data: {ex.Message}");
            }
        }
        private async Task GetPlayerRanks(string season, string region, string mode, string playerName, int currentPage)
        {

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    playerFoundPage = -1; // Reset playerFoundPage before each search

                    // Loop through the pages until the player is found or all pages are fetched
                    while (true)
                    {
                        if (!isSearching) return;


                        string url = $"https://hearthstone.blizzard.com/en-us/api/community/leaderboardsData?region={region}&leaderboardId={mode}&seasonId={season}&page={currentPage}";
                        LogMessage($"Fetching data from leaderboards... {currentPage}");

                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();

                        string jsonContent = await response.Content.ReadAsStringAsync();

                        LeaderboardData leaderboardData = JsonConvert.DeserializeObject<LeaderboardData>(jsonContent);

                        if (leaderboardData != null && leaderboardData.Leaderboard != null && leaderboardData.Leaderboard.Rows.Count > 0)
                        {
                            foreach (var row in leaderboardData.Leaderboard.Rows)
                            {
                                if (row.AccountId == playerName)
                                {
                                    playerFoundPage = currentPage; // Record the page number where the player was found
                                    LogMessage($"Player {playerName} found on page {currentPage}!");
                                    StopSearchingButton.Enabled = false;
                                    CurrentRankRichTextBox.Text = $"Player found on page {currentPage}!\nRank: {row.Rank}";
                                    playerFoundSound.Play();
                                    Thread.Sleep(1500);
                                    playerFoundTTS.Play();
                                    currentRank = row.Rank;
                                    return; // Stop the loop once the player is found
                                }
                            }
                        }

                        int totalPages = leaderboardData?.Leaderboard?.Pagination?.TotalPages ?? 0;
                        if (currentPage >= totalPages)
                        {
                            LogMessage("Reached Last Page.");
                            //SearchingProgressBar.Value = 100;
                            break;
                        }

                        currentPage++; // Move this inside the while loop to go to the next page after processing the current one

                        // Recursively call the method for the next iteration

                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"An error occurred while fetching data: {ex.Message}");
            }

            // If playerFoundPage is still -1, it means the player was not found
            if (playerFoundPage == -1)
            {
                CurrentRankRichTextBox.Text = "Player not found.";
            }
        }
        private void CurrentRankRichTextBox_TextChanged(object sender, EventArgs e)
        {

        }
        private void StopSearchingButton_Click(object sender, EventArgs e)
        {

            isSearching = false; // Set the flag to stop the search
            StopSearchingButton.Enabled = false; // Disable the button
            LogMessage("Search stopped by user.");
            SearchButton.Enabled = true;
        }

        private string GetRegionCode(string region)
        {
            switch (region)
            {
                case "Europe":
                    return "EU";
                case "Americas":
                    return "US";
                case "Asia-Pacific":
                    return "AP";
                default:
                    return "";
            }
        }

        private string GetModeCode(string mode)
        {
            switch (mode)
            {
                case "Standard":
                    return "standard";
                case "Wild":
                    return "wild";
                case "Twist":
                    return "twist";
                case "Arena":
                    return "arena";
                case "Battlegrounds":
                    return "battlegrounds";
                default:
                    return "";
            }
        }

        private void LogMessage(string message)
        {
            LogRichTextbox.AppendText(message + Environment.NewLine);
        }

        // Define classes to represent the JSON data structure
        public class LeaderboardData
        {
            [JsonProperty("seasonId")]
            public int SeasonId { get; set; }

            [JsonProperty("leaderboard")]
            public Leaderboard Leaderboard { get; set; }
        }

        public class Leaderboard
        {
            [JsonProperty("rows")]
            public List<LeaderboardRow> Rows { get; set; }

            [JsonProperty("columns")]
            public List<string> Columns { get; set; }

            [JsonProperty("pagination")]
            public Pagination Pagination { get; set; }
        }

        public class LeaderboardRow
        {
            [JsonProperty("rank")]
            public int Rank { get; set; }

            [JsonProperty("accountid")]
            public string AccountId { get; set; }

            // Add other properties if required...
        }

        public class Pagination
        {
            [JsonProperty("totalPages")]
            public int TotalPages { get; set; }

            [JsonProperty("totalSize")]
            public int TotalSize { get; set; }
        }


    }
}