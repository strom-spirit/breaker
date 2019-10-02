using AForge.Video;
using AForge.Video.DirectShow;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using System.Data;
using MySql.Data.MySqlClient;

using System.IO;

namespace WindowsFormsApplicationGUIuARM
{

    

    public partial class FormMain : Form
    {
        string string_koneksi = "server = localhost; port = 3306; username = root ; password = ; database = ta_pasukan";
        //string db_namadatabase = "ta_pasukan";
        MySqlConnection koneksi = new MySqlConnection();
        MySqlDataAdapter db_select;
        MySqlCommand mcd;
        DataSet db_data;

        GMapOverlay markersOverlay = new GMapOverlay();
        GMarkerGoogle markerMarkas = new GMarkerGoogle(new PointLatLng(), GMarkerGoogleType.red);
        GMarkerGoogle markerPrajurit = new GMarkerGoogle(new PointLatLng(), GMarkerGoogleType.red);

        private Stopwatch stopWatch = null;

        public FormMain()
        {
            InitializeComponent();
        }

        double defaultLatMarkas = -7.220491;
        double defaultLngMarkas = 112.7176309;
        private void Form1_Load(object sender, EventArgs e)
        {
            my_init_gmaps();

            Bitmap bmpPrajurit = new Bitmap(System.IO.Path.Combine(Application.StartupPath, @"img\marker50.png"));
            Bitmap bmpMarkerPrajurit = bmpPrajurit;
            markerPrajurit = new GMarkerGoogle(new PointLatLng(-7.220091, defaultLngMarkas), bmpMarkerPrajurit);


            Bitmap bmpMarkas = new Bitmap(System.IO.Path.Combine(Application.StartupPath, @"img\marker502.png"));
            Bitmap bmpMarkerMarkas = bmpMarkas;
            markerMarkas = new GMarkerGoogle(new PointLatLng(defaultLatMarkas, defaultLngMarkas), bmpMarkerMarkas);

            markersOverlay.Markers.Add(markerMarkas);
            markersOverlay.Markers.Add(markerPrajurit);
            gMapControl1.Overlays.Add(markersOverlay);

            myKonekDB();
            myBacaTabel();


            int i = 0; 
            //C:\Users\win10\Documents
            //string[] dirs = Directory.GetFiles(@"C:\Users\win10\Documents");
            string[] dirs = Directory.GetFileSystemEntries(System.IO.Path.Combine(Application.StartupPath, @"video"));
            foreach (string dir in dirs)
            {
                listView1.Items.Add(dir, i);
                //i++;
                //if (i == imageList1.Images.Count - 1)
                //    i = 0;
            }
               
        }

        private void myBacaTabel()
        {
            db_select = new MySqlDataAdapter("SELECT * FROM tbpesan", koneksi);
            db_data = new System.Data.DataSet();
            db_select.Fill(db_data, "akun");
            dataGridViewPesan.DataSource = db_data.Tables[0];
        }

        private void myKonekDB()
        {
            try
            {
                koneksi.ConnectionString = string_koneksi;
                koneksi.Open();
                if (koneksi.State == ConnectionState.Open)
                {
                    toolStripStatusLabelDb.Text = "Koneksi ke database BERHASIL";
                    toolStripStatusLabelDb.BackColor = Color.Lime;
                }
            }
            catch
            {
                toolStripStatusLabelDb.Text = "Koneksi ke database GAGAL";
                toolStripStatusLabelDb.BackColor = Color.Red;
            }
        }

