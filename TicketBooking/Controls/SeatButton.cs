using System;
using System.Drawing;
using System.Windows.Forms;
using TicketBooking.Models;

namespace TicketBooking.Controls
{
    public class SeatButton : Control
    {
        public Seat Seat { get; private set; }

        public bool SelectedState { get; private set; }

        public event EventHandler Toggled;

        public SeatButton()
        {
            // allow transparent BackColor and update styles before assigning Transparent
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            UpdateStyles();

            Width = 36;
            Height = 28;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            BackColor = Color.Transparent;
        }

        public void SetSeat(Seat seat)
        {
            Seat = seat;
            SelectedState = false;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (Seat == null || Seat.IsBooked) return;
            SelectedState = !SelectedState;
            Seat.IsSelected = SelectedState;
            Toggled?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Color fill;
            Color textColor = Color.White;
            if (Seat == null)
            {
                fill = Color.FromArgb(100, 100, 100);
            }
            else if (Seat.IsBooked)
            {
                fill = Color.FromArgb(120, 120, 120);
                textColor = Color.FromArgb(220, 220, 220);
            }
            else if (SelectedState)
            {
                fill = Color.FromArgb(0, 150, 136);
            }
            else
            {
                fill = Color.FromArgb(60, 70, 80);
            }

            using (var b = new SolidBrush(fill))
            {
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                g.FillRoundedRectangle(b, rect, 6);
            }

            using (var f = new Font("Segoe UI", 8, FontStyle.Regular))
            using (var tb = new SolidBrush(textColor))
            {
                var text = Seat?.Label ?? "";
                var sz = g.MeasureString(text, f);
                g.DrawString(text, f, tb, (Width - sz.Width) / 2, (Height - sz.Height) / 2);
            }
        }
    }
}
