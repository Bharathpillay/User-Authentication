namespace SimpleSite.Models
{
    public class Student
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int DepartmentId { get; set; }
        public decimal? CGPA { get; set; }
        public User User { get; set; }
        public Department Department { get; set; }
        public List<Semester> Semesters { get; set; } 
    }
}



