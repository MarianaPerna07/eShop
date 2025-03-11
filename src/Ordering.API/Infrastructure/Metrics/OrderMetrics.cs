using System.Diagnostics.Metrics;

namespace eShop.Ordering.API.Infrastructure.Metrics;
public static class OrderMetrics
{
    public static readonly Meter Meter = new("Ordering.API");

    // Histogram that measures the latency of the order query
    public static readonly Histogram<double> OrderQueryLatency = Meter.CreateHistogram<double>(
        "order_query_latency_ms", 
        unit: "ms",
        description: "Latency of the order query");

    // Counter that measures the total value of orders
    public static readonly Counter<double> TotalOrderValueCounter = Meter.CreateCounter<double>(
        "order_total_value", 
        unit: "$",  
        description: "Total amount of orders placed");
}