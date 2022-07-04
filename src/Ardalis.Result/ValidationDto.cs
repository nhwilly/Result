using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Ardalis.Result
{
    public class ValidationDto
    {
        public string Type => "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        public string Title { get; set; }
        public HttpStatusCode Status { get; set; }

        public List<ValidationErrorGroup> ValidationErrorGroups { get; set; } = new List<ValidationErrorGroup>();

        public ValidationDto(
            IReadOnlyCollection<ValidationError> errors, 
            HttpStatusCode status = HttpStatusCode.BadRequest, 
            string title = "One or more validation errors occurred.")
        {
            Title = title;
            Status = status;

            if (errors == null || !errors.Any()) return;

            ValidationErrorGroups = errors
                .GroupBy(g => g.Identifier)
                .Select(v =>
                    new ValidationErrorGroup { Name = v.Key, ValidationErrors = v.ToList() }
                )
                .ToList();
        }

        /// <summary>
        /// This method is to aid in unit tests.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasErrorWithName(string name)
        {
            return ValidationErrorGroups.FirstOrDefault(x => x.Name == name) != null;
        }
    }
}
