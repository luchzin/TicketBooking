using System;

namespace TicketBooking.Models
{
    public class Seat
    {
        public string Id { get; set; }
        public int Row { get; set; }
        public int Number { get; set; }
        public bool IsBooked { get; set; }
        public bool IsSelected { get; set; }
        public bool IsVip => Row >= 4; // Back rows are VIP seats

        public string Label => $"{(char)('A' + Row)}{Number + 1}";

        public override string ToString() => Label;
    }
}
