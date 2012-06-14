var root = 'http://taff2.coworky.fr';
var eworkyClass = function () {
    this.initialize.apply(this, arguments)
};
eworkyClass.prototype = {
    initialize: function () {
        this.widgetDict = {};
        //this.root = 'http://taff2.coworky.fr';
        this.iframeIntermediateDomain = root + '/widget/intermediate';
        this.iframeDomain = root + '/widget/intermediate/dispatch';
    },
    postInitialize: function () {
        this.initializeWidgets()
    },
    initializeWidgets: function () {
        var widgets = this.findWidgets();
        for (var i = 0; i < widgets.length; i++) {
            var widget = widgets[i];
            var index = i + 1;
            this.widgetDict[index] = new eworkyWidgetClass(widget, index, this.iframeIntermediateDomain, this.iframeDomain)
        }
    },
    findWidgets: function () {
        var divs = document.getElementsByTagName('div');
        var toFind = "eworky-widget";
        var toRet = [];
        for (var i = 0; i < divs.length; i++) {
            var elem = divs[i];
            if (elem.className.indexOf(toFind) >= 0) {
                toRet.push(elem)
            }
        }
        return toRet
    },
    listen: function () {
        return eworkyUtils.listen.apply(eworkyUtils, arguments)
    }
};
var eworkyUtilsClass = function () {
    this.initialize.apply(this, arguments)
};
eworkyUtilsClass.prototype = {
    initialize: function () {
        this.eventMap = {}
    },
    getUrlVars: function (url) {
        var toRet = {},
            param;
        url = url || window.location.href;
        var queries = url.slice(url.indexOf("?") + 1).split("&");
        for (var i = 0; i < queries.length; i++) {
            param = queries[i].split("=");
            toRet[param[0]] = decodeURIComponent(param[1])
        }
        return toRet
    },
    parseData: function (elem) {
        var toRet = {};
        for (var i = 0; i < elem.attributes.length; i++) {
            var attr = elem.attributes[i];
            if (attr.name.indexOf("data-") == 0) {
                var key = attr.name.replace("data-", "");
                var val = attr.value;
                toRet[key] = val
            }
        }
        return toRet
    },
    ready: function () {
        if (!eworkyUtils.isReady) {
            if (!document.body) {
                return setTimeout(eworkyUtils.ready, 13)
            }
            eworkyUtils.isReady = true;
            try {
                eworky = new eworkyClass();
                if (typeof (eworkyAsyncLoaded) != "undefined") {
                    eworkyAsyncLoaded()
                }
                eworky.postInitialize()
            } catch (ex) {
                if (typeof (console) != "undefined") {
                    console.log(ex)
                }
            }
        }
    },
    bindReady: function () {
        if (eworkyUtils.readyBound) {
            return
        }
        eworkyUtils.readyBound = true;
        if (document.readyState === "complete") {
            return eworkyUtils.ready()
        }
        if (document.addEventListener) {
            document.addEventListener("DOMContentLoaded", DOMContentLoaded, false);
            window.addEventListener("load", eworkyUtils.ready, false)
        } else {
            if (document.attachEvent) {
                document.attachEvent("onreadystatechange", DOMContentLoaded);
                window.attachEvent("onload", jQuery.ready);
                var hasFrame = false;
                try {
                    hasFrame = window.frameElement == null
                } catch (ex) { }
                if (document.documentElement.doScroll && hasFrame) {
                    doScrollCheck()
                }
            }
        }
    },
    listen: function (event, handler) {
        if (!(event in this.eventMap)) {
            this.eventMap[event] = []
        }
        this.eventMap[event].push(handler)
    },
    fire: function (event) {
        if (event in this.eventMap) {
            var array = this.eventMap[event];
            for (var i = 0; i < array.length; i++) {
                var handler = array[i];
                handler.apply(handler, arguments)
            }
        }
    }
};
eworkyUtils = new eworkyUtilsClass();
var eworkyWidgetClass = function () {
    this.initialize.apply(this, arguments)
};
eworkyWidgetClass.prototype = {
    initialize: function (elem, id, intermediateDomain, domain) {
        this.element = elem;
        this.data = eworkyUtils.parseData(this.element);
        this.widgetId = id;
        this.type = this.data['kind'];
        this.iframeIntermediateDomain = intermediateDomain;
        this.iframeDomain = domain;
        eworkyUtils.fire("pre_button_init", this.type, this.data, this);
        this.width = this.data['width'];
        if (this.type == "finder" || this.type == "detail") {
            if (this.width < 780)
                this.width = 780;
        }
        this.baseFormat();
    },
    getIframeUrl: function () {
        var intermediate = this.iframeIntermediateDomain + '?width=' + this.width + '&id=eworky_widget_' + this.widgetId + '&name=eworky_widget_' + this.widgetId;
        var url = this.iframeDomain + this.getPopupQueryString();
        return intermediate + '&url=' + url;
    },
    getPopupQueryString: function () {
        var toRet = '';
        for (dataKey in this.data) {
            var param = this.data[dataKey];
            if (param) {
                toRet += "&" + dataKey + "=" + encodeURIComponent(param)
            }
        }
        return toRet
    },
    baseFormat: function () {
        var width = this.width;
        var transport = new easyXDM.Socket({
            remote: this.getIframeUrl(),
            container: this.element,
            onMessage: function (message, origin) {
                this.container.getElementsByTagName("iframe")[0].style.height = message + "px";
            },
            onReady: function () {
                this.container.getElementsByTagName("iframe")[0].style.width = width + "px";
            }
        });
    }
};
if (document.addEventListener) {
    DOMContentLoaded = function () {
        document.removeEventListener("DOMContentLoaded", DOMContentLoaded, false);
        eworkyUtils.ready()
    }
} else {
    if (document.attachEvent) {
        DOMContentLoaded = function () {
            if (document.readyState === "complete") {
                document.detachEvent("onreadystatechange", DOMContentLoaded);
                eworkyUtils.ready()
            }
        }
    }
}
function doScrollCheck() {
    if (eworkyUtils.isReady) {
        return
    }
    try {
        document.documentElement.doScroll("left")
    } catch (ex) {
        setTimeout(doScrollCheck, 1);
        return
    }
    eworkyUtils.ready()
}

function loadScript(url, callback) {
    // adding the script tag to the head as suggested before
    var head = document.getElementsByTagName('head')[0];
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = url;

    // then bind the event to the callback function 
    // there are several events for cross browser compatibility
    script.onreadystatechange = callback;
    script.onload = callback;

    // fire the loading
    head.appendChild(script);
}


var initializeFrames = function () {
    eworkyUtils.bindReady();
};

loadScript(root + '/Scripts/easyXDM.js', initializeFrames);