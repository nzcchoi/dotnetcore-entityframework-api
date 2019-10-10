using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduler.Model
{
    public class User : IEntityBase, IValidatableObject
    {
        public User()
        {
            SchedulesCreated = new List<Schedule>();
            SchedulesAttended = new List<Attendee>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string Profession { get; set; }
        public ICollection<Schedule> SchedulesCreated { get; set; }
        public ICollection<Attendee> SchedulesAttended { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name)) {
                yield return new ValidationResult("Name cannot be empty", new string[] { "Name" });
            }
            if (string.IsNullOrWhiteSpace(Profession))
            {
                yield return new ValidationResult("Profession cannot be empty", new string[] { "Profession" });
            }
            if (string.IsNullOrWhiteSpace(Avatar))
            {
                yield return new ValidationResult("Avatar cannot be empty", new string[] { "Avatar" });
            }
        }
    }
}
