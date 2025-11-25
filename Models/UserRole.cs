using System.ComponentModel;

namespace Zoomag.Models;

public enum UserRole
{
    [Description("Администратор")]
    Admin,

    [Description("Продавец-консультант")]
    Seller
}
