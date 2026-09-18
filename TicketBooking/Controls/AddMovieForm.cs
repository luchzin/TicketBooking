using System;
using System.Drawing;
using System.Windows.Forms;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class AddMovieForm : Form
    {
        private TextBox txtTitle;
        private ComboBox cbGenre;
        private NumericUpDown numDuration;
        private NumericUpDown numPrice;
        private TextBox txtDescription;
        private DateTimePicker dtpShowTime;
        private ComboBox cbHall;
        private Button btnSave;
        private Button btnCancel;
        private Label lblError;

        public AddMovieForm()
        {
            Text = "Admin - Add New Movie";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(460, 530);
            BackColor = Color.FromArgb(24, 28, 36);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            var lblHeader = new Label
            {
                Text = "🎬 Add New Movie & Showtime",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 180, 160),
                Location = new Point(24, 18),
                AutoSize = true
            };

            int top = 55;
            int inputW = 410;

            // Title
            var lblTitle = new Label { Text = "Movie Title", ForeColor = Color.FromArgb(200, 210, 225), Location = new Point(24, top), AutoSize = true };
            top += 20;
            txtTitle = new TextBox { Location = new Point(26, top), Width = inputW, Font = new Font("Segoe UI", 10F), BackColor = Color.FromArgb(36, 42, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            top += 35;

            // Genre & Duration in a row
            var lblGenre = new Label { Text = "Genre", ForeColor = Color.FromArgb(200, 210, 225), Location = new Point(24, top), AutoSize = true };
            var lblDuration = new Label { Text = "Duration (minutes)", ForeColor = Color.FromArgb(200, 210, 225), Location = new Point(240, top), AutoSize = true };
            top += 20;

            cbGenre = new ComboBox
            {
                Location = new Point(26, top),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(36, 42, 54),
                ForeColor = Color.White
            };
            cbGenre.Items.AddRange(new object[] { "Action / Adventure", "Sci-Fi / Cyberpunk", "Drama / Music", "Comedy / Romance", "Thriller / Mystery", "Animation / Family" });
            cbGenre.SelectedIndex = 0;

            numDuration = new NumericUpDown
            {
                Location = new Point(242, top),
                Width = 194,
                Minimum = 45,
                Maximum = 300,
                Value = 120,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(36, 42, 54),
                ForeColor = Color.White
            };
            top += 35;

            // Price & Hall in a row
            var lblPrice = new Label { Text = "Ticket Price ($)", ForeColor = Color.FromArgb(200, 210, 225), Location = new Point(24, top), AutoSize = true };
            var lblHall = new Label { Text = "Cinema Hall", ForeColor = Color.FromArgb(200, 210, 225), Location = new Point(240, top), AutoSize = true };
            top += 20;

            numPrice = new NumericUpDown
            {
                Location = new Point(26, top),
                Width = 200,
                DecimalPlaces = 2,
                Minimum = 1,
                Maximum = 100,
                Value = 12.50m,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(36, 42, 54),
                ForeColor = Color.White
            };

            cbHall = new ComboBox
            {
                Location = new Point(242, top),
                Width = 194,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(36, 42, 54),
                ForeColor = Color.White
            };
            cbHall.Items.AddRange(new object[] { "Hall 1", "Hall 2", "Hall 3", "IMAX Theater", "VIP Hall" });
            cbHall.SelectedIndex = 0;
            top += 35;

            // Initial Showtime
            var lblShowTime = new Label { Text = "Initial Showtime", ForeColor = Color.FromArgb(200, 210, 225), Location = new Point(24, top), AutoSize = true };
            top += 20;

            dtpShowTime = new DateTimePicker
            {
                Location = new Point(26, top),
                Width = inputW,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd hh:mm tt",
                Value = DateTime.Now.AddHours(3),
                Font = new Font("Segoe UI", 9.5F)
            };
            top += 35;

            // Description
            var lblDesc = new Label { Text = "Description / Synopsis", ForeColor = Color.FromArgb(200, 210, 225), Location = new Point(24, top), AutoSize = true };
            top += 20;

            txtDescription = new TextBox
            {
                Location = new Point(26, top),
                Width = inputW,
                Height = 70,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(36, 42, 54),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            top += 80;

            lblError = new Label
            {
                Location = new Point(26, top),
                Width = inputW,
                Height = 20,
                ForeColor = Color.FromArgb(255, 100, 100),
                Visible = false
            };
            top += 24;

            btnSave = new Button
            {
                Text = "Save Movie & Showtime",
                Location = new Point(26, top),
                Width = 240,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(280, top),
                Width = 156,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 48, 60),
                ForeColor = Color.FromArgb(190, 200, 215),
                Font = new Font("Segoe UI", 9.5F),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(lblHeader);
            Controls.Add(lblTitle);
            Controls.Add(txtTitle);
            Controls.Add(lblGenre);
            Controls.Add(lblDuration);
            Controls.Add(cbGenre);
            Controls.Add(numDuration);
            Controls.Add(lblPrice);
            Controls.Add(lblHall);
            Controls.Add(numPrice);
            Controls.Add(cbHall);
            Controls.Add(lblShowTime);
            Controls.Add(dtpShowTime);
            Controls.Add(lblDesc);
            Controls.Add(txtDescription);
            Controls.Add(lblError);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            string title = txtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                lblError.Text = "Please enter a movie title.";
                lblError.Visible = true;
                return;
            }

            string genre = cbGenre.Text.Trim();
            int duration = (int)numDuration.Value;
            decimal price = numPrice.Value;
            string desc = txtDescription.Text.Trim();
            DateTime showTime = dtpShowTime.Value;
            string hall = cbHall.Text;

            try
            {
                MovieService.AddMovieWithShow(title, genre, duration, desc, price, showTime, hall);
                MessageBox.Show($"Movie '{title}' and showtime added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblError.Text = "Error saving movie: " + ex.Message;
                lblError.Visible = true;
            }
        }
    }
}
