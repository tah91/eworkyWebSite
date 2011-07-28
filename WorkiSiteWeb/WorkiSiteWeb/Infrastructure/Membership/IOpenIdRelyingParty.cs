//using System;
//using System.Collections.Generic;
//using System.Web.Mvc;
//using DotNetOpenAuth.Messaging;
//using DotNetOpenAuth.OpenId;
//using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
//using DotNetOpenAuth.OpenId.RelyingParty;

//namespace MvcRelyingParty
//{
//    public interface IOpenIdRelyingParty
//    {
//        Channel Channel { get; }
//        IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy);
//        IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy);
//        ActionResult AjaxDiscovery(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo, Uri privacyPolicy);
//        ActionResult ProcessAjaxOpenIdResponse();
//        IAuthenticationResponse GetResponse();
//        IAuthenticationResponse GetResponse(HttpRequestInfo request);
//    }
//}