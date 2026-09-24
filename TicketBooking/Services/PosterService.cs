using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace TicketBooking.Services
{
    public class MovieTemplate
    {
        public string Title { get; set; }
        public string Genre { get; set; }
        public int DurationMinutes { get; set; }
        public string Rating { get; set; }
        public string AgeRating { get; set; }
        public string ReleaseDate { get; set; }
        public decimal Price { get; set; }
        public string Tagline { get; set; }
        public string Description { get; set; }
        public string PosterFileName { get; set; }
        public Color ColorTop { get; set; }
        public Color ColorBottom { get; set; }
        public Color ColorAccent { get; set; }
        public string EmblemType { get; set; }
    }

    public static class PosterService
    {
        private static string _postersFolder;
        public static string PostersFolder
        {
            get
            {
                if (_postersFolder == null)
                {
                    string dir = null;
                    try
                    {
                        string asmLoc = typeof(PosterService).Assembly.Location;
                        if (!string.IsNullOrEmpty(asmLoc))
                        {
                            dir = Path.GetDirectoryName(asmLoc);
                        }
                    }
                    catch { }

                    if (string.IsNullOrEmpty(dir) || dir.ToLower().Contains("system32") || dir.ToLower().Contains("syswow64"))
                    {
                        dir = AppDomain.CurrentDomain.BaseDirectory;
                    }

                    if (string.IsNullOrEmpty(dir) || dir.ToLower().Contains("system32") || dir.ToLower().Contains("syswow64"))
                    {
                        dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TicketBooking");
                    }

                    string folder = Path.Combine(dir, "Posters");
                    try
                    {
                        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                        _postersFolder = folder;
                    }
                    catch
                    {
                        string fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TicketBooking", "Posters");
                        Directory.CreateDirectory(fallback);
                        _postersFolder = fallback;
                    }
                }
                return _postersFolder;
            }
        }

        public static readonly List<MovieTemplate> Catalog = new List<MovieTemplate>
        {
            new MovieTemplate
            {
                Title = "Oppenheimer",
                Genre = "Biography / Drama / History",
                DurationMinutes = 180,
                Rating = "8.9/10",
                AgeRating = "R",
                ReleaseDate = "2023-07-21",
                Price = 15.00m,
                Tagline = "Now I Am Become Death, Destroyer of Worlds",
                Description = "The story of American scientist J. Robert Oppenheimer and his decisive role in developing the atomic bomb during Project Manhattan. A harrowing chronicle of scientific genius colliding with irreversible moral consequences.",
                PosterFileName = "oppenheimer.png",
                ColorTop = Color.FromArgb(42, 18, 10),
                ColorBottom = Color.FromArgb(12, 10, 16),
                ColorAccent = Color.FromArgb(255, 175, 45),
                EmblemType = "atom"
            },
            new MovieTemplate
            {
                Title = "Interstellar",
                Genre = "Sci-Fi / Adventure / Drama",
                DurationMinutes = 169,
                Rating = "8.7/10",
                AgeRating = "PG-13",
                ReleaseDate = "2014-11-07",
                Price = 14.50m,
                Tagline = "Mankind Was Born On Earth. It Was Never Meant To Die Here.",
                Description = "When blight and catastrophic dust storms threaten global extinction, former NASA pilot Cooper leads an intrepid expedition through a newly discovered wormhole near Saturn in search of habitable worlds across deep space.",
                PosterFileName = "interstellar.png",
                ColorTop = Color.FromArgb(10, 22, 46),
                ColorBottom = Color.FromArgb(6, 10, 20),
                ColorAccent = Color.FromArgb(80, 210, 255),
                EmblemType = "saturn"
            },
            new MovieTemplate
            {
                Title = "Dune: Part Two",
                Genre = "Sci-Fi / Adventure / Action",
                DurationMinutes = 166,
                Rating = "8.6/10",
                AgeRating = "PG-13",
                ReleaseDate = "2024-03-01",
                Price = 15.50m,
                Tagline = "Long Live The Fighters of Arrakis",
                Description = "Paul Atreides unites with Chani and the Fremen to exact vengeance upon the conspirators who annihilated his family. Confronting a fateful choice between love and the destiny of the galaxy, he must avert a devastating holy war.",
                PosterFileName = "dune_part_two.png",
                ColorTop = Color.FromArgb(52, 28, 12),
                ColorBottom = Color.FromArgb(18, 12, 8),
                ColorAccent = Color.FromArgb(255, 180, 50),
                EmblemType = "dunes"
            },
            new MovieTemplate
            {
                Title = "The Dark Knight",
                Genre = "Action / Crime / Drama",
                DurationMinutes = 152,
                Rating = "9.0/10",
                AgeRating = "PG-13",
                ReleaseDate = "2008-07-18",
                Price = 13.50m,
                Tagline = "Why So Serious?",
                Description = "With the assistance of Lieutenant Gordon and DA Harvey Dent, Batman targets organized crime. But when the anarchist Joker unleashes explosive chaos across Gotham, Batman must navigate his darkest trial of faith and sacrifice.",
                PosterFileName = "dark_knight.png",
                ColorTop = Color.FromArgb(16, 24, 38),
                ColorBottom = Color.FromArgb(8, 10, 15),
                ColorAccent = Color.FromArgb(130, 185, 245),
                EmblemType = "bat"
            },
            new MovieTemplate
            {
                Title = "Inception",
                Genre = "Sci-Fi / Action / Thriller",
                DurationMinutes = 148,
                Rating = "8.8/10",
                AgeRating = "PG-13",
                ReleaseDate = "2010-07-16",
                Price = 14.00m,
                Tagline = "Your Mind Is The Scene Of The Crime",
                Description = "Dom Cobb is a master extractor who steals high-value corporate secrets from deep within the subconscious during sleep. Offered total redemption, he is hired for an unprecedented mission: planting an idea rather than stealing one.",
                PosterFileName = "inception.png",
                ColorTop = Color.FromArgb(25, 34, 48),
                ColorBottom = Color.FromArgb(12, 16, 24),
                ColorAccent = Color.FromArgb(0, 205, 180),
                EmblemType = "maze"
            },
            new MovieTemplate
            {
                Title = "Avatar: The Way of Water",
                Genre = "Sci-Fi / Action / Adventure",
                DurationMinutes = 192,
                Rating = "8.2/10",
                AgeRating = "PG-13",
                ReleaseDate = "2022-12-16",
                Price = 16.00m,
                Tagline = "Return To The Breathtaking Reefs of Pandora",
                Description = "More than a decade after their first triumph, Jake Sully and Neytiri fight to protect their family. When ruthless human colonizers return, they must seek refuge with the Metkayina clan and master the perilous oceans of Pandora.",
                PosterFileName = "avatar_water.png",
                ColorTop = Color.FromArgb(8, 48, 65),
                ColorBottom = Color.FromArgb(4, 16, 28),
                ColorAccent = Color.FromArgb(30, 230, 210),
                EmblemType = "ocean"
            },
            new MovieTemplate
            {
                Title = "Spider-Man: Across the Spider-Verse",
                Genre = "Animation / Action / Adventure",
                DurationMinutes = 140,
                Rating = "8.7/10",
                AgeRating = "PG",
                ReleaseDate = "2023-06-02",
                Price = 13.50m,
                Tagline = "It's How You Wear The Mask That Counts",
                Description = "Miles Morales catapults across the Multiverse, encountering the Spider-Society tasked with safeguarding existence itself. When moral clashes arise over preventing canon events, Miles must redefine heroism on his own terms.",
                PosterFileName = "spider_verse.png",
                ColorTop = Color.FromArgb(55, 12, 45),
                ColorBottom = Color.FromArgb(15, 10, 30),
                ColorAccent = Color.FromArgb(255, 60, 160),
                EmblemType = "spider"
            },
            new MovieTemplate
            {
                Title = "Gladiator II",
                Genre = "Action / Drama / Adventure",
                DurationMinutes = 148,
                Rating = "8.1/10",
                AgeRating = "R",
                ReleaseDate = "2024-11-22",
                Price = 14.50m,
                Tagline = "What We Do In Life Echoes In Eternity",
                Description = "Years after witnessing the martyrdom of general Maximus, Lucius is taken prisoner and forced to fight inside the brutal Colosseum. He must harness his fury to defeat Rome's decadent emperors and restore glory to the empire.",
                PosterFileName = "gladiator_two.png",
                ColorTop = Color.FromArgb(48, 14, 18),
                ColorBottom = Color.FromArgb(18, 8, 10),
                ColorAccent = Color.FromArgb(255, 200, 70),
                EmblemType = "swords"
            },
            new MovieTemplate
            {
                Title = "Cyberpunk: Neon Requiem",
                Genre = "Sci-Fi / Action / Cyberpunk",
                DurationMinutes = 128,
                Rating = "8.8/10",
                AgeRating = "R",
                ReleaseDate = "2025-05-15",
                Price = 14.00m,
                Tagline = "In A City Of Chrome, Soul Is The Ultimate Currency",
                Description = "In the rain-drenched vertical canyons of Neo-Shinjuku, a renegade data courier and a cyborg investigator unearth a sinister syndicate digitizing human souls into military servers. A breathless neon-drenched thrill ride.",
                PosterFileName = "cyberpunk_requiem.png",
                ColorTop = Color.FromArgb(40, 10, 55),
                ColorBottom = Color.FromArgb(10, 8, 22),
                ColorAccent = Color.FromArgb(0, 240, 220),
                EmblemType = "cyber"
            },
            new MovieTemplate
            {
                Title = "The Grand Budapest Hotel",
                Genre = "Comedy / Drama / Adventure",
                DurationMinutes = 99,
                Rating = "8.1/10",
                AgeRating = "R",
                ReleaseDate = "2014-03-28",
                Price = 11.50m,
                Tagline = "A Murder. A Masterpiece. A Magnificent Heist.",
                Description = "The adventures of Gustave H., a legendary concierge at a famous European resort between the wars, and Zero Moustafa, the lobby boy who becomes his closest confidant, amid the turbulent theft of a priceless Renaissance painting.",
                PosterFileName = "grand_budapest.png",
                ColorTop = Color.FromArgb(52, 20, 42),
                ColorBottom = Color.FromArgb(20, 12, 22),
                ColorAccent = Color.FromArgb(255, 170, 210),
                EmblemType = "hotel"
            },
            new MovieTemplate
            {
                Title = "Spirited Away",
                Genre = "Animation / Fantasy / Adventure",
                DurationMinutes = 125,
                Rating = "8.6/10",
                AgeRating = "PG",
                ReleaseDate = "2002-09-20",
                Price = 12.00m,
                Tagline = "Step Through The Tunnel Into A Realm of Wonder",
                Description = "Ten-year-old Chihiro inadvertently stumbles into an ancient realm ruled by spirits and gods. When her parents are bewitched, she must find the fortitude to work in a magnificent bathhouse and win back her family's freedom.",
                PosterFileName = "spirited_away.png",
                ColorTop = Color.FromArgb(14, 45, 35),
                ColorBottom = Color.FromArgb(8, 18, 14),
                ColorAccent = Color.FromArgb(255, 110, 80),
                EmblemType = "torii"
            },
            new MovieTemplate
            {
                Title = "A Quiet Place: Day One",
                Genre = "Horror / Sci-Fi / Drama",
                DurationMinutes = 99,
                Rating = "7.9/10",
                AgeRating = "PG-13",
                ReleaseDate = "2024-06-28",
                Price = 13.00m,
                Tagline = "Hear How The Entire World Went Silent",
                Description = "Witness the chaotic first hours of the alien invasion in New York City. A young terminally ill woman and her companion must navigate a devastated metropolis where the slightest sound triggers instantaneous death.",
                PosterFileName = "quiet_place.png",
                ColorTop = Color.FromArgb(26, 28, 34),
                ColorBottom = Color.FromArgb(10, 12, 15),
                ColorAccent = Color.FromArgb(220, 60, 60),
                EmblemType = "waveform"
            },
            new MovieTemplate
            {
                Title = "Everything Everywhere All at Once",
                Genre = "Sci-Fi / Comedy / Action",
                DurationMinutes = 139,
                Rating = "8.8/10",
                AgeRating = "R",
                ReleaseDate = "2022-04-08",
                Price = 13.50m,
                Tagline = "The Multiverse Is Bigger Than Your Laundromat",
                Description = "Exhausted laundromat owner Evelyn Wang discovers the ability to tap into the lives and skills of parallel versions of herself across infinite realities, uniting family love to vanquish a cosmic threat.",
                PosterFileName = "everything_everywhere.png",
                ColorTop = Color.FromArgb(45, 20, 50),
                ColorBottom = Color.FromArgb(14, 10, 24),
                ColorAccent = Color.FromArgb(255, 220, 70),
                EmblemType = "bagel"
            },
            new MovieTemplate
            {
                Title = "Top Gun: Maverick",
                Genre = "Action / Drama",
                DurationMinutes = 130,
                Rating = "8.6/10",
                AgeRating = "PG-13",
                ReleaseDate = "2022-05-27",
                Price = 14.50m,
                Tagline = "Feel The Need For Pure Supersonic Speed",
                Description = "After thirty years of daring aerial combat, Pete 'Maverick' Mitchell trains a fearless squad of elite TOP GUN graduates for a high-risk canyon strike mission that demands extraordinary courage and flight perfection.",
                PosterFileName = "top_gun_maverick.png",
                ColorTop = Color.FromArgb(16, 32, 54),
                ColorBottom = Color.FromArgb(8, 14, 25),
                ColorAccent = Color.FromArgb(255, 190, 50),
                EmblemType = "jet"
            },
            new MovieTemplate
            {
                Title = "Midnight in Paris",
                Genre = "Fantasy / Romance / Comedy",
                DurationMinutes = 94,
                Rating = "7.8/10",
                AgeRating = "PG-13",
                ReleaseDate = "2011-06-10",
                Price = 11.50m,
                Tagline = "Magic Happens After Midnight On The Seine",
                Description = "On vacation in Paris with his fiancée, nostalgic screenwriter Gil wanders the cobblestone streets at midnight and finds himself magically transported back to the 1920s jazz age among legends Hemingway and Fitzgerald.",
                PosterFileName = "midnight_paris.png",
                ColorTop = Color.FromArgb(22, 28, 50),
                ColorBottom = Color.FromArgb(10, 12, 25),
                ColorAccent = Color.FromArgb(255, 215, 110),
                EmblemType = "eiffel"
            },
            new MovieTemplate
            {
                Title = "Blade Runner 2049",
                Genre = "Sci-Fi / Mystery / Drama",
                DurationMinutes = 164,
                Rating = "8.5/10",
                AgeRating = "R",
                ReleaseDate = "2017-10-06",
                Price = 14.00m,
                Tagline = "The Key To Humanity Is Finally Unearthed",
                Description = "Thirty years after the original events, LAPD Officer K discovers an explosive secret that threatens to plunge what remains of civilization into warfare, sending him on a perilous journey to locate veteran Rick Deckard.",
                PosterFileName = "blade_runner_2049.png",
                ColorTop = Color.FromArgb(50, 24, 10),
                ColorBottom = Color.FromArgb(14, 10, 12),
                ColorAccent = Color.FromArgb(255, 145, 30),
                EmblemType = "pyramid"
            }
        };

        public static void EnsureAllPostersGenerated()
        {
            string folder = PostersFolder;
            foreach (var m in Catalog)
            {
                string targetPath = Path.Combine(folder, m.PosterFileName);
                if (!File.Exists(targetPath) || new FileInfo(targetPath).Length == 0)
                {
                    try
                    {
                        using (var bmp = GeneratePosterBitmap(m, 300, 450))
                        {
                            bmp.Save(targetPath, ImageFormat.Png);
                        }
                    }
                    catch
                    {
                        // Safely ignore if file locked or disk issue
                    }
                }
            }
        }

        public static Bitmap GeneratePosterBitmap(MovieTemplate m, int width = 300, int height = 450)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                var rect = new Rectangle(0, 0, width, height);

                // 1. Rich Gradient Background
                using (var brush = new LinearGradientBrush(rect, m.ColorTop, m.ColorBottom, 68f))
                {
                    g.FillRectangle(brush, rect);
                }

                // 2. Subtle Radial Glow in upper center
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(width * 0.1f, height * 0.05f, width * 0.8f, height * 0.55f);
                    using (var pgb = new PathGradientBrush(path))
                    {
                        pgb.CenterColor = Color.FromArgb(45, m.ColorAccent);
                        pgb.SurroundColors = new[] { Color.Transparent };
                        g.FillPath(pgb, path);
                    }
                }

                // 3. Dual Cinematic Border
                using (var penOuter = new Pen(Color.FromArgb(90, m.ColorAccent), 2f))
                {
                    g.DrawRectangle(penOuter, 6, 6, width - 12, height - 12);
                }
                using (var penInner = new Pen(Color.FromArgb(40, Color.White), 1f))
                {
                    g.DrawRectangle(penInner, 10, 10, width - 20, height - 20);
                }

                // Corner decorative accents
                int cornerLen = 14;
                using (var penCorner = new Pen(m.ColorAccent, 2.5f))
                {
                    // Top-Left
                    g.DrawLine(penCorner, 6, 6, 6 + cornerLen, 6);
                    g.DrawLine(penCorner, 6, 6, 6, 6 + cornerLen);
                    // Top-Right
                    g.DrawLine(penCorner, width - 6, 6, width - 6 - cornerLen, 6);
                    g.DrawLine(penCorner, width - 6, 6, width - 6, 6 + cornerLen);
                    // Bottom-Left
                    g.DrawLine(penCorner, 6, height - 6, 6 + cornerLen, height - 6);
                    g.DrawLine(penCorner, 6, height - 6, 6, height - 6 - cornerLen);
                    // Bottom-Right
                    g.DrawLine(penCorner, width - 6, height - 6, width - 6 - cornerLen, height - 6);
                    g.DrawLine(penCorner, width - 6, height - 6, width - 6, height - 6 - cornerLen);
                }

                // 4. Header Bar
                using (var fontHdr = new Font("Segoe UI", 7.5F, FontStyle.Bold))
                using (var brushHdr = new SolidBrush(Color.FromArgb(210, m.ColorAccent)))
                {
                    var sfHdr = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("★  CINETICKET CINEMA PREMIERE  ★", fontHdr, brushHdr, new RectangleF(0, 16, width, 18), sfHdr);
                }

                // 5. Center Emblem Art (Y = 45 to 225)
                DrawEmblem(g, m.EmblemType, m.ColorAccent, width / 2f, 135f);

                // 6. Title Section (Y = 230 to 310)
                using (var fontTitle = new Font("Segoe UI", m.Title.Length > 22 ? 14.5F : 16.5F, FontStyle.Bold))
                using (var brushShadow = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                using (var brushTitle = new SolidBrush(Color.White))
                {
                    var sfTitle = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };

                    var titleRect = new RectangleF(16, 230, width - 32, 62);
                    var shadowRect = new RectangleF(titleRect.X + 2, titleRect.Y + 2, titleRect.Width, titleRect.Height);
                    g.DrawString(m.Title, fontTitle, brushShadow, shadowRect, sfTitle);
                    g.DrawString(m.Title, fontTitle, brushTitle, titleRect, sfTitle);
                }

                // Divider line
                using (var penDiv = new Pen(Color.FromArgb(120, m.ColorAccent), 1.5f))
                {
                    g.DrawLine(penDiv, width * 0.22f, 298, width * 0.78f, 298);
                }

                // 7. Genre & Tagline (Y = 304 to 370)
                using (var fontGenre = new Font("Segoe UI", 8F, FontStyle.Bold))
                using (var brushGenre = new SolidBrush(Color.FromArgb(235, m.ColorAccent)))
                {
                    var sfGenre = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    string genreText = m.Genre.ToUpperInvariant().Replace("/", "•");
                    g.DrawString(genreText, fontGenre, brushGenre, new RectangleF(16, 304, width - 32, 18), sfGenre);
                }

                if (!string.IsNullOrWhiteSpace(m.Tagline))
                {
                    using (var fontTag = new Font("Segoe UI", 7.5F, FontStyle.Italic))
                    using (var brushTag = new SolidBrush(Color.FromArgb(200, 220, 235)))
                    {
                        var sfTag = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString($"\"{m.Tagline}\"", fontTag, brushTag, new RectangleF(18, 324, width - 36, 42), sfTag);
                    }
                }

                // 8. Bottom Badges Ribbon (Y = 385 to 430)
                int badgeY = 390;

                // Pill 1: Rating Badge (★ 8.9/10)
                var rectRating = new Rectangle(18, badgeY, 82, 28);
                DrawPill(g, rectRating, Color.FromArgb(32, 42, 60), Color.FromArgb(180, m.ColorAccent));
                using (var fontBadge = new Font("Segoe UI", 8F, FontStyle.Bold))
                using (var brushGold = new SolidBrush(Color.FromArgb(255, 220, 60)))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString($"★ {m.Rating}", fontBadge, brushGold, rectRating, sf);
                }

                // Pill 2: Age Rating (PG-13 / R)
                var rectAge = new Rectangle(110, badgeY, 78, 28);
                DrawPill(g, rectAge, Color.FromArgb(32, 42, 60), Color.FromArgb(120, Color.White));
                using (var fontAge = new Font("Segoe UI", 8F, FontStyle.Bold))
                using (var brushAge = new SolidBrush(Color.FromArgb(220, 235, 255)))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(m.AgeRating, fontAge, brushAge, rectAge, sf);
                }

                // Pill 3: Duration / Price
                var rectDur = new Rectangle(198, badgeY, 84, 28);
                DrawPill(g, rectDur, Color.FromArgb(32, 42, 60), Color.FromArgb(180, m.ColorAccent));
                using (var fontDur = new Font("Segoe UI", 8F, FontStyle.Bold))
                using (var brushDur = new SolidBrush(Color.FromArgb(120, 240, 200)))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString($"${m.Price:F2}", fontDur, brushDur, rectDur, sf);
                }

                // Release Date footer
                using (var fontRel = new Font("Segoe UI", 7F))
                using (var brushRel = new SolidBrush(Color.FromArgb(140, 160, 185)))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString($"Came Out: {m.ReleaseDate}  •  {m.DurationMinutes} min", fontRel, brushRel, new RectangleF(16, 422, width - 32, 16), sf);
                }
            }
            return bmp;
        }

        private static void DrawPill(Graphics g, Rectangle r, Color fill, Color border)
        {
            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(border, 1f))
            {
                int radius = 6;
                using (var path = CreateRoundedRect(r, radius))
                {
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                }
            }
        }

        private static GraphicsPath CreateRoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void DrawEmblem(Graphics g, string type, Color accent, float cx, float cy)
        {
            using (var penAccent = new Pen(accent, 2f))
            using (var brushAccent = new SolidBrush(accent))
            using (var penSoft = new Pen(Color.FromArgb(120, accent), 1.5f))
            {
                switch (type?.ToLowerInvariant())
                {
                    case "atom":
                        // Nucleus & Orbiting Ellipses
                        g.FillEllipse(brushAccent, cx - 10, cy - 10, 20, 20);
                        g.DrawEllipse(penAccent, cx - 45, cy - 16, 90, 32);
                        var state = g.Save();
                        g.TranslateTransform(cx, cy);
                        g.RotateTransform(60);
                        g.DrawEllipse(penAccent, -45, -16, 90, 32);
                        g.RotateTransform(60);
                        g.DrawEllipse(penAccent, -45, -16, 90, 32);
                        g.Restore(state);
                        break;

                    case "saturn":
                        // Sphere and Tilted Rings
                        g.FillEllipse(new SolidBrush(Color.FromArgb(40, accent)), cx - 30, cy - 30, 60, 60);
                        g.DrawEllipse(penAccent, cx - 30, cy - 30, 60, 60);
                        var sState = g.Save();
                        g.TranslateTransform(cx, cy);
                        g.RotateTransform(-25);
                        g.DrawEllipse(new Pen(accent, 2.5f), -56, -14, 112, 28);
                        g.DrawEllipse(penSoft, -64, -18, 128, 36);
                        g.Restore(sState);
                        break;

                    case "dunes":
                        // Sun and Layered Sand Dunes
                        g.FillEllipse(new SolidBrush(Color.FromArgb(180, accent)), cx - 22, cy - 36, 44, 44);
                        using (var p1 = new GraphicsPath())
                        {
                            p1.AddBezier(cx - 50, cy + 30, cx - 20, cy - 5, cx + 20, cy + 10, cx + 50, cy + 30);
                            p1.AddLine(cx + 50, cy + 30, cx - 50, cy + 30);
                            g.FillPath(new SolidBrush(Color.FromArgb(90, accent)), p1);
                            g.DrawPath(penAccent, p1);
                        }
                        using (var p2 = new GraphicsPath())
                        {
                            p2.AddBezier(cx - 55, cy + 32, cx - 10, cy + 12, cx + 15, cy - 2, cx + 55, cy + 32);
                            p2.AddLine(cx + 55, cy + 32, cx - 55, cy + 32);
                            g.FillPath(brushAccent, p2);
                        }
                        break;

                    case "bat":
                        // Bat Crest
                        PointF[] batPoints = {
                            new PointF(cx, cy - 10), new PointF(cx + 8, cy - 26), new PointF(cx + 16, cy - 16),
                            new PointF(cx + 36, cy - 22), new PointF(cx + 52, cy - 6), new PointF(cx + 42, cy + 10),
                            new PointF(cx + 30, cy + 16), new PointF(cx + 18, cy + 6), new PointF(cx, cy + 28),
                            new PointF(cx - 18, cy + 6), new PointF(cx - 30, cy + 16), new PointF(cx - 42, cy + 10),
                            new PointF(cx - 52, cy - 6), new PointF(cx - 36, cy - 22), new PointF(cx - 16, cy - 16),
                            new PointF(cx - 8, cy - 26)
                        };
                        g.FillPolygon(brushAccent, batPoints);
                        g.DrawPolygon(new Pen(Color.White, 1.5f), batPoints);
                        break;

                    case "maze":
                        // Spinning Top in Square Maze
                        for (int r = 16; r <= 46; r += 14)
                        {
                            g.DrawRectangle(penSoft, cx - r, cy - r, r * 2, r * 2);
                        }
                        PointF[] topPoints = {
                            new PointF(cx, cy - 24), new PointF(cx + 14, cy - 4),
                            new PointF(cx, cy + 22), new PointF(cx - 14, cy - 4)
                        };
                        g.FillPolygon(brushAccent, topPoints);
                        break;

                    case "ocean":
                        // Waves & Bioluminescent Crest
                        for (int w = -2; w <= 2; w++)
                        {
                            float wy = cy + w * 14;
                            g.DrawBezier(penAccent, cx - 45, wy, cx - 20, wy - 14, cx + 15, wy + 14, cx + 45, wy);
                        }
                        g.FillEllipse(brushAccent, cx - 8, cy - 28, 16, 16);
                        break;

                    case "spider":
                        // Web Geometry & Prism
                        for (int a = 0; a < 360; a += 45)
                        {
                            double rad = a * Math.PI / 180;
                            float x2 = cx + (float)(Math.Cos(rad) * 44);
                            float y2 = cy + (float)(Math.Sin(rad) * 44);
                            g.DrawLine(penSoft, cx, cy, x2, y2);
                        }
                        g.DrawEllipse(penAccent, cx - 18, cy - 18, 36, 36);
                        g.DrawEllipse(penAccent, cx - 34, cy - 34, 68, 68);
                        g.FillEllipse(brushAccent, cx - 8, cy - 8, 16, 16);
                        break;

                    case "swords":
                        // Crossed Gladius Swords & Laurel
                        g.DrawLine(new Pen(accent, 3f), cx - 32, cy - 32, cx + 32, cy + 32);
                        g.DrawLine(new Pen(accent, 3f), cx + 32, cy - 32, cx - 32, cy + 32);
                        g.DrawEllipse(new Pen(Color.FromArgb(160, Color.White), 2f), cx - 36, cy - 36, 72, 72);
                        g.FillEllipse(brushAccent, cx - 7, cy - 7, 14, 14);
                        break;

                    case "cyber":
                        // Neon Cyber Grid & Diamond
                        PointF[] diamond = {
                            new PointF(cx, cy - 38), new PointF(cx + 34, cy),
                            new PointF(cx, cy + 38), new PointF(cx - 34, cy)
                        };
                        g.DrawPolygon(new Pen(accent, 2.5f), diamond);
                        g.DrawLine(penSoft, cx - 34, cy, cx + 34, cy);
                        g.DrawLine(penSoft, cx, cy - 38, cx, cy + 38);
                        g.FillEllipse(brushAccent, cx - 6, cy - 6, 12, 12);
                        break;

                    case "hotel":
                        // Art-Deco Crown / Hotel Pediment
                        PointF[] arch = {
                            new PointF(cx - 36, cy + 24), new PointF(cx - 36, cy - 8),
                            new PointF(cx, cy - 36), new PointF(cx + 36, cy - 8),
                            new PointF(cx + 36, cy + 24)
                        };
                        g.DrawPolygon(penAccent, arch);
                        g.DrawLine(penAccent, cx - 22, cy + 24, cx + 22, cy + 24);
                        g.FillEllipse(brushAccent, cx - 8, cy - 6, 16, 16);
                        break;

                    case "torii":
                        // Torii Shrine Gate
                        g.DrawLine(new Pen(accent, 4f), cx - 44, cy - 24, cx + 44, cy - 24);
                        g.DrawLine(new Pen(accent, 3f), cx - 36, cy - 14, cx + 36, cy - 14);
                        g.DrawLine(new Pen(accent, 3.5f), cx - 22, cy - 24, cx - 22, cy + 30);
                        g.DrawLine(new Pen(accent, 3.5f), cx + 22, cy - 24, cx + 22, cy + 30);
                        g.FillEllipse(new SolidBrush(Color.FromArgb(160, Color.White)), cx - 6, cy + 4, 12, 12);
                        break;

                    case "waveform":
                        // Soundwave Frequency Bars
                        int[] heights = { 8, 16, 28, 44, 62, 44, 28, 16, 8 };
                        float startX = cx - (heights.Length * 8) / 2f;
                        for (int i = 0; i < heights.Length; i++)
                        {
                            float h = heights[i];
                            float x = startX + i * 8;
                            g.DrawLine(new Pen(accent, 3f), x, cy - h / 2f, x, cy + h / 2f);
                        }
                        break;

                    case "bagel":
                        // Everything Bagel Vortex
                        g.DrawEllipse(new Pen(accent, 4f), cx - 36, cy - 36, 72, 72);
                        g.DrawEllipse(new Pen(Color.FromArgb(160, Color.White), 2f), cx - 22, cy - 22, 44, 44);
                        g.FillEllipse(new SolidBrush(Color.FromArgb(60, accent)), cx - 12, cy - 12, 24, 24);
                        for (int d = 0; d < 8; d++)
                        {
                            double ang = d * Math.PI / 4;
                            float bx = cx + (float)(Math.Cos(ang) * 29);
                            float by = cy + (float)(Math.Sin(ang) * 29);
                            g.FillEllipse(new SolidBrush(Color.White), bx - 2.5f, by - 2.5f, 5, 5);
                        }
                        break;

                    case "jet":
                        // Supersonic Jet Silhouette
                        PointF[] jet = {
                            new PointF(cx, cy - 38), new PointF(cx + 8, cy - 12),
                            new PointF(cx + 36, cy + 8), new PointF(cx + 36, cy + 16),
                            new PointF(cx + 10, cy + 12), new PointF(cx + 10, cy + 24),
                            new PointF(cx + 18, cy + 30), new PointF(cx + 18, cy + 34),
                            new PointF(cx, cy + 30),
                            new PointF(cx - 18, cy + 34), new PointF(cx - 18, cy + 30),
                            new PointF(cx - 10, cy + 24), new PointF(cx - 10, cy + 12),
                            new PointF(cx - 36, cy + 16), new PointF(cx - 36, cy + 8),
                            new PointF(cx - 8, cy - 12)
                        };
                        g.FillPolygon(brushAccent, jet);
                        g.DrawPolygon(new Pen(Color.White, 1.5f), jet);
                        break;

                    case "eiffel":
                        // Eiffel Tower & Moon
                        g.FillEllipse(new SolidBrush(Color.FromArgb(160, Color.White)), cx + 18, cy - 34, 18, 18);
                        PointF[] tower = {
                            new PointF(cx, cy - 36), new PointF(cx + 4, cy - 14),
                            new PointF(cx + 14, cy + 8), new PointF(cx + 28, cy + 32),
                            new PointF(cx - 28, cy + 32), new PointF(cx - 14, cy + 8),
                            new PointF(cx - 4, cy - 14)
                        };
                        g.DrawPolygon(penAccent, tower);
                        g.DrawLine(penAccent, cx - 18, cy + 18, cx + 18, cy + 18);
                        break;

                    case "pyramid":
                        // Neon Blade Runner Pyramid & Spinner
                        PointF[] pyr = {
                            new PointF(cx, cy - 36), new PointF(cx + 38, cy + 26),
                            new PointF(cx - 38, cy + 26)
                        };
                        g.DrawPolygon(penAccent, pyr);
                        for (float py = cy - 20; py <= cy + 20; py += 10)
                        {
                            g.DrawLine(penSoft, cx - (py - cy + 36) * 0.7f, py, cx + (py - cy + 36) * 0.7f, py);
                        }
                        break;

                    default:
                        // Classic Cinema Film Reel
                        g.DrawEllipse(penAccent, cx - 32, cy - 32, 64, 64);
                        g.FillEllipse(brushAccent, cx - 8, cy - 8, 16, 16);
                        for (int i = 0; i < 4; i++)
                        {
                            double rad = i * Math.PI / 2;
                            float hx = cx + (float)(Math.Cos(rad) * 20);
                            float hy = cy + (float)(Math.Sin(rad) * 20);
                            g.FillEllipse(brushAccent, hx - 5, hy - 5, 10, 10);
                        }
                        break;
                }
            }
        }
    }
}
