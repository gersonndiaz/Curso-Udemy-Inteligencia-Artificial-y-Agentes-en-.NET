========================================================================
CURSO: Inteligencia Artificial y Agentes en .NET
========================================================================

Este repositorio contiene varios proyectos de chatbot en .NET. A continuación,
se indican las configuraciones necesarias para su ejecución local.

------------------------------------------------------------------------
1. CONFIGURACIÓN DE VARIABLES DE ENTORNO (.env)
------------------------------------------------------------------------
Debido a que las credenciales, llaves de API y tokens no se suben al
repositorio (están protegidos y excluidos por las reglas de .gitignore),
deberás crear un archivo llamado `.env` en los directorios de los proyectos
que lo requieran.

Actualmente, el proyecto 'Chatbot003' requiere el archivo `.env` dentro de
su carpeta principal (Chatbot003/Chatbot003/.env).

Contenido de ejemplo para tu archivo `.env` (crear uno igual y rellenar):
--------------------------------------------------
OPENAI_LLAVE=
ANTHROPIC_LLAVE=
CLIMA_API_KEY=
--------------------------------------------------

* Nota: Coloca tus claves reales después del signo '=' sin comillas ni espacios.

------------------------------------------------------------------------
2. PROYECTOS INCLUIDOS
------------------------------------------------------------------------

* Chatbot001:
  - Integración básica con Ollama (local) usando el modelo 'qwen3.5:9b'.
  - Tiene una constante opcional para OpenAI en Chatbot001/Constantes.cs.

* Chatbot002:
  - Estructura para chatbot multi-proveedor. Configurado por defecto para
    usar Ollama de forma local.

* Chatbot003:
  - Integración avanzada con herramientas y llamadas a funciones (Tools / Function Calling).
  - Consume servicios externos (como WeatherService). Requiere que configures
    las variables del archivo `.env` mencionado arriba para poder conectarse
    a OpenAI, Anthropic o la API de clima.

------------------------------------------------------------------------
3. REQUISITOS GENERALES
------------------------------------------------------------------------
- .NET 10.0 SDK o superior instalado.
- Ollama instalado localmente con el modelo configurado (ej: qwen3.5:9b) si
  se ejecutan los chats locales en la dirección predeterminada: http://localhost:11434
