using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using RedisIntegration.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RedisIntegration.Controllers
{
    //Distributed cache is a cache that can be shared by one or more applications and it is maintained as an external service
    //that is accessible to all servers. So distributed cache is external to the application.

    //The main advantage of distributed caching is that data is consistent throughout multiple servers
    //as the server is external to the application, any failure of any application will not affect the cache server.

    //Redis is an open-source(BSD licensed), in-memory data structure store,
    //used as a database cache and message broker. It is really fast key-value based database
    //and even NoSQL database as well. So Redis is a great option for implementing highly available cache.


    [Route("api/[controller]")]
    [ApiController]
    public class DistributedCacheController : ControllerBase
    {
        private readonly OmsContext _context;
        private readonly IDistributedCache _distributedCache;

        public DistributedCacheController(OmsContext context, IDistributedCache distributedCache)
        {
            _context = context;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrderItems()
        {
            var cacheKey = "orderItems";
            var cachedData = await _distributedCache.GetAsync(cacheKey);
            List<OrderItem> orderItems = new();
            string cachedDataString = default!;

            if (cachedData != null)
            {
                cachedDataString = Encoding.UTF8.GetString(cachedData);
                return Ok(JsonSerializer.Deserialize<List<OrderItem>>(cachedDataString));
            }

            orderItems = await _context.OrderItems.ToListAsync();
            cachedDataString = JsonSerializer.Serialize(orderItems);
            var dataToCache = Encoding.UTF8.GetBytes(cachedDataString);

            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            // Add the data into the cache
            await _distributedCache.SetAsync(cacheKey, dataToCache, options);

            return Ok(orderItems);
        }
    }
}
