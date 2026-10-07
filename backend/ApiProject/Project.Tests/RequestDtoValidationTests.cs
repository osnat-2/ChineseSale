using System.ComponentModel.DataAnnotations;
using Project.Dto;
using Xunit;

namespace Project.Tests;

public class RequestDtoValidationTests
{
    [Fact]
    public void UserDto_RejectsInvalidEmailPhoneAndPassword()
    {
        var errors = Validate(new UserDto
        {
            Name = "Test User",
            Phone = "123",
            Email = "not-an-email",
            Password = "x"
        });

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UserDto.Phone)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UserDto.Email)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UserDto.Password)));
    }

    [Fact]
    public void UserDto_RejectsNameShorterThanRegistrationFormMinimum()
    {
        var errors = Validate(new UserDto
        {
            Name = "X",
            Phone = "123456789",
            Email = "test@example.com",
            Password = "pass"
        });

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UserDto.Name)));
    }

    [Fact]
    public void PresentDto_RejectsMissingFieldsAndOutOfRangeValues()
    {
        var errors = Validate(new PresentDto
        {
            Name = "",
            Description = new string('x', 2001),
            DonorId = 0,
            CategoryId = -1,
            ImageUrl = "javascript:alert(1)",
            Quantity = 0,
            Price = -1
        });

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(PresentDto.Name)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(PresentDto.Description)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(PresentDto.DonorId)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(PresentDto.CategoryId)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(PresentDto.ImageUrl)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(PresentDto.Quantity)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(PresentDto.Price)));
    }

    [Fact]
    public void DonorUpdateDto_RejectsInvalidContactDetails()
    {
        var errors = Validate(new DonorUpdateDto
        {
            Name = "D",
            Phone = "123",
            Email = "not-an-email"
        });

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(DonorUpdateDto.Name)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(DonorUpdateDto.Phone)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(DonorUpdateDto.Email)));
    }

    [Fact]
    public void CategoryDto_RejectsBlankAndOversizedName()
    {
        Assert.NotEmpty(Validate(new CategoryDto { Name = " " }));
        Assert.NotEmpty(Validate(new CategoryDto { Name = new string('x', 101) }));
    }

    [Fact]
    public void CardDto_RejectsNonPositivePresentId()
    {
        Assert.NotEmpty(Validate(new CardDto { PresentId = 0 }));
    }

    private static List<ValidationResult> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);
        return results;
    }
}