        String myQuery;
        public void ExecuteQuery(string q)
        {
            try
            {
                
                mcd = new MySqlCommand(q, koneksi);
                mcd.ExecuteNonQuery();
                //if (mcd.ExecuteNonQuery() == 1)
                //{
                //    MessageBox.Show("Query Executed");
                //}
                //else
                //{
                //    MessageBox.Show("Query Not Executed");
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void my_init_gmaps()
        {
            gMapControl1.MapProvider = GMap.NET.MapProviders.BingSatelliteMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;

            gMapControl1.Position = new PointLatLng(defaultLatMarkas, defaultLngMarkas);
            gMapControl1.MinZoom = 0;
            gMapControl1.MaxZoom = 22;
            gMapControl1.Zoom = 19;
            gMapControl1.DragButton = MouseButtons.Left;
        }

        
        private void gpsMarkerMarkas(double latitude, double longitude)
        {

            label_latm.Text = latitude.ToString(".######");
            label_lotm.Text = longitude.ToString(".######");
            markerMarkas.Position = new PointLatLng(latitude, longitude);
            gMapControl1.UpdateMarkerLocalPosition(markerMarkas);
        }

        
        private void gpsMarkerPrajurit(double latitude, double longitude)
        {
            gMapControl1.Position = new PointLatLng(latitude, longitude);

            GMarkerGoogle marker1 = new GMarkerGoogle(new PointLatLng(latitude, longitude), GMarkerGoogleType.green);
            marker1.Position = new PointLatLng(latitude, longitude);
            markersOverlay.Markers.Remove(marker1);
            markersOverlay.Markers.Add(marker1);
        }

        private void my_COMPortAvaliable()
        {
            cboPortName.Items.Clear();
            string[] SerialPort = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string item in SerialPort)
            {
                cboPortName.Items.Add(item);
                cboPortName.Text = item;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormLogin Form21 = new FormLogin();
            Form21.Show();
        }

        string data = "";
        string data_in;
        int byte_data;
        string buffer;
        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                data_in = SerialPort.ReadLine();
            }
            catch { }
            this.BeginInvoke(new EventHandler(my_parse_data_rutin));
        }

        double longitude1, latitude1, alti, satelit, distance, sudut;
        private void my_parse_data_rutin(object sender, EventArgs e)
        {
            try
            {
                int i = 0;
                string[] data_str = new string[25];

                richTextBox_serial.AppendText(DateTime.Now.ToString("HH:mm:ss ") + data_in + "\n");
                richTextBox_serial.ScrollToCaret();

                foreach (string n in data_in.Split('_'))       //data dipisah dengan character "_"
                {
                    if (n == "#")
                        i = 1;
                    data_str[i] = n;                        //data yang sudah dipisah disimpan disini
                    i++;
                }

                if (data_str[1] == "#")
                {
                    try
                    {
                        latitude1 = Convert.ToDouble(data_str[2]);
                        longitude1 = Convert.ToDouble(data_str[3]);
                        alti = Convert.ToDouble(data_str[4]);
                        distance = Convert.ToDouble(data_str[7]);
                        sudut = Convert.ToDouble(data_str[8]);

                        if (longitude1 > 0)
                        {
                            label_lat.Text = data_str[2];
                            label_long.Text = data_str[3];
                            label_alt.Text = data_str[4];
                            label_distance.Text = data_str[7];
                            label_sudut.Text = data_str[8];
                            gpsMarkerPrajurit(latitude1, longitude1);
                        }
                    }
                    catch { }
                }
                data_in = "";
            }
            catch { }
        }


