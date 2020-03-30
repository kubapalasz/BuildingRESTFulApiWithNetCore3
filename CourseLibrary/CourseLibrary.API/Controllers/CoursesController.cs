using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;

        public CoursesController(ICourseLibraryRepository courseLibraryRepository)
        {
            _courseLibraryRepository = courseLibraryRepository ?? throw new ArgumentNullException(nameof(courseLibraryRepository));
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courses = _courseLibraryRepository.GetCourses(authorId);
            return Ok(courses.Select(x => new CourseDto
            {
                AuthorId = x.AuthorId,
                Description = x.Description,
                Id = x.Id,
                Title = x.Title
            }));
        }

        [HttpGet("{courseId:guid}", Name = "GetCourseForAuthor")]
        public ActionResult<CourseDto> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var course = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (course == null)
            {
                return NotFound();
            }

            return Ok(new CourseDto
            {
                AuthorId = course.AuthorId,
                Description = course.Description,
                Id = course.Id,
                Title = course.Title
            });
        }

        [HttpPost]
        public ActionResult<CourseDto> CreateCourseForAuthor(
            Guid authorId,
            CourseForCreationDto courseForCreationDto)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseEntity = new Entities.Course
            {
                Title = courseForCreationDto.Title,
                Description = courseForCreationDto.Description
            };

            _courseLibraryRepository.AddCourse(authorId, courseEntity);
            _courseLibraryRepository.Save();

            var courseToReturn = new CourseDto
            {
                Id = courseEntity.Id,
                AuthorId = courseEntity.AuthorId,
                Description = courseEntity.Description,
                Title = courseEntity.Title
            };

            return CreatedAtRoute(
                "GetCourseForAuthor",
                new
                {
                    authorId,
                    courseId = courseToReturn.Id
                },
                courseToReturn);
        }
    }
}
