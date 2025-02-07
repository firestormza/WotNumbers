﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Common;
using System.Data.SQLite;
using System.Net.Http;

namespace WinAdmin
{
	public partial class GetTankDataFromAPI : Form
	{
		#region variables

		private enum WotApiType
		{
			Tank = 1,
			Turret = 2,
			Gun = 3,
			Radio = 4,
			Achievement = 5,
			TankDetails = 6,
            Vehicles = 7
		}

		#endregion
		
		public GetTankDataFromAPI()
		{
			InitializeComponent();
		}

		private void GetTankDataFromAPI_Load(object sender, EventArgs e)
		{
			
		}

		private void GetTankDataFromAPI_Shown(object sender, EventArgs e)
		{
			
		}

		private async void cmdStart_Click(object sender, EventArgs e)
		{
			pbStatus.Value = 0;
			pbStatus.Maximum = 100;
			Refresh();
			if (chkFetchNewTanks.Checked)
				await ImportTanks();
			ImportTankImg();
		}

		#region fetchFromAPI

		private async static Task<string> FetchFromAPI(WotApiType WotAPi, int tankId, string server)
		{
			try
			{
                // Get app id
                string applicationId = "0a7f2eb79dce0dd45df9b8fedfed7530"; // EU
                string url = "https://api.worldoftanks.eu";
                if (server == "NA")
                {
                    applicationId = "417860beae5ef8a03e11520aaacbf123"; // NA
                    url = "https://api.worldoftanks.com";
                }
                
                // Get correct api url per api type
                if (WotAPi == WotApiType.Tank)
				{
					// OLD
                    url += "/wot/encyclopedia/tanks/?application_id=" + applicationId + "&fields=short_name_i18n%2Cimage%2Cimage_small%2Ccontour_image"; // EU
				}
                else if (WotAPi == WotApiType.Vehicles)
                {
                    // NEW
                    url += "/wot/encyclopedia/vehicles/?application_id=" + applicationId + "&fields=short_name%2Cimages";

                }

                // NOT IN USE

                //else if (WotAPi == WotApiType.TankDetails)
                //{
                //    // OLD
                //    url = "/wot/encyclopedia/tankinfo/?application_id=0a7f2eb79dce0dd45df9b8fedfed7530&tank_id=" + tankId;
                //}

                //else if (WotAPi == WotApiType.Turret)
                //{
                //    url = "https://api.worldoftanks.eu/wot/encyclopedia/tankturrets/?application_id=0a7f2eb79dce0dd45df9b8fedfed7530";
                //    // itemsInDB = await DB.FetchData("select id from modTurret");   // Fetch id of turrets already existing in db
                //}
                //else if (WotAPi == WotApiType.Gun)
                //{
                //    url = "https://api.worldoftanks.eu/wot/encyclopedia/tankguns/?application_id=0a7f2eb79dce0dd45df9b8fedfed7530";
                //    // itemsInDB = await DB.FetchData("select id from modGun");   // Fetch id of guns already existing in db
                //}
                //else if (WotAPi == WotApiType.Radio)
                //{
                //    url = "https://api.worldoftanks.eu/wot/encyclopedia/tankradios/?application_id=0a7f2eb79dce0dd45df9b8fedfed7530";
                //    // itemsInDB = await DB.FetchData("select id from modRadio");   // Fetch id of radios already existing in db
                //}
                //else if (WotAPi == WotApiType.Achievement)
                //{
                //    url = "https://api.worldoftanks.eu/wot/encyclopedia/achievements/?application_id=0a7f2eb79dce0dd45df9b8fedfed7530";
                //}

                HttpClient client = new HttpClient()
                {
                    Timeout = new TimeSpan(0, 0, 10) // 10 seconds
                };
                client.DefaultRequestHeaders.Add("User-Agent", "Wot Numbers Admin");
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Could not connect to WoT API, please check your Internet access." + Environment.NewLine + Environment.NewLine +
					ex.Message, "Problem connecting to WoT API");
				return "";
			}
			
		}

		#endregion

		#region importTanks

