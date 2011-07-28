<%--<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import namespace="DotNetOpenAuth.Mvc" %>
<%@ Import namespace="DotNetOpenAuth.OpenId.RelyingParty" %>
<% using (Html.BeginForm("LogOnPostAssertion", "Auth", FormMethod.Post, new { target = "_top" })) { %>
	<%: Html.AntiForgeryToken() %>
	<%: Html.Hidden("ReturnUrl", Request.QueryString["ReturnUrl"], new { id = "ReturnUrl" }) %>
	<%: Html.Hidden("openid_openidAuthData") %>
	<div id="login-oauth-container">

		<div class="helpDoc">
			<p>
            <%:WorkiSiteWeb.Resources.Views.Account.AccountString.ChooseProvider%>
				
			</p>
		</div>

	</div>
<% } %>--%>