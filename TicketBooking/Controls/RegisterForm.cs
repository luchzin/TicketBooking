using System;
using System.Drawing;
using System.Windows.Forms;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class RegisterForm : Form
    {
        private TextBox txtPhone;
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
            ClientSize = new Size(400, 420);
            BackColor = Color.FromArgb(24, 28, 36);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            var lblTitle = new Label
            {
                Text = "Create New Account",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 180, 160),
                Location = new Point(28, 20),
                AutoSize = true
            };

            var lblPhone = new Label
            {
                Text = "Phone Number (at least 6 digits)",
                ForeColor = Color.FromArgb(210, 215, 225),
                Location = new Point(28, 65),
                AutoSize = true
            };

            txtPhone = new TextBox
            {
                Location = new Point(30, 88),
                Width = 340,
                Font = new Font("Segoe UI", 10.5F),
                BackColor = Color.FromArgb(36, 42, 54),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblPass = new Label
            {
                Text = "Password (at least 4 characters)",
                ForeColor = Color.FromArgb(210, 215, 225),
                Location = new Point(28, 128),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(30, 150),
                Width = 340,
                Font = new Font("Segoe UI", 10.5F),
                BackColor = Color.FromArgb(36, 42, 54),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            var lblPass2 = new Label
            {
                Text = "Confirm Password",
                ForeColor = Color.FromArgb(210, 215, 225),
                Location = new Point(28, 190),
                AutoSize = true
            };

            txtPassword2 = new TextBox
            {
                Location = new Point(30, 212),
                Width = 340,
                Font = new Font("Segoe UI", 10.5F),
                BackColor = Color.FromArgb(36, 42, 54),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            chkShowPass = new CheckBox
            {
                Text = "Show Passwords",
                ForeColor = Color.FromArgb(160, 170, 185),
                Location = new Point(30, 248),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            chkShowPass.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPass.Checked;
                txtPassword2.UseSystemPasswordChar = !chkShowPass.Checked;
            };

            lblError = new Label
            {
                ForeColor = Color.FromArgb(255, 99, 99),
                Location = new Point(30, 274),
                Width = 340,
                Height = 22,
                Visible = false
            };

            btnCreate = new Button
            {
                Text = "Register Account",
                Location = new Point(30, 302),
                Width = 340,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Click += BtnCreate_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(30, 348),
                Width = 340,
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
            Controls.Add(lblPhone);
            Controls.Add(txtPhone);
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
            var phone = txtPhone.Text.Trim();
            var p1 = txtPassword.Text;
            var p2 = txtPassword2.Text;

            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(p1))
            {
                lblError.Text = "Phone and password are required.";
                lblError.Visible = true;
                return;
            }
            if (p1 != p2)
            {
                lblError.Text = "Passwords do not match.";
                lblError.Visible = true;
                return;
            }

            string errMsg;
            var ok = AuthService.CreateUser(phone, p1, false, out errMsg);
            if (ok)
            {
                RegisteredPhone = phone;
                MessageBox.Show("Your account has been created successfully! You can now log in.", "Registration Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblError.Text = string.IsNullOrEmpty(errMsg) ? "Failed to create account." : errMsg;
                lblError.Visible = true;
            }
        }
    }
}