		private async Task ImportTanks()
		{
			int itemCount;
			JToken rootToken;
			JToken itemToken;
			int itemId;
			string insertSql;
			string updateSql;

			lblStatus.Text = "Getting json data from Wot API (" + DateTime.Now.ToString() + ")";
			string server = "EU";
			if (rbNA.Checked) server = "NA";
			string json = await FetchFromAPI(WotApiType.Vehicles, 0, server);
			if (json == "")
			{
				MessageBox.Show("No data imported, no json result from WoT API.","Error");
			}
			else
			{
				bool tankExists = false;
				lblStatus.Text = "Start checking tanks (" + DateTime.Now.ToString() + ")";
				Refresh();
				try
				{
					JObject allTokens = JObject.Parse(json);
					rootToken = allTokens.First;   // returns status token

					if (((JProperty)rootToken).Name.ToString() == "status" && ((JProperty)rootToken).Value.ToString() == "ok")
					{
						
						rootToken = rootToken.Next;
						itemCount = (int)((JProperty)rootToken.First.First).Value;   // returns count (not in use for now)
						rootToken = rootToken.Next;   // start reading tanks
						JToken tanks = rootToken.Children().First();   // read all tokens in data token

						//List<string> logtext = new List<string>();
						string sqlTotal = "";
						DB.DBResult result = new DB.DBResult();
						int actualcount = 0;
						foreach (JProperty tank in tanks)   // tank = tankId + child tokens
						{
							actualcount++;
							itemToken = tank.First();   // First() returns only child tokens of tank

							itemId = Int32.Parse(((JProperty)itemToken.Parent).Name);   // step back to parent to fetch the isolated tankId
                            string name = itemToken["short_name"].ToString();
                            JToken imageToken = itemToken["images"];
                            string imgPath = imageToken["big_icon"].ToString();
                            string smallImgPath = imageToken["small_icon"].ToString();
                            string contourImgPath = imageToken["contour_icon"].ToString();
							// Write to db
							string sql = "select 1 from tank where id=@id";
							DB.AddWithValue(ref sql, "@id", itemId, DB.SqlDataType.Int, Settings.Config);
							DataTable dt = DB.FetchData(sql, Settings.Config, out result);
							tankExists = (dt.Rows.Count > 0);
							insertSql = "INSERT INTO tank (id, name, imgPath, smallImgPath, contourImgPath) VALUES (@id, @name, @imgPath, @smallImgPath, @contourImgPath); ";
							updateSql = "UPDATE tank set name=@name, imgPath=@imgPath, smallImgPath=@smallImgPath, contourImgPath=@contourImgPath WHERE id=@id; ";

							// insert if tank does not exist
							if (!tankExists)
							{
								DB.AddWithValue(ref insertSql, "@id", itemId, DB.SqlDataType.Int, Settings.Config);
								DB.AddWithValue(ref insertSql, "@name", name, DB.SqlDataType.VarChar, Settings.Config);
								DB.AddWithValue(ref insertSql, "@imgPath", imgPath, DB.SqlDataType.VarChar, Settings.Config);
								DB.AddWithValue(ref insertSql, "@smallImgPath", smallImgPath, DB.SqlDataType.VarChar, Settings.Config);
								DB.AddWithValue(ref insertSql, "@contourImgPath", contourImgPath, DB.SqlDataType.VarChar, Settings.Config);
								sqlTotal += insertSql + Environment.NewLine;
							}

							// update if tank exists
							else
							{
								DB.AddWithValue(ref updateSql, "@id", itemId, DB.SqlDataType.Int, Settings.Config);
								DB.AddWithValue(ref updateSql, "@name", name, DB.SqlDataType.VarChar, Settings.Config);
								DB.AddWithValue(ref updateSql, "@imgPath", imgPath, DB.SqlDataType.VarChar, Settings.Config);
								DB.AddWithValue(ref updateSql, "@smallImgPath", smallImgPath, DB.SqlDataType.VarChar, Settings.Config);
								DB.AddWithValue(ref updateSql, "@contourImgPath", contourImgPath, DB.SqlDataType.VarChar, Settings.Config);
								sqlTotal += updateSql + Environment.NewLine;
							}
						}
						lblStatus.Text = "Saving to DB";
                        Refresh();
						DB.ExecuteNonQuery(sqlTotal, Settings.Config, out result, true); // Run all SQL in batch
					}

					lblStatus.Text = "Tank import complete";
                    Refresh();
				}

				catch (Exception ex)
				{
					MessageBox.Show("Import error occured: " + Environment.NewLine + Environment.NewLine + ex.Message, "Error");
				}
			}
		}

