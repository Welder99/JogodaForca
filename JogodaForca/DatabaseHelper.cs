using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace JogodaForca
{
    public class DatabaseHelper
    {
        private string dbPath;

        public DatabaseHelper()
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            dbPath = Path.Combine(folderPath, "hangman.db");

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    string createCategoriesTable = @"
                    CREATE TABLE IF NOT EXISTS Categories (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL UNIQUE
                    );";

                    string createWordsTable = @"
                    CREATE TABLE IF NOT EXISTS Words (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CategoryId INTEGER,
                        Word TEXT NOT NULL,
                        FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                    );";

                    var command = connection.CreateCommand();
                    command.CommandText = createCategoriesTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createWordsTable;
                    command.ExecuteNonQuery();

                    connection.Close();
                }

                // Inserir categorias e palavras iniciais
                SeedDatabase();
            }
        }

        private void SeedDatabase()
        {
            // Inserir categorias e palavras iniciais aqui
            var categories = new List<string>
            {
                "Animal",
                "Cor",
                "Carro",
                "Linguagem de Programação",
                "Hardware",
                "Banco de Dados"
            };

            var words = new Dictionary<string, List<string>>
            {
                { "Animal", new List<string> { "ELEFANTE", "CANGURU", "HIPOPÓTAMO", "TARTARUGA", "RINOCERONTE", "GIRAFA", "GORILA", "ORNITORRINCO", "CROCODILO", "ESQUILO" } },
                { "Cor", new List<string> { "VERMELHO", "AZUL", "AMARELO", "VERDE", "ROSA", "LARANJA", "ROXO", "MARROM", "PRETO", "BRANCO" } },
                { "Carro", new List<string> { "FERRARI", "PORSCHE", "TOYOTA", "HONDA", "MERCEDES", "BMW", "CHEVROLET", "VOLKSWAGEN", "AUDI", "LAMBORGHINI" } },
                { "Linguagem de Programação", new List<string> { "PYTHON", "JAVA", "JAVASCRIPT", "C", "RUBY", "KOTLIN", "SWIFT", "GO", "RUST", "PHP" } },
                { "Hardware", new List<string> { "PROCESSADOR", "MEMORIA", "MONITOR", "TECLADO", "MOUSE", "PLACA", "FONTE", "SSD", "HD", "COOLER" } },
                { "Banco de Dados", new List<string> { "MYSQL", "POSTGRESQL", "MONGODB", "ORACLE", "SQLITE", "FIREBASE", "SQLSERVER", "CASSANDRA", "REDIS", "COUCHDB" } }
            };

            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                foreach (var category in categories)
                {
                    var insertCategoryCmd = connection.CreateCommand();
                    insertCategoryCmd.CommandText = "INSERT INTO Categories (Name) VALUES ($name)";
                    insertCategoryCmd.Parameters.AddWithValue("$name", category);
                    insertCategoryCmd.ExecuteNonQuery();
                }

                foreach (var entry in words)
                {
                    // Obter o CategoryId
                    var getCategoryCmd = connection.CreateCommand();
                    getCategoryCmd.CommandText = "SELECT Id FROM Categories WHERE Name = $name";
                    getCategoryCmd.Parameters.AddWithValue("$name", entry.Key);
                    var categoryId = (long)getCategoryCmd.ExecuteScalar();

                    foreach (var word in entry.Value)
                    {
                        var insertWordCmd = connection.CreateCommand();
                        insertWordCmd.CommandText = "INSERT INTO Words (CategoryId, Word) VALUES ($categoryId, $word)";
                        insertWordCmd.Parameters.AddWithValue("$categoryId", categoryId);
                        insertWordCmd.Parameters.AddWithValue("$word", word);
                        insertWordCmd.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }


        public List<string> GetCategories()
        {
            var categories = new List<string>();
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Name FROM Categories";
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(reader.GetString(0));
                }
                connection.Close();
            }
            return categories;
        }

        public List<string> GetWordsByCategory(string categoryName)
        {
            var words = new List<string>();
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                SELECT Words.Word FROM Words
                JOIN Categories ON Words.CategoryId = Categories.Id
                WHERE Categories.Name = $categoryName";
                command.Parameters.AddWithValue("$categoryName", categoryName);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    words.Add(reader.GetString(0));
                }
                connection.Close();
            }
            return words;
        }

        public void AddWord(string categoryName, string word)
        {
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                // Verificar se a categoria existe
                var getCategoryCmd = connection.CreateCommand();
                getCategoryCmd.CommandText = "SELECT Id FROM Categories WHERE Name = $name";
                getCategoryCmd.Parameters.AddWithValue("$name", categoryName);
                var categoryIdObj = getCategoryCmd.ExecuteScalar();

                long categoryId;
                if (categoryIdObj == null)
                {
                    // Inserir nova categoria
                    var insertCategoryCmd = connection.CreateCommand();
                    insertCategoryCmd.CommandText = "INSERT INTO Categories (Name) VALUES ($name)";
                    insertCategoryCmd.Parameters.AddWithValue("$name", categoryName);
                    insertCategoryCmd.ExecuteNonQuery();

                    // Obter o Id da nova categoria
                    var getLastIdCmd = connection.CreateCommand();
                    getLastIdCmd.CommandText = "SELECT last_insert_rowid();";
                    categoryId = (long)getLastIdCmd.ExecuteScalar();
                }
                else
                {
                    categoryId = (long)categoryIdObj;
                }

                // Inserir a palavra
                var insertWordCmd = connection.CreateCommand();
                insertWordCmd.CommandText = "INSERT INTO Words (CategoryId, Word) VALUES ($categoryId, $word)";
                insertWordCmd.Parameters.AddWithValue("$categoryId", categoryId);
                insertWordCmd.Parameters.AddWithValue("$word", word);
                insertWordCmd.ExecuteNonQuery();

                connection.Close();
            }
        }
    }
}
