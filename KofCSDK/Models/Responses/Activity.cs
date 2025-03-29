namespace KofCSDK.Models.Responses
{
    public class Activity
    {
        public Guid ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string ActivityDescription { get; set; } = string.Empty;
        public ActivityCategory ActivityCategory { get; set; }
        public List<Guid> ActivityCoordinators { get; set; } = new List<Guid>();
        public List<ActivityEventNotes> ActivityEventNotes { get; set; } = new List<ActivityEventNotes>();
        public string Notes { get; set; } = string.Empty;
    }
}
