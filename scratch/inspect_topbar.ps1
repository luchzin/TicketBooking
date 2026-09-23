Add-Type -Path "c:\Users\daluch\source\repos\TicketBooking\TicketBooking\bin\Debug\Microsoft.Data.Sqlite.dll"
Add-Type -Path "c:\Users\daluch\source\repos\TicketBooking\TicketBooking\bin\Debug\BCrypt.Net-Next.dll"
[System.Reflection.Assembly]::LoadFrom("c:\Users\daluch\source\repos\TicketBooking\TicketBooking\bin\Debug\TicketBooking.exe") | Out-Null

function Check-Form1-ForUser($roleName, $phone, $pass) {
    Write-Host "==================== TEST: $roleName ===================="
    $auth = [TicketBooking.Services.AuthService]::Login((New-Object TicketBooking.Models.LoginRequest -Property @{ Phone=$phone; Password=$pass }))
    if (-not $auth.Success) { throw "Login failed for $phone" }
    [TicketBooking.ProgramState]::SetUser($auth.User)

    $form = New-Object TicketBooking.Form1($false)
    $form.CreateControl()
    
    # Force layout execution
    $field = $form.GetType().GetField("topBarPanel", [System.Reflection.BindingFlags]"NonPublic,Instance")
    $topBar = $field.GetValue($form)

    Write-Host "topBarPanel Width: $($topBar.Width)"
    Write-Host "Controls in topBarPanel:"
    foreach ($c in $topBar.Controls) {
        Write-Host ("  {0,-16} | Text: '{1,-24}' | Visible: {2,-5} | Pos: ({3,4}, {4,2}) | Size: {5,3}x{6,2}" -f $c.Name, $c.Text, $c.Visible, $c.Location.X, $c.Location.Y, $c.Width, $c.Height)
    }

    $rField = $form.GetType().GetField("rightPanel", [System.Reflection.BindingFlags]"NonPublic,Instance")
    $rightPanel = $rField.GetValue($form)
    Write-Host "`nAdmin Detail Controls in rightPanel:"
    foreach ($c in @("btnEditMovieDetail", "btnDeleteMovieDetail", "btnAddShowDetail")) {
        $btn = $form.GetType().GetField($c, [System.Reflection.BindingFlags]"NonPublic,Instance").GetValue($form)
        if ($btn -ne $null) {
            Write-Host ("  {0,-20} | Text: '{1,-16}' | Visible: {2,-5} | Pos: ({3,4}, {4,2})" -f $c, $btn.Text, $btn.Visible, $btn.Location.X, $btn.Location.Y)
        }
    }
    Write-Host ""
}

[TicketBooking.Data.Database]::EnsureCreated()
Check-Form1-ForUser "REGULAR CUSTOMER" "098765432" "123456"
Check-Form1-ForUser "SUPER ADMINISTRATOR" "085909135" "168168"
