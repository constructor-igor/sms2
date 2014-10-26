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

		Button selectContactButton;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			selectContactButton = FindViewById<Button> (Resource.Id.selectContactButton);
			selectContactButton.Text = "<Select contact>";

			selectContactButton.Click += delegate {
				//Create a new intent for choosing a contact
				var contactPickerIntent = new Intent (Intent.ActionPick, Android.Provider.ContactsContract.Contacts.ContentUri);
				contactPickerIntent.SetType (ContactsContract.CommonDataKinds.Phone.ContentType); //(Phone.CONTENT_TYPE); // Show user only contacts w/ phone numbers
				// Start the contact picker expecting a result with the resultCode '101'  
				StartActivityForResult (contactPickerIntent, SELECT_CONTACT_SUCCESS_RESULT);
			};
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
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

				string displayName = contacts.GetString(contacts.GetColumnIndex("display_name"));

				//ContactsContract.CommonDataKinds.StructuredName.DisplayName

				int indexNumber = contacts.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number);
				string phoneNumber = contacts.GetString(indexNumber);

				contactData = new ContactData (displayName, phoneNumber);

				selectContactButton.Text = String.Format ("{0}: {1}", contactData.DisplayedName, contactData.PhoneNumber);
			}
		}
	}
}


