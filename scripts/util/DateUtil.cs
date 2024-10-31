using System;

public class DateUtil
{
    public static float MSNow()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
