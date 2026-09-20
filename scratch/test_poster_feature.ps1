Add-Type -Path "c:\Users\daluch\source\repos\TicketBooking\TicketBooking\bin\Debug\Microsoft.Data.Sqlite.dll"
Add-Type -Path "c:\Users\daluch\source\repos\TicketBooking\TicketBooking\bin\Debug\BCrypt.Net-Next.dll"
[System.Reflection.Assembly]::LoadFrom("c:\Users\daluch\source\repos\TicketBooking\TicketBooking\bin\Debug\TicketBooking.exe") | Out-Null

Write-Host "=== TEST 1: Database Initialization ==="
[TicketBooking.Data.Database]::EnsureCreated()
Write-Host "Database OK."

Write-Host "`n=== TEST 2: ImageService Placeholder Generation ==="
$placeholder = [TicketBooking.Services.ImageService]::CreatePlaceholder("Inception", 95, 126)
if ($placeholder -eq $null -or $placeholder.Width -ne 95 -or $placeholder.Height -ne 126) {
    throw "Placeholder generation failed!"
}
Write-Host "Placeholder generated successfully: $($placeholder.Width)x$($placeholder.Height)"

Write-Host "`n=== TEST 3: Add Movie with ImageUrl / PosterPath ==="
$testTitle = "Cyberpunk Neo " + [Guid]::NewGuid().ToString().Substring(0, 5)
$posterUrl = "https://images.unsplash.com/photo-1536440136628-849c177e76a1"
$releaseDate = [DateTime]::Today.AddDays(-10)

$movieId = [TicketBooking.Services.MovieService]::AddMovieWithShow(
    $testTitle,
    "Action/Sci-Fi",
    140,
    "Futuristic sci-fi film",
    15.00,
    [DateTime]::Now.AddDays(1),
    "Hall 1",
    "9.2/10",
    "PG-13",
    $releaseDate,
    $posterUrl,
    6,
    8
)
Write-Host "Created movie ID: $movieId with Poster: $posterUrl"

Write-Host "`n=== TEST 4: Verify Movie Retrieval & ImageUrl Alias ==="
$movies = [TicketBooking.Services.MovieService]::GetMoviesWithShows()
$createdMovie = $null
foreach ($m in $movies) {
    if ($m.Id -eq $movieId) {
        $createdMovie = $m
        break
    }
}

if ($createdMovie -eq $null) {
    throw "Created movie was not found in catalog!"
}

Write-Host "Retrieved Movie Title: $($createdMovie.Title)"
Write-Host "Retrieved PosterPath: $($createdMovie.PosterPath)"
Write-Host "Retrieved ImageUrl:   $($createdMovie.ImageUrl)"

if ($createdMovie.PosterPath -ne $posterUrl) {
    throw "PosterPath mismatch! Expected: $posterUrl, Got: $($createdMovie.PosterPath)"
}
if ($createdMovie.ImageUrl -ne $posterUrl) {
    throw "ImageUrl alias mismatch! Expected: $posterUrl, Got: $($createdMovie.ImageUrl)"
}

Write-Host "`n=== TEST 5: Update Movie Poster ==="
$updatedPoster = "https://images.unsplash.com/photo-1518709268805-4e9042af9f23"
$createdMovie.ImageUrl = $updatedPoster
$updateResult = [TicketBooking.Services.MovieService]::UpdateMovie($createdMovie)
Write-Host "Update Result: $updateResult"
if (-not $updateResult) {
    throw "UpdateMovie failed!"
}

$moviesAfterUpdate = [TicketBooking.Services.MovieService]::GetMoviesWithShows()
$verifyUpdated = $null
foreach ($m in $moviesAfterUpdate) {
    if ($m.Id -eq $movieId) {
        $verifyUpdated = $m
        break
    }
}
Write-Host "Updated Movie PosterPath: $($verifyUpdated.PosterPath)"
if ($verifyUpdated.PosterPath -ne $updatedPoster) {
    throw "Updated PosterPath mismatch!"
}

Write-Host "`n=== TEST 6: WinForms Controls Instantiation ==="
$addForm = New-Object TicketBooking.Controls.AddMovieForm
if ($addForm -eq $null) { throw "AddMovieForm could not be instantiated" }
Write-Host "AddMovieForm instantiated successfully."

$editForm = New-Object TicketBooking.Controls.EditMovieForm($createdMovie)
if ($editForm -eq $null) { throw "EditMovieForm could not be instantiated" }
Write-Host "EditMovieForm instantiated successfully."

# Test AdminPortalForm instantiation
$adminAuth = [TicketBooking.Services.AuthService]::Login((New-Object TicketBooking.Models.LoginRequest -Property @{ Phone="085909135"; Password="168168" }))
[TicketBooking.ProgramState]::SetUser($adminAuth.User)
$adminPortal = New-Object TicketBooking.Controls.AdminPortalForm
if ($adminPortal -eq $null) { throw "AdminPortalForm could not be instantiated" }
Write-Host "AdminPortalForm instantiated successfully."

# Test Form1 instantiation (passing isChildView = $false)
$mainForm = New-Object TicketBooking.Form1($false)
if ($mainForm -eq $null) { throw "Form1 could not be instantiated" }
Write-Host "Form1 instantiated successfully."

Write-Host "`nALL TESTS PASSED SUCCESSFULLY!"
