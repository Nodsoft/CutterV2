using System.ComponentModel.DataAnnotations;

namespace Nodsoft.Cutter.Infrastructure.Validation;

public class OptionalLengthAttribute : LengthAttribute
{
    public OptionalLengthAttribute(int minimumLength, int maximumLength) : base(minimumLength, maximumLength) { }
    
    public override bool IsValid(object? value) => value is null or "" || base.IsValid(value);
}