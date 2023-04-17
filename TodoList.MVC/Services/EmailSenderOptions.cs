namespace TodoList.MVC.Services
{
    public class EmailSenderOptions
    {
        public string SendGridKey { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
    }
}
