using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;

namespace MauiApp1.Scripts
{
    class DatabaseManager
    {

        static SqliteConnection connection = new SqliteConnection($"Data Source={Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/appdata.db");

        public static void CreateTable()
        {
            connection.Open();

            var command = connection.CreateCommand();

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
                    autosaveperiod INTEGER
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
        // GRA
        // =========================

        public static long AddGra(
            string nazwa,
            int steamappid,
            string path,
            string savepath,
            bool autosave,
            int autosaveperiod)
        {

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            @"
            INSERT INTO GRA
            (nazwa, steamappid, path, savepath, autosave, autosaveperiod)
            VALUES
            ($nazwa, $steamappid, $path, $savepath, $autosave, $autosaveperiod);

            SELECT last_insert_rowid();
            ";

            command.Parameters.AddWithValue("$nazwa", nazwa);
            command.Parameters.AddWithValue("$steamappid", steamappid);
            command.Parameters.AddWithValue("$path", path);
            command.Parameters.AddWithValue("$savepath", savepath);
            command.Parameters.AddWithValue("$autosave", autosave ? 1 : 0);
            command.Parameters.AddWithValue("$autosaveperiod", autosaveperiod);

            return (long)command.ExecuteScalar();
        }

        public static void DeleteGra(int id)
        {

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            @"
            DELETE FROM GRA
            WHERE id = $id;
            ";

            command.Parameters.AddWithValue("$id", id);

            command.ExecuteNonQuery();
        }

        public static List<Dictionary<string, object>> FindGra(
            string column,
            object value)
        {
            var result = new List<Dictionary<string, object>>();

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            $@"
            SELECT *
            FROM GRA
            WHERE {column} = $value;
            ";

            command.Parameters.AddWithValue("$value", value);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }

                result.Add(row);
            }

            return result;
        }

        // =========================
        // BRANCH
        // =========================

        public static long AddBranch(
            string nazwa,
            int gra_id)
        {

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            @"
            INSERT INTO BRANCH
            (nazwa, gra_id)
            VALUES
            ($nazwa, $gra_id);

            SELECT last_insert_rowid();
            ";

            command.Parameters.AddWithValue("$nazwa", nazwa);
            command.Parameters.AddWithValue("$gra_id", gra_id);

            return (long)command.ExecuteScalar();
        }

        public static void DeleteBranch(int id)
        {

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            @"
            DELETE FROM BRANCH
            WHERE id = $id;
            ";

            command.Parameters.AddWithValue("$id", id);

            command.ExecuteNonQuery();
        }

        public static List<Dictionary<string, object>> FindBranch(
            string column,
            object value)
        {
            var result = new List<Dictionary<string, object>>();

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            $@"
            SELECT *
            FROM BRANCH
            WHERE {column} = $value;
            ";

            command.Parameters.AddWithValue("$value", value);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }

                result.Add(row);
            }

            return result;
        }

        // =========================
        // ZAPIS
        // =========================

        public static long AddZapis(
            DateTime date,
            int branch_id,
            string filename)
        {
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            @"
            INSERT INTO ZAPIS
            (date, branch_id, filename)
            VALUES
            ($date, $branch_id, $filename);

            SELECT last_insert_rowid();
            ";

            command.Parameters.AddWithValue(
                "$date",
                date.ToString("o"));

            command.Parameters.AddWithValue(
                "$branch_id",
                branch_id);

            command.Parameters.AddWithValue(
                "$filename",
                filename);

            return (long)command.ExecuteScalar();
        }

        public static void DeleteZapis(int id)
        {
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            @"
            DELETE FROM ZAPIS
            WHERE id = $id;
            ";

            command.Parameters.AddWithValue("$id", id);

            command.ExecuteNonQuery();
        }

        public static List<Dictionary<string, object>> FindZapis(
            string column,
            object value)
        {
            var result = new List<Dictionary<string, object>>();

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
            $@"
            SELECT *
            FROM ZAPIS
            WHERE {column} = $value;
            ";

            command.Parameters.AddWithValue("$value", value);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }

                result.Add(row);
            }

            return result;
        }
    }
}
