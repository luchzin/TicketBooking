using System;
using System.IO;
using Microsoft.Data.Sqlite;
using TicketBooking.Services;

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

            // 2. Seed / enrich full catalog of movies, posters, and showtimes
            SeedOrEnrichCatalog(conn, adminId);
        }

        public static void SeedOrEnrichCatalog(SqliteConnection conn = null, long adminId = 1)
        {
            bool closeConn = false;
            if (conn == null)
            {
                conn = GetConnection();
                closeConn = true;
            }

            try
            {
                // 1. Ensure all 16 poster artwork files are generated on disk in Posters/
                PosterService.EnsureAllPostersGenerated();

                // 2. Clean up legacy placeholder movies with empty posters or test records if present
                using (var cleanCmd = conn.CreateCommand())
                {
                    cleanCmd.CommandText = @"
DELETE FROM Movies 
WHERE (PosterPath IS NULL OR PosterPath = '') 
   OR Title LIKE 'Automated Test%' 
   OR Title = 'Interstellar Journey' 
   OR Title LIKE 'Cyberpunk Neo %'
   OR Title IN ('Neon Nights', 'The Last Composer', 'Skybound Horizons', 'Midnight Bakery', 'Galactic Odyssey');";
                    cleanCmd.ExecuteNonQuery();
                }

                // 3. Find admin user id if not provided
                if (adminId <= 0)
                {
                    using (var getAdminCmd = conn.CreateCommand())
                    {
                        getAdminCmd.CommandText = "SELECT Id FROM Users WHERE Phone = '085909135' LIMIT 1";
                        var result = getAdminCmd.ExecuteScalar();
                        if (result != null) adminId = Convert.ToInt64(result);
                    }
                }

                DateTime today = DateTime.Today;

                // 4. Seed or update all 16 rich movies in the catalog
                foreach (var m in PosterService.Catalog)
                {
                    string posterVal = !string.IsNullOrEmpty(m.PosterUrl) ? m.PosterUrl : ("Posters/" + m.PosterFileName);
                    long movieId = 0;

                    // Check if movie already exists by Title
                    using (var checkCmd = conn.CreateCommand())
                    {
                        checkCmd.CommandText = "SELECT Id FROM Movies WHERE Title = $t LIMIT 1";
                        checkCmd.Parameters.AddWithValue("$t", m.Title);
                        var existing = checkCmd.ExecuteScalar();
                        if (existing != null)
                        {
                            movieId = Convert.ToInt64(existing);
                            // Update existing movie with enriched description, poster, ratings
                            using (var updateCmd = conn.CreateCommand())
                            {
                                updateCmd.CommandText = @"
UPDATE Movies 
SET Genre = $g, DurationMinutes = $dur, Description = $desc, PosterPath = $post, 
    Price = $pr, Rating = $rat, AgeRating = $age, ReleaseDate = $rd 
WHERE Id = $id";
                                updateCmd.Parameters.AddWithValue("$g", m.Genre);
                                updateCmd.Parameters.AddWithValue("$dur", m.DurationMinutes);
                                updateCmd.Parameters.AddWithValue("$desc", m.Description);
                                updateCmd.Parameters.AddWithValue("$post", posterVal);
                                updateCmd.Parameters.AddWithValue("$pr", m.Price);
                                updateCmd.Parameters.AddWithValue("$rat", m.Rating);
                                updateCmd.Parameters.AddWithValue("$age", m.AgeRating);
                                updateCmd.Parameters.AddWithValue("$rd", m.ReleaseDate);
                                updateCmd.Parameters.AddWithValue("$id", movieId);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Insert new movie
                            using (var insCmd = conn.CreateCommand())
                            {
                                insCmd.CommandText = @"
INSERT INTO Movies (Title, Genre, DurationMinutes, Description, PosterPath, Price, Rating, AgeRating, ReleaseDate)
VALUES ($t, $g, $dur, $desc, $post, $pr, $rat, $age, $rd);
SELECT last_insert_rowid();";
                                insCmd.Parameters.AddWithValue("$t", m.Title);
                                insCmd.Parameters.AddWithValue("$g", m.Genre);
                                insCmd.Parameters.AddWithValue("$dur", m.DurationMinutes);
                                insCmd.Parameters.AddWithValue("$desc", m.Description);
                                insCmd.Parameters.AddWithValue("$post", posterVal);
                                insCmd.Parameters.AddWithValue("$pr", m.Price);
                                insCmd.Parameters.AddWithValue("$rat", m.Rating);
                                insCmd.Parameters.AddWithValue("$age", m.AgeRating);
                                insCmd.Parameters.AddWithValue("$rd", m.ReleaseDate);
                                movieId = Convert.ToInt64(insCmd.ExecuteScalar());
                            }
                        }
                    }

                    // 5. Ensure shows exist for this movie
                    long showCount = 0;
                    using (var showCountCmd = conn.CreateCommand())
                    {
                        showCountCmd.CommandText = "SELECT COUNT(*) FROM Shows WHERE MovieId = $mid";
                        showCountCmd.Parameters.AddWithValue("$mid", movieId);
                        showCount = (long)showCountCmd.ExecuteScalar();
                    }

                    if (showCount < 3)
                    {
                        var showSchedule = new[]
                        {
                            new { Time = today.AddHours(14).ToString("yyyy-MM-dd 14:00:00"), Hall = "Hall 1 (Standard)" },
                            new { Time = today.AddHours(17).AddMinutes(30).ToString("yyyy-MM-dd 17:30:00"), Hall = "Hall 2 (Dolby Atmos)" },
                            new { Time = today.AddHours(20).AddMinutes(45).ToString("yyyy-MM-dd 20:45:00"), Hall = "Hall 3 (IMAX Laser)" },
                            new { Time = today.AddDays(1).AddHours(15).AddMinutes(15).ToString("yyyy-MM-dd 15:15:00"), Hall = "Hall 1 (Standard)" },
                            new { Time = today.AddDays(1).AddHours(19).ToString("yyyy-MM-dd 19:00:00"), Hall = "Hall 3 (IMAX Laser)" }
                        };

                        for (int sIdx = 0; sIdx < showSchedule.Length; sIdx++)
                        {
                            var s = showSchedule[sIdx];
                            long showId = 0;
                            using (var insShowCmd = conn.CreateCommand())
                            {
                                insShowCmd.CommandText = @"
INSERT INTO Shows (MovieId, ShowTime, HallName, TotalRows, TotalCols)
VALUES ($mid, $st, $hn, 6, 8);
SELECT last_insert_rowid();";
                                insShowCmd.Parameters.AddWithValue("$mid", movieId);
                                insShowCmd.Parameters.AddWithValue("$st", s.Time);
                                insShowCmd.Parameters.AddWithValue("$hn", s.Hall);
                                showId = Convert.ToInt64(insShowCmd.ExecuteScalar());
                            }

                            // Pre-book sample seats for prime evening shows on the first 3 movies
                            if (sIdx == 1 && movieId <= 3)
                            {
                                using (var bCmd = conn.CreateCommand())
                                {
                                    bCmd.CommandText = @"
INSERT INTO Bookings (UserId, ShowId, SeatCode, SeatRow, SeatCol, Price, BookingTime, Status, ReferenceCode)
VALUES ($uid, $sid, 'C4', 2, 3, $pr, $bt, 'Confirmed', $ref1);
INSERT INTO Bookings (UserId, ShowId, SeatCode, SeatRow, SeatCol, Price, BookingTime, Status, ReferenceCode)
VALUES ($uid, $sid, 'C5', 2, 4, $pr, $bt, 'Confirmed', $ref2);";
                                    bCmd.Parameters.AddWithValue("$uid", adminId);
                                    bCmd.Parameters.AddWithValue("$sid", showId);
                                    bCmd.Parameters.AddWithValue("$pr", m.Price);
                                    bCmd.Parameters.AddWithValue("$bt", DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                                    bCmd.Parameters.AddWithValue("$ref1", $"#CB-{movieId:D2}01");
                                    bCmd.Parameters.AddWithValue("$ref2", $"#CB-{movieId:D2}02");
                                    bCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (closeConn) conn.Dispose();
            }
        }
    }
}
