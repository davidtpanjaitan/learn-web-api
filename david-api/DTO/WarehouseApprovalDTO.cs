namespace david_api.DTO
{
    public class WarehouseApprovalDTO : ApprovalDTO
    {
        public string catatan { get; set; }
        public double beratBaru { get; set; }
        public string gambarWarehouseUrl { get; set; }
    }
}
