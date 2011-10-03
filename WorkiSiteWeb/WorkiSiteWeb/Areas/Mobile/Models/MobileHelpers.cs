using Worki.Infrastructure.Repository;
using Worki.Data.Models;

namespace Worki.Mobile.Helpers
{
    public static class MobileHelpers
    {
        public static int GetLocalisationCount()
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            return lRepo.GetCount();
        }
    }
}