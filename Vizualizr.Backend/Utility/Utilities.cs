using System.Runtime.CompilerServices;

namespace Vizualizr.Backend.Utility
{
    public static class Utilities
    {
        private const string NameNotProvided = "[Name not provided]";

        public static void ThrowIfGreaterThan(this byte value, byte maximum, [CallerMemberName] string name = NameNotProvided)
        {
            if (value > maximum)
            {
                throw new ArgumentOutOfRangeException(name, "Cannot be greater than" + maximum);
            }
        }
        
        public static void ThrowIfOutsideRange(this byte value, byte minimum, byte maximum, [CallerMemberName] string name = NameNotProvided)
        {
            if (value < minimum || value > maximum)
            {
                throw new ArgumentOutOfRangeException(name, "Value must be between " + minimum + " and " + maximum);
            }
        }
        
        public static void ThrowIfNull(this object? obj, string formatMessage = "{0} was null!", [CallerMemberName] string name = NameNotProvided)
        {
            if (obj == null)
            {
                throw new NullReferenceException(string.Format(formatMessage, name));
            }
        }
    }
}