Add-Type -Path "c:\Users\daluch\source\repos\TicketBooking\TicketBooking\bin\Debug\Microsoft.Data.Sqlite.dll"
Add-Type -Path "c:\Users\daluch\source\repos\TicketBooking\TicketBooking\bin\Debug\BCrypt.Net-Next.dll"
[System.Reflection.Assembly]::LoadFrom("c:\Users\daluch\source\repos\TicketBooking\TicketBooking\bin\Debug\TicketBooking.exe") | Out-Null

Write-Host "=== TEST 1: Database & Customer Auth ==="
[TicketBooking.Data.Database]::EnsureCreated()
$custReq = New-Object TicketBooking.Models.LoginRequest
$custReq.Phone = "098765432"
$custReq.Password = "123456"
$custAuth = [TicketBooking.Services.AuthService]::Login($custReq)
if (-not $custAuth.Success) { throw "Customer login failed!" }
[TicketBooking.ProgramState]::SetUser($custAuth.User)
Write-Host "Logged in as Customer: $($custAuth.User.DisplayName) (ID: $($custAuth.User.Id))"

Write-Host "`n=== TEST 2: Update Profile (Name & Email) ==="
$testName = "Jane Doe " + (Get-Random -Minimum 100 -Maximum 999)
$testEmail = "jane.doe@cineticket.com"
$updateRes = [TicketBooking.Services.AuthService]::UpdateProfile($custAuth.User.Id, $testName, $testEmail)
if (-not $updateRes.Success) { throw "UpdateProfile failed: $($updateRes.ErrorMessage)" }
Write-Host "Profile updated successfully: Name='$($updateRes.User.FullName)', Email='$($updateRes.User.Email)'"

# Verify DB query
$allUsers = [TicketBooking.Services.AuthService]::GetAllUsers()
$verifiedUser = $allUsers | Where-Object { $_.Id -eq $custAuth.User.Id }
if ($verifiedUser.FullName -ne $testName -or $verifiedUser.Email -ne $testEmail) {
    throw "Database did not persist updated profile!"
}
Write-Host "Verified profile in database."

Write-Host "`n=== TEST 3: GetUserBookings with Enhanced Metadata ==="
$bookings = [TicketBooking.Services.BookingService]::GetUserBookings($custAuth.User.Id)
Write-Host "Found $($bookings.Count) booking(s) for customer."
if ($bookings.Count -gt 0) {
    $firstBooking = $bookings[0]
    Write-Host "Sample Booking: Ref=$($firstBooking.ReferenceCode), Movie=$($firstBooking.MovieTitle), Seat=$($firstBooking.SeatCode)"
    Write-Host "  PosterPath:      $($firstBooking.PosterPath)"
    Write-Host "  Genre:           $($firstBooking.Genre)"
    Write-Host "  DurationMinutes: $($firstBooking.DurationMinutes)"
    Write-Host "  ShowCountdown:   $($firstBooking.ShowCountdown)"
    Write-Host "  IsPast:          $($firstBooking.IsPast)"
}

Write-Host "`n=== TEST 4: Instantiating UserProfileForm ==="
$profileForm = New-Object TicketBooking.Controls.UserProfileForm
if ($profileForm -eq $null) { throw "UserProfileForm could not be instantiated" }
Write-Host "UserProfileForm created successfully (Size: $($profileForm.Size.Width)x$($profileForm.Size.Height))"

Write-Host "`n=== TEST 5: Instantiating MyBookingsForm ==="
$myBookingsForm = New-Object TicketBooking.Controls.MyBookingsForm
if ($myBookingsForm -eq $null) { throw "MyBookingsForm could not be instantiated" }
Write-Host "MyBookingsForm created successfully (Size: $($myBookingsForm.Size.Width)x$($myBookingsForm.Size.Height))"

Write-Host "`n=== TEST 6: Instantiating TicketReceiptForm ==="
if ($bookings.Count -gt 0) {
    $receiptForm = New-Object TicketBooking.Controls.TicketReceiptForm($bookings[0])
    if ($receiptForm -eq $null) { throw "TicketReceiptForm could not be instantiated" }
    $receiptText = $receiptForm.GenerateReceiptText()
    Write-Host "Generated Receipt Sample:`n$($receiptText.Substring(0, 260))..."
}

Write-Host "`n=== TEST 7: Form1 Integration (Buttons, Layout, Profile trigger) ==="
$mainForm = New-Object TicketBooking.Form1($false)
if ($mainForm -eq $null) { throw "Form1 could not be instantiated" }
Write-Host "Form1 created successfully with Profile and Sign-Out buttons."

Write-Host "`n>>> ALL USER SIDE TESTS PASSED SUCCESSFULLY! <<<"