        private async Task ImportTanksOLD()
        {
            int itemCount;
            JToken rootToken;
            JToken itemToken;
            int itemId;
            string insertSql;
            string updateSql;

            lblStatus.Text = "Getting json data from Wot (OLD) API (" + DateTime.Now.ToString() + ")";
            string server = "EU";
            if (rbNA.Checked) server = "NA";
            string json = await FetchFromAPI(WotApiType.Tank, 0, server);
            if (json == "")
            {
                MessageBox.Show("No data imported, no json result from WoT API.", "Error");
            }
            else
            {
                bool tankExists = false;
                lblStatus.Text = "Start checking tanks (" + DateTime.Now.ToString() + ")";
                Refresh();
                try
                {
                    JObject allTokens = JObject.Parse(json);
                    rootToken = allTokens.First;   // returns status token

                    if (((JProperty)rootToken).Name.ToString() == "status" && ((JProperty)rootToken).Value.ToString() == "ok")
                    {

                        rootToken = rootToken.Next;
                        itemCount = (int)((JProperty)rootToken.First.First).Value;   // returns count (not in use for now)
                        rootToken = rootToken.Next;   // start reading tanks
                        JToken tanks = rootToken.Children().First();   // read all tokens in data token

                        //List<string> logtext = new List<string>();
                        string sqlTotal = "";
                        DB.DBResult result = new DB.DBResult();
                        int actualcount = 0;
                        foreach (JProperty tank in tanks)   // tank = tankId + child tokens
                        {
                            actualcount++;
                            itemToken = tank.First();   // First() returns only child tokens of tank

                            itemId = Int32.Parse(((JProperty)itemToken.Parent).Name);   // step back to parent to fetch the isolated tankId
                            string name = itemToken["short_name_i18n"].ToString();
                            string imgPath = itemToken["image"].ToString();
                            string smallImgPath = itemToken["image_small"].ToString();
                            string contourImgPath = itemToken["contour_image"].ToString();
                            // Write to db
                            string sql = "select 1 from tank where id=@id";
                            DB.AddWithValue(ref sql, "@id", itemId, DB.SqlDataType.Int, Settings.Config);
                            DataTable dt = DB.FetchData(sql, Settings.Config, out result);
                            tankExists = (dt.Rows.Count > 0);
                            insertSql = "INSERT INTO tank (id, name, imgPath, smallImgPath, contourImgPath) VALUES (@id, @name, @imgPath, @smallImgPath, @contourImgPath); ";
                            updateSql = "UPDATE tank set name=@name, imgPath=@imgPath, smallImgPath=@smallImgPath, contourImgPath=@contourImgPath WHERE id=@id; ";

                            // insert if tank does not exist
                            if (!tankExists)
                            {
                                DB.AddWithValue(ref insertSql, "@id", itemId, DB.SqlDataType.Int, Settings.Config);
                                DB.AddWithValue(ref insertSql, "@name", name, DB.SqlDataType.VarChar, Settings.Config);
                                DB.AddWithValue(ref insertSql, "@imgPath", imgPath, DB.SqlDataType.VarChar, Settings.Config);
                                DB.AddWithValue(ref insertSql, "@smallImgPath", smallImgPath, DB.SqlDataType.VarChar, Settings.Config);
                                DB.AddWithValue(ref insertSql, "@contourImgPath", contourImgPath, DB.SqlDataType.VarChar, Settings.Config);
                                sqlTotal += insertSql + Environment.NewLine;
                            }

                            // update if tank exists
                            else
                            {
                                DB.AddWithValue(ref updateSql, "@id", itemId, DB.SqlDataType.Int, Settings.Config);
                                DB.AddWithValue(ref updateSql, "@name", name, DB.SqlDataType.VarChar, Settings.Config);
                                DB.AddWithValue(ref updateSql, "@imgPath", imgPath, DB.SqlDataType.VarChar, Settings.Config);
                                DB.AddWithValue(ref updateSql, "@smallImgPath", smallImgPath, DB.SqlDataType.VarChar, Settings.Config);
                                DB.AddWithValue(ref updateSql, "@contourImgPath", contourImgPath, DB.SqlDataType.VarChar, Settings.Config);
                                sqlTotal += updateSql + Environment.NewLine;
                            }
                        }
                        lblStatus.Text = "Saving to DB";
                        Refresh();
                        DB.ExecuteNonQuery(sqlTotal, Settings.Config, out result, true); // Run all SQL in batch
                    }

                    lblStatus.Text = "Tank import complete";
                    Refresh();
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Import error occured: " + Environment.NewLine + Environment.NewLine + ex.Message, "Error");
                }
            }
        }

