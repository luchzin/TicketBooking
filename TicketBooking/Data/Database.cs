using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace TicketBooking.Data
{
    public static class Database
    {
        private static string _dbPath;
        public static string DbPath
        {
            get
            {
                if (_dbPath == null)
                {
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    if (string.IsNullOrEmpty(appDir) ||
                        appDir.ToLower().Contains("system32") ||
                        appDir.ToLower().Contains("syswow64"))
                    {
                        string localApp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TicketBooking");
                        Directory.CreateDirectory(localApp);
                        _dbPath = Path.Combine(localApp, "ticketbooking.db");
                    }
                    else
                    {
                        try
                        {
                            Directory.CreateDirectory(appDir);
                            string testFile = Path.Combine(appDir, ".writable_test");
                            File.WriteAllText(testFile, "test");
                            File.Delete(testFile);
                            _dbPath = Path.Combine(appDir, "ticketbooking.db");
                        }
                        catch
                        {
                            string localApp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TicketBooking");
                            Directory.CreateDirectory(localApp);
                            _dbPath = Path.Combine(localApp, "ticketbooking.db");
                        }
                    }
                }
                return _dbPath;
            }
            set => _dbPath = value;
        }

        public static SqliteConnection GetConnection()
        {
            string dir = Path.GetDirectoryName(DbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var cs = new SqliteConnectionStringBuilder
            {
                DataSource = DbPath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            var conn = new SqliteConnection(cs);
            conn.Open();

            // Enable foreign key constraints
            using (var pragmaCmd = conn.CreateCommand())
            {
                pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
                pragmaCmd.ExecuteNonQuery();
            }

            return conn;
        }

        public static void EnsureCreated()
        {
            using (var conn = GetConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Phone TEXT NOT NULL UNIQUE,
    FullName TEXT,
    Email TEXT,
    PasswordHash TEXT NOT NULL,
    IsAdmin INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Movies (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Genre TEXT,
    DurationMinutes INTEGER,
    Description TEXT,
    PosterPath TEXT,
    Price REAL NOT NULL DEFAULT 12.0,
    Rating TEXT DEFAULT '8.5/10',
    AgeRating TEXT DEFAULT 'PG-13',
    ReleaseDate TEXT
);

CREATE TABLE IF NOT EXISTS Shows (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MovieId INTEGER NOT NULL,
    ShowTime TEXT NOT NULL,
    HallName TEXT DEFAULT 'Hall 1',
    TotalRows INTEGER DEFAULT 6,
    TotalCols INTEGER DEFAULT 8,
    FOREIGN KEY(MovieId) REFERENCES Movies(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Bookings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    ShowId INTEGER NOT NULL,
    SeatCode TEXT NOT NULL,
    SeatRow INTEGER NOT NULL,
    SeatCol INTEGER NOT NULL,
    Price REAL NOT NULL,
    BookingTime TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'Confirmed',
    ReferenceCode TEXT,
    FOREIGN KEY(UserId) REFERENCES Users(Id),
    FOREIGN KEY(ShowId) REFERENCES Shows(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_shows_movieid ON Shows(MovieId);
CREATE INDEX IF NOT EXISTS idx_bookings_showid ON Bookings(ShowId);
CREATE INDEX IF NOT EXISTS idx_bookings_userid ON Bookings(UserId);
";
                    cmd.ExecuteNonQuery();
                }

                // Safe non-destructive column migrations for existing databases
                RunSafeMigrations(conn);

                // Seed initial data if empty
                SeedInitialData(conn);
            }
        }

        private static void RunSafeMigrations(SqliteConnection conn)
        {
            var migrations = new[]
            {
                "ALTER TABLE Users ADD COLUMN FullName TEXT;",
                "ALTER TABLE Users ADD COLUMN Email TEXT;",
                "ALTER TABLE Movies ADD COLUMN Rating TEXT DEFAULT '8.5/10';",
                "ALTER TABLE Movies ADD COLUMN AgeRating TEXT DEFAULT 'PG-13';",
                "ALTER TABLE Movies ADD COLUMN ReleaseDate TEXT;",
                "ALTER TABLE Bookings ADD COLUMN Status TEXT DEFAULT 'Confirmed';",
                "ALTER TABLE Bookings ADD COLUMN ReferenceCode TEXT;",
                "CREATE INDEX IF NOT EXISTS idx_shows_movieid ON Shows(MovieId);",
                "CREATE INDEX IF NOT EXISTS idx_bookings_showid ON Bookings(ShowId);",
                "CREATE INDEX IF NOT EXISTS idx_bookings_userid ON Bookings(UserId);"
            };

            foreach (var sql in migrations)
            {
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch
                {
                    // Column already exists or table already migrated; safely ignore
                }
            }
        }

        private static void SeedInitialData(SqliteConnection conn)
        {
            // 1. Seed Super Admin and Demo Customer if Users table is empty
            long userCount = 0;
            using (var countCmd = conn.CreateCommand())
            {
                countCmd.CommandText = "SELECT COUNT(*) FROM Users";
                userCount = (long)countCmd.ExecuteScalar();
            }

            long adminId = 1;

            if (userCount == 0)
            {
                string adminHash = BCrypt.Net.BCrypt.HashPassword("168168");
                string customerHash = BCrypt.Net.BCrypt.HashPassword("123456");

                using (var userCmd = conn.CreateCommand())
                {
                    userCmd.CommandText = @"
INSERT INTO Users (Phone, FullName, Email, PasswordHash, IsAdmin, CreatedAt)
VALUES ($p1, 'Super Administrator', 'admin@cineticket.com', $h1, 1, $t);

INSERT INTO Users (Phone, FullName, Email, PasswordHash, IsAdmin, CreatedAt)
VALUES ($p2, 'Alice Walker', 'alice.walker@example.com', $h2, 0, $t);
";
                    userCmd.Parameters.AddWithValue("$p1", "085909135");
                    userCmd.Parameters.AddWithValue("$h1", adminHash);
                    userCmd.Parameters.AddWithValue("$p2", "098765432");
                    userCmd.Parameters.AddWithValue("$h2", customerHash);
                    userCmd.Parameters.AddWithValue("$t", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    userCmd.ExecuteNonQuery();
                }
            }

            // Retrieve admin ID
            using (var getAdminCmd = conn.CreateCommand())
            {
                getAdminCmd.CommandText = "SELECT Id FROM Users WHERE Phone = '085909135' LIMIT 1";
                var result = getAdminCmd.ExecuteScalar();
                if (result != null) adminId = Convert.ToInt64(result);
            }

            // 2. Seed Movies and Shows if Movies table is empty
            long movieCount = 0;
            using (var countCmd = conn.CreateCommand())
            {
                countCmd.CommandText = "SELECT COUNT(*) FROM Movies";
                movieCount = (long)countCmd.ExecuteScalar();
            }

            if (movieCount == 0)
            {
                var movies = new[]
                {
                    new {
                        Title = "Neon Nights",
                        Genre = "Sci-Fi / Action",
                        Duration = 125,
                        Price = 14.50,
                        Rating = "8.9/10",
                        AgeRating = "PG-13",
                        Desc = "A neon-lit chase through a futuristic cyberpunk metropolis where memories can be bought and sold on the black market."
                    },
                    new {
                        Title = "The Last Composer",
                        Genre = "Drama / Music",
                        Duration = 98,
                        Price = 11.00,
                        Rating = "8.4/10",
                        AgeRating = "PG",
                        Desc = "An aging master composer discovers his passion rekindled when a mysterious young street prodigy arrives on his doorstep."
                    },
                    new {
                        Title = "Skybound Horizons",
                        Genre = "Adventure / Family",
                        Duration = 105,
                        Price = 12.50,
                        Rating = "8.1/10",
                        AgeRating = "G",
                        Desc = "Two daring siblings construct a backyard airship and discover a wondrous lost floating kingdom above the clouds."
                    },
                    new {
                        Title = "Midnight Bakery",
                        Genre = "Romance / Comedy",
                        Duration = 90,
                        Price = 10.00,
                        Rating = "7.8/10",
                        AgeRating = "PG",
                        Desc = "A dedicated night-shift baker and an eccentric, sleep-deprived programmer collide over warm croissants and broken algorithms."
                    },
                    new {
                        Title = "Galactic Odyssey",
                        Genre = "Sci-Fi / Space",
                        Duration = 140,
                        Price = 15.00,
                        Rating = "9.1/10",
                        AgeRating = "PG-13",
                        Desc = "A deep-space expedition encounters an enigmatic celestial artifact orbiting a dead star, changing humanity's destiny forever."
                    }
                };

                DateTime today = DateTime.Today;

                foreach (var m in movies)
                {
                    long movieId = 0;
                    using (var insCmd = conn.CreateCommand())
                    {
                        insCmd.CommandText = @"
INSERT INTO Movies (Title, Genre, DurationMinutes, Description, PosterPath, Price, Rating, AgeRating)
VALUES ($t, $g, $d, $desc, '', $pr, $rat, $age);
SELECT last_insert_rowid();
";
                        insCmd.Parameters.AddWithValue("$t", m.Title);
                        insCmd.Parameters.AddWithValue("$g", m.Genre);
                        insCmd.Parameters.AddWithValue("$d", m.Duration);
                        insCmd.Parameters.AddWithValue("$desc", m.Desc);
                        insCmd.Parameters.AddWithValue("$pr", m.Price);
                        insCmd.Parameters.AddWithValue("$rat", m.Rating);
                        insCmd.Parameters.AddWithValue("$age", m.AgeRating);
                        movieId = Convert.ToInt64(insCmd.ExecuteScalar());
                    }

                    // Create 3 shows for each movie
                    var showTimes = new[]
                    {
                        new { Time = today.AddHours(14).ToString("yyyy-MM-dd 14:00:00"), Hall = "Hall 1" },
                        new { Time = today.AddHours(18).AddMinutes(30).ToString("yyyy-MM-dd 18:30:00"), Hall = "Hall 1" },
                        new { Time = today.AddDays(1).AddHours(20).ToString("yyyy-MM-dd 20:00:00"), Hall = "Hall 2 (IMAX)" }
                    };

                    for (int sIdx = 0; sIdx < showTimes.Length; sIdx++)
                    {
                        var s = showTimes[sIdx];
                        long showId = 0;
                        using (var showCmd = conn.CreateCommand())
                        {
                            showCmd.CommandText = @"
INSERT INTO Shows (MovieId, ShowTime, HallName, TotalRows, TotalCols)
VALUES ($mid, $st, $hn, 6, 8);
SELECT last_insert_rowid();
";
                            showCmd.Parameters.AddWithValue("$mid", movieId);
                            showCmd.Parameters.AddWithValue("$st", s.Time);
                            showCmd.Parameters.AddWithValue("$hn", s.Hall);
                            showId = Convert.ToInt64(showCmd.ExecuteScalar());
                        }

                        // Seed sample pre-booked seats
                        if (sIdx == 0 && (movieId == 1 || movieId == 2))
                        {
                            using (var bCmd = conn.CreateCommand())
                            {
                                bCmd.CommandText = @"
INSERT INTO Bookings (UserId, ShowId, SeatCode, SeatRow, SeatCol, Price, BookingTime, Status, ReferenceCode)
VALUES ($uid, $sid, 'C4', 2, 3, $pr, $bt, 'Confirmed', '#CB-DEMO-001');
INSERT INTO Bookings (UserId, ShowId, SeatCode, SeatRow, SeatCol, Price, BookingTime, Status, ReferenceCode)
VALUES ($uid, $sid, 'C5', 2, 4, $pr, $bt, 'Confirmed', '#CB-DEMO-002');
";
                                bCmd.Parameters.AddWithValue("$uid", adminId);
                                bCmd.Parameters.AddWithValue("$sid", showId);
                                bCmd.Parameters.AddWithValue("$pr", m.Price);
                                bCmd.Parameters.AddWithValue("$bt", DateTime.Now.AddHours(-2).ToString("yyyy-MM-dd HH:mm:ss"));
                                bCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }
    }
}
