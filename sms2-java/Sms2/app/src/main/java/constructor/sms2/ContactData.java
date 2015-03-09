package constructor.sms2;

/**
 * Created by igor-z on 3/9/2015.
 */
public class ContactData {
    String _displayedName;
    public String getDisplayedName()
    {
        return _displayedName;
    }
    String _phoneNumber;
    public String getPhoneNumber()
    {
        return _phoneNumber;
    }

    public boolean getEmpty()
    {
        return _displayedName.isEmpty() && _phoneNumber.isEmpty();
    }

    public ContactData(String _displayedName, String _phoneNumber)
    {
        this._displayedName = _displayedName;
        this._phoneNumber = _phoneNumber;
    }
}