		#endregion

		#region ImportImgLinks

		private void ImportTankImg()  
		{
			lblStatus.Text = "Getting images (" + DateTime.Now.ToString() + ")";
            Refresh();
			DB.DBResult result = new DB.DBResult();
            string sql = "select * from tank ";
            if (txtTankId.Text != "")
            {
                sql += "where id=" + txtTankId.Text;
            }
            DataTable dtTanks = DB.FetchData(sql, Settings.Config, out result);   // Fetch id of tanks in db
			pbStatus.Maximum = dtTanks.Rows.Count; // Two part impot, first tanks, then download img
			foreach (DataRow dr in dtTanks.Rows)
			{
				pbStatus.Value++;
                int tankId = Convert.ToInt32(dr["id"]);
                Refresh();
				bool imgExist = (dr["img"] != DBNull.Value && dr["smallImg"] != DBNull.Value && dr["contourImg"] != DBNull.Value);
				if (!imgExist || !chkKeepExistingImg.Checked || tankId.ToString() == txtTankId.Text)
				{
					lblStatus.Text = "Downloading images for tank: " + dr["id"].ToString();
					byte[] img = getImageFromAPI(dr["imgPath"].ToString());
					byte[] smallImg = getImageFromAPI(dr["smallImgPath"].ToString());
					byte[] contourImg = getImageFromAPI(dr["contourImgPath"].ToString());
				
					//if image not found, skip
					if (img != null && smallImg != null && contourImg != null)
					{
						// SQL Lite binary insert
						string conString = Config.DatabaseConnection(Settings.Config);
						SQLiteConnection con = new SQLiteConnection(conString);
						SQLiteCommand cmd = con.CreateCommand();
						cmd.CommandText = "UPDATE tank SET img=@img, smallImg=@smallImg, contourImg=@contourImg WHERE id=@id";
						SQLiteParameter imgParam = new SQLiteParameter("@img", DbType.Binary);
						SQLiteParameter smallImgParam = new SQLiteParameter("@smallImg", DbType.Binary);
						SQLiteParameter contourImgParam = new SQLiteParameter("@contourImg", DbType.Binary);
						SQLiteParameter idParam = new SQLiteParameter("@id", DbType.Int32);
						imgParam.Value = img;
						smallImgParam.Value = smallImg;
						contourImgParam.Value = contourImg;
						idParam.Value = dr["id"];
						cmd.Parameters.Add(imgParam);
						cmd.Parameters.Add(smallImgParam);
						cmd.Parameters.Add(contourImgParam);
						cmd.Parameters.Add(idParam);
						con.Open();
						try
						{
							cmd.ExecuteNonQuery();
						}
						catch (Exception ex)
						{
							MessageBox.Show(ex.Message);
						}
						con.Close();
					}
				}
			}
			lblStatus.Text = "Tank image import complete";
		}

		#endregion

		#region getImageFromAPI

		public static byte[] getImageFromAPI(string url)
		{
			byte[] imgArray = null;

			// Fetch image from url
			WebRequest req = WebRequest.Create(url);
			try
			{
				WebResponse response = req.GetResponse();
				Stream stream = response.GetResponseStream();

				// Read into memoryStream
				int dataLength = (int)response.ContentLength;
				byte[] buffer = new byte[1024];
				MemoryStream memStream = new MemoryStream();
				while (true)
				{
					int bytesRead = stream.Read(buffer, 0, buffer.Length);  //Try to read the data
					if (bytesRead == 0) break;
					memStream.Write(buffer, 0, bytesRead);  //Write the downloaded data
				}

				// Read into byte array
				//Image image = Image.FromStream(memStream);
				imgArray = memStream.ToArray();
			}
			catch (Exception ex)
			{
				// error
				string msg = ex.Message;
				//MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + url , "Not found file");
			}
			
			return imgArray;
		}

		#endregion

        private async void button1_Click(object sender, EventArgs e)
        {
            pbStatus.Value = 0;
            pbStatus.Maximum = 100;
            Refresh();
            if (chkFetchNewTanks.Checked)
                await ImportTanksOLD();
            ImportTankImg();
        }

		


	}
}
