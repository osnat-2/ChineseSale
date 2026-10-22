using System.ComponentModel.DataAnnotations;

namespace Project.Dto;

public sealed class DevelopmentPaymentRequestDto : IValidatableObject
{
    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string CardholderName { get; set; } = string.Empty;

    [Required]
    [StringLength(19, MinimumLength = 13)]
    [RegularExpression(@"^[0-9]{13,19}\z")]
    [CreditCard]
    public string CardNumber { get; set; } = string.Empty;

    [Range(1, 12)]
    public int ExpirationMonth { get; set; }

    [Range(1, 9999)]
    public int ExpirationYear { get; set; }

    [Required]
    [RegularExpression(@"^[0-9]{3,4}\z")]
    public string SecurityCode { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var now = DateTime.UtcNow;
        if (ExpirationYear < now.Year ||
            ExpirationYear == now.Year && ExpirationMonth < now.Month)
        {
            yield return new ValidationResult(
                "The payment card has expired.",
                new[] { nameof(ExpirationMonth), nameof(ExpirationYear) });
        }
    }
}
