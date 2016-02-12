using System.Linq;
using System.Web.Mvc;

namespace ProducerInterfaceCommon.Heap
{
    public class MyViewEngine : RazorViewEngine
    {
        public MyViewEngine()
        {
            var newLocationFormat = new[]
                                        {
                                        "~/Views/{1}/Partial/{0}.cshtml",
                                    };

            PartialViewLocationFormats = PartialViewLocationFormats.Union(newLocationFormat).ToArray();
        }

    }
}
