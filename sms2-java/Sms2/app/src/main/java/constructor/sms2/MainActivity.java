package constructor.sms2;

import android.app.PendingIntent;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.Bundle;
import android.provider.ContactsContract;
import android.support.v7.app.ActionBarActivity;
import android.telephony.SmsManager;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;

public class MainActivity extends ActionBarActivity {
    static final int SELECT_CONTACT_SUCCESS_RESULT = 101;
    Button selectContactButton;
    Button sendSmsMessageButton;
    EditText customMessageEditText;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        selectContactButton = (Button) findViewById(R.id.selectContactButton);
        sendSmsMessageButton = (Button) findViewById(R.id.sendSmsButton);
        customMessageEditText = (EditText) findViewById(R.id.messageEditText);

        selectContactButton.setOnClickListener(new View.OnClickListener() {
            public void onClick(View v) {
                // Perform action on click

                // Create a new intent for choosing a contact
                // http://stackoverflow.com/questions/9496350/pick-a-number-and-name-from-contacts-list-in-android-app
                Intent contactPickerIntent = new Intent (Intent.ACTION_PICK, ContactsContract.Contacts.CONTENT_URI);
                contactPickerIntent.setType(ContactsContract.CommonDataKinds.Phone.CONTENT_TYPE); //(Phone.CONTENT_TYPE); // Show user only contacts w/ phone numbers
                // Start the contact picker expecting a result with the resultCode '101'
                //StartActivityForResult (contactPickerIntent, SELECT_CONTACT_SUCCESS_RESULT);
                startActivityForResult(contactPickerIntent, SELECT_CONTACT_SUCCESS_RESULT);
            }
        });
//        selectContactButton.setOnEditorActionListener(new Button.OnEditorActionListener() {
//            @Override
//            public boolean onEditorAction(TextView v, int actionId, KeyEvent event) {
//            }
//        });
        TextWatcher textWatcher = new TextWatcher() {
            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                //after text changed
            }

            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            }

            @Override
            public void afterTextChanged(Editable s) {
                updateButtonsStatus();
            }
        };
        selectContactButton.addTextChangedListener(textWatcher);

        final PendingIntent piSent = PendingIntent.getBroadcast(this, 0, new Intent("SMS_SENT"), 0);
        final PendingIntent piDelivered = PendingIntent.getBroadcast(this, 0, new Intent("SMS_DELIVERED"), 0);

        sendSmsMessageButton.setOnClickListener(new View.OnClickListener() {
            public void onClick(View v) {
                String phoneNo = "";
                String messageText = customMessageEditText.getText().toString();

                SmsManager smsManager = SmsManager.getDefault();
                smsManager.sendTextMessage(phoneNo, null, messageText, piSent, piDelivered);
            }
        });


        updateButtonsStatus();
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            return true;
        }

        return super.onOptionsItemSelected(item);
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent intent ) {

        Log.d("onActivityResult", String.format("requestCode = %d, resultCode= %d", requestCode, resultCode));

        switch (requestCode) {
            case SELECT_CONTACT_SUCCESS_RESULT:
                SelectingContactHandler(resultCode, intent);
                break;
        }
    }

    //
    // http://developer.android.com/training/basics/intents/result.html
    //
    protected void SelectingContactHandler(int resultCode, Intent data) {
        if (resultCode == RESULT_OK) {
            // Get the URI that points to the selected contact
            Uri contactUri = data.getData();
            // We only need the DISPLAY_NAME and NUMBER column, because there will be only one row in the result
            String[] projection = {ContactsContract.CommonDataKinds.Phone.DISPLAY_NAME, ContactsContract.CommonDataKinds.Phone.NUMBER};

            Cursor cursor = getContentResolver().query(contactUri, projection, null, null, null);
            cursor.moveToFirst();

            int column = cursor.getColumnIndex(ContactsContract.CommonDataKinds.Phone.DISPLAY_NAME);
            String displayName = cursor.getString(column);

            column = cursor.getColumnIndex(ContactsContract.CommonDataKinds.Phone.NUMBER);
            String phoneNumber = cursor.getString(column);

            cursor.close();

            ContactData contactData = new ContactData (displayName, phoneNumber);

            String selectContactText = ContactDataFormatter.format(contactData);

            selectContactButton.setText(selectContactText);
        }
    }

    private void updateButtonsStatus()
    {
        sendSmsMessageButton.setEnabled(ContactDataFormatter.isContactActual(selectContactButton.getText().toString()));
//        foreach (Button button in definedButtons)
//        {
//            button.Enabled = ContactDataFormatter.IsContactActual(selectContactButton.Text);
//        }
    }
}
