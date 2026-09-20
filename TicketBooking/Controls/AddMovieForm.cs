using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class AddMovieForm : Form
    {
        private TextBox txtTitle;
        private ComboBox cbGenre;
        private DateTimePicker dtpReleaseDate;
        private NumericUpDown numDuration;
        private NumericUpDown numPrice;
        private TextBox txtRating;
        private ComboBox cbAgeRating;
        private TextBox txtImageUrl;
        private Button btnBrowseImage;
        private PictureBox picPreview;
        private DateTimePicker dtpShowTime;
        private ComboBox cbHall;
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnCancel;
        private Label lblError;

        public AddMovieForm()
        {
            this.SetDoubleBuffered(true);
            InitializeCustomUi();
        }

        private void InitializeCustomUi()
        {
            Text = "Admin - Add Movie & Schedule Screening";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(560, 720);
            BackColor = Color.FromArgb(20, 24, 34);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            var lblHeader = new Label
            {
                Text = "🎬 Add New Movie, Poster & Screening Schedule",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 190, 160),
                Location = new Point(22, 14),
                AutoSize = true
            };
            Controls.Add(lblHeader);

            int top = 46;
            int inputW = 510;

            // Movie Title
            var lblTitle = new Label { Text = "Movie Title *", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(22, top), AutoSize = true };
            top += 20;
            txtTitle = new TextBox { Location = new Point(24, top), Width = inputW, Font = new Font("Segoe UI", 10F), BackColor = Color.FromArgb(32, 38, 52), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(lblTitle);
            Controls.Add(txtTitle);
            top += 35;

            // Genre & Came Out Date (Release Date)
            var lblGenre = new Label { Text = "Genre", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(22, top), AutoSize = true };
            var lblRelease = new Label { Text = "Came Out Date (Release Date) *", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(285, top), AutoSize = true };
            top += 20;

            cbGenre = new ComboBox
            {
                Location = new Point(24, top),
                Width = 245,
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(32, 38, 52),
                ForeColor = Color.White
            };
            cbGenre.Items.AddRange(new object[] { "Action / Adventure", "Sci-Fi / Cyberpunk", "Drama / Music", "Comedy / Romance", "Thriller / Mystery", "Animation / Family", "Horror / Suspense" });
            cbGenre.SelectedIndex = 0;

            dtpReleaseDate = new DateTimePicker
            {
                Location = new Point(285, top),
                Width = 249,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Value = DateTime.Today,
                Font = new Font("Segoe UI", 9.5F)
            };

            Controls.Add(lblGenre);
            Controls.Add(lblRelease);
            Controls.Add(cbGenre);
            Controls.Add(dtpReleaseDate);
            top += 38;

            // Duration, Ticket Price, Rating, Age Rating
            var lblDuration = new Label { Text = "Duration (min)", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(22, top), AutoSize = true };
            var lblPrice = new Label { Text = "Price ($)", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(155, top), AutoSize = true };
            var lblRating = new Label { Text = "Rating", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(285, top), AutoSize = true };
            var lblAge = new Label { Text = "Age Rating", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(415, top), AutoSize = true };
            top += 20;

            numDuration = new NumericUpDown
            {
                Location = new Point(24, top),
                Width = 115,
                Minimum = 30,
                Maximum = 360,
                Value = 120,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(32, 38, 52),
                ForeColor = Color.White
            };

            numPrice = new NumericUpDown
            {
                Location = new Point(155, top),
                Width = 115,
                DecimalPlaces = 2,
                Minimum = 1,
                Maximum = 150,
                Value = 12.50m,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(32, 38, 52),
                ForeColor = Color.White
            };

            txtRating = new TextBox
            {
                Location = new Point(285, top),
                Width = 115,
                Text = "8.5/10",
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(32, 38, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            cbAgeRating = new ComboBox
            {
                Location = new Point(415, top),
                Width = 119,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(32, 38, 52),
                ForeColor = Color.White
            };
            cbAgeRating.Items.AddRange(new object[] { "G", "PG", "PG-13", "R", "NC-17" });
            cbAgeRating.SelectedIndex = 2; // PG-13

            Controls.Add(lblDuration);
            Controls.Add(lblPrice);
            Controls.Add(lblRating);
            Controls.Add(lblAge);
            Controls.Add(numDuration);
            Controls.Add(numPrice);
            Controls.Add(txtRating);
            Controls.Add(cbAgeRating);
            top += 38;

            // Poster Image URL / File Path + Preview
            var lblImage = new Label
            {
                Text = "Movie Poster (Web Image URL or Local File Path):",
                ForeColor = Color.FromArgb(210, 220, 235),
                Location = new Point(22, top),
                AutoSize = true
            };
            Controls.Add(lblImage);
            top += 20;

            txtImageUrl = new TextBox
            {
                Location = new Point(24, top),
                Width = 330,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(32, 38, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtImageUrl.TextChanged += TxtImageUrl_TextChanged;

            btnBrowseImage = new Button
            {
                Text = "📁 Browse...",
                Location = new Point(360, top - 1),
                Size = new Size(85, 26),
                BackColor = Color.FromArgb(45, 58, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F),
                Cursor = Cursors.Hand
            };
            btnBrowseImage.FlatAppearance.BorderSize = 0;
            btnBrowseImage.Click += BtnBrowseImage_Click;

            picPreview = new PictureBox
            {
                Location = new Point(455, top - 20),
                Size = new Size(79, 90),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(28, 35, 48),
                BorderStyle = BorderStyle.FixedSingle
            };

            Controls.Add(txtImageUrl);
            Controls.Add(btnBrowseImage);
            Controls.Add(picPreview);
            top += 32;

            var lblImageTip = new Label
            {
                Text = "Paste web image URL (https://...) or choose an image file from your computer.",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(140, 160, 185),
                Location = new Point(24, top),
                AutoSize = true
            };
            Controls.Add(lblImageTip);
            top += 25;

            // Section: Screening Showtime
            var pnlShowtimeBox = new Panel
            {
                Location = new Point(24, top),
                Size = new Size(inputW, 105),
                BackColor = Color.FromArgb(28, 34, 48),
                Padding = new Padding(12)
            };

            var lblShowHeader = new Label
            {
                Text = "🕒 Initial Screening Showtime (Required for user ticket booking)",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(250, 190, 80),
                Location = new Point(10, 8),
                AutoSize = true
            };
            pnlShowtimeBox.Controls.Add(lblShowHeader);

            var lblSt = new Label { Text = "Screening Date & Time:", ForeColor = Color.FromArgb(200, 210, 225), Location = new Point(10, 32), AutoSize = true };
            var lblH = new Label { Text = "Cinema Hall:", ForeColor = Color.FromArgb(200, 210, 225), Location = new Point(265, 32), AutoSize = true };
            pnlShowtimeBox.Controls.Add(lblSt);
            pnlShowtimeBox.Controls.Add(lblH);

            dtpShowTime = new DateTimePicker
            {
                Location = new Point(12, 52),
                Width = 235,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd hh:mm tt",
                Value = DateTime.Today.AddHours(19),
                Font = new Font("Segoe UI", 9.5F)
            };
            pnlShowtimeBox.Controls.Add(dtpShowTime);

            cbHall = new ComboBox
            {
                Location = new Point(265, 52),
                Width = 230,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(36, 44, 60),
                ForeColor = Color.White
            };
            cbHall.Items.AddRange(new object[] { "Hall 1 (Main Cinema)", "Hall 2 (Standard)", "Hall 3 (Standard)", "IMAX Theater 4K", "VIP Lounge Screening" });
            cbHall.SelectedIndex = 0;
            pnlShowtimeBox.Controls.Add(cbHall);

            var lblCapInfo = new Label
            {
                Text = "Capacity: 48 seats (Rows A to F). Online ticket booking enabled automatically.",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(140, 160, 185),
                Location = new Point(12, 80),
                AutoSize = true
            };
            pnlShowtimeBox.Controls.Add(lblCapInfo);

            Controls.Add(pnlShowtimeBox);
            top += 115;

            // Description / Synopsis
            var lblDesc = new Label { Text = "Description / Synopsis", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(22, top), AutoSize = true };
            top += 20;

            txtDescription = new TextBox
            {
                Location = new Point(24, top),
                Width = inputW,
                Height = 65,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(32, 38, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(lblDesc);
            Controls.Add(txtDescription);
            top += 75;

            lblError = new Label
            {
                Location = new Point(24, top),
                Width = inputW,
                Height = 22,
                ForeColor = Color.FromArgb(255, 110, 110),
                Text = string.Empty
            };
            Controls.Add(lblError);
            top += 26;

            btnSave = new Button
            {
                Text = "🎬 Add Movie & Open for Booking",
                Location = new Point(24, top),
                Width = 300,
                Height = 38,
                BackColor = Color.FromArgb(0, 160, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(340, top),
                Width = 194,
                Height = 38,
                BackColor = Color.FromArgb(50, 58, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(btnSave);
            Controls.Add(btnCancel);
        }

        private void TxtImageUrl_TextChanged(object sender, EventArgs e)
        {
            string url = txtImageUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                picPreview.Image = null;
                return;
            }

            ImageService.LoadImageAsync(url, (img) =>
            {
                if (picPreview != null && !picPreview.IsDisposed)
                {
                    picPreview.Image = img;
                }
            }, picPreview);
        }

        private void BtnBrowseImage_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Movie Poster Image";
                ofd.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp|All Files (*.*)|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    txtImageUrl.Text = ofd.FileName;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;

            string title = txtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                lblError.Text = "Please enter the movie title.";
                txtTitle.Focus();
                return;
            }

            DateTime cameOutDate = dtpReleaseDate.Value.Date;
            DateTime showTime = dtpShowTime.Value;

            if (showTime < DateTime.Now.AddMinutes(-10))
            {
                lblError.Text = "Screening showtime must be in the future.";
                return;
            }

            string genre = cbGenre.Text.Trim();
            int duration = (int)numDuration.Value;
            decimal price = numPrice.Value;
            string rating = string.IsNullOrWhiteSpace(txtRating.Text) ? "8.5/10" : txtRating.Text.Trim();
            string ageRating = cbAgeRating.SelectedItem?.ToString() ?? "PG-13";
            string hall = cbHall.SelectedItem?.ToString() ?? "Hall 1";
            string desc = txtDescription.Text.Trim();
            string poster = txtImageUrl.Text.Trim();

            try
            {
                int movieId = MovieService.AddMovieWithShow(
                    title,
                    genre,
                    duration,
                    desc,
                    price,
                    showTime,
                    hall,
                    rating,
                    ageRating,
                    cameOutDate,
                    poster
                );

                MessageBox.Show(
                    $"Movie '{title}' (Came Out: {cameOutDate:yyyy-MM-dd}) was added successfully!\n\nFirst screening scheduled at {showTime:yyyy-MM-dd hh:mm tt} in {hall}.\nCustomers can now book tickets.",
                    "Movie Added & Available for Booking",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblError.Text = "Failed to add movie: " + ex.Message;
            }
        }
    }
}
