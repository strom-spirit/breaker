using System.Windows.Forms;

namespace WindowsFormsApplicationGUIuARM
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }



        private void ButtonMasuk_Click(object sender, System.EventArgs e)
        {
            if(textBoxSandi.Text == "admin" && textBoxNama.Text =="admin")
            {
                this.Hide();
                new FormMain().Show();
            }
            else
            {
                MessageBox.Show("Nama Pengguna atau Kata Sandi tidak tepat");
            }
        }

    }
}
