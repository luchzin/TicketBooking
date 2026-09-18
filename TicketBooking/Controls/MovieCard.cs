using System;
using System.Drawing;
using System.Windows.Forms;
using TicketBooking.Models;

namespace TicketBooking.Controls
{
    public class MovieCard : UserControl
    {
        private Button btnSelect;
        public Movie Movie { get; private set; }

        public event EventHandler Selected;

        public MovieCard()
        {
            DoubleBuffered = true;
            Height = 120;
            Width = 300;
            BackColor = Color.FromArgb(34, 40, 49);

            btnSelect = new Button();
            btnSelect.Text = "Select";
            btnSelect.FlatStyle = FlatStyle.Flat;
            btnSelect.ForeColor = Color.White;
            btnSelect.BackColor = Color.FromArgb(0, 150, 136);
            btnSelect.FlatAppearance.BorderSize = 0;
            btnSelect.Height = 28;
            btnSelect.Width = 80;
            btnSelect.Click += (s, e) => Selected?.Invoke(this, EventArgs.Empty);
            Controls.Add(btnSelect);
        }

        public void SetMovie(Movie movie)
        {
            Movie = movie;
            Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            // guard: OnLayout may be called during construction before btnSelect is created
            if (btnSelect == null) return;
            btnSelect.Location = new Point(Width - btnSelect.Width - 12, Height - btnSelect.Height - 12);
        } 
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // card background
            using (var brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // poster placeholder
            var posterRect = new Rectangle(12, 12, 80, Height - 24);
            using (var posterBrush = new SolidBrush(Color.FromArgb(45, 62, 80)))
            {
                g.FillRectangle(posterBrush, posterRect);
            }
            using (var pen = new Pen(Color.FromArgb(80, Color.White)))
            {
                g.DrawRectangle(pen, posterRect);
            }

            if (Movie != null)
            {
                var titleFont = new Font("Segoe UI", 11, FontStyle.Bold);
                var smallFont = new Font("Segoe UI", 9);
                var x = posterRect.Right + 12;
                var y = posterRect.Top;

                using (var titleBrush = new SolidBrush(Color.White))
                using (var metaBrush = new SolidBrush(Color.FromArgb(180, Color.White)))
                {
                    g.DrawString(Movie.Title, titleFont, titleBrush, x, y);
                    y += 26;
                    g.DrawString($"{Movie.Genre} • {Movie.Duration.TotalMinutes} min", smallFont, metaBrush, x, y);
                    y += 22;
                    var descRect = new RectangleF(x, y, Width - x - 12, Height - y - 12);
                    g.DrawString(Movie.Description, smallFont, metaBrush, descRect);
                }
            }
        }
    }
}
