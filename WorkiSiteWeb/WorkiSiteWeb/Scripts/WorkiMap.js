function Utils() { }

//methods
//to convert from js double
Utils.GetCSharpDouble = function (val) {
    var str1 = val.toString().replace(".", ",");
    return str1;
}

Utils.GetJSDouble = function (val) {
    var str1 = parseFloat(val.toString().replace(",", "."));
    return str1;
}

function WorkiAutoComplete(textField) {
    //properties
    var _textField = textField;
    var _geocoder = new google.maps.Geocoder();

    SetAutocomplete = function () {
        $(_textField).autocomplete({ minLength: 5,

            source: function (request, response) {

                if (_geocoder == null) {
                    _geocoder = new google.maps.Geocoder();
                }

                var bottomLeft = new google.maps.LatLng(41.395378, -5.93291);
                var topRight = new google.maps.LatLng(52.444311, 11.381544);
                var topLeft = new google.maps.LatLng(51.800574, -5.815127);
                var bottomRight = new google.maps.LatLng(40.737341, 11.713035);
                var frBounds = new google.maps.LatLngBounds(bottomLeft, topRight);
                _geocoder.geocode({ 'address': request.term + ', fr', 'bounds': frBounds, 'region': 'FR' }, function (results, status) {
                    if (status == google.maps.GeocoderStatus.OK) {
                        response($.map(results, function (loc) {
                            var searchLoc = loc.geometry.location;
                            var lat = loc.geometry.location.lat();
                            var lng = loc.geometry.location.lng();
                            var latlng = new google.maps.LatLng(lat, lng);
                            if (!frBounds.contains(latlng))
                                return;
                            return {
                                label: loc.formatted_address,
                                value: loc.formatted_address
                            }
                        }));
                    }
                });
            },
            select: function (event, ui) {
                var pos = ui.item.position;
                var lct = ui.item.locType;
            }
        });
    }

    //public methods
    this.SetAutocomplete = SetAutocomplete;
}

function WorkiGeocoder(latitudeField, longitudeField, addressField, form, evt, cityField, postalCodeField, countryField) {
    //properties
    var _latitudeField = latitudeField;
    var _longitudeField = longitudeField;
    var _addressField = addressField;
    var _cityField = cityField;
    var _postalCodeField = postalCodeField;
    var _countryField = countryField;
    var _form = form;
    var _evt = evt;
    var _geocoder = new google.maps.Geocoder();
    var _checkSimilarLocalisation = null;

    //Geocode from an address
    SearchFormSubmit = function () {
        _evt.preventDefault();
        $(_form).unbind('submit');
        var address = jQuery.trim($(_addressField).val());
        if (address.length < 1)
            return;
        GeocodeAddress(address);
    }

    //Geocode from an address
    GeocodeAddress = function (address, checkSimilarLocalisation) {
        $(_latitudeField).val("0");
        $(_longitudeField).val("0");
        _checkSimilarLocalisation = checkSimilarLocalisation;
        _geocoder.geocode({ 'address': address, 'region': 'FR' }, _callbackForGeocode);
    }

    _callbackForGeocode = function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            var latToFill = results[0].geometry.location.lat();
            var lngToFill = results[0].geometry.location.lng();
            $(_latitudeField).val(latToFill);
            $(_longitudeField).val(lngToFill);
            if (_countryField != null || _postalCodeField != null) {
                for (var addComponent in results[0].address_components) {
                    var component = results[0].address_components[addComponent];
                    for (typeIndex in component.types) {
                        if (component.long_name == null)
                            continue;
                        if (component.types[typeIndex] == 'country') {
                            $(_countryField).val(component.long_name);
                        }
                        else if (component.types[typeIndex] == 'postal_code') {
                            $(_postalCodeField).val(component.long_name);
                        }
                        else if (component.types[typeIndex] == 'locality') {
                            $(_cityField).val(component.long_name);
                        }
                    }
                }
            }
            if (_checkSimilarLocalisation != null) {
                _checkSimilarLocalisation.call(null, latToFill, lngToFill);
            }
        }
        if (form != null) {
            $(form).trigger('submit');
        }
        //        else {
        //            //alert("La géolocalisation de votre lieu a échouée");
        //            return;
        //        }
    }

    //public methods
    this.SearchFormSubmit = SearchFormSubmit;
    this.GeocodeAddress = GeocodeAddress;
}

