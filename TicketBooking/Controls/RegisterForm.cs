using System;
using System.Drawing;
using System.Windows.Forms;
using TicketBooking.Models;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class RegisterForm : Form
    {
        private TextBox txtFullName;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private TextBox txtPassword2;
        private CheckBox chkShowPass;
        private Button btnCreate;
        private Button btnCancel;
        private Label lblError;

        public string RegisteredPhone { get; private set; }

        public RegisterForm()
        {
            Text = "Ticket Booking - Create Account";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(420, 520);
            BackColor = Color.FromArgb(24, 28, 36);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            var lblTitle = new Label
            {
                Text = "Create New Account",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 180, 160),
                Location = new Point(28, 18),
                AutoSize = true
            };

            int top = 55;
            int inputW = 360;

            // Full Name
            var lblName = new Label { Text = "Full Name", ForeColor = Color.FromArgb(210, 215, 225), Location = new Point(28, top), AutoSize = true };
            top += 20;
            txtFullName = new TextBox { Location = new Point(30, top), Width = inputW, Font = new Font("Segoe UI", 10F), BackColor = Color.FromArgb(36, 42, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            top += 35;

            // Phone
            var lblPhone = new Label { Text = "Phone Number (at least 6 digits) *", ForeColor = Color.FromArgb(210, 215, 225), Location = new Point(28, top), AutoSize = true };
            top += 20;
            txtPhone = new TextBox { Location = new Point(30, top), Width = inputW, Font = new Font("Segoe UI", 10F), BackColor = Color.FromArgb(36, 42, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            top += 35;

            // Email
            var lblEmail = new Label { Text = "Email Address (optional)", ForeColor = Color.FromArgb(210, 215, 225), Location = new Point(28, top), AutoSize = true };
            top += 20;
            txtEmail = new TextBox { Location = new Point(30, top), Width = inputW, Font = new Font("Segoe UI", 10F), BackColor = Color.FromArgb(36, 42, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            top += 35;

            // Password
            var lblPass = new Label { Text = "Password (at least 4 characters) *", ForeColor = Color.FromArgb(210, 215, 225), Location = new Point(28, top), AutoSize = true };
            top += 20;
            txtPassword = new TextBox { Location = new Point(30, top), Width = inputW, Font = new Font("Segoe UI", 10F), BackColor = Color.FromArgb(36, 42, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true };
            top += 35;

            // Confirm Password
            var lblPass2 = new Label { Text = "Confirm Password *", ForeColor = Color.FromArgb(210, 215, 225), Location = new Point(28, top), AutoSize = true };
            top += 20;
            txtPassword2 = new TextBox { Location = new Point(30, top), Width = inputW, Font = new Font("Segoe UI", 10F), BackColor = Color.FromArgb(36, 42, 54), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true };
            top += 35;

            chkShowPass = new CheckBox
            {
                Text = "Show Passwords",
                ForeColor = Color.FromArgb(160, 170, 185),
                Location = new Point(30, top),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            chkShowPass.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPass.Checked;
                txtPassword2.UseSystemPasswordChar = !chkShowPass.Checked;
            };
            top += 26;

            lblError = new Label
            {
                ForeColor = Color.FromArgb(255, 99, 99),
                Location = new Point(30, top),
                Width = inputW,
                Height = 22,
                Visible = false
            };
            top += 26;

            btnCreate = new Button
            {
                Text = "Register Account",
                Location = new Point(30, top),
                Width = inputW,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Click += BtnCreate_Click;
            top += 44;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(30, top),
                Width = inputW,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(36, 42, 54),
                ForeColor = Color.FromArgb(180, 190, 205),
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(lblTitle);
            Controls.Add(lblName);
            Controls.Add(txtFullName);
            Controls.Add(lblPhone);
            Controls.Add(txtPhone);
            Controls.Add(lblEmail);
            Controls.Add(txtEmail);
            Controls.Add(lblPass);
            Controls.Add(txtPassword);
            Controls.Add(lblPass2);
            Controls.Add(txtPassword2);
            Controls.Add(chkShowPass);
            Controls.Add(lblError);
            Controls.Add(btnCreate);
            Controls.Add(btnCancel);

            AcceptButton = btnCreate;
            CancelButton = btnCancel;
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            var req = new RegisterRequest
            {
                FullName = txtFullName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Password = txtPassword.Text,
                ConfirmPassword = txtPassword2.Text,
                IsAdmin = false
            };

            var res = AuthService.Register(req);
            if (res.Success)
            {
                RegisteredPhone = req.Phone;
                MessageBox.Show($"Welcome, {req.FullName}! Your account has been created successfully. You can now log in.", "Registration Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblError.Text = res.ErrorMessage;
                lblError.Visible = true;
            }
        }
    }
}
