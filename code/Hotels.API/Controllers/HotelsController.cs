﻿using System;
using System.Linq;
using Hotels.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RateLimitServices;

namespace Hotels.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly ILogger<HotelsController> _logger;
        private readonly IRateLimitServiceForGetProjectByCity _rateLimitServiceForGetProjectByCity;
        private readonly IRateLimitServiceForGetProjectByRoom _rateLimitServiceForGetProjectByRoom;
        private readonly IHotelRepository _hotelRepository;

        public HotelsController(
            IHotelRepository hotelRepository,
            IRateLimitServiceForGetProjectByCity rateLimitServiceForGetProjectByCity,
            IRateLimitServiceForGetProjectByRoom rateLimitServiceForGetProjectByRoom,
            ILogger<HotelsController> logger)
        {
            _rateLimitServiceForGetProjectByCity = rateLimitServiceForGetProjectByCity ?? 
                throw new ArgumentNullException(nameof(rateLimitServiceForGetProjectByCity));

            _rateLimitServiceForGetProjectByRoom = rateLimitServiceForGetProjectByRoom ?? 
                throw new ArgumentNullException(nameof(rateLimitServiceForGetProjectByRoom));

            _hotelRepository = hotelRepository ?? throw new ArgumentException(nameof(hotelRepository));

            _logger = logger ?? throw new ArgumentException(nameof(logger));
        }

        // Get /api/hotels/city/bangkok
        [HttpGet("city/{city}", Name = "GetHotelsByCity")]
        public IActionResult GetHotelsByCity(string city)
        {
            if (_rateLimitServiceForGetProjectByCity.IsLimited)
            {
                // TooManyRequests
                return StatusCode(429, "Please try after some time.");
            }

            var cityHotels = _hotelRepository.GetAll()
                .Where(hotel => hotel.City.Equals(city, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(cityHotels);
        }

        // Get /api/hotels/room/deluxe
        [HttpGet("room/{roomType}", Name = "GetHotelsByRoom")]
        public IActionResult GetHotelsByRoom(string roomType)
        {
            if (_rateLimitServiceForGetProjectByRoom.IsLimited)
            {
                // TooManyRequests
                return StatusCode(429);
            }

            var roomHotels = _hotelRepository.GetAll()
                .Where(hotel => hotel.Room.Equals(roomType, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(roomHotels);
        }
    }
}
