using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using JSONAPI.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class JsonApiResponseBaseTests
    {
        // ReSharper disable ClassNeverInstantiated.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class Foo
        {
            public string Bar { get; set; }

            public Baz TheBaz { get; set; }

            public ICollection<Qux> TheQuxes { get; set; } 
        }

        private class Baz
        {
            public string Hello { get; set; }

            public Baz TheBaz { get; set; }

            public Foo TheFoo { get; set; }

            public ICollection<Qux> TheQuxes { get; set; }
        }

        private class Qux
        {
            public Baz TheBaz { get; set; }

            public ICollection<Foo> TheFoos { get; set; }

            public ICollection<Qux> TheQuxes { get; set; }
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
        // ReSharper restore UnusedMember.Local
        // ReSharper restore ClassNeverInstantiated.Local

        [TestMethod]
        public void ShouldExpand_returns_false_for_null_inclusion_path()
        {
            // Arrange
            const string currentPath = "Bar";
            const string inclusionPath = null;

            // Act
            var shouldExpand = GenericJsonApiResponse.ShouldExpand(currentPath, inclusionPath);

            // Assert
            shouldExpand.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldExpand_returns_false_for_empty_inclusion_path()
        {
            // Arrange
            const string currentPath = "Bar";
            const string inclusionPath = "";

            // Act
            var shouldExpand = GenericJsonApiResponse.ShouldExpand(currentPath, inclusionPath);

            // Assert
            shouldExpand.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldExpand_returns_true_for_match_on_single_property()
        {
            // Arrange
            const string currentPath = "Bar";
            const string inclusionPath = "Bar";

            // Act
            var shouldExpand = GenericJsonApiResponse.ShouldExpand(currentPath, inclusionPath);

            // Assert
            shouldExpand.Should().BeTrue();
        }

        [TestMethod]
        public void ShouldExpand_returns_false_when_first_item_is_off()
        {
            // Arrange
            const string currentPath = "Bar";
            const string inclusionPath = "Baz";

            // Act
            var shouldExpand = GenericJsonApiResponse.ShouldExpand(currentPath, inclusionPath);

            // Assert
            shouldExpand.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldExpand_returns_true_for_match_on_2_properties()
        {
            // Arrange
            const string currentPath = "TheBaz.TheQuxes";
            const string inclusionPath = "TheBaz.TheQuxes";

            // Act
            var shouldExpand = GenericJsonApiResponse.ShouldExpand(currentPath, inclusionPath);

            // Assert
            shouldExpand.Should().BeTrue();
        }

        [TestMethod]
        public void ShouldExpand_returns_true_when_inclusion_path_includes_current_path()
        {
            // Arrange
            const string currentPath = "TheBaz";
            const string inclusionPath = "TheBaz.TheQuxes";

            // Act
            var shouldExpand = GenericJsonApiResponse.ShouldExpand(currentPath, inclusionPath);

            // Assert
            shouldExpand.Should().BeTrue();
        }

        [TestMethod]
        public void ShouldExpand_returns_false_for_property_with_same_name_but_different_path()
        {
            // Arrange
            const string currentPath = "TheQuxes";
            const string inclusionPath = "TheBaz.TheQuxes";

            // Act
            var shouldExpand = GenericJsonApiResponse.ShouldExpand(currentPath, inclusionPath);

            // Assert
            shouldExpand.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldExpand_returns_false_when_current_path_is_longer_than_inclusion_path()
        {
            // Arrange
            const string currentPath = "TheBaz.TheQuxes";
            const string inclusionPath = "TheBaz";

            // Act
            var shouldExpand = GenericJsonApiResponse.ShouldExpand(currentPath, inclusionPath);

            // Assert
            shouldExpand.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldExpand_returns_false_for_mismatch_on_2nd_depth_property()
        {
            // Arrange
            const string currentPath = "TheBaz.TheBaz";
            const string inclusionPath = "TheBaz.Qux";

            // Act
            var shouldExpand = GenericJsonApiResponse.ShouldExpand(currentPath, inclusionPath);

            // Assert
            shouldExpand.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldExpand_returns_false_when_second_item_in_inclustion_path_contains_second_item_in_current_path()
        {
            // Arrange
            const string currentPath = "TheBaz.TheBaz";
            const string inclusionPath = "TheBaz.The";

            // Act
            var shouldExpand = GenericJsonApiResponse.ShouldExpand(currentPath, inclusionPath);

            // Assert
            shouldExpand.Should().BeFalse();
        }

        [TestMethod]
        public void ConvertInclusionExpression_returns_correct_value_for_simple_property()
        {
            // Arrange
            Expression<Func<Foo, object>> lambda = foo => foo.Bar;

            // Act
            var actualPath = GenericJsonApiResponse.ConvertInclusionExpression(lambda);

            // Assert
            actualPath.Should().Be("Bar");
        }

        [TestMethod]
        public void ConvertInclusionExpression_returns_correct_value_for_1_deep_simple_property()
        {
            // Arrange
            Expression<Func<Foo, object>> lambda = foo => foo.TheBaz.TheQuxes;

            // Act
            var actualPath = GenericJsonApiResponse.ConvertInclusionExpression(lambda);

            // Assert
            actualPath.Should().Be("TheBaz.TheQuxes");
        }

        [TestMethod]
        public void ConvertInclusionExpression_returns_correct_value_for_2_deep_simple_properties()
        {
            // Arrange
            Expression<Func<Foo, object>> lambda = foo => foo.TheBaz.Hello;

            // Act
            var actualPath = GenericJsonApiResponse.ConvertInclusionExpression(lambda);

            // Assert
            actualPath.Should().Be("TheBaz.Hello");
        }

        [TestMethod]
        public void ConvertInclusionExpression_returns_correct_value_for_1_deep_collection_property()
        {
            // Arrange
            Expression<Func<Foo, object>> lambda = foo => foo.TheQuxes;

            // Act
            var actualPath = GenericJsonApiResponse.ConvertInclusionExpression(lambda);

            // Assert
            actualPath.Should().Be("TheQuxes");
        }

        [TestMethod]
        public void ConvertInclusionExpression_returns_correct_value_for_collection_property_then_simple_property()
        {
            // Arrange
            Expression<Func<Foo, object>> lambda = foo => foo.TheQuxes.Select(q => q.TheBaz);

            // Act
            var actualPath = GenericJsonApiResponse.ConvertInclusionExpression(lambda);

            // Assert
            actualPath.Should().Be("TheQuxes.TheBaz");
        }

        [TestMethod]
        public void ConvertInclusionExpression_returns_correct_value_for_collection_property_then_collection_property()
        {
            // Arrange
            Expression<Func<Foo, object>> lambda = foo => foo.TheQuxes.Select(q => q.TheFoos);

            // Act
            var actualPath = GenericJsonApiResponse.ConvertInclusionExpression(lambda);

            // Assert
            actualPath.Should().Be("TheQuxes.TheFoos");
        }

        [TestMethod]
        public void ConvertInclusionExpression_returns_correct_value_for_2_collection_properties_then_simple_property()
        {
            // Arrange
            Expression<Func<Foo, object>> lambda = foo => foo.TheQuxes.Select(q => q.TheFoos.Select(r => r.Bar));

            // Act
            var actualPath = GenericJsonApiResponse.ConvertInclusionExpression(lambda);

            // Assert
            actualPath.Should().Be("TheQuxes.TheFoos.Bar");
        }

        [TestMethod]
        public void ConvertInclusionExpression_returns_correct_value_for_3_simple_properties()
        {
            // Arrange
            Expression<Func<Foo, object>> lambda = foo => foo.TheBaz.TheFoo.Bar;

            // Act
            var actualPath = GenericJsonApiResponse.ConvertInclusionExpression(lambda);

            // Assert
            actualPath.Should().Be("TheBaz.TheFoo.Bar");
        }

        [TestMethod]
        public void ConvertInclusionExpression_returns_correct_value_for_3_simple_properties_then_2_collection_properties()
        {
            // Arrange
            Expression<Func<Foo, object>> lambda = foo => foo.TheBaz.TheFoo.TheBaz.TheQuxes;

            // Act
            var actualPath = GenericJsonApiResponse.ConvertInclusionExpression(lambda);

            // Assert
            actualPath.Should().Be("TheBaz.TheFoo.TheBaz.TheQuxes");
        }

        [TestMethod]
        public void ConvertInclusionExpression_returns_correct_value_for_3_simple_properties_then_2_collection_properties_then_1_simple_property_then_collection()
        {
            // Arrange
            Expression<Func<Foo, object>> lambda = foo => foo.TheBaz.TheFoo.TheBaz.TheQuxes.Select(q => q.TheBaz.TheQuxes);

            // Act
            var actualPath = GenericJsonApiResponse.ConvertInclusionExpression(lambda);

            // Assert
            actualPath.Should().Be("TheBaz.TheFoo.TheBaz.TheQuxes.TheBaz.TheQuxes");
        }

        [TestMethod]
        public void ConvertInclusionExpression_throws_exception_if_call_on_simple_is_not_select()
        {
            // Arrange
            Expression<Func<Foo, object>> lambda = foo => foo.ToString();

            // Act
            Action action = () => GenericJsonApiResponse.ConvertInclusionExpression(lambda);

            // Assert
            action.ShouldThrow<ArgumentException>();
        }

        [TestMethod]
        public void ConvertInclusionExpression_throws_exception_if_call_on_collection_is_not_select()
        {
            // Arrange
            Expression<Func<Foo, object>> lambda = foo => foo.TheQuxes.Where(x => true);

            // Act
            Action action = () => GenericJsonApiResponse.ConvertInclusionExpression(lambda);

            // Assert
            action.ShouldThrow<ArgumentException>();
        }
    }
}
