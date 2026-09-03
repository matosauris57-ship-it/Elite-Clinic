namespace Clinic_System.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
       where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators,
            ILogger<ValidationBehavior<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Validating command {CommandType}", typeof(TRequest).Name);

            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

                if (failures.Count != 0)
                {
                    var messages = failures.Select(x => x.PropertyName + ": " + x.ErrorMessage).ToList();

                    _logger.LogWarning("Validation errors - {CommandType} - Errors: {@ValidationErrors}",
                        typeof(TRequest).Name, messages);

                    if (TryCreateValidationResponse(messages, out var validationResponse))
                        return validationResponse;

                    throw new ApiException("Validation Failed", 400, messages);
                }
            }

            _logger.LogInformation("Validation successful for command {CommandType}", typeof(TRequest).Name);
            return await next();
        }

        private static bool TryCreateValidationResponse(List<string> messages, out TResponse response)
        {
            response = default!;

            var responseType = typeof(TResponse);
            if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Response<>))
                return false;

            var dataType = responseType.GetGenericArguments()[0];
            var handler = new ResponseHandler();
            var method = typeof(ResponseHandler).GetMethod(nameof(ResponseHandler.BadRequest))!;
            var genericMethod = method.MakeGenericMethod(dataType);
            var result = genericMethod.Invoke(handler, new object?[] { "Validation Failed" });

            if (result == null)
                return false;

            var errorsProperty = responseType.GetProperty(nameof(Response<object>.Errors));
            errorsProperty?.SetValue(result, messages);

            response = (TResponse)result;
            return true;
        }
    }
}

/*
 {
  "fullName": "string",
  "gender": "Male",
  "dateOfBirth": "2000-02-09",
  "phone": "123",
  "address": "Mansora",
  "specialization": "string",
  "userName": "1doma",
  "email": "adham#g.c",
  "password": "doma.drr",
  "confirmPassword": "doma.drd"
}
 */