using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.SPAM
{
    public class Mail_Send
    {
        private Enums.TypeSpam TS;

        public Mail_Send(Enums.TypeSpam MailType)
        {
            this.TS = MailType;
        }




    }
}
