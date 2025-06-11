namespace Malshinon.Entities
{
    public class IntelReport(Person reporter, Person target, string text, int? id = null, DateTime? timestamp = null)
    {
        public int? Id { get; set; } = id;
        public Person Reporter { get; set; } = reporter;
        public Person Target { get; set; } = target;
        public string Text { get; set; } = text;
        public DateTime? Timestamp { get; set; } = timestamp;

        public override string ToString()
        {
            return $"[IntelReport] Id: {Id}, Reporter: {Reporter.FullName} (id={Reporter.Id}), Target: {Target.FullName} (id={Target.Id}), Text: {Text}, Timestamp: {Timestamp}";
        }

    }
}
