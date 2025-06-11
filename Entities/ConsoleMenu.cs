using System.Text;
using Malshinon.DAL;
using Type = Malshinon.Enums.Type;

namespace Malshinon.Entities
{
    // Handles all user interaction and menu logic
    public static class ConsoleMenu
    {
        private static readonly MalshinonDal _dal = new();
        public static void Run()
        {
            while (true)
            {
                Console.Clear();
                PrintMenu();
                string input = ReadMenuSelectionFromConsole();
                HandleMenuSelection(int.Parse(input));
                ContinuePrompt();
            }
        }

        private static string ReadMenuSelectionFromConsole()
        {
            string input;
            while (!IsValidMenuOption(input = Console.ReadLine() ?? ""))
            {
                PrintError("בחירה לא חוקית, נסה שוב. (Invalid choice, try again.)");
                Console.Write("בחר פעולה (Select option): ");
            }
            return input;
        }

        private static void PrintMenu()
        {
            // TODO: Add option to select a language
            StringBuilder sb = new();
            sb.AppendLine("╔════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║          MALSHINON: — Community Intel Reporting System         ║");
            sb.AppendLine("╠════════════════════════════════════════════════════════════════╣");
            sb.AppendLine("║  ברוך הבא למערכת דיווח מודיעין קהילתי                          ║");
            sb.AppendLine("║  Welcome to Community Intel Reporting System                   ║");
            sb.AppendLine("╠════════════════════════════════════════════════════════════════╣");
            sb.AppendLine("║  1. הוספת אדם (Add Person)                                     ║");
            sb.AppendLine("║  2. חפש אדם (Search Person)                                    ║");
            sb.AppendLine("║  3. קבלת כל האנשים (Get All People)                            ║");
            sb.AppendLine("║  4. הוסף דוח מודיעין חדש (Add New Intelligence Report)         ║");
            sb.AppendLine("║  5. הצג התראות (View Alerts)                                   ║");
            sb.AppendLine("║  0. יציאה (Exit)                                               ║");
            sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(sb.ToString());
            Console.ResetColor();
            Console.Write("\nבחר פעולה (Select option): ");
        }
        private static void HandleMenuSelection(int input)
        {
            switch (input)
            {
                case 1:
                    AddNewPerson();
                    break;
                case 2:
                    FindPerson();
                    break;
                case 3:
                    Console.WriteLine(string.Join("\n", _dal.FetchPeople()));
                    break;
                case 4:
                    AddNewIntelReport();
                    break;
                case 5:
                    PrintSection("[הצג התראות (View Alerts) - טרם מומש]", ConsoleColor.Green);
                    break;
                case 0:
                    PrintSection("להתראות! (Goodbye!)", ConsoleColor.Red);
                    Environment.Exit(0);
                    break;
                default:
                    PrintError("בחירה לא חוקית, נסה שוב. (Invalid choice, try again.)");
                    break;
            }
        }

        private static void AddNewPerson()
        {
            Person? person;
            string firstName;
            string? lastName;

            do
            {
                (firstName, lastName) = Person.ReadFullNameFromConsole();
                person = _dal.GetPersonByFullName(firstName, lastName);
                if (person is null)
                    break;

                PrintError($"שגיאה: האדם {person.FirstName} {person.LastName} כבר קיים במערכת! (id={person.Id})");
                PrintError($"Error: Person {person.FirstName} {person.LastName} already exists in the system! (id={person.Id})");
                Console.WriteLine("\nנסה שוב עם שם אחר (Try again with a different name):");
            } while (true);

            person = _dal.AddPerson(new Person(firstName, lastName));
            PrintSection($"האדם נוסף בהצלחה למערכת! (Person added successfully!) (id={person?.Id})", ConsoleColor.Green);
        }

        private static void FindPerson()
        {
            (string reporterFirstName, string? reporterLastName) = Person.ReadFullNameFromConsole();
            Person? person = _dal.GetPersonByFullName(reporterFirstName, reporterLastName);

            if (person != null)
                Console.WriteLine(person);
            else
            {
                PrintError($"האדם {reporterFirstName} {reporterLastName} לא נמצא במערכת.");
                PrintError($"Person {reporterFirstName} {reporterLastName} not found in the system.");
            }
        }

        private static void AddNewIntelReport()
        {
            // Identify the reporter
            (string reporterFirstName, string? reporterLastName) = Person.ReadFullNameFromConsole();
            Person? reporter = _dal.GetPersonByFullName(reporterFirstName, reporterLastName);

            // If reporter not exist, create new Reporter
            if (reporter is null)
                reporter = _dal.AddNewReporter(reporterFirstName, reporterLastName);

            // If person type is 'Target', update Type to 'Both'
            else if (reporter.Type == Type.Target) // What if the person type of reporter is Potential_Agent ???
            {
                reporter.Type = Type.Both;
                _dal.UpdatePerson(reporter);
            }

            // Get the a valid report including target name
            (string textReport, string targetFirstName, string? targetLastName) = ReadIntelReportFromConsole();

            // Identify the target
            Person? target = _dal.GetPersonByFullName(targetFirstName, targetLastName);

            // If target not exist, create new 'Target'
            if (target is null)
                target = _dal.AddNewTarget(targetFirstName, targetLastName); // Fix it

            // If person type of 'Target' is 'Reporter', update Type to 'Both'
            else if (target.Type == Type.Reporter) // What if the person type of target is Potential_Agent ???
            {
                target.Type = Type.Both;
                _dal.UpdatePerson(target);
            }

            if (reporter != null && target != null)
            {
                IntelReport intelReport = new(reporter, target, textReport);
                Console.WriteLine(_dal.AddIntelReport(intelReport));
                PrintSection("Intelligence report saved successfully.", ConsoleColor.Green);
            }
            else PrintError("Error. Cannot insert IntelReport if Reporter or Target is null.");
        }

        public static (string textReport, string firstName, string? lastName) ReadIntelReportFromConsole()
        {
            string freeTextReport;
            string? firstName, lastName;
            do
            {
                Console.WriteLine("Please enter your full intelligence report including the target name (in Capitalized Case): ");
                freeTextReport = Console.ReadLine() ?? "";
                (firstName, lastName) = Person.ExtractFullNameFromReport(freeTextReport);

                if (string.IsNullOrWhiteSpace(firstName))
                    PrintError("The intelligence report must include the target name.");
            } while (string.IsNullOrWhiteSpace(firstName));

            return (freeTextReport, firstName, lastName);
        }
        #region Menu Function
        /* Check if a string matches one of the options in the menu (0-5)*/
        private static bool IsValidMenuOption(string menuOption) =>
            int.TryParse(menuOption, out int val) && val >= 0 && val <= 5;

        private static void PrintSection(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        private static void PrintError(string message)
        {
            PrintSection(message, ConsoleColor.Red);
        }
        private static void ContinuePrompt()
        {
            Console.WriteLine("\nלחץ על מקש כלשהו להמשך...");
            Console.ReadKey();
        }
        #endregion Menu Function
    }
}