        //////////////////////////////////////////////////////////////////////// UX SEPERTI APLIKASI WINDOWS PADA UMUNYA {{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{
        // DISABLE X BUTTON
        //private const int CP_NOCLOSE_BUTTON = 0x200;
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams myCp = base.CreateParams;
        //        myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
        //        return myCp;
        //    }
        //}

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (PreClosingConfirmation() == System.Windows.Forms.DialogResult.Yes)
            {
                if (SerialPort.IsOpen)
                {
                    SerialPort.Write("D");
                    try
                    {
                        SerialPort.Close();
                    }
                    catch { }

                }
                Dispose(true);
                Application.Exit();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private DialogResult PreClosingConfirmation()
        {
            DialogResult res = System.Windows.Forms.MessageBox.Show(" Apakah ingin keluar?          ", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return res;
        }
        //////////////////////////////////////////////////////////////////////// UX SEPERTI APLIKASI WINDOWS PADA UMUNYA  }}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}}

        private void button1_Click(object sender, EventArgs e)
        {

            var f2 = new FormLogin();
            f2.Owner = this;  // <-- This is the important thing
            f2.ShowDialog(); // VS Show
        }


        //PERINTAH KONEK DAN TIDAK KONEK
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (!SerialPort.IsOpen)
            {
                try
                {
                    SerialPort.PortName = cboPortName.Text;
                    SerialPort.BaudRate = Convert.ToInt32(comboBox_baud.Text);
                    SerialPort.Open();
                    if (SerialPort != null)
                    {
                        Properties.Settings.Default.COM = SerialPort.PortName;
                        Properties.Settings.Default.Save();

                        cboPortName.Enabled = false;
                        buttonSambung.Text = "Disconnect";
                        toolStripStatusLabel2.Text = "CONNECTED TO " + SerialPort.PortName;
                        toolStripProgressBar1.Value = 100;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection Failed");
                }
            }
            else
            {
                try
                {
                    SerialPort.Close();
                }
                catch { }
                cboPortName.Enabled = true;
                buttonSambung.Text = "Connect";
                toolStripStatusLabel2.Text = "DISCONNECTED";
                toolStripProgressBar1.Value = 0;
            }
        }

        private void cboPortName_SelectedIndexChanged(object sender, EventArgs e)
        {
            my_COMPortAvaliable();
        }


        private void cboPortName_Click(object sender, EventArgs e)
        {
            my_COMPortAvaliable();
        }

        private void button16_Click_2(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "csv files (*.csv)|*.csv";

            saveFileDialog1.FileName = String.Format("Data {0}.csv", DateTime.Now.ToString(" yyy-MMM-dd HH mm ss"));

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK && saveFileDialog1.FileName.Length > 0)
            {

                richTextBox_serial.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText); //jika di save
            }
        }


        //TAMPIL PETA
        private void Button6_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 1;
            gpsMarkerPrajurit(Convert.ToDouble(label_lat.Text), Convert.ToDouble(label_long.Text));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            gpsMarkerPrajurit(-7.276884, 112.794983);
        }


        private void tabControl2_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush;

            // Get the item from the collection.
            //TabPage _tabPage = tabControl1.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            //Rectangle _tabBounds = tabControl1.GetTabRect(e.Index);

            if (e.State == DrawItemState.Selected)
            {

                // Draw a different background color, and don't paint a focus rectangle.
                _textBrush = new SolidBrush(Color.Red);
                g.FillRectangle(Brushes.Gray, e.Bounds);
            }
            else
            {
                _textBrush = new System.Drawing.SolidBrush(e.ForeColor);
                e.DrawBackground();
            }

            // Use our own font.
            Font _tabFont = new Font("Arial", (float)10.0, FontStyle.Bold, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Center;
            _stringFlags.LineAlignment = StringAlignment.Center;
            //g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));
        }

        private void OpenVideoSource(IVideoSource source)
        {
            // set busy cursor
            this.Cursor = Cursors.WaitCursor;

            // stop current video source
            CloseCurrentVideoSource();

            // start new video source
            videoSourcePlayer.VideoSource = source;
            videoSourcePlayer.Start();

            // reset stop watch
            stopWatch = null;

            // start timer
            timer.Start();

            this.Cursor = Cursors.Default;
        }

        private void CloseCurrentVideoSource()
        {
            if (videoSourcePlayer.VideoSource != null)
            {
                videoSourcePlayer.SignalToStop();
                // wait ~ 3 seconds
                for (int i = 0; i < 30; i++)
                {
                    if (!videoSourcePlayer.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                if (videoSourcePlayer.IsRunning)
                {
                    videoSourcePlayer.Stop();
                }

                videoSourcePlayer.VideoSource = null;
            }
        }



        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Apakah yakin akan mengganti lokasi?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //gps_markerm(latitude1, longitude1);
            }
        }

