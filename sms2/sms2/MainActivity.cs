using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Telephony;
using Android.Provider;
using Android.Util;
using Android.Locations;

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
	public class MainActivity : Activity, ILocationListener
	{
		const string SHORT_URL_TO_GOOGLE_PLAY_SMS2 = "http://goo.gl/9hAuWt";
		const int SELECT_CONTACT_SUCCESS_RESULT = 101;
		string[] definedMessages = new string[] {"Could you call me?", "I am here @map", "I wait you", "How are you?", "Yo!"};
		Button[] definedButtons = new Button[5];

		ContactData contactData = null;
		MessageItem messageItem = null;

		private BroadcastReceiver m_smsSentBroadcastReceiver;
		private BroadcastReceiver m_smsDeliveredBroadcastReceiver;

		private Location _currentLocation;
		private LocationManager _locationManager;
		private String _locationProvider;

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
			// http://stackoverflow.com/questions/6217378/place-cursor-at-the-end-of-text-in-edittext
			messageEditText.SetSelection (messageEditText.Text.Length);

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

			var piSent = PendingIntent.GetBroadcast(this, 0, new Intent("SMS_SENT"), 0);
			var piDelivered = PendingIntent.GetBroadcast(this, 0, new Intent("SMS_DELIVERED"), 0);

			sendSmsMessageButton.Click += delegate(object sender, EventArgs e) {
				var messageText = messageEditText.Text;
				if (contactData != null && !contactData.Empty && !String.IsNullOrEmpty(messageText)) {
					messageText = Processing(messageText);
					SmsManager.Default.SendTextMessage(contactData.PhoneNumber, null, messageText, piSent, piDelivered);
					AddSentSmsToHistory(contactData.PhoneNumber, messageText);
				}
			};

			for (int i = 0; i < definedButtons.Length; i++) 
			{
				string messageText = definedMessages [i];
				definedButtons [i].Click += delegate(object sender, EventArgs e) {
					if (contactData != null && !contactData.Empty && !String.IsNullOrEmpty(messageText)) {
						messageText = Processing(messageText);
						SmsManager.Default.SendTextMessage (contactData.PhoneNumber, null, messageText, piSent, piDelivered);
						AddSentSmsToHistory(contactData.PhoneNumber, messageText);
					}
				};
			};

			InitializeLocationManager();
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

		protected override void OnResume()
		{
			Log.Debug ("OnResume", "");
			base.OnResume ();

			m_smsSentBroadcastReceiver = new SMSSentReceiver();
			m_smsDeliveredBroadcastReceiver = new SMSDeliveredReceiver();

			RegisterReceiver(m_smsSentBroadcastReceiver, new IntentFilter("SMS_SENT"));
			RegisterReceiver(m_smsDeliveredBroadcastReceiver, new IntentFilter("SMS_DELIVERED"));

			_locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
		}
		protected override void OnPause()
		{
			Log.Debug ("OnPause", "");
			base.OnPause ();

			messageItem = new MessageItem (messageEditText.Text);
			SettingsLoader.Save(new SettingsData(contactData, messageItem));

			UnregisterReceiver(m_smsSentBroadcastReceiver);
			UnregisterReceiver(m_smsDeliveredBroadcastReceiver);

			_locationManager.RemoveUpdates(this);
		}

		protected override void OnDestroy()
		{
			messageItem = new MessageItem (messageEditText.Text);
			SettingsLoader.Save(new SettingsData(contactData, messageItem));
			base.OnDestroy ();
		}

		#region ILocationListener
		public void OnLocationChanged(Location location) 
		{
			_currentLocation = location;
		}
		public void OnProviderDisabled(string provider) {}
		public void OnProviderEnabled(string provider) {}
		public void OnStatusChanged(string provider, Availability status, Bundle extras) {}
		#endregion

		string Processing(string messageText)
		{
			if (_currentLocation == null)
				return messageText;
			string linkToGoogleMap = String.Format ("https://www.google.co.il/maps/place/{0}N{1}E", _currentLocation.Latitude, _currentLocation.Longitude);

			if (messageText == "@map")
				return linkToGoogleMap;
			if (messageText.StartsWith ("@map ")) {
				return messageText.Replace ("@map ", linkToGoogleMap + " ");
			}
			if (messageText.EndsWith(" @map")) {
				return messageText.Replace (" @map", " " + linkToGoogleMap);
			}
			if (messageText.IndexOf(" @map ")!=-1) {
				return messageText.Replace (" @map ", " " + linkToGoogleMap + " ");
			}
			return messageText;
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

		/*
		 * Add sms to history requires 2 permissions:
		 * <uses-permission android:name="android.permission.WRITE_SMS" />
	     * <uses-permission android:name="android.permission.READ_SMS" />
		 * 
		 */
		private void AddSentSmsToHistory(String address, String message) 
		{
			try {
				ContentValues values = new ContentValues();
				values.Put("address", address);
				values.Put("body", message);
				//values.Put("body", String.Format("{0} (sent by sms2, {1})", message, SHORT_URL_TO_GOOGLE_PLAY_SMS2));
				//values.Put("date", DateTime.Now.Ticks);
				//long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
				var totalPeriod = DateTime.UtcNow - new DateTime(1970, 1, 1);
				long milliseconds = (long)totalPeriod.TotalMilliseconds;
				values.Put("date", milliseconds);
				//getContentResolver.Insert(Android.Net.Uri.Parse("content://sms/sent"), values);
				ContentResolver.Insert(Android.Net.Uri.Parse("content://sms/sent"), values);
			}
			catch (Exception e) {
				//e.printStackTrace();
				Toast.MakeText(Application.Context, String.Format("SMS cannot be stored, becasue '{0}'", e.Message), ToastLength.Long).Show();
			}
		}

		void InitializeLocationManager()
		{
			_locationManager = (LocationManager)GetSystemService(LocationService);
			Criteria criteriaForLocationService = new Criteria
			{
				Accuracy = Accuracy.Fine
			};
			IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

			if (acceptableLocationProviders.Any())
			{
				_locationProvider = acceptableLocationProviders.First();
			}
			else
			{
				_locationProvider = String.Empty;
			}
		}
	}

	[BroadcastReceiver(Exported = true, Permission = "//receiver/@android:android.permission.SEND_SMS")]
	public class SMSSentReceiver : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			switch ((int)ResultCode)
			{
			case (int)Result.Ok:
				Toast.MakeText(Application.Context, "SMS has been sent", ToastLength.Short).Show();
				break;
			case (int)SmsResultError.GenericFailure:
				Toast.MakeText(Application.Context, "Generic Failure", ToastLength.Short).Show();
				break;
			case (int)SmsResultError.NoService:
				Toast.MakeText(Application.Context, "No Service", ToastLength.Short).Show();
				break;
			case (int)SmsResultError.NullPdu:
				Toast.MakeText(Application.Context, "Null PDU", ToastLength.Short).Show();
				break;
			case (int)SmsResultError.RadioOff:
				Toast.MakeText(Application.Context, "Radio Off", ToastLength.Short).Show();
				break;
			}
		}
	}
	[BroadcastReceiver(Exported = true, Permission = "//receiver/@android:android.permission.SEND_SMS")]
	public class SMSDeliveredReceiver : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			switch ((int)ResultCode) {
			case (int)Result.Ok:
				Toast.MakeText (Application.Context, "SMS Delivered", ToastLength.Short).Show ();
				break;
			case (int)Result.Canceled:
				Toast.MakeText (Application.Context, "SMS not delivered", ToastLength.Short).Show ();
				break;
			}
		}
	}
}
