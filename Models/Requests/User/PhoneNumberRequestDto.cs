using System.ComponentModel.DataAnnotations;
using BackEnd.Models.Constants.Errors;

namespace BackEnd.Models.Requests.User;

public class PhoneNumberRequestDto
{
    [Required(ErrorMessage = PhoneNumberError.PhoneNumberRequired)]
    // Pattern:
    // ^\+595  -> Must start with +595
    // [2-9]   -> The next digit cannot be 0 or 1 (typical for area/cell codes)
    // \d{8}   -> Exactly 8 numeric digits after the first digit
    // $       -> End of the string (prevents extra characters)
    [RegularExpression(@"^\+595[2-9]\d{8}$",
    ErrorMessage = PhoneNumberError.InvalidPhoneNumber)]
    public string Number { get; set; } = null!;
}