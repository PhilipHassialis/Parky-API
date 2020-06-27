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
    [Route("api/v{version:apiVersion}/trails")]
    //[Route("api/Trails")]
    [ApiController]
    //[ApiExplorerSettings(GroupName = "ParkyOpenAPISpecTrails")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class TrailsController : ControllerBase
    {
        private readonly ITrailRepository _trailRepo;
        private readonly IMapper _mapper;

        public TrailsController(ITrailRepository trailRepo, IMapper mapper)
        {
            _trailRepo = trailRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Get list of trails
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200,Type =typeof(List<TrailDto>))]
        public IActionResult GetTrails()
        {
            var objList = _trailRepo.GetTrails();
            var objOfDto = new List<TrailDto>();
            foreach (var obj in objList)
            {
                objOfDto.Add(_mapper.Map<TrailDto>(obj));
            }
            return Ok(objOfDto);
        }

        /// <summary>
        /// Get individual trail
        /// </summary>
        /// <param name="trailId">The id of the national park</param>
        /// <returns></returns>
        [HttpGet("{trailId:int}", Name = "GetTrail")]
        [ProducesResponseType(200, Type = typeof(TrailDto))]
        [ProducesResponseType(403)]
        [ProducesDefaultResponseType]
        [Authorize(Roles ="Admin")]
        public IActionResult GetTrail(int trailId)
        {
            var obj = _trailRepo.GetTrail(trailId);
            if (obj == null) return NotFound();
            var objDto = _mapper.Map<TrailDto>(obj);
            return Ok(objDto);
        }

        /// <summary>
        /// Gst all trails of a national park
        /// </summary>
        /// <param name="nationalParkId"></param>
        /// <returns></returns>
        [HttpGet("[action]/{nationalParkId:int}", Name = "GetTrailsInNationalPark")]
        [ProducesResponseType(200, Type = typeof(TrailDto))]
        [ProducesResponseType(403)]
        [ProducesDefaultResponseType]
        public IActionResult GetTrailsInNationalPark(int nationalParkId)
        {
            var objList = _trailRepo.GetTrailsInNationalPark(nationalParkId);
            if (objList == null || objList.Count()==0) return NotFound();
            var objDto = _mapper.Map<ICollection<TrailDto>>(objList);
            return Ok(objDto);
        }


        /// <summary>
        /// Creates a new trail
        /// </summary>
        /// <param name="trailDto">Trail information for creation</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TrailDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateTrail([FromBody] TrailCreateDto trailDto)
        {
            if (trailDto == null)
            {
                return BadRequest(ModelState);
            }

            if (_trailRepo.TrailExists(trailDto.Name))
            {
                ModelState.AddModelError("", "Trail exists!");
                return StatusCode(404, ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trailObj = _mapper.Map<Trail>(trailDto);
            if (!_trailRepo.CreateTrail(trailObj))
            {
                ModelState.AddModelError("", $"Unable to save. Something went wrong when saving {trailDto.Name}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetTrail", new { version=HttpContext.GetRequestedApiVersion().ToString(), trailId = trailObj.Id }, trailObj);

        }

        /// <summary>
        /// Updates a trail
        /// </summary>
        /// <param name="trailId">Trail to be updated</param>
        /// <param name="trailDto">Trail information to overwrite the old ones</param>
        /// <returns></returns>
        [HttpPatch("{trailId:int}", Name = "UpdateTrail")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateTrail(int trailId, [FromBody] TrailUpdateDto trailDto)
        {
            if (trailDto == null || trailId != trailDto.Id)
            {
                return BadRequest(ModelState);
            }

            if (!_trailRepo.TrailExists(trailId))
            {
                return NotFound();
            }

            var trailObj = _mapper.Map<Trail>(trailDto);
            if (!_trailRepo.UpdateTrail(trailObj))
            {
                ModelState.AddModelError("", $"Unable to update. Something went wrong when updating {trailDto.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a trail
        /// </summary>
        /// <param name="trailId">Trail id for deletion</param>
        /// <returns></returns>
        [HttpDelete("{trailId:int}", Name = "DeleteTrail")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteTrail(int trailId)
        {

            if (!_trailRepo.TrailExists(trailId))
            {

                return NotFound();
            }

            var trailObj = _trailRepo.GetTrail(trailId);

            if (!_trailRepo.DeleteTrail(trailObj))
            {
                ModelState.AddModelError("", $"Unable to delete. Something went wrong when deleting {trailObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

    }
}