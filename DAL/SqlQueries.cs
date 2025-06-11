namespace Malshinon.DAL
{
    public static class SqlQueries
    {
        public const string InsertPerson = @"
            INSERT INTO people (first_name, last_name, secret_code, type, num_reports, num_mentions)
            VALUES (@fname, @lname, @secret_code, @type, @num_reports, @num_mentions);";

        public const string SelectAllPeople = "SELECT * FROM people";

        public const string SelectPersonByFullName = @"
            SELECT * FROM people 
            WHERE first_name = @firstName AND last_name = @lastName";

        public const string SelectPersonById = @"
            SELECT * FROM people 
            WHERE id = @id";

        public const string UpdatePerson = @"
            UPDATE people SET
                first_name = @firstName,
                last_name = @lastName,
                secret_code = @secterCode,
                type = @type,
                num_reports = @numReports,
                num_mentions = @numMentions
            WHERE id = @id";

        public const string InsertIntelReport = @"
            INSERT INTO intel_reports (reporter_id, target_id, text) 
            VALUES (@reporterId, @targetId, @text)";

        public const string GetLastInsertId = "SELECT LAST_INSERT_ID()";
    }
}