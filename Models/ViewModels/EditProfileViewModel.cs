using System.ComponentModel.DataAnnotations;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

public class EditProfileViewModel
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    [StringLength(25)]
    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }

    [DataType(DataType.Password)]
    [MinLength(6)]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Yeni şifre tekrarı eşleşmiyor.")]
    public string? ConfirmNewPassword { get; set; }
}
