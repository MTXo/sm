using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace MauiApp1.Scripts
{
    // =========================
    // MODELS
    // =========================

    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SteamAppId { get; set; }
        public string GamePath { get; set; } = string.Empty;
        public string SavePath { get; set; } = string.Empty;
        public bool AutoSave { get; set; }
        public int AutoSavePeriod { get; set; }
        public int LastSelectedBranch { get; set; }
    }

    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int GameId { get; set; }
    }

    public class Save
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int BranchId { get; set; }
        public string FileName { get; set; } = string.Empty;
    }

    // =========================
    // DATABASE
    // =========================

    public static class Database
    {

        private static string _connectionString =
            $"Data Source={Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/MW Save Manager/ProgramDB.db";

        // =========================
        // CREATE DATABASE
        // =========================

        public static void CreateDatabase(bool clear = false)
        {
            var dbDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MW Save Manager");
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
                Directory.CreateDirectory(Path.Combine(dbDirectory, "Backups"));
            }

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            if (clear)
            {
                var command2 = connection.CreateCommand();
                command2.CommandText =
                @"
                DROP TABLE IF EXISTS ZAPIS;
                DROP TABLE IF EXISTS BRANCH;
                DROP TABLE IF EXISTS GRA;
                ";
                command2.ExecuteNonQuery();
            }
            
            command.CommandText =
            @"
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS GRA (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                nazwa TEXT NOT NULL,
                steamappid INTEGER,
                path TEXT,
                savepath TEXT,
                autosave INTEGER NOT NULL DEFAULT 0,
                autosaveperiod INTEGER,
                lastselectedbranch INTEGER
            );

            CREATE TABLE IF NOT EXISTS BRANCH (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                nazwa TEXT NOT NULL,
                gra_id INTEGER NOT NULL,

                FOREIGN KEY (gra_id)
                    REFERENCES GRA(id)
                    ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS ZAPIS (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                date TEXT NOT NULL,
                branch_id INTEGER NOT NULL,
                filename TEXT NOT NULL,

                FOREIGN KEY (branch_id)
                    REFERENCES BRANCH(id)
                    ON DELETE CASCADE
            );
            ";

            command.ExecuteNonQuery();
            
            connection.Close();
        }

        // =========================
        // ADD GAME
        // =========================

        public static long AddGame(
            string name,
            int steamAppId,
            string gamePath,
            string savePath,
            bool autoSave,
            int autoSavePeriod)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            @"
            INSERT INTO GRA
            (nazwa, steamappid, path, savepath,
             autosave, autosaveperiod, lastselectedbranch)

            VALUES
            ($name, $steamappid, $path,
             $savepath, $autosave,
             $autosaveperiod, $lastselectedbranch);

            SELECT last_insert_rowid();
            ";

            command.Parameters.AddWithValue(
                "$name", name);

            command.Parameters.AddWithValue(
                "$steamappid", steamAppId);

            command.Parameters.AddWithValue(
                "$path", gamePath);

            command.Parameters.AddWithValue(
                "$savepath", savePath);

            command.Parameters.AddWithValue(
                "$autosave", autoSave ? 1 : 0);

            command.Parameters.AddWithValue(
                "$autosaveperiod", autoSavePeriod);

            command.Parameters.AddWithValue(
                "$lastselectedbranch", 0);

            var tmp = command.ExecuteScalar() ?? -1;

            connection.Close();

            return (long)(tmp);
        }

        // =========================
        // ADD BRANCH
        // =========================

        public static long AddBranch(
            string name,
            int gameId)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            @"
            INSERT INTO BRANCH
            (nazwa, gra_id)

            VALUES
            ($name, $gameid);

            SELECT last_insert_rowid();
            ";

            command.Parameters.AddWithValue(
                "$name", name);

            command.Parameters.AddWithValue(
                "$gameid", gameId);

            var tmp = command.ExecuteScalar() ?? -1;

            connection.Close();

            return (long)(tmp);
        }

        // =========================
        // ADD SAVE
        // =========================

        public static long AddSave(
            DateTime date,
            int branchId,
            string fileName)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            @"
            INSERT INTO ZAPIS
            (date, branch_id, filename)

            VALUES
            ($date, $branchid, $filename);

            SELECT last_insert_rowid();
            ";

            command.Parameters.AddWithValue(
                "$date",
                date.ToString("o"));

            command.Parameters.AddWithValue(
                "$branchid",
                branchId);

            command.Parameters.AddWithValue(
                "$filename",
                fileName);

            var tmp = command.ExecuteScalar() ?? -1;

            connection.Close();

            return (long)(tmp);
        }

        // =========================
        // DELETE
        // =========================

        public static void DeleteGame(int id)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            "DELETE FROM GRA WHERE id = $id;";

            command.Parameters.AddWithValue(
                "$id", id);

            command.ExecuteNonQuery();

            connection.Close();
        }

        public static void DeleteBranch(int id)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            "DELETE FROM BRANCH WHERE id = $id;";

            command.Parameters.AddWithValue(
                "$id", id);

            command.ExecuteNonQuery();

            connection.Close();
        }

        public static void DeleteSave(int id)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            "DELETE FROM ZAPIS WHERE id = $id;";

            command.Parameters.AddWithValue(
                "$id", id);

            command.ExecuteNonQuery();

            connection.Close();
        }

        // =========================
        // UPDATE
        // =========================

        public static void UpdateGame(
            int id,
            string name,
            int steamAppId,
            string gamePath,
            string savePath,
            bool autoSave,
            int autoSavePeriod)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            @"
            UPDATE GRA
            SET
                nazwa = $name,
                steamappid = $steamappid,
                path = $path,
                savepath = $savepath,
                autosave = $autosave,
                autosaveperiod = $autosaveperiod
            WHERE id = $id;
            ";

            command.Parameters.AddWithValue(
                "$id", id);

            command.Parameters.AddWithValue(
                "$name", name);

            command.Parameters.AddWithValue(
                "$steamappid", steamAppId);

            command.Parameters.AddWithValue(
                "$path", gamePath);

            command.Parameters.AddWithValue(
                "$savepath", savePath);

            command.Parameters.AddWithValue(
                "$autosave", autoSave ? 1 : 0);

            command.Parameters.AddWithValue(
                "$autosaveperiod", autoSavePeriod);

            command.ExecuteNonQuery();

            connection.Close();
        }

        public static void UpdateGameSelectedBranch(
            int id,
            int branch_id)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            @"
            UPDATE GRA
            SET
                lastselectedbranch = $lastselectedbranch
            WHERE id = $id;
            ";

            command.Parameters.AddWithValue(
                "$id", id);

            command.Parameters.AddWithValue(
                "$lastselectedbranch", branch_id);

            command.ExecuteNonQuery();

            connection.Close();
        }

        public static void UpdateBranch(
            int id,
            string name)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            @"
            UPDATE BRANCH
            SET
                nazwa = $name,
            WHERE id = $id;
            ";

            command.Parameters.AddWithValue(
                "$id", id);

            command.Parameters.AddWithValue(
                "$name", name);

            command.ExecuteNonQuery();

            connection.Close();
        }

        public static void UpdateSave(
            int id,
            DateTime date,
            int branchId,
            string fileName)
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            @"
            UPDATE ZAPIS
            SET
                date = $date,
                branch_id = $branchid,
                filename = $filename
            WHERE id = $id;
            ";

            command.Parameters.AddWithValue(
                "$id", id);

            command.Parameters.AddWithValue(
                "$date",
                date.ToString("o"));

            command.Parameters.AddWithValue(
                "$branchid",
                branchId);

            command.Parameters.AddWithValue(
                "$filename",
                fileName);

            command.ExecuteNonQuery();

            connection.Close();
        }

        // =========================
        // GET ALL
        // =========================

        public static List<Game> GetAllGames()
        {
            var result = new List<Game>();

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                "SELECT * FROM GRA;";

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new Game
                {
                    Id =
                        Convert.ToInt32(reader["id"]),

                    Name =
                        reader["nazwa"].ToString() ?? "",

                    SteamAppId =
                        Convert.ToInt32(
                            reader["steamappid"]),

                    GamePath =
                        reader["path"].ToString() ?? "",

                    SavePath =
                        reader["savepath"].ToString() ?? "",

                    AutoSave =
                        Convert.ToInt32(
                            reader["autosave"]) == 1,

                    AutoSavePeriod =
                        Convert.ToInt32(
                            reader["autosaveperiod"]),

                    LastSelectedBranch =
                        Convert.ToInt32(
                            reader["lastselectedbranch"])
                });
            }

            connection.Close();

            return result;
        }

        public static List<Branch> GetAllBranches()
        {
            var result = new List<Branch>();

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                "SELECT * FROM BRANCH;";

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new Branch
                {
                    Id =
                        Convert.ToInt32(reader["id"]),

                    Name =
                        reader["nazwa"].ToString() ?? "",

                    GameId =
                        Convert.ToInt32(
                            reader["gra_id"])
                });
            }

            connection.Close();

            return result;
        }

        public static List<Save> GetAllSaves()
        {
            var result = new List<Save>();

            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                "SELECT * FROM ZAPIS;";

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new Save
                {
                    Id =
                        Convert.ToInt32(reader["id"]),

                    Date =
                        DateTime.Parse(
                            reader["date"].ToString() ?? ""),

                    BranchId =
                        Convert.ToInt32(
                            reader["branch_id"]),

                    FileName =
                        reader["filename"].ToString() ?? ""
                });
            }

            connection.Close();

            return result;
        }

        // =========================
        // COUNT
        // =========================

        public static int GetGameCount()
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                "SELECT COUNT(*) FROM GRA;";

            var tmp = command.ExecuteScalar();
            
            connection.Close();

            return Convert.ToInt32(
                tmp);
        }

        public static int GetBranchCount()
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                "SELECT COUNT(*) FROM BRANCH;";

            var tmp = command.ExecuteScalar();

            connection.Close();

            return Convert.ToInt32(
                tmp);
        }

        public static int GetSaveCount()
        {
            using var connection =
                new SqliteConnection(_connectionString);

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                "SELECT COUNT(*) FROM ZAPIS;";

            var tmp = command.ExecuteScalar();

            connection.Close();

            return Convert.ToInt32(
                tmp);
        }
    }
}