namespace SimpleSite.Models
{
    public class Semester
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SemesterNumber { get; set; }
        public decimal? Grade { get; set; }
        public Student Student { get; set; }
    }
}