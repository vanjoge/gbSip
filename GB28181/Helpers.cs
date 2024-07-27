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
        /// <summary>
        /// 获取ID类型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetIdType(this string str)
        {
            if (str.Length == 20)
            {
                return str[10..13];
            }
            return null;
        }
    }
}
