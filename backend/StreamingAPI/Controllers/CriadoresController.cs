using Microsoft.AspNetCore.Mvc;
using StreamingAPI.Data;
using StreamingAPI.DTOs;
using StreamingAPI.Models;
using Microsoft.EntityFrameworkCore;
using StreamingAPI.Repositories;

namespace StreamingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CriadoresController : ControllerBase
    {
        private readonly CriadorRepository _repository;

        public CriadoresController(CriadorRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CriadorDTO>>> GetAll()
        {
            var criadores = await _repository.GetAllAsync();
            var criadorDTOs = criadores.Select(MapToDTO).ToList();

            return Ok(criadorDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CriadorDTO>> GetById(int id)
        {
            var criador = await _repository.GetByIdAsync(id);
            if (criador == null)
                return NotFound();

            var criadorDTO = MapToDTO(criador);

            return Ok(criadorDTO);
        }

        private static CriadorDTO MapToDTO(Criador criador)
        {
            return new CriadorDTO
            {
                Id = criador.Id,
                Nome = criador.Nome
            };
        }

        [HttpPost]
        public async Task<ActionResult<CriadorDTO>> Create(CreateCriadorDTO createDTO)
        {
            var criador = new Criador
            {
                Nome = createDTO.Nome
            };

            await _repository.CreateAsync(criador);

            return CreatedAtAction(nameof(GetById), new { id = criador.Id }, MapToDTO(criador));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateCriadorDTO updateDTO)
        {
            var criador = await _repository.GetByIdAsync(id);
            if (criador == null)
                return NotFound();

            criador.Nome = updateDTO.Nome;

            await _repository.UpdateAsync(criador);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
