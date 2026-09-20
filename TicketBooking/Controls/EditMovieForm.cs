using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TicketBooking.Models;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class EditMovieForm : Form
    {
        private readonly Movie _movie;

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
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnCancel;
        private Label lblError;

        public EditMovieForm(Movie movie)
        {
            _movie = movie ?? throw new ArgumentNullException(nameof(movie));
            this.SetDoubleBuffered(true);
            InitializeCustomUi();
            LoadMovieData();
        }

        private void InitializeCustomUi()
        {
            Text = $"Admin - Edit Movie: {_movie.Title}";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(540, 650);
            BackColor = Color.FromArgb(20, 24, 34);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            var lblHeader = new Label
            {
                Text = "✏️ Edit Movie Details, Poster & Came Out Date",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 190, 160),
                Location = new Point(22, 14),
                AutoSize = true
            };
            Controls.Add(lblHeader);

            int top = 46;
            int inputW = 490;

            // Title
            var lblTitle = new Label { Text = "Movie Title *", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(22, top), AutoSize = true };
            top += 20;
            txtTitle = new TextBox { Location = new Point(24, top), Width = inputW, Font = new Font("Segoe UI", 10F), BackColor = Color.FromArgb(32, 38, 52), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(lblTitle);
            Controls.Add(txtTitle);
            top += 35;

            // Genre & Came Out Date
            var lblGenre = new Label { Text = "Genre", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(22, top), AutoSize = true };
            var lblRelease = new Label { Text = "Came Out Date (Release Date)", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(275, top), AutoSize = true };
            top += 20;

            cbGenre = new ComboBox
            {
                Location = new Point(24, top),
                Width = 235,
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(32, 38, 52),
                ForeColor = Color.White
            };
            cbGenre.Items.AddRange(new object[] { "Action / Adventure", "Sci-Fi / Cyberpunk", "Drama / Music", "Comedy / Romance", "Thriller / Mystery", "Animation / Family", "Horror / Suspense" });

            dtpReleaseDate = new DateTimePicker
            {
                Location = new Point(275, top),
                Width = 239,
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

            // Duration & Price & Rating
            var lblDuration = new Label { Text = "Duration (min)", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(22, top), AutoSize = true };
            var lblPrice = new Label { Text = "Ticket Price ($)", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(150, top), AutoSize = true };
            var lblRating = new Label { Text = "Rating", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(275, top), AutoSize = true };
            var lblAge = new Label { Text = "Age Rating", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(395, top), AutoSize = true };
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
                Location = new Point(150, top),
                Width = 115,
                DecimalPlaces = 2,
                Minimum = 1,
                Maximum = 150,
                Value = 12.00m,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(32, 38, 52),
                ForeColor = Color.White
            };

            txtRating = new TextBox
            {
                Location = new Point(275, top),
                Width = 110,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(32, 38, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            cbAgeRating = new ComboBox
            {
                Location = new Point(395, top),
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
                Width = 320,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(32, 38, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtImageUrl.TextChanged += TxtImageUrl_TextChanged;

            btnBrowseImage = new Button
            {
                Text = "📁 Browse...",
                Location = new Point(350, top - 1),
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
                Location = new Point(445, top - 20),
                Size = new Size(69, 85),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(28, 35, 48),
                BorderStyle = BorderStyle.FixedSingle
            };

            Controls.Add(txtImageUrl);
            Controls.Add(btnBrowseImage);
            Controls.Add(picPreview);
            top += 30;

            var lblImageTip = new Label
            {
                Text = "Supports web image URLs (https://...) and local image files (.jpg, .png, .webp).",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(140, 160, 185),
                Location = new Point(24, top),
                AutoSize = true
            };
            Controls.Add(lblImageTip);
            top += 25;

            // Description
            var lblDesc = new Label { Text = "Description / Synopsis", ForeColor = Color.FromArgb(210, 220, 235), Location = new Point(22, top), AutoSize = true };
            top += 20;

            txtDescription = new TextBox
            {
                Location = new Point(24, top),
                Width = inputW,
                Height = 85,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(32, 38, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(lblDesc);
            Controls.Add(txtDescription);
            top += 95;

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
                Text = "💾 Save Changes",
                Location = new Point(24, top),
                Width = 280,
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
                Location = new Point(320, top),
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

        private void LoadMovieData()
        {
            txtTitle.Text = _movie.Title;
            cbGenre.Text = _movie.Genre;
            dtpReleaseDate.Value = _movie.ReleaseDate ?? DateTime.Today;
            numDuration.Value = Math.Max(numDuration.Minimum, Math.Min(numDuration.Maximum, (decimal)_movie.Duration.TotalMinutes));
            numPrice.Value = Math.Max(numPrice.Minimum, Math.Min(numPrice.Maximum, _movie.Price));
            txtRating.Text = string.IsNullOrEmpty(_movie.Rating) ? "8.5/10" : _movie.Rating;

            int ageIdx = cbAgeRating.FindStringExact(_movie.AgeRating);
            if (ageIdx >= 0) cbAgeRating.SelectedIndex = ageIdx;
            else cbAgeRating.Text = _movie.AgeRating;

            txtDescription.Text = _movie.Description;
            txtImageUrl.Text = _movie.PosterPath ?? "";
            LoadPreviewImage(_movie.PosterPath);
        }

        private void TxtImageUrl_TextChanged(object sender, EventArgs e)
        {
            LoadPreviewImage(txtImageUrl.Text.Trim());
        }

        private void LoadPreviewImage(string pathOrUrl)
        {
            if (string.IsNullOrEmpty(pathOrUrl))
            {
                picPreview.Image = null;
                return;
            }

            ImageService.LoadImageAsync(pathOrUrl, (img) =>
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
                lblError.Text = "Movie title is required.";
                txtTitle.Focus();
                return;
            }

            _movie.Title = title;
            _movie.Genre = cbGenre.Text.Trim();
            _movie.ReleaseDate = dtpReleaseDate.Value.Date;
            _movie.Duration = TimeSpan.FromMinutes((int)numDuration.Value);
            _movie.Price = numPrice.Value;
            _movie.Rating = string.IsNullOrWhiteSpace(txtRating.Text) ? "8.5/10" : txtRating.Text.Trim();
            _movie.AgeRating = cbAgeRating.SelectedItem?.ToString() ?? "PG-13";
            _movie.Description = txtDescription.Text.Trim();
            _movie.PosterPath = txtImageUrl.Text.Trim();

            try
            {
                bool ok = MovieService.UpdateMovie(_movie);
                if (ok)
                {
                    MessageBox.Show($"Movie '{_movie.Title}' updated successfully!", "Movie Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    lblError.Text = "Could not update movie. Please verify record exists.";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Error saving changes: " + ex.Message;
            }
        }
    }
}
