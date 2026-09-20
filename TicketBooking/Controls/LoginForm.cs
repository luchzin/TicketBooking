using System;
using System.Drawing;
using System.Windows.Forms;
using TicketBooking.Models;
using TicketBooking.Services;

namespace TicketBooking.Controls
{
    public class LoginForm : Form
    {
        private TextBox txtPhone;
        private TextBox txtPassword;
        private CheckBox chkShowPass;
        private Button btnLogin;
        private Button btnRegister;
        private Button btnDemoAdmin;
        private Button btnDemoCustomer;
        private Label lblError;

        public LoginForm()
        {
            Text = "Ticket Booking - Sign In";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(420, 450);
            BackColor = Color.FromArgb(24, 28, 36);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            var lblBrand = new Label
            {
                Text = "🎬 CineBooking",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 180, 160),
                Location = new Point(28, 22),
                AutoSize = true
            };

            var lblSub = new Label
            {
                Text = "Sign in to book movies and manage tickets",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(160, 170, 185),
                Location = new Point(30, 56),
                AutoSize = true
            };

            var lblPhone = new Label
            {
                Text = "Phone Number",
                ForeColor = Color.FromArgb(210, 215, 225),
                Location = new Point(28, 90),
                AutoSize = true
            };

            txtPhone = new TextBox
            {
                Location = new Point(30, 112),
                Width = 360,
                Font = new Font("Segoe UI", 10.5F),
                BackColor = Color.FromArgb(36, 42, 54),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblPass = new Label
            {
                Text = "Password",
                ForeColor = Color.FromArgb(210, 215, 225),
                Location = new Point(28, 152),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(30, 174),
                Width = 360,
                Font = new Font("Segoe UI", 10.5F),
                BackColor = Color.FromArgb(36, 42, 54),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            chkShowPass = new CheckBox
            {
                Text = "Show Password",
                ForeColor = Color.FromArgb(160, 170, 185),
                Location = new Point(30, 208),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            chkShowPass.CheckedChanged += (s, e) => txtPassword.UseSystemPasswordChar = !chkShowPass.Checked;

            lblError = new Label
            {
                ForeColor = Color.FromArgb(255, 99, 99),
                Location = new Point(30, 234),
                Width = 360,
                Height = 20,
                Visible = false
            };

            btnLogin = new Button
            {
                Text = "Sign In",
                Location = new Point(30, 260),
                Width = 360,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            btnRegister = new Button
            {
                Text = "Don't have an account? Register",
                Location = new Point(30, 306),
                Width = 360,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(36, 42, 54),
                ForeColor = Color.FromArgb(180, 200, 225),
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;

            var lblDemo = new Label
            {
                Text = "⚡ Quick Demo Login:",
                ForeColor = Color.FromArgb(130, 140, 155),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                Location = new Point(30, 354),
                AutoSize = true
            };

            btnDemoAdmin = new Button
            {
                Text = "🔑 Super Admin (168168)",
                Location = new Point(30, 376),
                Width = 175,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 50, 45),
                ForeColor = Color.FromArgb(120, 220, 180),
                Font = new Font("Segoe UI", 8F),
                Cursor = Cursors.Hand
            };
            btnDemoAdmin.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 100);
            btnDemoAdmin.Click += (s, e) =>
            {
                txtPhone.Text = "085909135";
                txtPassword.Text = "168168";
                BtnLogin_Click(btnLogin, EventArgs.Empty);
            };

            btnDemoCustomer = new Button
            {
                Text = "👤 Customer (123456)",
                Location = new Point(215, 376),
                Width = 175,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 42, 60),
                ForeColor = Color.FromArgb(140, 180, 240),
                Font = new Font("Segoe UI", 8F),
                Cursor = Cursors.Hand
            };
            btnDemoCustomer.FlatAppearance.BorderColor = Color.FromArgb(60, 90, 140);
            btnDemoCustomer.Click += (s, e) =>
            {
                txtPhone.Text = "098765432";
                txtPassword.Text = "123456";
                BtnLogin_Click(btnLogin, EventArgs.Empty);
            };

            Controls.Add(lblBrand);
            Controls.Add(lblSub);
            Controls.Add(lblPhone);
            Controls.Add(txtPhone);
            Controls.Add(lblPass);
            Controls.Add(txtPassword);
            Controls.Add(chkShowPass);
            Controls.Add(lblError);
            Controls.Add(btnLogin);
            Controls.Add(btnRegister);
            Controls.Add(lblDemo);
            Controls.Add(btnDemoAdmin);
            Controls.Add(btnDemoCustomer);

            AcceptButton = btnLogin;
        }

        public void SetPrefilledPhone(string phone)
        {
            txtPhone.Text = phone;
            txtPassword.Focus();
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            using (var rf = new RegisterForm())
            {
                if (rf.ShowDialog(this) == DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(rf.RegisteredPhone))
                    {
                        txtPhone.Text = rf.RegisteredPhone;
                        txtPassword.Focus();
                        lblError.ForeColor = Color.FromArgb(120, 220, 180);
                        lblError.Text = "Account created successfully! Please enter password.";
                        lblError.Visible = true;
                    }
                }
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            var phone = txtPhone.Text.Trim();
            var pass = txtPassword.Text;

            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(pass))
            {
                lblError.ForeColor = Color.FromArgb(255, 99, 99);
                lblError.Text = "Please enter both phone and password.";
                lblError.Visible = true;
                return;
            }

            var res = AuthService.Login(new LoginRequest { Phone = phone, Password = pass });
            if (res.Success)
            {
                ProgramState.SetUser(res.User);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblError.ForeColor = Color.FromArgb(255, 99, 99);
                lblError.Text = res.ErrorMessage;
                lblError.Visible = true;
            }
        }
    }
}
