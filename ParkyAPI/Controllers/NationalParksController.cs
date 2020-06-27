using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkyAPI.Models;
using ParkyAPI.Models.Dtos;
using ParkyAPI.Repository.IRepository;

namespace ParkyAPI.Controllers
{
    [Route("api/v{version:apiVersion}/nationalparks")]
    //[Route("api/[controller]")]
    [ApiController]
    //[ApiExplorerSettings(GroupName = "ParkyOpenAPISpecNP")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class NationalParksController : ControllerBase
    {
        private readonly INationalParkRepository _npRepo;
        private readonly IMapper _mapper;

        public NationalParksController(INationalParkRepository npRepo, IMapper mapper)
        {
            _npRepo = npRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Get list of national parks
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        [ProducesResponseType(200,Type =typeof(List<NationalParkDto>))]
        public IActionResult GetNationalParks()
        {
            var objList = _npRepo.GetNationalParks();
            var objOfDto = new List<NationalParkDto>();
            foreach (var obj in objList)
            {
                objOfDto.Add(_mapper.Map<NationalParkDto>(obj));
            }
            return Ok(objOfDto);
        }

        /// <summary>
        /// Get individual national park
        /// </summary>
        /// <param name="nationalParkId">The id of the national park</param>
        /// <returns></returns>
        [HttpGet("{nationalParkId:int}", Name = "GetNationalPark")]
        [ProducesResponseType(200, Type = typeof(NationalParkDto))]
        [ProducesResponseType(403)]
        [ProducesDefaultResponseType]
        [Authorize]
        public IActionResult GetNationalPark(int nationalParkId)
        {
            var obj = _npRepo.GetNationalPark(nationalParkId);
            if (obj == null) return NotFound();
            var objDto = _mapper.Map<NationalParkDto>(obj);
            return Ok(objDto);
        }

        /// <summary>
        /// Creates a national park
        /// </summary>
        /// <param name="nationalParkDto">The new national park info inside body</param>
        /// <returns></returns>

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(NationalParkDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize]
        public IActionResult CreateNationalPark([FromBody] NationalParkDto nationalParkDto)
        {
            if (nationalParkDto == null)
            {
                return BadRequest(ModelState);
            }

            if (_npRepo.NationalParkExists(nationalParkDto.Name))
            {
                ModelState.AddModelError("", "National park exists!");
                return StatusCode(404, ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nationalParkObj = _mapper.Map<NationalPark>(nationalParkDto);
            if (!_npRepo.CreateNationalPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Unable to save. Something went wrong when saving {nationalParkDto.Name}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetNationalPark", new { version = HttpContext.GetRequestedApiVersion().ToString(), nationalParkId = nationalParkObj.Id }, nationalParkObj);

        }

        /// <summary>
        /// Updates a national park
        /// </summary>
        /// <param name="nationalParkId">The id of the national park</param>
        /// <param name="nationalParkDto">The national park info inside body</param>
        /// <returns></returns>
        [HttpPatch("{nationalParkId:int}", Name = "UpdateNationalPark")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateNationalPark(int nationalParkId, [FromBody] NationalParkDto nationalParkDto)
        {
            if (nationalParkDto == null || nationalParkId != nationalParkDto.Id)
            {
                return BadRequest(ModelState);
            }

            if (!_npRepo.NationalParkExists(nationalParkId))
            {
                return NotFound();
            }

            var nationalParkObj = _mapper.Map<NationalPark>(nationalParkDto);
            if (!_npRepo.UpdateNationalPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Unable to update. Something went wrong when updating {nationalParkDto.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a national parrk
        /// </summary>
        /// <param name="nationalParkId">The national park id for deletion</param>
        /// <returns></returns>
        [HttpDelete("{nationalParkId:int}", Name = "DeleteNationalPark")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteNationalPark(int nationalParkId)
        {

            if (!_npRepo.NationalParkExists(nationalParkId))
            {

                return NotFound();
            }

            var nationalParkObj = _npRepo.GetNationalPark(nationalParkId);

            if (!_npRepo.DeleteNationalPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Unable to delete. Something went wrong when deleting {nationalParkObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

    }
}