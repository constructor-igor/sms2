﻿using System;

namespace sms2
{
	public class ContactData
	{
		public string DisplayedName { get; private set; }
		public string PhoneNumber { get; private set; }
		public bool Empty
		{
			get 
			{
				return String.IsNullOrWhiteSpace (DisplayedName) && String.IsNullOrEmpty (PhoneNumber);
			}
		}
		public ContactData (string displayedName, string phoneNumber)
		{
			DisplayedName = displayedName;
			PhoneNumber = phoneNumber;
		}
	}

	public class ContactDataFormatter
	{
		const string DEFAULT_CONTACT_TEXT = "<Select contact>";
		public static string Format(ContactData contactData)
		{
			if (contactData == null || contactData.Empty)
				return DEFAULT_CONTACT_TEXT;
			return String.Format ("{0}: {1}", contactData.DisplayedName, contactData.PhoneNumber);
		}
		public static bool IsContactActual(string contactText)
		{
			return (contactText != DEFAULT_CONTACT_TEXT) && !String.IsNullOrWhiteSpace (contactText);
		}
	}
}