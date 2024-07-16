using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapp.DAL
{
    public class Constants
    {
        public enum PanenStatus
        {
            GENERATED, SUBMITTED, PIC_APPROVED, ARRIVED_WAREHOUSE, ADMIN_CONFIRMED
        }

        public enum ProdukStatus
        {
            GENERATED, SUBMITTED, ADMIN_APPROVED
        }
    }
}
