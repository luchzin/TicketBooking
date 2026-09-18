using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TicketBooking.Models;
using TicketBooking.Controls;

namespace TicketBooking
{
    public partial class Form1 : Form
    {
        private List<Movie> _movies;
        private Movie _selectedMovie;
        private Show _selectedShow;

        public Form1()
        {
            InitializeComponent();
            InitUi();
        }

        private void InitUi()
        {
            _movies = Models.DummyData.GetMovies();
            // populate movie cards
            flowMovies.Controls.Clear();
            foreach (var m in _movies)
            {
                var card = new MovieCard();
                card.SetMovie(m);
                card.Margin = new Padding(6);
                card.Selected += (s, e) => SelectMovie(m);
                flowMovies.Controls.Add(card);
            }

            if (_movies.Count > 0) SelectMovie(_movies[0]);

            cbShows.SelectedIndexChanged += CbShows_SelectedIndexChanged;
            btnPurchase.Click += BtnPurchase_Click;
        }

        private void SelectMovie(Movie m)
        {
            _selectedMovie = m;
            lblTitle.Text = m.Title;
            lblMeta.Text = $"{m.Genre} • {m.Duration.TotalMinutes} min";
            cbShows.Items.Clear();
            for (int i = 0; i < m.Shows.Count; i++)
            {
                cbShows.Items.Add(m.Shows[i].Time.ToString("ddd hh:mm tt"));
            }
            if (m.Shows.Count > 0)
            {
                cbShows.SelectedIndex = 0;
                _selectedShow = m.Shows[0];
                RenderSeats();
            }
        }

        private void CbShows_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedMovie == null) return;
            var idx = cbShows.SelectedIndex;
            if (idx >= 0 && idx < _selectedMovie.Shows.Count)
            {
                _selectedShow = _selectedMovie.Shows[idx];
                RenderSeats();
            }
        }

        private void RenderSeats()
        {
            pnlSeats.Controls.Clear();
            if (_selectedShow == null) return;
            int rows = _selectedShow.Seats.Select(s => s.Row).DefaultIfEmpty(0).Max() + 1;
            int cols = _selectedShow.Seats.Select(s => s.Number).DefaultIfEmpty(0).Max() + 1;

            int seatW = 48;
            int seatH = 34;
            int gapX = 8;
            int gapY = 10;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var seat = _selectedShow.Seats.FirstOrDefault(s => s.Row == r && s.Number == c);
                    var sb = new SeatButton();
                    sb.SetSeat(seat);
                    sb.Width = seatW;
                    sb.Height = seatH;
                    sb.Left = c * (seatW + gapX);
                    sb.Top = r * (seatH + gapY);
                    sb.Toggled += (s, e) => UpdateSelectedCount();
                    pnlSeats.Controls.Add(sb);
                }
            }

            pnlSeats.AutoScroll = true;
            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            if (_selectedShow == null) { lblSelectedCount.Text = ""; return; }
            var selected = _selectedShow.Seats.Count(s => s.IsSelected);
            lblSelectedCount.Text = $"Selected: {selected}";
        }

        private void BtnPurchase_Click(object sender, EventArgs e)
        {
            if (_selectedShow == null) return;
            var selectedSeats = _selectedShow.Seats.Where(s => s.IsSelected && !s.IsBooked).ToList();
            if (!selectedSeats.Any())
            {
                MessageBox.Show("Please select seats to purchase.", "No seats", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // simple purchase simulation: mark seats as booked
            foreach (var s in selectedSeats)
            {
                s.IsBooked = true;
                s.IsSelected = false;
            }

            MessageBox.Show($"Purchased {selectedSeats.Count} seat(s) for '{_selectedMovie.Title}'.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RenderSeats();
        }
    }
}
