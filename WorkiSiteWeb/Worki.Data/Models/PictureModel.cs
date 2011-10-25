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
        /// Internal id of the provider
        /// </summary>
        /// <returns>the id</returns>
        int GetId();

		/// <summary>
		/// Internal type of the provider
		/// </summary>
		/// <returns>the type</returns>
		ProviderType GetProviderType();

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
        Rental,
		Offer
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

		const string _RentalFolder = "rental";
		const string _OfferFolder = "offer";

		public static string GetFolder(ProviderType type)
		{
			switch (type)
			{
				case ProviderType.Rental:
					return _RentalFolder;
				case ProviderType.Offer:
					return _OfferFolder;
				case ProviderType.Localisation:
				default:
					return null;
			}
		}
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
}
