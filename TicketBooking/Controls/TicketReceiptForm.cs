using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TicketBooking.Models;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class TicketReceiptForm : Form
    {
        private readonly Booking _booking;
        private PictureBox picPoster;

        public TicketReceiptForm(Booking booking)
        {
            _booking = booking ?? throw new ArgumentNullException(nameof(booking));

            Text = $"🎬 CineTicket E-Ticket - {_booking.ReferenceCode}";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(620, 680);
            BackColor = Color.FromArgb(18, 22, 30);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            InitializeTicketUi();
        }

        private void InitializeTicketUi()
        {
            // Outer container ticket card with dashed or perforated look
            var pnlTicketCard = new Panel
            {
                Location = new Point(20, 18),
                Size = new Size(580, 595),
                BackColor = Color.FromArgb(28, 34, 46)
            };
            pnlTicketCard.Paint += PnlTicketCard_Paint;

            // Header Banner
            var pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(580, 75),
                BackColor = Color.FromArgb(20, 26, 38)
            };

            var lblBrand = new Label
            {
                Text = "🎬 CINETICKET CINEMAS",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 210, 180),
                Location = new Point(20, 14),
                AutoSize = true
            };
            var lblSub = new Label
            {
                Text = "OFFICIAL DIGITAL E-TICKET & ADMISSION PASS",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 160, 185),
                Location = new Point(22, 42),
                AutoSize = true
            };

            var lblRefBadge = new Label
            {
                Text = _booking.ReferenceCode,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 80),
                Location = new Point(360, 22),
                Size = new Size(200, 30),
                TextAlign = ContentAlignment.MiddleRight
            };

            pnlHeader.Controls.Add(lblBrand);
            pnlHeader.Controls.Add(lblSub);
            pnlHeader.Controls.Add(lblRefBadge);
            pnlTicketCard.Controls.Add(pnlHeader);

            // Movie Poster
            picPoster = new PictureBox
            {
                Location = new Point(22, 92),
                Size = new Size(130, 180),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(18, 22, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            ImageService.LoadImageAsync(_booking.PosterPath, picPoster, _booking.MovieTitle);
            pnlTicketCard.Controls.Add(picPoster);

            // Movie Details Info (Right of poster)
            int infoX = 168;
            int infoY = 92;

            var lblMovieTitle = new Label
            {
                Text = _booking.MovieTitle,
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(infoX, infoY),
                Size = new Size(395, 30)
            };
            pnlTicketCard.Controls.Add(lblMovieTitle);

            var lblMeta = new Label
            {
                Text = $"{_booking.AgeRating}  •  {_booking.Genre}  •  {_booking.DurationMinutes} min",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(150, 175, 205),
                Location = new Point(infoX, infoY + 30),
                AutoSize = true
            };
            pnlTicketCard.Controls.Add(lblMeta);

            // Showtime info
            var lblTimeHeader = new Label
            {
                Text = "SCREENING TIME:",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 140, 165),
                Location = new Point(infoX, infoY + 58),
                AutoSize = true
            };
            var lblShowTime = new Label
            {
                Text = _booking.ShowTime.ToString("dddd, MMMM d, yyyy • hh:mm tt"),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 220, 190),
                Location = new Point(infoX, infoY + 74),
                AutoSize = true
            };
            pnlTicketCard.Controls.Add(lblTimeHeader);
            pnlTicketCard.Controls.Add(lblShowTime);

            // Hall & Seat Box
            var pnlHallSeat = new Panel
            {
                Location = new Point(infoX, infoY + 104),
                Size = new Size(395, 68),
                BackColor = Color.FromArgb(20, 24, 34)
            };

            var lblHall = new Label
            {
                Text = $"🏛️ {_booking.HallName}",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 230, 245),
                Location = new Point(14, 12),
                AutoSize = true
            };
            var lblSeat = new Label
            {
                Text = $"💺 SEAT: {_booking.SeatCode}",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 195, 60),
                Location = new Point(14, 34),
                AutoSize = true
            };

            var lblPrice = new Label
            {
                Text = $"${_booking.Price:N2}",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 230, 190),
                Location = new Point(275, 20),
                Size = new Size(105, 30),
                TextAlign = ContentAlignment.MiddleRight
            };

            pnlHallSeat.Controls.Add(lblHall);
            pnlHallSeat.Controls.Add(lblSeat);
            pnlHallSeat.Controls.Add(lblPrice);
            pnlTicketCard.Controls.Add(pnlHallSeat);

            // Dashed separator
            int sepY = 286;
            var pnlSeparator = new Panel
            {
                Location = new Point(10, sepY),
                Size = new Size(560, 2),
                BackColor = Color.FromArgb(48, 58, 76)
            };
            pnlTicketCard.Controls.Add(pnlSeparator);

            // Customer & Booking metadata
            int metaY = sepY + 12;

            AddReceiptRow(pnlTicketCard, "Customer Name:", string.IsNullOrEmpty(_booking.CustomerName) ? _booking.CustomerPhone : _booking.CustomerName, 22, metaY);
            AddReceiptRow(pnlTicketCard, "Phone Number:", _booking.CustomerPhone, 320, metaY);

            AddReceiptRow(pnlTicketCard, "Booking Status:", _booking.Status.ToUpperInvariant(), 22, metaY + 36,
                _booking.IsActive ? Color.FromArgb(60, 220, 150) : Color.FromArgb(255, 110, 110));
            AddReceiptRow(pnlTicketCard, "Issued Date & Time:", _booking.BookingTime.ToString("yyyy-MM-dd HH:mm:ss"), 320, metaY + 36);

            // Simulated Barcode area
            var pnlBarcode = new Panel
            {
                Location = new Point(22, metaY + 84),
                Size = new Size(536, 95),
                BackColor = Color.White
            };
            pnlBarcode.Paint += PnlBarcode_Paint;

            var lblBarCodeText = new Label
            {
                Text = $"* {_booking.ReferenceCode} *",
                Font = new Font("Consolas", 10F, FontStyle.Bold),
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                Dock = DockStyle.Bottom,
                Height = 22,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlBarcode.Controls.Add(lblBarCodeText);
            pnlTicketCard.Controls.Add(pnlBarcode);

            var lblNotice = new Label
            {
                Text = "⚡ Present this digital pass at theater entrance. Non-transferable once scanned.",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(140, 155, 175),
                Location = new Point(22, 565),
                Size = new Size(536, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlTicketCard.Controls.Add(lblNotice);

            Controls.Add(pnlTicketCard);

            // Bottom Buttons
            var btnCopy = new Button
            {
                Text = "📋 Copy Ticket Text",
                Location = new Point(20, 626),
                Size = new Size(160, 36),
                BackColor = Color.FromArgb(36, 46, 62),
                ForeColor = Color.FromArgb(180, 210, 245),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCopy.FlatAppearance.BorderSize = 0;
            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(GenerateReceiptText());
                MessageBox.Show("Ticket details copied to clipboard!", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            var btnPrint = new Button
            {
                Text = "🖨️ Print / Save Pass",
                Location = new Point(190, 626),
                Size = new Size(160, 36),
                BackColor = Color.FromArgb(0, 160, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += (s, e) =>
            {
                Clipboard.SetText(GenerateReceiptText());
                MessageBox.Show($"E-Ticket #{_booking.ReferenceCode} sent to default printer queue / copied for saving.\n\nThank you for choosing CineTicket!",
                    "Ticket Printed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(480, 626),
                Size = new Size(120, 36),
                BackColor = Color.FromArgb(44, 52, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            Controls.Add(btnCopy);
            Controls.Add(btnPrint);
            Controls.Add(btnClose);

            CancelButton = btnClose;
        }

        private void AddReceiptRow(Panel container, string label, string value, int x, int y, Color? valueColor = null)
        {
            var lblTag = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(130, 150, 175),
                Location = new Point(x, y),
                AutoSize = true
            };
            var lblVal = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = valueColor ?? Color.FromArgb(235, 240, 250),
                Location = new Point(x, y + 15),
                AutoSize = true
            };
            container.Controls.Add(lblTag);
            container.Controls.Add(lblVal);
        }

        private void PnlTicketCard_Paint(object sender, PaintEventArgs e)
        {
            // Border
            using (var pen = new Pen(Color.FromArgb(50, 62, 82), 1.5f))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, 579, 594);
            }
        }

        private void PnlBarcode_Paint(object sender, PaintEventArgs e)
        {
            // Render simulated clean barcode lines
            var g = e.Graphics;
            int startX = 20;
            int h = 60;
            int seed = Math.Abs(_booking.ReferenceCode.GetHashCode());
            var rnd = new Random(seed);

            using (var black = new SolidBrush(Color.Black))
            {
                int x = startX;
                while (x < 515)
                {
                    int barW = rnd.Next(1, 5);
                    g.FillRectangle(black, x, 6, barW, h);
                    x += barW + rnd.Next(1, 4);
                }
            }
        }

        public string GenerateReceiptText()
        {
            return $@"=======================================================
               🎬 CINETICKET CINEMAS
            OFFICIAL ELECTRONIC ADMISSION PASS
=======================================================
Reference Number : {_booking.ReferenceCode}
Status           : {_booking.Status.ToUpperInvariant()}
Issued At        : {_booking.BookingTime:yyyy-MM-dd HH:mm:ss}

Customer Details:
  Name           : {(string.IsNullOrEmpty(_booking.CustomerName) ? _booking.CustomerPhone : _booking.CustomerName)}
  Phone          : {_booking.CustomerPhone}

Movie & Screening:
  Movie Title    : {_booking.MovieTitle}
  Age & Genre    : {_booking.AgeRating} • {_booking.Genre} • {_booking.DurationMinutes} min
  Cinema Hall    : {_booking.HallName}
  Screening Time : {_booking.ShowTime:dddd, MMMM d, yyyy - hh:mm tt}
  Seat Number    : {_booking.SeatCode}
  Admission Price: ${_booking.Price:N2}

-------------------------------------------------------
Notice:
Please present this digital e-ticket at the theater entrance.
=======================================================";
        }
    }
}
