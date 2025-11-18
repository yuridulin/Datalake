using Datalake.Domain.Exceptions;
using Datalake.Shared.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Datalake.Shared.Hosting.Middlewares;

/// <summary>
/// Перехватчик для обработки ошибок
/// </summary>
public class SharedExceptionsMiddleware
{
	/// <summary>
	/// Перехватчик для обработки ошибок
	/// </summary>
	public static void Handler(IApplicationBuilder exceptionHandlerApp)
	{
		exceptionHandlerApp.Run(async context =>
		{
			var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
			var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
			var error = exceptionHandlerPathFeature?.Error;

			var problem = error switch
			{
				ValidationException ex => CreateValidationProblem(ex),
				UnauthenticatedException ex => CreateUnauthenticatedProblem(ex),
				UnauthorizedException ex => CreateUnauthorizedProblem(ex),
				NotFoundException ex => CreateNotFoundProblem(ex),
				ConflictException ex => CreateConfictProblem(ex),
				DomainException ex => CreateDomainProblem(ex),
				InfrastructureException ex => CreateInfrastructureProblem(ex),
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
			Extensions =
			{
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
			Status = StatusCodes.Status400BadRequest,
			Detail = exception.Message,
			Extensions =
			{
				[nameof(exception.Code)] = exception.Code,
			}
		};
	}

	private static ProblemDetails CreateUnauthorizedProblem(UnauthorizedException exception)
	{
		return new ProblemDetails
		{
			Title = "Ошибка доступа",
			Status = StatusCodes.Status401Unauthorized,
			Detail = exception.Message,
			Extensions =
			{
				[nameof(exception.Code)] = exception.Code,
			}
		};
	}

	private static ProblemDetails CreateConfictProblem(ConflictException exception)
	{
		return new ProblemDetails
		{
			Title = "Выполнение прекращено",
			Status = StatusCodes.Status400BadRequest,
			Detail = exception.Message,
			Extensions =
			{
				[nameof(exception.Code)] = exception.Code,
			}
		};
	}

	private static ProblemDetails CreateNotFoundProblem(NotFoundException exception)
	{
		return new ProblemDetails
		{
			Title = "Объект не найден",
			Status = StatusCodes.Status400BadRequest,
			Detail = exception.Message,
			Extensions =
			{
				[nameof(exception.Code)] = exception.Code,
			}
		};
	}

	private static ProblemDetails CreateDomainProblem(DomainException exception)
	{
		return new ProblemDetails
		{
			Title = "Выполнение прекращено",
			Status = StatusCodes.Status400BadRequest,
			Detail = exception.Message,
			Extensions =
			{
				[nameof(exception.Code)] = exception.Code,
			}
		};
	}

	private static ProblemDetails CreateInfrastructureProblem(InfrastructureException exception)
	{
		return new ProblemDetails
		{
			Title = "Ошибка инфраструктуры",
			Status = StatusCodes.Status500InternalServerError,
			Detail = exception.Message,
			Extensions =
			{
				[nameof(exception.Code)] = exception.Code,
			}
		};
	}

	private static ProblemDetails CreateDefaultProblem(Exception? exception, IWebHostEnvironment env)
	{
		var problem = new ProblemDetails
		{
			Title = "Внутренняя ошибка сервера",
			Status = StatusCodes.Status500InternalServerError,
			Detail = "Произошла непредвиденная ошибка",
		};

		if (env.IsDevelopment())
		{
			problem.Extensions["debug"] = new
			{
				message = exception?.Message,
				type = exception?.GetType().Name,
				stackTrace = exception?.StackTrace
			};
		}

		return problem;
	}
}
