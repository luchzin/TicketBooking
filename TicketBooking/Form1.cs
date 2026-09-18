using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TicketBooking.Controls;
using TicketBooking.Data;
using TicketBooking.Models;
using TicketBooking.Services;

namespace TicketBooking
{
    public partial class Form1 : Form
    {
        private List<Movie> _allMovies = new List<Movie>();
        private List<Movie> _filteredMovies = new List<Movie>();
        private Movie _selectedMovie;
        private Show _selectedShow;
        private readonly List<MovieCard> _movieCards = new List<MovieCard>();

        public Form1()
        {
            InitializeComponent();

            // Ensure database is created and seeded
            Database.EnsureCreated();

            // Setup custom UI components like the legend
            SetupLegend();

            // Wire event handlers
            txtSearch.TextChanged += TxtSearch_TextChanged;
            cbShows.SelectedIndexChanged += CbShows_SelectedIndexChanged;
            btnPurchase.Click += BtnPurchase_Click;
            btnAddMovie.Click += BtnAddMovie_Click;
            btnMyBookings.Click += BtnMyBookings_Click;
            btnLogout.Click += BtnLogout_Click;

            // Authentication check
            if (!PerformLogin())
            {
                Load += (s, e) => Close();
                return;
            }

            InitData();
        }

        private bool PerformLogin()
        {
            using (var login = new LoginForm())
            {
                if (login.ShowDialog(this) == DialogResult.OK && ProgramState.IsLoggedIn)
                {
                    UpdateUserSessionUi();
                    return true;
                }
            }
            return false;
        }

        private void UpdateUserSessionUi()
        {
            if (ProgramState.IsLoggedIn)
            {
                string roleText = ProgramState.CurrentUserIsAdmin ? " [ADMIN]" : "";
                lblUserInfo.Text = $"👤 {ProgramState.CurrentUserPhone}{roleText}";
                btnAddMovie.Visible = ProgramState.CurrentUserIsAdmin;
                btnMyBookings.Visible = true;
                btnLogout.Visible = true;
            }
            else
            {
                lblUserInfo.Text = "Not signed in";
                btnAddMovie.Visible = false;
                btnMyBookings.Visible = false;
                btnLogout.Visible = false;
            }
        }

        private void SetupLegend()
        {
            pnlLegend.Controls.Clear();

            var legendItems = new[]
            {
                new { Label = "Available", Color = Color.FromArgb(36, 46, 62), Border = Color.FromArgb(56, 70, 92) },
                new { Label = "Selected", Color = Color.FromArgb(0, 170, 150), Border = Color.FromArgb(100, 240, 220) },
                new { Label = "Booked", Color = Color.FromArgb(42, 46, 56), Border = Color.FromArgb(55, 60, 72) }
            };

            int left = 14;
            foreach (var item in legendItems)
            {
                var swatch = new Panel
                {
                    Location = new Point(left, 4),
                    Size = new Size(16, 16),
                    BackColor = item.Color
                };
                swatch.Paint += (s, e) =>
                {
                    using (var p = new Pen(item.Border, 1))
                    {
                        e.Graphics.DrawRectangle(p, 0, 0, 15, 15);
                    }
                };

                var lbl = new Label
                {
                    Text = item.Label,
                    ForeColor = Color.FromArgb(180, 195, 215),
                    Font = new Font("Segoe UI", 8.5F),
                    Location = new Point(left + 22, 3),
                    AutoSize = true
                };

                pnlLegend.Controls.Add(swatch);
                pnlLegend.Controls.Add(lbl);

                left += 120;
            }
        }

        private void InitData()
        {
            _allMovies = MovieService.GetMoviesWithShows();
            _filteredMovies = new List<Movie>(_allMovies);
            RenderMovieList();

            if (_filteredMovies.Count > 0)
            {
                SelectMovie(_filteredMovies[0]);
            }
            else
            {
                ClearMovieDetails();
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string query = txtSearch.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(query))
            {
                _filteredMovies = new List<Movie>(_allMovies);
            }
            else
            {
                _filteredMovies = _allMovies
                    .Where(m => (m.Title != null && m.Title.ToLowerInvariant().Contains(query)) ||
                                (m.Genre != null && m.Genre.ToLowerInvariant().Contains(query)))
                    .ToList();
            }

            lblMoviesHeader.Text = $"NOW SHOWING ({_filteredMovies.Count})";
            RenderMovieList();

            if (_filteredMovies.Count > 0)
            {
                if (_selectedMovie == null || !_filteredMovies.Contains(_selectedMovie))
                {
                    SelectMovie(_filteredMovies[0]);
                }
            }
            else
            {
                ClearMovieDetails();
            }
        }

