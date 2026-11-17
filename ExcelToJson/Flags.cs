using Dignus.Collections;

namespace ExcelToJson
{
    internal class Flags
    {
        public const string NoPause = "--no-pause";

        public static bool Contains(string[] args, string flag)
        {
            foreach (var arg in args)
            {
                if (arg.Equals(flag))
                {
                    return true;
                }
            }
            return false;
        }
    }
}