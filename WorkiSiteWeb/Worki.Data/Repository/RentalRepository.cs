using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Worki.Data.Models
{
    public interface IRentalRepository : IRepository<Rental>
    {
        IList<Rental> FindByCriteria(RentalSearchCriteria criteria);
    }

    public class RentalRepository : RepositoryBase<Rental>, IRentalRepository
    {
		public RentalRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}

        #region FeatureProjection

        public class FeatureProjection
        {
            public int Feature { get; set; }
        }

        public class FeatureProjectionEqualityComparer : IEqualityComparer<FeatureProjection>
        {
            #region IEqualityComparer<FeatureProjection> Members

            public bool Equals(FeatureProjection x, FeatureProjection y)
            {
                return x.Feature == y.Feature;
            }

            public int GetHashCode(FeatureProjection obj)
            {
                return base.GetHashCode();
            }

            #endregion
        }

        #endregion

        Regex _PlaceRegex = new Regex("(?<address>.+)\\((?<postalCode>[0-9]+)\\)");

        class PlaceDefinition
        {
            public string Address { get; set; }
            public string PostalCode { get; set; }

            public bool Compare(PlaceDefinition criterion)
            {
                //case departement
                if (criterion.PostalCode.Length == 2)
                {
                    var dep = PostalCode.Substring(0, 2);
                    return dep == criterion.PostalCode;
                }
                //case city
                else 
                {
                    //return PostalCode == criterion.PostalCode && Address.Contains(criterion.Address);
                    return PostalCode == criterion.PostalCode;
                }
            }
        }

        public IList<Rental> FindByCriteria(RentalSearchCriteria criteria)
        {
            var idsToLoad = new List<int>();
            var rentals = _Context.Rentals.AsQueryable();

            if (criteria.MinRate.HasValue)
            {   
                rentals = from rent
                               in rentals
                          where ((rent.Rate + rent.Charges) >= criteria.MinRate)
                          select rent;
            }

            if (criteria.MaxRate.HasValue)
            {
                rentals = from rent
                               in rentals
                          where ((rent.Rate + rent.Charges) <= criteria.MaxRate)
                          select rent;
            }
            if (criteria.MinSurface.HasValue)
            {
                rentals = from rent
                               in rentals
                          where (rent.Surface >= criteria.MinSurface)
                          select rent;
            }
            if (criteria.MaxSurface.HasValue)
            {
                rentals = from rent
                               in rentals
                          where (rent.Surface <= criteria.MaxSurface)
                          select rent;
            }
            if (criteria.RentalData.AvailableDate.HasValue)
            {
                rentals = from rent
                               in rentals
                          where System.DateTime.Compare(rent.AvailableDate.Value, criteria.RentalData.AvailableDate.Value) <= 0
                          select rent;
            }
            if (criteria.RentalData.AvailableNow)
            {
                rentals = from rent
                               in rentals
                          where rent.AvailableNow == true
                          select rent;
            }
            if (criteria.RentalData.LeaseType != -1)
            {
                rentals = from rent
                               in rentals
                          where (rent.LeaseType == criteria.RentalData.LeaseType)
                          select rent;
            }

            if (criteria.RentalData.Type != -1)
            {
                rentals = from rent
                               in rentals
                          where (rent.Type == criteria.RentalData.Type)
                          select rent;
            }

            //load temporary list
            var templist = (from item in rentals
                            select new
                            {
                                ID = item.Id,
                                Place = new PlaceDefinition
                                {
                                   Address = item.City.ToLower(),
                                   PostalCode = item.PostalCode
                                },
                                Features = (from f in item.RentalFeatures
                                            select new FeatureProjection
                                            {
                                                Feature = f.FeatureId
                                            })
                            }).ToList();

            if (!string.IsNullOrEmpty(criteria.Place))
            {
                var places = from item
                                in criteria.Place.Split('|')
                             where _PlaceRegex.Match(item).Success
                             select new PlaceDefinition
                             {
                                 Address = _PlaceRegex.Match(item).Groups["address"].Value.TrimEnd(' ').ToLower(),
                                 PostalCode = _PlaceRegex.Match(item).Groups["postalCode"].Value
                             };

                //filter OR by places
                templist = templist.Where(rent =>
                {
                    foreach (var item in places)
                    {
                        if (rent.Place.Compare(item))
                            return true;
                    }
                    return false;
                }).ToList();
            }

            var neededFeatures = new List<FeatureProjection>();
            foreach (var criteriaFeatures in criteria.RentalData.RentalFeatures)
            {
                neededFeatures.Add(new FeatureProjection { Feature = criteriaFeatures.FeatureId });
            }

            var equalityComparer = new FeatureProjectionEqualityComparer();
            //filter AND by features
            idsToLoad = templist.Where(rent =>
            {
                foreach (var item in neededFeatures)
                {
                    if (!rent.Features.Contains(item, equalityComparer))
                        return false;
                }
                return true;
            }).Select(rent => rent.ID).ToList();


            return _Context.Rentals.Where(rent => idsToLoad.Contains(rent.Id)).ToList();
        }
    }
}