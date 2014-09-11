﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json.Linq;

namespace WinApp.Code
{
	class Battle2json
	{
		private static List<string> battleResultDatFileCopyed = new List<string>(); // List of dat-files copyed from wargaming battle folder, to avoid copy several times
		private static List<string> battleResultJsonFileExists = new List<string>(); // List of json-files already existing in battle folder, to avoid converting several times
		public static FileSystemWatcher battleResultFileWatcher = new FileSystemWatcher();

		private class BattleValues
		{
			public string colname;
			public object value;
		}

		public static void UpdateBattleResultFileWatcher()
		{
			try
			{
				bool run = (Config.Settings.dossierFileWathcherRun == 1);
				if (Directory.Exists(Path.GetDirectoryName(Config.Settings.battleFilePath)))
				{
					battleResultFileWatcher.Path = Path.GetDirectoryName(Config.Settings.battleFilePath);
					battleResultFileWatcher.Filter = "*.dat";
					battleResultFileWatcher.IncludeSubdirectories = true;
					battleResultFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
					battleResultFileWatcher.Changed += new FileSystemEventHandler(BattleResultFileChanged);
					battleResultFileWatcher.EnableRaisingEvents = run;
				}
				else
					battleResultFileWatcher.EnableRaisingEvents = false;
			}
			catch (Exception ex)
			{
				battleResultFileWatcher.EnableRaisingEvents = false;
				Log.LogToFile(ex, "Inncorrect dossier file path");
			}
			
		}

		private static void BattleResultFileChanged(object source, FileSystemEventArgs e)
		{
			RunBattleResultRead();
			Log.WriteLogBuffer();
		}

		public static void GetExistingBattleFiles()
		{
			// Get existing json files
			string[] filesJson = Directory.GetFiles(Config.AppDataBattleResultFolder, "*.json");
			foreach (string file in filesJson)
			{
				battleResultJsonFileExists.Add(Path.GetFileNameWithoutExtension(file).ToString()); // Remove file extension
			}
			// Get existing dst files
			string[] filesDat = Directory.GetFiles(Config.AppDataBattleResultFolder, "*.dat");
			foreach (string file in filesDat)
			{
				battleResultDatFileCopyed.Add(file); // Complete file with path
			}
		}

		public static void ConvertBattleFilesToJson()
		{
			// Get WoT top level battle_result folder for getting dat-files
			if (Directory.Exists(Path.GetDirectoryName(Config.Settings.battleFilePath)))
			{
				DirectoryInfo di = new DirectoryInfo(Config.Settings.battleFilePath);
				DirectoryInfo[] folders = di.GetDirectories();
				// testing one file
				foreach (DirectoryInfo folder in folders)
				{
					string[] filesDat = Directory.GetFiles(folder.FullName, "*.dat");
					int count = 0;
					foreach (string file in filesDat)
					{
						string filenameWihoutExt = Path.GetFileNameWithoutExtension(file).ToString();
						// Check if not copyed previous (during this session), and that converted json file do not already exists (from previous sessions)
						if (!battleResultDatFileCopyed.Exists(x => x == file) && !battleResultJsonFileExists.Exists(x => x == filenameWihoutExt))
						{
							// Copy
							Log.AddToLogBuffer(" > * Start copying battle DAT-file: " + file);
							FileInfo fileBattleOriginal = new FileInfo(file); // the original dossier file
							string filename = Path.GetFileName(file);
							fileBattleOriginal.CopyTo(Config.AppDataBattleResultFolder + filename, true); // copy original dossier fil and rename it for analyze
							Application.DoEvents();
							// if successful copy remember it
							if (File.Exists(Config.AppDataBattleResultFolder + filename))
							{
								battleResultDatFileCopyed.Add(file);
								Log.AddToLogBuffer(" > * Copyed successfully battle DAT-file: " + file);
							}
						}
						else
							count++;
					}
					if (count > 0)
						Log.AddToLogBuffer(" > DAT-files skipped, read previous: " + count.ToString());
					if (filesDat.Length == 0)
						Log.AddToLogBuffer(" > * No battle DAT-files found");
				}

				// Loop through all dat-files copyed to local folder
				string[] filesDatCopyed = Directory.GetFiles(Config.AppDataBattleResultFolder, "*.dat");
				foreach (string file in filesDatCopyed)
				{
					// Convert file to json
					if (ConvertBattleUsingPython(file))
					{
						// Success, json file is now created, clean up by delete dat file
						FileInfo fileBattleDatCopyed = new FileInfo(file); // the original dossier file
						fileBattleDatCopyed.Delete(); // delete original DAT file
						Log.AddToLogBuffer(" > * Deleted battle DAT-file: " + file);
						Application.DoEvents();
					}
				}
			}
		}

