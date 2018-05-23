using System.Globalization;

public static class CultureParser
{
    private static CultureInfo _enUs;

    public static CultureInfo enUs
    {
        get
        {
            if (_enUs == null) _enUs = new CultureInfo("en-US");
            return _enUs;
        }
    }
}