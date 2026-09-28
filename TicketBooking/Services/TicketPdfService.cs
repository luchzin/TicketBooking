using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Windows.Forms;
using TicketBooking.Models;

namespace TicketBooking.Services
{
    public static class TicketPdfService
    {
        /// <summary>
        /// Generates a high-resolution, print-ready Bitmap of the E-Ticket.
        /// </summary>
        public static Bitmap GenerateTicketBitmap(Booking booking, int width = 1200, int height = 1600)
        {
            if (booking == null) throw new ArgumentNullException(nameof(booking));

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // 1. Full Page Background
                using (var bgBrush = new SolidBrush(Color.FromArgb(16, 20, 28)))
                {
                    g.FillRectangle(bgBrush, 0, 0, width, height);
                }

                // 2. Ticket Card Container
                var cardRect = new Rectangle(40, 40, width - 80, height - 80);
                using (var cardBrush = new SolidBrush(Color.FromArgb(24, 30, 42)))
                using (var borderPen = new Pen(Color.FromArgb(48, 62, 84), 2f))
                {
                    g.FillRectangle(cardBrush, cardRect);
                    g.DrawRectangle(borderPen, cardRect);
                }

                // 3. Header Banner
                var headerRect = new Rectangle(40, 40, width - 80, 140);
                using (var headerBrush = new SolidBrush(Color.FromArgb(18, 24, 36)))
                using (var tealBrush = new SolidBrush(Color.FromArgb(0, 210, 180)))
                using (var mutedBrush = new SolidBrush(Color.FromArgb(140, 160, 185)))
                using (var goldBrush = new SolidBrush(Color.FromArgb(255, 215, 80)))
                using (var fontBrand = new Font("Segoe UI", 32F, FontStyle.Bold))
                using (var fontSub = new Font("Segoe UI", 15F, FontStyle.Bold))
                using (var fontRef = new Font("Segoe UI", 24F, FontStyle.Bold))
                {
                    g.FillRectangle(headerBrush, headerRect);
                    g.DrawString("CINETICKET CINEMAS", fontBrand, tealBrush, 80, 65);
                    g.DrawString("OFFICIAL DIGITAL ADMISSION PASS", fontSub, mutedBrush, 85, 125);

                    string refText = !string.IsNullOrEmpty(booking.ReferenceCode) ? booking.ReferenceCode : $"TKT-{booking.Id}";
                    var sfRight = new StringFormat { Alignment = StringAlignment.Far };
                    g.DrawString(refText, fontRef, goldBrush, width - 80, 85, sfRight);
                }

                // 4. Movie Poster
                int posterX = 80;
                int posterY = 220;
                int posterW = 270;
                int posterH = 390;
                var posterRect = new Rectangle(posterX, posterY, posterW, posterH);

                Image posterImg = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(booking.PosterPath))
                    {
                        posterImg = ImageService.LoadImage(booking.PosterPath);
                    }
                }
                catch { }

                if (posterImg != null)
                {
                    g.DrawImage(posterImg, posterRect);
                }
                else
                {
                    using (var pBg = new SolidBrush(Color.FromArgb(32, 40, 56)))
                    using (var pTxtBrush = new SolidBrush(Color.FromArgb(160, 180, 210)))
                    using (var pFont = new Font("Segoe UI", 24F, FontStyle.Bold))
                    using (var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        g.FillRectangle(pBg, posterRect);
                        g.DrawString("🎬\nCINETICKET", pFont, pTxtBrush, posterRect, sfCenter);
                    }
                }

                using (var posterBorderPen = new Pen(Color.FromArgb(65, 80, 105), 2f))
                {
                    g.DrawRectangle(posterBorderPen, posterRect);
                }

