namespace SimpleSite.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;

    public class Subject
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        public List<DepartmentSubject> DepartmentSubjects { get; set; } = new List<DepartmentSubject>();
        public List<StaffSubject> StaffSubjects { get; set; } = new List<StaffSubject>(); //Subject
    }
}
