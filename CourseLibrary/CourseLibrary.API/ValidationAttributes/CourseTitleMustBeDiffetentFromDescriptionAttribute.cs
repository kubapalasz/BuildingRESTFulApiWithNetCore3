using CourseLibrary.API.Models;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.ValidationAttributes
{
    public class CourseTitleMustBeDiffetentFromDescriptionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, 
            ValidationContext validationContext)
        {
            var course = (CourseForCreationDto)validationContext.ObjectInstance;

            if (course.Title == course.Description)
            {
                return new ValidationResult(
                    $"{nameof(course.Title)} should be different from {nameof(course.Description)}",
                    new[] { nameof(CourseForCreationDto) });
            }

            return ValidationResult.Success;
        }
    }
}
