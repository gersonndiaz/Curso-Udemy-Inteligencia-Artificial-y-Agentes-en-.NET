using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace ServerMCP.Prompts;

[McpServerPromptType]
public class PersonPrompts
{
    [McpServerPrompt, Description("Prompt para consultar todas las personas")]
    public static ChatMessage GetAll()
    => new(
        ChatRole.User,
        """
        Obtén el listado completo de personas usando la tool disponible.
        Luego presenta la información en español de forma clara y resumida.
        """
    );

    [McpServerPrompt, Description("Prompt para consultar una persona por id.")]
    public static ChatMessage GetById([Description("Id de la persona a consultar.")] int id)
    => new(
        ChatRole.User,
        $"""
        Busca la persona con id {id} usando la tool disponible.

        Si existe:
        - Muestra sus datos en español
        - Indica si está activa o no.

        Si no existe:
        - Indícalo claramente.
        """
    );

    [McpServerPrompt, Description("Prompt para activar una persona.")]
    public static ChatMessage ActivePerson([Description("Id de la persona.")] int id)
    => new(
        ChatRole.User,
        $"""
        Activa la persona con id {id} usando la tool disponible.
        Debes enviar active = true.

        Luego explica en español si la operación fue o no exitosa.
        """
    );

    [McpServerPrompt, Description("Prompt para desactivar una persona.")]
    public static ChatMessage InactivePerson([Description("Id de la persona.")] int id)
    => new(
        ChatRole.User,
        $"""
        Desactiva la persona con id {id} usando la tool disponible.
        Debes enviar active = false.

        Luego explica en español si la operación fue o no exitosa.
        """
    );
}
