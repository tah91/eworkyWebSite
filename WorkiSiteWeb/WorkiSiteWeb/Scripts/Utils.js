
InitOpenGraph = function (callback) {
	//check if user has athorized the app
	FB.getLoginStatus(function (response) {
		if (response.status != 'connected') {
			//get authorization
			FB.login(function (response) {
				if (response.authResponse) {
					callback.call();
				} else {
					console.log('User cancelled login or did not fully authorize.');
				}
			},
			{ scope: 'publish_actions' }
			);
		} else {
			callback.call();
		}
	});
}

AppAjax = function (url, type, datagetter, onsuccess, onerror) {
	/// <summary>jQuery extension that executes Ajax calls and handles errors in application specific ways.</summary>
	/// <param name="url" type="String">URL address where to issue this Ajax call.</param>
	/// <param name="type" type="String">HTTP method type (GET, POST, DELETE, PUT, HEAD)</param>
	/// <param name="datagetter" type="String">This is ajax data.</param>
	/// <param name="onsuccess" type="Function">This optional function(data) will be called after a successful Ajax call.</param>
	/// <param name="onerror" type="Function">This optional function(data) will be called after a error 400.</param>

	var execSuccess = $.isFunction(onsuccess) ? onsuccess : $.noop;
	var execError = $.isFunction(onerror) ? onerror : function () { return onerror; };
	var datata = datagetter;

	$.ajax({
		url: url,
		type: type,
		data: datagetter,
		error: function (xhr, status, err) {
			if (xhr.status == 400) {
				execError(xhr.responseText)
			}
			else {
				execError('An error occurred.');
			}
		},
		success: function (data, status, xhr) {
			window.setTimeout(function () {
				execSuccess(data);
			}, 10);
		}
	});
}