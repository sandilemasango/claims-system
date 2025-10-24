using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Linq;

namespace ClaimSystem
{
    public partial class MainWindow : Window
    {
        private List<Claim> claims = new List<Claim>();
        private string selectedFilePath = string.Empty;
        private int claimIdCounter = 1;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSampleData();
            CalculateTotalAmount();
        }

        private void InitializeSampleData()
        {
            // Add some sample claims for demonstration
            claims.Add(new Claim
            {
                ClaimId = claimIdCounter++,
                LecturerName = "Dr. Smith",
                Date = DateTime.Now.AddDays(-2).ToString("MMM dd, yyyy"),
                Hours = 40,
                HourlyRate = 75,
                TotalAmount = 3000,
                Status = "Approved",
                StatusColor = "#27ae60",
                Notes = "Regular teaching hours for October",
                Documents = "syllabus.pdf"
            });

            claims.Add(new Claim
            {
                ClaimId = claimIdCounter++,
                LecturerName = "Prof. Johnson",
                Date = DateTime.Now.AddDays(-1).ToString("MMM dd, yyyy"),
                Hours = 35,
                HourlyRate = 80,
                TotalAmount = 2800,
                Status = "Pending",
                StatusColor = "#f39c12",
                Notes = "Additional workshop preparation",
                Documents = "workshop_plan.docx"
            });
        }

        private void BtnLecturerView_Click(object sender, RoutedEventArgs e)
        {
            ShowView(LecturerView);
        }

        private void BtnTrackClaims_Click(object sender, RoutedEventArgs e)
        {
            RefreshClaimsList();
            ShowView(TrackClaimsView);
        }

        private void BtnManagerView_Click(object sender, RoutedEventArgs e)
        {
            RefreshPendingClaims();
            ShowView(ManagerView);
        }

        private void ShowView(UIElement viewToShow)
        {
            LecturerView.Visibility = Visibility.Collapsed;
            TrackClaimsView.Visibility = Visibility.Collapsed;
            ManagerView.Visibility = Visibility.Collapsed;

            viewToShow.Visibility = Visibility.Visible;
        }

        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allow only numbers and decimal point
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }

            // Calculate total when numbers change
            Dispatcher.BeginInvoke(new Action(CalculateTotalAmount));
        }

        private void CalculateTotalAmount()
        {
            try
            {
                double hours = double.Parse(TxtHours.Text);
                double rate = double.Parse(TxtHourlyRate.Text);
                double total = hours * rate;
                TxtTotalAmount.Text = $"${total:F2}";
            }
            catch
            {
                TxtTotalAmount.Text = "$0.00";
            }
        }

        private void BtnBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Supported files (*.pdf;*.docx;*.xlsx)|*.pdf;*.docx;*.xlsx|All files (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);

                // Check file size (5MB limit)
                if (fileInfo.Length > 5 * 1024 * 1024)
                {
                    MessageBox.Show("File size exceeds 5MB limit. Please choose a smaller file.",
                                  "File Too Large",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                selectedFilePath = openFileDialog.FileName;
                TxtFileName.Text = System.IO.Path.GetFileName(selectedFilePath);
            }
        }

        private void BtnSubmitClaim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (!double.TryParse(TxtHours.Text, out double hours) || hours <= 0)
                {
                    MessageBox.Show("Please enter valid hours worked.", "Validation Error",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(TxtHourlyRate.Text, out double rate) || rate <= 0)
                {
                    MessageBox.Show("Please enter valid hourly rate.", "Validation Error",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create new claim
                Claim newClaim = new Claim
                {
                    ClaimId = claimIdCounter++,
                    LecturerName = "Current User", // In real app, get from authentication
                    Date = DateTime.Now.ToString("MMM dd, yyyy"),
                    Hours = hours,
                    HourlyRate = rate,
                    TotalAmount = hours * rate,
                    Status = "Pending",
                    StatusColor = "#f39c12",
                    Notes = TxtNotes.Text,
                    Documents = string.IsNullOrEmpty(selectedFilePath) ? "None" : System.IO.Path.GetFileName(selectedFilePath)
                };

                claims.Add(newClaim);

                // Show success message
                MessageBox.Show("Claim submitted successfully!", "Success",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                // Reset form
                ResetForm();

                // Switch to track claims view
                BtnTrackClaims_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting claim: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetForm()
        {
            TxtHours.Text = "0";
            TxtHourlyRate.Text = "0";
            TxtNotes.Text = "";
            TxtFileName.Text = "No file selected";
            selectedFilePath = string.Empty;
            CalculateTotalAmount();
        }

        private void RefreshClaimsList()
        {
            // Filter claims for current user (in real app, filter by authenticated user)
            var userClaims = claims; // For demo, show all claims

            LvClaims.ItemsSource = null;
            LvClaims.ItemsSource = userClaims;
        }

        private void RefreshPendingClaims()
        {
            var pendingClaims = claims.Where(c => c.Status == "Pending").ToList();
            IcPendingClaims.ItemsSource = pendingClaims;

            TxtNoPendingClaims.Visibility = pendingClaims.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ApproveClaim_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int claimId)
            {
                var claim = claims.FirstOrDefault(c => c.ClaimId == claimId);
                if (claim != null)
                {
                    claim.Status = "Approved";
                    claim.StatusColor = "#27ae60";

                    MessageBox.Show($"Claim from {claim.LecturerName} has been approved.",
                                  "Claim Approved",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    RefreshPendingClaims();
                }
            }
        }

        private void RejectClaim_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int claimId)
            {
                var claim = claims.FirstOrDefault(c => c.ClaimId == claimId);
                if (claim != null)
                {
                    claim.Status = "Rejected";
                    claim.StatusColor = "#e74c3c";

                    MessageBox.Show($"Claim from {claim.LecturerName} has been rejected.",
                                  "Claim Rejected",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    RefreshPendingClaims();
                }
            }
        }
    }

    public class Claim
    {
        public int ClaimId { get; set; }
        public string LecturerName { get; set; }
        public string Date { get; set; }
        public double Hours { get; set; }
        public double HourlyRate { get; set; }
        public double TotalAmount { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
        public string Notes { get; set; }
        public string Documents { get; set; }

        // Properties for manager view
        public string DocumentInfo => !string.IsNullOrEmpty(Documents) && Documents != "None" ?
                                    $"📎 {Documents}" : "No documents";
    }
}
