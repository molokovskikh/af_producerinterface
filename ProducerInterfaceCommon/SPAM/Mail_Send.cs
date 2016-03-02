using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.SPAM
{
    public class Mail_Send
    {
        private void SendEmail(string to, string subject, string body, string path, bool HtmlBody = false)
        {

        }

        public void SendEmail(List<string> to, string subject, string body, string path)
        {
            foreach (var toItem in to)
            {
                SendEmail(toItem, subject, body, path);
            }
        }

        private ProducerInterfaceCommon.ContextModels.producerinterface_Entities cntx__ = new ContextModels.producerinterface_Entities();

        private List<string> GetSpamMailList(Enums.TypeSpam typeSpam)
        {
            int TS_ID = (int)typeSpam;

            List<string> SpamMailList = new List<string>();

            switch (TS_ID)
            {
                case (int)Enums.TypeSpam.All:
                    break;
                case (int)Enums.TypeSpam.Reports:
                    break;
                case (int)Enums.TypeSpam.CustomReports:
                    break;
                case (int)Enums.TypeSpam.Promotion:
                    break;
                case (int)Enums.TypeSpam.CustomPromotions:
                    break;
                case (int)Enums.TypeSpam.News:
                    break;
                case (int)Enums.TypeSpam.CustomNews:
                    break;
                case (int)Enums.TypeSpam.Registration:
                    break;
                case (int)Enums.TypeSpam.FeedBack:
                    break;
                case (int)Enums.TypeSpam.CustomFeedBack:
                    break;
                case (int)Enums.TypeSpam.Producer:
                    break;
                case (int)Enums.TypeSpam.Drug:
                    break;
            }

            return SpamMailList.ToList().Select(v => v.ToLower()).GroupBy(x => x).Select(x => x.First()).ToList();
        }
    }











}

