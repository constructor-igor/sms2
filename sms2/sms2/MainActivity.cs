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
	[Activity (Label = "sms2", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		const int SELECT_CONTACT_SUCCESS_RESULT = 101;

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

			// Get our button from the layout resource,
			// and attach an event to it
			selectContactButton = FindViewById<Button> (Resource.Id.selectContactButton);
			selectContactButton.Text = ContactDataFormatter.Format (contactData);

			messageEditText = FindViewById<EditText> (Resource.Id.message1EditText);
			messageEditText.Text = MessageItemFormatter.Format (messageItem);

			sendSmsMessageButton = FindViewById<Button> (Resource.Id.sendSmsButton);
			sendSmsMessageButton.Text = "Send SMS";

			selectContactButton.Click += delegate {
				//Create a new intent for choosing a contact
				var contactPickerIntent = new Intent (Intent.ActionPick, Android.Provider.ContactsContract.Contacts.ContentUri);
				contactPickerIntent.SetType (ContactsContract.CommonDataKinds.Phone.ContentType); //(Phone.CONTENT_TYPE); // Show user only contacts w/ phone numbers
				// Start the contact picker expecting a result with the resultCode '101'  
				StartActivityForResult (contactPickerIntent, SELECT_CONTACT_SUCCESS_RESULT);
			};

			sendSmsMessageButton.Click += delegate(object sender, EventArgs e) {
				if (contactData!=null)
					SmsManager.Default.SendTextMessage(contactData.PhoneNumber, null, messageEditText.Text, null, null);
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


