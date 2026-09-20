using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using TicketBooking.Models;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class UserProfileForm : Form
    {
        private readonly User _user;
        private TextBox txtFullName;
        private TextBox txtEmail;
        private Label lblPhoneVal;
        private Label lblMemberSinceVal;
        private Label lblRoleBadge;

        // Stats Labels
        private Label lblTotalTickets;
        private Label lblActiveTickets;
        private Label lblTotalSpent;
        private Label lblMembershipTier;

        // Password Fields
        private TextBox txtOldPassword;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;

        public bool RequestedSignOut { get; private set; } = false;

        public UserProfileForm()
        {
            _user = ProgramState.CurrentUser ?? new User();

            Text = "👤 My CineTicket Account & Profile";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(680, 680);
            BackColor = Color.FromArgb(20, 24, 34);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            InitializeProfileUi();
            LoadUserStats();
        }

        private void InitializeProfileUi()
        {
            // Top Header Card
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = Color.FromArgb(26, 32, 46),
                Padding = new Padding(24, 16, 24, 16)
            };

            // Avatar circle with user initials
            var pnlAvatar = new Panel
            {
                Location = new Point(24, 16),
                Size = new Size(76, 76),
                BackColor = Color.FromArgb(0, 160, 140)
            };
            pnlAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, 75, 75);
                    pnlAvatar.Region = new Region(path);
                }
                string initials = GetUserInitials();
                using (var font = new Font("Segoe UI", 20F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    e.Graphics.DrawString(initials, font, brush, new RectangleF(0, 0, 76, 76), sf);
                }
            };
            pnlHeader.Controls.Add(pnlAvatar);

            var lblHeaderName = new Label
            {
                Text = string.IsNullOrWhiteSpace(_user.FullName) ? "Valued CineTicket Member" : _user.FullName,
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(115, 18),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblHeaderName);

            var lblHeaderPhone = new Label
            {
                Text = $"📱 {_user.Phone}  •  Member ID: #{_user.Id}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(170, 185, 205),
                Location = new Point(117, 48),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblHeaderPhone);

            lblRoleBadge = new Label
            {
                Text = _user.IsAdmin ? "👑 System Administrator" : "🎬 CineTicket Member",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = _user.IsAdmin ? Color.FromArgb(255, 200, 70) : Color.FromArgb(0, 220, 190),
                BackColor = Color.FromArgb(36, 44, 62),
                Padding = new Padding(8, 3, 8, 3),
                Location = new Point(117, 72),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblRoleBadge);

            Controls.Add(pnlHeader);

            // ================= STATS SUMMARY CARDS =================
            var pnlStats = new Panel
            {
                Location = new Point(20, 122),
                Size = new Size(640, 72),
                BackColor = Color.Transparent
            };

            int cardW = 152;
            int cardGap = 10;

            lblTotalTickets = CreateStatCard(pnlStats, "🎟️ TICKETS", "0", Color.FromArgb(100, 190, 255), 0, cardW);
            lblActiveTickets = CreateStatCard(pnlStats, "🟢 UPCOMING", "0", Color.FromArgb(0, 220, 180), cardW + cardGap, cardW);
            lblTotalSpent = CreateStatCard(pnlStats, "💰 TOTAL SPENT", "$0.00", Color.FromArgb(255, 210, 80), (cardW + cardGap) * 2, cardW);
            lblMembershipTier = CreateStatCard(pnlStats, "⭐ MEMBERSHIP", "Standard", Color.FromArgb(220, 150, 255), (cardW + cardGap) * 3, cardW);

            Controls.Add(pnlStats);

            // ================= PROFILE DETAILS GROUP =================
            var grpProfile = new GroupBox
            {
                Text = "  Identity & Account Details  ",
                Location = new Point(20, 204),
                Size = new Size(640, 175),
                ForeColor = Color.FromArgb(160, 185, 215),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            // Full Name
            var lblName = new Label { Text = "Full Name:", Location = new Point(24, 32), AutoSize = true, ForeColor = Color.FromArgb(200, 215, 235), Font = new Font("Segoe UI", 9F) };
            txtFullName = new TextBox
            {
                Text = _user.FullName ?? "",
                Location = new Point(130, 28),
                Size = new Size(260, 26),
                BackColor = Color.FromArgb(32, 40, 54),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5F)
            };
            grpProfile.Controls.Add(lblName);
            grpProfile.Controls.Add(txtFullName);

            // Phone
            var lblPhone = new Label { Text = "Phone Number:", Location = new Point(24, 68), AutoSize = true, ForeColor = Color.FromArgb(200, 215, 235), Font = new Font("Segoe UI", 9F) };
            lblPhoneVal = new Label { Text = _user.Phone ?? "N/A", Location = new Point(130, 68), AutoSize = true, ForeColor = Color.FromArgb(255, 200, 80), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            grpProfile.Controls.Add(lblPhone);
            grpProfile.Controls.Add(lblPhoneVal);

            // Email
            var lblEmail = new Label { Text = "Email Address:", Location = new Point(24, 102), AutoSize = true, ForeColor = Color.FromArgb(200, 215, 235), Font = new Font("Segoe UI", 9F) };
            txtEmail = new TextBox
            {
                Text = _user.Email ?? "",
                Location = new Point(130, 98),
                Size = new Size(260, 26),
                BackColor = Color.FromArgb(32, 40, 54),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5F)
            };
            grpProfile.Controls.Add(lblEmail);
            grpProfile.Controls.Add(txtEmail);

            // Member Since
            var lblMemberSince = new Label { Text = "Member Since:", Location = new Point(24, 138), AutoSize = true, ForeColor = Color.FromArgb(200, 215, 235), Font = new Font("Segoe UI", 9F) };
            lblMemberSinceVal = new Label { Text = _user.CreatedAt.ToString("MMMM d, yyyy"), Location = new Point(130, 138), AutoSize = true, ForeColor = Color.FromArgb(170, 190, 215), Font = new Font("Segoe UI", 9F) };
            grpProfile.Controls.Add(lblMemberSince);
            grpProfile.Controls.Add(lblMemberSinceVal);

            // Save Profile Button
            var btnSaveProfile = new Button
            {
                Text = "💾 Save Details",
                Location = new Point(430, 45),
                Size = new Size(180, 42),
                BackColor = Color.FromArgb(0, 160, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSaveProfile.FlatAppearance.BorderSize = 0;
            btnSaveProfile.Click += BtnSaveProfile_Click;
            grpProfile.Controls.Add(btnSaveProfile);

            Controls.Add(grpProfile);

            // ================= CHANGE PASSWORD GROUP =================
            var grpSecurity = new GroupBox
            {
                Text = "  Security & Password  ",
                Location = new Point(20, 390),
                Size = new Size(640, 150),
                ForeColor = Color.FromArgb(160, 185, 215),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var lblOldPass = new Label { Text = "Current Password:", Location = new Point(24, 32), AutoSize = true, ForeColor = Color.FromArgb(200, 215, 235), Font = new Font("Segoe UI", 9F) };
            txtOldPassword = new TextBox { Location = new Point(155, 28), Size = new Size(220, 25), UseSystemPasswordChar = true, BackColor = Color.FromArgb(32, 40, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            grpSecurity.Controls.Add(lblOldPass);
            grpSecurity.Controls.Add(txtOldPassword);

            var lblNewPass = new Label { Text = "New Password:", Location = new Point(24, 68), AutoSize = true, ForeColor = Color.FromArgb(200, 215, 235), Font = new Font("Segoe UI", 9F) };
            txtNewPassword = new TextBox { Location = new Point(155, 64), Size = new Size(220, 25), UseSystemPasswordChar = true, BackColor = Color.FromArgb(32, 40, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            grpSecurity.Controls.Add(lblNewPass);
            grpSecurity.Controls.Add(txtNewPassword);

            var lblConfirmPass = new Label { Text = "Confirm New:", Location = new Point(24, 104), AutoSize = true, ForeColor = Color.FromArgb(200, 215, 235), Font = new Font("Segoe UI", 9F) };
            txtConfirmPassword = new TextBox { Location = new Point(155, 100), Size = new Size(220, 25), UseSystemPasswordChar = true, BackColor = Color.FromArgb(32, 40, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            grpSecurity.Controls.Add(lblConfirmPass);
            grpSecurity.Controls.Add(txtConfirmPassword);

            var btnChangePass = new Button
            {
                Text = "🔑 Update Password",
                Location = new Point(410, 55),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(46, 58, 78),
                ForeColor = Color.FromArgb(190, 220, 255),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnChangePass.FlatAppearance.BorderSize = 0;
            btnChangePass.Click += BtnChangePass_Click;
            grpSecurity.Controls.Add(btnChangePass);

            Controls.Add(grpSecurity);

            // ================= BOTTOM ACTIONS (OPT OUT TO SIGN IN / CLOSE) =================
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.FromArgb(24, 28, 38),
                Padding = new Padding(20, 16, 20, 16)
            };

            var btnSignOut = new Button
            {
                Text = "🚪 Sign Out / Switch Account",
                Location = new Point(20, 20),
                Size = new Size(240, 42),
                BackColor = Color.FromArgb(70, 32, 38),
                ForeColor = Color.FromArgb(255, 160, 160),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSignOut.FlatAppearance.BorderColor = Color.FromArgb(160, 60, 70);
            btnSignOut.Click += BtnSignOut_Click;
            pnlBottom.Controls.Add(btnSignOut);

            var btnClose = new Button
            {
                Text = "Close Profile",
                Location = new Point(520, 20),
                Size = new Size(140, 42),
                BackColor = Color.FromArgb(44, 52, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();
            pnlBottom.Controls.Add(btnClose);

            Controls.Add(pnlBottom);
            CancelButton = btnClose;
        }

        private Label CreateStatCard(Panel container, string title, string initialValue, Color valueColor, int x, int width)
        {
            var pnl = new Panel
            {
                Location = new Point(x, 0),
                Size = new Size(width, 70),
                BackColor = Color.FromArgb(28, 34, 48)
            };

            var lblT = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(130, 150, 175),
                Location = new Point(10, 10),
                AutoSize = true
            };
            var lblV = new Label
            {
                Text = initialValue,
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = valueColor,
                Location = new Point(10, 30),
                AutoSize = true
            };
            pnl.Controls.Add(lblT);
            pnl.Controls.Add(lblV);
            container.Controls.Add(pnl);
            return lblV;
        }

        private string GetUserInitials()
        {
            if (!string.IsNullOrWhiteSpace(_user.FullName))
            {
                var parts = _user.FullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2) return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[parts.Length - 1][0])}";
                if (parts.Length == 1 && parts[0].Length > 0) return char.ToUpper(parts[0][0]).ToString();
            }
            if (!string.IsNullOrWhiteSpace(_user.Phone) && _user.Phone.Length >= 2)
            {
                return _user.Phone.Substring(_user.Phone.Length - 2);
            }
            return "U";
        }

        private void LoadUserStats()
        {
            try
            {
                var bookings = BookingService.GetUserBookings(_user.Id);
                int totalTickets = bookings.Count;
                int upcoming = bookings.Count(b => b.IsActive && !b.IsPast);
                decimal totalSpent = bookings.Where(b => b.IsActive).Sum(b => b.Price);

                lblTotalTickets.Text = totalTickets.ToString();
                lblActiveTickets.Text = upcoming.ToString();
                lblTotalSpent.Text = $"${totalSpent:F2}";

                if (totalTickets >= 8)
                {
                    lblMembershipTier.Text = "Gold VIP 👑";
                    lblMembershipTier.ForeColor = Color.FromArgb(255, 215, 80);
                }
                else if (totalTickets >= 3)
                {
                    lblMembershipTier.Text = "Silver Pass ⭐";
                    lblMembershipTier.ForeColor = Color.FromArgb(170, 220, 255);
                }
                else
                {
                    lblMembershipTier.Text = "Standard 🎟️";
                    lblMembershipTier.ForeColor = Color.FromArgb(200, 210, 225);
                }
            }
            catch
            {
                // Fallback gracefully if database lookup fails
            }
        }

        private void BtnSaveProfile_Click(object sender, EventArgs e)
        {
            string newName = txtFullName.Text?.Trim() ?? "";
            string newEmail = txtEmail.Text?.Trim() ?? "";

            var res = AuthService.UpdateProfile(_user.Id, newName, newEmail);
            if (res.Success)
            {
                MessageBox.Show("Your profile details have been saved successfully!", "Profile Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(res.ErrorMessage, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnChangePass_Click(object sender, EventArgs e)
        {
            string oldPass = txtOldPassword.Text;
            string newPass = txtNewPassword.Text;
            string confPass = txtConfirmPassword.Text;

            if (string.IsNullOrEmpty(oldPass))
            {
                MessageBox.Show("Please enter your current password.", "Password Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(newPass) || newPass.Length < 4)
            {
                MessageBox.Show("New password must be at least 4 characters.", "Password Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (newPass != confPass)
            {
                MessageBox.Show("New passwords do not match.", "Password Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var res = AuthService.ChangePassword(_user.Id, oldPass, newPass);
            if (res.Success)
            {
                MessageBox.Show("Your password has been changed successfully.", "Password Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtOldPassword.Clear();
                txtNewPassword.Clear();
                txtConfirmPassword.Clear();
            }
            else
            {
                MessageBox.Show(res.ErrorMessage, "Change Password Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSignOut_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "Are you sure you want to sign out?\n\nYou will be returned to the sign-in screen to log into another account.",
                "Confirm Sign Out",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                RequestedSignOut = true;
                DialogResult = DialogResult.Abort;
                Close();
            }
        }
    }
}
