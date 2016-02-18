using System.Web;
using System.Threading;
using System.IO;

namespace ProducerInterfaceCommon.Controllers
{
    public class Account_Statistics
    {
        private HttpContextBase httpContext;
        private ProducerInterfaceCommon.ContextModels.Account user;
        public Account_Statistics(HttpContextBase httpContext, ProducerInterfaceCommon.ContextModels.Account user)
        {
            this.httpContext = httpContext;
            this.user = user;
        }

        public void SaveAccountAsync()
        {

            // На стадии разработки

            //if (user != null)
            //{
            //    string[] lines = new string[] { user.Name , user.ID_LOG.ToString(), user.Login, httpContext.Request.Browser.Browser.ToString() , httpContext.Request.Browser.GatewayVersion, httpContext.Request.UserAgent, "**************" };
            //    System.IO.File.WriteAllLines(@"C:\Users\alegusov\desktop\WriteLines.txt", lines, System.Text.Encoding.UTF8);
            //}
            //else
            //{
            //    string[] lines = new string[] { httpContext.Request.Browser.Browser.ToString(), "**************" };
            //    System.IO.File.WriteAllLines(@"C:\Users\alegusov\desktop\WriteLines.txt", lines, System.Text.Encoding.UTF8);
            //}
        }
    }
}
