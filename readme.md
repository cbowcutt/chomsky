# Setup 
This solution requirese .net core version 2.2 or greater installed

Chrome version 77 must be installed in order to run the automated tests.

[Chrome version 77 can be found here](https://www.google.com/chrome/)

# Running the automated test
The automated test for this assignment is in /UITests/AutomationPracticeUITests.cs
To run the automated test, enter the command in  UITests/: 
```
dotnet test
```

# About

I used the Page Object Model design pattern to represent UI elements for the webpages.
I wanted them to be declarative, and separate the representation from the implementation.
The implementation is done by classes implementing IPageObjectIplementor. IPageObjectImplementor declares
methods that take in a PageElementID instance and returns either an IClickable, IReadable, or IInputable
instance that represents a page element identified by the given PageElementID instance.

I created one implementation of IPageObjectImplementor, WebDriverPageObjectImplementor. This implementation uses the Selenium WebDriver to map PageElementIDs to actual web elements and perform
user actions on them.

I also created a Scripts class in Scripts.cs that contains methods for reusable user actions.

There are also unit tests  in UnitTests/ to verify that the classes and pages are working as they should be.



