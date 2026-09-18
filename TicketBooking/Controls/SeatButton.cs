using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TicketBooking.Models;

namespace TicketBooking.Controls
{
    public class SeatButton : Control
    {
        private bool _isHovered;

        public Seat Seat { get; private set; }

        public bool SelectedState
        {
            get => Seat != null && Seat.IsSelected;
            set
            {
                if (Seat != null)
                {
                    Seat.IsSelected = value;
                    Invalidate();
                }
            }
        }

        public event EventHandler Toggled;

        public SeatButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Width = 44;
            Height = 32;
            Cursor = Cursors.Hand;
            BackColor = Color.FromArgb(18, 22, 28);
        }

        public void SetSeat(Seat seat)
        {
            Seat = seat;
            if (Seat != null && Seat.IsBooked)
            {
                Cursor = Cursors.No;
            }
            else
            {
                Cursor = Cursors.Hand;
            }
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (Seat != null && !Seat.IsBooked)
            {
                _isHovered = true;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_isHovered)
            {
                _isHovered = false;
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (Seat == null || Seat.IsBooked) return;

            Seat.IsSelected = !Seat.IsSelected;
            Toggled?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(1, 1, Width - 3, Height - 3);

            Color fill;
            Color border;
            Color textColor;

            if (Seat == null)
            {
                fill = Color.FromArgb(40, 45, 55);
                border = Color.FromArgb(60, 65, 75);
                textColor = Color.FromArgb(120, 125, 135);
            }
            else if (Seat.IsBooked)
            {
                fill = Color.FromArgb(42, 46, 56);
                border = Color.FromArgb(55, 60, 72);
                textColor = Color.FromArgb(95, 100, 115);
            }
            else if (Seat.IsSelected)
            {
                fill = Color.FromArgb(0, 170, 150);
                border = Color.FromArgb(100, 240, 220);
                textColor = Color.White;
            }
            else if (_isHovered)
            {
                fill = Color.FromArgb(55, 72, 95);
                border = Color.FromArgb(90, 130, 170);
                textColor = Color.White;
            }
            else
            {
                fill = Color.FromArgb(36, 46, 62);
                border = Color.FromArgb(56, 70, 92);
                textColor = Color.FromArgb(205, 215, 230);
            }

            using (var b = new SolidBrush(fill))
            {
                g.FillRoundedRectangle(b, rect, 6);
            }
            using (var p = new Pen(border, Seat != null && Seat.IsSelected ? 2 : 1))
            {
                g.DrawRoundedRectangle(p, rect, 6);
            }

            using (var f = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var tb = new SolidBrush(textColor))
            {
                var text = Seat?.Label ?? "";
                var sz = g.MeasureString(text, f);
                g.DrawString(text, f, tb, (Width - sz.Width) / 2, (Height - sz.Height) / 2);
            }
        }
    }
}
