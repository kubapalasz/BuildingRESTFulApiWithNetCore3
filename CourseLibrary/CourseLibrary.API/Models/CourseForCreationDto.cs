using CourseLibrary.API.ValidationAttributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
    /*Custom attributes are executed before Validate method gets called*/
    [CourseTitleMustBeDiffetentFromDescriptionAttribute]
    public class CourseForCreationDto //: IValidatableObject
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(1500)]
        public string Description { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (Title == Description)
        //    {
        //        yield return new ValidationResult(
        //            $"{nameof(Title)} should be different from {nameof(Description)}", 
        //            new[] { "CourseForCreationDto" });
        //    }
        //}
    }
}
