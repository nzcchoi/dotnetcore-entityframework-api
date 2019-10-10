using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Data.Abstract;
using Scheduler.Model;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Scheduler.API.Controllers
{
    [Route("api/[controller]")]
    public class SchedulesController : Controller
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IAttendeeRepository _attendeeRepository;
        private readonly IUserRepository _userRepository;
        int page = 1;
        int pageSize = 4;
        public SchedulesController(IScheduleRepository scheduleRepository,
                                    IAttendeeRepository attendeeRepository,
                                    IUserRepository userRepository)
        {
            _scheduleRepository = scheduleRepository;
            _attendeeRepository = attendeeRepository;
            _userRepository = userRepository;
        }

        public IEnumerable<Schedule> Get()
        {
            var pagination = Request.Headers["Pagination"];

            if (!string.IsNullOrEmpty(pagination))
            {
                string[] vals = pagination.ToString().Split(',');
                int.TryParse(vals[0], out page);
                int.TryParse(vals[1], out pageSize);
            }

            int currentPage = page;
            int currentPageSize = pageSize;
            var totalSchedules = _scheduleRepository.Count();
            var totalPages = (int)Math.Ceiling((double)totalSchedules / pageSize);

            return _scheduleRepository
                .AllIncluding(s => s.Creator, s => s.Attendees)
                .OrderBy(s => s.Id)
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToList();

        }

        [HttpGet("{id}", Name = "GetSchedule")]
        public Schedule Get(int id)
        {
            return _scheduleRepository
                .GetSingle(s => s.Id == id, s => s.Creator, s => s.Attendees);
        }

        [HttpGet("{id}/details", Name = "GetScheduleDetails")]
        public Schedule GetScheduleDetails(int id)
        {
            return _scheduleRepository
                .GetSingle(s => s.Id == id, s => s.Creator, s => s.Attendees);
        }

        [HttpPost]
        public Schedule Create([FromBody]Schedule schedule)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception();
            }

            schedule.DateCreated = DateTime.Now;

            _scheduleRepository.Add(schedule);
            _scheduleRepository.Commit();
            return schedule;
        }

        [HttpPut("{id}")]
        public Schedule Put(int id, [FromBody]Schedule schedule)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception();
            }

            Schedule scheduleDb = _scheduleRepository.GetSingle(id);

            if (scheduleDb == null)
            {
                throw new KeyNotFoundException();
            }
            else
            {
                scheduleDb.Title = schedule.Title;
                scheduleDb.Location = schedule.Location;
                scheduleDb.Description = schedule.Description;
                scheduleDb.Status = schedule.Status;
                scheduleDb.Type = schedule.Type;
                scheduleDb.TimeStart = schedule.TimeStart;
                scheduleDb.TimeEnd = schedule.TimeEnd;

                // Remove current attendees
                _attendeeRepository.DeleteWhere(a => a.ScheduleId == id);

                foreach (var user in schedule.Attendees)
                {
                    scheduleDb.Attendees.Add(user);
                }

                _scheduleRepository.Commit();
            }
            return scheduleDb;
        }

        [HttpDelete("{id}", Name = "RemoveSchedule")]
        public IActionResult Delete(int id)
        {
            Schedule _scheduleDb = _scheduleRepository.GetSingle(id);

            if (_scheduleDb == null)
            {
                return new NotFoundResult();
            }
            else
            {
                _attendeeRepository.DeleteWhere(a => a.ScheduleId == id);
                _scheduleRepository.Delete(_scheduleDb);

                _scheduleRepository.Commit();

                return new NoContentResult();
            }
        }

        [HttpDelete("{id}/removeattendee/{attendee}")]
        public IActionResult Delete(int id, int attendee)
        {
            Schedule _scheduleDb = _scheduleRepository.GetSingle(id);

            if (_scheduleDb == null)
            {
                return new NotFoundResult();
            }
            else
            {
                _attendeeRepository.DeleteWhere(a => a.ScheduleId == id && a.UserId == attendee);

                _attendeeRepository.Commit();

                return new NoContentResult();
            }
        }
    }
}
