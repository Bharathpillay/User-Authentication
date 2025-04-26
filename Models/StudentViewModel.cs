namespace SimpleSite.Models
{
    using System.ComponentModel.DataAnnotations;

    public class StudentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public int DepartmentId { get; set; }

        [Range(0, 10, ErrorMessage = "CGPA must be between 0 and 10.")]
        public decimal? CGPA { get; set; }
    }
}
