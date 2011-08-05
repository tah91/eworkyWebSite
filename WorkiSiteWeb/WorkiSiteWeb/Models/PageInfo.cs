using System;

namespace Worki.Web.Models
{
    public class PagingInfo
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages
        {
            get { return (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage); }
        }
        public int GlobalIndex(int localIndex)
        {
            return ItemsPerPage * (CurrentPage - 1) + localIndex;
        }
    }
}