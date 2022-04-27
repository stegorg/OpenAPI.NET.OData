﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class RefDeleteOperationHandlerTests
    {
        private RefDeleteOperationHandler _operationHandler = new RefDeleteOperationHandler();

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void CreateNavigationRefDeleteOperationReturnsCorrectOperation(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                UseHTTPStatusCodeClass2XX = useHTTPStatusCodeClass2XX
            };
            ODataContext context = new ODataContext(model, settings);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "Trips");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(people),
                new ODataKeySegment(people.EntityType()),
                new ODataNavigationPropertySegment(navProperty),
                ODataRefSegment.Instance);

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Delete a trip.", operation.Summary);
            Assert.Equal("Delete an instance of a trip.", operation.Description);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Trip", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.NotEmpty(operation.Parameters);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "204";
            Assert.Equal(new string[] { statusCode, "default" }, operation.Responses.Select(e => e.Key));

            if (enableOperationId)
            {
                Assert.Equal("People.DeleteRefTrips", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }
    }
}