		public static string RunBattleResultRead(bool refreshGridOnFoundBattles = true, bool forceReadFiles = false)
		{
			try
			{
				string returVal = "";
				Log.AddToLogBuffer(" > Start looking for battle result");
				if (forceReadFiles)
				{
					battleResultDatFileCopyed = new List<string>();
					Log.AddToLogBuffer(" > Clear history, force check all DAT-files");
				}
				bool refreshAfterUpdate = false;
				// Look for new files
				ConvertBattleFilesToJson();
				// Get all json files
				string[] filesJson = Directory.GetFiles(Config.AppDataBattleResultFolder, "*.json");
				// Any files?
				if (filesJson.Length == 0)
					returVal = "No battle result availebale";
				// count action
				int processed = 0;
				int added = 0;
				foreach (string file in filesJson)
				{
					processed++;
					// Read content
					StreamReader sr = new StreamReader(file, Encoding.UTF8);
					string json = sr.ReadToEnd();
					sr.Close();
					// Root token
					JToken token_root = JObject.Parse(json);
					// Common token
					JToken token_common = token_root["common"];
					string result = (string)token_common.SelectToken("result"); // Find unique id
					// Check if ok
					bool deleteFileAfterRead = false;
					if (result == "ok")
					{
						double arenaUniqueID = (double)token_root.SelectToken("arenaUniqueID"); // Find unique id
						double arenaCreateTime = (double)token_common.SelectToken("arenaCreateTime"); // Arena create time
						double duration = (double)token_common.SelectToken("duration"); // Arena duration
						double battlefinishUnix = arenaCreateTime + duration; // Battle finish time
						DateTime battleTime = DateTimeHelper.AdjustForTimeZone(DateTimeHelper.ConvertFromUnixTimestamp(battlefinishUnix)).AddSeconds(45);
						// Personal token
						JToken token_personel = token_root["personal"];
						int tankId = (int)token_personel.SelectToken("typeCompDescr"); // tankId
						// Now find battle
						string sql =
							"select b.id as battleId, pt.id as playerTankId, pt.gGrindXP, b.arenaUniqueID  " +
							"from battle b left join playerTank pt on b.playerTankId = pt.id " +
							"where pt.tankId=@tankId and b.battleTime>@battleTimeFrom and b.battleTime<@battleTimeTo and b.battlesCount=1;";
						DB.AddWithValue(ref sql, "@tankId", tankId, DB.SqlDataType.Int);
						DB.AddWithValue(ref sql, "@battleTimeFrom", battleTime.AddSeconds(-30).ToString("yyyy-MM-dd HH:mm:ss"), DB.SqlDataType.DateTime);
						DB.AddWithValue(ref sql, "@battleTimeTo", battleTime.AddSeconds(30).ToString("yyyy-MM-dd HH:mm:ss"), DB.SqlDataType.DateTime);
						DataTable dt = DB.FetchData(sql);
						if (dt.Rows.Count > 0)
						{
							// Check if read
							if (dt.Rows[0]["arenaUniqueID"] != DBNull.Value)
							{
								// Battle read, delete file
								deleteFileAfterRead = true;
							}
							else
							{
								// New battle found, get battleId
								int battleId = Convert.ToInt32(dt.Rows[0]["battleId"]);
								int playerTankId = Convert.ToInt32(dt.Rows[0]["playerTankId"]);
								int grindXP = Convert.ToInt32(dt.Rows[0]["gGrindXP"]);
								// Get values
								List<BattleValues> battleValues = new List<BattleValues>();
								// common
								battleValues.Add(new BattleValues() { colname = "arenaTypeID", value = (int)token_common.SelectToken("arenaTypeID") });
								battleValues.Add(new BattleValues() { colname = "bonusType", value = (int)token_common.SelectToken("bonusType") });
								battleValues.Add(new BattleValues() { colname = "bonusTypeName", value = "'" + (string)token_common.SelectToken("bonusTypeName") + "'" });
								battleValues.Add(new BattleValues() { colname = "finishReasonName", value = "'" + (string)token_common.SelectToken("finishReasonName") + "'" });
								battleValues.Add(new BattleValues() { colname = "gameplayName", value = "'" + (string)token_common.SelectToken("gameplayName") + "'" });
								// personal - credits
								battleValues.Add(new BattleValues() { colname = "originalCredits", value = (int)token_personel.SelectToken("originalCredits") });
								battleValues.Add(new BattleValues() { colname = "credits", value = (int)token_personel.SelectToken("credits") });
								battleValues.Add(new BattleValues() { colname = "creditsPenalty", value = (int)token_personel.SelectToken("creditsPenalty") });
								battleValues.Add(new BattleValues() { colname = "creditsToDraw", value = (int)token_personel.SelectToken("creditsToDraw") });
								battleValues.Add(new BattleValues() { colname = "creditsContributionIn", value = (int)token_personel.SelectToken("creditsContributionIn") });
								battleValues.Add(new BattleValues() { colname = "creditsContributionOut", value = (int)token_personel.SelectToken("creditsContributionOut") });
								battleValues.Add(new BattleValues() { colname = "autoRepairCost", value = (int)token_personel.SelectToken("autoRepairCost") });
								battleValues.Add(new BattleValues() { colname = "eventCredits", value = (int)token_personel.SelectToken("eventCredits") });
								battleValues.Add(new BattleValues() { colname = "premiumCreditsFactor10", value = (int)token_personel.SelectToken("premiumCreditsFactor10") });
								battleValues.Add(new BattleValues() { colname = "achievementCredits", value = (int)token_personel.SelectToken("achievementCredits") });
								// personal XP
								battleValues.Add(new BattleValues() { colname = "real_xp", value = (int)token_personel.SelectToken("xp") });
								battleValues.Add(new BattleValues() { colname = "xpPenalty", value = (int)token_personel.SelectToken("xpPenalty") });
								battleValues.Add(new BattleValues() { colname = "freeXP", value = (int)token_personel.SelectToken("freeXP") });
								battleValues.Add(new BattleValues() { colname = "dailyXPFactor10", value = (int)token_personel.SelectToken("dailyXPFactor10") });
								battleValues.Add(new BattleValues() { colname = "premiumXPFactor10", value = (int)token_personel.SelectToken("premiumXPFactor10") });
								battleValues.Add(new BattleValues() { colname = "eventFreeXP", value = (int)token_personel.SelectToken("eventFreeXP") });
								battleValues.Add(new BattleValues() { colname = "achievementFreeXP", value = (int)token_personel.SelectToken("achievementFreeXP") });
								battleValues.Add(new BattleValues() { colname = "achievementXP", value = (int)token_personel.SelectToken("achievementXP") });
								battleValues.Add(new BattleValues() { colname = "eventXP", value = (int)token_personel.SelectToken("eventXP") });
								battleValues.Add(new BattleValues() { colname = "eventTMenXP", value = (int)token_personel.SelectToken("eventTMenXP") });
								// personal others
								battleValues.Add(new BattleValues() { colname = "markOfMastery", value = (int)token_personel.SelectToken("markOfMastery") });
								battleValues.Add(new BattleValues() { colname = "vehTypeLockTime", value = (int)token_personel.SelectToken("vehTypeLockTime") });
								battleValues.Add(new BattleValues() { colname = "marksOnGun", value = (int)token_personel.SelectToken("marksOnGun") });
								battleValues.Add(new BattleValues() { colname = "def", value = (int)token_personel.SelectToken("droppedCapturePoints") }); // override def - might be above 100
								// field returns null
								if (token_personel.SelectToken("fortResource").HasValues)
									battleValues.Add(new BattleValues() { colname = "fortResource", value = (int)token_personel.SelectToken("fortResource") });
								// dayly double
								int dailyXPFactor = (int)token_personel.SelectToken("dailyXPFactor10") / 10;
								battleValues.Add(new BattleValues() { colname = "dailyXPFactorTxt", value = "'" + dailyXPFactor.ToString() + " X'" });
								// Special fields: death reason, convert to string
								int deathReasonId = (int)token_personel.SelectToken("deathReason");
								string deathReason = "Unknown";
								switch (deathReasonId)
								{
									case -1: deathReason = "Alive"; break;
									case 0: deathReason = "Shot"; break;
									case 1: deathReason = "Burned"; break;
									case 2: deathReason = "Rammed"; break;
									case 3: deathReason = "Chrashed"; break;
									case 4: deathReason = "Death zone"; break;
									case 5: deathReason = "Drowned"; break;
								}
								battleValues.Add(new BattleValues() { colname = "deathReason", value = "'" + deathReason + "'" });
								// Get from array autoLoadCost
								JArray array_autoload = (JArray)token_personel.SelectToken("autoLoadCost");
								int autoLoadCost = (int)array_autoload[0];
								battleValues.Add(new BattleValues() { colname = "autoLoadCost", value = autoLoadCost });
								// Get from array autoEquipCost
								JArray array_autoequip = (JArray)token_personel.SelectToken("autoEquipCost");
								int autoEquipCost = (int)array_autoequip[0];
								battleValues.Add(new BattleValues() { colname = "autoEquipCost", value = autoEquipCost });
								// Calculated net credits
								int creditsNet = (int)token_personel.SelectToken("credits");
								creditsNet -= (int)token_personel.SelectToken("creditsPenalty"); // fine for damage to allies
								creditsNet += (int)token_personel.SelectToken("creditsToDraw"); // compensation for dmg caused by allies
								creditsNet -= (int)token_personel.SelectToken("autoRepairCost"); // repear cost
								creditsNet -= autoLoadCost;
								creditsNet -= autoEquipCost;
								battleValues.Add(new BattleValues() { colname = "creditsNet", value = creditsNet });
								// map id
								int arenaTypeID = (int)token_common.SelectToken("arenaTypeID");
								int mapId = arenaTypeID & 32767;
								battleValues.Add(new BattleValues() { colname = "mapId", value = mapId });
								// insert data
								string fields = "";
								foreach (var battleValue in battleValues)
								{
									fields += battleValue.colname + " = " + battleValue.value.ToString() + ", ";
								}
								sql = "update battle set " + fields + " arenaUniqueID=@arenaUniqueID where id=@battleId";
								DB.AddWithValue(ref sql, "@battleId", battleId, DB.SqlDataType.Int);
								DB.AddWithValue(ref sql, "@arenaUniqueID", arenaUniqueID, DB.SqlDataType.Float);
								DB.ExecuteNonQuery(sql);
								// If grinding, adjust grogress
								if (grindXP > 0)
									GrindingProgress(playerTankId, (int)token_personel.SelectToken("xp"));
								// Done
								deleteFileAfterRead = true;
								refreshAfterUpdate = true;
								GridView.scheduleGridRefresh = true;
								Log.AddToLogBuffer(" > * Done reading into DB JSON file: " + file);
								added++;
							}
						}
						else
						{
							Log.AddToLogBuffer(" > * New battle file not read, battle do not exists for JSON file: " + file);
							// Battle do not exists, delete if old file file
							if (battleTime < DateTime.Now.AddDays(-3))
							{
								deleteFileAfterRead = true;
								Log.AddToLogBuffer(" > * Old battle found, schedule for delete for JSON file: " + file);
							}
						}
						// Delete file if handled or old
						if (deleteFileAfterRead)
						{
							// Done - delete file
							FileInfo fileBattleJson = new FileInfo(file);
							fileBattleJson.Delete();
							Log.AddToLogBuffer(" > * Deleted read or old JSON file: " + file);
						}
					}
				}
				// Create alert file if new battle result added 
				if (refreshAfterUpdate && refreshGridOnFoundBattles)
				{
					GridView.scheduleGridRefresh = false;
					Log.BattleResultDoneLog();
					Log.WriteLogBuffer();
				}
				// Return result
				if (added == 0)
					returVal = processed.ToString() + " files checked, no new battle result detected";
				else
					returVal = processed.ToString() + " files checked, " + added + " files added as battle result";
				return returVal;
			}
			catch (Exception ex)
			{
				Log.LogToFile(ex);
				return "Battle result terminated due to error, see logfile";
			}
		}
		
