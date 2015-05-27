using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions;
using JSONAPI.Core;
using JSONAPI.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class SingleResultResponseTests
    {
        private class City
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public Country Country { get; set; }
        }

        private class Country
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public ICollection<City> Cities { get; set; } 

            public Continent Continent { get; set; }
        }

        private class Continent
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public ICollection<Country> Countries { get; set; }
        }

        private City _newYork, _losAngeles, _london, _paris;
        private Country _usa, _uk, _france;
        private Continent _northAmerica, _europe;

        [TestInitialize]
        public void BeforeEach()
        {
            _northAmerica = new Continent { Id = "31", Name = "North America" };
            _europe = new Continent { Id = "32", Name = "Europe" };

            _usa = new Country { Id = "21", Name = "USA", Continent = _northAmerica };
            _uk = new Country { Id = "22", Name = "UK", Continent = _europe };
            _france = new Country { Id = "23", Name = "France", Continent = _europe };

            _newYork = new City { Id = "11", Name = "New York", Country = _usa };
            _losAngeles = new City { Id = "12", Name = "Los Angeles", Country = _usa };
            _london = new City { Id = "13", Name = "London", Country = _uk };
            _paris = new City { Id = "14", Name = "Paris", Country = _france };

            _northAmerica.Countries = new Collection<Country>(new [] { _usa });
            _europe.Countries = new Collection<Country>(new [] { _uk, _france });

            _usa.Cities = new Collection<City>(new [] { _newYork, _losAngeles });
            _uk.Cities = new Collection<City>(new [] { _london });
            _france.Cities = new Collection<City>(new [] { _paris });
        }

        [TestMethod]
        public void SingleResultResponse_Resolve_works_with_no_inclusion()
        {
            // Arrange
            var pluralization = new PluralizationService();
            pluralization.AddMapping("city", "cities");
            pluralization.AddMapping("country", "countries");
            var modelManager = new ModelManager(pluralization);
            modelManager.RegisterResourceType(typeof(Continent));
            modelManager.RegisterResourceType(typeof(Country));
            modelManager.RegisterResourceType(typeof(City));

            // Act
            var response = new SingleResultResponse<Continent>(_northAmerica, null, modelManager);
            var payload = response.Resolve().Result;

            // Assert
            var primaryData = payload.PrimaryData.ToArray();
            primaryData.Length.Should().Be(1);

            var northAmericaResourceObject = primaryData[0];
            northAmericaResourceObject.Id.Should().Be("31");
            northAmericaResourceObject.ResourceType.Should().Be(typeof(Continent));
            northAmericaResourceObject.Relationships.Count.Should().Be(1);
            northAmericaResourceObject.Relationships["countries"].ShouldBeEquivalentTo(new ToManyRelationship("/continents/31/countries", "/continents/31/links/countries"));

            payload.RelatedData.Count.Should().Be(0, "No related data should have been specified.");
        }

        [TestMethod]
        public void SingleResultResponse_Resolve_works_with_inclusion()
        {
            // Arrange
            var pluralization = new PluralizationService();
            pluralization.AddMapping("city", "cities");
            pluralization.AddMapping("country", "countries");
            var modelManager = new ModelManager(pluralization);
            modelManager.RegisterResourceType(typeof(Continent));
            modelManager.RegisterResourceType(typeof(Country));
            modelManager.RegisterResourceType(typeof(City));

            // Act
            var response = new SingleResultResponse<Continent>(_northAmerica, null, modelManager);
            response.Include<Continent>(c => c.Countries);
            var payload = response.Resolve().Result;

            // Assert
            var primaryData = payload.PrimaryData.ToArray();
            primaryData.Length.Should().Be(1);

            var northAmericaResourceObject = primaryData[0];
            northAmericaResourceObject.Id.Should().Be("31");
            northAmericaResourceObject.ResourceType.Should().Be(typeof(Continent));
            northAmericaResourceObject.Relationships.Count.Should().Be(1);
            var expectedLinkage = new[]
            {
                Tuple.Create(typeof (Country), "21")
            };
            var expectedCountriesRelationship = new ToManyRelationship(expectedLinkage, "/continents/31/countries",
                "/continents/31/links/countries");
            var actualCountriesRelationship = northAmericaResourceObject.Relationships["countries"];
            actualCountriesRelationship.Should().BeOfType<ToManyRelationship>();
            actualCountriesRelationship.ShouldBeEquivalentTo(expectedCountriesRelationship);

            payload.RelatedData.Count.Should().Be(1, "Countries are included");

            var usa = payload.RelatedData[0];
            usa.Id.Should().Be("21");
            usa.ResourceType.Should().Be(typeof(Country));
            usa.Relationships.Count.Should().Be(2);
            usa.Relationships["continent"].ShouldBeEquivalentTo(new ToOneRelationship("/countries/21/continent", "/countries/21/links/continent"));
        }
    }
}
