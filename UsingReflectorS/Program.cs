using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReflectorS;

namespace UsingReflectorS {

	class Address {
		public string Street { get; set; }
		public string City { get; set; }
	}

	class Country {
		public string Name { get; set; }
		public int AreaCode { get; set; }
	}

	class PhoneNumber {
		public Country Country { get; set; }
		public int Number { get; set; }
	}

	class Person {
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public Address HomeAddress { get; set; }
		public Address WorkAddress { get; set; }
		public List<Person> Employees { get; } = new List<Person>();
		public Country CitizenOf { get; set; }
		public PhoneNumber MobilePhone { get; set; }
	}

	class Program {
		static void Main(string[] args) {
			var czechRepublic = new Country { Name = "Czech Republic", AreaCode = 420 };
			var person = new Person {
				FirstName = "Pavel",
				LastName = "Jezek",
				HomeAddress = new Address { Street = "Patkova", City = "Prague" },
				WorkAddress = new Address { Street = "Malostranske namesti", City = "Prague" },
				CitizenOf = czechRepublic,
				MobilePhone = new PhoneNumber { Country = czechRepublic, Number = 123456789 },
				Employees = {
					new Person {
						FirstName = "Jiri",
						LastName = "Vesely",
						HomeAddress = new Address { Street = "Ctvrtkova", City = "Prague" },
						WorkAddress = new Address { Street = "Dvorakova", City = "Horni Slavkov" },
						CitizenOf = czechRepublic,
						MobilePhone = new PhoneNumber { Country = czechRepublic, Number = 111222333 }
					},
					new Person {
						FirstName = "Jan",
						LastName = "Kofron",
						HomeAddress = new Address { Street = "Stredecni", City = "Beroun" },
						WorkAddress = new Address { Street = "Husitska", City = "Brno" },
						CitizenOf = czechRepublic,
						MobilePhone = new PhoneNumber { Country = czechRepublic, Number = 777666555 }
					},
				}
			};

			var serializer = new Serializer();

			Console.WriteLine("SINGLE INT:");
			serializer.Serialize(Console.Out, 42);
			Console.WriteLine();

			// FOLLOWING DOES NOT HAVE TO BE SUPPORTED!!!
			// Console.WriteLine("SINGLE STRING:");
			// serializer.Serialize(Console.Out, "Hello");

			Console.WriteLine("DEFAULT INDENT:");
			serializer.Serialize(Console.Out, person);
			Console.WriteLine();

			Console.WriteLine("INDENT BY 4:");
			serializer.IndentSpaceCount = 4;
			serializer.Serialize(Console.Out, person);
			Console.WriteLine();
			
			Console.WriteLine("INDENT BY 2 + STRING AS ENUMERABLE:");
			serializer.IndentSpaceCount = 2;
			serializer.TreatStringAsEnumerable = true;
			serializer.Serialize(Console.Out, person);
			Console.WriteLine();
		}
	}
}
