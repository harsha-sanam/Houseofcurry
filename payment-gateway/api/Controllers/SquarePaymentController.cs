using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using OrderModel = PaymentApi.Models.Order;
using Square;
using Square.Exceptions;
using Square.Models;
using System;
using System.Threading.Tasks;

namespace PaymentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SquarePaymentController : ControllerBase
    {
        private readonly DatabaseConfig _dbConfig;
        private readonly ISquareClient _squareClient;

        public SquarePaymentController(DatabaseConfig dbConfig, ISquareClient squareClient)
        {
            _dbConfig = dbConfig;
            _squareClient = squareClient;
        }

        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            // Return non-secret info frontend needs (application id, location id if available)
            var appId = System.Environment.GetEnvironmentVariable("SQUARE_APPLICATION_ID") ?? string.Empty;
            var locationId = System.Environment.GetEnvironmentVariable("SQUARE_LOCATION_ID") ?? string.Empty;
            return Ok(new { applicationId = appId, locationId = locationId });
        }

        public record ProcessOrderRequest(string SourceId, long AmountCents, string Currency, string CustomerName, string Phone, string? Note);

        [HttpPost("process-order")]
        public async Task<IActionResult> ProcessOrder([FromBody] ProcessOrderRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.SourceId) || req.AmountCents <= 0)
                return BadRequest("Invalid request payload");

            var paymentsApi = _squareClient.PaymentsApi;

            // Resolve location id: environment variable preferred; otherwise pick first available location
            var locationId = System.Environment.GetEnvironmentVariable("SQUARE_LOCATION_ID");
            if (string.IsNullOrEmpty(locationId))
            {
                try
                {
                    var locationsResponse = await _squareClient.LocationsApi.ListLocationsAsync();
                    if (locationsResponse?.Errors == null || locationsResponse.Errors.Count == 0)
                    {
                        locationId = locationsResponse.Locations?.Count > 0 ? locationsResponse.Locations[0].Id : null;
                    }
                }
                catch
                {
                    // ignore; locationId may remain null
                }
            }

            var idempotencyKey = Guid.NewGuid().ToString();

            var amountMoney = new Money.Builder()
                .Amount(req.AmountCents)
                .Currency(req.Currency ?? "USD")
                .Build();

            var createPaymentRequest = new CreatePaymentRequest.Builder(req.SourceId, idempotencyKey, amountMoney)
                .Autocomplete(true)
                .BuyerEmailAddress(null)
                .Note(req.Note)
                .Build();

            try
            {
                var createPaymentResponse = await paymentsApi.CreatePaymentAsync(createPaymentRequest);
                if (createPaymentResponse.Errors != null && createPaymentResponse.Errors.Count > 0)
                {
                    return StatusCode(502, new { errors = createPaymentResponse.Errors });
                }

                var payment = createPaymentResponse.Payment;
                var order = new OrderModel
                {
                    SourceId = req.SourceId,
                    AmountCents = req.AmountCents,
                    Currency = req.Currency ?? "USD",
                    CustomerName = req.CustomerName,
                    Phone = req.Phone,
                    SquarePaymentId = payment?.Id,
                    CreatedAt = DateTime.UtcNow,
                    RawResponse = payment?.ToString()
                };

                using var connection = new MySqlConnection(_dbConfig.ConnectionString);
                await connection.OpenAsync();

                const string insertSql = @"
INSERT INTO Orders (SourceId, AmountCents, Currency, CustomerName, Phone, SquarePaymentId, CreatedAt, RawResponse)
VALUES (@SourceId, @AmountCents, @Currency, @CustomerName, @Phone, @SquarePaymentId, @CreatedAt, @RawResponse);";

                using var insertCommand = new MySqlCommand(insertSql, connection);
                insertCommand.Parameters.AddWithValue("@SourceId", order.SourceId);
                insertCommand.Parameters.AddWithValue("@AmountCents", order.AmountCents);
                insertCommand.Parameters.AddWithValue("@Currency", order.Currency);
                insertCommand.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                insertCommand.Parameters.AddWithValue("@Phone", order.Phone);
                insertCommand.Parameters.AddWithValue("@SquarePaymentId", (object?)order.SquarePaymentId ?? DBNull.Value);
                insertCommand.Parameters.AddWithValue("@CreatedAt", order.CreatedAt);
                insertCommand.Parameters.AddWithValue("@RawResponse", (object?)order.RawResponse ?? DBNull.Value);

                await insertCommand.ExecuteNonQueryAsync();

                return Ok(new { success = true, payment = payment });
            }
            catch (ApiException ex)
            {
                return StatusCode(502, new { message = ex.Message, details = ex.Errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
