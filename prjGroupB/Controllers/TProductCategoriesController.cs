using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjGroupB.DTO;
using prjGroupB.Models;

namespace prjGroupB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TProductCategoriesController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TProductCategoriesController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TProductCategories
        [HttpGet]
        public async Task<IEnumerable<TProductCategoryDTO>?> GetTProductCategories()
        {
            try
            {
                return await _context.TProductCategories.Select(c => new TProductCategoryDTO
                {
                    FCategoryName = c.FCategoryName,
                    FProductCategoryId = c.FProductCategoryId,
                    ProductCount = _context.TProducts
                        .Count(p => p.FProductCategoryId == c.FProductCategoryId && p.FIsOnSales == true)
                }).ToListAsync();

            }catch(Exception ex)
            {
                Console.WriteLine($"獲取商品類別失敗: {ex.Message}");
                return null;
            }
        }        
    }
}
