namespace TicketBooking
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private System.Windows.Forms.Panel topBarPanel;
        private System.Windows.Forms.Label lblBrand;
        private System.Windows.Forms.Label lblUserInfo;
        private System.Windows.Forms.Button btnAddMovie;
        private System.Windows.Forms.Button btnMyBookings;
        private System.Windows.Forms.Button btnLogout;

        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.Panel leftHeaderPanel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblMoviesHeader;
        private System.Windows.Forms.FlowLayoutPanel flowMovies;

        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblMeta;
        private System.Windows.Forms.Label lblPriceBadge;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblShowPrompt;
        private System.Windows.Forms.ComboBox cbShows;

        private System.Windows.Forms.Panel pnlScreen;
        private System.Windows.Forms.Label lblScreen;
        private System.Windows.Forms.Panel pnlLegend;

        private System.Windows.Forms.Panel pnlSeats;
        private System.Windows.Forms.Panel checkoutPanel;
        private System.Windows.Forms.Label lblSelectedSeats;
        private System.Windows.Forms.Label lblTotalPrice;
        private System.Windows.Forms.Button btnPurchase;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1080, 700);
            this.MinimumSize = new System.Drawing.Size(980, 640);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CineTicket - Modern Movie Booking System";
            this.BackColor = System.Drawing.Color.FromArgb(16, 20, 26);

            // topBarPanel
            this.topBarPanel = new System.Windows.Forms.Panel();
            this.topBarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topBarPanel.Height = 56;
            this.topBarPanel.BackColor = System.Drawing.Color.FromArgb(20, 25, 33);
            this.topBarPanel.Padding = new System.Windows.Forms.Padding(16, 0, 16, 0);

            this.lblBrand = new System.Windows.Forms.Label();
            this.lblBrand.Text = "🎬 CineTicket";
            this.lblBrand.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblBrand.ForeColor = System.Drawing.Color.FromArgb(0, 200, 180);
            this.lblBrand.Location = new System.Drawing.Point(16, 14);
            this.lblBrand.AutoSize = true;

            this.btnLogout = new System.Windows.Forms.Button();
            this.btnLogout.Text = "Sign Out";
            this.btnLogout.Size = new System.Drawing.Size(84, 32);
            this.btnLogout.Location = new System.Drawing.Point(970, 12);
            this.btnLogout.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.BackColor = System.Drawing.Color.FromArgb(44, 52, 66);
            this.btnLogout.ForeColor = System.Drawing.Color.FromArgb(220, 225, 235);
            this.btnLogout.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogout.FlatAppearance.BorderSize = 0;

            this.btnMyBookings = new System.Windows.Forms.Button();
            this.btnMyBookings.Text = "🎟️ My Bookings";
            this.btnMyBookings.Size = new System.Drawing.Size(115, 32);
            this.btnMyBookings.Location = new System.Drawing.Point(845, 12);
            this.btnMyBookings.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnMyBookings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMyBookings.BackColor = System.Drawing.Color.FromArgb(36, 48, 64);
            this.btnMyBookings.ForeColor = System.Drawing.Color.FromArgb(140, 200, 255);
            this.btnMyBookings.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnMyBookings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMyBookings.FlatAppearance.BorderSize = 0;

            this.btnAddMovie = new System.Windows.Forms.Button();
            this.btnAddMovie.Text = "+ Add Movie";
            this.btnAddMovie.Size = new System.Drawing.Size(105, 32);
            this.btnAddMovie.Location = new System.Drawing.Point(730, 12);
            this.btnAddMovie.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnAddMovie.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddMovie.BackColor = System.Drawing.Color.FromArgb(25, 75, 60);
            this.btnAddMovie.ForeColor = System.Drawing.Color.FromArgb(100, 230, 190);
            this.btnAddMovie.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnAddMovie.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddMovie.FlatAppearance.BorderSize = 0;
            this.btnAddMovie.Visible = false;

            this.lblUserInfo = new System.Windows.Forms.Label();
            this.lblUserInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.lblUserInfo.ForeColor = System.Drawing.Color.FromArgb(180, 190, 205);
            this.lblUserInfo.Location = new System.Drawing.Point(450, 18);
            this.lblUserInfo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblUserInfo.AutoSize = true;

            this.topBarPanel.Controls.Add(this.lblBrand);
            this.topBarPanel.Controls.Add(this.lblUserInfo);
            this.topBarPanel.Controls.Add(this.btnAddMovie);
            this.topBarPanel.Controls.Add(this.btnMyBookings);
            this.topBarPanel.Controls.Add(this.btnLogout);

            // leftPanel
            this.leftPanel = new System.Windows.Forms.Panel();
            this.leftPanel.BackColor = System.Drawing.Color.FromArgb(25, 30, 39);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPanel.Width = 345;
            this.leftPanel.Padding = new System.Windows.Forms.Padding(10);

            // leftHeaderPanel
            this.leftHeaderPanel = new System.Windows.Forms.Panel();
            this.leftHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.leftHeaderPanel.Height = 70;
            this.leftHeaderPanel.BackColor = System.Drawing.Color.Transparent;

            this.lblMoviesHeader = new System.Windows.Forms.Label();
            this.lblMoviesHeader.Text = "NOW SHOWING";
            this.lblMoviesHeader.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblMoviesHeader.ForeColor = System.Drawing.Color.FromArgb(130, 145, 165);
            this.lblMoviesHeader.Location = new System.Drawing.Point(4, 6);
            this.lblMoviesHeader.AutoSize = true;

            this.txtSearch = new System.Windows.Forms.TextBox();
            this.txtSearch.Location = new System.Drawing.Point(4, 30);
            this.txtSearch.Width = 317;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtSearch.BackColor = System.Drawing.Color.FromArgb(36, 44, 56);
            this.txtSearch.ForeColor = System.Drawing.Color.FromArgb(220, 225, 235);
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            this.leftHeaderPanel.Controls.Add(this.lblMoviesHeader);
            this.leftHeaderPanel.Controls.Add(this.txtSearch);

            // flowMovies
            this.flowMovies = new System.Windows.Forms.FlowLayoutPanel();
            this.flowMovies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowMovies.AutoScroll = true;
            this.flowMovies.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowMovies.WrapContents = false;
            this.flowMovies.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);

            this.leftPanel.Controls.Add(this.flowMovies);
            this.leftPanel.Controls.Add(this.leftHeaderPanel);

            // rightPanel
            this.rightPanel = new System.Windows.Forms.Panel();
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.BackColor = System.Drawing.Color.FromArgb(16, 20, 26);
            this.rightPanel.AutoScroll = true;
            this.rightPanel.Padding = new System.Windows.Forms.Padding(24, 16, 24, 16);

            // lblTitle
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 17F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(24, 14);
            this.lblTitle.AutoSize = true;

            // lblPriceBadge
            this.lblPriceBadge = new System.Windows.Forms.Label();
            this.lblPriceBadge.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblPriceBadge.ForeColor = System.Drawing.Color.FromArgb(0, 220, 180);
            this.lblPriceBadge.Location = new System.Drawing.Point(26, 48);
            this.lblPriceBadge.AutoSize = true;

            // lblMeta
            this.lblMeta = new System.Windows.Forms.Label();
            this.lblMeta.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.lblMeta.ForeColor = System.Drawing.Color.FromArgb(160, 175, 195);
            this.lblMeta.Location = new System.Drawing.Point(26, 72);
            this.lblMeta.AutoSize = true;

            // lblDescription
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblDescription.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblDescription.ForeColor = System.Drawing.Color.FromArgb(145, 155, 170);
            this.lblDescription.Location = new System.Drawing.Point(26, 96);
            this.lblDescription.MaximumSize = new System.Drawing.Size(660, 48);
            this.lblDescription.AutoSize = true;

            // lblShowPrompt
            this.lblShowPrompt = new System.Windows.Forms.Label();
            this.lblShowPrompt.Text = "SELECT SHOWTIME";
            this.lblShowPrompt.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblShowPrompt.ForeColor = System.Drawing.Color.FromArgb(130, 145, 165);
            this.lblShowPrompt.Location = new System.Drawing.Point(26, 145);
            this.lblShowPrompt.AutoSize = true;

            // cbShows
            this.cbShows = new System.Windows.Forms.ComboBox();
            this.cbShows.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbShows.Location = new System.Drawing.Point(26, 165);
            this.cbShows.Width = 320;
            this.cbShows.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cbShows.BackColor = System.Drawing.Color.FromArgb(32, 40, 52);
            this.cbShows.ForeColor = System.Drawing.Color.White;

            // pnlScreen
            this.pnlScreen = new System.Windows.Forms.Panel();
            this.pnlScreen.Location = new System.Drawing.Point(26, 210);
            this.pnlScreen.Size = new System.Drawing.Size(560, 32);
            this.pnlScreen.BackColor = System.Drawing.Color.FromArgb(28, 36, 48);

            this.lblScreen = new System.Windows.Forms.Label();
            this.lblScreen.Text = "——————   C I N E M A   S C R E E N   ——————";
            this.lblScreen.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblScreen.ForeColor = System.Drawing.Color.FromArgb(0, 190, 170);
            this.lblScreen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblScreen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.pnlScreen.Controls.Add(this.lblScreen);

            // pnlLegend
            this.pnlLegend = new System.Windows.Forms.Panel();
            this.pnlLegend.Location = new System.Drawing.Point(26, 248);
            this.pnlLegend.Size = new System.Drawing.Size(560, 24);
            this.pnlLegend.BackColor = System.Drawing.Color.Transparent;

            // pnlSeats
            this.pnlSeats = new System.Windows.Forms.Panel();
            this.pnlSeats.Location = new System.Drawing.Point(26, 280);
            this.pnlSeats.Size = new System.Drawing.Size(560, 270);
            this.pnlSeats.BackColor = System.Drawing.Color.FromArgb(20, 25, 33);
            this.pnlSeats.Padding = new System.Windows.Forms.Padding(12);

            // checkoutPanel
            this.checkoutPanel = new System.Windows.Forms.Panel();
            this.checkoutPanel.Location = new System.Drawing.Point(26, 560);
            this.checkoutPanel.Size = new System.Drawing.Size(650, 60);
            this.checkoutPanel.BackColor = System.Drawing.Color.FromArgb(24, 30, 40);

            this.lblSelectedSeats = new System.Windows.Forms.Label();
            this.lblSelectedSeats.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            this.lblSelectedSeats.ForeColor = System.Drawing.Color.FromArgb(200, 210, 225);
            this.lblSelectedSeats.Location = new System.Drawing.Point(14, 10);
            this.lblSelectedSeats.Size = new System.Drawing.Size(380, 20);
            this.lblSelectedSeats.Text = "Selected Seats: None";

            this.lblTotalPrice = new System.Windows.Forms.Label();
            this.lblTotalPrice.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTotalPrice.ForeColor = System.Drawing.Color.FromArgb(0, 210, 180);
            this.lblTotalPrice.Location = new System.Drawing.Point(14, 30);
            this.lblTotalPrice.Size = new System.Drawing.Size(380, 24);
            this.lblTotalPrice.Text = "Total: $0.00";

            this.btnPurchase = new System.Windows.Forms.Button();
            this.btnPurchase.Text = "🎟️ Confirm Booking";
            this.btnPurchase.BackColor = System.Drawing.Color.FromArgb(0, 150, 136);
            this.btnPurchase.ForeColor = System.Drawing.Color.White;
            this.btnPurchase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPurchase.FlatAppearance.BorderSize = 0;
            this.btnPurchase.Size = new System.Drawing.Size(180, 42);
            this.btnPurchase.Location = new System.Drawing.Point(455, 9);
            this.btnPurchase.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPurchase.Cursor = System.Windows.Forms.Cursors.Hand;

            this.checkoutPanel.Controls.Add(this.lblSelectedSeats);
            this.checkoutPanel.Controls.Add(this.lblTotalPrice);
            this.checkoutPanel.Controls.Add(this.btnPurchase);

            this.rightPanel.Controls.Add(this.checkoutPanel);
            this.rightPanel.Controls.Add(this.pnlSeats);
            this.rightPanel.Controls.Add(this.pnlLegend);
            this.rightPanel.Controls.Add(this.pnlScreen);
            this.rightPanel.Controls.Add(this.cbShows);
            this.rightPanel.Controls.Add(this.lblShowPrompt);
            this.rightPanel.Controls.Add(this.lblDescription);
            this.rightPanel.Controls.Add(this.lblMeta);
            this.rightPanel.Controls.Add(this.lblPriceBadge);
            this.rightPanel.Controls.Add(this.lblTitle);

            // Add main panels in proper docking sequence
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this.topBarPanel);
        }

        #endregion
    }
}

