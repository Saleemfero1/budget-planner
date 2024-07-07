public class Expense
{
    public int id { get; set; }
    public string name { get; set; }
    public decimal amount { get; set; }
    public DateTime? date { get; set; }
    public string description { get; set; }
    public int budget_id { get; set; }
}