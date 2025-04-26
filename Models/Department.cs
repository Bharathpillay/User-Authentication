using System.ComponentModel.DataAnnotations;

namespace SimpleSite.Models
{
    public class Department
    {
        public int Id { get; set; }
        [Required, StringLength(100)] public string Name { get; set; }
        public List<DepartmentSubject> DepartmentSubjects { get; set; } 
        public List<Staff> Staff { get; set; } 
        public List<Student> Students { get; set; } 
    }
}


