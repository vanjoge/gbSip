using System;

namespace GB28181
{
    public static class Helpers
    {

        /// <summary>  
        /// 非法字符转换  
        /// </summary>  
        /// <param name="str">携带(特殊字符)字符串</param>  
        /// <returns></returns>  
        public static string SafeReplace(this string str)
        {
            char[] codes = { ',', '\'', ';', ':', '/', '?', '<', '>', '.', '#', '%','&','?',
                             '^', '\\', '@', '*', '~', '`', '$', '{', '}', '[', ']' ,'"'};
            for (int i = 0; i < codes.Length; i++)
            {
                str = str.Replace(codes[i], char.MinValue);
            }

            return str;
        }
    }
}
