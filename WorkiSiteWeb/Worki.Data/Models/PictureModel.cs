using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Worki.Infrastructure;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace Worki.Data.Models
{
    public interface IPictureDataProvider
    {
        List<PictureData> GetPictureData();

        /// <summary>
        /// Get filename of default image
        /// </summary>
        /// <returns>the filename</returns>
        string GetMainPic();

        /// <summary>
        /// get filename of image at an index
        /// </summary>
        /// <param name="index">the index</param>
        /// <returns>the filename</returns>
        string GetPic(int index);

        /// <summary>
        /// Get filename of logo
        /// </summary>
        /// <returns>the filename</returns>
        string GetLogoPic();

        /// <summary>
        /// Get displayName of the instance
        /// </summary>
        /// <returns>the displayName</returns>
        string GetDisplayName();

        /// <summary>
        /// Get description of the instance
        /// </summary>
        /// <returns>the description</returns>
        string GetDescription();
    }

    public enum ProviderType
    {
        Localisation,
        Rental
    }

    public class PictureData
    {
        //prefixs for localisation gallery form
        public const string HiddenImagePrefix = "HiddenImg_";
        public const string IsDefaultPrefix = "IsDefault_";
        public const string IsLogoPrefix = "IsLogo_";

        public const string PictureDataString = "PictureData";

        public string FileName { get; set; }
        public bool IsDefault { get; set; }
        public bool IsLogo { get; set; }
    }

    [DataContract]
    public class PictureDataContainer
    {
        public PictureDataContainer(IPictureDataProvider provider)
        {
            Files = provider.GetPictureData();
        }

        [DataMember]
        public List<PictureData> Files { get; set; }
    }

    public class ImageJson
    {
        const string DeleteType = "POST";

        public ImageJson()
        {
            delete_type = DeleteType;
        }

        public string name { get; set; }
        public int size { get; set; }
        public string url { get; set; }
        public string thumbnail_url { get; set; }
        public string delete_url { get; set; }
        public string delete_type { get; set; }
        public string is_default { get; set; }
        public string is_logo { get; set; }
    }
}
