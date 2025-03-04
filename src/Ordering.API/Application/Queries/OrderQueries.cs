namespace eShop.Ordering.API.Application.Queries;

using System.Diagnostics;

public class OrderQueries(OrderingContext context)
    : IOrderQueries
{

    private static readonly ActivitySource ActivitySource = new("Ordering.API");

    public async Task<Order> GetOrderAsync(int id)
    {
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);
      
        if (order is null)
            throw new KeyNotFoundException();

        return new Order
        {
            OrderNumber = order.Id,
            Date = order.OrderDate,
            Description = order.Description,
            City = order.Address.City,
            Country = order.Address.Country,
            State = order.Address.State,
            Street = order.Address.Street,
            Zipcode = order.Address.ZipCode,
            Status = order.OrderStatus.ToString(),
            Total = order.GetTotal(),
            OrderItems = order.OrderItems.Select(oi => new Orderitem
            {
                ProductName = oi.ProductName,
                Units = oi.Units,
                UnitPrice = (double)oi.UnitPrice,
                PictureUrl = oi.PictureUrl
            }).ToList()
        };
    }

    // USEFUL TO SEE MY ORDERS
    // this method call database context to get the orders from the user
    // search for the orders that have the same user id as the one passed as parameter
    // and return them to the user
    public async Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId)
    {
        using var activity = ActivitySource.StartActivity("DatabaseQuery.GetOrdersFromUser");

        activity?.SetTag("db.statement", "SELECT * FROM Orders WHERE BuyerId = @userId");
        activity?.SetTag("db.user", userId);

        var orders = await context.Orders
            .Where(o => o.Buyer.IdentityGuid == userId)  
            .Select(o => new OrderSummary
            {
                OrderNumber = o.Id,
                Date = o.OrderDate,
                Status = o.OrderStatus.ToString(),
                Total =(double) o.OrderItems.Sum(oi => oi.UnitPrice* oi.Units)
            })
            .ToListAsync();

        activity?.SetTag("db.rows_returned", orders.Count());

        return orders;
    } 
    
    public async Task<IEnumerable<CardType>> GetCardTypesAsync() => 
        await context.CardTypes.Select(c=> new CardType { Id = c.Id, Name = c.Name }).ToListAsync();
}
