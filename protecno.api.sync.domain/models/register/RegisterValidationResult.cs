using FluentValidation.Results;
using protecno.api.sync.domain.entities;
using ServiceStack.Messaging;
using System.Collections.Generic;

namespace protecno.api.sync.domain.models.register
{
    public class RegisterValidationResult: ValidationResult
    {
        public string Message { get; set; }

        public Register RegisterObject { get; set; }
    }
}
