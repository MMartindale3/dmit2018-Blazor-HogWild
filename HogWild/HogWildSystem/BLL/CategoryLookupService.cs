using HogWildSystem.DAL;
using HogWildSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HogWildSystem.BLL
{
    public class CategoryLookupService
    {
        private readonly HogWildContext _hogWildContext;

        internal CategoryLookupService(HogWildContext hogWildContext)
        {
            _hogWildContext = hogWildContext;
        }

        public List<LookupView> GetLookups(string categoryName) 
        {
            return _hogWildContext.Lookups
                    .Where(lookup => lookup.Category.CategoryName == categoryName)
                    .OrderBy(lookup => lookup.Name)
                    .Select(lookup => new LookupView
                    {
                        LookupID = lookup.LookupID,
                        CategoryID = lookup.CategoryID,
                        Name = lookup.Name,
                        RemoveFromViewFlag = lookup.RemoveFromViewFlag
                    })
                    .ToList();
        }
    }
}
