//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/******************************************************************/
/*****                                                        *****/
/*****     Project:           Adobe Color Picker Clone 1      *****/
/*****     Filename:          frmColorPicker.cs               *****/
/*****     Original Author:   Danny Blanchard                 *****/
/*****                        - scrabcakes@gmail.com          *****/
/*****     Updates:                                           *****/
/*****      3/28/2005 - Initial Version : Danny Blanchard     *****/
/*****                                                        *****/
/******************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.ColorEditing
{
    /// <summary>
    /// Color picker drawing style</summary>
    public enum ColorPickerDrawStyle
    {
        /// <summary>
        /// Hue</summary>
        Hue,
        /// <summary>
        /// Saturation</summary>
        Saturation,
        /// <summary>
        /// Brightness</summary>
        Brightness,
        /// <summary>
        /// Red</summary>
        Red,
        /// <summary>
        /// Green</summary>
        Green,
        /// <summary>
        /// Blue</summary>
        Blue
    }

    /// <summary>
    /// Color picker</summary>
    /// <remarks>Adapted from http://www.codeproject.com/csharp/adobe_cp_clone_part_1.asp. </remarks>
    // TODO: allow editing of alpha value
    public class ColorPicker : System.Windows.Forms.Form
    {
        #region Class Variables

        private AdobeColors.HSL        m_hsl;
        private Color                  m_rgb;
        private AdobeColors.CMYK       m_cmyk;
        private bool m_enableAlpha;

        #endregion

        #region Designer Generated Variables

        private System.Windows.Forms.PictureBox m_pbx_BlankBox;
        private System.Windows.Forms.Button m_cmd_OK;
        private System.Windows.Forms.Button m_cmd_Cancel;
        private System.Windows.Forms.TextBox m_txt_Hue;
        private System.Windows.Forms.TextBox m_txt_Sat;
        private System.Windows.Forms.TextBox m_txt_Bright;
        private System.Windows.Forms.TextBox m_txt_Red;
        private System.Windows.Forms.TextBox m_txt_Green;
        private System.Windows.Forms.TextBox m_txt_Blue;
        private System.Windows.Forms.TextBox m_txt_Cyan;
        private System.Windows.Forms.TextBox m_txt_Magenta;
        private System.Windows.Forms.TextBox m_txt_Yellow;
        private System.Windows.Forms.TextBox m_txt_K;
        private System.Windows.Forms.TextBox m_txt_Hex;
        private System.Windows.Forms.RadioButton m_rbtn_Hue;
        private System.Windows.Forms.RadioButton m_rbtn_Sat;
        private System.Windows.Forms.RadioButton m_rbtn_Bright;
        private System.Windows.Forms.RadioButton m_rbtn_Red;
        private System.Windows.Forms.RadioButton m_rbtn_Green;
        private System.Windows.Forms.RadioButton m_rbtn_Blue;
        private System.Windows.Forms.Label m_lbl_HexPound;
        private System.Windows.Forms.Label m_lbl_Cyan;
        private System.Windows.Forms.Label m_lbl_Magenta;
        private System.Windows.Forms.Label m_lbl_Yellow;
        private System.Windows.Forms.Label m_lbl_K;
        private System.Windows.Forms.Label m_lbl_Primary_Color;
        private System.Windows.Forms.Label m_lbl_Secondary_Color;
        private VerticalColorSlider m_ctrl_ThinBox;
        private ColorBox m_ctrl_BigBox;
        private System.Windows.Forms.Label m_lbl_Hue_Symbol;
        private System.Windows.Forms.Label m_lbl_Saturation_Symbol;
        private System.Windows.Forms.Label m_lbl_Bright_Symbol;
        private System.Windows.Forms.Label m_lbl_Cyan_Symbol;
        private System.Windows.Forms.Label m_lbl_Magenta_Symbol;
        private System.Windows.Forms.Label m_lbl_Yellow_Symbol;
        private TextBox m_txt_A;
        private Label m_lbl_A;
        /// <summary>
        /// Required designer variable</summary>
        private readonly System.ComponentModel.Container components = null;

        #endregion

        #region Constructors / Destructors

        /// <summary>
        /// Constructor with starting color</summary>
        /// <param name="starting_color">The starting color</param>
        public ColorPicker(Color starting_color) : this(starting_color, false)
        {
        }

        /// <summary>
        /// Constructor with starting color and alpha enable</summary>
        /// <param name="starting_color">The starting color</param>
        /// <param name="enableAlpha">Enable the alpha channel iff true</param>
        public ColorPicker(Color starting_color, bool enableAlpha)
        {
            InitializeComponent();
            SetStartColor(starting_color, enableAlpha);
        }

        /// <summary>
        /// Sets starting color</summary>
        /// <param name="starting_color">The starting color</param>
        /// <param name="enableAlpha">Enable the alpha channel iff true</param>
        public void SetStartColor(Color starting_color, bool enableAlpha)
        {
            m_enableAlpha = enableAlpha;
            if (enableAlpha)
            {
                m_txt_A.Visible = true;
                m_lbl_A.Visible = true;
                m_txt_Hex.MaxLength = 8;
            }
            else
            {
                starting_color = MakeOpaque(starting_color);
                m_txt_A.Visible = false;
                m_lbl_A.Visible = false;
                m_txt_Hex.MaxLength = 6;
            }

            m_rgb = starting_color;
            m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
            m_cmyk = AdobeColors.RGB_to_CMYK(m_rgb);

            m_txt_Hue.Text = Round(m_hsl.H * 360).ToString();
            m_txt_Sat.Text = Round(m_hsl.S * 100).ToString();
            m_txt_Bright.Text = Round(m_hsl.L * 100).ToString();
            m_txt_Red.Text = m_rgb.R.ToString();
            m_txt_Green.Text = m_rgb.G.ToString();
            m_txt_Blue.Text = m_rgb.B.ToString();
            m_txt_Cyan.Text = Round(m_cmyk.C * 100).ToString();
            m_txt_Magenta.Text = Round(m_cmyk.M * 100).ToString();
            m_txt_Yellow.Text = Round(m_cmyk.Y * 100).ToString();
            m_txt_K.Text = Round(m_cmyk.K * 100).ToString();
            m_txt_A.Text = m_rgb.A.ToString();

            m_txt_Hue.Update();
            m_txt_Sat.Update();
            m_txt_Red.Update();
            m_txt_Green.Update();
            m_txt_Blue.Update();
            m_txt_Cyan.Update();
            m_txt_Magenta.Update();
            m_txt_Yellow.Update();
            m_txt_K.Update();
            m_txt_A.Update();

            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;

            m_lbl_Primary_Color.BackColor = MakeOpaque(starting_color);
            m_lbl_Secondary_Color.BackColor = MakeOpaque(starting_color);

            m_rbtn_Hue.Checked = true;

            WriteHexData(m_rgb);
        }
        /// <summary>
        /// Disposes of the resources (other than memory) used by the <see cref="T:System.Windows.Forms.Form"></see></summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }


        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor</summary>
        private void InitializeComponent()
        {
            Sce.Atf.Controls.ColorEditing.AdobeColors.HSL hsl1 = new Sce.Atf.Controls.ColorEditing.AdobeColors.HSL();
            Sce.Atf.Controls.ColorEditing.AdobeColors.HSL hsl2 = new Sce.Atf.Controls.ColorEditing.AdobeColors.HSL();
            this.m_pbx_BlankBox = new System.Windows.Forms.PictureBox();
            this.m_cmd_OK = new System.Windows.Forms.Button();
            this.m_cmd_Cancel = new System.Windows.Forms.Button();
            this.m_txt_Hue = new System.Windows.Forms.TextBox();
            this.m_txt_Sat = new System.Windows.Forms.TextBox();
            this.m_txt_Bright = new System.Windows.Forms.TextBox();
            this.m_txt_Red = new System.Windows.Forms.TextBox();
            this.m_txt_Green = new System.Windows.Forms.TextBox();
            this.m_txt_Blue = new System.Windows.Forms.TextBox();
            this.m_txt_Cyan = new System.Windows.Forms.TextBox();
            this.m_txt_Magenta = new System.Windows.Forms.TextBox();
            this.m_txt_Yellow = new System.Windows.Forms.TextBox();
            this.m_txt_K = new System.Windows.Forms.TextBox();
            this.m_txt_Hex = new System.Windows.Forms.TextBox();
            this.m_rbtn_Hue = new System.Windows.Forms.RadioButton();
            this.m_rbtn_Sat = new System.Windows.Forms.RadioButton();
            this.m_rbtn_Bright = new System.Windows.Forms.RadioButton();
            this.m_rbtn_Red = new System.Windows.Forms.RadioButton();
            this.m_rbtn_Green = new System.Windows.Forms.RadioButton();
            this.m_rbtn_Blue = new System.Windows.Forms.RadioButton();
            this.m_lbl_HexPound = new System.Windows.Forms.Label();
            this.m_lbl_Cyan = new System.Windows.Forms.Label();
            this.m_lbl_Magenta = new System.Windows.Forms.Label();
            this.m_lbl_Yellow = new System.Windows.Forms.Label();
            this.m_lbl_K = new System.Windows.Forms.Label();
            this.m_lbl_Primary_Color = new System.Windows.Forms.Label();
            this.m_lbl_Secondary_Color = new System.Windows.Forms.Label();
            this.m_lbl_Hue_Symbol = new System.Windows.Forms.Label();
            this.m_lbl_Saturation_Symbol = new System.Windows.Forms.Label();
            this.m_lbl_Bright_Symbol = new System.Windows.Forms.Label();
            this.m_lbl_Cyan_Symbol = new System.Windows.Forms.Label();
            this.m_lbl_Magenta_Symbol = new System.Windows.Forms.Label();
            this.m_lbl_Yellow_Symbol = new System.Windows.Forms.Label();
            this.m_txt_A = new System.Windows.Forms.TextBox();
            this.m_lbl_A = new System.Windows.Forms.Label();
            this.m_ctrl_BigBox = new Sce.Atf.Controls.ColorEditing.ColorBox();
            this.m_ctrl_ThinBox = new Sce.Atf.Controls.ColorEditing.VerticalColorSlider();
            ((System.ComponentModel.ISupportInitialize)(this.m_pbx_BlankBox)).BeginInit();
            this.SuspendLayout();
            // 
            // m_pbx_BlankBox
            // 
            this.m_pbx_BlankBox.BackColor = System.Drawing.Color.Black;
            this.m_pbx_BlankBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.m_pbx_BlankBox.Location = new System.Drawing.Point(316, 11);
            this.m_pbx_BlankBox.Name = "m_pbx_BlankBox";
            this.m_pbx_BlankBox.Size = new System.Drawing.Size(62, 70);
            this.m_pbx_BlankBox.TabIndex = 3;
            this.m_pbx_BlankBox.TabStop = false;
            // 
            // m_cmd_OK
            // 
            this.m_cmd_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_cmd_OK.Location = new System.Drawing.Point(412, 12);
            this.m_cmd_OK.Name = "m_cmd_OK";
            this.m_cmd_OK.Size = new System.Drawing.Size(72, 23);
            this.m_cmd_OK.TabIndex = 4;
            this.m_cmd_OK.Text = "OK";
            // 
            // m_cmd_Cancel
            // 
            this.m_cmd_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_cmd_Cancel.Location = new System.Drawing.Point(412, 41);
            this.m_cmd_Cancel.Name = "m_cmd_Cancel";
            this.m_cmd_Cancel.Size = new System.Drawing.Size(72, 23);
            this.m_cmd_Cancel.TabIndex = 5;
            this.m_cmd_Cancel.Text = "Cancel";
            // 
            // m_txt_Hue
            // 
            this.m_txt_Hue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_txt_Hue.Location = new System.Drawing.Point(352, 101);
            this.m_txt_Hue.Name = "m_txt_Hue";
            this.m_txt_Hue.Size = new System.Drawing.Size(35, 21);
            this.m_txt_Hue.TabIndex = 6;
            this.m_txt_Hue.Leave += new System.EventHandler(this.m_txt_Hue_Leave);
            // 
            // m_txt_Sat
            // 
            this.m_txt_Sat.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_txt_Sat.Location = new System.Drawing.Point(352, 126);
            this.m_txt_Sat.Name = "m_txt_Sat";
            this.m_txt_Sat.Size = new System.Drawing.Size(35, 21);
            this.m_txt_Sat.TabIndex = 7;
            this.m_txt_Sat.Leave += new System.EventHandler(this.m_txt_Sat_Leave);
            // 
            // m_txt_Bright
            // 
            this.m_txt_Bright.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_txt_Bright.Location = new System.Drawing.Point(352, 151);
            this.m_txt_Bright.Name = "m_txt_Bright";
            this.m_txt_Bright.Size = new System.Drawing.Size(35, 21);
            this.m_txt_Bright.TabIndex = 8;
            this.m_txt_Bright.Leave += new System.EventHandler(this.m_txt_Bright_Leave);
            // 
            // m_txt_Red
            // 
            this.m_txt_Red.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_txt_Red.Location = new System.Drawing.Point(352, 190);
            this.m_txt_Red.Name = "m_txt_Red";
            this.m_txt_Red.Size = new System.Drawing.Size(35, 21);
            this.m_txt_Red.TabIndex = 9;
            this.m_txt_Red.Leave += new System.EventHandler(this.m_txt_Red_Leave);
            // 
            // m_txt_Green
            // 
            this.m_txt_Green.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_txt_Green.Location = new System.Drawing.Point(352, 215);
            this.m_txt_Green.Name = "m_txt_Green";
            this.m_txt_Green.Size = new System.Drawing.Size(35, 21);
            this.m_txt_Green.TabIndex = 10;
            this.m_txt_Green.Leave += new System.EventHandler(this.m_txt_Green_Leave);
            // 
            // m_txt_Blue
            // 
            this.m_txt_Blue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_txt_Blue.Location = new System.Drawing.Point(352, 240);
            this.m_txt_Blue.Name = "m_txt_Blue";
            this.m_txt_Blue.Size = new System.Drawing.Size(35, 21);
            this.m_txt_Blue.TabIndex = 11;
            this.m_txt_Blue.Leave += new System.EventHandler(this.m_txt_Blue_Leave);
            // 
            // m_txt_Cyan
            // 
            this.m_txt_Cyan.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_txt_Cyan.Location = new System.Drawing.Point(447, 101);
            this.m_txt_Cyan.Name = "m_txt_Cyan";
            this.m_txt_Cyan.Size = new System.Drawing.Size(35, 21);
            this.m_txt_Cyan.TabIndex = 15;
            this.m_txt_Cyan.Leave += new System.EventHandler(this.m_txt_Cyan_Leave);
            // 
            // m_txt_Magenta
            // 
            this.m_txt_Magenta.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_txt_Magenta.Location = new System.Drawing.Point(447, 126);
            this.m_txt_Magenta.Name = "m_txt_Magenta";
            this.m_txt_Magenta.Size = new System.Drawing.Size(35, 21);
            this.m_txt_Magenta.TabIndex = 16;
            this.m_txt_Magenta.Leave += new System.EventHandler(this.m_txt_Magenta_Leave);
            // 
            // m_txt_Yellow
            // 
            this.m_txt_Yellow.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_txt_Yellow.Location = new System.Drawing.Point(447, 151);
            this.m_txt_Yellow.Name = "m_txt_Yellow";
            this.m_txt_Yellow.Size = new System.Drawing.Size(35, 21);
            this.m_txt_Yellow.TabIndex = 17;
            this.m_txt_Yellow.Leave += new System.EventHandler(this.m_txt_Yellow_Leave);
            // 
            // m_txt_K
            // 
            this.m_txt_K.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_txt_K.Location = new System.Drawing.Point(447, 176);
            this.m_txt_K.Name = "m_txt_K";
            this.m_txt_K.Size = new System.Drawing.Size(35, 21);
            this.m_txt_K.TabIndex = 18;
            this.m_txt_K.Leave += new System.EventHandler(this.m_txt_K_Leave);
            // 
            // m_txt_Hex
            // 
            this.m_txt_Hex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_txt_Hex.Location = new System.Drawing.Point(416, 239);
            this.m_txt_Hex.MaxLength = 8;
            this.m_txt_Hex.Name = "m_txt_Hex";
            this.m_txt_Hex.Size = new System.Drawing.Size(66, 21);
            this.m_txt_Hex.TabIndex = 19;
            this.m_txt_Hex.Text = "AAAAAAAA";
            this.m_txt_Hex.Leave += new System.EventHandler(this.m_txt_Hex_Leave);
            // 
            // m_rbtn_Hue
            // 
            this.m_rbtn_Hue.Location = new System.Drawing.Point(316, 101);
            this.m_rbtn_Hue.Name = "m_rbtn_Hue";
            this.m_rbtn_Hue.Size = new System.Drawing.Size(38, 24);
            this.m_rbtn_Hue.TabIndex = 20;
            this.m_rbtn_Hue.Text = "H:";
            this.m_rbtn_Hue.AutoSize = true;            
            this.m_rbtn_Hue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_rbtn_Hue.CheckedChanged += new System.EventHandler(this.m_rbtn_Hue_CheckedChanged);
            // 
            // m_rbtn_Sat
            // 
            this.m_rbtn_Sat.Location = new System.Drawing.Point(316, 126);
            this.m_rbtn_Sat.Name = "m_rbtn_Sat";
            this.m_rbtn_Sat.Size = new System.Drawing.Size(38, 24);
            this.m_rbtn_Sat.TabIndex = 21;
            this.m_rbtn_Sat.Text = "S:";
            this.m_rbtn_Sat.AutoSize = true;
            this.m_rbtn_Sat.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_rbtn_Sat.CheckedChanged += new System.EventHandler(this.m_rbtn_Sat_CheckedChanged);
            // 
            // m_rbtn_Bright
            // 
            this.m_rbtn_Bright.Location = new System.Drawing.Point(316, 151);
            this.m_rbtn_Bright.Name = "m_rbtn_Bright";
            this.m_rbtn_Bright.Size = new System.Drawing.Size(38, 24);
            this.m_rbtn_Bright.TabIndex = 22;
            this.m_rbtn_Bright.Text = "B:";
            this.m_rbtn_Bright.AutoSize = true;
            this.m_rbtn_Bright.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_rbtn_Bright.CheckedChanged += new System.EventHandler(this.m_rbtn_Bright_CheckedChanged);
            // 
            // m_rbtn_Red
            // 
            this.m_rbtn_Red.Location = new System.Drawing.Point(316, 190);
            this.m_rbtn_Red.Name = "m_rbtn_Red";
            this.m_rbtn_Red.Size = new System.Drawing.Size(38, 24);
            this.m_rbtn_Red.TabIndex = 23;
            this.m_rbtn_Red.Text = "R:";
            this.m_rbtn_Red.AutoSize = true;
            this.m_rbtn_Red.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_rbtn_Red.CheckedChanged += new System.EventHandler(this.m_rbtn_Red_CheckedChanged);
            // 
            // m_rbtn_Green
            // 
            this.m_rbtn_Green.Location = new System.Drawing.Point(316, 215);
            this.m_rbtn_Green.Name = "m_rbtn_Green";
            this.m_rbtn_Green.Size = new System.Drawing.Size(38, 24);
            this.m_rbtn_Green.TabIndex = 24;
            this.m_rbtn_Green.Text = "G:";
            this.m_rbtn_Green.AutoSize = true;
            this.m_rbtn_Green.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_rbtn_Green.CheckedChanged += new System.EventHandler(this.m_rbtn_Green_CheckedChanged);
            // 
            // m_rbtn_Blue
            // 
            this.m_rbtn_Blue.Location = new System.Drawing.Point(316, 240);
            this.m_rbtn_Blue.Name = "m_rbtn_Blue";
            this.m_rbtn_Blue.Size = new System.Drawing.Size(38, 24);
            this.m_rbtn_Blue.TabIndex = 25;
            this.m_rbtn_Blue.Text = "B:";
            this.m_rbtn_Blue.AutoSize = true;
            this.m_rbtn_Blue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_rbtn_Blue.CheckedChanged += new System.EventHandler(this.m_rbtn_Blue_CheckedChanged);
            // 
            // m_lbl_HexPound
            // 
            this.m_lbl_HexPound.Location = new System.Drawing.Point(402, 243);
            this.m_lbl_HexPound.Name = "m_lbl_HexPound";
            this.m_lbl_HexPound.Size = new System.Drawing.Size(16, 14);
            this.m_lbl_HexPound.TabIndex = 27;
            this.m_lbl_HexPound.Text = "#";
            // 
            // m_lbl_Cyan
            // 
            this.m_lbl_Cyan.Location = new System.Drawing.Point(423, 105);
            this.m_lbl_Cyan.Name = "m_lbl_Cyan";
            this.m_lbl_Cyan.Size = new System.Drawing.Size(24, 16);
            this.m_lbl_Cyan.TabIndex = 31;
            this.m_lbl_Cyan.Text = "C:";
            this.m_lbl_Cyan.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_lbl_Magenta
            // 
            this.m_lbl_Magenta.Location = new System.Drawing.Point(423, 131);
            this.m_lbl_Magenta.Name = "m_lbl_Magenta";
            this.m_lbl_Magenta.Size = new System.Drawing.Size(24, 16);
            this.m_lbl_Magenta.TabIndex = 32;
            this.m_lbl_Magenta.Text = "M:";
            this.m_lbl_Magenta.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_lbl_Yellow
            // 
            this.m_lbl_Yellow.Location = new System.Drawing.Point(423, 156);
            this.m_lbl_Yellow.Name = "m_lbl_Yellow";
            this.m_lbl_Yellow.Size = new System.Drawing.Size(24, 16);
            this.m_lbl_Yellow.TabIndex = 33;
            this.m_lbl_Yellow.Text = "Y:";
            this.m_lbl_Yellow.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_lbl_K
            // 
            this.m_lbl_K.Location = new System.Drawing.Point(423, 181);
            this.m_lbl_K.Name = "m_lbl_K";
            this.m_lbl_K.Size = new System.Drawing.Size(24, 16);
            this.m_lbl_K.TabIndex = 34;
            this.m_lbl_K.Text = "K:";
            this.m_lbl_K.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_lbl_Primary_Color
            // 
            this.m_lbl_Primary_Color.Location = new System.Drawing.Point(317, 12);
            this.m_lbl_Primary_Color.Name = "m_lbl_Primary_Color";
            this.m_lbl_Primary_Color.Size = new System.Drawing.Size(60, 34);
            this.m_lbl_Primary_Color.TabIndex = 36;
            this.m_lbl_Primary_Color.Click += new System.EventHandler(this.m_lbl_Primary_Color_Click);
            // 
            // m_lbl_Secondary_Color
            // 
            this.m_lbl_Secondary_Color.Location = new System.Drawing.Point(317, 46);
            this.m_lbl_Secondary_Color.Name = "m_lbl_Secondary_Color";
            this.m_lbl_Secondary_Color.Size = new System.Drawing.Size(60, 34);
            this.m_lbl_Secondary_Color.TabIndex = 37;
            this.m_lbl_Secondary_Color.Click += new System.EventHandler(this.m_lbl_Secondary_Color_Click);
            // 
            // m_lbl_Hue_Symbol
            // 
            this.m_lbl_Hue_Symbol.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_lbl_Hue_Symbol.Location = new System.Drawing.Point(389, 102);
            this.m_lbl_Hue_Symbol.Name = "m_lbl_Hue_Symbol";
            this.m_lbl_Hue_Symbol.Size = new System.Drawing.Size(16, 21);
            this.m_lbl_Hue_Symbol.TabIndex = 40;
            this.m_lbl_Hue_Symbol.Text = "";
            // 
            // m_lbl_Saturation_Symbol
            // 
            this.m_lbl_Saturation_Symbol.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_lbl_Saturation_Symbol.Location = new System.Drawing.Point(389, 130);
            this.m_lbl_Saturation_Symbol.Name = "m_lbl_Saturation_Symbol";
            this.m_lbl_Saturation_Symbol.Size = new System.Drawing.Size(16, 21);
            this.m_lbl_Saturation_Symbol.TabIndex = 41;
            this.m_lbl_Saturation_Symbol.Text = "%";
            // 
            // m_lbl_Bright_Symbol
            // 
            this.m_lbl_Bright_Symbol.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_lbl_Bright_Symbol.Location = new System.Drawing.Point(389, 155);
            this.m_lbl_Bright_Symbol.Name = "m_lbl_Bright_Symbol";
            this.m_lbl_Bright_Symbol.Size = new System.Drawing.Size(16, 21);
            this.m_lbl_Bright_Symbol.TabIndex = 42;
            this.m_lbl_Bright_Symbol.Text = "%";
            // 
            // m_lbl_Cyan_Symbol
            // 
            this.m_lbl_Cyan_Symbol.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_lbl_Cyan_Symbol.Location = new System.Drawing.Point(483, 103);
            this.m_lbl_Cyan_Symbol.Name = "m_lbl_Cyan_Symbol";
            this.m_lbl_Cyan_Symbol.Size = new System.Drawing.Size(16, 21);
            this.m_lbl_Cyan_Symbol.TabIndex = 43;
            this.m_lbl_Cyan_Symbol.Text = "%";
            // 
            // m_lbl_Magenta_Symbol
            // 
            this.m_lbl_Magenta_Symbol.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_lbl_Magenta_Symbol.Location = new System.Drawing.Point(483, 130);
            this.m_lbl_Magenta_Symbol.Name = "m_lbl_Magenta_Symbol";
            this.m_lbl_Magenta_Symbol.Size = new System.Drawing.Size(16, 21);
            this.m_lbl_Magenta_Symbol.TabIndex = 44;
            this.m_lbl_Magenta_Symbol.Text = "%";
            // 
            // m_lbl_Yellow_Symbol
            // 
            this.m_lbl_Yellow_Symbol.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_lbl_Yellow_Symbol.Location = new System.Drawing.Point(483, 155);
            this.m_lbl_Yellow_Symbol.Name = "m_lbl_Yellow_Symbol";
            this.m_lbl_Yellow_Symbol.Size = new System.Drawing.Size(16, 21);
            this.m_lbl_Yellow_Symbol.TabIndex = 45;
            this.m_lbl_Yellow_Symbol.Text = "%";
            // 
            // m_txt_A
            // 
            this.m_txt_A.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.m_txt_A.Location = new System.Drawing.Point(447, 208);
            this.m_txt_A.Name = "m_txt_A";
            this.m_txt_A.Size = new System.Drawing.Size(35, 21);
            this.m_txt_A.TabIndex = 46;
            this.m_txt_A.Leave += new System.EventHandler(this.m_txt_A_Leave);
            // 
            // m_lbl_A
            // 
            this.m_lbl_A.Location = new System.Drawing.Point(404, 213);
            this.m_lbl_A.Name = "m_lbl_A";
            this.m_lbl_A.Size = new System.Drawing.Size(43, 16);
            this.m_lbl_A.TabIndex = 47;
            this.m_lbl_A.Text = "Alpha:";
            this.m_lbl_A.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_ctrl_BigBox
            // 
            this.m_ctrl_BigBox.DrawStyle = Sce.Atf.Controls.ColorEditing.ColorBox.eDrawStyle.Hue;
            hsl1.A = 1;
            hsl1.H = 0;
            hsl1.L = 1;
            hsl1.S = 1;
            this.m_ctrl_BigBox.HSL = hsl1;
            this.m_ctrl_BigBox.Location = new System.Drawing.Point(10, 11);
            this.m_ctrl_BigBox.Name = "m_ctrl_BigBox";
            this.m_ctrl_BigBox.RGB = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.m_ctrl_BigBox.Size = new System.Drawing.Size(260, 260);
            this.m_ctrl_BigBox.TabIndex = 39;
            this.m_ctrl_BigBox.ColorChanged += new System.EventHandler(this.m_ctrl_BigBox_ColorChanged);
            // 
            // m_ctrl_ThinBox
            // 
            this.m_ctrl_ThinBox.DrawStyle = Sce.Atf.Controls.ColorEditing.VerticalColorSlider.eDrawStyle.Hue;
            hsl2.A = 1;
            hsl2.H = 0;
            hsl2.L = 1;
            hsl2.S = 1;
            this.m_ctrl_ThinBox.HSL = hsl2;
            this.m_ctrl_ThinBox.Location = new System.Drawing.Point(271, 9);
            this.m_ctrl_ThinBox.Name = "m_ctrl_ThinBox";
            this.m_ctrl_ThinBox.RGB = System.Drawing.Color.Red;
            this.m_ctrl_ThinBox.Size = new System.Drawing.Size(40, 264);
            this.m_ctrl_ThinBox.TabIndex = 38;
            this.m_ctrl_ThinBox.ColorChanged += new System.EventHandler(this.m_ctrl_ThinBox_ColorChanged);
            // 
            // ColorPicker
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;            
            this.ClientSize = new System.Drawing.Size(513, 280);
            this.Controls.Add(this.m_lbl_A);
            this.Controls.Add(this.m_txt_A);
            this.Controls.Add(this.m_lbl_Yellow_Symbol);
            this.Controls.Add(this.m_lbl_Magenta_Symbol);
            this.Controls.Add(this.m_lbl_Cyan_Symbol);
            this.Controls.Add(this.m_lbl_Bright_Symbol);
            this.Controls.Add(this.m_lbl_Saturation_Symbol);
            this.Controls.Add(this.m_lbl_Hue_Symbol);
            this.Controls.Add(this.m_ctrl_BigBox);
            this.Controls.Add(this.m_ctrl_ThinBox);
            this.Controls.Add(this.m_lbl_Secondary_Color);
            this.Controls.Add(this.m_lbl_Primary_Color);
            this.Controls.Add(this.m_txt_Hex);
            this.Controls.Add(this.m_txt_K);
            this.Controls.Add(this.m_txt_Yellow);
            this.Controls.Add(this.m_txt_Magenta);
            this.Controls.Add(this.m_txt_Cyan);
            this.Controls.Add(this.m_txt_Blue);
            this.Controls.Add(this.m_txt_Green);
            this.Controls.Add(this.m_txt_Red);
            this.Controls.Add(this.m_txt_Bright);
            this.Controls.Add(this.m_txt_Sat);
            this.Controls.Add(this.m_txt_Hue);
            this.Controls.Add(this.m_cmd_Cancel);
            this.Controls.Add(this.m_cmd_OK);
            this.Controls.Add(this.m_pbx_BlankBox);
            this.Controls.Add(this.m_rbtn_Red);
            this.Controls.Add(this.m_rbtn_Blue);
            this.Controls.Add(this.m_rbtn_Green);
            this.Controls.Add(this.m_rbtn_Bright);
            this.Controls.Add(this.m_rbtn_Sat);
            this.Controls.Add(this.m_rbtn_Hue);
            this.Controls.Add(this.m_lbl_K);
            this.Controls.Add(this.m_lbl_Yellow);
            this.Controls.Add(this.m_lbl_Magenta);
            this.Controls.Add(this.m_lbl_Cyan);
            this.Controls.Add(this.m_lbl_HexPound);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ColorPicker";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Color Picker";
            ((System.ComponentModel.ISupportInitialize)(this.m_pbx_BlankBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        #region Events

        #region Primary Picture Box (m_ctrl_BigBox)

        private void m_ctrl_BigBox_ColorChanged(object sender, System.EventArgs e)
        {
            m_hsl = m_ctrl_BigBox.HSL;
            m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
            m_cmyk = AdobeColors.RGB_to_CMYK(m_rgb);
            
            m_txt_Hue.Text =        Round(m_hsl.H * 360).ToString();
            m_txt_Sat.Text =        Round(m_hsl.S * 100).ToString();
            m_txt_Bright.Text =     Round(m_hsl.L * 100).ToString();
            m_txt_Red.Text =        m_rgb.R.ToString();
            m_txt_Green.Text =      m_rgb.G.ToString();
            m_txt_Blue.Text =       m_rgb.B.ToString();
            m_txt_Cyan.Text =       Round(m_cmyk.C * 100).ToString();
            m_txt_Magenta.Text =    Round(m_cmyk.M * 100).ToString();
            m_txt_Yellow.Text =     Round(m_cmyk.Y * 100).ToString();
            m_txt_K.Text =          Round(m_cmyk.K * 100).ToString();
            m_txt_A.Text =          m_rgb.A.ToString();

            m_txt_Hue.Update();
            m_txt_Sat.Update();
            m_txt_Bright.Update();
            m_txt_Red.Update();
            m_txt_Green.Update();
            m_txt_Blue.Update();
            m_txt_Cyan.Update();
            m_txt_Magenta.Update();
            m_txt_Yellow.Update();
            m_txt_K.Update();
            m_txt_A.Update();

            m_ctrl_ThinBox.HSL = m_hsl;

            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);
            m_lbl_Primary_Color.Update();

            WriteHexData(m_rgb);
        }


        #endregion

        #region Secondary Picture Box (m_ctrl_ThinBox)

        private void m_ctrl_ThinBox_ColorChanged(object sender, System.EventArgs e)
        {
            m_hsl = m_ctrl_ThinBox.HSL;
            m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
            m_cmyk = AdobeColors.RGB_to_CMYK(m_rgb);
            
            m_txt_Hue.Text =        Round(m_hsl.H * 360).ToString();
            m_txt_Sat.Text =        Round(m_hsl.S * 100).ToString();
            m_txt_Bright.Text =     Round(m_hsl.L * 100).ToString();
            m_txt_Red.Text =        m_rgb.R.ToString();
            m_txt_Green.Text =      m_rgb.G.ToString();
            m_txt_Blue.Text =       m_rgb.B.ToString();
            m_txt_Cyan.Text =       Round(m_cmyk.C * 100).ToString();
            m_txt_Magenta.Text =    Round(m_cmyk.M * 100).ToString();
            m_txt_Yellow.Text =     Round(m_cmyk.Y * 100).ToString();
            m_txt_K.Text =          Round(m_cmyk.K * 100).ToString();
            m_txt_A.Text =          m_rgb.A.ToString();

            m_txt_Hue.Update();
            m_txt_Sat.Update();
            m_txt_Bright.Update();
            m_txt_Red.Update();
            m_txt_Green.Update();
            m_txt_Blue.Update();
            m_txt_Cyan.Update();
            m_txt_Magenta.Update();
            m_txt_Yellow.Update();
            m_txt_K.Update();
            m_txt_A.Update();

            m_ctrl_BigBox.HSL = m_hsl;

            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);
            m_lbl_Primary_Color.Update();

            WriteHexData(m_rgb);
        }


        #endregion

        #region Hex Box (m_txt_Hex)

        private void m_txt_Hex_Leave(object sender, System.EventArgs e)
        {
            string text = m_txt_Hex.Text.ToUpper();
            bool has_illegal_chars = false;

            if ( text.Length <= 0 )
                has_illegal_chars = true;
            foreach ( char letter in text )
            {
                if ( !char.IsNumber(letter) )
                {
                    if ( letter >= 'A' && letter <= 'F' )
                        continue;
                    has_illegal_chars = true;
                    break;
                }
            }

            if ( has_illegal_chars )
            {
                MessageBox.Show("Hex must be a hex value between 00000000 and FFFFFFFF".Localize());
                WriteHexData(m_rgb);
                return;
            }

            Color hexColor = ParseHexData(text);
            if (hexColor.IsEmpty)
            {
                WriteHexData(m_rgb);
                return;
            }

            m_rgb = hexColor;
            m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
            m_cmyk = AdobeColors.RGB_to_CMYK(m_rgb);

            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;
            m_lbl_Primary_Color.BackColor = m_rgb;

            UpdateTextBoxes();
        }


        #endregion

        #region Color Boxes

        private void m_lbl_Primary_Color_Click(object sender, System.EventArgs e)
        {
            Color primary = m_lbl_Primary_Color.BackColor;
            m_rgb = Color.FromArgb(m_rgb.A, primary.R, primary.G, primary.B);
            m_hsl = AdobeColors.RGB_to_HSL(m_rgb);

            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;

            m_cmyk = AdobeColors.RGB_to_CMYK(m_rgb);
            
            m_txt_Hue.Text =        Round(m_hsl.H * 360).ToString();
            m_txt_Sat.Text =        Round(m_hsl.S * 100).ToString();
            m_txt_Bright.Text =     Round(m_hsl.L * 100).ToString();
            m_txt_Red.Text =        m_rgb.R.ToString();
            m_txt_Green.Text =      m_rgb.G.ToString();
            m_txt_Blue.Text =       m_rgb.B.ToString();
            m_txt_Cyan.Text =       Round(m_cmyk.C * 100).ToString();
            m_txt_Magenta.Text =    Round(m_cmyk.M * 100).ToString();
            m_txt_Yellow.Text =     Round(m_cmyk.Y * 100).ToString();
            m_txt_K.Text =          Round(m_cmyk.K * 100).ToString();
            m_txt_A.Text =          m_rgb.A.ToString();

            m_txt_Hue.Update();
            m_txt_Sat.Update();
            m_txt_Red.Update();
            m_txt_Green.Update();
            m_txt_Blue.Update();
            m_txt_Cyan.Update();
            m_txt_Magenta.Update();
            m_txt_Yellow.Update();
            m_txt_K.Update();
            m_txt_A.Update();
        }


        private void m_lbl_Secondary_Color_Click(object sender, System.EventArgs e)
        {
            Color secondary = m_lbl_Secondary_Color.BackColor;
            m_rgb = Color.FromArgb(m_rgb.A, secondary.R, secondary.G, secondary.B);
            m_hsl = AdobeColors.RGB_to_HSL(m_rgb);

            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;

            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);
            m_lbl_Primary_Color.Update();

            m_cmyk = AdobeColors.RGB_to_CMYK(m_rgb);
            
            m_txt_Hue.Text =        Round(m_hsl.H * 360).ToString();
            m_txt_Sat.Text =        Round(m_hsl.S * 100).ToString();
            m_txt_Bright.Text =     Round(m_hsl.L * 100).ToString();
            m_txt_Red.Text =        m_rgb.R.ToString();
            m_txt_Green.Text =      m_rgb.G.ToString();
            m_txt_Blue.Text =       m_rgb.B.ToString();
            m_txt_Cyan.Text =       Round(m_cmyk.C * 100).ToString();
            m_txt_Magenta.Text =    Round(m_cmyk.M * 100).ToString();
            m_txt_Yellow.Text =     Round(m_cmyk.Y * 100).ToString();
            m_txt_K.Text =          Round(m_cmyk.K * 100).ToString();
            m_txt_A.Text =          m_rgb.A.ToString();

            m_txt_Hue.Update();
            m_txt_Sat.Update();
            m_txt_Red.Update();
            m_txt_Green.Update();
            m_txt_Blue.Update();
            m_txt_Cyan.Update();
            m_txt_Magenta.Update();
            m_txt_Yellow.Update();
            m_txt_K.Update();
            m_txt_A.Update();
        }


        #endregion

        #region Radio Buttons

        private void m_rbtn_Hue_CheckedChanged(object sender, System.EventArgs e)
        {
            if ( m_rbtn_Hue.Checked )
            {
                m_ctrl_ThinBox.DrawStyle = VerticalColorSlider.eDrawStyle.Hue;
                m_ctrl_BigBox.DrawStyle = ColorBox.eDrawStyle.Hue;
            }
        }


        private void m_rbtn_Sat_CheckedChanged(object sender, System.EventArgs e)
        {
            if ( m_rbtn_Sat.Checked )
            {
                m_ctrl_ThinBox.DrawStyle = VerticalColorSlider.eDrawStyle.Saturation;
                m_ctrl_BigBox.DrawStyle = ColorBox.eDrawStyle.Saturation;
            }
        }


        private void m_rbtn_Bright_CheckedChanged(object sender, System.EventArgs e)
        {
            if ( m_rbtn_Bright.Checked )
            {
                m_ctrl_ThinBox.DrawStyle = VerticalColorSlider.eDrawStyle.Brightness;
                m_ctrl_BigBox.DrawStyle = ColorBox.eDrawStyle.Brightness;
            }
        }


        private void m_rbtn_Red_CheckedChanged(object sender, System.EventArgs e)
        {
            if ( m_rbtn_Red.Checked )
            {
                m_ctrl_ThinBox.DrawStyle = VerticalColorSlider.eDrawStyle.Red;
                m_ctrl_BigBox.DrawStyle = ColorBox.eDrawStyle.Red;
            }
        }


        private void m_rbtn_Green_CheckedChanged(object sender, System.EventArgs e)
        {
            if ( m_rbtn_Green.Checked )
            {
                m_ctrl_ThinBox.DrawStyle = VerticalColorSlider.eDrawStyle.Green;
                m_ctrl_BigBox.DrawStyle = ColorBox.eDrawStyle.Green;
            }
        }


        private void m_rbtn_Blue_CheckedChanged(object sender, System.EventArgs e)
        {
            if ( m_rbtn_Blue.Checked )
            {
                m_ctrl_ThinBox.DrawStyle = VerticalColorSlider.eDrawStyle.Blue;
                m_ctrl_BigBox.DrawStyle = ColorBox.eDrawStyle.Blue;
            }
        }


        #endregion

        #region Text Boxes

        private void m_txt_Hue_Leave(object sender, System.EventArgs e)
        {
            string text = m_txt_Hue.Text;
            bool has_illegal_chars = false;

            if ( text.Length <= 0 )
                has_illegal_chars = true;
            else
                foreach ( char letter in text )
                {
                    if ( !char.IsNumber(letter) )
                    {
                        has_illegal_chars = true;
                        break;
                    }
                }

            if ( has_illegal_chars )
            {
                MessageBox.Show("Hue must be a number between 0 and 360".Localize());
                UpdateTextBoxes();
                return;
            }

            int hue = int.Parse(text);

            if ( hue < 0 )
            {
                m_txt_Hue.Text = "0";
                m_hsl.H = 0.0;
            }
            else if ( hue > 360 )
            {
                m_txt_Hue.Text = "360";
                m_hsl.H = 1.0;
            }
            else
            {
                m_hsl.H = (double)hue/360;
            }

            m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
            m_cmyk = AdobeColors.RGB_to_CMYK(m_rgb);
            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;
            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);

            UpdateTextBoxes();
        }


        private void m_txt_Sat_Leave(object sender, System.EventArgs e)
        {
            string text = m_txt_Sat.Text;
            bool has_illegal_chars = false;

            if ( text.Length <= 0 )
                has_illegal_chars = true;
            else
                foreach ( char letter in text )
                {
                    if ( !char.IsNumber(letter) )
                    {
                        has_illegal_chars = true;
                        break;
                    }
                }

            if ( has_illegal_chars )
            {
                MessageBox.Show("Saturation must be a number between 0 and 100".Localize());
                UpdateTextBoxes();
                return;
            }

            int sat = int.Parse(text);

            if ( sat < 0 )
            {
                m_txt_Sat.Text = "0";
                m_hsl.S = 0.0;
            }
            else if ( sat > 100 )
            {
                m_txt_Sat.Text = "100";
                m_hsl.S = 1.0;
            }
            else
            {
                m_hsl.S = (double)sat/100;
            }

            m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
            m_cmyk = AdobeColors.RGB_to_CMYK(m_rgb);
            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;
            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);

            UpdateTextBoxes();
        }


        private void m_txt_Bright_Leave(object sender, System.EventArgs e)
        {
            string text = m_txt_Bright.Text;
            bool has_illegal_chars = false;

            if ( text.Length <= 0 )
                has_illegal_chars = true;
            else
                foreach ( char letter in text )
                {
                    if ( !char.IsNumber(letter) )
                    {
                        has_illegal_chars = true;
                        break;
                    }
                }

            if ( has_illegal_chars )
            {
                MessageBox.Show("Brightness must be a number between 0 and 100".Localize());
                UpdateTextBoxes();
                return;
            }

            int lum = int.Parse(text);

            if ( lum < 0 )
            {
                m_txt_Bright.Text = "0";
                m_hsl.L = 0.0;
            }
            else if ( lum > 100 )
            {
                m_txt_Bright.Text = "100";
                m_hsl.L = 1.0;
            }
            else
            {
                m_hsl.L = (double)lum/100;
            }

            m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
            m_cmyk = AdobeColors.RGB_to_CMYK(m_rgb);
            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;
            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);

            UpdateTextBoxes();
        }


        private void m_txt_Red_Leave(object sender, System.EventArgs e)
        {
            string text = m_txt_Red.Text;
            bool has_illegal_chars = false;

            if ( text.Length <= 0 )
                has_illegal_chars = true;
            else
                foreach ( char letter in text )
                {
                    if ( !char.IsNumber(letter) )
                    {
                        has_illegal_chars = true;
                        break;
                    }
                }

            if ( has_illegal_chars )
            {
                MessageBox.Show("Red must be a number between 0 and 255".Localize());
                UpdateTextBoxes();
                return;
            }

            int red = int.Parse(text);

            if ( red < 0 )
            {
                m_txt_Red.Text = "0";
                m_rgb = Color.FromArgb(m_rgb.A, 0, m_rgb.G, m_rgb.B);
            }
            else if ( red > 255 )
            {
                m_txt_Red.Text = "255";
                m_rgb = Color.FromArgb(m_rgb.A, 255, m_rgb.G, m_rgb.B);
            }
            else
            {
                m_rgb = Color.FromArgb(m_rgb.A, red, m_rgb.G, m_rgb.B);
            }

            m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
            m_cmyk = AdobeColors.RGB_to_CMYK(m_rgb);
            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;
            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);

            UpdateTextBoxes();
        }


        private void m_txt_Green_Leave(object sender, System.EventArgs e)
        {
            string text = m_txt_Green.Text;
            bool has_illegal_chars = false;

            if ( text.Length <= 0 )
                has_illegal_chars = true;
            else
                foreach ( char letter in text )
                {
                    if ( !char.IsNumber(letter) )
                    {
                        has_illegal_chars = true;
                        break;
                    }
                }

            if ( has_illegal_chars )
            {
                MessageBox.Show("Green must be a number between 0 and 255".Localize());
                UpdateTextBoxes();
                return;
            }

            int green = int.Parse(text);

            if ( green < 0 )
            {
                m_txt_Green.Text = "0";
                m_rgb = Color.FromArgb(m_rgb.A, m_rgb.R, 0, m_rgb.B);
            }
            else if ( green > 255 )
            {
                m_txt_Green.Text = "255";
                m_rgb = Color.FromArgb(m_rgb.A, m_rgb.R, 255, m_rgb.B);
            }
            else
            {
                m_rgb = Color.FromArgb(m_rgb.A, m_rgb.R, green, m_rgb.B);
            }

            m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
            m_cmyk = AdobeColors.RGB_to_CMYK(m_rgb);
            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;
            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);

            UpdateTextBoxes();
        }


        private void m_txt_Blue_Leave(object sender, System.EventArgs e)
        {
            string text = m_txt_Blue.Text;
            bool has_illegal_chars = false;

            if ( text.Length <= 0 )
                has_illegal_chars = true;
            else
                foreach ( char letter in text )
                {
                    if ( !char.IsNumber(letter) )
                    {
                        has_illegal_chars = true;
                        break;
                    }
                }

            if ( has_illegal_chars )
            {
                MessageBox.Show("Blue must be a number between 0 and 255".Localize());
                UpdateTextBoxes();
                return;
            }

            int blue = int.Parse(text);

            if ( blue < 0 )
            {
                m_txt_Blue.Text = "0";
                m_rgb = Color.FromArgb(m_rgb.A, m_rgb.R, m_rgb.G, 0);
            }
            else if ( blue > 255 )
            {
                m_txt_Blue.Text = "255";
                m_rgb = Color.FromArgb(m_rgb.A, m_rgb.R, m_rgb.G, 255);
            }
            else
            {
                m_rgb = Color.FromArgb(m_rgb.A, m_rgb.R, m_rgb.G, blue);
            }

            m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
            m_cmyk = AdobeColors.RGB_to_CMYK(m_rgb);
            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;
            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);

            UpdateTextBoxes();
        }

        private void m_txt_A_Leave(object sender, System.EventArgs e)
        {
            string text = m_txt_A.Text;
            bool has_illegal_chars = false;

            if ( text.Length <= 0 )
                has_illegal_chars = true;
            else
                foreach ( char letter in text )
                {
                    if ( !char.IsNumber(letter) )
                    {
                        has_illegal_chars = true;
                        break;
                    }
                }

            if ( has_illegal_chars )
            {
                MessageBox.Show("Alpha must be a number between 0 and 255".Localize());
                UpdateTextBoxes();
                return;
            }

            int alpha = int.Parse(text);

            if ( alpha < 0 )
            {
                m_txt_Red.Text = "0";
                m_rgb = Color.FromArgb(0, m_rgb.R, m_rgb.G, m_rgb.B);
            }
            else if ( alpha > 255 )
            {
                m_txt_Red.Text = "255";
                m_rgb = Color.FromArgb(255, m_rgb.R, m_rgb.G, m_rgb.B);
            }
            else
            {
                m_rgb = Color.FromArgb(alpha, m_rgb.R, m_rgb.G, m_rgb.B);
            }

            m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
            m_cmyk = AdobeColors.RGB_to_CMYK(m_rgb);
            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;
            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);

            UpdateTextBoxes();
        }

        private void m_txt_Cyan_Leave(object sender, System.EventArgs e)
        {
            string text = m_txt_Cyan.Text;
            bool has_illegal_chars = false;

            if ( text.Length <= 0 )
                has_illegal_chars = true;
            else
                foreach ( char letter in text )
                {
                    if ( !char.IsNumber(letter) )
                    {
                        has_illegal_chars = true;
                        break;
                    }
                }

            if ( has_illegal_chars )
            {
                MessageBox.Show("Cyan must be a number between 0 and 100".Localize());
                UpdateTextBoxes();
                return;
            }

            int cyan = int.Parse(text);

            if ( cyan < 0 )
            {
                m_cmyk.C = 0.0;
            }
            else if ( cyan > 100 )
            {
                m_cmyk.C = 1.0;
            }
            else
            {
                m_cmyk.C = (double)cyan/100;
            }

            m_rgb = AdobeColors.CMYK_to_RGB(m_cmyk);
            m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;
            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);

            UpdateTextBoxes();
        }


        private void m_txt_Magenta_Leave(object sender, System.EventArgs e)
        {
            string text = m_txt_Magenta.Text;
            bool has_illegal_chars = false;

            if ( text.Length <= 0 )
                has_illegal_chars = true;
            else
                foreach ( char letter in text )
                {
                    if ( !char.IsNumber(letter) )
                    {
                        has_illegal_chars = true;
                        break;
                    }
                }

            if ( has_illegal_chars )
            {
                MessageBox.Show("Magenta must be a number between 0 and 100".Localize());
                UpdateTextBoxes();
                return;
            }

            int magenta = int.Parse(text);

            if ( magenta < 0 )
            {
                m_txt_Magenta.Text = "0";
                m_cmyk.M = 0.0;
            }
            else if ( magenta > 100 )
            {
                m_txt_Magenta.Text = "100";
                m_cmyk.M = 1.0;
            }
            else
            {
                m_cmyk.M = (double)magenta/100;
            }

            m_rgb = AdobeColors.CMYK_to_RGB(m_cmyk);
            m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;
            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);

            UpdateTextBoxes();
        }


        private void m_txt_Yellow_Leave(object sender, System.EventArgs e)
        {
            string text = m_txt_Yellow.Text;
            bool has_illegal_chars = false;

            if ( text.Length <= 0 )
                has_illegal_chars = true;
            else
                foreach ( char letter in text )
                {
                    if ( !char.IsNumber(letter) )
                    {
                        has_illegal_chars = true;
                        break;
                    }
                }

            if ( has_illegal_chars )
            {
                MessageBox.Show("Yellow must be a number between 0 and 100".Localize());
                UpdateTextBoxes();
                return;
            }

            int yellow = int.Parse(text);

            if ( yellow < 0 )
            {
                m_txt_Yellow.Text = "0";
                m_cmyk.Y = 0.0;
            }
            else if ( yellow > 100 )
            {
                m_txt_Yellow.Text = "100";
                m_cmyk.Y = 1.0;
            }
            else
            {
                m_cmyk.Y = (double)yellow/100;
            }

            m_rgb = AdobeColors.CMYK_to_RGB(m_cmyk);
            m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;
            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);

            UpdateTextBoxes();
        }


        private void m_txt_K_Leave(object sender, System.EventArgs e)
        {
            string text = m_txt_K.Text;
            bool has_illegal_chars = false;

            if ( text.Length <= 0 )
                has_illegal_chars = true;
            else
                foreach ( char letter in text )
                {
                    if ( !char.IsNumber(letter) )
                    {
                        has_illegal_chars = true;
                        break;
                    }
                }

            if ( has_illegal_chars )
            {
                MessageBox.Show("Key must be a number between 0 and 100".Localize());
                UpdateTextBoxes();
                return;
            }

            int key = int.Parse(text);

            if ( key < 0 )
            {
                m_txt_K.Text = "0";
                m_cmyk.K = 0.0;
            }
            else if ( key > 100 )
            {
                m_txt_K.Text = "100";
                m_cmyk.K = 1.0;
            }
            else
            {
                m_cmyk.K = (double)key/100;
            }

            m_rgb = AdobeColors.CMYK_to_RGB(m_cmyk);
            m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
            m_ctrl_BigBox.HSL = m_hsl;
            m_ctrl_ThinBox.HSL = m_hsl;
            m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);

            UpdateTextBoxes();
        }


        #endregion

        #endregion

        #region Private Functions

        private int Round(double val)
        {
            int ret_val = (int)val;
            
            int temp = (int)(val * 100);

            if ( (temp % 100) >= 50 )
                ret_val += 1;

            return ret_val;
        }


        private void WriteHexData(Color rgb)
        {
            string alpha = Convert.ToString(rgb.A, 16);
            if ( alpha.Length < 2 ) alpha = "0" + alpha;
            string red = Convert.ToString(rgb.R, 16);
            if ( red.Length < 2 ) red = "0" + red;
            string green = Convert.ToString(rgb.G, 16);
            if ( green.Length < 2 ) green = "0" + green;
            string blue = Convert.ToString(rgb.B, 16);
            if ( blue.Length < 2 ) blue = "0" + blue;

            if (m_enableAlpha)
                m_txt_Hex.Text = alpha.ToUpper() + red.ToUpper() + green.ToUpper() + blue.ToUpper();
            else
                m_txt_Hex.Text = red.ToUpper() + green.ToUpper() + blue.ToUpper();
            m_txt_Hex.Update();
        }


        private Color ParseHexData(string hex_data)
        {
            // 6 hex chars for RGB, 8 for ARGB
            int length = (m_enableAlpha ? 8 : 6);
            if ( hex_data.Length != length )
                return Color.Empty;

            string a_text, r_text, g_text, b_text;
            int a, r, g, b;

            if (m_enableAlpha)
            {
                a_text = hex_data.Substring(0, 2);
                r_text = hex_data.Substring(2, 2);
                g_text = hex_data.Substring(4, 2);
                b_text = hex_data.Substring(6, 2);

                a = int.Parse(a_text, System.Globalization.NumberStyles.HexNumber);
                r = int.Parse(r_text, System.Globalization.NumberStyles.HexNumber);
                g = int.Parse(g_text, System.Globalization.NumberStyles.HexNumber);
                b = int.Parse(b_text, System.Globalization.NumberStyles.HexNumber);

                return Color.FromArgb(a, r, g, b);
            }
            else
            {
                r_text = hex_data.Substring(0, 2);
                g_text = hex_data.Substring(2, 2);
                b_text = hex_data.Substring(4, 2);

                r = int.Parse(r_text, System.Globalization.NumberStyles.HexNumber);
                g = int.Parse(g_text, System.Globalization.NumberStyles.HexNumber);
                b = int.Parse(b_text, System.Globalization.NumberStyles.HexNumber);

                return Color.FromArgb(r, g, b);
            }

        }


        private void UpdateTextBoxes()
        {
            m_txt_Hue.Text =        Round(m_hsl.H * 360).ToString();
            m_txt_Sat.Text =        Round(m_hsl.S * 100).ToString();
            m_txt_Bright.Text =     Round(m_hsl.L * 100).ToString();
            m_txt_Cyan.Text =       Round(m_cmyk.C * 100).ToString();
            m_txt_Magenta.Text =    Round(m_cmyk.M * 100).ToString();
            m_txt_Yellow.Text =     Round(m_cmyk.Y * 100).ToString();
            m_txt_K.Text =          Round(m_cmyk.K * 100).ToString();
            m_txt_Red.Text =        m_rgb.R.ToString();
            m_txt_Green.Text =      m_rgb.G.ToString();
            m_txt_Blue.Text =       m_rgb.B.ToString();
            m_txt_A.Text =          m_rgb.A.ToString();

            m_txt_Red.Update();
            m_txt_Green.Update();
            m_txt_Blue.Update();
            m_txt_Hue.Update();
            m_txt_Sat.Update();
            m_txt_Bright.Update();
            m_txt_Cyan.Update();
            m_txt_Magenta.Update();
            m_txt_Yellow.Update();
            m_txt_K.Update();
            m_txt_A.Update();

            WriteHexData(m_rgb);
        }

        private Color MakeOpaque(Color color)
        {
            return Color.FromArgb(255, color.R, color.G, color.B);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets or sets the primary color</summary>
        /// <value>The primary color</value>
        public Color PrimaryColor
        {
            get
            {
                return m_rgb;
            }
            set
            {
                m_rgb = value;
                m_hsl = AdobeColors.RGB_to_HSL(m_rgb);

                m_txt_Hue.Text =        Round(m_hsl.H * 360).ToString();
                m_txt_Sat.Text =        Round(m_hsl.S * 100).ToString();
                m_txt_Bright.Text =     Round(m_hsl.L * 100).ToString();
                m_txt_Red.Text =        m_rgb.R.ToString();
                m_txt_Green.Text =      m_rgb.G.ToString();
                m_txt_Blue.Text =       m_rgb.B.ToString();
                m_txt_A.Text =          m_rgb.A.ToString();

                m_txt_Hue.Update();
                m_txt_Sat.Update();
                m_txt_Red.Update();
                m_txt_Green.Update();
                m_txt_Blue.Update();
                m_txt_A.Update();

                m_ctrl_BigBox.HSL = m_hsl;
                m_ctrl_ThinBox.HSL = m_hsl;

                m_lbl_Primary_Color.BackColor = MakeOpaque(m_rgb);
            }
        }

        /// <summary>
        /// Gets or sets the draw style</summary>
        /// <value>The draw style</value>
        public ColorPickerDrawStyle DrawStyle
        {
            get
            {
                if ( m_rbtn_Hue.Checked )
                    return ColorPickerDrawStyle.Hue;
                else if ( m_rbtn_Sat.Checked )
                    return ColorPickerDrawStyle.Saturation;
                else if ( m_rbtn_Bright.Checked )
                    return ColorPickerDrawStyle.Brightness;
                else if ( m_rbtn_Red.Checked )
                    return ColorPickerDrawStyle.Red;
                else if ( m_rbtn_Green.Checked )
                    return ColorPickerDrawStyle.Green;
                else if ( m_rbtn_Blue.Checked )
                    return ColorPickerDrawStyle.Blue;
                else
                    return ColorPickerDrawStyle.Hue;
            }
            set
            {
                switch(value)
                {
                    case ColorPickerDrawStyle.Hue :
                        m_rbtn_Hue.Checked = true;
                        break;
                    case ColorPickerDrawStyle.Saturation :
                        m_rbtn_Sat.Checked = true;
                        break;
                    case ColorPickerDrawStyle.Brightness :
                        m_rbtn_Bright.Checked = true;
                        break;
                    case ColorPickerDrawStyle.Red :
                        m_rbtn_Red.Checked = true;
                        break;
                    case ColorPickerDrawStyle.Green :
                        m_rbtn_Green.Checked = true;
                        break;
                    case ColorPickerDrawStyle.Blue :
                        m_rbtn_Blue.Checked = true;
                        break;
                    default :
                        m_rbtn_Hue.Checked = true;
                        break;
                }
            }
        }

        #endregion
    }
}
