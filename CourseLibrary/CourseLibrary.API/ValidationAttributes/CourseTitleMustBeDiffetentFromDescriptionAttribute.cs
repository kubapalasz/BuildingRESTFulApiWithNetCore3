using CourseLibrary.API.Models;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.ValidationAttributes
{
    public class CourseTitleMustBeDiffetentFromDescriptionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, 
            ValidationContext validationContext)
        {
            var course = (CourseForManupulationDto)validationContext.ObjectInstance;

            if (course.Title == course.Description)
            {
                return new ValidationResult(
                    ErrorMessage,
                    //$"{nameof(course.Title)} should be different from {nameof(course.Description)}",
                    new[] { nameof(CourseForManupulationDto) });
            }

            return ValidationResult.Success;
        }
    }
}
