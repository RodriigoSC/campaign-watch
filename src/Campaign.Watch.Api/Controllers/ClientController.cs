using Campaign.Watch.Application.Dtos;
using Campaign.Watch.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IClientApplication _clientApplication;

        public ClientController(IClientApplication clientApplication)
        {
            _clientApplication = clientApplication;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ClientDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateClient([FromBody] ClientInputDto clientDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdClient = await _clientApplication.CreateClientAsync(clientDto);
                return CreatedAtAction(nameof(GetClientById), new { id = createdClient.Id }, createdClient);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocorreu um erro interno ao criar o cliente.");
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClientDto>), 200)]
        public async Task<IActionResult> GetAllClients()
        {
            var clients = await _clientApplication.GetAllClientsAsync();
            return Ok(clients);
        }

        [HttpGet("{id}", Name = "GetClientById")]
        [ProducesResponseType(typeof(ClientDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetClientById(string id)
        {
            var client = await _clientApplication.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return Ok(client);
        }

        [HttpGet("by-name/{clientName}")]
        [ProducesResponseType(typeof(ClientDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetClientByName(string clientName)
        {
            var client = await _clientApplication.GetClientByNameAsync(clientName);
            if (client == null)
            {
                return NotFound($"Cliente com o nome '{clientName}' não encontrado.");
            }
            return Ok(client);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateClient(string id, [FromBody] ClientInputDto clientDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _clientApplication.UpdateClientAsync(id, clientDto);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteClient(string id)
        {
            var success = await _clientApplication.DeleteClientAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
