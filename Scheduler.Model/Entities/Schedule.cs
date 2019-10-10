using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduler.Model
{
    public class Schedule : IEntityBase, IValidatableObject
    {
        public Schedule()
        {
            Attendees = new List<Attendee>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public string Location { get; set; }
        public ScheduleType Type { get; set; }

        public ScheduleStatus Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public User Creator { get; set; }
        public int CreatorId { get; set; }
        public ICollection<Attendee> Attendees { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TimeStart > TimeEnd)
            {
                yield return new ValidationResult($"Schedule's End time must be greater than Start time", new[] { "TimeEnd" });
            }
        }
    }
}
