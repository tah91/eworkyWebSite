//Set default open/close settings
$('.acc_container').hide();  //Hide/close all containers
//$('.acc_trigger:first').addClass('active').next().show(); //Add "active" class to first trigger, then show/open the immediate next container

//On Click
$('.acc_trigger').click(function () {
    if ($(this).next().is(':hidden')) { //If immediate next container is closed...
        $('.acc_trigger').removeClass('active').next().slideUp(); //Remove all "active" state and slide up the immediate next container
        $(this).toggleClass('active').next().slideDown(); //Add "active" state to clicked trigger and slide down the immediate next container
    }
    return false; //Prevent the browser jump to the link anchor
});

//Hide (Collapse) the toggle containers on load
   $(".toogle_container:not(.active)").hide();

//$('.toogle_trigger_click:first').addClass('active').next().show();
//Switch the "Open" and "Close" state per click then slide up/down (depending on open/close state)
$(".toogle_trigger_click").click(function () {
    $(this).toggleClass("active").next().slideToggle();
    return false; //Prevent the browser jump to the link anchor
});

function InitTab(tabId) {
	//When page loads...
	var hiddenClass = "visuallyhidden";
	$(tabId + " .tab_content").addClass(hiddenClass); //.hide(); //Hide all content
    $(tabId + " ul.tabs li:first").addClass("active");//.show(); //Activate first tab
    $(tabId + " .tab_content:first").removeClass(hiddenClass); //.show(); //Show first tab content

    //On Click Event
    $(tabId + " ul.tabs li").click(function () {

        $(tabId + " ul.tabs li").removeClass("active"); //Remove any "active" class
        $(this).addClass("active"); //Add "active" class to selected tab
        $(tabId + " .tab_content").addClass(hiddenClass); //hide(); //Hide all tab content

        var activeTab = $(this).find("a").attr("href"); //Find the href attribute value to identify the active tab + content
        $(activeTab).removeClass(hiddenClass);//.fadeIn(); //Fade in the active ID content
        return false;
    });
};

$(document).ready(function () {
    // On cache les sous-menus
    // sauf celui qui porte la classe "open_at_load" :
    $(".navigation .subMenu:not('.open_at_load')").hide();
    // On sélectionne tous les items de liste portant la classe "toggleSubMenu"

    // On modifie l'évènement "click" sur les liens dans les items de liste
    // qui portent la classe "toggleSubMenu" :
    $(".navigation .toggleSubMenu").click(function () {
        // Si le sous-menu était déjà ouvert, on le referme :
        if ($(this).next(".subMenu:visible").length != 0) {
            $(this).next(".subMenu").fadeToggle("normal");
            $(this).removeClass("active");
        }
        // Si le sous-menu est caché, on ferme les autres et on l'affiche :
        else {
            // $(".navigation .subMenu").slideUp("normal", function () { $(this).parent().removeClass("active") });
            $(this).next(".subMenu").fadeToggle("normal");
            $(this).addClass("active");
        }
        // On empêche le navigateur de suivre le lien :
        return false;
    });

});