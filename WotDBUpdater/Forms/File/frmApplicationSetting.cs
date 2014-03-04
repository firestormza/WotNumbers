﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WotDBUpdater
{
    public partial class frmApplicationSetting : Form
    {
        public frmApplicationSetting()
        {
            InitializeComponent();
        }

        private void frmDossierFileSelect_Load(object sender, EventArgs e)
        {
            // Startup settings
            txtDossierFilePath.Text = Config.Settings.dossierFilePath;
            UpdatePlayerList();
        }

        private void UpdatePlayerList()
        {
            try
            {
                cboPlayer.Items.Clear();
                using (SqlConnection con = new SqlConnection(Config.DatabaseConnection()))
                {
                    con.Open();
                    string sql = "SELECT * FROM player";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        cboPlayer.Items.Add(reader["name"]);
                    }
                    con.Close();
                }
                cboPlayer.SelectedIndex = cboPlayer.FindString(Config.Settings.playerName);
            }
            catch (Exception)
            {
                // none
            }
        }

        private void btnOpenDossierFile_Click(object sender, EventArgs e)
        {
            // Select dossier file
            openFileDialogDossierFile.FileName = "*.dat";
            if (txtDossierFilePath.Text == "")
            {
                openFileDialogDossierFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wargaming.net\\WorldOfTanks\\dossier_cache";
            }
            else
            {
                openFileDialogDossierFile.InitialDirectory = txtDossierFilePath.Text;
            }
            openFileDialogDossierFile.ShowDialog();
            // If file selected save config with new values
            if (openFileDialogDossierFile.FileName != "")
            {
                txtDossierFilePath.Text = Path.GetDirectoryName(openFileDialogDossierFile.FileName);  
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Config.Settings.dossierFilePath = txtDossierFilePath.Text;
            Config.Settings.playerName = cboPlayer.Text;
            string msg = "";
            bool saveOk = false;
            saveOk = Config.SaveAppConfig(out msg);
            MessageBox.Show(msg, "Save application settings");
            if (saveOk) 
            {
                Form.ActiveForm.Close();
            }
        }

        private void btnAddPlayer_Click(object sender, EventArgs e)
        {
            Form frm = new Forms.File.frmAddPlayer();
            frm.ShowDialog();
            UpdatePlayerList();
        }

        private void btnRemovePlayer_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to remove player: " + cboPlayer.Text + " ?", "Remove player", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    SqlConnection con = new SqlConnection(Config.DatabaseConnection());
                    con.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM player WHERE name=@name", con);
                    cmd.Parameters.AddWithValue("@name", cboPlayer.Text);
                    cmd.ExecuteNonQuery();
                    con.Close();
                    MessageBox.Show("Player successfully removed.", "Player removed");
                    UpdatePlayerList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cannot remove this player, probaly because data is stored for the player. Only players without any data can be removed.\n\n" + ex.Message, "Cannot remove player");
                }
            }
        }


    }
}
