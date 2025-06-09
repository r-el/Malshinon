using Type = Malshinon.Enums.Type;

namespace Malshinon.Entities
{
      public class Person(int? id, string firstName, string lastName, Guid? secretCode = null, Type type = Type.Reporter, int numReports = 0, int numMentions = 0)
      {
            public int? Id { get; set; } = id; // Can be null here
            public string FirstName { get; set; } = firstName;
            public string LastName { get; set; } = lastName;
            public Guid SecretCode { get; set; } = secretCode ?? Guid.NewGuid();
            public Type Type { get; set; } = type;
            public int NumReports { get; set; } = numReports;
            public int NumMentions { get; set; } = numMentions;

            public string FullName => FirstName + LastName;

            public static (string firstName, string lastName) ReadFullNameFromConsole()
            {
                  Console.Write("Enter first name: ");
                  string firstName = Console.ReadLine() ?? "";

                  Console.Write("Enter last name: ");
                  string lastName = Console.ReadLine() ?? "";

                  return (firstName, lastName);
            }

            public override string ToString()
            {
                  return $"[Person] Id: {Id}, FirstName: {FirstName}, LastName: {LastName}, Type: {Type}, NumReports: {NumReports}, NumMentions: {NumMentions}, SecretCode: {SecretCode}";
            }
      }
}