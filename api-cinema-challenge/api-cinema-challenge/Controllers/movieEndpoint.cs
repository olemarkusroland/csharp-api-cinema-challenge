﻿using api_cinema_challenge.Models.MovieModels;
using api_cinema_challenge.Models.ScreeningModels;
using api_cinema_challenge.Repositories;
using api_cinema_challenge.Services;
using Microsoft.AspNetCore.Mvc;
using workshop.wwwapi.Models;

namespace api_cinema_challenge.Controllers
{
    public static class MovieEndpoint
    {
        public static void ConfigureMovieEndpoint(this WebApplication app)
        {
            var group = app.MapGroup("movies");

            group.MapGet("", GetAll);
            group.MapGet("{id}", Get);
            group.MapPost("", Create);
            group.MapPut("{id}", Update);
            group.MapDelete("{id}", Delete);
            group.MapGet("{id}/screenings", GetScreenings);
            group.MapPost("{id}/screenings", CreateScreening);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        private static async Task<IResult> GetAll(IRepository<Movie> repository)
        {
            IEnumerable<Movie> movies = await repository.Get();

            IEnumerable<OutputMovie> outputMovies = MovieDtoManager.Convert(movies);
            var payload = new Payload<IEnumerable<OutputMovie>>(outputMovies);
            return TypedResults.Ok(payload);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        private static async Task<IResult> Get(int id, IRepository<Movie> repository)
        {
            Movie? movie = await repository.Get(id);
            if (movie == null)
                return TypedResults.NotFound(new Payload<Movie>(movie));

            OutputMovie outputMovie = MovieDtoManager.Convert(movie);
            var payload = new Payload<OutputMovie>(outputMovie);
            return TypedResults.Ok(payload);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        private static async Task<IResult> Create(InputMovie movie, IRepository<Movie> repository)
        {
            Movie newMovie = MovieDtoManager.Convert(movie);

            Movie result = await repository.Create(newMovie);

            OutputMovie outputMovie = MovieDtoManager.Convert(result);
            var payload = new Payload<OutputMovie>(outputMovie);
            return TypedResults.Created("url", payload);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        private static async Task<IResult> Update(int id, InputMovie inputMovie, IRepository<Movie> repository)
        {
            Movie? movieToUpdate = await repository.Get(id);
            if (movieToUpdate == null)
                return TypedResults.NotFound(new Payload<Movie>(movieToUpdate));

            movieToUpdate.UpdatedAt = DateTime.UtcNow;
            movieToUpdate.Title = inputMovie.Title;
            movieToUpdate.Runtime = inputMovie.Runtime;
            movieToUpdate.Rating = inputMovie.Rating;
            movieToUpdate.Description = inputMovie.Description;

            Movie result = await repository.Update(movieToUpdate);

            OutputMovie outputMovie = MovieDtoManager.Convert(result);
            var payload = new Payload<OutputMovie>(outputMovie);
            return TypedResults.Ok(payload);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        private static async Task<IResult> Delete(int id, IRepository<Movie> repository)
        {
            Movie? movie = await repository.Delete(id);
            if (movie == null)
                return TypedResults.NotFound(new Payload<Movie>(movie));

            OutputMovie outputMovie = MovieDtoManager.Convert(movie);
            var payload = new Payload<OutputMovie>(outputMovie);
            return TypedResults.Ok(payload);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        private static async Task<IResult> GetScreenings(int id, IRepository<Screening> screeningRepository, IRepository<Movie> movieRepository)
        {
            Movie? movie = await movieRepository.Get(id);
            if (movie == null)
                return TypedResults.NotFound($"Movie with id {id} not found");

            IEnumerable<Screening> screenings = await screeningRepository.GetWhere(s => s.MovieId == id);

            IEnumerable<OutputScreening> outputScreenings = ScreeningDtoManager.Convert(screenings);
            var payload = new Payload<IEnumerable<OutputScreening>>(outputScreenings);
            return TypedResults.Ok(payload);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        private static async Task<IResult> CreateScreening(int id, InputScreening inputScreening, IRepository<Screening> screeningRepository, IRepository<Movie> movieRepository)
        {
            Movie? movie = await movieRepository.Get(id);
            if (movie == null)
                return TypedResults.NotFound($"Movie with id {id} not found");

            Screening newScreening = ScreeningDtoManager.Convert(inputScreening);

            newScreening.MovieId = id;

            Screening result = await screeningRepository.Create(newScreening);

            OutputScreening outputScreening = ScreeningDtoManager.Convert(result);
            var payload = new Payload<OutputScreening>(outputScreening);
            return TypedResults.Created("url", payload);
        }
    }
}
