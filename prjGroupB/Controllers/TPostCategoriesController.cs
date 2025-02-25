using System;
using System.Collections.Generic;
using System.Drawing.Printing;
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
    public class TPostCategoriesController : ControllerBase
    {
        private readonly dbGroupBContext _context;

        public TPostCategoriesController(dbGroupBContext context)
        {
            _context = context;
        }

        // GET: api/TPostCategories
        [HttpGet]
        public async Task<IEnumerable<TPostCategoryDTO>> GetTPostCategories()
        {
            return _context.TPostCategories
               .Select(e => new TPostCategoryDTO
               {
                   FCategoryId = e.FCategoryId,
                   FName = e.FName
               });
        }
    }
}
