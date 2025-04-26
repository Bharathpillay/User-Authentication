namespace SimpleSite.Models
{
    public class StaffSubject
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
    }
}