using webapp.DAL.Models;

namespace webapp.DAL.DTO
{
    public class StatistikResult
    {
        public int LifetimeItemCount { get; set; }

        public List<Stats> MonthlyStats { get; set; } = new List<Stats>();

    }
}
