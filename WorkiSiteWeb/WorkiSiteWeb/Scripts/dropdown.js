function InitDropdown(dropdownId) {
	$(dropdownId + " dt a").click(function () {
		$(dropdownId + " dd ul").toggle();
	});

	$(dropdownId + " dd ul li a").click(function () {
		//var text = $(this).html();
		//$(".dropdown dt a span").html(text);
		$(dropdownId + " dd ul").hide();
	});

	$(document).bind('click', function (e) {
		var clicked = $(e.target);
		if (!clicked.parents().hasClass("dropdown"))
			$(dropdownId + " dd ul").hide();
	});
};