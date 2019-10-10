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
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IAttendeeRepository _attendeeRepository;

        int page = 1;
        int pageSize = 10;
        public UsersController(IUserRepository userRepository,
                                IScheduleRepository scheduleRepository,
                                IAttendeeRepository attendeeRepository)
        {
            _userRepository = userRepository;
            _scheduleRepository = scheduleRepository;
            _attendeeRepository = attendeeRepository;
        }

        public IEnumerable<User> Get()
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
            var totalUsers = _userRepository.Count();
            var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            return _userRepository
                .AllIncluding(u => u.SchedulesCreated)
                .OrderBy(u => u.Id)
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToList();
        }

        [HttpGet("{id}", Name = "GetUser")]
        public User Get(int id)
        {
            return _userRepository.GetSingle(u => u.Id == id, u => u.SchedulesCreated);
        }

        [HttpGet("{id}/schedules", Name = "GetUserSchedules")]
        public IEnumerable<Schedule> GetSchedules(int id)
        {
            return _scheduleRepository.FindBy(s => s.CreatorId == id);
        }
        [HttpPost]
        public User Create([FromBody]User user)
        {
            User _newUser = new User { Name = user.Name, Profession = user.Profession, Avatar = user.Avatar };

            _userRepository.Add(_newUser);
            _userRepository.Commit();
            return _newUser;
        }

        [HttpPut("{id}")]
        public User Put(int id, [FromBody]User user)
        {
            if (!ModelState.IsValid)
            {
                return null;
            }

            User userDb = _userRepository.GetSingle(id);

            if (userDb == null)
            {
                throw new KeyNotFoundException();
            }
            else
            {
                userDb.Name = user.Name;
                userDb.Profession = user.Profession;
                userDb.Avatar = user.Avatar;
                _userRepository.Commit();
            }
            return userDb;
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            User _userDb = _userRepository.GetSingle(id);

            if (_userDb == null)
            {
                return new NotFoundResult();
            }
            else
            {
                IEnumerable<Attendee> _attendees = _attendeeRepository.FindBy(a => a.UserId == id);
                IEnumerable<Schedule> _schedules = _scheduleRepository.FindBy(s => s.CreatorId == id);

                foreach (var attendee in _attendees)
                {
                    _attendeeRepository.Delete(attendee);
                }

                foreach (var schedule in _schedules)
                {
                    _attendeeRepository.DeleteWhere(a => a.ScheduleId == schedule.Id);
                    _scheduleRepository.Delete(schedule);
                }

                _userRepository.Delete(_userDb);

                _userRepository.Commit();

                return new NoContentResult();
            }
        }

    }

}
