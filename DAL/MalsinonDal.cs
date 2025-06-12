using System.Data;
using Malshinon.Entities;
using MySql.Data.MySqlClient;
using Type = Malshinon.Enums.Type;

namespace Malshinon.DAL
{
    public class MalshinonDal
    {
        #region Fields and Constructor
        private readonly string _connStr = "server=localhost;port=3307;user=root;password=;database=malshinon";
        private MySqlConnection? _conn;

        public MalshinonDal()
        {
            try { OpenConnection(); }
            catch (MySqlException ex) { Console.WriteLine($"MySQL Error: {ex.Message}"); }
            catch (Exception ex) { Console.WriteLine($"General Error: {ex.Message}"); }
        }
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
        public Person? AddPerson(Person _person)
        {
            // If person exists return null
            Person? person = GetPersonByFullName(_person.FirstName, _person.LastName);
            if (person != null)
            {
                Console.WriteLine($"[WARN] Person already exists: {_person.FullName}");
                return null;
            }

            try
            {
                Console.WriteLine($"[INFO] Creating new person: {_person.FullName} as {_person.Type}");

                OpenConnection();
                MySqlCommand cmd = new(SqlQueries.InsertPerson, _conn);

                cmd.Parameters.AddWithValue("@fname", _person.FirstName);
                cmd.Parameters.AddWithValue("@lname", _person.LastName ?? "");
                cmd.Parameters.AddWithValue("@secret_code", _person.SecretCode);
                cmd.Parameters.AddWithValue("@type", _person.Type.ToString());
                cmd.Parameters.AddWithValue("@num_reports", _person.NumReports);
                cmd.Parameters.AddWithValue("@num_mentions", _person.NumMentions);
                cmd.ExecuteNonQuery();

                CloseConnection();
                person = GetPersonByFullName(_person.FirstName, _person.LastName);

                if (person != null) Console.WriteLine($"[SUCCESS] Person created successfully: ID={person.Id}, Name={person.FullName}, Type={person.Type}");
            }
            catch (Exception ex) { Console.WriteLine($"[ERROR] Failed to add person {_person.FullName}: {ex.Message}"); }
            finally { CloseConnection(); }

            return person;
        }

        // reutrn new reporter if not exist
        public Person? AddNewReporter(string firstName, string? lastName) // TODO: maybe to move to controller file
        {
            Person? reporter = GetPersonByFullName(firstName, lastName);

            // reutrn bew reporter if not exist
            return (reporter != null) ? null : AddPerson(new(firstName, lastName));
        }

        // return new target if not exist
        public Person? AddNewTarget(string targetFirstName, string? targetLastName)
        {
            Person? target = GetPersonByFullName(targetFirstName, targetLastName);

            // return new target if not exist
            return (target != null) ? null : AddPerson(new(targetFirstName, targetLastName, type: Type.Target));
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
                cmd.Parameters.AddWithValue("@secterCode", person.SecretCode);
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
        private double GetReporterAverageTextLength(int? reporterId)
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
            Console.WriteLine($"[INFO] Starting report submission: Reporter={_intelReport.Reporter.FullName} (ID={_intelReport.Reporter.Id}), Target={_intelReport.Target.FullName} (ID={_intelReport.Target.Id})");

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

                CloseConnection();

                _intelReport.Id = lastId;

                Console.WriteLine($"[SUCCESS] Report successfully submitted with ID={lastId}");

                // Increace numReaport in Reporter
                _intelReport.Reporter.NumReports += 1;
                UpdatePerson(_intelReport.Reporter);
                Console.WriteLine($"[INFO] Updated reporter stats: {_intelReport.Reporter.FullName} now has {_intelReport.Reporter.NumReports} reports");

                // Increace numMentions in Target
                _intelReport.Target.NumMentions += 1;
                UpdatePerson(_intelReport.Target);
                Console.WriteLine($"[INFO] Updated target stats: {_intelReport.Target.FullName} now has {_intelReport.Target.NumMentions} mentions");

                // Check if reporter should be promoted to potential agent
                if (_intelReport.Reporter.NumReports >= 10)
                    if (GetReporterAverageTextLength(_intelReport.Reporter.Id) >= 100)
                    {
                        Console.WriteLine($"[ALERT] STATUS CHANGE: Promoting reporter to Potential Agent - {_intelReport.Reporter.FullName} (ID={_intelReport.Reporter.Id}) has {_intelReport.Reporter.NumReports} reports with avg length {GetReporterAverageTextLength(_intelReport.Reporter.Id):F2} chars");
                        _intelReport.Reporter.Type = Type.Potential_Agent;
                        UpdatePerson(_intelReport.Reporter);
                        Console.WriteLine($"[SUCCESS] Successfully promoted {_intelReport.Reporter.FullName} to Potential_Agent status");
                    }

                // Check if target should trigger threat alert
                if (_intelReport.Target.NumMentions >= 20)
                    Console.WriteLine($"[ALERT] STATUS CHANGE: POTENTIAL THREAT ALERT - Target {_intelReport.Target.FullName} (ID={_intelReport.Target.Id}) has {_intelReport.Target.NumMentions} mentions");
            }
            catch (Exception ex) { Console.WriteLine($"[ERROR] Error while submitting report: {ex.Message}"); }
            finally { CloseConnection(); }

            return _intelReport;
        }
        #endregion IntelReport Section
    }
}
