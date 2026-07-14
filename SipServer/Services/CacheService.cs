using System;
using System.Threading.Tasks;

namespace SipServer.Services
{
    /// <summary>
    /// 缓存服务，提供通用的缓存能力
    /// </summary>
    public class CacheService
    {
        private SipServer sipServer;

        public CacheService(SipServer sipServer)
        {
            this.sipServer = sipServer;
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expiry">过期时间（分钟），默认30分钟</param>
        /// <returns>是否设置成功</returns>
        public async Task<bool> SetAsync(string key, string value, int expiry = 30)
        {
            return await sipServer.DB.RedisHelper.StringSetAsync(key, value, TimeSpan.FromMinutes(expiry));
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值，如果不存在返回null</returns>
        public async Task<string> GetAsync(string key)
        {
            return await sipServer.DB.RedisHelper.StringGetAsync(key);
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>是否删除成功</returns>
        public async Task<bool> DeleteAsync(string key)
        {
            return await sipServer.DB.RedisHelper.GetDatabase().KeyDeleteAsync(key);
        }

        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>是否存在</returns>
        public async Task<bool> ExistsAsync(string key)
        {
            return await sipServer.DB.RedisHelper.GetDatabase().KeyExistsAsync(key);
        }

        /// <summary>
        /// 设置缓存过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>是否设置成功</returns>
        public async Task<bool> ExpireAsync(string key, TimeSpan expiry)
        {
            return await sipServer.DB.RedisHelper.GetDatabase().KeyExpireAsync(key, expiry);
        }

        /// <summary>
        /// 获取缓存剩余过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>剩余时间，如果不存在返回null</returns>
        public async Task<TimeSpan?> GetTtlAsync(string key)
        {
            return await sipServer.DB.RedisHelper.GetDatabase().KeyTimeToLiveAsync(key);
        }

        /// <summary>
        /// 自增操作
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">增量，默认1</param>
        /// <returns>自增后的值</returns>
        public async Task<long> IncrementAsync(string key, long value = 1)
        {
            return await sipServer.DB.RedisHelper.GetDatabase().StringIncrementAsync(key, value);
        }

        /// <summary>
        /// 自减操作
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">减量，默认1</param>
        /// <returns>自减后的值</returns>
        public async Task<long> DecrementAsync(string key, long value = 1)
        {
            return await sipServer.DB.RedisHelper.GetDatabase().StringDecrementAsync(key, value);
        }
    }
}