using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Worki.Infrastructure;
using System.Collections.Generic;

namespace Worki.Data.Models
{
    public class AlternateUrl
    {
        public string Lang { get; set; }
        public string Url { get; set; }
    }
    public class MetaData
    {
        public MetaData()
        {
            AlternateUrls = new List<AlternateUrl>();
        }

        #region Properties

        public string Title { get; set; }
        public string CanonicalUrl { get; set; }
        public IList<AlternateUrl> AlternateUrls { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }

        #endregion
    }
}
