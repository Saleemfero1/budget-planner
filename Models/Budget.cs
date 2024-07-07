public class Budget
{
    public int id { get; set; }
    public string name { get; set; }
    public decimal total_amount { get; set; }
    public decimal remaining_amount { get; set; }
    public DateTime? start_date { get; set; }
    public DateTime? end_date { get; set; }
    public int user_id { get; set; }
}
