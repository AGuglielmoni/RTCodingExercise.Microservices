
namespace PlateLogic
{
    public class PlateHelper : IPlateHelper
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlateHelper> _logger;

        public PlateHelper(ApplicationDbContext context, ILogger<PlateHelper> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PlateDto>> GetPlatesAsync(int pageNumber, int pageSize, string? sortOrder = null, string? filter = null, bool? isForSale = null)
        {
            try
            {
                IQueryable<Plate> query = _context.Plates.AsQueryable();

                if (isForSale.HasValue)
                {
                    query = query.Where(p => p.IsForSale == isForSale.Value);
                }

                if (!string.IsNullOrEmpty(filter))
                {
                    query = query.Where(p => p.Registration.Contains(filter));
                }

                if (sortOrder == "asc")
                {
                    query = query.OrderBy(p => p.SalePrice);
                }
                else if (sortOrder == "desc")
                {
                    query = query.OrderByDescending(p => p.SalePrice);
                }

                var plates = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

                return plates.Select(p => new PlateDto
                {
                    Registration = p.Registration,
                    PurchasePrice = p.PurchasePrice,
                    SalePrice = p.SalePrice
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching plates.");
                throw;
            }
        }

        public async Task<PlateDto> AddPlateAsync(PlateDto plateDto)
        {
            try
            {
                var newPlate = new Plate
                {
                    Id = Guid.NewGuid(),
                    Registration = plateDto.Registration,
                    PurchasePrice = plateDto.PurchasePrice,
                    SalePrice = plateDto.SalePrice
                };

                _context.Plates.Add(newPlate);
                await _context.SaveChangesAsync();

                return new PlateDto
                {
                    Registration = newPlate.Registration,
                    PurchasePrice = newPlate.PurchasePrice,
                    SalePrice = newPlate.SalePrice
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a new plate.");
                throw;
            }
        }

        public async Task<List<PlateDto>> ApplyMarkupAsync()
        {
            try
            {
                var plates = await _context.Plates.ToListAsync();
                foreach (var plate in plates)
                {
                    plate.SalePrice = plate.PurchasePrice * 1.20m;
                }

                await _context.SaveChangesAsync();

                return plates.Select(p => new PlateDto
                {
                    Registration = p.Registration,
                    PurchasePrice = p.PurchasePrice,
                    SalePrice = p.SalePrice
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying markup to plates.");
                throw;
            }
        }

        public async Task<bool> MarkPlateAsSoldAsync(Guid plateId)
        {
            try
            {
                var plate = await _context.Plates.FirstOrDefaultAsync(p => p.Id == plateId);

                if (plate == null)
                {
                    _logger.LogWarning("Attempted to mark a plate as sold, but the plate with ID {PlateId} was not found.", plateId);
                    return false;
                }

                plate.IsForSale = false;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while marking a plate as sold.");
                throw;
            }
        }
    }
}
