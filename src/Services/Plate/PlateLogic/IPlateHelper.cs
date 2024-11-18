using RTC.Data;

namespace PlateLogic
{
    public interface IPlateHelper
    {
        /// <summary>
        /// Retrieves a paginated and optionally filtered list of plates.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="sortOrder">Optional sort order: "asc" for ascending, "desc" for descending.</param>
        /// <param name="filterString">Optional filter string to search plate registrations.</param>
        /// <param name="isForSale">Optional filter to include only plates that are for sale.</param>
        /// <returns>A list of plates in the specified format.</returns>
        Task<List<PlateDto>> GetPlatesAsync(int pageNumber, int pageSize, string? sortOrder = null, string? filterString = null, bool? isForSale = null);

        /// <summary>
        /// Adds a new plate to the database.
        /// </summary>
        /// <param name="plateDto">The data transfer object containing plate details.</param>
        /// <returns>The added plate as a data transfer object.</returns>
        Task<PlateDto> AddPlateAsync(PlateDto plateDto);

        /// <summary>
        /// Applies a markup to all plates' sale prices.
        /// </summary>
        /// <returns>A list of plates with updated sale prices.</returns>
        Task<List<PlateDto>> ApplyMarkupAsync();

        /// <summary>
        /// Marks a specific plate as sold.
        /// </summary>
        /// <param name="plateId">The unique identifier of the plate to mark as sold.</param>
        /// <returns>True if the plate was successfully marked as sold, otherwise false.</returns>
        Task<bool> MarkPlateAsSoldAsync(Guid plateId);
    }
}
