
# The Redakt Hotel & Resort

The Redakt Hotel & Resort is a fictional showcase web application for [Redakt](https://www.redaktcms.com), a modern and advanced platform for content management and digital transformation based on .NET Core.

The Redakt Hotel & Resort application can also be used as a starting point for implementing your own Redakt-based web applications.

## First time startup

No special steps are required to setup this application. Simply clone the repository and run the web project in Visual Studio.

The Redakt Hotel & Resort application uses an embedded LiteDB database by default. When running the application for the first time, startup will take a little bit longer as the database will be seeded with automatically generated content.

After that, you will be redirected to the short setup wizard where you can enter the administrator account details.

## Projects

The Redakt Resort & Hotel application consists of the following projects:

 - RedaktHotel
This is the main runnable web project and includes the application's views and models.
 - RedaktHotel.BackOfficeExtensions
This projects contains example custom extensions to the back office application, including a custom property editor and a booking module built with Redakt back office components.

## Using Redakt for your project

For more information on working with Redakt and acquiring a commercial license, please see the [Redakt website](https://www.redaktcms.com).