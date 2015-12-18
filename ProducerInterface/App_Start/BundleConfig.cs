using System.Web;
using System.Web.Optimization;

namespace ProducerInterface
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {     
            bundles.Add(new StyleBundle("~/Assets/css").Include(
"~/Assets/css/bootstrap.css",
"~/Assets/css/font-awesome.min.css",
"~/Assets/css/jslider.css",
"~/Assets/css/settings.css",
"~/Assets/css/jquery.fancybox.css",
"~/Assets/css/animate.css",
"~/Assets/css/video-js.min.css",
"~/Assets/css/morris.css",
"~/Assets/css/royalslider/royalslider.css",
"~/Assets/css/royalslider/skins/minimal-white/rs-minimal-white.css",
"~/Assets/css/layerslider/layerslider.css",
"~/Assets/css/ladda.min.css",
"~/Assets/css/datepicker.css",
"~/Assets/css/jquery.scrollbar.css",
"~/Assets/css/style.css",
"~/Assets/css/producer.css",
"~/Assets/css/customizer/pages.css",
"~/Assets/css/customizer/home-pages-customizer.css",
"~/Assets/css/ie/ie.css",
"~/Assets/css/bootstrap.css"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jqCron").Include(
                            "~/Content/js/jqCron.js",
                            "~/Content/js/jqCron.ru.js",
                            "~/Content/js/chosen.jquery.js",
                            "~/Content/js/bootstrap-datepicker.js",
                            //"~/Scripts/ajax-chosen.js",
                            "~/Scripts/bootstrap-datepicker.js"                            
                            //"~/Scripts/init.js"
                            ));
            bundles.Add(new StyleBundle("~/Content/css").Include(
                            "~/Content/css/bootstrap.css",
                            "~/Content/css/site.css",
                            "~/Content/css/chosen.css",
                            "~/Content/datepicker.css",
                            "~/Content/css/jqCron.css"));

            bundles.Add(new ScriptBundle("~/Assets/js").Include(
"~/Assets/js/price-regulator/jshashtable-2.1_src.js",
"~/Assets/js/price-regulator/jquery.numberformatter-1.2.3.js",
"~/Assets/js/price-regulator/tmpl.js",
"~/Assets/js/price-regulator/jquery.dependClass-0.1.js",
"~/Assets/js/price-regulator/draggable-0.1.js",
"~/Assets/js/price-regulator/jquery.slider.js",
"~/Assets/js/jquery.carouFredSel-6.2.1-packed.js",
"~/Assets/js/jquery.touchwipe.min.js",
"~/Assets/js/jquery.elevateZoom-3.0.8.min.js",
"~/Assets/js/jquery.imagesloaded.min.js",
"~/Assets/js/jquery.appear.js",
"~/Assets/js/jquery.sparkline.min.js",
"~/Assets/js/jquery.easypiechart.min.js",
"~/Assets/js/jquery.easing.1.3.js",
"~/Assets/js/jquery.knob.js",
"~/Assets/js/jquery.selectBox.min.js",
"~/Assets/js/jquery.royalslider.min.js",
"~/Assets/js/jquery.tubular.1.0.js",
"~/Assets/js/SmoothScroll.js",
"~/Assets/js/country.js",
"~/Assets/js/spin.min.js",
"~/Assets/js/ladda.min.js",
"~/Assets/js/masonry.pkgd.min.js",
"~/Assets/js/morris.min.js",
"~/Assets/js/raphael.min.js",
"~/Assets/js/video.js",
"~/Assets/js/pixastic.custom.js",
"~/Assets/js/livicons-1.4.min.js",
"~/Assets/js/layerslider/greensock.js",
"~/Assets/js/layerslider/layerslider.transitions.js",
"~/Assets/js/layerslider/layerslider.kreaturamedia.jquery.js",
"~/Assets/js/revolution/jquery.themepunch.tools.min.js",
"~/Assets/js/revolution/jquery.themepunch.revolution.min.js",
"~/Assets/js/bootstrapValidator.min.js",
"~/Assets/js/bootstrap-datepicker.js",
"~/Assets/js/jplayer/jquery.jplayer.min.js",
"~/Assets/js/jplayer/jplayer.playlist.min.js",
"~/Assets/js/jquery.scrollbar.min.js"
                ));
        }
    }
}