        private void RenderMovieList()
        {
            flowMovies.SuspendLayout();
            flowMovies.Controls.Clear();
            _movieCards.Clear();

            foreach (var m in _filteredMovies)
            {
                var card = new MovieCard();
                card.SetMovie(m);
                card.Margin = new Padding(4, 4, 4, 8);
                card.IsSelectedCard = (_selectedMovie != null && _selectedMovie.Id == m.Id);

                card.Selected += (s, e) =>
                {
                    SelectMovie(m);
                };

                _movieCards.Add(card);
                flowMovies.Controls.Add(card);
            }

            flowMovies.ResumeLayout();
        }

        private void SelectMovie(Movie m)
        {
            _selectedMovie = m;

            // Highlight selected card
            foreach (var card in _movieCards)
            {
                card.IsSelectedCard = (card.Movie != null && card.Movie.Id == m.Id);
            }

            lblTitle.Text = m.Title;
            lblPriceBadge.Text = $"🎟️ ${m.Price:F2} per ticket";
            lblMeta.Text = $"{m.Genre} • {(int)m.Duration.TotalMinutes} min";
            lblDescription.Text = m.Description;

            cbShows.Items.Clear();
            if (m.Shows != null && m.Shows.Count > 0)
            {
                foreach (var show in m.Shows)
                {
                    cbShows.Items.Add(show);
                }
                cbShows.SelectedIndex = 0;
                _selectedShow = m.Shows[0];
                RenderSeats();
            }
            else
            {
                _selectedShow = null;
                cbShows.Items.Add("No scheduled showtimes");
                cbShows.SelectedIndex = 0;
                pnlSeats.Controls.Clear();
                UpdateSelectedCount();
            }
        }

        private void ClearMovieDetails()
        {
            _selectedMovie = null;
            _selectedShow = null;
            lblTitle.Text = "No movies available";
            lblPriceBadge.Text = "";
            lblMeta.Text = "";
            lblDescription.Text = "No movie found matching your search. Try another search or add a new movie.";
            cbShows.Items.Clear();
            pnlSeats.Controls.Clear();
            UpdateSelectedCount();
        }

