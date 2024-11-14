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

            Console.WriteLine($"Caminho do banco de dados: {dbPath}"); // Adicionado para mostrar o caminho

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                // Verifica se a tabela Categories existe
                var checkCategoriesTableCmd = connection.CreateCommand();
                checkCategoriesTableCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Categories';";
                var categoriesTableName = checkCategoriesTableCmd.ExecuteScalar();

                if (categoriesTableName == null)
                {
                    // Cria a tabela Categories
                    string createCategoriesTable = @"
            CREATE TABLE Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE
            );";
                    var createCategoriesCmd = connection.CreateCommand();
                    createCategoriesCmd.CommandText = createCategoriesTable;
                    createCategoriesCmd.ExecuteNonQuery();
                }

                // Verifica se a tabela Words existe
                var checkWordsTableCmd = connection.CreateCommand();
                checkWordsTableCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Words';";
                var wordsTableName = checkWordsTableCmd.ExecuteScalar();

                if (wordsTableName == null)
                {
                    // Cria a tabela Words com a coluna Difficulty
                    string createWordsTable = @"
            CREATE TABLE Words (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CategoryId INTEGER,
                Word TEXT NOT NULL,
                Difficulty TEXT NOT NULL,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
            );";
                    var createWordsCmd = connection.CreateCommand();
                    createWordsCmd.CommandText = createWordsTable;
                    createWordsCmd.ExecuteNonQuery();
                }
                else
                {
                    // Verifica se a coluna Difficulty existe
                    var checkColumnCmd = connection.CreateCommand();
                    checkColumnCmd.CommandText = "PRAGMA table_info(Words);";
                    var reader = checkColumnCmd.ExecuteReader();
                    bool difficultyColumnExists = false;
                    while (reader.Read())
                    {
                        var columnName = reader.GetString(1); // O segundo campo é o nome da coluna
                        if (columnName == "Difficulty")
                        {
                            difficultyColumnExists = true;
                            break;
                        }
                    }
                    reader.Close();

                    if (!difficultyColumnExists)
                    {
                        // Adiciona a coluna Difficulty
                        var alterTableCmd = connection.CreateCommand();
                        alterTableCmd.CommandText = "ALTER TABLE Words ADD COLUMN Difficulty TEXT NOT NULL DEFAULT 'Médio';";
                        alterTableCmd.ExecuteNonQuery();
                    }
                }

                // Verifica se a tabela Categories está vazia
                var checkCategoriesEmptyCmd = connection.CreateCommand();
                checkCategoriesEmptyCmd.CommandText = "SELECT COUNT(*) FROM Categories;";
                var categoriesCount = (long)checkCategoriesEmptyCmd.ExecuteScalar();

                if (categoriesCount == 0)
                {
                    // Popula o banco de dados
                    SeedDatabase();
                }

                connection.Close();
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
    { "Animal", new List<string> {
        "GATO", "CÃO", "ELEFANTE", "CANGURU", "HIPOPOTAMO", "TARTARUGA",
        "RINOCERONTE", "GIRAFA", "GORILA", "ORNITORRINCO", "CROCODILO",
        "ESQUILO", "ABELHA", "PAPAGAIO", "BALEIA", "CAMELO", "LEOPARDO",
        "PANTERA", "TIGRE", "URSO", "LOBISOMEM", "PEIXE", "ZEBRA", "PINGUIM",
        "FOCA", "AVESTRUZ", "BORBOLETA", "CORUJA", "DRAGAO", "GAIVOTA",
        "HIPOGRIFFO", "IGUANA", "JACARE", "KOALA", "LEBRE", "MACACO", "NARVAL",
        "ORNITORRINCO", "PATO", "QUATI", "RAPOSA", "SALAMANDRA", "TOURO", "URUBU"
    }},
    { "Cor", new List<string> {
        "VERMELHO", "AZUL", "AMARELO", "VERDE", "ROSA", "LARANJA",
        "ROXO", "MARROM", "PRETO", "BRANCO", "CINZA", "BEGE",
        "VIOLETA", "SALMÃO", "TURQUESA", "LILÁS", "MAGENTA",
        "AMETISTA", "BRONZE", "DOURADO", "PRATA", "ESMERALDA",
        "JADE", "MALVA", "BORDÔ", "ESCARLATE", "ACINZENTADO", "CREME",
        "LAVANDA", "NEON", "PASTEL", "MOSTARDA", "SÉPIA", "PETRÓLEO"
    }},
    { "Carro", new List<string> {
        "FIAT", "FERRARI", "PORSCHE", "TOYOTA", "HONDA", "MERCEDES",
        "BMW", "CHEVROLET", "VOLKSWAGEN", "AUDI", "LAMBORGHINI",
        "MASERATI", "JAGUAR", "BUGATTI", "ASTONMARTIN", "NISSAN",
        "SUZUKI", "PEUGEOT", "CITROEN", "HYUNDAI", "TESLA", "LEXUS",
        "CHRYSLER", "DODGE", "SUBARU", "KIA", "ROLLSROYCE", "BENTLEY",
        "ALFAROMEO", "CADILLAC", "VOLVO", "RENAULT", "MCLAREN",
        "LUXGEN", "TROLLER", "CHANGAN", "MAHINDRA", "ISUZU"
    }},
    { "Linguagem de Programação", new List<string> {
        "C", "JAVA", "PYTHON", "JAVASCRIPT", "RUBY", "KOTLIN",
        "SWIFT", "GO", "RUST", "PHP", "HTML", "CSS", "TYPESCRIPT",
        "DART", "PERL", "LUA", "HASKELL", "SCALA", "R", "OBJECTIVEC",
        "COBOL", "FORTRAN", "ELIXIR", "CLOJURE", "ERLANG", "PASCAL",
        "F#", "VB", "ASSEMBLY", "MATLAB", "APL", "JULIA", "ADA"
    }},
    { "Hardware", new List<string> {
        "CPU", "RAM", "PROCESSADOR", "MEMORIA", "MONITOR", "TECLADO",
        "MOUSE", "PLACA", "FONTE", "SSD", "HD", "COOLER", "GABINETE",
        "CABO", "SPEAKER", "HEADSET", "MICROFONE", "PLACAVIDEO",
        "FONTEALIMENTACAO", "MODULO", "MOTHERBOARD", "AMPLIFICADOR",
        "FAN", "DISQUETE", "NOBREAK", "MODULADOR", "TRANSFORMADOR",
        "DISSIPADOR", "MEMORIAFLASH", "MEMORIARAM", "MEMORIAROM", "BLUETOOTH"
    }},
    { "Banco de Dados", new List<string> {
        "MYSQL", "SQLITE", "POSTGRESQL", "MONGODB", "ORACLE",
        "FIREBASE", "SQLSERVER", "CASSANDRA", "REDIS", "COUCHDB",
        "DYNAMODB", "INFLUXDB", "MARIA", "MARIADB", "COCKROACHDB",
        "BIGTABLE", "NEO4J", "ELASTICSEARCH", "HBASE", "CLICKHOUSE",
        "FOUNDATIONDB", "SCYLLADB", "TIDB", "POLARDB", "IMDB", "SYBASE",
        "TERADATA", "GREENPLUM", "TIGERGRAPH", "SAP", "CUBRID", "EXASOL"
    }}
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
                        insertWordCmd.CommandText = "INSERT INTO Words (CategoryId, Word, Difficulty) VALUES ($categoryId, $word, $difficulty)";
                        insertWordCmd.Parameters.AddWithValue("$categoryId", categoryId);
                        insertWordCmd.Parameters.AddWithValue("$word", word);
                        insertWordCmd.Parameters.AddWithValue("$difficulty", DetermineDifficulty(word));
                        insertWordCmd.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }

        private string DetermineDifficulty(string word)
        {
            if (word.Length <= 4)
                return "Fácil";
            else if (word.Length <= 7)
                return "Médio";
            else
                return "Difícil";
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

        public List<string> GetWordsByCategoryAndDifficulty(string categoryName, string difficulty)
        {
            var words = new List<string>();
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                SELECT Words.Word FROM Words
                JOIN Categories ON Words.CategoryId = Categories.Id
                WHERE Categories.Name = $categoryName AND Words.Difficulty = $difficulty";
                command.Parameters.AddWithValue("$categoryName", categoryName);
                command.Parameters.AddWithValue("$difficulty", difficulty);
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
                insertWordCmd.CommandText = "INSERT INTO Words (CategoryId, Word, Difficulty) VALUES ($categoryId, $word, $difficulty)";
                insertWordCmd.Parameters.AddWithValue("$categoryId", categoryId);
                insertWordCmd.Parameters.AddWithValue("$word", word);
                insertWordCmd.Parameters.AddWithValue("$difficulty", DetermineDifficulty(word));
                insertWordCmd.ExecuteNonQuery();

                connection.Close();
            }
        }
    }
}
