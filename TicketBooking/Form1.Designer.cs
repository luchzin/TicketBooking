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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.FlowLayoutPanel flowMovies;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblMeta;
        private System.Windows.Forms.ComboBox cbShows;
        private System.Windows.Forms.Panel pnlSeats;
        private System.Windows.Forms.Button btnPurchase;
        private System.Windows.Forms.Label lblSelectedCount;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 640);
            this.Text = "Ticket Booking";

            this.leftPanel = new System.Windows.Forms.Panel();
            this.flowMovies = new System.Windows.Forms.FlowLayoutPanel();
            this.rightPanel = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblMeta = new System.Windows.Forms.Label();
            this.cbShows = new System.Windows.Forms.ComboBox();
            this.pnlSeats = new System.Windows.Forms.Panel();
            this.btnPurchase = new System.Windows.Forms.Button();
            this.lblSelectedCount = new System.Windows.Forms.Label();

            // leftPanel
            this.leftPanel.BackColor = System.Drawing.Color.FromArgb(28,34,40);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPanel.Width = 360;
            this.leftPanel.Padding = new System.Windows.Forms.Padding(12);
            this.leftPanel.Controls.Add(this.flowMovies);

            // flowMovies
            this.flowMovies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowMovies.AutoScroll = true;
            this.flowMovies.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowMovies.WrapContents = false;

            // rightPanel
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Padding = new System.Windows.Forms.Padding(18);
            this.rightPanel.BackColor = System.Drawing.Color.FromArgb(18,22,28);
            this.rightPanel.Controls.Add(this.lblSelectedCount);
            this.rightPanel.Controls.Add(this.btnPurchase);
            this.rightPanel.Controls.Add(this.pnlSeats);
            this.rightPanel.Controls.Add(this.cbShows);
            this.rightPanel.Controls.Add(this.lblMeta);
            this.rightPanel.Controls.Add(this.lblTitle);

            // lblTitle
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(12, 12);
            this.lblTitle.AutoSize = false;
            this.lblTitle.Height = 40;

            // lblMeta
            this.lblMeta.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblMeta.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            this.lblMeta.Location = new System.Drawing.Point(14, 56);
            this.lblMeta.AutoSize = true;

            // cbShows
            this.cbShows.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbShows.Location = new System.Drawing.Point(14, 84);
            this.cbShows.Width = 220;

            // pnlSeats
            this.pnlSeats.Location = new System.Drawing.Point(14, 120);
            this.pnlSeats.Size = new System.Drawing.Size(560, 380);
            this.pnlSeats.BackColor = System.Drawing.Color.Transparent;

            // btnPurchase
            this.btnPurchase.Text = "Purchase";
            this.btnPurchase.BackColor = System.Drawing.Color.FromArgb(0, 150, 136);
            this.btnPurchase.ForeColor = System.Drawing.Color.White;
            this.btnPurchase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPurchase.FlatAppearance.BorderSize = 0;
            this.btnPurchase.Size = new System.Drawing.Size(120, 36);
            this.btnPurchase.Location = new System.Drawing.Point(14, 520);

            // lblSelectedCount
            this.lblSelectedCount.AutoSize = true;
            this.lblSelectedCount.ForeColor = System.Drawing.Color.FromArgb(220,220,220);
            this.lblSelectedCount.Location = new System.Drawing.Point(150, 528);

            // Form controls
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.leftPanel);
        }

        #endregion
    }
}

