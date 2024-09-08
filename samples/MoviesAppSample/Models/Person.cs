using System.Diagnostics;

namespace MoviesAppSample.Models
{
    [DebuggerDisplay("Id={Id}, Name={Name}")]
    public class Person
    {
        public long Id { get; set; }
        public required string Name { get; set; }
    }

    public static class PersonExtensions
    {
        public static PersonDto ToDto(this Person person)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(person);
#else
            Throw.IfNull(person);
#endif
            return new PersonDto(person.Id, person.Name);
        }
    }
}
