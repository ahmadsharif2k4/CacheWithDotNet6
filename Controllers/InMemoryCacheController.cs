using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using RedisIntegration.Data;

namespace RedisIntegration.Controllers
{
    //Caching refers to the process of storing frequently used data,
    //so that those data can be served much faster for any future requests.
    //So we take the most frequently used data and copy it into temporary storage,
    //so that it can be accessed much faster in future calls from the client. 
    //Caching significantly improves the performance of an application, reducing the complexity to generate content. 
    //The application should only cache data that don't change frequently and use the cache data only if it is available.

    //ASP.NET Core has many caching features. But among them the two main types are,
    // 1- In-memory caching
    // 2- Distributed Caching

    //An in-memory cache is stored in the memory of a single server hosting the application.
    //Basically, the data is cached within the application. 
    //The main advantage of In-memory caching is it is much quicker than distributed caching
    //because it avoids communicating over a network and it's suitable for small-scale applications.
    //And the main disadvantage is maintaining the consistency of caches while deployed in the cloud.



    [Route("api/[controller]")]
    [ApiController]
    public class InMemoryCacheController : ControllerBase
    {
        private readonly IMemoryCache _inMemoryCache;
        private readonly OmsContext _context;

        public InMemoryCacheController(IMemoryCache memoryCache, OmsContext context)
        {
            _inMemoryCache = memoryCache;
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllOrderItems()
        {
            var cacheKey = "orderItems";

            if (!_inMemoryCache.TryGetValue(cacheKey, out List<OrderItem> orderItems))
            {
                //calling the db context because items not found in cache
                orderItems = await _context.OrderItems.ToListAsync();

                //setting up in cache
                var cacheExpiryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddSeconds(50),
                    Priority = CacheItemPriority.High,
                    SlidingExpiration = TimeSpan.FromSeconds(20)
                };
                _inMemoryCache.Set(cacheKey, orderItems);
            }

            return Ok(orderItems);
        }
    }
}