		private static void GrindingProgress(int playerTankId, int XP)
		{
			// Yes, apply grinding progress to playerTank now
			// Get grinding data
			string sql = "SELECT tank.name, gCurrentXP, gGrindXP, gGoalXP, gProgressXP, gBattlesDay, gComment, lastVictoryTime, " +
					"        SUM(playerTankBattle.battles) as battles, SUM(playerTankBattle.wins) as wins, " +
					"        MAX(playerTankBattle.maxXp) AS maxXP, SUM(playerTankBattle.xp) AS totalXP, " +
					"        SUM(playerTankBattle.xp) / SUM(playerTankBattle.battles) AS avgXP " +
					"FROM    tank INNER JOIN " +
					"        playerTank ON tank.id = playerTank.tankId INNER JOIN " +
					"        playerTankBattle ON playerTank.id = playerTankBattle.playerTankId " +
					"WHERE  (playerTank.id = @playerTankId) " +
					"GROUP BY tank.name, gCurrentXP, gGrindXP, gGoalXP, gProgressXP, gBattlesDay, gComment, lastVictoryTime ";
			DB.AddWithValue(ref sql, "@playerTankId", playerTankId, DB.SqlDataType.Int);
			DataRow grinding = DB.FetchData(sql).Rows[0];
			// Get parameters for grinding calc
			int progress = Convert.ToInt32(grinding["gProgressXP"]) + XP; // Added XP to previous progress
			int grind = Convert.ToInt32(grinding["gGrindXP"]);
			int btlPerDay = Convert.ToInt32(grinding["gBattlesDay"]);
			// Calc values according to increased XP (progress)
			int progressPercent = GrindingHelper.CalcProgressPercent(grind, progress);
			int restXP = GrindingHelper.CalcProgressRestXP(grind, progress);
			int realAvgXP = GrindingHelper.CalcRealAvgXP(grinding["battles"].ToString(), grinding["wins"].ToString(), grinding["totalXP"].ToString(),
															grinding["avgXP"].ToString(), btlPerDay.ToString());
			int restBattles = GrindingHelper.CalcRestBattles(restXP, realAvgXP);
			int restDays = GrindingHelper.CalcRestDays(restXP, realAvgXP, btlPerDay);
			// Save to playerTank
			sql = "UPDATE playerTank SET gProgressXP=@ProgressXP, gRestXP=@RestXP, gProgressPercent=@ProgressPercent, " +
					"					     gRestBattles=@RestBattles, gRestDays=@RestDays  " +
					"WHERE id=@id; ";
			DB.AddWithValue(ref sql, "@ProgressXP", progress, DB.SqlDataType.Int);
			DB.AddWithValue(ref sql, "@RestXP", restXP, DB.SqlDataType.Int);
			DB.AddWithValue(ref sql, "@ProgressPercent", progressPercent, DB.SqlDataType.Int);
			DB.AddWithValue(ref sql, "@RestBattles", restBattles, DB.SqlDataType.Int);
			DB.AddWithValue(ref sql, "@RestDays", restDays, DB.SqlDataType.Int);
			DB.AddWithValue(ref sql, "@id", playerTankId, DB.SqlDataType.Int);
			DB.ExecuteNonQuery(sql);
		}

