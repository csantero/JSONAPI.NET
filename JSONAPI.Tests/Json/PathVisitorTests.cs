using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;
using JSONAPI.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class PathVisitorTests
    {
        private class Foo
        {
            public string Baz { get; set; }

            public Bar Bar { get; set; }

            public ICollection<Qux> Quxes { get; set; }
        }

        private class Bar
        {
            public string Baz { get; set; }
        }

        private class Qux
        {
            public Bar Bar { get; set; }

            public string Hi { get; set; }
        }

        [TestMethod]
        public void Path_is_correct_for_simple_member()
        {
            // Arrange
            Expression<Func<Foo, object>> expr = foo => foo.Baz;
            var visitor = new PathVisitor();

            // Act
            visitor.Visit(expr);

            // Assert
            visitor.Path.Should().Be("Baz");
        }
    }
}
