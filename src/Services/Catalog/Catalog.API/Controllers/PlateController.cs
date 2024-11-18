using PlateLogic;

namespace Catalog.API.Controllers
{
    public class PlatesController : Controller
    {
        private readonly IPlateHelper _plateHelper;

        public PlatesController(IPlateHelper plateHelper)
        {
            _plateHelper = plateHelper;
        }

        public async Task<IActionResult> GetPlates(int pageNumber = 1, int pageSize = 10, string sortOrder = null, string filterString = null, bool? isForSale = null)
        {
            var plates = await _plateHelper.GetPlatesAsync(pageNumber, pageSize, sortOrder, filterString, isForSale);

            return Json(plates);
        }

        [HttpPost]
        public async Task<IActionResult> AddPlate([FromBody] PlateDto plateDto)
        {
            if (plateDto == null)
            {
                return BadRequest("Plate data is required.");
            }

            var createdPlate = await _plateHelper.AddPlateAsync(plateDto);

            return Json(createdPlate);
        }

        [HttpPut("MarkAsSold/{id}")]
        public async Task<IActionResult> MarkPlateAsSold(Guid id)
        {
            var result = await _plateHelper.MarkPlateAsSoldAsync(id);

            if (!result)
            {
                return NotFound("Plate not found.");
            }

            return NoContent();
        }

        public async Task<IActionResult> GetPlateById(Guid id)
        {

            var plate = await _plateHelper.GetPlatesAsync(1, 1);  

            return Json(plate);
        }
    }
}
