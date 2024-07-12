#nullable disable
using HogWildSystem.DAL;
using HogWildSystem.BLL;
using HogWildSystem.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HogWildSystem.ViewModels;

namespace HogWildSystem.BLL
{
    public class WorkingVersionsService
    {
        #region Fields
        /// <summary>
        /// The HogWild context
        /// </summary>
        private readonly HogWildContext _hogWildContext;

        #endregion

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
