﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapp.DAL.Models
{
    public class Produk : BaseModel
    {
        public class PanenItem
        {
            public string id { get; set;}
            public string namaLokasi { get; set;}
            public DateTime? tanggalPanen { get; set;}
            public string jenisMadu { get; set;}
            public double berat { get; set; }
        }
        public string nama { get; set; }
        public DateTime? tanggal { get; set; }
        public string status { get; set; }
        public string idPetugasMixing { get; set; }
        public string namaPetugasMixing { get; set; }
        public string idAdmin { get; set; }
        public string namaAdmin { get; set; }
        public List<PanenItem> listPanen { get; set; } = new List<PanenItem>();

    }
}
