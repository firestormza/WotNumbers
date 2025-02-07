﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using WinApp.Code;

namespace WinApp.Forms
{
	public partial class PlayerTankDetail : FormCloseOnEsc
    {
		int initPlayerTankId = 0;
		int initTankId = 0;
		public PlayerTankDetail(int playerTankId = 0, int tankId = 0)
		{
			InitializeComponent();
			initPlayerTankId = playerTankId;
			initTankId = tankId;
        }

		// To be able to minimize from task bar
		const int WS_MINIMIZEBOX = 0x20000;
		const int CS_DBLCLKS = 0x8;
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.Style |= WS_MINIMIZEBOX;
				cp.ClassStyle |= CS_DBLCLKS;
				return cp;
			}
		}

		private async void PlayerTankDetails_Load(object sender, EventArgs e)
		{
			// Mouse scrolling
			dataGridTankDetail.MouseWheel += new MouseEventHandler(dataGridTankDetail_MouseWheel);
			// Style datagrid
			GridHelper.StyleDataGrid(dataGridTankDetail);
			lblFooter.Text = "";
			ResizeNow();
			if (initPlayerTankId != 0 || initTankId != 0)
			{
				// Get tank id and name
				string sql = "SELECT * FROM tank INNER JOIN playerTank ON tank.id = playerTank.tankId WHERE playerTank.id=@id; ";
				DB.AddWithValue(ref sql, "@id", initPlayerTankId, DB.SqlDataType.Int);
				// If no playertank, get only tank data
				if (initPlayerTankId == 0)
				{
					sql = "SELECT * FROM tank WHERE id=@id; ";
					DB.AddWithValue(ref sql, "@id", initTankId, DB.SqlDataType.Int);
				}
                DataTable dt = await DB.FetchData(sql);
                DataRow drTankData = dt.Rows[0];
				string tankName = drTankData["name"].ToString();
				int tankId = Convert.ToInt32(drTankData["id"]);
				// Show name in title bar
				PlayerTankDetailsTheme.Text = tankName;
				// Show picture
				picLarge.Image = ImageHelper.GetTankImage(tankId,"img");
				// Get all columns to show as rows in grid
                var tankDataColListItems = await ColListHelper.GetAllTankDataColumn();
                // Use playerTankBattleTotalsView in stead of playerTankBattle to show totals
                tankDataColListItems.Select = tankDataColListItems.Select.Replace("playerTankBattle", "playerTankBattleTotalsView");
				// Get Tank data to show in grid
				string playerTankWhere = "";
				string playerTankJoin = "";
				if (initPlayerTankId != 0)
				{
					playerTankWhere = "playerTank.playerId=@playerId and ";
					playerTankJoin = "playerTankBattleTotalsView ON playerTankBattleTotalsView.playerTankId = playerTank.id LEFT OUTER JOIN ";
				}
				else
					playerTankJoin = "playerTankBattleTotalsView ON playerTankBattleTotalsView.playerTankId = -1 LEFT OUTER JOIN ";
				sql =
					"SELECT   " + tankDataColListItems.Select + " " + Environment.NewLine +
					"FROM     tank LEFT JOIN " + Environment.NewLine +
					"         playerTank ON tank.id = playerTank.tankId INNER JOIN " + Environment.NewLine +
					"         tankType ON tank.tankTypeId = tankType.id INNER JOIN " + Environment.NewLine +
					"         country ON tank.countryId = country.id LEFT JOIN " + Environment.NewLine +
					"         " + playerTankJoin + Environment.NewLine +
					"         modTurret ON playerTank.modTurretId = modTurret.id LEFT OUTER JOIN " + Environment.NewLine +
					"         modRadio ON modRadio.id = playerTank.modRadioId LEFT OUTER JOIN " + Environment.NewLine +
					"         modGun ON playerTank.modGunId = modGun.id " + Environment.NewLine +
					"WHERE    " + playerTankWhere + " tank.id=@tankId ";
				DB.AddWithValue(ref sql, "@playerId", Config.Settings.playerId, DB.SqlDataType.Int);
				DB.AddWithValue(ref sql, "@tankId", tankId, DB.SqlDataType.Int);
				DataTable dtTankData = await DB.FetchData(sql, Config.Settings.showDBErrors);
				// Make cols to rows from the two datatables into new to show in grid
				DataTable dtGrid = new DataTable();
				dtGrid.Columns.Add("Parameter", typeof(string));
				dtGrid.Columns.Add("Value", typeof(string));
				dtGrid.Columns.Add("Header", typeof(bool));
				dtGrid.Columns.Add("ToolTip", typeof(string));
				string currentHeader = "";
				foreach (ColListHelper.ColListItem col in tankDataColListItems.ColListItemList)
				{
					// Add header
					if (currentHeader != col.colGroup)
					{
						DataRow drHeader = dtGrid.NewRow();
						drHeader["Parameter"] = col.colGroup + " Details";
						drHeader["Header"] = true;
						dtGrid.Rows.Add(drHeader);
						currentHeader = col.colGroup;
					}
					// Add value
					DataRow drTankDetail = dtGrid.NewRow();
					drTankDetail["Parameter"] = col.name;
					drTankDetail["Value"] = dtTankData.Rows[0][col.name].ToString();
					drTankDetail["Header"] = false;
					drTankDetail["ToolTip"] = col.description;
					dtGrid.Rows.Add(drTankDetail);
				}
				// Add to grid
				dataGridTankDetail.DataSource = dtGrid;
				// Hiode cols
				dataGridTankDetail.Columns["Header"].Visible = false;
				dataGridTankDetail.Columns["ToolTip"].Visible = false;
				// Unfocus
				dataGridTankDetail.ClearSelection();
				// Connect to scrollbar
				scrollTankDetails.ScrollElementsTotals = dtGrid.Rows.Count;
				scrollTankDetails.ScrollElementsVisible = dataGridTankDetail.DisplayedRowCount(false);
			}
		}

		
		private void ResizeNow()
		{
			dataGridTankDetail.Width = PlayerTankDetailsTheme.MainArea.Width - scrollTankDetails.Width - 1;
			dataGridTankDetail.Height = PlayerTankDetailsTheme.MainArea.Top + PlayerTankDetailsTheme.MainArea.Height - dataGridTankDetail.Top;
			scrollTankDetails.Left = PlayerTankDetailsTheme.MainArea.Right - scrollTankDetails.Width -1;
			scrollTankDetails.Height = dataGridTankDetail.Height;
		}

		private void PlayerTankDetails_Resize(object sender, EventArgs e)
		{
			ResizeNow();
		}

		private void PlayerTankDetails_ResizeEnd(object sender, EventArgs e)
		{
			ResizeNow();
		}

		private void dataGridTankDetail_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			// Dark background on headers
			if (Convert.ToBoolean(dataGridTankDetail["Header", e.RowIndex].Value) == true)
				dataGridTankDetail.Rows[e.RowIndex].DefaultCellStyle.BackColor = ColorTheme.ToolGrayMainBack;
			// Tooltip on Parameters
			if (dataGridTankDetail.Columns[e.ColumnIndex].Name == "Parameter")
				dataGridTankDetail[e.ColumnIndex, e.RowIndex].ToolTipText = dataGridTankDetail["ToolTip", e.RowIndex].Value.ToString();	
		}

		private void dataGridTankDetail_CellEnter(object sender, DataGridViewCellEventArgs e)
		{
			lblFooter.Text = dataGridTankDetail["ToolTip", e.RowIndex].Value.ToString();
		}

		#region Scrolling

		private void MoveScrollBar()
		{
			scrollTankDetails.ScrollPosition = dataGridTankDetail.FirstDisplayedScrollingRowIndex;
		}

		// Enable mouse wheel scrolling for datagrid
		private async void dataGridTankDetail_MouseWheel(object sender, MouseEventArgs e)
		{
			try
			{
				// scroll in grid from mouse wheel
				int currentIndex = this.dataGridTankDetail.FirstDisplayedScrollingRowIndex;
				int scrollLines = SystemInformation.MouseWheelScrollLines;
				if (e.Delta > 0)
				{
					this.dataGridTankDetail.FirstDisplayedScrollingRowIndex = Math.Max(0, currentIndex - scrollLines);
				}
				else if (e.Delta < 0)
				{
					this.dataGridTankDetail.FirstDisplayedScrollingRowIndex = currentIndex + scrollLines;
				}
				// move scrollbar
				MoveScrollBar();
			}
			catch (Exception ex)
			{
				await Log.LogToFile(ex);
				// throw;
			}
		}

		private bool scrolling = false;
		private void scrollTankDetails_MouseDown(object sender, MouseEventArgs e)
		{
			if (dataGridTankDetail.RowCount > 0)
			{
				scrolling = true;
				dataGridTankDetail.FirstDisplayedScrollingRowIndex = scrollTankDetails.ScrollPosition;
			}
		}

		private void scrollTankDetails_MouseMove(object sender, MouseEventArgs e)
		{
			if (dataGridTankDetail.RowCount > 0 && scrolling)
			{
				int currentFirstRow = dataGridTankDetail.FirstDisplayedScrollingRowIndex;
				dataGridTankDetail.FirstDisplayedScrollingRowIndex = scrollTankDetails.ScrollPosition;
				if (currentFirstRow != dataGridTankDetail.FirstDisplayedScrollingRowIndex) Refresh();
			}
		}

		private void scrollTankDetails_MouseUp(object sender, MouseEventArgs e)
		{
			scrolling = false;
		}

		#endregion

		private void PlayerTankDetail_FormClosed(object sender, FormClosedEventArgs e)
		{
			FormHelper.ClosedOne();
		}

		

	}
}
