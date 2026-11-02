using System.Web.Script.Serialization;

namespace LinkGrab.Services
{
    public static class Json
    {
        static JavaScriptSerializer Create()
        {
            return new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        }

        public static string Write(object value)
        {
            return Create().Serialize(value);
        }

        public static T Read<T>(string text)
        {
            return Create().Deserialize<T>(text);
        }
    }
}
