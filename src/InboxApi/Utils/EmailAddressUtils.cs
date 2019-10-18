namespace InboxApi.Utils
{
    public static class EmailAddressUtils
    {
        public static (string domain, string inbox) Split(string address)
        {
            var pos = address.IndexOf('@');
            if (pos == -1)
                return ("", address);

            return (
                address.Substring(pos + 1),
                address.Substring(0, pos)
            );
        }
    }
}