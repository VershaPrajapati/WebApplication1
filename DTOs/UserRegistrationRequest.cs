using System.ComponentModel.DataAnnotations;

public class UserRegistrationRequest
{
    [Required]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string FullName { get; set; }

    public int Age { get; set; }

    public string Gender { get; set; }
}
