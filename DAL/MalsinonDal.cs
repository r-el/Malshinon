using System.Data;
using Malshinon.Entities;
using MySql.Data.MySqlClient;
using Type = Malshinon.Enums.Type;

namespace Malshinon.DAL
{
    public sealed class MalshinonDal // Sealed to prevent inheritance
    {
        #region Singleton Implementation
        private static MalshinonDal? _instance = null;

        public static MalshinonDal Instance { get { return _instance ??= new MalshinonDal(); } }
        #endregion Singleton Implementation

        #region Fields and Constructor
        private readonly string _connStr = "server=localhost;port=3307;user=root;password=;database=malshinon";
        private MySqlConnection? _conn;

        private MalshinonDal() { /* Private constructor to prevent instantiation */ }
        #endregion Fields and Constructor

        #region Connection Management
        public MySqlConnection OpenConnection()
        {
            _conn ??= new MySqlConnection(_connStr);

            if (_conn.State != System.Data.ConnectionState.Open)
                _conn.Open();

            return _conn;
        }

        public void CloseConnection()
        {
            if (_conn != null && _conn.State == System.Data.ConnectionState.Open)
            {
                _conn.Close();
                _conn = null;
            }
        }
        #endregion Connection Management

        #region CREATE Person Section        
        public Person? CreatePersonIfNotExists(Person _person)
        {
            // Check if Person already exists (basic validation)
            if (GetPersonByFullName(_person.FirstName, _person.LastName) != null)
            {
                return null;
            }

            try
            {
                OpenConnection();

                MySqlCommand cmd = new(SqlQueries.InsertPerson, _conn);
                cmd.Parameters.AddWithValue("@fname", _person.FirstName);
                cmd.Parameters.AddWithValue("@lname", _person.LastName ?? "");
                cmd.Parameters.AddWithValue("@secret_code", _person.SecretCode);
                cmd.Parameters.AddWithValue("@type", _person.Type.ToString());
                cmd.Parameters.AddWithValue("@num_reports", _person.NumReports);
                cmd.Parameters.AddWithValue("@num_mentions", _person.NumMentions);

                if (cmd.ExecuteNonQuery() == 1)
                {
                    _person.Id = Convert.ToInt32(new MySqlCommand(SqlQueries.GetLastInsertId, _conn).ExecuteScalar());
                    return _person;
                }

                return null;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"[ERROR] MySQL error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] General error: {ex.Message}");
                return null;
            }
            finally { CloseConnection(); }
        }

        #endregion CREATE Person Section

        #region READ Person Section
        public List<Person> FetchPeople(string query = SqlQueries.SelectAllPeople)
        {
            List<Person> peopleList = [];
            MySqlDataReader? reader = null;
            try
            {
                OpenConnection();
                MySqlCommand cmd = new(query, _conn);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Person person = new(
                        reader.GetString("first_name"),
                        reader.IsDBNull("last_name") ? null : reader.GetString("last_name"),
                        reader.GetInt32("id"),
                        reader.GetGuid("secret_code"),
                        Enum.Parse<Type>(reader.GetString("type"), true),
                        reader.GetInt32("num_reports"),
                        reader.GetInt32("num_mentions")
                    );
                    peopleList.Add(person);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching people: {ex.Message}");
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                CloseConnection();
            }
            return peopleList;
        }

