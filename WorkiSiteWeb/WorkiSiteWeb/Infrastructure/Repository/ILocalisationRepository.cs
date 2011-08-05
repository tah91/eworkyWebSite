using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Worki.Web.Models;
using Worki.Web.Infrastructure.Logging;

namespace Worki.Web.Infrastructure.Repository
{
    public interface ILocalisationRepository : IRepository<Localisation>
    {
        IList<Localisation> FindByLocation(float latitude, float longitude);
        IList<Localisation> FindSimilarLocalisation(float latitude, float longitude);
        IList<Localisation> FindByCriteria(SearchCriteria criteria);
        void GeoCode(ILogger logger, string address, out float lat, out float lg);
        float DistanceBetween(float latitude, float longitude, int localisationId);
		Comment GetComment(int comId);
    }
}