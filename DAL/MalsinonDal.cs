using Malshinon.Entities;
using MySql.Data.MySqlClient;
using Type = Malshinon.Enums.Type;

namespace Malshinon.DAL
{
    public class MalshinonDal
    {
        private readonly string _connStr = "server=localhost;port=3307;user=root;password=;database=malshinon";
        private MySqlConnection? _conn;

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

        public MalshinonDal()
        {
            try { OpenConnection(); }
            catch (MySqlException ex) { Console.WriteLine($"MySQL Error: {ex.Message}"); }
            catch (Exception ex) { Console.WriteLine($"General Error: {ex.Message}"); }
        }

        public List<Person> FetchPeople(string query = "SELECT * FROM people")
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
                        reader.GetString("last_name"),
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

        public Person? AddPerson(Person _person)
        {
            // Check if person exists return null
            Person? person = GetPersonByFullName(_person.FirstName, _person.LastName);
            if (person != null)
                return null;

            MySqlDataReader? reader = null;
            try
            {
                OpenConnection();
                string query = @"INSERT INTO people
                            (first_name, last_name, secret_code, type, num_reports, num_mentions)
                            VALUES (@fname, @lname, @secret_code, @type, @num_reports, @num_mentions);";

                MySqlCommand cmd = new(query, _conn);

                cmd.Parameters.AddWithValue("@fname", _person.FirstName);
                cmd.Parameters.AddWithValue("@lname", _person.LastName);
                cmd.Parameters.AddWithValue("@secret_code", _person.SecretCode);
                cmd.Parameters.AddWithValue("@type", _person.Type.ToString());
                cmd.Parameters.AddWithValue("@num_reports", _person.NumReports);
                cmd.Parameters.AddWithValue("@num_mentions", _person.NumMentions);
                cmd.ExecuteNonQuery();

                CloseConnection();
                person = GetPersonByFullName(_person.FirstName, _person.LastName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while adding person {_person.Id}: {ex.Message}");
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                CloseConnection();
            }

            return person;
        }

        public bool UpdatePerson(Person person)
        {
            MySqlDataReader? reader = null;
            bool isUpdated = false;
            try
            {
                OpenConnection();
                string query = @"UPDATE people SET
                    first_name = @fname,
                    last_name = @lname,
                    secret_code = @secret_code,
                    type = @type,
                    num_reports = @num_reports,
                    num_mentions = @num_mentions
                    WHERE id = @id";

                MySqlCommand cmd = new(query, _conn);

                cmd.Parameters.AddWithValue("@id", person.Id);
                cmd.Parameters.AddWithValue("@fname", person.FirstName);
                cmd.Parameters.AddWithValue("@lname", person.LastName);
                cmd.Parameters.AddWithValue("@secret_code", person.SecretCode);
                cmd.Parameters.AddWithValue("@type", person.Type.ToString());
                cmd.Parameters.AddWithValue("@num_reports", person.NumReports);
                cmd.Parameters.AddWithValue("@num_mentions", person.NumMentions);

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

        public Person? AddNewReporter(string firstName, string? lastName) // TODO: maybe to move to controller file
        {
            Person? reporter = GetPersonByFullName(firstName, lastName);

            // if reporter exist
            if (reporter != null)
                return null;

            return AddPerson(new(firstName, lastName));
        }

        public Person? GetPersonByFullName(string firstName, string? lastName)
        {
            MySqlDataReader? reader = null;
            Person? person = null;
            try
            {
                OpenConnection();
                string query = @"SELECT * FROM people WHERE first_name = @fname AND last_name = @lname";

                MySqlCommand cmd = new(query, _conn);
                cmd.Parameters.AddWithValue("@fname", firstName);
                cmd.Parameters.AddWithValue("@lname", lastName);

                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    person = new(
                        reader.GetString("first_name"),
                        reader.GetString("last_name"),
                        reader.GetInt32("id"),
                        reader.GetGuid("secret_code"),
                        Enum.Parse<Type>(reader.GetString("type"), true),
                        reader.GetInt32("num_reports"),
                        reader.GetInt32("num_mentions")
                    );
                }
                CloseConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting person {firstName} {lastName}: {ex.Message}");
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                CloseConnection();
            }

            return person;
        }

        public bool PersonExists(string firstName, string? lastName) => GetPersonByFullName(firstName, lastName) != null;

        #region IntelReport
        public IntelReport? AddIntelReport(int reporterId, int targetId, string text)
        {
            MySqlDataReader? reader = null;
            IntelReport? intelReport = null;
            try
            {
                OpenConnection();
                string query = @"INSERT INTO intel_reports (reporter_id, target_id, text) 
                               VALUES (@reporter_id, @target_id, @text)";

                MySqlCommand cmd = new(query, _conn);
                cmd.Parameters.AddWithValue("@reporter_id", reporterId);
                cmd.Parameters.AddWithValue("@target_id", targetId);
                cmd.Parameters.AddWithValue("@text", text);

                cmd.ExecuteNonQuery();

                // Get the last inserted ID
                cmd = new("SELECT LAST_INSERT_ID()", _conn);
                int lastId = Convert.ToInt32(cmd.ExecuteScalar());

                CloseConnection();

                intelReport = new IntelReport(reporterId, targetId, text, lastId);
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error while adding intel report: {ex.Message}");
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                CloseConnection();
            }

            return intelReport;
        }

        internal Person? AddNewTarget(string targetFirstName, string? targeLastName)
        {
            Person? target = GetPersonByFullName(targetFirstName, targeLastName);

            // if target exist
            if (target != null)
                return null;

            return AddPerson(new(targetFirstName, targeLastName, type: Type.Target));
        }
        #endregion IntelReport
    }
}