using Type = Malshinon.Enums.Type;

namespace Malshinon.Entities
{
    public class Person(string firstName, string? lastName, int? id = null, Guid? secretCode = null, Type type = Type.Reporter, int numReports = 0, int numMentions = 0)
    {
        public int? Id { get; set; } = id; // Can be null here
        public string FirstName { get; set; } = firstName;
        public string? LastName { get; set; } = lastName;
        public Guid SecretCode { get; set; } = secretCode ?? Guid.NewGuid();
        public Type Type { get; set; } = type;
        public int NumReports { get; set; } = numReports;
        public int NumMentions { get; set; } = numMentions;

        public string FullName => FirstName + LastName ?? "";

        public static (string firstName, string? lastName) ReadFullNameFromConsole()
        {
            string? firstName, lastName;
            do
            {
                Console.Write("Enter first name: ");
                firstName = Console.ReadLine();

                Console.Write("Enter last name: ");
                lastName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(firstName)) Console.WriteLine("The first name can not be an empty. Please try again.");
            } while (string.IsNullOrWhiteSpace(firstName));

            return (firstName, lastName);
        }

        public static (string? firstName, string? lastName) ExtractFullNameFromReport(string? freeTextReport)
        {
            if (string.IsNullOrWhiteSpace(freeTextReport))
                return (null, null);

            string[] words = freeTextReport.Trim().Split();

            List<string> names = [];
            for (int i = 0; i < words.Length; i++)
                if (char.IsUpper(words[i][0]))
                {
                    while (i < words.Length && char.IsUpper(words[i][0]))
                        names.Add(words[i++]);
                    break; // Break when we find the first name
                }

            if (names.Count < 2)
                return ((names.Count > 0 ? names[0] : null), null);

            string firstName = string.Join(" ", names[0..(names.Count - 1)]);
            string lastName = names[^1]; // C#8+ index operator
            return (firstName, lastName);
        }

        public override string ToString()
        {
            return $"[Person] Id: {Id}, FirstName: {FirstName}, LastName: {LastName}, Type: {Type}, NumReports: {NumReports}, NumMentions: {NumMentions}, SecretCode: {SecretCode}";
        }
    }
}
