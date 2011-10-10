using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

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

        public IList<Rental> FindByCriteria(RentalSearchCriteria criteria)
        {
            var idsToLoad = new List<int>();
            var rentals = _Context.Rentals.AsQueryable();

            if (criteria.MinRate != null)
            {
                var min_rate = criteria.MinRate.Value;

                rentals = from rent
                               in rentals
                          where ((rent.Rate + rent.Charges) >= min_rate)
                          select rent;
            }

            if (criteria.MaxRate != null)
            {
                var max_rate = criteria.MaxRate.Value;

                rentals = from rent
                               in rentals
                          where ((rent.Rate + rent.Charges) <= max_rate)
                          select rent;
            }

            if (criteria.MinSurface != null)
            {
                var min_surface = criteria.MinSurface.Value;

                rentals = from rent
                               in rentals
                          where (rent.Surface >= min_surface)
                          select rent;
            }
            if (criteria.MaxSurface != null)
            {
                var max_surface = criteria.MaxSurface.Value;

                rentals = from rent
                               in rentals
                          where (rent.Surface <= max_surface)
                          select rent;
            }

            if ((criteria.RentalData.AvailableDate != null && criteria.RentalData.AvailableDate.HasValue) || criteria.RentalData.AvailableNow)
            {
                rentals = from rent
                               in rentals
                          where ((System.DateTime.Compare(rent.AvailableDate.Value, criteria.RentalData.AvailableDate.Value) <= 0) || (rent.AvailableNow == true))
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

            if (criteria.Places != null && criteria.Places.Count != 0)
            {
                foreach (var place in criteria.Places)
                {
                    rentals = from rent
                               in rentals
                              where ((rent.Address + " " + rent.PostalCode + " " + rent.City + " " + rent.Country).ToLower().Contains(criteria.Place.ToLower()))
                              select rent;
                }
            }

            var templist = (from item in rentals
                            select new
                            {
                                ID = item.Id,
                                Features = (from f in item.RentalFeatures
                                            select new FeatureProjection
                                            {
                                                Feature = f.FeatureId
                                            })
                            }).ToList();

            var neededFeatures = new List<FeatureProjection>();

            foreach (var criteriaFeatures in criteria.RentalData.RentalFeatures)
            {
                neededFeatures.Add(new FeatureProjection { Feature = criteriaFeatures.FeatureId });
            }

            var equalityComparer = new FeatureProjectionEqualityComparer();
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