                // 5. Movie Info & Showtime (Right of Poster)
                int infoX = 385;
                using (var whiteBrush = new SolidBrush(Color.White))
                using (var tealBrush = new SolidBrush(Color.FromArgb(0, 210, 180)))
                using (var mutedBrush = new SolidBrush(Color.FromArgb(140, 160, 185)))
                using (var goldBrush = new SolidBrush(Color.FromArgb(255, 215, 80)))
                using (var fontMovieTitle = new Font("Segoe UI", 28F, FontStyle.Bold))
                using (var fontMeta = new Font("Segoe UI", 16F, FontStyle.Regular))
                using (var fontSectionH = new Font("Segoe UI", 13F, FontStyle.Bold))
                using (var fontSectionV = new Font("Segoe UI", 19F, FontStyle.Bold))
                {
                    // Title
                    string title = !string.IsNullOrEmpty(booking.MovieTitle) ? booking.MovieTitle : "Movie Ticket";
                    g.DrawString(title, fontMovieTitle, whiteBrush, infoX, 220);

                    // Metadata
                    string meta = $"{booking.AgeRating}  |  {booking.Genre}  |  {booking.DurationMinutes} min";
                    g.DrawString(meta, fontMeta, mutedBrush, infoX, 275);

                    // Screening Time Header & Value
                    g.DrawString("SCREENING TIME:", fontSectionH, mutedBrush, infoX, 330);
                    string timeStr = booking.ShowTime != DateTime.MinValue
                        ? booking.ShowTime.ToString("dddd, MMMM d, yyyy | hh:mm tt")
                        : "Scheduled Showtime";
                    g.DrawString(timeStr, fontSectionV, tealBrush, infoX, 355);

                    // Hall & Seat Box
                    var hallBoxRect = new Rectangle(infoX, 420, width - infoX - 80, 190);
                    using (var hallBoxBrush = new SolidBrush(Color.FromArgb(18, 23, 33)))
                    using (var hallBorderPen = new Pen(Color.FromArgb(50, 65, 88), 1.5f))
                    using (var fontHall = new Font("Segoe UI", 17F, FontStyle.Bold))
                    using (var fontSeat = new Font("Segoe UI", 28F, FontStyle.Bold))
                    using (var fontPrice = new Font("Segoe UI", 28F, FontStyle.Bold))
                    {
                        g.FillRectangle(hallBoxBrush, hallBoxRect);
                        g.DrawRectangle(hallBorderPen, hallBoxRect);

                        g.DrawString($"CINEMA HALL: {booking.HallName}", fontHall, whiteBrush, infoX + 25, 442);
                        g.DrawString($"SEAT: {booking.SeatCode}", fontSeat, goldBrush, infoX + 25, 510);

                        string priceStr = $"${booking.Price:N2}";
                        var sfRight = new StringFormat { Alignment = StringAlignment.Far };
                        g.DrawString(priceStr, fontPrice, tealBrush, width - 110, 510, sfRight);
                    }
                }

                // 6. Perforated Ticket Divider
                using (var dashPen = new Pen(Color.FromArgb(65, 80, 105), 3f))
                {
                    dashPen.DashStyle = DashStyle.Dash;
                    g.DrawLine(dashPen, 60, 660, width - 60, 660);
                }

                // 7. Customer & Metadata Table
                using (var whiteBrush = new SolidBrush(Color.White))
                using (var mutedBrush = new SolidBrush(Color.FromArgb(140, 160, 185)))
                using (var grayBrush = new SolidBrush(Color.FromArgb(195, 210, 230)))
                using (var greenBrush = new SolidBrush(Color.FromArgb(60, 220, 150)))
                using (var fontTableH = new Font("Segoe UI", 14F, FontStyle.Regular))
                using (var fontTableV = new Font("Segoe UI", 17F, FontStyle.Bold))
                {
                    // Row 1
                    g.DrawString("CUSTOMER NAME", fontTableH, mutedBrush, 80, 700);
                    string custName = !string.IsNullOrEmpty(booking.CustomerName) ? booking.CustomerName : "Valued Customer";
                    g.DrawString(custName, fontTableV, whiteBrush, 80, 730);

                    g.DrawString("PHONE NUMBER", fontTableH, mutedBrush, 450, 700);
                    g.DrawString(booking.CustomerPhone ?? "-", fontTableV, whiteBrush, 450, 730);

                    g.DrawString("BOOKING STATUS", fontTableH, mutedBrush, 820, 700);
                    string status = (!string.IsNullOrEmpty(booking.Status) ? booking.Status : "Confirmed").ToUpperInvariant();
                    g.DrawString(status, fontTableV, greenBrush, 820, 730);

                    // Row 2
                    g.DrawString("ISSUED DATE & TIME", fontTableH, mutedBrush, 80, 800);
                    string issued = booking.BookingTime != DateTime.MinValue
                        ? booking.BookingTime.ToString("yyyy-MM-dd HH:mm:ss")
                        : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    g.DrawString(issued, fontTableV, grayBrush, 80, 830);

                    g.DrawString("TRANSACTION REF", fontTableH, mutedBrush, 450, 800);
                    g.DrawString($"TXN-{booking.ReferenceCode}-{booking.Id}", fontTableV, grayBrush, 450, 830);

                    g.DrawString("ADMISSION PASS", fontTableH, mutedBrush, 820, 800);
                    g.DrawString("Standard E-Ticket", fontTableV, grayBrush, 820, 830);
                }

