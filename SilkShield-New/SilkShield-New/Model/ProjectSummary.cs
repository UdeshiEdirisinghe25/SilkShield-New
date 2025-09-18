namespace SilkShield_New.Model
{
    public class ProjectSummary
    {
        public int Upcoming { get; set; }
        public int Ongoing { get; set; }
        public int Pending { get; set; } // Completed වෙනුවට Pending ලෙස වෙනස් කරන්න
    }
}