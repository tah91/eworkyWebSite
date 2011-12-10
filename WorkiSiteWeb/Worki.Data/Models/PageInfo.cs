using System;
using System.Collections.Generic;

namespace Worki.Data.Models
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

	public class PagingList<T>
	{
		public IList<T> List { get; set; }
		public PagingInfo PagingInfo { get; set; }
	}

    public class MasterViewModel<T, U>
    {
        public PagingList<T> List { get; set; }
        public U Item { get; set; }
    }
}