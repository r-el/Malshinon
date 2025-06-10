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

        public override string ToString()
        {
            return $"[Person] Id: {Id}, FirstName: {FirstName}, LastName: {LastName}, Type: {Type}, NumReports: {NumReports}, NumMentions: {NumMentions}, SecretCode: {SecretCode}";
        }
    }
}
