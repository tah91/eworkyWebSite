InitOpenGraph = function (callback) {
	//check if user has athorized the app
	/*FB.getLoginStatus(function (response) {
		if (response.status != 'connected') {*/
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
		/*} else {
			callback.call();
		}
	});*/
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
            else if (xhr.status != 0) {
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

function ErrorBuilder(id, errorId) {
    //properties
    var _errorId = errorId;
    var _id = id;

    _BuildError = function (error, id) {
        return '<div id="' + id + '" class="validation-summary-errors borderRadius">' + error + '</div>';
    }

    ErrorFunc = function (data) {
        $('#' + errorId).remove();
        $('#' + id).prepend($(_BuildError(data, errorId)));
    }

    //public methods
    this.ErrorFunc = ErrorFunc;
}

function InitLoadPending() {
    //ajax load pending
    $('.loaderBlock').addClass('visuallyhidden');   // hide it initially
    jQuery.ajaxSetup({
        beforeSend: function () {
            $('.loaderBlock').removeClass('visuallyhidden');
        },
        complete: function () {
            $('.loaderBlock').addClass('visuallyhidden');
        },
        success: function () { }
    });
}

function InitScroller() {
    //keep search panel visible 
    var offset = $('#resultSearchBlock').offset();
    var width = $('#resultSearchBlock').width();

    $(window).scroll(function () {
        var searchBlock = $('#resultSearchBlock');
        var container = $('#globalContainer');
        var y = $(window).scrollTop();
        var yo = offset.top;
        var searchBottom = y + searchBlock.outerHeight();
        var containerBottom = yo + container.outerHeight();
        if (y < yo) {
            searchBlock.removeClass('fixed').removeClass('newbottom');
        }
        else if (y > yo && y + searchBlock.outerHeight() < yo + container.outerHeight()) {
            searchBlock.removeClass('newbottom').addClass('fixed').width(width);
        }
        else if (container.outerHeight() - searchBlock.outerHeight() > 30) {
            searchBlock.removeClass('fixed').addClass('newbottom');
        }
    });
}