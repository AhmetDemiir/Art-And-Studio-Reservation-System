namespace Online_Art_Gallery_and_Studio_Reservation_System.Models;

public enum ReservationStatus
{
    Pending = 1,
    Confirmed = 2,
    Cancelled = 3,
    Completed = 4,
    NoShow = 5
}

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Paid = 3,
    Cancelled = 4,
    Refunded = 5,
    Completed = 6
}

public enum PaymentMethod
{
    CreditCard = 1,
    DebitCard = 2,
    BankTransfer = 3,
    CashOnDelivery = 4
}

public enum PaymentStatus
{
    Pending = 1,
    Succeeded = 2,
    Failed = 3,
    Refunded = 4
}

public enum SupportTicketStatus
{
    Open = 1,
    InProgress = 2,
    WaitingForCustomer = 3,
    Resolved = 4,
    Closed = 5
}

public enum SupportTicketPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

public enum ComparisonType
{
    Artwork = 1,
    Workshop = 2
}

public enum ReviewTargetType
{
    Artwork = 1,
    Workshop = 2
}
