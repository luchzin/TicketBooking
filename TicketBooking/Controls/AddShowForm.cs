using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TicketBooking.Models;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class AddShowForm : Form
    {
        private ComboBox cbMovie;
        private DateTimePicker dtpDate;
        private DateTimePicker dtpTime;
        private ComboBox cbHall;
        private NumericUpDown numRows;
        private NumericUpDown numCols;
        private Label lblTotalSeats;
        private Label lblError;
        private Button btnSave;
        private Button btnCancel;

        private readonly List<Movie> _movies;
        private readonly int _preselectedMovieId;

        public AddShowForm(List<Movie> movies = null, int preselectedMovieId = 0)
        {
            _movies = movies ?? MovieService.GetMoviesWithShows();
            _preselectedMovieId = preselectedMovieId;

            InitializeCustomUi();
        }

        private void InitializeCustomUi()
        {
            Text = "Schedule Showtime";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(460, 480);
            BackColor = Color.FromArgb(20, 24, 33);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            var lblHeader = new Label
            {
                Text = "🕒 Schedule New Showtime",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 190, 160),
                Location = new Point(24, 18),
                AutoSize = true
            };
            Controls.Add(lblHeader);

            int top = 60;
            int inputW = 400;

            // Movie Selection
            var lblMovie = new Label
            {
                Text = "Target Movie",
                ForeColor = Color.FromArgb(200, 210, 225),
                Location = new Point(24, top),
                AutoSize = true
            };
            Controls.Add(lblMovie);
            top += 22;

            cbMovie = new ComboBox
            {
                Location = new Point(26, top),
                Width = inputW,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(32, 38, 50),
                ForeColor = Color.White
            };
            foreach (var m in _movies)
            {
                cbMovie.Items.Add(new MovieComboItem(m.Id, m.Title));
            }
            if (cbMovie.Items.Count > 0)
            {
                int selectedIdx = 0;
                if (_preselectedMovieId > 0)
                {
                    for (int i = 0; i < cbMovie.Items.Count; i++)
                    {
                        if (((MovieComboItem)cbMovie.Items[i]).Id == _preselectedMovieId)
                        {
                            selectedIdx = i;
                            break;
                        }
                    }
                }
                cbMovie.SelectedIndex = selectedIdx;
            }
            Controls.Add(cbMovie);
            top += 40;

            // Date & Time pickers
            var lblDate = new Label
            {
                Text = "Show Date",
                ForeColor = Color.FromArgb(200, 210, 225),
                Location = new Point(24, top),
                AutoSize = true
            };
            var lblTime = new Label
            {
                Text = "Show Time",
                ForeColor = Color.FromArgb(200, 210, 225),
                Location = new Point(230, top),
                AutoSize = true
            };
            Controls.Add(lblDate);
            Controls.Add(lblTime);
            top += 22;

            dtpDate = new DateTimePicker
            {
                Location = new Point(26, top),
                Width = 190,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(1),
                Font = new Font("Segoe UI", 9.5F)
            };
            dtpTime = new DateTimePicker
            {
                Location = new Point(230, top),
                Width = 196,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Value = DateTime.Today.AddHours(19), // 7:00 PM
                Font = new Font("Segoe UI", 9.5F)
            };
            Controls.Add(dtpDate);
            Controls.Add(dtpTime);
            top += 42;

            // Hall Selection
            var lblHall = new Label
            {
                Text = "Cinema Hall / Theater",
                ForeColor = Color.FromArgb(200, 210, 225),
                Location = new Point(24, top),
                AutoSize = true
            };
            Controls.Add(lblHall);
            top += 22;

            cbHall = new ComboBox
            {
                Location = new Point(26, top),
                Width = inputW,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(32, 38, 50),
                ForeColor = Color.White
            };
            cbHall.Items.AddRange(new object[] { "Hall 1 (Main Cinema)", "Hall 2 (Standard)", "Hall 3 (Standard)", "IMAX Theater 4K", "VIP Lounge Screening" });
            cbHall.SelectedIndex = 0;
            Controls.Add(cbHall);
            top += 42;

            // Seating Grid Rows & Cols
            var lblRows = new Label
            {
                Text = "Total Rows (A-Z)",
                ForeColor = Color.FromArgb(200, 210, 225),
                Location = new Point(24, top),
                AutoSize = true
            };
            var lblCols = new Label
            {
                Text = "Total Columns / Row",
                ForeColor = Color.FromArgb(200, 210, 225),
                Location = new Point(230, top),
                AutoSize = true
            };
            Controls.Add(lblRows);
            Controls.Add(lblCols);
            top += 22;

            numRows = new NumericUpDown
            {
                Location = new Point(26, top),
                Width = 190,
                Minimum = 2,
                Maximum = 12,
                Value = 6,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(32, 38, 50),
                ForeColor = Color.White
            };
            numCols = new NumericUpDown
            {
                Location = new Point(230, top),
                Width = 196,
                Minimum = 4,
                Maximum = 14,
                Value = 8,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(32, 38, 50),
                ForeColor = Color.White
            };
            numRows.ValueChanged += UpdateTotalSeatsLabel;
            numCols.ValueChanged += UpdateTotalSeatsLabel;
            Controls.Add(numRows);
            Controls.Add(numCols);
            top += 35;

            lblTotalSeats = new Label
            {
                Location = new Point(26, top),
                Width = inputW,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(140, 160, 185),
                Text = "Capacity: 48 seats (Rows A to F, Seats 1 to 8)"
            };
            Controls.Add(lblTotalSeats);
            top += 25;

            lblError = new Label
            {
                Location = new Point(26, top),
                Width = inputW,
                Height = 22,
                ForeColor = Color.FromArgb(255, 100, 100),
                Text = string.Empty
            };
            Controls.Add(lblError);
            top += 28;

            btnSave = new Button
            {
                Text = "Schedule Showtime",
                Location = new Point(26, top),
                Width = 240,
                Height = 38,
                BackColor = Color.FromArgb(0, 170, 150),
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
                Location = new Point(276, top),
                Width = 150,
                Height = 38,
                BackColor = Color.FromArgb(50, 58, 72),
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

        private void UpdateTotalSeatsLabel(object sender, EventArgs e)
        {
            int r = (int)numRows.Value;
            int c = (int)numCols.Value;
            char lastRowLetter = (char)('A' + r - 1);
            lblTotalSeats.Text = $"Capacity: {r * c} seats (Rows A to {lastRowLetter}, Seats 1 to {c})";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;

            if (cbMovie.SelectedItem == null)
            {
                lblError.Text = "Please select a target movie.";
                return;
            }

            var item = (MovieComboItem)cbMovie.SelectedItem;
            DateTime combinedDateTime = dtpDate.Value.Date + dtpTime.Value.TimeOfDay;

            if (combinedDateTime < DateTime.Now)
            {
                lblError.Text = "Showtime must be scheduled in the future.";
                return;
            }

            string hall = cbHall.SelectedItem?.ToString() ?? "Hall 1";
            int rows = (int)numRows.Value;
            int cols = (int)numCols.Value;

            try
            {
                MovieService.AddShowToMovie(item.Id, combinedDateTime, hall, rows, cols);
                MessageBox.Show($"New showtime scheduled successfully for '{item.Title}' at {combinedDateTime:yyyy-MM-dd hh:mm tt} in {hall}!",
                    "Showtime Scheduled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblError.Text = "Failed to add showtime: " + ex.Message;
            }
        }

        private class MovieComboItem
        {
            public int Id { get; }
            public string Title { get; }

            public MovieComboItem(int id, string title)
            {
                Id = id;
                Title = title;
            }

            public override string ToString() => Title;
        }
    }
}
