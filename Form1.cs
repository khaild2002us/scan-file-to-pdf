using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WIA;
using MySql.Data.MySqlClient;

namespace scannerfortest
{
    public partial class Form1 : Form
    {
        private List<string> scannedImages = new List<string>();

        [Obsolete]
        public Form1()
        {
            InitializeComponent();
        }

        [Obsolete]
        private void button1_Click(object sender, EventArgs e)
        {
            // Open the dialog to acquire an image
            WIA.CommonDialog dialog = new WIA.CommonDialog();
            ImageFile image = dialog.ShowAcquireImage(AlwaysSelectDevice: true);

            if (image != null)
            {
                // Create a unique path for the scanned image
                string path = Path.Combine(@"C:\scanner\", tabControl1.SelectedTab.Text + ".jpg");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                image.SaveFile(path);
                scannedImages.Add(path);

                // Display the scanned image in the selected tab's PictureBox
                PictureBox pictureBox = tabControl1.SelectedTab.Controls.OfType<PictureBox>().FirstOrDefault();
                if (pictureBox != null)
                {
                    pictureBox.Image = Image.FromFile(path);
                }

                // Add the path to the scanned images list
                scannedImages.Add(path);
            }
        }


        private void button2_Click_1(object sender, EventArgs e)
        {

            SaveImagesToDatabase();
        }

        private void SaveImagesToDatabase()
        {
            

            try
            {
                string NNI = textBox1.Text;
                byte[] bytesFormulaire = File.ReadAllBytes(@"C:\scanner\Formulaire.jpg");
                byte[] bytesActeNaissance = File.ReadAllBytes(@"C:\scanner\Acte_naissance.jpg");
                byte[] bytesDiplome = File.ReadAllBytes(@"C:\scanner\Diplome.jpg");
                byte[] bytesTemoins = File.ReadAllBytes(@"C:\scanner\Temoins.jpg");
                byte[] bytesQuittance = File.ReadAllBytes(@"C:\scanner\Quittance.jpg");
                DateTime dateTime = DateTime.UtcNow;
                string createdOn = dateTime.ToString("yyyy-MM-dd");
                using (MySqlConnection con = new MySqlConnection("Server=localhost;Database=kmt;Uid=root;Pwd=;"))
                {
                    con.Open();

                    // ضبط max_allowed_packet
                    using (MySqlCommand cmd = new MySqlCommand("SET GLOBAL max_allowed_packet=67108864;", con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    string CmdString = "INSERT INTO demande_folders(nni, formulaire, acte_naissance, diplome, temoins, quittance, date_creation) " +
                                       "VALUES(@nni, @formulaire, @acte_naissance, @diplome, @temoins, @quittance, @date_creation)";
                    using (MySqlCommand cmd = new MySqlCommand(CmdString, con))
                    {
                        cmd.Parameters.AddWithValue("@nni", NNI);
                        cmd.Parameters.AddWithValue("@formulaire", bytesFormulaire);
                        cmd.Parameters.AddWithValue("@acte_naissance", bytesActeNaissance);
                        cmd.Parameters.AddWithValue("@diplome", bytesDiplome);
                        cmd.Parameters.AddWithValue("@temoins", bytesTemoins);
                        cmd.Parameters.AddWithValue("@quittance", bytesQuittance);
                        cmd.Parameters.AddWithValue("@date_creation", createdOn);

                        int RowsAffected = cmd.ExecuteNonQuery();
                        con.Close();

                        if (RowsAffected > 0)
                        {
                            MessageBox.Show("File stored successfully!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to store the file. Please contact the application vendor.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
