using ServerMCP.Entities;

namespace ServerMCP.Services;

public interface IPersonRepository
{
    bool UpdateActive(int id, bool active);
    Person GetById(int id);
    List<Person> GetAll();
}
