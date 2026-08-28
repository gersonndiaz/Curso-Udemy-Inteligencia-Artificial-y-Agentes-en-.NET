using ServerMCP.Entities;

namespace ServerMCP.Services;

public class PersonRepository : IPersonRepository
{
    private List<Person> _persons;

        public PersonRepository()
        {
            _persons = new List<Person>
        {
            new Person
            {
                Id = 1,
                Name = "Felipe Gavilán",
                Email = "Felipe.Gavilan@email.com",
                Salary = 50000,
                Active = true
            },
            new Person
            {
                Id = 2,
                Name = "Claudia Rodríguez",
                Email = "claudia.rodriguez@email.com",
                Salary = 65000,
                Active = true
            },
            new Person
            {
                Id = 3,
                Name = "Carlos Rodríguez",
                Email = "carlos.rodriguez@email.com",
                Salary = 45000,
                Active = false
            }
        };
        }


        public bool UpdateActive(int id, bool active)
        {
            var person = _persons.FirstOrDefault(p => p.Id == id);

            if (person is null)
            {
                return false;
            }

            person.Active = active;
            return true;
        }

        public Person GetById(int id)
        {
            return _persons.FirstOrDefault(p => p.Id == id);
        }

        public List<Person> GetAll()
        {
            return _persons;
        }
}