        public Person? GetPersonByFullName(string firstName, string? lastName)
        {
            MySqlDataReader? reader = null;
            Person? person = null;
            try
            {
                OpenConnection();
                MySqlCommand cmd = new(SqlQueries.SelectPersonByFullName, _conn);
                cmd.Parameters.AddWithValue("@firstName", firstName);
                cmd.Parameters.AddWithValue("@lastName", lastName ?? "");

                reader = cmd.ExecuteReader();
                if (reader.Read())
                    person = new(
                        reader.GetString("first_name"),
                        reader.IsDBNull("last_name") ? null : reader.GetString("last_name"),
                        reader.GetInt32("id"),
                        reader.GetGuid("secret_code"),
                        Enum.Parse<Type>(reader.GetString("type"), true),
                        reader.GetInt32("num_reports"),
                        reader.GetInt32("num_mentions")
                    );
            }
            catch (Exception ex) { Console.WriteLine($"Error while getting person {firstName} {lastName}: {ex.Message}"); }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                CloseConnection();
            }
            return person;
        }

        public Person? GetPersonById(int personId)
        {
            MySqlDataReader? reader = null;
            Person? person = null;
            try
            {
                if (personId <= 0)
                    throw new ArgumentException("Person ID must be positive", nameof(personId));

                OpenConnection();
                MySqlCommand cmd = new(SqlQueries.SelectPersonById, _conn);
                cmd.Parameters.AddWithValue("@id", personId);

                reader = cmd.ExecuteReader();
                if (reader.Read())
                    person = new(
                        reader.GetString("first_name"),
                        reader.IsDBNull("last_name") ? null : reader.GetString("last_name"),
                        reader.GetInt32("id"),
                        reader.GetGuid("secret_code"),
                        Enum.Parse<Type>(reader.GetString("type"), true),
                        reader.GetInt32("num_reports"),
                        reader.GetInt32("num_mentions")
                    );
            }
            catch (Exception ex) { Console.WriteLine($"Error while getting person by ID {personId}: {ex.Message}"); }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                CloseConnection();
            }
            return person;
        }

        public bool PersonExists(string firstName, string? lastName) => GetPersonByFullName(firstName, lastName) != null;
        #endregion READ Person Section

        #region UPDATE Person Section
        public bool UpdatePerson(Person person)
        {
            MySqlDataReader? reader = null;
            bool isUpdated = false;
            try
            {
                OpenConnection();
                MySqlCommand cmd = new(SqlQueries.UpdatePerson, _conn);

                cmd.Parameters.AddWithValue("@id", person.Id);
                cmd.Parameters.AddWithValue("@firstName", person.FirstName);
                cmd.Parameters.AddWithValue("@lastName", person.LastName ?? "");
                cmd.Parameters.AddWithValue("@secretCode", person.SecretCode);
                cmd.Parameters.AddWithValue("@type", person.Type.ToString());
                cmd.Parameters.AddWithValue("@numReports", person.NumReports);
                cmd.Parameters.AddWithValue("@numMentions", person.NumMentions);

                int rowsAffected = cmd.ExecuteNonQuery();
                isUpdated = rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while updating person {person.Id}: {ex.Message}");
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                CloseConnection();
            }

            return isUpdated;
        }
        #endregion UPDATE Person Section

        #region Helper Methods
        public double GetReporterAverageTextLength(int? reporterId)
        {
            if (reporterId == null || reporterId <= 0) return 0;

            double averageLength = 0;
            try
            {
                OpenConnection();
                MySqlCommand cmd = new(SqlQueries.GetReporterAverageTextLength, _conn);
                cmd.Parameters.AddWithValue("@reporterId", reporterId.Value);

                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    averageLength = Convert.ToDouble(result);
            }
            catch (Exception ex) { Console.WriteLine($"Error while getting average text length for reporter {reporterId}: {ex.Message}"); }
            finally { CloseConnection(); }

            return averageLength;
        }
        #endregion Helper Methods

        #region DELETE Person Section
        // DELETE methods if needed..
        #endregion DELETE Person Section

        #region IntelReport Section
        public IntelReport? AddIntelReport(IntelReport _intelReport)
        {
            try
            {
                OpenConnection();
                MySqlCommand cmd = new(SqlQueries.InsertIntelReport, _conn);
                cmd.Parameters.AddWithValue("@reporterId", _intelReport.Reporter.Id);
                cmd.Parameters.AddWithValue("@targetId", _intelReport.Target.Id);
                cmd.Parameters.AddWithValue("@text", _intelReport.Text);

                cmd.ExecuteNonQuery();

                // Get the last inserted ID
                cmd = new(SqlQueries.GetLastInsertId, _conn);
                int lastId = Convert.ToInt32(cmd.ExecuteScalar());

                _intelReport.Id = lastId;
                return _intelReport;
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"[ERROR] Error while submitting report: {ex.Message}");
                return null;
            }
            finally { CloseConnection(); }
        }
        #endregion IntelReport Section
    }
}
