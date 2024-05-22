namespace SubReddit.DataModel.Model
{
    public class Posts
    {
        public DateTime CurrentDate { get; set; }
        public int ups { get; set; }
        public string author { get; set; } = string.Empty;
        public string author_fullname { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string subreddit { get; set; } = string.Empty;   
        public DateTime createdDt { get; set; }
    }
}
