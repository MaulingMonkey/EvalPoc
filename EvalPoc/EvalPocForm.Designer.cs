namespace EvalPoc {
	partial class EvalPocForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem( new string[] {
            "a",
            "b",
            "c"}, -1 );
			this.pbRenderTarget = new System.Windows.Forms.PictureBox();
			this.tbCode = new System.Windows.Forms.TextBox();
			this.lvErrors = new System.Windows.Forms.ListView();
			this.colType = new System.Windows.Forms.ColumnHeader();
			this.colMessage = new System.Windows.Forms.ColumnHeader();
			this.bNukeState = new System.Windows.Forms.Button();
			( (System.ComponentModel.ISupportInitialize)( this.pbRenderTarget ) ).BeginInit();
			this.SuspendLayout();
			// 
			// pbRenderTarget
			// 
			this.pbRenderTarget.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.pbRenderTarget.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pbRenderTarget.Location = new System.Drawing.Point( 12, 12 );
			this.pbRenderTarget.Name = "pbRenderTarget";
			this.pbRenderTarget.Size = new System.Drawing.Size( 673, 379 );
			this.pbRenderTarget.TabIndex = 0;
			this.pbRenderTarget.TabStop = false;
			this.pbRenderTarget.Paint += new System.Windows.Forms.PaintEventHandler( this.pbRenderTarget_Paint );
			// 
			// tbCode
			// 
			this.tbCode.AcceptsReturn = true;
			this.tbCode.AcceptsTab = true;
			this.tbCode.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
						| System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.tbCode.Location = new System.Drawing.Point( 12, 521 );
			this.tbCode.Multiline = true;
			this.tbCode.Name = "tbCode";
			this.tbCode.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tbCode.Size = new System.Drawing.Size( 673, 139 );
			this.tbCode.TabIndex = 1;
			this.tbCode.TextChanged += new System.EventHandler( this.tbCode_TextChanged );
			// 
			// lvErrors
			// 
			this.lvErrors.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
						| System.Windows.Forms.AnchorStyles.Right ) ) );
			this.lvErrors.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lvErrors.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.colType,
            this.colMessage} );
			this.lvErrors.GridLines = true;
			this.lvErrors.Items.AddRange( new System.Windows.Forms.ListViewItem[] {
            listViewItem1} );
			this.lvErrors.Location = new System.Drawing.Point( 12, 397 );
			this.lvErrors.Name = "lvErrors";
			this.lvErrors.Scrollable = false;
			this.lvErrors.Size = new System.Drawing.Size( 592, 118 );
			this.lvErrors.TabIndex = 2;
			this.lvErrors.UseCompatibleStateImageBehavior = false;
			this.lvErrors.View = System.Windows.Forms.View.Details;
			// 
			// colType
			// 
			this.colType.Text = "Type";
			this.colType.Width = 87;
			// 
			// colMessage
			// 
			this.colMessage.Text = "Message";
			this.colMessage.Width = 1000;
			// 
			// bNukeState
			// 
			this.bNukeState.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right ) ) );
			this.bNukeState.Location = new System.Drawing.Point( 610, 397 );
			this.bNukeState.Name = "bNukeState";
			this.bNukeState.Size = new System.Drawing.Size( 75, 23 );
			this.bNukeState.TabIndex = 3;
			this.bNukeState.Text = "Nuke State";
			this.bNukeState.UseVisualStyleBackColor = true;
			this.bNukeState.Click += new System.EventHandler( this.bNukeState_Click );
			// 
			// EvalPocForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 697, 672 );
			this.Controls.Add( this.bNukeState );
			this.Controls.Add( this.lvErrors );
			this.Controls.Add( this.tbCode );
			this.Controls.Add( this.pbRenderTarget );
			this.DoubleBuffered = true;
			this.Name = "EvalPocForm";
			this.Text = "Form1";
			this.Load += new System.EventHandler( this.EvalPocForm_Load );
			( (System.ComponentModel.ISupportInitialize)( this.pbRenderTarget ) ).EndInit();
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pbRenderTarget;
		private System.Windows.Forms.TextBox tbCode;
		private System.Windows.Forms.ListView lvErrors;
		private System.Windows.Forms.ColumnHeader colType;
		private System.Windows.Forms.ColumnHeader colMessage;
		private System.Windows.Forms.Button bNukeState;
	}
}

