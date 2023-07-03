﻿using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static FluentValidation.AssemblyScanner;

namespace Shared.Kernel.BuildingBlocks.ModelValidation
{
    public class ValidationService
    {
        private readonly List<AssemblyScanResult> assemblyScanResult;
        private readonly IServiceProvider serviceProvider;
        public ValidationService(IServiceProvider serviceProvider, Assembly assembly)
        {
            this.serviceProvider = serviceProvider;
            assemblyScanResult = new List<AssemblyScanResult>();
            assemblyScanResult.AddRange(FindValidatorsInAssembly(assembly));
        }

        public ValidationServiceResult Validate<T>(T model)
        {
            var validator = GetValidatorForModel(model);
            if (validator == null)
            {
                //throw new ValidationServiceException("No Validator is registerd");
            }
            ValidationResult validationResult = validator.Validate(new ValidationContext<T>(model));
            return new ValidationServiceResult
            {
                IsValid = validationResult.IsValid,
                Errors = validationResult.Errors.Select(s => s.ErrorMessage).ToList()
            };
        }

        public void ThrowIfInvalidModel<T>(T model)
        {
            var validator = GetValidatorForModel(model);
            if (validator == null)
            {
                //throw new ValidationServiceException("No Validator is registerd");
            }
            ValidationResult validationResult = validator.Validate(new ValidationContext<T>(model));
            if(validationResult.IsValid is false)
            {
                throw new Exception();
            }
        }

        private IValidator GetValidatorForModel(object model)
        {
            var interfaceValidatorType = typeof(IValidator<>).MakeGenericType(model.GetType());

            Type modelValidatorType = assemblyScanResult.FirstOrDefault(i => interfaceValidatorType.IsAssignableFrom(i.InterfaceType))?.ValidatorType;

            if (modelValidatorType == null)
            {
                return null;
            }

            return (IValidator)ActivatorUtilities.CreateInstance(serviceProvider, modelValidatorType);
        }
    }
}