function WorkiMap(mapDivId, latitudeField, longitudeField) {
    //properties
    var _mapDivId = mapDivId;
    var _searchMap = null;
    var _detailMap = null;
    var _markersArray = [];
    var _geocoder = new google.maps.Geocoder();
    var _latitudeField = latitudeField;
    var _longitudeField = longitudeField;

    //methods
    //load an empty map to fill
    LoadSearchMap = function () {
        var options = {
            zoom: 9,
            mapTypeControl: false,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };
        _searchMap = new google.maps.Map(document.getElementById(_mapDivId), options);

        var center = new google.maps.LatLng(48, 2); //Paris...
        _searchMap.setCenter(center);
    }

    //load map with an adress
    LoadDetailMap = function (latitude, longitude, title, editable) {
        var options = {
            zoom: 15,
            mapTypeControl: false,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };
        _detailMap = new google.maps.Map(document.getElementById(_mapDivId), options);

        var center = new google.maps.LatLng(48, 2); //Paris...
        if (latitude != null && latitude != 0 && longitude != null && longitude != 0) {
            center = new google.maps.LatLng(latitude, longitude);
        }
        else {
            _detailMap.setZoom(5);
        }

        _detailMap.setCenter(center);
        LoadPin(center, title, editable, _detailMap);
    }

    ClearMap = function () {
        if (_markersArray) {
            for (i in _markersArray) {
                _markersArray[i].setMap(null);
            }
            _markersArray.length = 0;
        }
    }

    //load an pin with address, nam and desc
    LoadPin = function (LL, name, dragable, theMap) {
        if (dragable == null)
            dragable = false;
        if (theMap == null)
            theMap = _searchMap;
        var title = "<span class=\"pinTitle\"> " + name + "</span>";
        var image = '/Content/images/iconeMap.png';
        var marker = new google.maps.Marker({
            position: LL,
            map: theMap,
            title: name,
            icon: image
        });
        if (dragable == true)
            _markersArray.push(marker);

        marker.setDraggable(dragable);
        if (dragable) {
            google.maps.event.addListener(marker, 'dragend', function () {
                //alert(marker.getPosition().lng());
                $(_latitudeField).val(marker.getPosition().lat());
                $(_longitudeField).val(marker.getPosition().lng());
            });
        }

    }

    //ajust zoom of the search map on an address
    FitBoundsSearchResults = function (bounds) {
        _searchMap.fitBounds(bounds);
    }

    //center the search map on an address
    CenterSearchResults = function (where) {
        _geocoder.geocode({ 'address': where }, _callbackForCenterSearchResults);
    }

    _callbackForCenterSearchResults = function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            _searchMap.setCenter(results[0].geometry.location);
            //_map.setZoom(9);
            //alert(results[0].geometry.location);
            //LoadPin(results[0].geometry.location, 'Votre recherche');
        }
        else {
            //alert("La géolocalisation de votre lieu a échouée");
            return;
        }
    }

    //find address on the map with draggable pin
    FindAddressOnMap = function (where) {
        _geocoder.geocode({ 'address': where }, _callbackForFindAddressOnMap);
    }

    _callbackForFindAddressOnMap = function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            _detailMap.setCenter(results[0].geometry.location);
            _detailMap.setZoom(15);
            //alert(results[0].geometry.location);
            LoadPin(results[0].geometry.location, 'Votre choix', true, _detailMap);
        }
        else {
            //alert("La géolocalisation de votre lieu a échouée");
            return;
        }
    }

    _renderLocalisations = function (localisations) {
        $("#localisationList").empty();

        ClearMap();

        $.each(localisations, function (i, localisation) {

            var LL = new google.maps.LatLng(localisation.Latitude, localisation.Longitude);

            // Add Pin to Map
            LoadPin(LL, _getLocalisationLinkHTML(localisation));

            //Add a localisation to the <ul> localisationList on the right
            $('#localisationList').append($('<li/>')
                              .attr("class", "localisationItem")
                              .append(_getLocalisationLinkHTML(localisation))
                              .append($('<br/>'))
                              .append(_getLocalisationDescriptionHTML(localisation)));
        });

        // Adjust zoom to display all the pins we just added.

        // Display the event's pin-bubble on hover.
        /*$(".localisationItem").each(function (i, localisation) {
        $(localisation).hover(
        function () { _map.ShowInfoBox(_shapes[i]); },
        function () { _map.HideInfoBox(_shapes[i]); }
        );
        });*/

        function _getLocalisationLinkHTML(localisation) {
            return '<a href=/Localisation/Details/' + localisation.ID + '>' + localisation.Name + '</a>';
        }

        function _getLocalisationDescriptionHTML(localisation) {
            return '<p>' + localisation.Description + '</p>';
        }
    }

    //public methods
    this.LoadSearchMap = LoadSearchMap;
    this.LoadDetailMap = LoadDetailMap;
    this.ClearMap = ClearMap;
    this.LoadPin = LoadPin;
    this.FindAddressOnMap = FindAddressOnMap;
    this.CenterSearchResults = CenterSearchResults;
    this.FitBoundsSearchResults = FitBoundsSearchResults;
}
