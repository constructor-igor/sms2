using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Telephony;
using Android.Provider;
using Android.Util;

namespace sms2
{
	public class SettingsData
	{
		public ContactData ContactData { get; private set; }
		public MessageItem Message { get; private set; }

		public SettingsData (ContactData contactData, MessageItem message)
		{
			ContactData = contactData;
			Message = message;
		}
	}

	public class SettingsLoader
	{
		const int ACTUAL_FORMAT_ID = 1;
		static public void Save(SettingsData settingsData)
		{
			var prefs = Application.Context.GetSharedPreferences ("sms2", Android.Content.FileCreationMode.Private);
			var prefEditor = prefs.Edit ();
			prefEditor.PutInt ("version", ACTUAL_FORMAT_ID);
			prefEditor.PutString ("name", settingsData.ContactData.DisplayedName);
			prefEditor.PutString ("number", settingsData.ContactData.PhoneNumber);
			prefEditor.PutString ("message", settingsData.Message.Message);
			prefEditor.Commit ();
		}
		static public SettingsData Load()
		{
			var prefs = Application.Context.ApplicationContext.GetSharedPreferences ("sms2", Android.Content.FileCreationMode.Private);
			int version = prefs.GetInt ("version", 0);
			switch (version) {
				case 1:
					string displayedName = prefs.GetString ("name", "");
					string phoneNumber = prefs.GetString ("number", "");
					string message = prefs.GetString ("message", "");
					return new SettingsData (new ContactData (displayedName, phoneNumber), new MessageItem (message));
				default:
					return new SettingsData (new ContactData (null, null), new MessageItem (null));
			}
		}
	}

	[Activity (Label = "sms2", MainLauncher = true)]
	public class MainActivity : Activity
	{
		const int SELECT_CONTACT_SUCCESS_RESULT = 101;
		string[] definedMessages = new string[] {"Could you call me?", "I am here", "I wait you", "How are you?", "Yo!"};
		Button[] definedButtons = new Button[5];

		ContactData contactData = null;
		MessageItem messageItem = null;

		Button selectContactButton;
		EditText messageEditText;
		Button sendSmsMessageButton;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
					
			SettingsData settingsData = SettingsLoader.Load ();
			contactData = settingsData.ContactData;
			messageItem = settingsData.Message;

			// Get our button from the layout resource,
			// and attach an event to it
			selectContactButton = FindViewById<Button> (Resource.Id.selectContactButton);
			selectContactButton.Text = ContactDataFormatter.Format (contactData);

			messageEditText = FindViewById<EditText> (Resource.Id.message1EditText);
			messageEditText.Text = MessageItemFormatter.Format (messageItem);

			sendSmsMessageButton = FindViewById<Button> (Resource.Id.sendSmsButton);
			sendSmsMessageButton.Text = "Send SMS";

			definedButtons[0] = FindViewById<Button> (Resource.Id.send1Button);
			definedButtons[1] = FindViewById<Button> (Resource.Id.send2Button);
			definedButtons[2] = FindViewById<Button> (Resource.Id.send3Button);
			definedButtons[3] = FindViewById<Button> (Resource.Id.send4Button);
			definedButtons[4] = FindViewById<Button> (Resource.Id.send5Button);

			for (int i = 0; i < definedMessages.Length; i++) {
				definedButtons[i].Text = String.Format ("Send '{0}'", definedMessages [i]);
			}

			selectContactButton.Click += delegate {
				//Create a new intent for choosing a contact
				var contactPickerIntent = new Intent (Intent.ActionPick, Android.Provider.ContactsContract.Contacts.ContentUri);
				contactPickerIntent.SetType (ContactsContract.CommonDataKinds.Phone.ContentType); //(Phone.CONTENT_TYPE); // Show user only contacts w/ phone numbers
				// Start the contact picker expecting a result with the resultCode '101'  
				StartActivityForResult (contactPickerIntent, SELECT_CONTACT_SUCCESS_RESULT);
			};

			sendSmsMessageButton.Click += delegate(object sender, EventArgs e) {
				if (contactData != null && !contactData.Empty)
					SmsManager.Default.SendTextMessage(contactData.PhoneNumber, null, messageEditText.Text, null, null);
			};

			for (int i = 0; i < definedButtons.Length; i++) 
			{
				string message = definedMessages [i];
				definedButtons [i].Click += delegate(object sender, EventArgs e) {
					if (contactData != null && !contactData.Empty)
						SmsManager.Default.SendTextMessage (contactData.PhoneNumber, null, message, null, null);
				};
			};

//			messageEditText.AfterTextChanged += delegate(object sender, Android.Text.AfterTextChangedEventArgs e) {
//				messageItem = new MessageItem(e.ToString());
//			};
			//message1EditText.Click
			//message1EditText.LongClick
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			Log.Debug ("OnActivityResult", string.Format("requestCode = {0}, resultCode = {1}", requestCode, resultCode));

			base.OnActivityResult(requestCode, resultCode, data);

			switch (requestCode) {
				case SELECT_CONTACT_SUCCESS_RESULT:
					SelectContactHandling (resultCode, data);
				break;
			}
		}

		protected override void OnDestroy()
		{
			messageItem = new MessageItem (messageEditText.Text);
			SettingsLoader.Save(new SettingsData(contactData, messageItem));
			base.OnDestroy ();
		}

		void SelectContactHandling(Result resultCode, Intent data)
		{
			if (resultCode == Result.Ok && data!=null && data.Data!=null) {
				var id = data.Data.LastPathSegment;
				var contacts = ManagedQuery(ContactsContract.CommonDataKinds.Phone.ContentUri, null, "_id = ?", new string[] { id }, null);
				contacts.MoveToFirst();

				int displayedNameIndexNumber = contacts.GetColumnIndex ("display_name");
				string displayName = contacts.GetString(displayedNameIndexNumber);

				int phoneNumberIndexNumber = contacts.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number);			
				string phoneNumber = contacts.GetString(phoneNumberIndexNumber);

				contactData = new ContactData (displayName, phoneNumber);

				selectContactButton.Text = ContactDataFormatter.Format (contactData);
			}
		}
	}
}


