using System;

namespace sms2
{
	public class MessageItem
	{
		public string Message { get; private set; }
		public MessageItem (string message)
		{
			Message = message;
		}
	}

	public class MessageItemFormatter
	{
		public static string Format(MessageItem message)
		{
			if (message == null)
				return "<enter message>";
			return message.Message;
		}
	}
}

