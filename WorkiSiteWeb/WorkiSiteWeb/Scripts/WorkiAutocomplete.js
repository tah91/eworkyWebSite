function WorkiCityAutoComplete(textField) {
    //properties
    var _textField = textField;

    SetAutocomplete = function () {
        $(_textField).autocomplete({ minLength: 3,
            //http://www.geonames.org/export/geonames-search.html
            source: function (request, response) {
                $.ajax({
                    url: "http://api.geonames.org/searchJSON?username=tah91&country=FR&featureClass=A&featureClass=P",
                    dataType: "jsonp",
                    data: {
                        style: "full",
                        maxRows: 12,
                        q: request.term
                    },
                    success: function (data) {
                        var toDisplay = new Array();
                        response($.map(data.geonames, function (item) {
                            var postalCode = null;
                            if (item.alternateNames != null) {
                                $.each(item.alternateNames, function () {
                                    if (this.lang == 'post' && this.name.length == 5) {
                                        postalCode = this.name;
                                        return false;
                                    }
                                });
                            }
                            if (postalCode == null) {
                                postalCode = item.adminCode2;
                            }
                            var displayName = item.name + ' (' + postalCode + ')';
                            var displayName2 = item.name + ' (' + item.adminCode2 + ')';
                            var alreadyHere = $.inArray(displayName, toDisplay);
                            if (alreadyHere > -1)
                                return;
                            toDisplay.push(displayName);
                            toDisplay.push(displayName2);
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