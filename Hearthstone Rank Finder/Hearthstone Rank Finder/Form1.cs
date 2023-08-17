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
        #region Initialization and Configuration

        // Fields to store various settings and data
        private int playerFoundPage = -1;
        private int currentRank = -1;
        private string mode = "Standard";
        private string region = "Europe";
        private string? season;
        private string? playerName;
        private int currentPage;
        private bool isStalking = false;
        private bool isSearching = false;
        private SoundPlayer playerFoundSound;
        private SoundPlayer playerFoundTTS;
        private SoundPlayer rankChangedTTS;
        private int NumberOfPackages = 20;
        private LeaderboardData? leaderboardData;
        private bool playerFound = false;
        private int lastRankChangePage = -1;
        private int lastFoundPage = -1;

        public RankLookup()
        {
            InitializeComponent();

            // Initialize sound players for various notifications
            playerFoundSound = new SoundPlayer(Properties.Resources.triple_ping);
            playerFoundTTS = new SoundPlayer(Properties.Resources.playerfoundtts);
            rankChangedTTS = new SoundPlayer(Properties.Resources.RankChangedTTS);

            // Disable notification types checkbox list until needed
            NotificationTypeCheckedListBox.Enabled = false;

            // Set minimum and maximum values for numeric up-down controls
            StalkingRangeNumericUpdown.Minimum = 1;
            StalkingRangeNumericUpdown.Maximum = 10000;
            season = "69";
        }
        #endregion

        #region Player Search Section
        private async void SearchButton_Click(object sender, EventArgs e)
        {
            // Check if required fields are filled out
            if (string.IsNullOrWhiteSpace(SearchPlayerTextBox.Text) ||
                SelectModeComboBox.SelectedItem == null ||
                SelectRegionComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(SelectSeasonTextBox.Text))
            {
                // Display a validation error message
                MessageBox.Show("Please fill out all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {

                currentPage = 1; // Initialize current page
                isSearching = true; // Set searching flag to true
                StopSearchingButton.Enabled = true; // Enable stop searching button
                StartStalkingButton.Enabled = false; // Disable start stalking button
                SearchButton.Enabled = false; // Disable search button
                StopStalkingButton.Enabled = false;
                season = SelectSeasonTextBox.Text.Trim();
                if (SelectRegionComboBox.SelectedItem != null)
                {
                    region = GetRegionCode(SelectRegionComboBox.SelectedItem.ToString());
                }
                else
                {
                    // Handle the case where selected region is null
                    throw new InvalidOperationException("Selected region cannot be null.");
                }

                if (SelectModeComboBox.SelectedItem != null)
                {
                    mode = GetModeCode(SelectModeComboBox.SelectedItem.ToString());
                }
                else
                {
                    // Handle the case where selected mode is null
                    throw new InvalidOperationException("Selected mode cannot be null.");
                }
                playerName = SearchPlayerTextBox.Text.Trim();
                CurrentRankRichTextBox.Text = ""; // Clear current rank display
                LogRichTextbox.Text = ""; // Clear log display
                LogMessage("Getting ready...");
                // Call the GetTotalPages method to fetch the total number of pages
                await GetTotalPages(season, region, mode);

                int totalPages = leaderboardData?.Leaderboard?.Pagination?.TotalPages ?? 0;
                int pagesPerPackage = totalPages / NumberOfPackages;
                int remainingPages = totalPages % NumberOfPackages;

                List<Task> searchTasks = new List<Task>();

                // Iterate through each page and fetch player ranks
                for (int page = 1; page <= totalPages; page++)
                {
                    searchTasks.Add(GetPlayerRanksInPackage(season, region, mode, playerName, page, page));
                }

                // Wait for all page requests to complete
                await Task.WhenAll(searchTasks);

                // Enable or disable the "Start Stalking" button based on player found status
                if (playerFound)
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
                await Task.Delay(5000);
                SearchButton_Click(sender, e); // Retry the method
            }
        }

        private async Task GetTotalPages(string season, string region, string mode)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Perform a null check on the 'region' argument
                    if (region == null)
                    {
                        // Handle the case where 'region' is null
                        throw new ArgumentNullException(nameof(region), "Region argument cannot be null.");
                    }
                    // Construct the URL to fetch total pages information
                    string url = $"https://hearthstone.blizzard.com/en-us/api/community/leaderboardsData?region={region}&leaderboardId={mode}&seasonId={season}&page=1";

                    // Send a GET request to the API
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    // Read the JSON content of the response
                    string jsonContent = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON into leaderboardData object if jsonContent is not null
                    if (!string.IsNullOrWhiteSpace(jsonContent))
                    {
                        leaderboardData = JsonConvert.DeserializeObject<LeaderboardData>(jsonContent);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching data: {ex.Message}");
            }
        }

        private async Task GetPlayerRanksInPackage(string season, string region, string mode, string playerName, int startPage, int endPage)
        {
            for (int currentPage = startPage; currentPage <= endPage; currentPage++)
            {
                // Call the GetPlayerRanks method or start the searching process
                await GetPlayerRanks(season, region, mode, playerName, currentPage);

                // If player is found, stop searching
                if (playerFound)
                {
                    isSearching = false;
                    return;
                }
            }
        }

        private async Task<bool> GetPlayerRanks(string season, string region, string mode, string playerName, int currentPage)
        {
            while (isSearching) // While the application is still searching for the player...
            {
                try
                {
                    using (HttpClient client = new HttpClient()) // Create an HttpClient for making HTTP requests.
                    {
                        playerFoundPage = -1; // Initialize the playerFoundPage to -1.

                        string url = $"https://hearthstone.blizzard.com/en-us/api/community/leaderboardsData?region={region}&leaderboardId={mode}&seasonId={season}&page={currentPage}";
                        LogMessage($"Scanning leaderboards... {currentPage}"); // Log that the application is scanning leaderboards for the specified page.

                        HttpResponseMessage response = await client.GetAsync(url); // Send an HTTP GET request to the specified URL.
                        response.EnsureSuccessStatusCode(); // Ensure that the response is successful.

                        string jsonContent = await response.Content.ReadAsStringAsync(); // Read the response content as a string.

                        LeaderboardData leaderboardData = JsonConvert.DeserializeObject<LeaderboardData>(jsonContent); // Deserialize the JSON data.

                        if (leaderboardData != null && leaderboardData.Leaderboard != null && leaderboardData.Leaderboard.Rows.Count > 0)
                        {
                            // Check if the current page's leaderboard data contains the target player.
                            var playerRow = leaderboardData.Leaderboard.Rows.FirstOrDefault(row => row.AccountId == playerName);
                            if (playerRow != null)
                            {
                                playerFoundPage = currentPage; // Record the page number where the player was found.
                                LogMessage($"Player {playerName} found on page {currentPage}!");
                                StopSearchingButton.Enabled = false; // Disable the "Stop Searching" button.
                                playerFound = true; // Set playerFound flag to true.
                                playerFoundPage = currentPage; // Record the playerFoundPage.
                                CurrentRankRichTextBox.Text = $"Player found on page {currentPage}!\nRank: {playerRow.Rank}"; // Update the UI.
                                //playerFoundSound.Play(); // Play the player found sound.
                                currentRank = playerRow.Rank; // Set the currentRank to the player's rank.
                                return true; // Return true to indicate player was found.
                            }
                        }

                        currentPage++; // Move to the next page.
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"An error occurred while fetching data: {ex.Message}");
                    LogMessage("Retrying after 5 seconds...");
                    await Task.Delay(5000); // Delay before retrying.
                }
            }

            return false; // Return false if the player was not found while searching.
        }

        private void SearchPlayerTextBox_TextChanged(object sender, EventArgs e)
        {
            // Enable the "Search" button when the search player text changes
            SearchButton.Enabled = true;
        }

        private void StopSearchingButton_Click(object sender, EventArgs e)
        {
            // Set the flag to stop searching
            isSearching = false;

            // Disable the "Stop Searching" button
            StopSearchingButton.Enabled = false;

            // Log a message indicating the search has been stopped by the user
            LogMessage("Search stopped by user.");

            // Enable the "Search" button
            SearchButton.Enabled = true;
        }
        #endregion

        #region Rank Stalking Section
        private async void StartStalkingButton_Click_1(object sender, EventArgs e)
        {
            // Check if player name is entered
            if (!string.IsNullOrEmpty(SearchPlayerTextBox.Text.Trim()))
            {
                isStalking = true; // Set stalking flag to true

                LogMessage("Started stalking...");
                NotificationCheckbox.Enabled = false; // Disable notification checkbox
                NotificationTypeCheckedListBox.Enabled = false; // Disable notification type list box
                StartStalkingButton.Enabled = false; // Disable start stalking button
                StopSearchingButton.Enabled = false; // Disable stop searching button
                SearchButton.Enabled = false; // Disable search button
                StopStalkingButton.Enabled = true; // Enable stop stalking button
                await StalkPlayerRanks(); // Start stalking player ranks
            }
            else
            {
                // Display error message if player name is missing
                MessageBox.Show("Please enter a player name to start stalking.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task StalkPlayerRanks()
        {
            if (!isStalking) return; // If stalking is disabled, stop the process.

            int previousRank = -1; // Initialize the previous rank to -1.
            List<Task> pageRequests = new List<Task>(); // List to store tasks for fetching page data.

            try
            {
                using (HttpClient client = new HttpClient()) // Create an HttpClient for making HTTP requests.
                {
                    int startPage = playerFoundPage; // Start fetching data from the page where the player was initially found.
                    int stalkingRange = (int)StalkingRangeNumericUpdown.Value; // Get the stalking range from the UI.

                    // Iterate over a range of pages surrounding the startPage.
                    for (int i = startPage - stalkingRange; i <= startPage + stalkingRange; i++)
                    {
                        if (i <= 0) continue; // Skip pages with invalid index.

                        string url = $"https://hearthstone.blizzard.com/en-us/api/community/leaderboardsData?region={region}&leaderboardId={mode}&seasonId={season}&page={i}";

                        if (i == startPage - stalkingRange)
                        {
                            LogMessage($"Scanning pages {i} to {startPage + stalkingRange}...");
                        }

                        // Add an asynchronous task to the pageRequests list for fetching and processing data for the given page.
                        pageRequests.Add(GetPageDataAsync(client, url, i, previousRank));
                    }

                    await Task.WhenAll(pageRequests); // Wait for all page fetching tasks to complete.

                    if (lastRankChangePage != -1)
                    {
                        playerFoundPage = lastRankChangePage; // Update the playerFoundPage with the page where the last rank change occurred.
                    }
                }

                await Task.Delay(0); // Introduce a small delay before making the next recursive call.

                await StalkPlayerRanks(); // Call the method again to continue stalking.
            }
            catch (Exception ex)
            {
                LogMessage($"An error occurred while stalking data: {ex.Message}");
                LogMessage("Retrying in 5 seconds...");
                await Task.Delay(5000); // Delay before retrying.
                await StalkPlayerRanks(); // Retry the stalking process.
            }
        }

        private async Task GetPageDataAsync(HttpClient client, string url, int currentPage, int previousRank)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url); // Send an HTTP GET request to the specified URL.
                response.EnsureSuccessStatusCode(); // Ensure that the response is successful.

                string jsonContent = await response.Content.ReadAsStringAsync(); // Read the response content as a string.

                LeaderboardData leaderboardData = JsonConvert.DeserializeObject<LeaderboardData>(jsonContent); // Deserialize the JSON data.

                List<(string Rank, string Link)> ranksWithLinks = new List<(string Rank, string Link)>(); // List to store rank and link data.

                if (leaderboardData != null && leaderboardData.Leaderboard != null && leaderboardData.Leaderboard.Rows.Count > 0)
                {
                    foreach (var row in leaderboardData.Leaderboard.Rows)
                    {
                        if (row.AccountId == playerName) // Check if the current row belongs to the target player.
                        {
                            int stalkingRange = (int)StalkingRangeNumericUpdown.Value; // Get the stalking range from the UI.
                            int refreshRateInSeconds = (int)RefreshRateNumericUpDown.Value; // Get the refresh rate from the UI.

                            await Task.Delay(refreshRateInSeconds * 1000); // Introduce a delay based on the refresh rate.

                            ranksWithLinks.Add((row.Rank.ToString(), url)); // Add rank and link data to the list.

                            CurrentRankRichTextBox.Text = $"Stalking {playerName}...\nCurrent Rank: {row.Rank}"; // Update the UI.

                            LogMessage($"Checking for rank changes...");

                            if (currentRank == -1)
                            {
                                currentRank = row.Rank; // Set the current rank if it's not initialized yet.
                            }
                            else if (currentRank != row.Rank)
                            {
                                RankChangeDetected(row.Rank, currentRank); // Handle rank change.
                                LogMessage($"Rank change: {currentRank} to {row.Rank}");
                                currentRank = row.Rank; // Update the current rank.
                                lastRankChangePage = currentPage; // Update the lastRankChangePage.

                                if (lastFoundPage != currentPage)
                                {
                                    lastFoundPage = currentPage; // Update the lastFoundPage.
                                    LogMessage($"Player {playerName} now on page {currentPage}");
                                }

                                await Task.Delay(10000); // Introduce a delay after a rank change is detected.
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"An error occurred while fetching data: {ex.Message}");
            }
        }

        private async void RankChangeDetected(int newRank, int previousRank)
        {
            // Check if notifications are enabled
            if (NotificationCheckbox.Checked)
            {
                // Check if the Text-to-Speech notification is selected
                if (NotificationTypeCheckedListBox.GetItemChecked(0))
                {
                    // Play the rankChangedTTS sound
                    rankChangedTTS.Play();
                }

                // Check if the Discord notification is selected
                if (NotificationTypeCheckedListBox.GetItemChecked(2))
                {
                    // Check if the Discord webhook URL is provided
                    if (!string.IsNullOrWhiteSpace(DiscordWebhookTextbox.Text))
                    {
                        // Send a Discord notification with the new and previous rank
                        await SendDiscordNotification(DiscordWebhookTextbox.Text, newRank, previousRank);
                    }
                    else
                    {
                        // Log a message indicating the Discord webhook URL is not set
                        LogMessage("Discord webhook URL is not set.");
                    }
                }

                // Check if the Popup notification is selected
                if (NotificationTypeCheckedListBox.GetItemChecked(1))
                {
                    // Show a popup notification indicating the rank change
                    ShowPopupNotification(newRank, previousRank);
                }
            }
            else
            {
                // If notifications are not enabled, return without taking any action
                return;
            }
        }

        private void StopStalkingButton_Click(object sender, EventArgs e)
        {
            isStalking = false;
            LogMessage($"Stalking stopped by user.");
            StartStalkingButton.Enabled = true;
            SearchButton.Enabled = true;
            NotificationCheckbox.Enabled = true;
            NotificationTypeCheckedListBox.Enabled = NotificationCheckbox.Checked;

        }
        #endregion

        #region Notification Handling
        private void ShowPopupNotification(int newRank, int previousRank)
        {
            // Show a MessageBox with the rank change information
            MessageBox.Show($"Rank change detected!\nPrevious Rank: {previousRank}\nCurrent Rank: {newRank}",
                "Rank Change Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private async Task SendDiscordNotification(string webhookUrl, int newRank, int previousRank)
        {
            // Set the maximum number of notification retries
            int maxRetries = 3;

            // Set the delay between notification retries in seconds
            int retryDelayInSeconds = 5;

            // Attempt sending the notification up to the maximum number of retries
            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                try
                {
                    // Create an HTTP client
                    using (HttpClient client = new HttpClient())
                    {
                        // Construct the JSON payload for the Discord notification
                        string jsonPayload = $"{{\"content\": \"{playerName}'s rank changed from {previousRank} to {newRank}!\"}}";

                        // Create the content for the HTTP POST request
                        StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        // Send the HTTP POST request to the Discord webhook URL
                        HttpResponseMessage response = await client.PostAsync(webhookUrl, content);

                        // Check if the notification was sent successfully
                        if (response.IsSuccessStatusCode)
                        {
                            // Log a message indicating the successful notification
                            LogMessage("Discord notification sent successfully.");

                            // Break the loop as the notification was sent successfully
                            break;
                        }
                        else
                        {
                            // Log a message indicating the failure to send the Discord notification
                            LogMessage($"Failed to send Discord notification. Status code: {response.StatusCode}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log an error message if an exception occurs during notification sending
                    Console.WriteLine($"An error occurred while sending Discord notification: {ex.Message}");
                }

                // Delay before the next notification retry
                await Task.Delay(retryDelayInSeconds * 1000);
            }
        }

        #endregion

        #region UI Event Handlers
        private void CurrentRankRichTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void NotificationCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            // Enable or disable the notification type checkbox list based on the notification checkbox state
            NotificationTypeCheckedListBox.Enabled = NotificationCheckbox.Checked;
        }

        private void NotificationTypeCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Currently, this event handler doesn't contain specific functionality
        }

        private void DiscordWebhookTextbox_TextChanged(object sender, EventArgs e)
        {
            // Why can't I delete this??? 
        }

        private void SetWebhookButton_Click(object sender, EventArgs e)
        {
            // Check if a valid webhook URL is provided
            if (!string.IsNullOrWhiteSpace(DiscordWebhookTextbox.Text))
            {
                // Save the provided webhook URL in application settings
                Properties.Settings.Default.DiscordWebhookURL = DiscordWebhookTextbox.Text;
                Properties.Settings.Default.Save();

                // Disable the Discord webhook textbox and "Set Webhook" button
                DiscordWebhookTextbox.Enabled = false;
                SetWebhookButton.Enabled = false;

                // Enable the "Reset Webhook" button
                ResetWebhookButton.Enabled = true;
            }
            else
            {
                // Show an error message if the webhook URL is not provided
                MessageBox.Show("Please enter a valid webhook URL.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetWebhookButton_Click(object sender, EventArgs e)
        {
            // Clear the saved webhook URL from application settings
            Properties.Settings.Default.DiscordWebhookURL = "";
            Properties.Settings.Default.Save();

            // Enable the Discord webhook textbox and "Set Webhook" button
            DiscordWebhookTextbox.Enabled = true;
            SetWebhookButton.Enabled = true;

            // Disable the "Reset Webhook" button
            ResetWebhookButton.Enabled = false;

            // Reset the text in the Discord webhook textbox
            DiscordWebhookTextbox.Text = "";
        }

        private void StalkingRangeNumericUpdown_ValueChanged(object sender, EventArgs e)
        {
            // Retrieve the stalking range value from the numeric up-down control
            int stalkingRange = (int)StalkingRangeNumericUpdown.Value;
        }

        private void LogMessage(string message)
        {
            // Append the message to the LogRichTextbox
            LogRichTextbox.AppendText(message + Environment.NewLine);

            // Scroll the LogRichTextbox to the caret
            LogRichTextbox.ScrollToCaret();
        }
        #endregion

        #region Settings Management
        private void LoadSettings()
        {
            SelectRegionComboBox.SelectedItem = Properties.Settings.Default.Region;
            SelectSeasonTextBox.Text = Properties.Settings.Default.Season;
            SelectModeComboBox.SelectedItem = Properties.Settings.Default.Mode;
            SearchPlayerTextBox.Text = Properties.Settings.Default.PlayerName;
            DiscordWebhookTextbox.Text = Properties.Settings.Default.DiscordWebhookURL;
            NotificationCheckbox.Checked = Properties.Settings.Default.NotificationEnabled;
            StalkingRangeNumericUpdown.Value = Properties.Settings.Default.StalkingRange;
            RefreshRateNumericUpDown.Value = Properties.Settings.Default.RefreshRate;

            // Load saved notification type checked items
            if (Properties.Settings.Default.NotificationTypeCheckedList != null)
            {
                foreach (string index in Properties.Settings.Default.NotificationTypeCheckedList)
                {
                    int i;
                    if (int.TryParse(index, out i))
                    {
                        NotificationTypeCheckedListBox.SetItemChecked(i, true);
                    }
                }
            }
        }

        private void SaveSettings()
        {
            // Save user-selected settings to the application's settings
            Properties.Settings.Default.Region = SelectRegionComboBox.SelectedItem?.ToString() ?? "";
            Properties.Settings.Default.Season = SelectSeasonTextBox.Text.Trim();
            Properties.Settings.Default.Mode = SelectModeComboBox.SelectedItem?.ToString() ?? "";
            Properties.Settings.Default.PlayerName = SearchPlayerTextBox.Text.Trim();
            Properties.Settings.Default.DiscordWebhookURL = DiscordWebhookTextbox.Text.Trim();
            Properties.Settings.Default.NotificationEnabled = NotificationCheckbox.Checked;
            Properties.Settings.Default.StalkingRange = (int)StalkingRangeNumericUpdown.Value;
            Properties.Settings.Default.RefreshRate = (int)RefreshRateNumericUpDown.Value;
            Properties.Settings.Default.NotificationTypeCheckedList = new System.Collections.Specialized.StringCollection();

            // Save checked indices of notification types
            foreach (int index in NotificationTypeCheckedListBox.CheckedIndices)
            {
                Properties.Settings.Default.NotificationTypeCheckedList.Add(index.ToString());
            }

            // Save the changes to the settings
            Properties.Settings.Default.Save();
        }

        private void RankLookup_Load(object sender, EventArgs e)
        {
            // Load user settings when the form loads
            LoadSettings();

            // Enable/disable Discord related controls based on webhook URL
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
            // Save user settings when the form is closing
            SaveSettings();
        }
        #endregion

        #region Code Mapper
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
        #endregion

        #region Data Models
        public class LeaderboardData
        {
            [JsonProperty("seasonId")]
            public int SeasonId { get; set; }

            [JsonProperty("leaderboard")]
            public Leaderboard? Leaderboard { get; set; }
        }

        public class Leaderboard
        {
            [JsonProperty("rows")]
            public List<LeaderboardRow>? Rows { get; set; }

            [JsonProperty("columns")]
            public List<string>? Columns { get; set; }

            [JsonProperty("pagination")]
            public Pagination? Pagination { get; set; }
        }

        public class LeaderboardRow
        {
            [JsonProperty("rank")]
            public int Rank { get; set; }

            [JsonProperty("accountid")]
            public string? AccountId { get; set; }
        }

        public class Pagination
        {
            [JsonProperty("totalPages")]
            public int TotalPages { get; set; }

            [JsonProperty("totalSize")]
            public int TotalSize { get; set; }
        }
        #endregion

    }
}