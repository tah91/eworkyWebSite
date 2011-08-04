using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkiSiteWeb.Models;
using WorkiSiteWeb.Infrastructure.Logging;

namespace WorkiSiteWeb.Infrastructure.Repository
{
    public interface ILocalisationRepository : IRepository<Localisation>
    {
        IList<Localisation> GetMainLocalisations();
        IList<Localisation> FindByLocation(float latitude, float longitude);
        IList<Localisation> FindSimilarLocalisation(float latitude, float longitude);
        IList<Localisation> FindByCriteria(SearchCriteria criteria);
        void GeoCode(ILogger logger, string address, out float lat, out float lg);
        float DistanceBetween(float latitude, float longitude, int localisationId);
        Localisation GetLocalisation(string name);
		Comment GetComment(int comId);
    }
}