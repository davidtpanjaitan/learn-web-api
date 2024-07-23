namespace david_api.DTO
{
    public class SubmitPanenDTO
    {
        public string jenisMadu { get; set; }
        public double beratPanen { get; set; }
        public DateTime tanggalPanen { get; set; }
        public string idPetugasPanen { get; set; }
        public string namaPetugasPanen { get; set; }

        public IFormFile gambar { get; set; }
    }
}
