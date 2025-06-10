namespace Malshinon.Entities
{
    public class IntelReport(int reporterId, int target_id, string text, int? id = null, DateTime? timestamp = null)
    {
        public int? Id { get; set; } = id;
        public int ReporterId { get; set; } = reporterId;
        public int TargetId { get; set; } = target_id;
        public string Text { get; set; } = text;
        public DateTime? Timestamp { get; set; } = timestamp;

        public override string ToString()
        {
            return $"[IntelReport] Id: {Id}, ReporterId: {ReporterId}, TargetId: {TargetId}, Text: {Text}, Timestamp: {Timestamp}";
        }

    }
}
