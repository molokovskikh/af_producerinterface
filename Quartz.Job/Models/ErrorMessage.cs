namespace Quartz.Job.Models
{
	public class ErrorMessage
	{
		public string PropertyName { get; set; }

		public string Message { get; set; }

		public ErrorMessage(string propertyName, string message)
		{
			PropertyName = propertyName;
			Message = message;
		}
	}
}
