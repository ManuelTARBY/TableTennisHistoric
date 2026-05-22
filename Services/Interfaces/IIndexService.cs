using TableTennisHistoric.DTO;

namespace TableTennisHistoric.Services.Interfaces
{
    public interface IIndexService
    {
        Task<IndexDataDTO> GetIndexDataAsync(int seasonId);
        Task CreateSeasonAutomaticallyAsync();
    }
}
