import $ from "jquery";
import persistentNav from '../persistentNav'
import Navigation from '../navigation'
import Popup from '../popUp'
import PopupCta from '../popUpCta'
import Footer from '../footer'
import Accordion from '../accordion'
// import '../../../../App_Plugins/UmbracoForms/Assets/themes/default/umbracoForms.js'
import 'jquery-validation'
import 'jquery-validation-unobtrusive'
import 'jquery-ajax-unobtrusive'
import Forms from '../forms'
import VideoBlock from '../videoBlock'
import ImageGallery from '../imageGallery'
import Print from '../print'
import UnsupportedBrowser from '../unsupportedBrowser'
import Scrollbar from '../scrollBar'
import AutoCompleteSearch from '../autoCompleteSearch'
import MegaNav from '../megaNav'
import SetScroll from '../setScroll'
import BackToTop from '../backToTop'
import RichTextTables from '../richtext-tables'

import externalLinks from '../externalLinks'

export default function Globals() {
    $()
    Navigation()
    persistentNav()
    Footer()
    Accordion()
    Forms()
    VideoBlock()
    ImageGallery()
    Print()
    UnsupportedBrowser()
    Popup()
    PopupCta()
    Scrollbar()
    AutoCompleteSearch()
    MegaNav()
    SetScroll()
    externalLinks();
    BackToTop();
    RichTextTables();
}