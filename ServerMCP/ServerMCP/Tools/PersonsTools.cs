using System.ComponentModel;
using ModelContextProtocol.Server;
using ServerMCP.DTOs;
using ServerMCP.Entities;
using ServerMCP.Services;

namespace ServerMCP.Tools;

[McpServerToolType]
public class PersonsTools(IPersonRepository personRepository)
{
    [McpServerTool, Description("Obtiene el listado de todas las personas registradas")]
    public List<Person> GetAll()
    {
        var persons = personRepository.GetAll();
        return persons;
    }

    [McpServerTool, Description("Obtiene una persona por su identificador")]
    public Person GetById([Description("")] int id)
    {
        var person = personRepository.GetById(id);
        return person;
    }

    [McpServerTool, Description("Actualiza o desactiva una persona según su identificador.")]
    public OperationResultDto UpdateActive(
        [Description("Identificador de la persona")] int id
        , [Description("Indica si la persona estará activa (true) o inactiva (false)")] bool active
    )
    {
        var updated = personRepository.UpdateActive(id, active);

        if (!updated)
        {
            return new OperationResultDto(false, $"No se pudo actualizar la persona con id {id}. Verifique que exista.");
        }

        return new OperationResultDto(true, $"La operación fue completada exitosamente.");
    }
}
