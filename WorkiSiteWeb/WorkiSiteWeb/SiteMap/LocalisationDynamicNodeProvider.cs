using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using MvcSiteMapProvider.Extensibility;
using Ninject;
using Worki.Data.Models;
using Worki.Web.Helpers;
using Worki.Infrastructure.Helpers;

namespace Worki.SiteMap
{
	/// <summary>
	/// LocalisationDynamicNodeProvider class
	/// </summary>
	public class LocalisationDynamicNodeProvider
		: DynamicNodeProviderBase
	{
		static IKernel _Kernel;

		public static void RegisterKernel(IKernel kernel)
		{
			_Kernel = kernel;
		}

		public LocalisationDynamicNodeProvider()
			: base()
		{
			LocalisationRepository = _Kernel.Get<ILocalisationRepository>();
		}

		public ILocalisationRepository LocalisationRepository { get; set; }

		/// <summary>
		/// Gets the dynamic node collection.
		/// </summary>
		/// <returns>
		/// A dynamic node collection represented as a <see cref="IEnumerable&lt;MvcSiteMapProvider.Extensibility.DynamicNode&gt;"/> instance 
		/// </returns>
		public override IEnumerable<DynamicNode> GetDynamicNodeCollection()
		{
			// Create a node for each localisation
			var locs = LocalisationRepository.GetAll();
			foreach (var loc in locs)
			{
				DynamicNode node = new DynamicNode("Id_" + loc.ID.ToString(), loc.Name);
				node.RouteValues.Add("id", loc.ID);
				node.RouteValues.Add("name", MiscHelpers.GetSeoString(loc.Name));
				node.RouteValues.Add("type", MiscHelpers.GetSeoString(Localisation.GetLocalisationType(loc.TypeValue)));

				yield return node;
			}
		}
	}
}
