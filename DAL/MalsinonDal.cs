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
            try
            {
                OpenConnection();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"MySQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
            }
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
                        reader.GetInt32("id"),
                        reader.GetString("first_name"),
                        reader.GetString("last_name"),
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

        public bool AddPerson(Person person)
        {
            // Check if person already exists
            if (PersonExists(person.FirstName, person.LastName))
            {
                return false;
            }

            MySqlDataReader? reader = null;
            bool isOk = false;
            try
            {
                OpenConnection();
                string query = @"INSERT INTO people
                            (first_name, last_name, secret_code, type, num_reports, num_mentions)
                            VALUES (@fname, @lname, @secret_code, @type, @num_reports, @num_mentions);";

                MySqlCommand cmd = new(query, _conn);

                cmd.Parameters.AddWithValue("@fname", person.FirstName);
                cmd.Parameters.AddWithValue("@lname", person.LastName);
                cmd.Parameters.AddWithValue("@secret_code", person.SecretCode);
                cmd.Parameters.AddWithValue("@type", person.Type.ToString());
                cmd.Parameters.AddWithValue("@num_reports", person.NumReports);
                cmd.Parameters.AddWithValue("@num_mentions", person.NumMentions);

                cmd.ExecuteNonQuery();
                CloseConnection();
                isOk = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while adding person {person.Id}: {ex.Message}");
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                CloseConnection();
            }

            return isOk;
        }

        public Person? GetPersonByFullName(string firstName, string lastName)
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
                        reader.GetInt32("id"),
                        reader.GetString("first_name"),
                        reader.GetString("last_name"),
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

        public bool PersonExists(string firstName, string lastName) => GetPersonByFullName(firstName, lastName) != null;
    }
}
