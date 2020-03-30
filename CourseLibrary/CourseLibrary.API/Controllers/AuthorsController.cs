using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository)
        {
            _courseLibraryRepository = courseLibraryRepository ?? throw new ArgumentNullException(nameof(courseLibraryRepository));
        }

        [HttpGet()]
        [HttpHead]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors(
            [FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            var authorsFromRepo = _courseLibraryRepository.GetAuthors(authorsResourceParameters);
            var authors = new List<AuthorDto>();

            foreach (var author in authorsFromRepo)
            {
                authors.Add(new AuthorDto
                {
                    Id = author.Id,
                    Name = $"{author.FirstName} {author.LastName}",
                    MainCategory = author.MainCategory,
                    Age = author.DateOfBirth.GetCurrentAge()
                });
            }

            return Ok(authors);
        }

        [HttpGet("{authorId:guid}", Name = "GetAuthor")]
        public ActionResult<AuthorDto> GetAuthor(Guid authorId)
        {
            var author = _courseLibraryRepository.GetAuthor(authorId);

            if (author == null)
            {
                return NotFound();
            }

            return Ok(new AuthorDto
            {
                Id = author.Id,
                Name = $"{author.FirstName} {author.LastName}",
                MainCategory = author.MainCategory,
                Age = author.DateOfBirth.GetCurrentAge()
            });
        }

        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto author) // automatically from body
        {
            // We do have ApiController attribute so bad request will be automatically returned
            //if (author == null)
            //{
            //    return BadRequest();
            //}
            var authorEntity = new Entities.Author
            {
                FirstName = author.FirstName,
                LastName = author.LastName,
                DateOfBirth = author.DateOfBirth,
                MainCategory = author.MainCategory,
                Courses = author.Courses.Select(x => new Entities.Course
                {
                    Title = x.Title,
                    Description = x.Description
                }).ToList()
            };

            _courseLibraryRepository.AddAuthor(authorEntity);
            _courseLibraryRepository.Save();

            var authorToReturn = new AuthorDto
            {
                Id = authorEntity.Id,
                Name = $"{authorEntity.FirstName} {authorEntity.LastName}",
                MainCategory = authorEntity.MainCategory,
                Age = authorEntity.DateOfBirth.GetCurrentAge()
            };

            return CreatedAtRoute(
                "GetAuthor",
                new { authorId = authorToReturn.Id },
                authorToReturn);
        }
    }
}
