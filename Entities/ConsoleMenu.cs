using System.Text;
using Malshinon.DAL;

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
                    PrintSection("[הוסף דוח מודיעין חדש (Add New Intelligence Report) - טרם מומש]", ConsoleColor.Green);
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
            bool personAdded;
            do
            {
                string firstName, lastName;
                (firstName, lastName) = Person.ReadFullNameFromConsole();
                personAdded = _dal.AddPerson(new Person(null, firstName, lastName, null));

                if (!personAdded)
                {
                    PrintError($"שגיאה: האדם {firstName} {lastName} כבר קיים במערכת!");
                    PrintError($"Error: Person {firstName} {lastName} already exists in the system!");
                    Console.WriteLine("\nנסה שוב עם שם אחר (Try again with a different name):");
                }
            } while (!personAdded);

            PrintSection("האדם נוסף בהצלחה למערכת! (Person added successfully!)", ConsoleColor.Green);
        }

        private static void FindPerson()
        {
            string firstName, lastName;
            (firstName, lastName) = Person.ReadFullNameFromConsole();
            Console.WriteLine(_dal.GetPersonByFullName(firstName, lastName));
        }

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

    }
}
