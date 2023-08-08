using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static Hearthstone_Rank_Finder.RankLookup;
using System.Media;
using System.IO;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Policy;
using System.Collections.Concurrent;

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
            NotificationTypeCheckedListBox.Enabled = false;
        }
        private void LoadSettings()
        {
            SelectRegionComboBox.SelectedItem = Properties.Settings.Default.Region;
            SelectSeasonTextBox.Text = Properties.Settings.Default.Season;
            SelectModeComboBox.SelectedItem = Properties.Settings.Default.Mode;
            SearchPlayerTextBox.Text = Properties.Settings.Default.PlayerName;
            DiscordWebhookTextbox.Text = Properties.Settings.Default.DiscordWebhookURL;
        }
        private void SaveSettings()
        {
            Properties.Settings.Default.Region = SelectRegionComboBox.SelectedItem?.ToString() ?? "";
            Properties.Settings.Default.Season = SelectSeasonTextBox.Text.Trim();
            Properties.Settings.Default.Mode = SelectModeComboBox.SelectedItem?.ToString() ?? "";
            Properties.Settings.Default.PlayerName = SearchPlayerTextBox.Text.Trim();
            Properties.Settings.Default.DiscordWebhookURL = DiscordWebhookTextbox.Text.Trim();
            Properties.Settings.Default.Save();
        }
        private void RankLookup_Load(object sender, EventArgs e)
        {
            LoadSettings();

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.DiscordWebhookURL))
            {
                DiscordWebhookTextbox.Enabled = true;
                SetWebhookButton.Enabled = true;
                ResetWebhookButton.Enabled = true;
            }
            else
            {
                DiscordWebhookTextbox.Enabled = true;
                SetWebhookButton.Enabled = true;
                ResetWebhookButton.Enabled = true;
            }
        }
        private void RankLookup_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
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

            try
            {
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
            catch (Exception ex)
            {
                LogMessage($"An error occurred while searching for player: {ex.Message}");
                LogMessage("Retrying in 5 seconds...");
                await Task.Delay(5000); // Delay for 5 seconds before retrying
                SearchButton_Click(sender, e); // Retry the method
            }
        }
        private async void StartStalkingButton_Click_1(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(SearchPlayerTextBox.Text.Trim()))
            {
                isStalking = true;

                //SearchingProgressBar.Value = 0;

                LogMessage("Started stalking...");
                NotificationCheckbox.Enabled = false;
                NotificationTypeCheckedListBox.Enabled = false;
                StartStalkingButton.Enabled = false;
                StopSearchingButton.Enabled = false;
                SearchButton.Enabled = false;
                StopStalkingButton.Enabled = true;
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
            NotificationCheckbox.Enabled = true;
            NotificationTypeCheckedListBox.Enabled = NotificationCheckbox.Checked;

        }
        private async Task StalkPlayerRanks()
        {
            if (!isStalking) return; // Stop stalking if "Stop Stalking" button is pressed

            int previousRank = -1; // Keep track of the previous rank
            LeaderboardData leaderboardData = null; // Declare leaderboardData here
            List<(string Rank, string Link)> ranksWithLinks = new List<(string Rank, string Link)>(); // Declare the list here

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Stalk through 5 consecutive pages (before, current, and after)
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
                                    playerFoundPage = i;

                                    // Check for rank changes
                                    if (currentRank == -1)
                                    {
                                        currentRank = row.Rank;
                                    }
                                    else if (currentRank != row.Rank)
                                    {
                                        // Perform notifications only when rank changes
                                        RankChangeDetected(row.Rank, currentRank);
                                        LogMessage($"{playerName}'s Rank changed from {currentRank} to {row.Rank}");
                                        currentRank = row.Rank;
                                        LogMessage("Sleep 15000");
                                        await Task.Delay(15000);
                                    }

                                    // Delay for inner loop
                                    // Delay between rank checks based on RefreshRateNumericUpDown value
                                    int refreshRateInSeconds = (int)RefreshRateNumericUpDown.Value;
                                    LogMessage($"Delay: {refreshRateInSeconds}");
                                    await Task.Delay(refreshRateInSeconds * 1000);

                                }
                            }
                        }
                    }

                    // Update the current page for the next iteration
                    currentPage = startPage;

                    // Delay for outer loop
                    await Task.Delay(0);

                    // Recursively call the method for the next iteration
                    await StalkPlayerRanks();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"An error occurred while stalking data: {ex.Message}");
                LogMessage("Retrying in 5 seconds...");
                await Task.Delay(5000); // Delay for 5 seconds before retrying
                await StalkPlayerRanks(); // Retry the method
            }
        }
        private async Task GetPlayerRanks(string season, string region, string mode, string playerName, int currentPage)
        {
            while (isSearching)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        playerFoundPage = -1; // Reset playerFoundPage before each search

                        string url = $"https://hearthstone.blizzard.com/en-us/api/community/leaderboardsData?region={region}&leaderboardId={mode}&seasonId={season}&page={currentPage}";
                        LogMessage($"Fetching data from leaderboards... {currentPage}");

                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();

                        string jsonContent = await response.Content.ReadAsStringAsync();

                        LeaderboardData leaderboardData = JsonConvert.DeserializeObject<LeaderboardData>(jsonContent);

                        if (leaderboardData != null && leaderboardData.Leaderboard != null && leaderboardData.Leaderboard.Rows.Count > 0)
                        {
                            var playerRow = leaderboardData.Leaderboard.Rows.FirstOrDefault(row => row.AccountId == playerName);
                            if (playerRow != null)
                            {
                                playerFoundPage = currentPage; // Record the page number where the player was found
                                LogMessage($"Player {playerName} found on page {currentPage}!");
                                StopSearchingButton.Enabled = false;
                                CurrentRankRichTextBox.Text = $"Player found on page {currentPage}!\nRank: {playerRow.Rank}";
                                playerFoundSound.Play();
                                await Task.Delay(1500);
                                playerFoundTTS.Play();
                                currentRank = playerRow.Rank;
                                break; // Stop the loop once the player is found
                            }
                        }

                        int totalPages = leaderboardData?.Leaderboard?.Pagination?.TotalPages ?? 0;
                        if (currentPage >= totalPages)
                        {
                            LogMessage("Reached Last Page.");
                            break;
                        }

                        currentPage++; // Move this inside the while loop to go to the next page after processing the current one
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"An error occurred while fetching data: {ex.Message}");
                    LogMessage("Retrying after 5 seconds...");
                    await Task.Delay(5000); // Delay for 5 seconds before retrying
                }
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
        private void NotificationCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            NotificationTypeCheckedListBox.Enabled = NotificationCheckbox.Checked;
        }

        private void NotificationTypeCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void DiscordWebhookTextbox_TextChanged(object sender, EventArgs e)
        {

        }
        private async void RankChangeDetected(int newRank, int previousRank)
        {
            if (NotificationCheckbox.Checked)
            {


                // Check if the Sound notification option is selected
                if (NotificationTypeCheckedListBox.GetItemChecked(0))
                {
                    // Play sound notification
                    rankChangedTTS.Play();
                }
                // Check if the Discord notification option is selected
                if (NotificationTypeCheckedListBox.GetItemChecked(2))
                {
                    // Check if a webhook URL is set
                    if (!string.IsNullOrWhiteSpace(DiscordWebhookTextbox.Text))
                    {
                        // Call the SendDiscordNotification method with the webhook URL, new rank, and previous rank
                        await SendDiscordNotification(DiscordWebhookTextbox.Text, newRank, previousRank);
                    }
                    else
                    {
                        LogMessage("Discord webhook URL is not set.");
                    }
                }
                // Check if Popup notification is selected
                if (NotificationTypeCheckedListBox.GetItemChecked(1))
                {
                    // Show Popup notification
                    ShowPopupNotification(newRank, previousRank);
                }
            }
            else
            {
                return;
            }
        }
        private void ShowPopupNotification(int newRank, int previousRank)
        {
            MessageBox.Show($"Rank change detected!\nPrevious Rank: {previousRank}\nCurrent Rank: {newRank}",
                "Rank Change Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private async Task SendDiscordNotification(string webhookUrl, int newRank, int previousRank)
        {
            int maxRetries = 3;
            int retryDelayInSeconds = 5;

            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        // Create a JSON payload for the Discord webhook
                        string jsonPayload = $"{{\"content\": \"{playerName}'s rank changed from {previousRank} to {newRank}!\"}}";

                        // Create a StringContent object with the JSON payload and content type
                        StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        // Send a POST request to the webhook URL
                        HttpResponseMessage response = await client.PostAsync(webhookUrl, content);

                        // Check if the response indicates success
                        if (response.IsSuccessStatusCode)
                        {
                            // Notification sent successfully
                            LogMessage("Discord notification sent successfully.");
                            break; // Break out of the retry loop on success
                        }
                        else
                        {
                            // Failed to send notification, log the status code
                            LogMessage($"Failed to send Discord notification. Status code: {response.StatusCode}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions and log an error message
                    Console.WriteLine($"An error occurred while sending Discord notification: {ex.Message}");
                }

                // Delay before retrying
                await Task.Delay(retryDelayInSeconds * 1000);
            }
        }


        private void SetWebhookButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(DiscordWebhookTextbox.Text))
            {
                Properties.Settings.Default.DiscordWebhookURL = DiscordWebhookTextbox.Text;
                Properties.Settings.Default.Save();
                DiscordWebhookTextbox.Enabled = false;
                SetWebhookButton.Enabled = false;
                ResetWebhookButton.Enabled = true;
            }
            else
            {
                MessageBox.Show("Please enter a valid webhook URL.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetWebhookButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DiscordWebhookURL = "";
            Properties.Settings.Default.Save();
            DiscordWebhookTextbox.Enabled = true;
            SetWebhookButton.Enabled = true;
            ResetWebhookButton.Enabled = false;
            DiscordWebhookTextbox.Text = "";
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

        private void RankLookup_Load_1(object sender, EventArgs e)
        {

        }


    }
}