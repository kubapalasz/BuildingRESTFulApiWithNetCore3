using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;

        public AuthorCollectionsController(ICourseLibraryRepository courseLibraryRepository)
        {
            _courseLibraryRepository = courseLibraryRepository ?? throw new ArgumentNullException(nameof(courseLibraryRepository));
        }

        [HttpGet("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection(
        [FromRoute]
        [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var authorEntities = _courseLibraryRepository.GetAuthors(ids);

            if (ids.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn = authorEntities.Select(x => new AuthorDto
            {
                Id = x.Id,
                Name = $"{x.FirstName} {x.LastName}",
                MainCategory = x.MainCategory,
                Age = x.DateOfBirth.GetCurrentAge()
            }).ToList();

            return Ok(authorsToReturn);
        }

        [HttpPost]
        public ActionResult<IEnumerable<AuthorDto>> CreateAuthors(IEnumerable<AuthorForCreationDto> authorCollection) // automatically from body
        {
            var authorEntities = authorCollection.Select(x => new Entities.Author
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                DateOfBirth = x.DateOfBirth,
                MainCategory = x.MainCategory,
                Courses = x.Courses.Select(c => new Entities.Course
                {
                    Title = c.Title,
                    Description = c.Description
                }).ToList()
            }).ToList();

            foreach (var authorEntity in authorEntities)
            {
                _courseLibraryRepository.AddAuthor(authorEntity);
            }

            _courseLibraryRepository.Save();

            var authorsToReturn = authorEntities.Select(x => new AuthorDto
            {
                Id = x.Id,
                Name = $"{x.FirstName} {x.LastName}",
                MainCategory = x.MainCategory,
                Age = x.DateOfBirth.GetCurrentAge()
            }).ToList();

            var idsAsString = string.Join(",", authorsToReturn.Select(a => a.Id));
            return CreatedAtRoute("GetAuthorCollection",
             new { ids = idsAsString },
             authorsToReturn);
        }
    }
}
