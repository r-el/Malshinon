using Malshinon.DAL;
using Malshinon.Entities;
using Type = Malshinon.Enums.Type;

namespace Malshinon.BusinessLogic
{
    public class IntelService
    {
        private readonly MalshinonDal _dal;

        public IntelService() { _dal = MalshinonDal.Instance; }


        public IntelReport? SubmitIntelReport(Person reporter, Person target, string reportText)
        {
            Console.WriteLine($"[INFO] Starting report submission: Reporter={reporter.FullName} (ID={reporter.Id}), Target={target.FullName} (ID={target.Id})");

            IntelReport intelReport = new(reporter, target, reportText);

            // Save in db
            IntelReport? savedReport = _dal.AddIntelReport(intelReport);
            if (savedReport == null)
                return null;

            Console.WriteLine($"[SUCCESS] Report successfully submitted with ID={savedReport.Id}");

            // Update statistics
            UpdateReporterStats(reporter);
            UpdateTargetStats(target);

            CheckReporterPromotion(reporter);
            
            CheckThreatAlert(target);

            return savedReport;
        }

        private void UpdateReporterStats(Person reporter)
        {
            reporter.NumReports += 1;
            _dal.UpdatePerson(reporter);
            Console.WriteLine($"[INFO] Updated reporter stats: {reporter.FullName} now has {reporter.NumReports} reports");
        }

        private void UpdateTargetStats(Person target)
        {
            target.NumMentions += 1;
            _dal.UpdatePerson(target);
            Console.WriteLine($"[INFO] Updated target stats: {target.FullName} now has {target.NumMentions} mentions");
        }

        private void CheckReporterPromotion(Person reporter)
        {
            if (reporter.NumReports >= 10)
            {
                double avgLength = _dal.GetReporterAverageTextLength(reporter.Id);
                if (avgLength >= 100)
                {
                    Console.WriteLine($"[ALERT] STATUS CHANGE: Promoting reporter to Potential Agent - {reporter.FullName} (ID={reporter.Id}) has {reporter.NumReports} reports with avg length {avgLength:F1} chars");
                    reporter.Type = Type.Potential_Agent;
                    _dal.UpdatePerson(reporter);
                    Console.WriteLine($"[SUCCESS] Successfully promoted {reporter.FullName} to Potential_Agent status");
                }
            }
        }

        private void CheckThreatAlert(Person target)
        {
            if (target.NumMentions >= 20)
                Console.WriteLine($"[ALERT] STATUS CHANGE: POTENTIAL THREAT ALERT - Target {target.FullName} (ID={target.Id}) has {target.NumMentions} mentions");
        }

        public Person? AddNewReporter(string firstName, string? lastName)
        {
            // Business rule: Check if reporter already exists
            Person? existingPerson = _dal.GetPersonByFullName(firstName, lastName);
            if (existingPerson != null)
            {
                Console.WriteLine($"[WARN] Person already exists: {existingPerson.FullName}");
                return null;
            }

            // Business rule: Create new reporter with default type
            return _dal.CreatePersonIfNotExists(new Person(firstName, lastName));
        }

        public Person? AddNewTarget(string firstName, string? lastName)
        {
            // Business rule: Check if target already exists
            var existingPerson = _dal.GetPersonByFullName(firstName, lastName);
            if (existingPerson != null)
            {
                Console.WriteLine($"[WARN] Person already exists: {existingPerson.FullName}");
                return null;
            }

            // Business rule: Create new target
            return _dal.CreatePersonIfNotExists(new Person(firstName, lastName, type: Type.Target));
        }

        public List<Person> GetAllPeople() => _dal.FetchPeople();

        public Person? GetPersonByFullName(string firstName, string? lastName) => _dal.GetPersonByFullName(firstName, lastName);

        public Person? GetPersonById(int id) => _dal.GetPersonById(id);

        public bool PersonExists(string firstName, string? lastName) => _dal.PersonExists(firstName, lastName);

        public bool UpdatePersonType(Person person, Type newType)
        {
            person.Type = newType;
            return _dal.UpdatePerson(person);
        }
    }
}
