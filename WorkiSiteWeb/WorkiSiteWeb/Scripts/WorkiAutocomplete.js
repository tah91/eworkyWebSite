function WorkiCityAutoComplete(textField) {
    //properties
    var _textField = textField;
    var _overrideCities = {
        75001: "Paris 1er",
        75002: "Paris 2e"
    }

    SetAutocomplete = function () {
        $(_textField).autocomplete({ minLength: 3,
            //http://www.geonames.org/export/geonames-search.html
            //http://www.geonames.org/export/web-services.html
            source: function (request, response) {
                $.ajax({
                    url: "http://api.geonames.org/postalCodeSearchJSON?username=tah91&country=FR&featureClass=P",
                    dataType: "jsonp",
                    data: {
                        style: "full",
                        maxRows: 12,
                        placename: request.term
                    },
                    success: function (data) {
                        var toDisplay = new Array();
                        response($.map(data.postalCodes, function (item) {
                            var name = item.placeName;
                            if (item.postalCode in _overrideCities)
                                name = _overrideCities[item.postalCode];
                            var displayName = name + ' (' + item.postalCode + ')';
                            var alreadyHere = $.inArray(displayName, toDisplay);
                            if (alreadyHere > -1)
                                return;
                            toDisplay.push(displayName);
                            if (item.postalCode.length > 5)
                                return;
                            return {
                                label: displayName,
                                value: displayName
                            }
                        }));
                    }
                });
            },
            select: function (event, ui) {
                var pos = ui.item.position;
                var lct = ui.item.locType;
            },
            selectFirst: true
        });
    }

    //public methods
    this.SetAutocomplete = SetAutocomplete;
}