using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;

namespace MauiApp1.Scripts
{
    class DatabaseManager
    {
        SqliteConnection connection = new SqliteConnection($"Data Source={Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/appdata.db");

        public void CreateTable()
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
    }
}
