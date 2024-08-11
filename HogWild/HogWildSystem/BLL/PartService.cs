using HogWildSystem.DAL;
using HogWildSystem.Entities;
using HogWildSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable
namespace HogWildSystem.BLL
{
    public class PartService
    {
        private readonly HogWildContext _hogWildContext;

        internal PartService(HogWildContext hogWildContext)
        {
            _hogWildContext = hogWildContext;
        }

        public PartView GetPart(int partID)
        {
            if (partID <= 0)
            {
                throw new ArgumentNullException("Provide a valid part ID!");
            }

            return _hogWildContext.Parts
                    .Where(part => part.PartID == partID && !part.RemoveFromViewFlag)
                    .Select(part => new PartView
                    {
                        PartID = part.PartID,
                        PartCategoryID = part.PartCategoryID,
                        CategoryName = _hogWildContext.Lookups
                                        .Where(lookup => lookup.LookupID == part.PartCategoryID)
                                        .Select(lookup => lookup.Name)
                                        .FirstOrDefault(),
                        Description = part.Description,
                        Cost = part.Cost,
                        Price = part.Price,
                        ROL = part.ROL,
                        QOH = part.QOH,
                        Taxable = part.Taxable,
                        RemoveFromViewFlag = part.RemoveFromViewFlag
                    })
                    .FirstOrDefault();
        }


        public List<PartView> GetParts(int partCategoryID, string description, List<int> existingPartIDs)
        {
            if (partCategoryID <= 0 && string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException("Please provide either a valid category ID and/or a description!");
            }

            Guid tempGuid = new Guid();
            if (string.IsNullOrWhiteSpace(description))
            {
                description = tempGuid.ToString();
            }

            return _hogWildContext.Parts
                    .Where(part => !existingPartIDs.Contains(part.PartID)
                            && (description.Length > 0 && description != tempGuid.ToString() && partCategoryID > 0 ?
                                (part.Description.Contains(description) && part.PartCategoryID == partCategoryID)
                            : (part.Description.Contains(description) || part.PartCategoryID == partCategoryID)
                            && !part.RemoveFromViewFlag))
                    .Select(part => new PartView
                    {
                        PartID = part.PartID,
                        PartCategoryID = part.PartCategoryID,
                        CategoryName = part.PartCategory.Name,
                        Description = part.Description,
                        Cost = part.Cost,
                        Price = part.Price,
                        ROL = part.ROL,
                        QOH = part.QOH,
                        Taxable = part.Taxable,
                        RemoveFromViewFlag = part.RemoveFromViewFlag
                    })
                    .OrderBy(part => part.Description)
                    .ToList();
        }
    }
}
