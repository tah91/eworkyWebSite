using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Worki.Infrastructure;

namespace Worki.Data.Models
{
    public class MetaData
    {
        #region Properties

        public string Title { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }

        #endregion
    }
}
