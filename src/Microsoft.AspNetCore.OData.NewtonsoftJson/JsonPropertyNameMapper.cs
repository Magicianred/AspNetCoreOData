﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.OData.NewtonsoftJson
{
    /// <summary>
    /// Edm Property name mapper.
    /// </summary>
    public class JsonPropertyNameMapper : IPropertyMapper
    {
        private IEdmModel _model;
        private IEdmStructuredType _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertyNameMapper"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="type">The Edm structured type.</param>
        public JsonPropertyNameMapper(IEdmModel model, IEdmStructuredType type)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Map the given property name.
        /// </summary>
        /// <param name="propertyName">The given property name.</param>
        /// <returns>The mapped property name.</returns>
        public string MapProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            IEdmProperty property = _type.Properties().Single(s => s.Name == propertyName);
            PropertyInfo info = GetPropertyInfo(property);
            JsonPropertyAttribute jsonProperty = GetJsonProperty(info);
            if (jsonProperty != null && !string.IsNullOrWhiteSpace(jsonProperty.PropertyName))
            {
                return jsonProperty.PropertyName;
            }
            else
            {
                return property.Name;
            }
        }

        private PropertyInfo GetPropertyInfo(IEdmProperty property)
        {
            ClrPropertyInfoAnnotation clrPropertyAnnotation = _model.GetAnnotationValue<ClrPropertyInfoAnnotation>(property);
            if (clrPropertyAnnotation != null)
            {
                return clrPropertyAnnotation.ClrPropertyInfo;
            }

            ClrTypeAnnotation clrTypeAnnotation = _model.GetAnnotationValue<ClrTypeAnnotation>(property.DeclaringType);
            Contract.Assert(clrTypeAnnotation != null);

            PropertyInfo info = clrTypeAnnotation.ClrType.GetProperty(property.Name);
            Contract.Assert(info != null);

            return info;
        }

        private static JsonPropertyAttribute GetJsonProperty(PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(JsonPropertyAttribute), inherit: false)
                   .OfType<JsonPropertyAttribute>().SingleOrDefault();
        }
    }
}