        private void CbShows_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedMovie == null || _selectedMovie.Shows == null) return;
            var idx = cbShows.SelectedIndex;
            if (idx >= 0 && idx < _selectedMovie.Shows.Count)
            {
                _selectedShow = _selectedMovie.Shows[idx];
                // Refresh booked seats from database for this show
                RefreshShowBookings(_selectedShow);
                RenderSeats();
            }
        }

        private void RefreshShowBookings(Show show)
        {
            if (show == null) return;
            var bookedCodes = MovieService.GetBookedSeatCodesForShow(show.Id);
            foreach (var seat in show.Seats)
            {
                seat.IsBooked = bookedCodes.Contains(seat.Label);
                seat.IsSelected = false;
            }
        }

        private void RenderSeats()
        {
            pnlSeats.SuspendLayout();
            pnlSeats.Controls.Clear();

            if (_selectedShow == null || _selectedShow.Seats.Count == 0)
            {
                pnlSeats.ResumeLayout();
                UpdateSelectedCount();
                return;
            }

            int rows = _selectedShow.TotalRows;
            int cols = _selectedShow.TotalCols;

            int seatW = 46;
            int seatH = 32;
            int gapX = 10;
            int gapY = 8;
            int startX = 40;
            int startY = 15;

            // Draw Column headers (1, 2, 3...)
            for (int c = 0; c < cols; c++)
            {
                var lblCol = new Label
                {
                    Text = (c + 1).ToString(),
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(120, 135, 155),
                    Location = new Point(startX + c * (seatW + gapX), 0),
                    Size = new Size(seatW, 14),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                pnlSeats.Controls.Add(lblCol);
            }

            // Draw Row headers (A, B, C...) and seat buttons
            for (int r = 0; r < rows; r++)
            {
                char rowLetter = (char)('A' + r);
                var lblRow = new Label
                {
                    Text = rowLetter.ToString(),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 180, 160),
                    Location = new Point(10, startY + r * (seatH + gapY) + 6),
                    Size = new Size(24, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                pnlSeats.Controls.Add(lblRow);

                for (int c = 0; c < cols; c++)
                {
                    var seat = _selectedShow.Seats.FirstOrDefault(s => s.Row == r && s.Number == c);
                    if (seat != null)
                    {
                        var sb = new SeatButton();
                        sb.SetSeat(seat);
                        sb.Width = seatW;
                        sb.Height = seatH;
                        sb.Left = startX + c * (seatW + gapX);
                        sb.Top = startY + r * (seatH + gapY);
                        sb.Toggled += (s, e) => UpdateSelectedCount();
                        pnlSeats.Controls.Add(sb);
                    }
                }
            }

            pnlSeats.ResumeLayout();
            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            if (_selectedShow == null || _selectedMovie == null)
            {
                lblSelectedSeats.Text = "Selected Seats: None";
                lblTotalPrice.Text = "Total: $0.00";
                btnPurchase.Enabled = false;
                return;
            }

            var selectedSeats = _selectedShow.Seats.Where(s => s.IsSelected && !s.IsBooked).ToList();
            if (selectedSeats.Count == 0)
            {
                lblSelectedSeats.Text = "Selected Seats: None";
                lblTotalPrice.Text = "Total: $0.00";
                btnPurchase.Enabled = false;
            }
            else
            {
                string seatCodes = string.Join(", ", selectedSeats.Select(s => s.Label));
                decimal total = selectedSeats.Count * _selectedMovie.Price;
                lblSelectedSeats.Text = $"Selected Seats: {seatCodes} ({selectedSeats.Count} {(selectedSeats.Count == 1 ? "seat" : "seats")})";
                lblTotalPrice.Text = $"Total: ${total:F2}  ({selectedSeats.Count} × ${_selectedMovie.Price:F2})";
                btnPurchase.Enabled = true;
            }
        }

        private void BtnPurchase_Click(object sender, EventArgs e)
        {
            if (_selectedShow == null || _selectedMovie == null) return;

            if (!ProgramState.IsLoggedIn)
            {
                MessageBox.Show("Please sign in to book tickets.", "Sign In Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PerformLogin();
                return;
            }

            var selectedSeats = _selectedShow.Seats.Where(s => s.IsSelected && !s.IsBooked).ToList();
            if (!selectedSeats.Any())
            {
                MessageBox.Show("Please select at least one seat to purchase.", "No Seats Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string seatCodes = string.Join(", ", selectedSeats.Select(s => s.Label));
            decimal totalAmount = selectedSeats.Count * _selectedMovie.Price;

            string confirmMsg = $"Please confirm your booking:\n\n" +
                               $"Movie: {_selectedMovie.Title}\n" +
                               $"Showtime: {_selectedShow.DisplayText}\n" +
                               $"Seats: {seatCodes} ({selectedSeats.Count} seats)\n" +
                               $"Total Price: ${totalAmount:F2}\n\n" +
                               $"Proceed with purchase?";

            var result = MessageBox.Show(confirmMsg, "Confirm Booking", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            string error;
            bool success = BookingService.BookSeats(ProgramState.CurrentUserId, _selectedShow.Id, selectedSeats, _selectedMovie.Price, out error);

            if (success)
            {
                foreach (var s in selectedSeats)
                {
                    s.IsBooked = true;
                    s.IsSelected = false;
                }

                MessageBox.Show($"🎉 Booking confirmed!\n\nYou have successfully booked {selectedSeats.Count} ticket(s) for '{_selectedMovie.Title}'.\nSeats: {seatCodes}\nTotal: ${totalAmount:F2}",
                                "Booking Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                RenderSeats();
            }
            else
            {
                MessageBox.Show($"Failed to book seats: {error}", "Booking Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Refresh seats from DB in case another user booked them
                RefreshShowBookings(_selectedShow);
                RenderSeats();
            }
        }

        private void BtnAddMovie_Click(object sender, EventArgs e)
        {
            if (!ProgramState.CurrentUserIsAdmin)
            {
                MessageBox.Show("Only administrators can add movies.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var addForm = new AddMovieForm())
            {
                if (addForm.ShowDialog(this) == DialogResult.OK)
                {
                    // Reload movies from database
                    _allMovies = MovieService.GetMoviesWithShows();
                    TxtSearch_TextChanged(txtSearch, EventArgs.Empty);

                    // Select the newest movie (last in list)
                    if (_allMovies.Count > 0)
                    {
                        SelectMovie(_allMovies[_allMovies.Count - 1]);
                    }
                }
            }
        }

        private void BtnMyBookings_Click(object sender, EventArgs e)
        {
            if (!ProgramState.IsLoggedIn)
            {
                MessageBox.Show("Please sign in to view your bookings.", "Sign In", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var myBookingsForm = new MyBookingsForm())
            {
                myBookingsForm.ShowDialog(this);
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show("Are you sure you want to sign out?", "Sign Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            ProgramState.Logout();
            UpdateUserSessionUi();

            // Re-prompt login
            if (!PerformLogin())
            {
                Close();
            }
            else
            {
                InitData();
            }
        }
    }
}

