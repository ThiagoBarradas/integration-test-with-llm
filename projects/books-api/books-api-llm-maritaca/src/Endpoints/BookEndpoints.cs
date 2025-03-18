﻿using Books.Api.Docker.Filters;
using System.Runtime.CompilerServices;

namespace Books.Api.Docker.Endpoints;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        var bookGroup = app.MapGroup("books");

        bookGroup.MapGet("", GetAllBooks)
            .WithName(nameof(GetAllBooks))
            .Produces<List<Book>>(200)
            .WithOpenApi();

        bookGroup.MapGet("{id}", GetBookById).WithName(nameof(GetBookById))
            .Produces<Book>(200)
            .Produces(404)
            .WithOpenApi();

        bookGroup.MapPost("", CreateBook).WithName(nameof(CreateBook))
            .Produces<ErrorResponse>(400)
            .Produces<Book>(200)
            .AddEndpointFilter<ValidationFilter<CreateBookRequest>>()
            .WithOpenApi();

        bookGroup.MapPut("{id}", UpdateBook).WithName(nameof(UpdateBook))
            .Produces(204)
            .Produces<ErrorResponse>(400)
            .Produces(404)
            .AddEndpointFilter<ValidationFilter<UpdateBookRequest>>()
            .WithOpenApi();

        bookGroup.MapDelete("{id}", DeleteBookById).WithName(nameof(DeleteBookById))
            .Produces(204)
            .Produces(404)
            .WithOpenApi();
    }

    public static async Task<IResult> GetAllBooks(
        IBookService bookService,
        CancellationToken cancellationToken)
    {
        var books = await bookService.GetBooksAsync(cancellationToken);

        return Results.Ok(books.Select(b => b.ToResponseDto()));
    }

    public static async Task<IResult> GetBookById(
         int id,
         IBookService bookService,
         IRedisCacheService cacheService,
         CancellationToken cancellationToken)
    {
        var cacheKey = $"book_{id}";

        var response = await cacheService.GetDataAsync<BookResponse>(
            cacheKey,
            cancellationToken);

        if (response is not null)
        {
            return Results.Ok(response);
        }

        var book = await bookService.GetBookByIdAsync(id, cancellationToken);

        if (book is null)
        {
            return Results.NotFound();
        }

        response = book.ToResponseDto();

        await cacheService.SetDataAsync<BookResponse>(
            cacheKey,
            response,
            cancellationToken);

        return Results.Ok(response);
    }

    public static async Task<IResult> CreateBook(
            CreateBookRequest request,
            IBookService bookService,
            CancellationToken cancellationToken)
    {
        var book = request.ToEntity();

        book.Id = await bookService.CreateBookAsync(book, cancellationToken);

        return Results.Ok(book);
    }

    public static async Task<IResult> UpdateBook(
            int id,
            UpdateBookRequest request,
            IBookService bookService,
            IRedisCacheService cacheService,
            CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = $"book_{id}";

            var book = request.ToEntity(id);

            await bookService.UpdateBookAsync(book, cancellationToken);

            await cacheService.RemoveDataAsync(cacheKey, cancellationToken);

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.NotFound();
        }
    }

    public static async Task<IResult> DeleteBookById(
            int id,
            IBookService bookService,
            IRedisCacheService cacheService,
            CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = $"book_{id}";

            await bookService.DeleteBookByIdAsync(id, cancellationToken);

            await cacheService.RemoveDataAsync(cacheKey, cancellationToken);

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.NotFound();
        }
    }
}
