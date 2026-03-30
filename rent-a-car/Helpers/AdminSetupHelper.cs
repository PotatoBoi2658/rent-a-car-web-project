using Microsoft.AspNetCore.Identity;
using rent_a_car.Models;

namespace rent_a_car.Helpers
{
    /// <summary>
    /// Helper class for admin role management.
    /// This is a reference guide for manually assigning admin roles via database.
    /// 
    /// TO MAKE A USER AN ADMINISTRATOR:
    /// 1. Register a new user through the website
    /// 2. Open your database management tool (MySQL Workbench, phpMyAdmin, etc.)
    /// 3. Run this query (replace 'username' with the actual username):
    ///    
    ///    INSERT INTO `aspnetuserroles` (`userid`, `roleid`) 
    ///    SELECT u.`Id`, r.`Id` 
    ///    FROM `aspnetusers` u, `aspnetroles` r 
    ///    WHERE u.`UserName` = 'username' AND r.`Name` = 'Administrator';
    /// 
    /// 4. The user will have Administrator role on next login
    /// 
    /// TO REMOVE ADMIN ROLE:
    /// Run this query (replace 'username' with the actual username):
    ///    
    ///    DELETE FROM `aspnetuserroles` 
    ///    WHERE `userid` IN (SELECT `id` FROM `aspnetusers` WHERE `UserName` = 'username')
    ///    AND `roleid` IN (SELECT `id` FROM `aspnetroles` WHERE `Name` = 'Administrator');
    /// 
    /// TO VIEW ALL ADMINS:
    ///    
    ///    SELECT u.`UserName`, u.`Email` 
    ///    FROM `aspnetusers` u
    ///    INNER JOIN `aspnetuserroles` ur ON u.`Id` = ur.`userid`
    ///    INNER JOIN `aspnetroles` r ON ur.`roleid` = r.`Id`
    ///    WHERE r.`Name` = 'Administrator';
    /// </summary>
    public class AdminSetupHelper
    {
        // This is just a reference class with SQL queries in comments
        // Actual role assignment happens in Register.cshtml.cs and can be done via database
    }
}