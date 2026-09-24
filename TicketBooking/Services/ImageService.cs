using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TicketBooking.Services
{
    public static class ImageService
    {
        private static readonly ConcurrentDictionary<string, Image> _memoryCache = new ConcurrentDictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private static readonly string _diskCacheFolder;

        static ImageService()
        {
            try
            {
                // Set modern TLS protocols for downloading web images
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)3072;

                _diskCacheFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TicketBooking", "Posters"
                );
                if (!Directory.Exists(_diskCacheFolder))
                {
                    Directory.CreateDirectory(_diskCacheFolder);
                }
            }
            catch
            {
                _diskCacheFolder = AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public static Image LoadImage(string pathOrUrl)
        {
            if (string.IsNullOrWhiteSpace(pathOrUrl)) return null;

            pathOrUrl = pathOrUrl.Trim();

            // 1. Check memory cache
            if (_memoryCache.TryGetValue(pathOrUrl, out Image cached) && cached != null)
            {
                return cached;
            }

            try
            {
                // 2. Check if Web URL
                if (pathOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    pathOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    string diskPath = GetDiskCachePath(pathOrUrl);
                    if (File.Exists(diskPath))
                    {
                        var img = LoadImageFromFile(diskPath);
                        if (img != null)
                        {
                            _memoryCache[pathOrUrl] = img;
                            return img;
                        }
                    }

                    // Download to disk cache with 3.5s timeout so offline machines don't hang
                    try
                    {
                        var req = (HttpWebRequest)WebRequest.Create(pathOrUrl);
                        req.Timeout = 3500;
                        req.ReadWriteTimeout = 3500;
                        req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) CineTicket/1.0";
                        using (var resp = req.GetResponse())
                        using (var stream = resp.GetResponseStream())
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            byte[] data = ms.ToArray();
                            File.WriteAllBytes(diskPath, data);
                            using (var imgMs = new MemoryStream(data))
                            {
                                var img = new Bitmap(Image.FromStream(imgMs));
                                _memoryCache[pathOrUrl] = img;
                                return img;
                            }
                        }
                    }
                    catch
                    {
                        // Web download failed or timed out; fall through to local lookup or placeholder
                    }
                }

                // 3. Local file (supports absolute paths, relative paths, and PostersFolder lookup)
                string localFile = pathOrUrl;
                if (!File.Exists(localFile))
                {
                    string posterCandidate = Path.Combine(PosterService.PostersFolder, Path.GetFileName(localFile));
                    if (File.Exists(posterCandidate))
                    {
                        localFile = posterCandidate;
                    }
                    else
                    {
                        string candidate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, localFile.TrimStart('/', '\\'));
                        if (File.Exists(candidate))
                        {
                            localFile = candidate;
                        }
                    }
                }

                if (File.Exists(localFile))
                {
                    var img = LoadImageFromFile(localFile);
                    if (img != null)
                    {
                        _memoryCache[pathOrUrl] = img;
                        return img;
                    }
                }
            }
            catch
            {
                // Return null on failure; fallback placeholder will be rendered
            }

            return null;
        }

        public static void LoadImageAsync(string pathOrUrl, Action<Image> onLoaded, Control syncControl = null)
        {
            if (string.IsNullOrWhiteSpace(pathOrUrl))
            {
                onLoaded?.Invoke(null);
                return;
            }

            // If already in memory cache, invoke immediately
            if (_memoryCache.TryGetValue(pathOrUrl.Trim(), out Image cached) && cached != null)
            {
                onLoaded?.Invoke(cached);
                return;
            }

            Task.Run(() =>
            {
                Image img = LoadImage(pathOrUrl);
                if (syncControl != null && syncControl.IsHandleCreated && !syncControl.IsDisposed)
                {
                    try
                    {
                        syncControl.BeginInvoke(new Action(() =>
                        {
                            if (!syncControl.IsDisposed)
                            {
                                onLoaded?.Invoke(img);
                            }
                        }));
                    }
                    catch
                    {
                        // Control disposed or invoke failed
                    }
                }
                else
                {
                    onLoaded?.Invoke(img);
                }
            });
        }

        public static void LoadImageAsync(string pathOrUrl, PictureBox pb, string placeholderTitle = null)
        {
            if (pb == null) return;
            LoadImageAsync(pathOrUrl, (img) =>
            {
                if (pb.IsDisposed) return;
                if (img != null)
                {
                    pb.Image = img;
                }
                else if (!string.IsNullOrEmpty(placeholderTitle))
                {
                    pb.Image = CreatePlaceholder(placeholderTitle, Math.Max(pb.Width, 32), Math.Max(pb.Height, 32));
                }
                else
                {
                    pb.Image = null;
                }
            }, pb);
        }

        private static Image LoadImageFromFile(string filePath)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                using (var ms = new MemoryStream(bytes))
                {
                    return new Bitmap(Image.FromStream(ms));
                }
            }
            catch
            {
                return null;
            }
        }

        private static string GetDiskCachePath(string url)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(url));
                var sb = new StringBuilder();
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return Path.Combine(_diskCacheFolder, sb.ToString() + ".poster");
            }
        }

        public static Image CreatePlaceholder(string title, int width, int height)
        {
            var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, width, height);

                using (var brush = new LinearGradientBrush(rect, Color.FromArgb(18, 24, 36), Color.FromArgb(32, 44, 64), 60f))
                {
                    g.FillRectangle(brush, rect);
                }

                using (var pen = new Pen(Color.FromArgb(50, 70, 95), 1))
                {
                    g.DrawRectangle(pen, 0, 0, width - 1, height - 1);
                }

                string initial = string.IsNullOrWhiteSpace(title) ? "🎬" : title.Substring(0, 1).ToUpper();
                using (var font = new Font("Segoe UI", Math.Max(12, width / 4), FontStyle.Bold))
                using (var textBrush = new SolidBrush(Color.FromArgb(180, 205, 235)))
                {
                    var sz = g.MeasureString(initial, font);
                    g.DrawString(initial, font, textBrush, (width - sz.Width) / 2, (height - sz.Height) / 2 - 4);
                }
            }
            return bmp;
        }
    }
}
