var eworkyClass = function () {
    this.initialize.apply(this, arguments)
};
eworkyClass.prototype = {
    initialize: function () {
        this.widgetDict = {};
        this.iframeDomain = "http://www.eworky.com/widget";
    },
    postInitialize: function () {
        this.initializeWidgets()
    },
    initializeWidgets: function () {
        var widgets = this.findWidgets();
        for (var i = 0; i < widgets.length; d++) {
            var widget = widgets[i];
            var index = i + 1;
            this.widgetDict[index] = new fashiolistaButtonClass(widget, index, this.iframeDomain)
        }
    },
    findWidgets: function () {
        var links = document.links;
        var toFind = "eworky-widget";
        var toRet = [];
        for (var i = 0; i < links.length; i++) {
            var elem = links[i];
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
var fashiolistaButtonClass = function () {
    this.initialize.apply(this, arguments)
};
fashiolistaButtonClass.prototype = {
    initialize: function (elem, id, domain) {
        this.element = elem;
        this.data = eworkyUtils.parseData(this.element);
        this.buttonId = id;
        var elems = this.element.className.split(" ");
        var widgetType = elems[1].replace("eworky-", "");
        this.type = widgetType;
        this.iframeDomain = domain;
        eworkyUtils.fire("pre_button_init", this.type, this.data, this);
        if (widgetType == "finder" || widgetType == "detail") {
            this.width = 500;
            this.height = 500
        }
        this.baseFormat();
        eworkyUtils.fire("post_button_init", this, elem, id, domain)
    },
    getIframeUrl: function () {
        var toRet = this.iframeDomain + "/?widget_type=" + this.type + this.getPopupQueryString();
        return toRet
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
        var frame = document.createElement("IFRAME");
        frame.src = this.getIframeUrl();
        frame.frameBorder = 0;
        frame.marginWidth = 0;
        frame.marginHeight = 0;
        frame.width = this.width + "px";
        frame.height = this.height + "px";
        frame.scrolling = "no";
        frame.id = "eworky_widget_" + this.buttonId;
        frame.name = "eworky_widget_" + this.buttonId;
        this.element.parentNode.replaceChild(frame, this.element);
        this.element = frame
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
eworkyUtils.bindReady();