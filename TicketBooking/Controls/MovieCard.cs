using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using TicketBooking.Models;

namespace TicketBooking.Controls
{
    public class MovieCard : UserControl
    {
        private Image _poster;
        private bool _isHovered;
        private bool _isSelectedCard;

        public Movie Movie { get; private set; }

        public bool IsSelectedCard
        {
            get => _isSelectedCard;
            set
            {
                if (_isSelectedCard != value)
                {
                    _isSelectedCard = value;
                    Invalidate();
                }
            }
        }

        public event EventHandler Selected;

        public MovieCard()
        {
            DoubleBuffered = true;
            Height = 135;
            Width = 315;
            BackColor = Color.FromArgb(30, 36, 46);
            Cursor = Cursors.Hand;
        }

        public void SetMovie(Movie movie)
        {
            Movie = movie;
            if (_poster != null)
            {
                _poster.Dispose();
                _poster = null;
            }

            if (Movie?.PosterPath != null && File.Exists(Movie.PosterPath))
            {
                try
                {
                    _poster = Image.FromFile(Movie.PosterPath);
                }
                catch
                {
                    _poster = null;
                }
            }

            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Selected?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var cardRect = new Rectangle(2, 2, Width - 5, Height - 5);

            Color bg = _isSelectedCard
                ? Color.FromArgb(38, 48, 62)
                : (_isHovered ? Color.FromArgb(35, 42, 54) : Color.FromArgb(28, 33, 42));

            Color border = _isSelectedCard
                ? Color.FromArgb(0, 180, 160)
                : (_isHovered ? Color.FromArgb(80, 100, 125) : Color.FromArgb(45, 52, 65));

            int borderWidth = _isSelectedCard ? 2 : 1;

            using (var brush = new SolidBrush(bg))
            {
                g.FillRoundedRectangle(brush, cardRect, 8);
            }
            using (var pen = new Pen(border, borderWidth))
            {
                g.DrawRoundedRectangle(pen, cardRect, 8);
            }

            // Poster thumbnail
            var posterRect = new Rectangle(12, 12, 72, Height - 24);
            if (_poster != null)
            {
                g.DrawImage(_poster, posterRect);
            }
            else
            {
                // Attractive gradient placeholder with movie initial
                using (var lgb = new LinearGradientBrush(posterRect, Color.FromArgb(0, 120, 110), Color.FromArgb(20, 40, 60), 60f))
                {
                    g.FillRoundedRectangle(lgb, posterRect, 6);
                }
                using (var pen = new Pen(Color.FromArgb(60, Color.White), 1))
                {
                    g.DrawRoundedRectangle(pen, posterRect, 6);
                }

                string initial = string.IsNullOrEmpty(Movie?.Title) ? "🎬" : Movie.Title.Substring(0, 1).ToUpper();
                using (var font = new Font("Segoe UI", 18, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(220, Color.White)))
                {
                    var sz = g.MeasureString(initial, font);
                    g.DrawString(initial, font, brush, posterRect.X + (posterRect.Width - sz.Width) / 2, posterRect.Y + (posterRect.Height - sz.Height) / 2 - 4);
                }
            }

            if (Movie != null)
            {
                int x = posterRect.Right + 12;
                int y = posterRect.Top + 2;
                int textW = Width - x - 12;

                // Title
                using (var titleFont = new Font("Segoe UI", 10.5F, FontStyle.Bold))
                using (var titleBrush = new SolidBrush(_isSelectedCard ? Color.FromArgb(0, 220, 190) : Color.White))
                {
                    g.DrawString(Movie.Title, titleFont, titleBrush, new RectangleF(x, y, textW, 22), new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap });
                }
                y += 24;

                // Genre, duration and Came Out date
                using (var metaFont = new Font("Segoe UI", 8F))
                using (var metaBrush = new SolidBrush(Color.FromArgb(160, 175, 195)))
                {
                    string relText = Movie.ReleaseDate.HasValue ? $" • {Movie.ReleaseDate.Value:yyyy-MM-dd}" : "";
                    string meta = $"{Movie.Genre} • {(int)Movie.Duration.TotalMinutes}m{relText}";
                    g.DrawString(meta, metaFont, metaBrush, x, y);
                }
                y += 20;

                // Price Badge
                string priceText = $"${Movie.Price:F2}";
                using (var badgeFont = new Font("Segoe UI", 8F, FontStyle.Bold))
                using (var badgeBg = new SolidBrush(Color.FromArgb(25, 70, 60)))
                using (var badgePen = new Pen(Color.FromArgb(0, 150, 136), 1))
                using (var badgeTextBrush = new SolidBrush(Color.FromArgb(100, 230, 190)))
                {
                    var bSz = g.MeasureString(priceText, badgeFont);
                    var bRect = new Rectangle(x, y, (int)bSz.Width + 10, 18);
                    g.FillRoundedRectangle(badgeBg, bRect, 4);
                    g.DrawRoundedRectangle(badgePen, bRect, 4);
                    g.DrawString(priceText, badgeFont, badgeTextBrush, bRect.X + 5, bRect.Y + 2);

                    x += bRect.Width + 6;
                }

                // Show count tag
                if (Movie.Shows != null && Movie.Shows.Count > 0)
                {
                    string showCountText = $"{Movie.Shows.Count} Shows";
                    using (var showFont = new Font("Segoe UI", 7.5F))
                    using (var showBrush = new SolidBrush(Color.FromArgb(140, 160, 180)))
                    {
                        g.DrawString(showCountText, showFont, showBrush, x, y + 2);
                    }
                }

                y += 24;
                x = posterRect.Right + 12;

                // Description (truncated)
                using (var descFont = new Font("Segoe UI", 8F))
                using (var descBrush = new SolidBrush(Color.FromArgb(135, 145, 160)))
                {
                    var descRect = new RectangleF(x, y, Width - x - 12, Height - y - 6);
                    g.DrawString(Movie.Description ?? "", descFont, descBrush, descRect, new StringFormat { Trimming = StringTrimming.EllipsisWord });
                }
            }
        }
    }
}