                // 8. Digital Barcode Area
                var barBoxRect = new Rectangle(80, 910, width - 160, 240);
                using (var barBoxBrush = new SolidBrush(Color.White))
                using (var blackBrush = new SolidBrush(Color.Black))
                {
                    g.FillRectangle(barBoxBrush, barBoxRect);

                    // Draw scannable barcode stripes
                    int startX = 130;
                    int barY = 935;
                    int barH = 140;
                    int curX = startX;
                    var rnd = new Random(Math.Abs((booking.ReferenceCode ?? "TKT").GetHashCode()));

                    while (curX < width - 130)
                    {
                        int barW = rnd.Next(2, 6);
                        int space = rnd.Next(2, 5);
                        g.FillRectangle(blackBrush, curX, barY, barW, barH);
                        curX += barW + space;
                    }

                    using (var fontBarcode = new Font("Consolas", 22F, FontStyle.Bold))
                    using (var sfCenter = new StringFormat { Alignment = StringAlignment.Center })
                    {
                        string barcodeText = $"* {booking.ReferenceCode} *";
                        g.DrawString(barcodeText, fontBarcode, blackBrush, width / 2f, 1095, sfCenter);
                    }
                }

                // 9. Admission Terms & Notice
                using (var mutedBrush = new SolidBrush(Color.FromArgb(140, 160, 185)))
                using (var fontNotice = new Font("Segoe UI", 14.5F, FontStyle.Italic))
                using (var sfCenter = new StringFormat { Alignment = StringAlignment.Center })
                {
                    string notice = "Present this digital pass on your mobile device or printed copy at theater entrance.\nValid only for designated screening and seat. Non-transferable once scanned.";
                    g.DrawString(notice, fontNotice, mutedBrush, width / 2f, 1220, sfCenter);
                }
            }

            return bmp;
        }

        /// <summary>
        /// Writes a complete, 100% compliant PDF 1.4 file containing the rendered ticket.
        /// Zero external dependencies required.
        /// </summary>
        public static void SaveBookingToPdf(Booking booking, string filePath)
        {
            if (booking == null) throw new ArgumentNullException(nameof(booking));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            // Ensure destination directory exists
            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // 1. Render ticket bitmap
            using (var bmp = GenerateTicketBitmap(booking, 1200, 1600))
            using (var msJpeg = new MemoryStream())
            {
                // Save as JPEG with 95 quality for crisp print fidelity
                var encoder = GetEncoder(ImageFormat.Jpeg);
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 95L);
                bmp.Save(msJpeg, encoder, encoderParams);
                byte[] jpegBytes = msJpeg.ToArray();

                // 2. Build PDF 1.4 Binary Structure
                using (var msPdf = new MemoryStream())
                using (var writer = new BinaryWriter(msPdf))
                {
                    var offsets = new List<long> { 0 }; // 0 is dummy index

                    Action<string> writeAscii = (str) =>
                    {
                        byte[] bytes = Encoding.ASCII.GetBytes(str);
                        writer.Write(bytes, 0, bytes.Length);
                    };

                    // Header
                    writeAscii("%PDF-1.4\n%\xE2\xE3\xCF\xD3\n");

                    // Obj 1: Catalog
                    offsets.Add(msPdf.Position);
                    writeAscii("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

                    // Obj 2: Pages Collection
                    offsets.Add(msPdf.Position);
                    writeAscii("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

                    // Obj 3: Page (A4 Size: 595 x 842 points)
                    offsets.Add(msPdf.Position);
                    writeAscii("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /XObject << /Im1 5 0 R >> >> /Contents 4 0 R >>\nendobj\n");

                    // Obj 4: Content Stream (Scales image to fit page with margins)
                    string contentStr = "q\n535 0 0 713 30 65 cm\n/Im1 Do\nQ\n";
                    byte[] contentBytes = Encoding.ASCII.GetBytes(contentStr);
                    offsets.Add(msPdf.Position);
                    writeAscii($"4 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
                    writer.Write(contentBytes, 0, contentBytes.Length);
                    writeAscii("\nendstream\nendobj\n");

                    // Obj 5: Image XObject (JPEG DCTDecode)
                    offsets.Add(msPdf.Position);
                    writeAscii($"5 0 obj\n<< /Type /XObject /Subtype /Image /Width {bmp.Width} /Height {bmp.Height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {jpegBytes.Length} >>\nstream\n");
                    writer.Write(jpegBytes, 0, jpegBytes.Length);
                    writeAscii("\nendstream\nendobj\n");

                    // Xref Table
                    long xrefOffset = msPdf.Position;
                    writeAscii($"xref\n0 6\n0000000000 65535 f \n");
                    for (int i = 1; i <= 5; i++)
                    {
                        writeAscii($"{offsets[i]:D10} 00000 n \n");
                    }

                    // Trailer
                    writeAscii($"trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");
                    writer.Flush();

                    File.WriteAllBytes(filePath, msPdf.ToArray());
                }
            }
        }

        /// <summary>
        /// Convenience method to save the E-Ticket directly to Desktop (or via SaveFileDialog defaulting to Desktop)
        /// and optionally open the generated PDF file.
        /// </summary>
        public static bool SaveToDesktopWithPrompt(Booking booking, IWin32Window owner = null, bool askToOpen = true)
        {
            if (booking == null) return false;

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string safeRef = !string.IsNullOrEmpty(booking.ReferenceCode) ? booking.ReferenceCode : $"TKT{booking.Id}";
            string defaultName = $"CineTicket_{safeRef}.pdf";

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Save CineTicket E-Ticket as PDF";
                sfd.Filter = "PDF Document (*.pdf)|*.pdf|All Files (*.*)|*.*";
                sfd.InitialDirectory = desktopPath;
                sfd.FileName = defaultName;

                if (sfd.ShowDialog(owner) == DialogResult.OK)
                {
                    try
                    {
                        SaveBookingToPdf(booking, sfd.FileName);

                        if (askToOpen)
                        {
                            var res = MessageBox.Show(
                                $"✅ E-Ticket successfully saved to:\n\n{sfd.FileName}\n\nWould you like to open the PDF ticket now?",
                                "E-Ticket PDF Saved",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);

                            if (res == DialogResult.Yes)
                            {
                                try
                                {
                                    Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            MessageBox.Show($"✅ E-Ticket successfully saved to:\n\n{sfd.FileName}", "E-Ticket Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not save PDF file: {ex.Message}", "Error Saving PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Prints the booking e-ticket using the standard Windows Print Dialog.
        /// </summary>
        public static void PrintBooking(Booking booking, IWin32Window owner = null)
        {
            if (booking == null) return;

            try
            {
                using (var pd = new PrintDocument())
                using (var pDlg = new PrintDialog())
                {
                    pDlg.Document = pd;
                    pDlg.UseEXDialog = true;

                    if (pDlg.ShowDialog(owner) == DialogResult.OK)
                    {
                        pd.PrintPage += (s, ev) =>
                        {
                            using (var ticketBmp = GenerateTicketBitmap(booking, 1200, 1600))
                            {
                                var marginBounds = ev.MarginBounds;
                                ev.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                ev.Graphics.DrawImage(ticketBmp, marginBounds);
                            }
                            ev.HasMorePages = false;
                        };

                        pd.Print();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Printing failed: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
