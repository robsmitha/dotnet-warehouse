using System;

namespace Domain.Entities
{
    public class Lineage
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public DateTime StartLoad { get; set; }
        public DateTime? FinishLoad { get; set; }
        public DateTime LastLoadDate { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
    }
}
