using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Worki.Infrastructure;

namespace Worki.Data.Models
{
    public interface IMapModelProvider
    {
        MapModel GetMapModel();
    }

    public class MapModel
    {
        #region Properties

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; }

        #endregion
    }
}
