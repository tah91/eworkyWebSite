﻿var iframe;
if (typeof String.prototype.startsWith != 'function') {
    String.prototype.startsWith = function (str) {
        return this.indexOf(str) == 0;
    };
}
var socket = new easyXDM.Socket({
    swf: "../easyxdm.swf",
    onReady: function () {
        iframe = document.createElement("iframe");
        iframe.frameBorder = 0;
        iframe.marginWidth = 0;
        iframe.marginHeight = 0;
        iframe.width = easyXDM.query.width + "px";
        iframe.id = easyXDM.query.id;
        iframe.name = easyXDM.query.name;
        iframe.scrolling = "no";
        document.body.appendChild(iframe);

        var query = '?';
        for (var param in easyXDM.query) {
            if (param == 'url')
                continue;
            if (param.startsWith('xdm'))
                continue;
            query += param + "=" + easyXDM.query[param] + '&';
        }
        query = query.replace(/(\s+)?.$/, "");
        //alert(query);
        iframe.src = easyXDM.query.url + query;

        computeHeight = function (d) {
            var height = d.body.clientHeight || d.body.offsetHeight || d.body.scrollHeight;
            return height;// +10;
        }
        var timer;
        iframe.onload = function () {
            var d = iframe.contentWindow.document;
            var originalHeight = computeHeight(d);
            // We want to monitor the document for resize events in case of the use of accordions and such,
            // but unfortunately only the window node emits the resize event, and we need the body's.
            // The only solution then is to use polling..

            // Lets start the polling if not all ready started
            if (!timer) {
                timer = setInterval(function () {
                    try {
                        var d = iframe.contentWindow.document;
                        var newHeight = computeHeight(d);
                        if (newHeight != originalHeight) {
                            // The height has changed since last we checked
                            originalHeight = newHeight;
                            socket.postMessage(originalHeight);
                        }
                    } catch (e) {
                        // We tried to read the property at some point when it wasn't available
                    }
                }, 300);
            }
            // Send the first message
            socket.postMessage(originalHeight);
        };
    },
    onMessage: function (url, origin) {
        iframe.src = url;
    }
});