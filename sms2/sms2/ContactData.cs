using System;

namespace sms2
{
	public class ContactData
	{
		public string DisplayedName { get; private set; }
		public string PhoneNumber { get; private set; }
		public ContactData (string displayedName, string phoneNumber)
		{
			DisplayedName = displayedName;
			PhoneNumber = phoneNumber;
		}
	}
}