        Boolean status_maps = true;
        private void PictureBox3_Click(object sender, EventArgs e)
        {
            if (status_maps)
            {
                status_maps = false;
                Bitmap bmp = new Bitmap(System.IO.Path.Combine(Application.StartupPath, @"img\1.png"));
                pictureBoxMapMode.Image = bmp;
                gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            }
            else
            {
                status_maps = true;
                Bitmap bmp = new Bitmap(System.IO.Path.Combine(Application.StartupPath, @"img\2.png"));
                pictureBoxMapMode.Image = bmp;
                gMapControl1.MapProvider = GMap.NET.MapProviders.BingSatelliteMapProvider.Instance;
            }

        }

        Boolean kameraSize = true;
        private void Panel10_MouseClick(object sender, MouseEventArgs e)
        {
            tabControl2.SelectedIndex = 0;
        }

        private void Panel11_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 1;
        }

        private void Panel13_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 2;
        }

        private void Panel12_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 3;
        }

        private void ButtonKameraAktif_Click(object sender, EventArgs e)
        {
            VideoCaptureDeviceForm form = new VideoCaptureDeviceForm();

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // create video source
                VideoCaptureDevice videoSource = form.VideoDevice;

                // open it
                OpenVideoSource(videoSource);
            }
        }

        private void ButtonPesanBersihkan_Click(object sender, EventArgs e)
        {

            //gMapControl1.Overlays.Remove(markersOverlay);

            if(gMapControl1.Overlays.Count > 0)
            {
                gMapControl1.Overlays.RemoveAt(0);
                gMapControl1.Refresh();

                //label40.Text = gMapControl1.Overlays.Count.ToString();
                //label41.Text = markersOverlay.Markers.Count.ToString();
            }
            

            textBoxPesan.Text = "";
        }

        private void Label4_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 0;
        }

        private void PictureBox6_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 0;
        }

        private void Label5_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 1;
        }

        private void PictureBox4_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 1;
        }

        private void Label15_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 3;
        }

        private void PictureBox7_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 3;
        }

        private void Label33_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 2;
        }

        private void PictureBox8_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 2;
        }

        private void DataGridViewPesan_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            textBoxRiwayatPesan.Text = dataGridViewPesan.SelectedRows[0].Cells[2].Value.ToString();
        }

        private void ButtonRefresh_Click(object sender, EventArgs e)
        {
            myBacaTabel();
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
        }

        private void Button2_Click_1(object sender, EventArgs e)
        {
            listView1.View = View.SmallIcon;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            listView1.View = View.Details;
        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
           

        }

        string currentDir = Directory.GetCurrentDirectory();

        private void ListView1_Click(object sender, EventArgs e)
        {
            String text = listView1.SelectedItems[0].Text;
            label40.Text = text; 
            
        }

        private void Button1_Click_2(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(label40.Text);
        }

        private void ButtonPesanKirim_Click(object sender, EventArgs e)
        {
            if (SerialPort.IsOpen)
            {
                string mydatex = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                //textBoxPesan.Text = mydatex;
                myQuery = "insert into tbpesan (`date`, `message`) values ('" + mydatex + "', '" + textBoxPesan.Text + "')";
                ExecuteQuery(myQuery);


                SerialPort.WriteLine(textBoxPesan.Text);
                MessageBox.Show("Pesan Terkirim");
            }
            

        }

        private void ButtonKameraUkuran_Click(object sender, EventArgs e)
        {
            if (kameraSize)
            {
                buttonKameraUkuran.Text = "Perkecil";
                kameraSize = false;
                videoSourcePlayer.Dock = System.Windows.Forms.DockStyle.Fill;
            }
            else
            {
                buttonKameraUkuran.Text = "Perbesar";
                kameraSize = true;
                videoSourcePlayer.Dock = System.Windows.Forms.DockStyle.None;
            }
        }

        private void ButtonMarkasSet_Click(object sender, EventArgs e)
        {
            gpsMarkerMarkas(latMarkasNew, lngMarkasNew);
        }



        double latMarkasNew, lngMarkasNew;
        private void gMapControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                latMarkasNew = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat;
                lngMarkasNew = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng;

                label3.Text = latMarkasNew.ToString(".######");
                label2.Text = lngMarkasNew.ToString(".######");

                gpsMarkerMarkas(latMarkasNew, lngMarkasNew);
            }
        }


    }
}
