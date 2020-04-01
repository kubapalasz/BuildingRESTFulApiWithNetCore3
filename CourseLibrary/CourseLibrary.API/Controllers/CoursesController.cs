using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using CourseLibrary.API.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

            var courseEntity = new Course
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

        [HttpPut("{courseId}")]
        public IActionResult UpdateCourseForAuthor(Guid authorId, 
            Guid courseId,
            CourseForUpdateDto course)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
            {
                var newCourseEntity = new Course
                {
                    Title = course.Title,
                    Description = course.Description,
                    Id = courseId
                };

                _courseLibraryRepository.AddCourse(authorId, newCourseEntity);

                _courseLibraryRepository.Save();

                var courseToReturn = new CourseDto
                {
                    AuthorId = newCourseEntity.AuthorId,
                    Title = newCourseEntity.Title,
                    Description = newCourseEntity.Description,
                    Id = newCourseEntity.Id
                };

                return CreatedAtRoute(
                    "GetCourseForAuthor",
                    new
                    {
                        authorId = authorId,
                        courseId = courseToReturn.Id
                    },
                    courseToReturn);
            }

            // map entity to a CourseForUpdateDto
            var mappedEntity = new CourseForUpdateDto
            {
                Title = courseForAuthorFromRepo.Title,
                Description = courseForAuthorFromRepo.Description
            };

            // apply the updated field values to that dto
            mappedEntity.Title = course.Title;
            mappedEntity.Description = course.Description;

            // map the CourseForUpdateDto back to entity
            courseForAuthorFromRepo.Title = mappedEntity.Title;
            courseForAuthorFromRepo.Description = mappedEntity.Description;

            _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);

            _courseLibraryRepository.Save();


            return NoContent();
        }

        [HttpPatch("{courseId}")]
        public IActionResult PartiallyUpdateCourseForAuthor(Guid authorId,
            Guid courseId,
            JsonPatchDocument<CourseForUpdateDto> pathDocument)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null)
            {
                return NotFound();
            }

            var courseToPatch = new CourseForUpdateDto
            {
                Title = courseForAuthorFromRepo.Title,
                Description = courseForAuthorFromRepo.Description
            };

            // TODO - add validation
            pathDocument.ApplyTo(courseToPatch, ModelState);

            if (!TryValidateModel(courseToPatch))
            {
                return ValidationProblem(ModelState);
            }

            // BELOW IS REGULAR PATCH CODE
            // map entity to a CourseForUpdateDto
            var mappedEntity = new CourseForUpdateDto
            {
                Title = courseForAuthorFromRepo.Title,
                Description = courseForAuthorFromRepo.Description
            };

            // apply the updated field values to that dto
            mappedEntity.Title = courseToPatch.Title;
            mappedEntity.Description = courseToPatch.Description;

            // map the CourseForUpdateDto back to entity
            courseForAuthorFromRepo.Title = mappedEntity.Title;
            courseForAuthorFromRepo.Description = mappedEntity.Description;

            _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);

            _courseLibraryRepository.Save();
            return NoContent();
        }

        public override ActionResult ValidationProblem(
            [ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}
