using HogWildSystem.DAL;
using HogWildSystem.Entities;
using HogWildSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HogWildSystem.BLL
{
    public class WorkingVersionsService
    {
        private readonly HogWildContext _hogWildContext;

        internal WorkingVersionsService(HogWildContext hogWildContext)
        {
            _hogWildContext = hogWildContext;
        }

        public WorkingVersionsView GetWorkingVersion()
        {
            return _hogWildContext.WorkingVersions
                    .Select(version => new WorkingVersionsView
                    {
                        VersionId = version.VersionId,
                        Major = version.Major,
                        Minor = version.Minor,
                        Build = version.Build,
                        Revision = version.Revision,
                        AsOfDate = version.AsOfDate,
                        Comments = version.Comments
                    }).FirstOrDefault();
        }
    }
}
