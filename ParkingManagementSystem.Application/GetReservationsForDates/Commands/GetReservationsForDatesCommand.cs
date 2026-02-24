using ErrorOr;
using MediatR;
using ParkingManagementSystem.Application.GetReservationsForDates.Models;

namespace ParkingManagementSystem.Application.GetReservationsForDates.Commands;

public record GetReservationsForDatesCommand(List<DateOnly> Dates) : IRequest<ErrorOr<GetReservationsForDatesResult>>;