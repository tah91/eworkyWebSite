using System;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.OAuth2;
using Worki.Data.Models;
using Worki.Infrastructure;
using Worki.Infrastructure.Logging;
using DotNetOpenAuth.Messaging;
using System.Net;
using Worki.Infrastructure.Repository;
using Worki.Web.Helpers;

namespace Worki.Web.Controllers
{
	public partial class OAuthController : ControllerBase
    {
		private readonly AuthorizationServer authorizationServer = new AuthorizationServer(new OAuth2AuthorizationServer());

		public OAuthController(ILogger logger, IObjectStore objectStore)
            : base(logger, objectStore)
        {
            
        }

		/// <summary>
		/// The OAuth 2.0 token endpoint.
		/// </summary>
		/// <returns>The response to the Client.</returns>
		public virtual ActionResult Token()
		{
			return this.authorizationServer.HandleTokenRequest(this.Request).AsActionResult();
		}

		/// <summary>
		/// Prompts the user to authorize a client to access the user's private data.
		/// </summary>
		/// <returns>The browser HTML response that prompts the user to authorize the client.</returns>
		[Authorize, AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
		[HttpHeader("x-frame-options", "SAMEORIGIN")] // mitigates clickjacking
		public virtual ActionResult Authorize()
		{
			var pendingRequest = this.authorizationServer.ReadAuthorizationRequest();
			if (pendingRequest == null)
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
			}

			var context = ModelFactory.GetUnitOfWork();
			var acRepo = ModelFactory.GetRepository<IApiClientRepository>(context);

			var requestingClient = acRepo.Get(c => c.ClientIdentifier == pendingRequest.ClientIdentifier);

			// Consider auto-approving if safe to do so.
			if (((OAuth2AuthorizationServer)this.authorizationServer.AuthorizationServerServices).CanBeAutoApproved(pendingRequest))
			{
				var approval = this.authorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, HttpContext.User.Identity.Name);
				return this.authorizationServer.Channel.PrepareResponse(approval).AsActionResult();
			}

			var model = new AccountAuthorizeModel
			{
				ClientApp = requestingClient.Name,
				Scope = pendingRequest.Scope,
				AuthorizationRequest = pendingRequest,
			};

			return View(model);
		}

		/// <summary>
		/// Processes the user's response as to whether to authorize a Client to access his/her private data.
		/// </summary>
		/// <param name="isApproved">if set to <c>true</c>, the user has authorized the Client; <c>false</c> otherwise.</param>
		/// <returns>HTML response that redirects the browser to the Client.</returns>
		[Authorize, HttpPost, ValidateAntiForgeryToken]
		public virtual ActionResult AuthorizeResponse(bool isApproved)
		{
			var pendingRequest = this.authorizationServer.ReadAuthorizationRequest();
			if (pendingRequest == null)
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
			}

			IDirectedProtocolMessage response;
			if (isApproved)
			{
				// The authorization we file in our database lasts until the user explicitly revokes it.
				// You can cause the authorization to expire by setting the ExpirationDateUTC
				// property in the below created ClientAuthorization.
				var context = ModelFactory.GetUnitOfWork();
				var acRepo = ModelFactory.GetRepository<IApiClientRepository>(context);
				var memberId = WebHelper.GetIdentityId(User.Identity);
				try
				{
					var client = acRepo.Get(c => c.ClientIdentifier == pendingRequest.ClientIdentifier);
					client.ApiClientAuthorizations.Add(
						new ApiClientAuthorization
						{
							Scope = OAuthUtilities.JoinScopes(pendingRequest.Scope),
							MemberId = memberId, //TODO Check
							CreatedOnUtc = DateTime.UtcNow,
						});

					context.Complete(); // submit now so that this new row can be retrieved later in this same HTTP request
				}
				catch (Exception ex)
				{
					context.Complete();
					_Logger.Error("AuthorizeResponse", ex);
				}


				// In this simple sample, the user either agrees to the entire scope requested by the client or none of it.  
				// But in a real app, you could grant a reduced scope of access to the client by passing a scope parameter to this method.
				response = this.authorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, User.Identity.Name);
			}
			else
			{
				response = this.authorizationServer.PrepareRejectAuthorizationRequest(pendingRequest);
			}

			return this.authorizationServer.Channel.PrepareResponse(response).AsActionResult();
		}
    }
}
