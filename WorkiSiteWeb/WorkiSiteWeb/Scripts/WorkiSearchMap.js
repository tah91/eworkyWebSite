function WorkiMapSearch(markerPopupUrl, markerLinkUrl, ajaxSubmitUrl, bigMap, refreshResults, needAutocomplete, needMap, ajaxSubmit) {
    var _markerPopupUrl = markerPopupUrl;
    var _markerLinkUrl = markerLinkUrl;
    var _ajaxSubmitUrl = ajaxSubmitUrl;
    var _bigMap = bigMap;
    var _refreshResults = refreshResults;
    var _needAutocomplete = needAutocomplete == null ? true : needAutocomplete;
    var _needMap = needMap == null ? true : needMap;
    var _ajaxSubmit = ajaxSubmit == null ? true : ajaxSubmit;

    var _resultsMap = null;
    var _workiGeoCoder = null;
    var _infowindow = null;
    var _newResultsPushed = false;

    var _shouldProccess = true;
    var _history = window.History; // Note: We are using a capital H instead of a lower h
    var _rootUrl = _history.getRootUrl();
    // Bind to StateChange Event
    _history.Adapter.bind(window, 'statechange', function () { // Note: We are using statechange instead of popstate
        // Prepare Variables
        var state = _history.getState(),
			url = state.url,
			relativeUrl = url.replace(_rootUrl, '');

        if (_shouldProccess == false) {
            _shouldProccess = true;
            return;
        }

        $.ajax({
            url: url,
            cache: false,
            success: _refreshResults
        });
    });

    _bounds_changed = function () {
        //this is gg map, we are in an handler
        var bounds = this.getBounds();
        var nelat = bounds.getNorthEast().lat();
        var nelng = bounds.getNorthEast().lng();
        var swlat = bounds.getSouthWest().lat();
        var swlng = bounds.getSouthWest().lng();

        $('#Criteria_NorthEastLat').val(nelat);
        $('#Criteria_NorthEastLng').val(nelng);
        $('#Criteria_SouthWestLat').val(swlat);
        $('#Criteria_SouthWestLng').val(swlng);

        if (!_newResultsPushed) {
            $("#searchFormReset form").submit();
        }
    }

    _markerDetailPopUp = function () {
        var map = this.map;
        var marker = this;
        $.ajax({
            url: _markerPopupUrl,
            data: { "id": this.get("id") },
            success: function (data) {
                if (_infowindow) {
                    _infowindow.close();
                }

                _infowindow = new google.maps.InfoWindow({
                    content: data
                });

                _infowindow.open(map, marker);
                $('div.rateit').rateit();
            }
        });
    }

    _markerDetailPage = function () {
        var map = this.map;
        var marker = this;
        $.ajax({
            url: _markerLinkUrl,
            data: { "id": this.get("id") },
            success: function (data) {
                window.location.href = data;
            }
        });
    }

    _askForResults = function () {
        _newResultsPushed = false;
    }

    _goToTop = function () {
        $('html, body').animate({ scrollTop: 0 }, 0);
    }

    //fill the map
    _pushResults = function (place, results) {
        var bounds = new google.maps.LatLngBounds();

        this.Map.ClearMap();
        if (!_bigMap) {
            this.Map.CenterSearchResults(place);
        }

        for (var loc in results) {
            var markerHandler = _bigMap ? _markerDetailPopUp : _markerDetailPage;
            this.Map.AddMarker(results[loc].latitude, results[loc].longitude, results[loc].name, results[loc].id, bounds, markerHandler);
        }

        if (results.length >= 2 && !_bigMap) {
            this.Map.FitBoundsSearchResults(bounds);
        }

        if (_bigMap) {
            this.Map.SetCluster();
        }
        _newResultsPushed = true;
    }

    _applyResults = function (link) {
        _goToTop.apply();
        // Ajaxify this link
        _history.pushState(null, "", link.href);
        //event.preventDefault();
        return false;
//        $.ajax({
//            url: link.href,
//            success: _refreshResults
//        });
    }

    var errorBuilder = new ErrorBuilder('searchFormReset', 'searchFormError');

    _onSubmitSuccess = function (data) {
        _refreshResults(data);
        _shouldProccess = false;
        _history.pushState(null, "", data.url);
    }

    _submitData = function () {
        _goToTop.apply();
        AppAjax(
			_ajaxSubmitUrl,
			"POST",
			$('#searchFormReset form').serializeArray(),
            _onSubmitSuccess,
            errorBuilder.ErrorFunc
		);
    }

    this.Map = _resultsMap;
    this.WorkiGeoCoder = _workiGeoCoder;
    this.IsBig = _bigMap;
    this.Bounds_changed = _bounds_changed;
    this.AskForResults = _askForResults;
    this.ApplyResults = _applyResults;
    this.SubmitData = _submitData;
    this.PushResults = _pushResults;
    this.NeedAutocomplete = _needAutocomplete;
    this.NeedMap = _needMap;
    this.AjaxSubmit = _ajaxSubmit;
}