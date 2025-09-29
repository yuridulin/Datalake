using Datalake.PublicApi.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Datalake.PrivateApi.Middlewares;

public class ErrorsMiddleware
{
	public static void ErrorHandler(IApplicationBuilder exceptionHandlerApp)
	{
		exceptionHandlerApp.Run(async context =>
		{
			var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
			var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
			var error = exceptionHandlerPathFeature?.Error;

			var problem = error switch
			{
				ValidationException validationException => CreateValidationProblem(validationException),
				UnauthenticatedException unauthenticatedException => CreateUnauthenticatedProblem(unauthenticatedException),
				ForbiddenException forbiddenException => CreateForbiddenProblem(forbiddenException),
				DatabaseException databaseException => CreateDatabaseProblem(databaseException),
				_ => CreateDefaultProblem(error, env)
			};

			context.Response.ContentType = "application/problem+json"; // Более правильный content type
			context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
			await context.Response.WriteAsJsonAsync(problem);
			return;
		});
	}

	private static ProblemDetails CreateValidationProblem(ValidationException exception)
	{
		return new ProblemDetails
		{
			Title = "Ошибка валидации",
			Status = StatusCodes.Status400BadRequest,
			Detail = "Некорректные данные в запросе",
			Extensions = {
				["errors"] = exception.Errors
					.GroupBy(e => e.PropertyName)
					.ToDictionary(
						g => g.Key,
						g => g.Select(e => e.ErrorMessage).ToArray()
					)
			}
		};
	}

	private static ProblemDetails CreateUnauthenticatedProblem(UnauthenticatedException exception)
	{
		return new ProblemDetails
		{
			Title = "Ошибка аутентификации",
			Status = StatusCodes.Status401Unauthorized,
			Detail = exception.Message,
		};
	}

	private static ProblemDetails CreateForbiddenProblem(ForbiddenException exception)
	{
		return new ProblemDetails
		{
			Title = "Доступ запрещен",
			Status = StatusCodes.Status403Forbidden,
			Detail = exception.Message,
		};
	}

	private static ProblemDetails CreateDatabaseProblem(DatabaseException exception)
	{
		return new ProblemDetails
		{
			Title = "Ошибка базы данных",
			Status = StatusCodes.Status500InternalServerError,
			Detail = exception.Message,
		};
	}

	private static ProblemDetails CreateDefaultProblem(Exception? exception, IWebHostEnvironment env)
	{
		var problem = new ProblemDetails
		{
			Title = "Внутренняя ошибка сервера",
			Status = StatusCodes.Status500InternalServerError,
			Detail = "Произошла непредвиденная ошибка",
			Extensions = {
				["debug"] = new
				{
					message = exception?.Message,
					type = exception?.GetType().Name,
					stackTrace = exception?.StackTrace
				}
			}
		};

		return problem;
	}
}