		private static bool ConvertBattleUsingPython(string filename)
		{
			bool ok = false;
			// Locate Python script
			string appPath = Path.GetDirectoryName(Application.ExecutablePath); // path to app dir
			string battle2jsonScript = appPath + "/dossier2json/wotbr2j.py"; // python-script for converting dossier file
			// Use IronPython
			try
			{
				//var ipy = Python.CreateRuntime();
				//dynamic ipyrun = ipy.UseFile(dossier2jsonScript);
				//ipyrun.main();
				if (!PythonEngine.InUse)
				{
					PythonEngine.InUse = true;
					var argv = new List();
					argv.Add(battle2jsonScript); // Have to add filename to run as first arg
					argv.Add(filename);
					argv.Add("-f");
					PythonEngine.Engine.GetSysModule().SetVariable("argv", argv);
					Microsoft.Scripting.Hosting.ScriptScope scope = PythonEngine.Engine.ExecuteFile(battle2jsonScript); // this is your python program
					dynamic result = scope.GetVariable("main")();

					//ScriptRuntimeSetup setup = new ScriptRuntimeSetup();
					//setup.DebugMode = true;
					//setup.LanguageSetups.Add(Python.CreateLanguageSetup(null));
					//ScriptRuntime runtime = new ScriptRuntime(setup);
					//ScriptEngine engine = runtime.GetEngineByTypeName(typeof(PythonContext).AssemblyQualifiedName);
					//ScriptSource script = engine.CreateScriptSourceFromFile(battle2jsonScript);
					//CompiledCode code = script.Compile();
					//ScriptScope scope = engine.CreateScope();
					//script.Execute(scope);
					Application.DoEvents();
					Log.AddToLogBuffer("Converted battle DAT-file til JSON file: " + filename);
					ok = true;
					PythonEngine.InUse = false;
				}
				else
					Log.AddToLogBuffer("IronPython Engine in use, not converted battle DAT-file til JSON file: " + filename);
			}
			catch (Exception ex)
			{
				Log.LogToFile(ex);
				Code.MsgBox.Show("Error running Python script converting battle file: " + ex.Message + Environment.NewLine + Environment.NewLine +
				"Inner Exception: " + ex.InnerException, "Error converting battle file to json");
				PythonEngine.InUse = false;
			}
			return ok;
		}
	}
}
