using System.Collections.Generic;

namespace Ardalis.Result;

public class ValidationErrorGroup
{
    public string Name { get; set; }
    public List<ValidationError> ValidationErrors { get; set; }
}