package constructor.sms2;

public class ContactDataFormatter {
    final static String DEFAULT_CONTACT_TEXT = "<Select contact>";
    public static String format(ContactData contactData)
    {
        if (contactData == null || contactData.getEmpty())
            return DEFAULT_CONTACT_TEXT;
        return String.format ("%s: %s", contactData.getDisplayedName(), contactData.getPhoneNumber());
    }

    public static boolean isContactActual(String contactText)
    {
        return (contactText != DEFAULT_CONTACT_TEXT) && !contactText.isEmpty();
    }
}
