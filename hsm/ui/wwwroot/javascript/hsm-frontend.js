//Copyright: OCC

const cookieBanner = document.getElementById('hsm-cookie-banner')
const cookieMessage = document.getElementById('hsm-cookie-message')
const cookieMessageAccept = document.getElementById('hsm-cookies-accepted')
const cookieMessageReject = document.getElementById('hsm-cookies-rejected')

const btnCookiesAccept = document.getElementById('hsm-btn-cookies-accept')
const btnCookiesReject = document.getElementById('hsm-btn-cookies-reject')
const btnHideCookieMessage = document.querySelectorAll('.hsm-cookie-message-hide-btn')

const cookiePageMessage = document.getElementById('cookie-settings-message')
const cookiesAnalyticsYesBtn = document.getElementById('cookies-analytics-yes');
const cookiesAnalyticsNoBtn = document.getElementById('cookies-analytics-no');

const acceptedPropertyName = "hsm_cookies_accepted";
const rejectedPropertyName = "hsm_cookies_rejected";
const msgHidePropertyName = "hsm_msg_hide";

// ie 11 Polyfill start 
if (window.NodeList && !NodeList.prototype.forEach) {
    NodeList.prototype.forEach = Array.prototype.forEach;
}
if (!String.prototype.includes) {
    String.prototype.includes = function(search, start) {
        'use strict';

        if (search instanceof RegExp) {
        throw TypeError('first argument must not be a RegExp');
        }
        if (start === undefined) { start = 0; }
        return this.indexOf(search, start) !== -1;
    };
}
if (!String.prototype.startsWith) {
    Object.defineProperty(String.prototype, 'startsWith', {
        value: function(search, rawPos) {
            var pos = rawPos > 0 ? rawPos|0 : 0;
            return this.substring(pos, pos + search.length) === search;
        }
    });
}
// ie 11 Polyfill end


// Check if banner has been dismissed on page load
let dismissBanner = localStorage.getItem('hideBanner')

//  If banner has not been dismissed unhide it
if(!dismissBanner) {
    cookieBanner.classList.remove('hsm-hidden')
}

// Toggle HTML 'hidden' attribute 
function cookieMessageToggle(message) {
    message.hidden = !message.hidden;
}

// Hides the main Cookie message, displays Cookies Accepted message
function acceptCookies(e) {
    e.preventDefault()
    cookieMessageToggle(cookieMessage)
    cookieMessageToggle(cookieMessageAccept)
}

// Hides the main Cookie message, displays Cookies Rejected message
function rejectCookies(e) {
    e.preventDefault()
    cookieMessageToggle(cookieMessage)
    cookieMessageToggle(cookieMessageReject)
}

// Hides the entire Cookie Banner
function hideCookieMessage() {
    saveToStorage(msgHidePropertyName, true, oneYear);
    cookieMessageToggle(cookieBanner)
    localStorage.setItem('hideBanner', true) 
}

btnCookiesAccept.addEventListener('click', acceptCookies)
btnCookiesReject.addEventListener('click', rejectCookies)
btnHideCookieMessage.forEach(function(button) {
    button.addEventListener('click', hideCookieMessage)
})

function SaveCookiesSettings() {
    if (cookiesAnalyticsYesBtn != null && cookiesAnalyticsYesBtn.checked) {
        onAcceptOptionalCookies();
        cookieMessageToggle(cookiePageMessage)  
    }
    else if (cookiesAnalyticsNoBtn != null && cookiesAnalyticsNoBtn.checked) {
        onRevokeOptionalCookies();
        cookieMessageToggle(cookiePageMessage)  
    }

    localStorage.setItem("messageVisible", true);
}

// Disable until opt-in. Works with Universal Analytics and Google Analytics 4.
window['ga-disable-' + jsGoogleAnalyticsTrackingId] = true;
window.dataLayer = window.dataLayer || [];
function gtag() {    
    dataLayer.push(arguments);
} // called in onAccept/Revoke event-handlers...


function onAcceptOptionalCookies() {    
    //Add the cookie
    saveToStorage(acceptedPropertyName, true, oneYear);

    //Remove rejected cookie in case it was set before
    //Adding the cookie again will overwrite the existing one, adding it with empty value will sort of remove it
    saveToStorage(rejectedPropertyName, "", oneYear);

    window['ga-disable-' + jsGoogleAnalyticsTrackingId] = false;
    
    // Now fire Google Tag Manager
    gtag('js', new Date());
    gtag('set', { cookie_flags: 'SameSite=None;Secure' });
    gtag('config', jsGoogleAnalyticsTrackingId);
}

function onRevokeOptionalCookies() {
    saveToStorage(rejectedPropertyName, true, oneYear);

    //Remove accepted cookie in case it was set before
    //Adding the cookie again will overwrite the existing one, adding it with empty value will sort of remove it
    saveToStorage(acceptedPropertyName, "", oneYear);
        
    window['ga-disable-' + jsGoogleAnalyticsTrackingId] = true;
}

const cookieStorage = {
    getItem: function(key) {

        var docCookies = document.cookie.split(';');
        var cookies = {};

        docCookies.forEach(function(x) {
            if (x.trim().startsWith("hsm_")) {
                var keys = x.split('=');
                    if (keys != null && keys.length > 1) {
                        cookies[keys[0].trim()] = keys[1];
                    }             
            }
        });
        return cookies[key];
    },
    setItem: function(key, value, expireDate) {        
        document.cookie = (key +'=')+(value +'; expires=')+(expireDate +'; secure;')        
        // document.cookie = `${key}=${value}; expires=${expireDate}; secure`; 
        console.log('here', document.cookie)       
    },
};

function add_years(dt, n) {
    return new Date(dt.setFullYear(dt.getFullYear() + n));
}
var today = new Date;
var oneYear = add_years(today, 1).toString();

const storageType = cookieStorage;
const shouldHide = function(cookieKey) {
    return !storageType.getItem(cookieKey)
};
const saveToStorage = function(cookieKey, value, expireDate) {
    return storageType.setItem(cookieKey, value, expireDate)
};

window.onload = function() {    
    var messageVisible = localStorage.getItem("messageVisible");
    if (messageVisible === "true") {
        if (cookiePageMessage != null) {
            cookiePageMessage.hidden = false;
        }
        localStorage.setItem("messageVisible", false);
    }
    else {
        if (cookiePageMessage != null) {
            cookiePageMessage.hidden = true;
        }
        localStorage.setItem("messageVisible", false);
    }    

    if (!shouldHide(acceptedPropertyName)) {
        // Accept Cookies button
        // Hides the main Cookie message, displays Cookies Accepted message
        cookieMessageToggle(cookieMessage)
        cookieMessageToggle(cookieMessageAccept)

        if (cookiesAnalyticsYesBtn != null) {
            cookiesAnalyticsYesBtn.checked = true;
        }
    }
    else if (!shouldHide(rejectedPropertyName)) {

        // Reject Cookies button
        // Hides the main Cookie message, displays Cookies Rejected message
        cookieMessageToggle(cookieMessage)
        cookieMessageToggle(cookieMessageReject)
        if (cookiesAnalyticsNoBtn != null) {
            cookiesAnalyticsNoBtn.checked = true;
        }
    }

    // Hide Cookie message buttons
    // Hides the entire Cookie Banner
    if (!shouldHide(msgHidePropertyName)) {
        cookieMessageToggle(cookieBanner)        
    }

    //If there is a 'Print this page' button on the page, display it    
    var printBtn = document.getElementById('printPage')
    if (printBtn) {
        printBtn.classList.remove('hsm-hidden');
    }

    printPage = function () {
        window.print();
    }

}
